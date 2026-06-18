<template>
  <div>
    <AdminResourceView
      title="Học phí và hóa đơn"
      subtitle="Theo dõi học phí, số tiền đã đóng, công nợ và trạng thái hóa đơn từng học viên."
      :api="tuitionApi"
      :columns="columns"
      :fields="fields"
      :searchable-fields="['invoiceCode', 'studentNameSnapshot', 'courseNameSnapshot', 'classNameSnapshot']"
      :status-options="statusOptions"
      :form-groups="formGroups"
      :filter-fn="customFilter"
      :can-select="record => !isPaid(record.status)"
      @reset="resetCustomFilters"
    >
      <!-- Custom Filters -->
      <template #filters>
        <!-- Trạng thái hóa đơn -->
        <a-select
          v-model:value="filterStatus"
          placeholder="Trạng thái"
          allow-clear
          size="small"
          class="w-40"
        >
          <a-select-option :value="1">Chưa thanh toán</a-select-option>
          <a-select-option :value="2">Một phần</a-select-option>
          <a-select-option :value="3">Đã thanh toán</a-select-option>
          <a-select-option :value="4">Quá hạn</a-select-option>
        </a-select>

        <!-- Khóa học -->
        <a-select
          v-model:value="filterCourseId"
          placeholder="Khóa học"
          allow-clear
          size="small"
          class="w-48"
          :loading="loadingCourses"
        >
          <a-select-option v-for="course in courses" :key="course.id" :value="course.id">
            {{ course.name }}
          </a-select-option>
        </a-select>

        <!-- Hạn thanh toán -->
        <a-range-picker
          v-model:value="filterDueDateRange"
          value-format="YYYY-MM-DD"
          size="small"
          class="w-56"
          :placeholder="['Hạn từ', 'Đến']"
        />
      </template>

      <!-- Bulk actions -->
      <template #bulkActions="{ selectedRowKeys, refresh }">
        <a-button
          size="small"
          class="h-8 px-3 border border-amber-200 dark:border-amber-800 text-amber-600 dark:text-amber-400 text-xs font-medium"
          :disabled="!selectedRowKeys.length"
          @click="triggerBulkOverdue(selectedRowKeys, refresh)"
        >
          Đánh dấu quá hạn
        </a-button>
      </template>

      <!-- Row Actions -->
      <template #rowActions="{ record, refresh }">
        <a-menu-item
          v-if="!isPaid(record.status)"
          class="rounded-lg px-3 py-2 text-xs"
          @click="triggerMarkOverdue(record.id, refresh)"
        >
          Đánh dấu quá hạn
        </a-menu-item>
        <a-menu-item
          class="rounded-lg px-3 py-2 text-xs"
          @click="printInvoiceReceipt(record)"
        >
          In biên lai (PDF)
        </a-menu-item>
        <a-menu-item
          v-if="!isPaid(record.status) && getRemaining(record) > 0"
          class="rounded-lg px-3 py-2 text-xs text-rose-600 font-semibold"
          @click="sendDebtNotice(record.id)"
        >
          Gửi nhắc nợ (Email)
        </a-menu-item>
      </template>

      <!-- Custom Body Cells -->
      <template #bodyCell="{ column, record }">
        <!-- Invoice code cell: readable short format -->
        <template v-if="column.key === 'invoiceCode'">
          <span class="font-mono text-[11px] font-semibold text-slate-600 dark:text-slate-300 bg-slate-100 dark:bg-slate-800 px-1.5 py-0.5 rounded">
            {{ shortCode(record.invoiceCode) }}
          </span>
        </template>

        <!-- Remaining amount cell -->
        <template v-else-if="column.key === 'remainingAmount'">
          <span
            v-if="getRemaining(record) === 0"
            class="text-xs font-semibold text-emerald-600 dark:text-emerald-400 whitespace-nowrap"
          >
            —
          </span>
          <span
            v-else
            class="text-xs font-bold text-rose-600 dark:text-rose-400 whitespace-nowrap"
          >
            {{ formatVnd(getRemaining(record)) }}
          </span>
        </template>

        <!-- Progress bar cell -->
        <template v-else-if="column.key === 'progress'">
          <div class="space-y-1 max-w-[160px]">
            <div class="flex items-center justify-between text-[10px] gap-2">
              <span class="text-emerald-600 dark:text-emerald-400 font-semibold whitespace-nowrap">
                {{ formatVnd(record.paidAmount || 0) }}
              </span>
              <span class="text-base-muted whitespace-nowrap">
                / {{ formatVnd(record.totalAmount || 0) }}
              </span>
            </div>
            <div class="w-full h-1.5 bg-slate-100 dark:bg-slate-800 rounded-full overflow-hidden">
              <div
                class="h-full rounded-full transition-all"
                :class="getProgressBarClass(record)"
                :style="{ width: `${getPaymentProgress(record)}%` }"
              ></div>
            </div>
          </div>
        </template>

        <!-- Student name cell with code -->
        <template v-else-if="column.key === 'studentNameSnapshot'">
          <div class="leading-tight">
            <div class="text-xs font-semibold text-base-primary truncate max-w-[160px]" :title="record.studentNameSnapshot">
              {{ record.studentNameSnapshot || '—' }}
            </div>
            <div v-if="record.studentCodeSnapshot" class="text-[10px] text-base-muted truncate max-w-[160px]">
              {{ record.studentCodeSnapshot }}
            </div>
          </div>
        </template>
      </template>
    </AdminResourceView>

    <!-- Overdue Confirm Modal -->
    <ConfirmActionModal
      v-model:open="confirmOpen"
      :title="confirmTitle"
      :message="confirmMsg"
      type="warning"
      :loading="confirmLoading"
      ok-text="Đánh dấu quá hạn"
      @confirm="handleExecuteAction"
    />
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import ConfirmActionModal from '@/components/admin/ConfirmActionModal.vue'
import { tuitionApi } from '@/api/tuitionApi'
import { courseApi } from '@/api/courseApi'
import { classApi } from '@/api/classApi'
import { studentApi } from '@/api/studentApi'
import { INVOICE_STATUS, toOptions } from '@/lib/constants'
import { formatVnd } from '@/lib/formatters'
import {
  applyClassSnapshot,
  applyCourseSnapshot,
  applyStudentSnapshot,
  asList,
  classOptions,
  courseOptions,
  findById,
  studentOptions,
} from '@/lib/adminRelationOptions'

