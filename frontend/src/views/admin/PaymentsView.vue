<template>
  <div>
    <AdminResourceView
      title="Lịch sử thanh toán"
      subtitle="Ghi nhận giao dịch học phí, phương thức thanh toán và trạng thái xử lý."
      :api="paymentApi"
      :columns="columns"
      :fields="fields"
      :searchable-fields="['invoiceId', 'createdBy', 'note']"
      :status-options="statusOptions"
      :form-groups="formGroups"
      :filter-fn="customFilter"
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
          <a-select-option :value="2">Đang xử lý</a-select-option>
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
          Hủy giao dịch
        </a-button>
      </template>

      <!-- Row actions -->
      <template #rowActions="{ record, refresh }">
        <a-menu-item
          v-if="record.status === 2 || record.status === 'Pending'"
          class="rounded-lg px-3 py-2 text-xs text-emerald-600"
          @click="triggerConfirmOne(record.id, refresh)"
        >
          Xác nhận thanh toán
        </a-menu-item>
        <a-menu-item
          v-if="record.status === 2 || record.status === 'Pending'"
          class="rounded-lg px-3 py-2 text-xs text-rose-600"
          @click="triggerCancelOne(record.id, refresh)"
        >
          Hủy giao dịch
        </a-menu-item>
      </template>

      <!-- Custom Body Cells -->
      <template #bodyCell="{ column, record }">
        <!-- Invoice ID: short readable format -->
        <template v-if="column.key === 'invoiceId'">
          <span class="font-mono text-[11px] font-semibold text-slate-600 dark:text-slate-300 bg-slate-100 dark:bg-slate-800 px-1.5 py-0.5 rounded">
            {{ shortCode(record.invoiceId) }}
          </span>
        </template>

        <!-- Payment method with icon -->
        <template v-else-if="column.key === 'method'">
          <div class="flex items-center gap-2">
            <span class="text-base leading-none">{{ methodIcon(record.method) }}</span>
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
      type="danger"
      :loading="confirmLoading"
      ok-text="Hủy giao dịch"
      @confirm="handleExecuteAction"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { message } from 'ant-design-vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import ConfirmActionModal from '@/components/admin/ConfirmActionModal.vue'
import { paymentApi } from '@/api/paymentApi'
import { PAYMENT_METHOD, PAYMENT_STATUS, toOptions } from '@/lib/constants'
import { formatVnd } from '@/lib/formatters'

const statusOptions = toOptions(PAYMENT_STATUS, { 1: 'green', 2: 'blue', 3: 'red', 4: 'default' })
const methodOptions = toOptions(PAYMENT_METHOD).filter((item) => item.value !== 1)

// Filter states
const filterStatus = ref(undefined)
const filterMethod = ref(undefined)
const filterDateRange = ref(null)

// Confirm modal states
const confirmOpen = ref(false)
const confirmTitle = ref('')
const confirmMsg = ref('')
const confirmLoading = ref(false)
let confirmActionCallback = null

// Table columns
const columns = [
  { title: 'Mã hóa đơn', key: 'invoiceId', width: 140 },
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
  { name: 'method', label: 'Phương thức', type: 'select', options: methodOptions, default: 2 },
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
  const matchStatus = filterStatus.value === undefined || Number(item.status) === Number(filterStatus.value)
  const matchMethod = filterMethod.value === undefined || Number(item.method) === Number(filterMethod.value)

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
const methodIconMap = { 1: '💵', 2: '🏧', 3: '📱', 4: '🏦' }
const methodLabelMap = Object.fromEntries(methodOptions.map(o => [o.value, o.label]))

function methodIcon(method) {
  return methodIconMap[method] || '💳'
}

function methodLabel(method) {
  return methodLabelMap[method] || method
}

function formatDate(value) {
  if (!value) return '—'
  return new Date(value).toLocaleDateString('vi-VN')
}

// Confirm trigger helpers
function triggerConfirmOne(id, refresh) {
  confirmTitle.value = 'Xác nhận thanh toán?'
  confirmMsg.value = 'Giao dịch sẽ được ghi nhận thành công và cập nhật công nợ hóa đơn.'
  confirmActionCallback = async () => {
    await paymentApi.confirm(id)
    message.success('Đã xác nhận thanh toán')
    refresh()
  }
  confirmOpen.value = true
}

function triggerCancelOne(id, refresh) {
  confirmTitle.value = 'Hủy giao dịch này?'
  confirmMsg.value = 'Giao dịch sẽ bị đánh dấu là đã hủy và không thể hoàn nguyên.'
  confirmActionCallback = async () => {
    await paymentApi.cancel(id)
    message.success('Đã hủy giao dịch')
    refresh()
  }
  confirmOpen.value = true
}

function triggerBulkCancel(ids, refresh) {
  confirmTitle.value = `Hủy ${ids.length} giao dịch?`
  confirmMsg.value = `Tất cả ${ids.length} giao dịch đã chọn sẽ bị đánh dấu là đã hủy.`
  confirmActionCallback = async () => {
    await paymentApi.bulkCancel(ids)
    message.success('Đã hủy các giao dịch đã chọn')
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
    message.error(error.message || 'Không thể hủy giao dịch')
  } finally {
    confirmLoading.value = false
  }
}
</script>
