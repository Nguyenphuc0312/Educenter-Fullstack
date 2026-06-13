<template>
  <div>
    <AdminResourceView
      title="Học phí và hóa đơn"
      subtitle="Theo dõi học phí, số tiền đã đóng, công nợ và trạng thái hóa đơn từng học viên."
      :api="tuitionResourceApi"
      :columns="columns"
      :fields="fields"
      :searchable-fields="['invoiceCode', 'studentNameSnapshot', 'courseNameSnapshot', 'classNameSnapshot']"
      :status-options="statusOptions"
      :form-groups="formGroups"
      :filter-fn="customFilter"
      :selectable="false"
      :can-show-more-actions="canShowInvoiceActions"
      :more-action-icon="WalletOutlined"
      more-action-title="Xử lý hóa đơn"
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

      <template #actions>
        <button
          type="button"
          class="admin-btn admin-btn-secondary h-9 px-3"
          @click="triggerMarkDueOverdue"
        >
          Quét quá hạn
        </button>
      </template>

      <!-- Row Actions -->
      <template #rowActions="{ record, refresh }">
        <a-menu-item
          v-if="getRemaining(record) > 0"
          class="rounded-lg px-3 py-2 text-xs"
          @click="openPaymentModal(record, refresh)"
        >
          Ghi nhận thanh toán
        </a-menu-item>
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

    <a-modal
      v-model:open="paymentModalOpen"
      width="560px"
      :footer="null"
      :destroy-on-close="true"
      :centered="true"
    >
      <template #title>
        <div class="text-sm font-bold text-base-primary pb-3 border-b border-base">
          Ghi nhận thanh toán học phí
        </div>
      </template>

      <div v-if="selectedInvoice" class="mt-4 space-y-4">
        <div class="rounded-lg border border-base bg-slate-50 dark:bg-slate-900/40 p-3">
          <div class="text-xs font-semibold text-base-primary">{{ selectedInvoice.studentNameSnapshot }}</div>
          <div class="text-[11px] text-base-secondary mt-1">
            {{ selectedInvoice.courseNameSnapshot }} - {{ selectedInvoice.invoiceCode }}
          </div>
          <div class="grid grid-cols-3 gap-2 mt-3">
            <div class="text-[11px]">
              <div class="text-base-muted">Tổng</div>
              <div class="font-bold text-base-primary">{{ formatVnd(selectedInvoice.totalAmount || 0) }}</div>
            </div>
            <div class="text-[11px]">
              <div class="text-base-muted">Đã đóng</div>
              <div class="font-bold text-emerald-600">{{ formatVnd(selectedInvoice.paidAmount || 0) }}</div>
            </div>
            <div class="text-[11px]">
              <div class="text-base-muted">Còn nợ</div>
              <div class="font-bold text-rose-600">{{ formatVnd(getRemaining(selectedInvoice)) }}</div>
            </div>
          </div>
        </div>

        <a-form ref="paymentFormRef" :model="paymentForm" layout="vertical">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
            <a-form-item
              label="Số tiền thanh toán"
              name="amount"
              :rules="[{ required: true, message: 'Vui lòng nhập số tiền' }]"
            >
              <a-input-number
                v-model:value="paymentForm.amount"
                class="w-full"
                :min="1"
                :max="getRemaining(selectedInvoice)"
              />
            </a-form-item>

            <a-form-item label="Phương thức" name="method">
              <a-select v-model:value="paymentForm.method">
                <a-select-option v-for="option in methodOptions" :key="option.value" :value="option.value">
                  {{ option.label }}
                </a-select-option>
              </a-select>
            </a-form-item>

            <a-form-item label="Ngày thanh toán" name="paymentDate">
              <a-date-picker v-model:value="paymentForm.paymentDate" class="w-full" value-format="YYYY-MM-DD" />
            </a-form-item>

            <a-form-item label="Trạng thái" name="status">
              <a-select v-model:value="paymentForm.status">
                <a-select-option v-for="option in paymentStatusOptions" :key="option.value" :value="option.value">
                  {{ option.label }}
                </a-select-option>
              </a-select>
            </a-form-item>

            <a-form-item label="Người thu" name="createdBy">
              <a-input v-model:value="paymentForm.createdBy" />
            </a-form-item>

            <a-form-item label="Ghi chú" name="note" class="md:col-span-2">
              <a-textarea v-model:value="paymentForm.note" :rows="3" />
            </a-form-item>
          </div>
        </a-form>

        <div class="flex justify-end gap-2 pt-4 border-t border-base">
          <button type="button" class="admin-btn admin-btn-secondary h-10 px-5" :disabled="paymentSaving" @click="paymentModalOpen = false">
            Đóng
          </button>
          <button type="button" class="admin-btn admin-btn-primary h-10 px-5" :disabled="paymentSaving" @click="submitPayment">
            {{ paymentSaving ? 'Đang lưu...' : 'Ghi nhận thanh toán' }}
          </button>
        </div>
      </div>
    </a-modal>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue'
