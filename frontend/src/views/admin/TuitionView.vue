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

      <!-- Row Actions -->
      <template #rowActions="{ record, refresh }">
        <a-menu-item
          v-if="canMarkOverdue(record)"
          class="rounded-lg px-3 py-2 text-xs"
          @click="triggerMarkOverdue(record.id, refresh)"
        >
          Đánh dấu quá hạn
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
import { onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import ConfirmActionModal from '@/components/admin/ConfirmActionModal.vue'
import { tuitionApi } from '@/api/tuitionApi'
import { courseApi } from '@/api/courseApi'
import { INVOICE_STATUS, toOptions } from '@/lib/constants'
import { formatVnd } from '@/lib/formatters'

const statusOptions = toOptions(INVOICE_STATUS, { 1: 'amber', 2: 'orange', 3: 'green', 4: 'red' })

// Filter states
const filterStatus = ref(undefined)
const filterCourseId = ref(undefined)
const filterDueDateRange = ref(null)
const courses = ref([])
const loadingCourses = ref(false)

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
  { title: 'Còn nợ', key: 'remainingAmount', width: 110 },
  { title: 'Tiến độ', key: 'progress', width: 200 },
  { title: 'Hạn đóng', dataIndex: 'dueDate', key: 'dueDate', type: 'date', width: 120 },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 130 },
]

// Form fields — clean, no raw IDs
const fields = [
  { name: 'invoiceCode', label: 'Mã hóa đơn', required: true, default: '' },
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

const formGroups = [
  {
    title: 'Thông tin hóa đơn',
    fields: ['invoiceCode', 'status', 'dueDate']
  },
  {
    title: 'Học viên',
    fields: ['studentId', 'studentNameSnapshot', 'studentCodeSnapshot']
  },
  {
    title: 'Khóa học & Lớp',
    fields: ['courseId', 'courseNameSnapshot', 'classId', 'classNameSnapshot']
  },
  {
    title: 'Thanh toán',
    fields: ['totalAmount', 'paidAmount']
  }
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

function canMarkOverdue(record) {
  return Number(record?.debtAmount || 0) > 0 && record?.status !== 3 && record?.status !== 'Paid' && record?.status !== 4 && record?.status !== 'Overdue'
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

async function loadCourses() {
  loadingCourses.value = true
  try {
    const res = await courseApi.getAll()
    courses.value = res?.items || res?.data || res || []
  } catch (e) {}
  finally { loadingCourses.value = false }
}

onMounted(loadCourses)
</script>
