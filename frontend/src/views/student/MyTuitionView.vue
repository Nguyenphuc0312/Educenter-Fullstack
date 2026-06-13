<template>
  <div class="space-y-6">
    <PageHeader title="Học phí & Hóa đơn" subtitle="Theo dõi hóa đơn học phí, số tiền đã thanh toán và công nợ còn lại.">
      <template #actions>
        <button class="student-secondary-btn" type="button" @click="loadData">Làm mới</button>
      </template>
    </PageHeader>

    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div class="student-mini-stat"><span>Tổng học phí</span><strong>{{ formatVnd(summary.total) }}</strong></div>
      <div class="student-mini-stat"><span>Đã thanh toán</span><strong>{{ formatVnd(summary.paid) }}</strong></div>
      <div class="student-mini-stat"><span>Còn phải đóng</span><strong>{{ formatVnd(summary.debt) }}</strong></div>
    </div>

    <LoadingSpinner v-if="loading" size="lg" class="py-20" />
    <div v-else-if="error" class="student-empty">{{ error }}</div>
    <div v-else-if="invoices.length === 0" class="student-card student-empty">Chưa có hóa đơn học phí.</div>
    <div v-else class="space-y-4">
      <article v-for="invoice in invoices" :key="invoice.id" class="student-invoice-row">
        <div class="min-w-0">
          <div class="flex flex-wrap items-center gap-2">
            <h3>{{ invoice.courseNameSnapshot }}</h3>
            <span :class="statusClass(invoice.status)">{{ statusText(invoice.status) }}</span>
          </div>
          <p>{{ invoice.classNameSnapshot }} - Mã hóa đơn {{ invoice.invoiceCode }}</p>
          <p>Hạn thanh toán: {{ formatDate(invoice.dueDate) }}</p>
          <button
            v-if="getRemaining(invoice) > 0"
            type="button"
            class="student-secondary-btn mt-3"
            @click="openPaymentModal(invoice)"
          >
            Thanh toán học phí
          </button>
        </div>
        <div class="grid grid-cols-3 gap-3 w-full lg:w-[520px]">
          <div class="student-mini-stat"><span>Tổng</span><strong class="!text-base">{{ formatVnd(invoice.totalAmount) }}</strong></div>
          <div class="student-mini-stat"><span>Đã đóng</span><strong class="!text-base">{{ formatVnd(invoice.paidAmount) }}</strong></div>
          <div class="student-mini-stat"><span>Còn nợ</span><strong class="!text-base">{{ formatVnd(invoice.debtAmount) }}</strong></div>
        </div>
      </article>
    </div>

    <a-modal
      v-model:open="paymentModalOpen"
      width="520px"
      :footer="null"
      :destroy-on-close="true"
      :centered="true"
    >
      <template #title>
        <div class="text-sm font-bold pb-3 border-b border-slate-200">Thanh toán học phí</div>
      </template>

      <div v-if="selectedInvoice" class="mt-4 space-y-4">
        <div class="rounded-lg border border-slate-200 bg-slate-50 p-3">
          <div class="text-sm font-semibold text-slate-900">{{ selectedInvoice.courseNameSnapshot }}</div>
          <div class="text-xs text-slate-500 mt-1">{{ selectedInvoice.invoiceCode }} - Còn nợ {{ formatVnd(getRemaining(selectedInvoice)) }}</div>
        </div>

        <a-form ref="paymentFormRef" :model="paymentForm" layout="vertical">
          <a-form-item label="Mức thanh toán" name="percent">
            <a-radio-group v-model:value="paymentForm.percent" class="w-full" @change="syncAmountFromPercent">
              <a-radio-button v-for="option in paymentPercentOptions" :key="option.value" :value="option.value">
                {{ option.label }}
              </a-radio-button>
            </a-radio-group>
          </a-form-item>
          <a-form-item label="Số tiền thanh toán" name="amount" :rules="[{ required: true, message: 'Vui lòng chọn số tiền' }]">
            <a-input-number v-model:value="paymentForm.amount" class="w-full" :min="getMinimumPayment(selectedInvoice)" :max="getRemaining(selectedInvoice)" :disabled="true" />
          </a-form-item>
          <a-form-item label="Phương thức" name="method">
            <a-select v-model:value="paymentForm.method">
              <a-select-option v-for="option in methodOptions" :key="option.value" :value="option.value">
                {{ option.label }}
              </a-select-option>
            </a-select>
          </a-form-item>
          <a-form-item label="Ghi chú" name="note">
            <a-textarea v-model:value="paymentForm.note" :rows="3" :placeholder="Number(paymentForm.method) === 1 ? 'Ví dụ: Em sẽ đóng tiền mặt tại trung tâm...' : 'Ghi chú giao dịch nếu có...'" />
          </a-form-item>
        </a-form>

        <div class="flex justify-end gap-2 pt-4 border-t border-slate-200">
          <button type="button" class="student-secondary-btn" :disabled="paymentSaving" @click="paymentModalOpen = false">Đóng</button>
          <button type="button" class="student-secondary-btn" :disabled="paymentSaving" @click="submitPaymentRequest">
            {{ paymentSaving ? 'Đang xử lý...' : (Number(paymentForm.method) === 1 ? 'Gửi xác nhận tiền mặt' : 'Thanh toán ngay') }}
          </button>
        </div>
      </div>
    </a-modal>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { message } from 'ant-design-vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { tuitionApi } from '@/api/tuitionApi'
