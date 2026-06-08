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
          <p>{{ invoice.classNameSnapshot }} · Mã hóa đơn {{ invoice.invoiceCode }}</p>
          <p>Hạn thanh toán: {{ formatDate(invoice.dueDate) }}</p>
        </div>
        <div class="grid grid-cols-3 gap-3 w-full lg:w-[520px]">
          <div class="student-mini-stat"><span>Tổng</span><strong class="!text-base">{{ formatVnd(invoice.totalAmount) }}</strong></div>
          <div class="student-mini-stat"><span>Đã đóng</span><strong class="!text-base">{{ formatVnd(invoice.paidAmount) }}</strong></div>
          <div class="student-mini-stat"><span>Còn nợ</span><strong class="!text-base">{{ formatVnd(invoice.debtAmount) }}</strong></div>
        </div>
      </article>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { tuitionApi } from '@/api/tuitionApi'
import { useAuthStore } from '@/stores/auth'
import { formatDate, formatVnd } from '@/lib/formatters'

const auth = useAuthStore()
const invoices = ref([])
const loading = ref(true)
const error = ref('')

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

function statusText(status) {
  return ({ Paid: 'Đã thanh toán', Partial: 'Thanh toán một phần', Unpaid: 'Chưa thanh toán', Overdue: 'Quá hạn' })[status] || status
}
function statusClass(status) {
  const base = 'student-status-pill '
  if (status === 'Paid') return base + 'is-green'
  if (status === 'Partial') return base + 'is-blue'
  if (status === 'Overdue') return base + 'is-red'
  return base + 'is-muted'
}
</script>
