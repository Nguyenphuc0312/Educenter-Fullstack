<template>
  <div>
    <AdminResourceView
      title="Lịch sử thanh toán"
      subtitle="Ghi nhận giao dịch học phí, phương thức thanh toán và trạng thái xử lý."
      :api="paymentApi"
      :columns="columns"
      :fields="fields"
      :searchable-fields="['invoiceCode', 'studentNameSnapshot', 'invoiceId', 'createdBy', 'note']"
      :status-options="statusOptions"
      :form-groups="formGroups"
      :filter-fn="customFilter"
      :can-select-record="canSelectPayment"
      :can-show-more-actions="canShowPaymentActions"
      :more-action-icon="CheckCircleOutlined"
      more-action-title="Xử lý yêu cầu thanh toán"
      @reset="resetCustomFilters"
    >
      <!-- Custom Filters -->
      <template #filters>
        <!-- Trạng thái giao dịch -->
        <a-select
          v-model:value="filterStatus"
          placeholder="Trạng thái"
          allow-clear
          size="small"
          class="w-36"
        >
          <a-select-option :value="1">Thành công</a-select-option>
          <a-select-option :value="2">Chờ xác nhận</a-select-option>
          <a-select-option :value="3">Thất bại</a-select-option>
          <a-select-option :value="4">Đã hủy</a-select-option>
        </a-select>

        <!-- Phương thức thanh toán -->
        <a-select
          v-model:value="filterMethod"
          placeholder="Phương thức"
          allow-clear
          size="small"
          class="w-36"
        >
          <a-select-option v-for="opt in methodOptions" :key="opt.value" :value="opt.value">
            {{ opt.label }}
          </a-select-option>
        </a-select>

        <!-- Khoảng ngày thanh toán -->
        <a-range-picker
          v-model:value="filterDateRange"
          value-format="YYYY-MM-DD"
          size="small"
          class="w-56"
          :placeholder="['Từ ngày', 'Đến ngày']"
        />
      </template>

      <!-- Bulk cancel -->
      <template #bulkActions="{ selectedRowKeys, refresh }">
        <a-button
          size="small"
          class="h-8 px-3 border border-rose-200 dark:border-rose-800 text-rose-600 dark:text-rose-400 text-xs font-medium"
          :disabled="!selectedRowKeys.length"
          @click="triggerBulkCancel(selectedRowKeys, refresh)"
        >
          Hủy yêu cầu
        </a-button>
      </template>

      <!-- Row actions -->
      <template #rowActions="{ record, refresh }">
        <a-menu-item
          v-if="statusValue(record.status) === 2"
          class="rounded-lg px-3 py-2 text-xs"
          @click="triggerConfirmPayment(record.id, refresh)"
        >
          Xác nhận thanh toán
        </a-menu-item>
        <a-menu-item
          v-if="statusValue(record.status) === 2"
          class="rounded-lg px-3 py-2 text-xs text-rose-600"
          @click="triggerCancelOne(record.id, refresh)"
        >
          Hủy yêu cầu
        </a-menu-item>
      </template>

      <!-- Custom Body Cells -->
      <template #bodyCell="{ column, record }">
        <!-- Invoice ID: short readable format -->
        <template v-if="column.key === 'invoiceId'">
          <span class="font-mono text-[11px] font-semibold text-slate-600 dark:text-slate-300 bg-slate-100 dark:bg-slate-800 px-1.5 py-0.5 rounded">
            {{ shortCode(record.invoiceCode || record.invoiceId) }}
          </span>
        </template>

        <template v-else-if="column.key === 'studentNameSnapshot'">
          <div class="leading-tight">
            <div class="text-xs font-semibold text-base-primary truncate max-w-[180px]" :title="record.studentNameSnapshot">
              {{ record.studentNameSnapshot || 'Không rõ học viên' }}
            </div>
            <div class="text-[10px] text-base-muted truncate max-w-[180px]">
              {{ shortCode(record.studentId) }}
            </div>
          </div>
        </template>

        <!-- Payment method with icon -->
        <template v-else-if="column.key === 'method'">
          <div class="flex items-center gap-2">
            <span class="inline-flex h-6 w-6 items-center justify-center rounded-full text-[10px] font-bold bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-300">
              {{ methodShortLabel(record.method) }}
            </span>
            <span class="text-xs font-medium text-base-primary">{{ methodLabel(record.method) }}</span>
          </div>
        </template>

        <!-- Amount: colored -->
        <template v-else-if="column.key === 'amount'">
          <span class="font-bold text-emerald-700 dark:text-emerald-400 whitespace-nowrap">
            {{ formatVnd(record.amount) }}
          </span>
        </template>

        <!-- Payment date formatted -->
        <template v-else-if="column.key === 'paymentDate'">
          <span class="text-xs text-base-secondary whitespace-nowrap">
            {{ formatDate(record.paymentDate) }}
          </span>
        </template>
      </template>
    </AdminResourceView>

    <!-- Cancel Confirm Modal -->
    <ConfirmActionModal
      v-model:open="confirmOpen"
      :title="confirmTitle"
      :message="confirmMsg"
      :type="confirmType"
      :loading="confirmLoading"
      :ok-text="confirmOkText"
      @confirm="handleExecuteAction"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { message } from 'ant-design-vue'
