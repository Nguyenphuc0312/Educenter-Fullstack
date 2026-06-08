<template>
  <div class="space-y-6">
    <PageHeader title="Chuyên cần" subtitle="Theo dõi lịch sử điểm danh và tỷ lệ tham gia học tập." />

    <div class="grid grid-cols-1 sm:grid-cols-4 gap-4">
      <div class="student-mini-stat"><span>Tổng buổi</span><strong>{{ records.length }}</strong></div>
      <div class="student-mini-stat"><span>Có mặt</span><strong>{{ countByStatus('Present') }}</strong></div>
      <div class="student-mini-stat"><span>Đi muộn / Có phép</span><strong>{{ countByStatus('Late') + countByStatus('Excused') }}</strong></div>
      <div class="student-mini-stat"><span>Tỷ lệ</span><strong>{{ attendanceRate }}%</strong></div>
    </div>

    <section class="student-card">
      <div class="student-section-head">
        <div>
          <h2>Lịch sử điểm danh</h2>
          <p>Dữ liệu lấy trực tiếp từ các buổi điểm danh của giảng viên.</p>
        </div>
      </div>

      <LoadingSpinner v-if="loading" size="lg" class="py-16" />
      <div v-else-if="error" class="student-empty">{{ error }}</div>
      <div v-else-if="records.length === 0" class="student-empty">Chưa có bản ghi điểm danh.</div>
      <div v-else class="student-table-card">
        <a-table
          :data-source="records"
          :columns="columns"
          row-key="id"
          size="middle"
          :pagination="{ pageSize: 8, showSizeChanger: false }"
          :scroll="{ x: 760 }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'markedAt'">{{ formatDateTime(record.markedAt) }}</template>
            <template v-else-if="column.key === 'status'">
              <span :class="statusClass(record.status)">{{ statusText(record.status) }}</span>
            </template>
            <template v-else-if="column.key === 'note'">{{ record.note || '-' }}</template>
          </template>
        </a-table>
      </div>
    </section>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { studentApi } from '@/api/studentApi'
import { useAuthStore } from '@/stores/auth'
import { formatDateTime } from '@/lib/formatters'

const auth = useAuthStore()
const records = ref([])
const loading = ref(true)
const error = ref('')
const columns = [
  { title: 'Ngày điểm danh', dataIndex: 'markedAt', key: 'markedAt' },
  { title: 'Học viên', dataIndex: 'studentNameSnapshot', key: 'studentNameSnapshot' },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status' },
  { title: 'Ghi chú', dataIndex: 'note', key: 'note' },
]

const attendanceRate = computed(() => {
  if (!records.value.length) return 0
  const point = records.value.reduce((sum, item) => {
    if (item.status === 'Present') return sum + 1
    if (['Late', 'Excused'].includes(item.status)) return sum + 0.5
    return sum
  }, 0)
  return Math.round((point / records.value.length) * 100)
})

onMounted(loadData)

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    records.value = auth.user?.referenceId ? await studentApi.getMyAttendance(auth.user.referenceId) : []
  } catch (err) {
    error.value = err.message || 'Không tải được dữ liệu chuyên cần.'
  } finally {
    loading.value = false
  }
}

function countByStatus(status) {
  return records.value.filter(x => x.status === status).length
}
function statusText(status) {
  return ({ Present: 'Có mặt', Absent: 'Vắng', Late: 'Đi muộn', Excused: 'Có phép' })[status] || status
}
function statusClass(status) {
  const base = 'student-status-pill '
  if (status === 'Present') return base + 'is-green'
  if (['Late', 'Excused'].includes(status)) return base + 'is-blue'
  if (status === 'Absent') return base + 'is-red'
  return base + 'is-muted'
}
</script>