const statusOptions = toOptions(INVOICE_STATUS, { 1: 'amber', 2: 'orange', 3: 'green', 4: 'red' })

// Filter states
const filterStatus = ref(undefined)
const filterCourseId = ref(undefined)
const filterDueDateRange = ref(null)
const courses = ref([])
const classes = ref([])
const students = ref([])
const loadingCourses = ref(false)
const loadingClasses = ref(false)
const loadingStudents = ref(false)

// Confirm modal states
const confirmOpen = ref(false)
const confirmTitle = ref('')
const confirmMsg = ref('')
const confirmLoading = ref(false)
let confirmActionCallback = null

// Table columns
const columns = [
  { title: 'Mã hóa đơn', key: 'invoiceCode', width: 140 },
  { title: 'Học viên', key: 'studentNameSnapshot', width: 180 },
  { title: 'Khóa học', dataIndex: 'courseNameSnapshot', key: 'courseNameSnapshot', width: 160, ellipsis: true },
  { title: 'Tổng tiền', dataIndex: 'totalAmount', key: 'totalAmount', type: 'money', width: 130, sorter: (a, b) => (a.totalAmount || 0) - (b.totalAmount || 0) },
  { title: 'Đã đóng', dataIndex: 'paidAmount', key: 'paidAmount', type: 'money', width: 130, sorter: (a, b) => (a.paidAmount || 0) - (b.paidAmount || 0) },
  { title: 'Còn nợ', key: 'remainingAmount', width: 110, sortValue: (record) => Number(record.totalAmount || 0) - Number(record.paidAmount || 0) },
  { title: 'Tiến độ', key: 'progress', width: 200, sortValue: (record) => Number(record.totalAmount) > 0 ? Number(record.paidAmount || 0) / Number(record.totalAmount) : 0 },
  { title: 'Hạn đóng', dataIndex: 'dueDate', key: 'dueDate', type: 'date', width: 120 },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 130 },
]