import { paymentApi } from '@/api/paymentApi'
import { useAuthStore } from '@/stores/auth'
import { PAYMENT_METHOD, toOptions } from '@/lib/constants'
import { formatDate, formatVnd } from '@/lib/formatters'

const auth = useAuthStore()
const invoices = ref([])
const loading = ref(true)
const error = ref('')
const paymentModalOpen = ref(false)
const paymentSaving = ref(false)
const selectedInvoice = ref(null)
const paymentFormRef = ref()
const methodOptions = toOptions(PAYMENT_METHOD)
const invoiceStatusNameToValue = { Unpaid: 1, Partial: 2, Paid: 3, Overdue: 4 }
const paymentPercentOptions = [
  { value: 25, label: '25%' },
  { value: 50, label: '50%' },
  { value: 75, label: '75%' },
  { value: 100, label: 'Thanh toán full' }
]
const paymentForm = reactive({
  amount: 0,
  percent: 100,
  method: 2,
  note: ''
})

const summary = computed(() => invoices.value.reduce((sum, item) => ({
  total: sum.total + Number(item.totalAmount || 0),
  paid: sum.paid + Number(item.paidAmount || 0),
  debt: sum.debt + Number(item.debtAmount || 0),
}), { total: 0, paid: 0, debt: 0 }))

onMounted(loadData)

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    invoices.value = auth.user?.referenceId ? await tuitionApi.getByStudent(auth.user.referenceId) : []
  } catch (err) {
    error.value = err.message || 'Không tải được hóa đơn học phí.'
  } finally {
    loading.value = false
  }
}

function getRemaining(invoice) {
  return Math.max(Number(invoice?.debtAmount ?? ((invoice?.totalAmount || 0) - (invoice?.paidAmount || 0))), 0)
}

function getMinimumPayment(invoice) {
  return Math.ceil(getRemaining(invoice) * 0.25)
}

function getAmountByPercent(invoice, percent) {
  const remaining = getRemaining(invoice)
  if (Number(percent) === 100) return remaining
  return Math.ceil(remaining * Number(percent) / 100)
}

function invoiceStatusValue(status) {
  return Number.isFinite(Number(status)) ? Number(status) : invoiceStatusNameToValue[status] || 0
}

function statusText(status) {
  return ({ 1: 'Chưa thanh toán', 2: 'Thanh toán một phần', 3: 'Đã thanh toán', 4: 'Quá hạn' })[invoiceStatusValue(status)] || status
}

function statusClass(status) {
  const base = 'student-status-pill '
  const value = invoiceStatusValue(status)
  if (value === 3) return base + 'is-green'
  if (value === 2) return base + 'is-blue'
  if (value === 4) return base + 'is-red'
  return base + 'is-muted'
}

function openPaymentModal(invoice) {
  selectedInvoice.value = invoice
  paymentForm.percent = 100
  paymentForm.amount = getAmountByPercent(invoice, paymentForm.percent)
  paymentForm.method = 2
  paymentForm.note = ''
  paymentModalOpen.value = true
}

function syncAmountFromPercent() {
  if (!selectedInvoice.value) return
  paymentForm.amount = getAmountByPercent(selectedInvoice.value, paymentForm.percent)
}

async function submitPaymentRequest() {
  if (!selectedInvoice.value) return
  try {
    await paymentFormRef.value?.validate()
    if (Number(paymentForm.amount) < getMinimumPayment(selectedInvoice.value) || Number(paymentForm.amount) > getRemaining(selectedInvoice.value)) {
      message.error('Số tiền thanh toán không hợp lệ')
      return
    }
    paymentSaving.value = true
    const isCash = Number(paymentForm.method) === 1
    await paymentApi.createStudentRequest({
      invoiceId: selectedInvoice.value.id,
      amount: Number(paymentForm.amount),
      method: paymentForm.method,
      paymentDate: new Date().toISOString(),
      status: isCash ? 2 : 1,
      createdBy: auth.user?.username || 'student',
      note: paymentForm.note || null
    })
    message.success(isCash ? 'Đã gửi yêu cầu tiền mặt cho admin xác nhận' : 'Thanh toán thành công')
    paymentModalOpen.value = false
    await loadData()
  } catch (err) {
    if (err?.errorFields) return
    message.error(err.message || 'Không thể xử lý thanh toán')
  } finally {
    paymentSaving.value = false
  }
}
</script>
