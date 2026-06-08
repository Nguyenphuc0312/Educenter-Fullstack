<template>
  <div class="space-y-6">
    <PageHeader title="Lịch học" subtitle="Các buổi học được tổng hợp từ những lớp bạn đang theo học.">
      <template #actions>
        <button class="student-secondary-btn" type="button" @click="loadData">Làm mới</button>
      </template>
    </PageHeader>

    <div class="student-card">
      <div class="student-section-head">
        <div>
          <h2>Lịch học theo tuần</h2>
          <p>{{ schedules.length }} buổi học · {{ courseCount }} lớp</p>
        </div>
        <div class="flex items-center gap-2 text-sm font-bold text-base-secondary">
          <span class="student-status-pill is-blue">Tuần hiện tại</span>
        </div>
      </div>

      <LoadingSpinner v-if="loading" size="lg" class="py-16" />
      <div v-else-if="error" class="student-empty">{{ error }}</div>
      <div v-else-if="schedules.length === 0" class="student-empty">Chưa có lịch học cho các lớp đang học.</div>
      <div v-else class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
        <article v-for="schedule in schedules" :key="schedule.id" class="student-schedule-card">
          <div class="flex items-center justify-between gap-3">
            <span class="student-status-pill is-blue">{{ dayText(schedule.dayOfWeek) }}</span>
            <span class="text-xs font-extrabold text-base-muted">Buổi {{ schedule.sessionNumber || '-' }}</span>
          </div>
          <h3>{{ schedule.classNameSnapshot || 'Lớp học' }}</h3>
          <p>{{ schedule.topic || 'Nội dung buổi học sẽ được cập nhật' }}</p>
          <div class="student-schedule-meta">
            <span>{{ shiftText(schedule.studyShift) }}</span>
            <strong>{{ formatTime(schedule.startTime) }} - {{ formatTime(schedule.endTime) }}</strong>
          </div>
          <div class="student-schedule-meta">
            <span>Phòng học</span>
            <strong>{{ schedule.room || '-' }}</strong>
          </div>
        </article>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { studentApi } from '@/api/studentApi'
import { scheduleApi } from '@/api/scheduleApi'
import { useAuthStore } from '@/stores/auth'
import { formatTime } from '@/lib/formatters'

const auth = useAuthStore()
const schedules = ref([])
const enrollments = ref([])
const loading = ref(true)
const error = ref('')
const courseCount = computed(() => new Set(enrollments.value.map(x => x.classId)).size)

onMounted(loadData)

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    enrollments.value = auth.user?.referenceId ? await studentApi.getMyCourses(auth.user.referenceId) : []
    const classIds = [...new Set(enrollments.value.map(x => x.classId).filter(Boolean))]
    const groups = await Promise.all(classIds.map(id => scheduleApi.getByClass(id).catch(() => [])))
    schedules.value = groups.flat().sort((a, b) => dayOrder(a.dayOfWeek) - dayOrder(b.dayOfWeek) || String(a.startTime).localeCompare(String(b.startTime)))
  } catch (err) {
    error.value = err.message || 'Không tải được lịch học.'
  } finally {
    loading.value = false
  }
}

function dayOrder(day) {
  return ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'].indexOf(day)
}
function dayText(day) {
  return ({ Monday: 'Thứ 2', Tuesday: 'Thứ 3', Wednesday: 'Thứ 4', Thursday: 'Thứ 5', Friday: 'Thứ 6', Saturday: 'Thứ 7', Sunday: 'Chủ nhật' })[day] || day
}
function shiftText(shift) {
  return ({ Morning: 'Ca sáng', Afternoon: 'Ca chiều', Evening: 'Ca tối' })[shift] || shift
}
</script>

<style scoped>
.student-schedule-card {
  padding: 18px;
  border: 1px solid rgba(148, 163, 184, 0.18);
  border-radius: 20px;
  background: rgba(148, 163, 184, 0.045);
}
.student-schedule-card h3 {
  margin: 16px 0 4px;
  color: var(--admin-text-primary, #0f172a);
  font-size: 16px;
  font-weight: 850;
}
.student-schedule-card > p {
  min-height: 40px;
  color: var(--admin-text-secondary, #64748b);
  font-size: 13px;
}
.student-schedule-meta {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid rgba(148, 163, 184, 0.16);
  color: var(--admin-text-secondary, #64748b);
  font-size: 13px;
}
.student-schedule-meta strong {
  color: var(--admin-text-primary, #0f172a);
}
</style>