// Form fields — clean, no raw IDs
const legacyFields = [
  { name: 'invoiceCode', label: 'Mã hóa đơn', required: true, editOnly: true, default: '' },
  { name: 'studentId', label: 'ID Học viên', required: true, default: '' },
  { name: 'studentNameSnapshot', label: 'Tên học viên', required: true, default: '' },
  { name: 'studentCodeSnapshot', label: 'Mã học viên', default: '' },
  { name: 'courseId', label: 'ID Khóa học', required: true, default: '' },
  { name: 'courseNameSnapshot', label: 'Tên khóa học', required: true, default: '' },
  { name: 'classId', label: 'ID Lớp học', default: '' },
  { name: 'classNameSnapshot', label: 'Tên lớp', default: '' },
  { name: 'totalAmount', label: 'Tổng tiền', type: 'number', required: true, default: 0 },
  { name: 'paidAmount', label: 'Đã đóng', type: 'number', default: 0 },
  { name: 'dueDate', label: 'Hạn đóng', type: 'date', default: '' },
  { name: 'status', label: 'Trạng thái', type: 'select', options: statusOptions, default: 1 },
]

const fields = computed(() => [
  { name: 'invoiceCode', label: 'Mã hóa đơn', required: true, editOnly: true, default: '' },
  {
    name: 'studentId',
    label: 'Học viên',
    type: 'select',
    options: studentOptions(students.value),
    required: true,
    default: '',
    placeholder: loadingStudents.value ? 'Đang tải học viên...' : 'Chọn học viên',
    onChange: (_value, formState, { option }) => applyStudentSnapshot(formState, option?.item),
  },
  {
    name: 'courseId',
    label: 'Khóa học',
    type: 'select',
    options: courseOptions(courses.value),
    required: true,
    default: '',
    placeholder: loadingCourses.value ? 'Đang tải khóa học...' : 'Chọn khóa học',
    onChange: (value, formState, { option }) => {
      applyCourseSnapshot(formState, option?.item)
      const currentClass = findById(classes.value, formState.classId)
      if (currentClass?.courseId && currentClass.courseId !== value) {
        formState.classId = ''
        formState.classNameSnapshot = ''
      }
    },
  },
  {
    name: 'classId',
    label: 'Lớp học',
    type: 'select',
    options: classOptions(classes.value),
    required: true,
    default: '',
    placeholder: loadingClasses.value ? 'Đang tải lớp học...' : 'Chọn lớp học',
    onChange: (_value, formState, { option }) => applyClassSnapshot(formState, option?.item, courses.value),
  },
  { name: 'studentNameSnapshot', label: 'Tên học viên', hidden: true, required: true, default: '' },
  { name: 'studentCodeSnapshot', label: 'Mã học viên', hidden: true, default: '' },
  { name: 'courseNameSnapshot', label: 'Tên khóa học', hidden: true, required: true, default: '' },
  { name: 'classNameSnapshot', label: 'Tên lớp', hidden: true, default: '' },
  { name: 'totalAmount', label: 'Tổng tiền', type: 'number', required: true, default: 0 },
  { name: 'paidAmount', label: 'Đã đóng', type: 'number', default: 0 },
  { name: 'dueDate', label: 'Hạn đóng', type: 'date', default: '' },
  { name: 'status', label: 'Trạng thái', type: 'select', options: statusOptions, default: 1 },
])