import { CheckCircleOutlined } from '@ant-design/icons-vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import ConfirmActionModal from '@/components/admin/ConfirmActionModal.vue'
import { paymentApi } from '@/api/paymentApi'
import { PAYMENT_METHOD, PAYMENT_STATUS, toOptions } from '@/lib/constants'
import { formatVnd } from '@/lib/formatters'

const statusOptions = toOptions(PAYMENT_STATUS, { 1: 'green', 2: 'blue', 3: 'red', 4: 'default' })
const methodOptions = toOptions(PAYMENT_METHOD)

// Filter states
const filterStatus = ref(undefined)
const filterMethod = ref(undefined)
const filterDateRange = ref(null)

// Confirm modal states
const confirmOpen = ref(false)
const confirmTitle = ref('')
const confirmMsg = ref('')
const confirmType = ref('warning')
const confirmOkText = ref('Xác nhận')
const confirmLoading = ref(false)
let confirmActionCallback = null

// Table columns
const columns = [
  { title: 'Mã hóa đơn', key: 'invoiceId', width: 140 },
  { title: 'Học viên', key: 'studentNameSnapshot', width: 190 },
  { title: 'Số tiền', key: 'amount', type: 'money', width: 140, sorter: (a, b) => (a.amount || 0) - (b.amount || 0) },
  { title: 'Phương thức', key: 'method', width: 150 },
  { title: 'Ngày thanh toán', key: 'paymentDate', width: 140, sorter: (a, b) => new Date(a.paymentDate || 0) - new Date(b.paymentDate || 0) },
  { title: 'Người tạo', dataIndex: 'createdBy', key: 'createdBy', width: 140, ellipsis: true },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 130 },
]

// Form fields
const fields = [
  { name: 'invoiceId', label: 'Mã hóa đơn', required: true, default: '' },
  { name: 'amount', label: 'Số tiền', type: 'number', required: true, default: 0 },
  { name: 'method', label: 'Phương thức', type: 'select', options: methodOptions, default: 1 },
  { name: 'paymentDate', label: 'Ngày thanh toán', type: 'date', default: '' },
  { name: 'status', label: 'Trạng thái', type: 'select', options: statusOptions, default: 1 },
  { name: 'createdBy', label: 'Người tạo', default: 'admin' },
  { name: 'note', label: 'Ghi chú', type: 'textarea', fullWidth: true, default: '' },
]

const formGroups = [
  {
    title: 'Thông tin giao dịch',
    fields: ['invoiceId', 'amount', 'method', 'paymentDate']
  },
  {
    title: 'Thông tin hệ thống',
    fields: ['status', 'createdBy', 'note']
  }
]