import { message } from 'ant-design-vue'
import { WalletOutlined } from '@ant-design/icons-vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import ConfirmActionModal from '@/components/admin/ConfirmActionModal.vue'
import { tuitionApi } from '@/api/tuitionApi'
import { paymentApi } from '@/api/paymentApi'
import { courseApi } from '@/api/courseApi'
import { INVOICE_STATUS, PAYMENT_METHOD, PAYMENT_STATUS, toOptions } from '@/lib/constants'
import { formatVnd } from '@/lib/formatters'

const statusOptions = toOptions(INVOICE_STATUS, { 1: 'amber', 2: 'orange', 3: 'green', 4: 'red' })
const methodOptions = toOptions(PAYMENT_METHOD)
const paymentStatusOptions = toOptions(PAYMENT_STATUS, { 1: 'green', 2: 'blue', 3: 'red', 4: 'default' })
const invoiceStatusNameToValue = { Unpaid: 1, Partial: 2, Paid: 3, Overdue: 4 }
const tuitionResourceApi = {
  getAll: tuitionApi.getAll,
  getById: tuitionApi.getById,
  export: tuitionApi.export
}

function invoiceStatusValue(status) {
  return Number.isFinite(Number(status)) ? Number(status) : invoiceStatusNameToValue[status] || 0
}

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

const paymentModalOpen = ref(false)
const paymentSaving = ref(false)
const selectedInvoice = ref(null)
const paymentFormRef = ref()
let paymentRefreshCallback = null
const paymentForm = reactive({
  amount: 0,
  method: 1,
  paymentDate: '',
  status: 1,
  createdBy: 'admin',
  note: ''
})

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
  const matchStatus = filterStatus.value === undefined || invoiceStatusValue(item.status) === Number(filterStatus.value)
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

function canMarkOverdue(record) {
  return getRemaining(record) > 0 && invoiceStatusValue(record.status) !== 4
}

function canShowInvoiceActions(record) {
  return getRemaining(record) > 0
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

function todayInputValue() {
  const now = new Date()
  const yyyy = now.getFullYear()
  const mm = String(now.getMonth() + 1).padStart(2, '0')
  const dd = String(now.getDate()).padStart(2, '0')
  return `${yyyy}-${mm}-${dd}`
}

function openPaymentModal(record, refresh) {
  selectedInvoice.value = record
  paymentRefreshCallback = refresh
  paymentForm.amount = getRemaining(record)
  paymentForm.method = 1
  paymentForm.paymentDate = todayInputValue()
  paymentForm.status = 1
  paymentForm.createdBy = 'admin'
  paymentForm.note = ''
  paymentModalOpen.value = true
}

async function submitPayment() {
  if (!selectedInvoice.value) return
  try {
    await paymentFormRef.value?.validate()
    const remaining = getRemaining(selectedInvoice.value)
    if (Number(paymentForm.amount) <= 0 || Number(paymentForm.amount) > remaining) {
      message.error('Số tiền thanh toán không hợp lệ')
      return
    }
    paymentSaving.value = true
    await paymentApi.create({
      invoiceId: selectedInvoice.value.id,
      amount: Number(paymentForm.amount),
      method: paymentForm.method,
      paymentDate: paymentForm.paymentDate,
      status: paymentForm.status,
      createdBy: paymentForm.createdBy || 'admin',
      note: paymentForm.note || null
    })
    message.success('Đã ghi nhận thanh toán')
    paymentModalOpen.value = false
    await paymentRefreshCallback?.()
  } catch (error) {
    if (error?.errorFields) return
    message.error(error.message || 'Không thể ghi nhận thanh toán')
  } finally {
    paymentSaving.value = false
  }
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

async function triggerMarkDueOverdue() {
  try {
    const result = await tuitionApi.markOverdueDue()
    message.success(`Đã cập nhật ${result?.updated || 0} hóa đơn quá hạn`)
  } catch (error) {
    message.error(error.message || 'Không thể quét hóa đơn quá hạn')
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