const formGroups = [
  { title: 'Thông tin hóa đơn', fields: ['invoiceCode', 'status', 'dueDate'] },
  { title: 'Học viên', fields: ['studentId', 'studentNameSnapshot', 'studentCodeSnapshot'] },
  { title: 'Khóa học & Lớp học', fields: ['courseId', 'courseNameSnapshot', 'classId', 'classNameSnapshot'] },
  { title: 'Thanh toán', fields: ['totalAmount', 'paidAmount'] },
]

function customFilter(item) {
  const matchStatus = filterStatus.value === undefined || Number(item.status) === Number(filterStatus.value)
  const matchCourse = filterCourseId.value === undefined || item.courseId === filterCourseId.value

  let matchDueDate = true
  if (filterDueDateRange.value && filterDueDateRange.value.length >= 2) {
    const [start, end] = filterDueDateRange.value
    const startDate = new Date(start)
    const endDate = new Date(end)
    endDate.setHours(23, 59, 59, 999)
    if (item.dueDate) {
      const dueDate = new Date(item.dueDate)
      matchDueDate = dueDate >= startDate && dueDate <= endDate
    }
  }

  return matchStatus && matchCourse && matchDueDate
}

function resetCustomFilters() {
  filterStatus.value = undefined
  filterCourseId.value = undefined
  filterDueDateRange.value = null
}

function getRemaining(record) {
  return Math.max((record.totalAmount || 0) - (record.paidAmount || 0), 0)
}

function isPaid(status) {
  return Number(status) === 3 || status === 'Paid'
}

function getPaymentProgress(record) {
  if (!record.totalAmount || record.totalAmount === 0) return 0
  return Math.min(Math.round(((record.paidAmount || 0) / record.totalAmount) * 100), 100)
}

function getProgressBarClass(record) {
  const progress = getPaymentProgress(record)
  if (progress >= 100) return 'bg-emerald-500'
  if (progress >= 50) return 'bg-blue-400'
  if (progress > 0) return 'bg-amber-400'
  return 'bg-rose-400'
}

function shortCode(code) {
  if (!code) return '—'
  if (code.length <= 12) return code
  return code.substring(0, 8) + '…'
}

function triggerMarkOverdue(id, refresh) {
  confirmTitle.value = 'Đánh dấu hóa đơn quá hạn?'
  confirmMsg.value = 'Trạng thái hóa đơn sẽ được cập nhật thành Quá hạn.'
  confirmActionCallback = async () => {
    await tuitionApi.markOverdue(id)
    message.success('Đã đánh dấu quá hạn')
    refresh()
  }
  confirmOpen.value = true
}

function triggerBulkOverdue(ids, refresh) {
  confirmTitle.value = `Đánh dấu ${ids.length} hóa đơn quá hạn?`
  confirmMsg.value = `Trạng thái của ${ids.length} hóa đơn đã chọn sẽ được cập nhật thành Quá hạn.`
  confirmActionCallback = async () => {
    await tuitionApi.bulkMarkOverdue(ids)
    message.success('Đã đánh dấu quá hạn hàng loạt')
    refresh()
  }
  confirmOpen.value = true
}

async function sendDebtNotice(invoiceId) {
  try {
    message.loading({ content: 'Đang gửi email nhắc nợ...', key: 'debt_notice' })
    await tuitionApi.sendDebtNotice(invoiceId)
    message.success({ content: 'Đã gửi thông báo nhắc nợ qua email thành công!', key: 'debt_notice' })
  } catch (err) {
    message.error({ content: 'Gửi nhắc nợ thất bại: ' + (err.message || ''), key: 'debt_notice' })
  }
}