function customFilter(item) {
  const matchStatus = filterStatus.value === undefined || statusValue(item.status) === Number(filterStatus.value)
  const matchMethod = filterMethod.value === undefined || methodValue(item.method) === Number(filterMethod.value)

  let matchDate = true
  if (filterDateRange.value && filterDateRange.value.length >= 2) {
    const [start, end] = filterDateRange.value
    const startDate = new Date(start)
    const endDate = new Date(end)
    endDate.setHours(23, 59, 59, 999)
    if (item.paymentDate) {
      const paymentDate = new Date(item.paymentDate)
      matchDate = paymentDate >= startDate && paymentDate <= endDate
    }
  }

  return matchStatus && matchMethod && matchDate
}

function resetCustomFilters() {
  filterStatus.value = undefined
  filterMethod.value = undefined
  filterDateRange.value = null
}

function canSelectPayment(record) {
  return statusValue(record.status) === 2
}

function canShowPaymentActions(record) {
  return statusValue(record.status) === 2
}

// Shorten UUID to readable prefix
function shortCode(code) {
  if (!code) return '—'
  if (code.length <= 12) return code
  // Try to find a short readable segment
  if (code.includes('-')) {
    const parts = code.split('-')
    return parts[0].toUpperCase()
  }
  return code.substring(0, 8).toUpperCase()
}

// Method icons and labels
const methodLabelMap = Object.fromEntries(methodOptions.map(o => [o.value, o.label]))
const methodNameToValue = { Cash: 1, BankTransfer: 2, Momo: 3, Vnpay: 4, VNPay: 4 }
const statusNameToValue = { Success: 1, Pending: 2, Processing: 2, Failed: 3, Cancelled: 4 }

function methodValue(method) {
  return Number.isFinite(Number(method)) ? Number(method) : methodNameToValue[method] || 0
}

function statusValue(status) {
  return Number.isFinite(Number(status)) ? Number(status) : statusNameToValue[status] || 0
}

function methodLabel(method) {
  return methodLabelMap[methodValue(method)] || method
}

function methodShortLabel(method) {
  const value = methodValue(method)
  const label = methodLabel(method)
  if (value === 2) return 'BT'
  if (value === 4) return 'VP'
  return String(label || '?').slice(0, 2).toUpperCase()
}

function formatDate(value) {
  if (!value) return '—'
  return new Date(value).toLocaleDateString('vi-VN')
}

// Confirm trigger helpers
function triggerCancelOne(id, refresh) {
  confirmTitle.value = 'Hủy yêu cầu thanh toán?'
  confirmMsg.value = 'Yêu cầu chờ xác nhận sẽ chuyển sang đã hủy. Công nợ của hóa đơn không thay đổi.'
  confirmType.value = 'danger'
  confirmOkText.value = 'Hủy yêu cầu'
  confirmActionCallback = async () => {
    await paymentApi.cancel(id)
    message.success('Đã hủy yêu cầu thanh toán')
    refresh()
  }
  confirmOpen.value = true
}

function triggerBulkCancel(ids, refresh) {
  confirmTitle.value = `Hủy ${ids.length} yêu cầu thanh toán?`
  confirmMsg.value = `Các yêu cầu chờ xác nhận đã chọn sẽ chuyển sang đã hủy.`
  confirmType.value = 'danger'
  confirmOkText.value = 'Hủy yêu cầu'
  confirmActionCallback = async () => {
    await paymentApi.bulkCancel(ids)
    message.success('Đã hủy các yêu cầu đã chọn')
    refresh()
  }
  confirmOpen.value = true
}

function triggerConfirmPayment(id, refresh) {
  confirmTitle.value = 'Xác nhận giao dịch thanh toán?'
  confirmMsg.value = 'Giao dịch sẽ chuyển sang thành công và hóa đơn tương ứng sẽ được cập nhật số tiền đã đóng.'
  confirmType.value = 'success'
  confirmOkText.value = 'Xác nhận thanh toán'
  confirmActionCallback = async () => {
    await paymentApi.confirm(id)
    message.success('Đã xác nhận thanh toán')
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
    message.error(error.message || 'Không thể thực hiện thao tác')
  } finally {
    confirmLoading.value = false
  }
}
</script>