function printInvoiceReceipt(record) {
  const printWindow = window.open('', '_blank')
  if (!printWindow) {
    message.error('Không thể mở cửa sổ in. Vui lòng tắt chặn popup.')
    return
  }

  const remaining = getRemaining(record)
  const paidProgress = getPaymentProgress(record)
  const statusLabel = {
    1: 'Chưa thanh toán',
    2: 'Đóng một phần',
    3: 'Đã thanh toán',
    4: 'Quá hạn'
  }[Number(record.status)] || record.status

  printWindow.document.write(`
    <html>
      <head>
        <title>Biên lai học phí - ${record.invoiceCode}</title>
        <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
        <style>
          body {
            font-family: 'Inter', sans-serif;
            margin: 40px;
            color: #1e293b;
            line-height: 1.5;
            font-size: 13px;
          }
          .receipt-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            border-bottom: 2px solid #4f46e5;
            padding-bottom: 20px;
            margin-bottom: 30px;
          }
          .logo-text {
            font-size: 24px;
            font-weight: 700;
            color: #4f46e5;
            margin: 0;
            letter-spacing: -0.5px;
          }
          .logo-sub {
            font-size: 11px;
            color: #64748b;
            margin: 2px 0 0 0;
            text-transform: uppercase;
          }
          .receipt-title-block {
            text-align: right;
          }
          .receipt-title {
            font-size: 20px;
            font-weight: 700;
            margin: 0 0 5px 0;
            color: #0f172a;
          }
          .receipt-code {
            font-family: monospace;
            font-size: 13px;
            font-weight: 600;
            color: #4f46e5;
            background: #f1f5f9;
            padding: 2px 6px;
            border-radius: 4px;
          }
          .info-section {
            display: grid;
            grid-template-cols: 1fr 1fr;
            gap: 20px;
            margin-bottom: 30px;
          }
          .info-block h3 {
            font-size: 11px;
            font-weight: 700;
            text-transform: uppercase;
            color: #64748b;
            margin: 0 0 8px 0;
            border-bottom: 1px solid #e2e8f0;
            padding-bottom: 4px;
          }
          .info-row {
            display: flex;
            margin-bottom: 6px;
          }
          .info-label {
            width: 120px;
            color: #64748b;
            font-weight: 500;
          }
          .info-val {
            font-weight: 600;
            color: #0f172a;
          }
          .table-container {
            margin-bottom: 40px;
          }
          .receipt-table {
            width: 100%;
            border-collapse: collapse;
          }
          .receipt-table th, .receipt-table td {
            padding: 12px;
            text-align: left;
            border-bottom: 1px solid #e2e8f0;
          }
          .receipt-table th {
            background-color: #f8fafc;
            color: #475569;
            font-weight: 600;
            font-size: 12px;
            text-transform: uppercase;
          }
          .receipt-table td {
            font-size: 13px;
          }
          .align-right {
            text-align: right !important;
          }
          .total-row td {
            border-bottom: none;
            padding-top: 8px;
            padding-bottom: 8px;
          }
          .grand-total {
            font-size: 16px;
            font-weight: 700;
            color: #4f46e5;
            border-top: 2px solid #e2e8f0;
            padding-top: 12px !important;
          }
          .signature-section {
            display: grid;
            grid-template-cols: 1fr 1fr;
            gap: 40px;
            text-align: center;
            margin-top: 60px;
            page-break-inside: avoid;
          }
          .signature-block {
            display: flex;
            flex-direction: column;
            align-items: center;
          }
          .signature-title {
            font-weight: 600;
            color: #475569;
            margin-bottom: 70px;
          }
          .signature-name {
            font-weight: 600;
            color: #0f172a;
            border-top: 1px dashed #cbd5e1;
            width: 180px;
            padding-top: 6px;
          }
          @media print {
            body { margin: 20px; }
            .grand-total { color: #000000 !important; }
            .receipt-table th { background-color: #f8fafc !important; -webkit-print-color-adjust: exact; print-color-adjust: exact; }
          }
        </style>
      </head>
      <body>
        <div class="receipt-header">
          <div>
            <h1 class="logo-text">EduCenter</h1>
            <p class="logo-sub">Hệ thống đào tạo Full-Stack lập trình</p>
          </div>
          <div class="receipt-title-block">
            <h2 class="receipt-title">BIÊN LAI HỌC PHÍ</h2>
            <span class="receipt-code">${record.invoiceCode}</span>
          </div>
        </div>

        <div class="info-section">
          <div class="info-block">
            <h3>Thông tin người nộp</h3>
            <div class="info-row">
              <span class="info-label">Học viên:</span>
              <span class="info-val">${record.studentNameSnapshot}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Mã học viên:</span>
              <span class="info-val">${record.studentCodeSnapshot || '—'}</span>
            </div>
          </div>
          <div class="info-block">
            <h3>Thông tin lớp học</h3>
            <div class="info-row">
              <span class="info-label">Khóa học:</span>
              <span class="info-val">${record.courseNameSnapshot}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Lớp học:</span>
              <span class="info-val">${record.classNameSnapshot || '—'}</span>
            </div>
          </div>
        </div>

        <div class="table-container">
          <table class="receipt-table">
            <thead>
              <tr>
                <th>Nội dung thanh toán</th>
                <th class="align-right">Số tiền</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Học phí khóa học <strong>${record.courseNameSnapshot}</strong> (${record.classNameSnapshot || 'Lớp học'})</td>
                <td class="align-right">${formatVnd(record.totalAmount)}</td>
              </tr>
              <tr class="total-row">
                <td class="align-right" style="color: #64748b; font-weight: 500;">Tổng học phí:</td>
                <td class="align-right" style="font-weight: 600;">${formatVnd(record.totalAmount)}</td>
              </tr>
              <tr class="total-row">
                <td class="align-right" style="color: #64748b; font-weight: 500;">Số tiền đã đóng:</td>
                <td class="align-right" style="color: #16a34a; font-weight: 600;">${formatVnd(record.paidAmount || 0)}</td>
              </tr>
              <tr class="total-row">
                <td class="align-right grand-total">Công nợ còn lại:</td>
                <td class="align-right grand-total">${formatVnd(remaining)}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="info-section">
          <div class="info-block" style="grid-column: span 2;">
            <h3>Chi tiết công nợ</h3>
            <div class="info-row">
              <span class="info-label">Hạn thanh toán:</span>
              <span class="info-val" style="color: #dc2626;">${new Date(record.dueDate).toLocaleDateString('vi-VN')}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Trạng thái:</span>
              <span class="info-val">${statusLabel}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Tỷ lệ hoàn thành:</span>
              <span class="info-val">${paidProgress}%</span>
            </div>
          </div>
        </div>

        <div class="signature-section">
          <div class="signature-block">
            <span class="signature-title">Người nộp tiền</span>
            <span class="signature-name">${record.studentNameSnapshot}</span>
          </div>
          <div class="signature-block">
            <span class="signature-title">Người thu tiền (Ký, họ tên)</span>
            <span class="signature-name">Thủ quỹ EduCenter</span>
          </div>
        </div>
      </body>
    </html>
  `)
  printWindow.document.close()
  printWindow.focus()
  setTimeout(() => {
    printWindow.print()
    printWindow.close()
  }, 500)
}

async function handleExecuteAction() {
  if (!confirmActionCallback) return
  confirmLoading.value = true
  try {
    await confirmActionCallback()
    confirmOpen.value = false
  } catch (error) {
    message.error(error.message || 'Không thể thực hiện hành động')
  } finally {
    confirmLoading.value = false
  }
}

async function loadDependencies() {
  loadingCourses.value = true
  loadingClasses.value = true
  loadingStudents.value = true
  try {
    const [coursesRes, classesRes, studentsRes] = await Promise.all([
      courseApi.getAll(),
      classApi.getAll(),
      studentApi.getAll(),
    ])
    courses.value = asList(coursesRes)
    classes.value = asList(classesRes)
    students.value = asList(studentsRes)
  } catch (e) {}
  finally {
    loadingCourses.value = false
    loadingClasses.value = false
    loadingStudents.value = false
  }
}

onMounted(loadDependencies)
</script>
