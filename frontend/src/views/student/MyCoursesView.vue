<template>
  <div class="space-y-6">
    <PageHeader
      title="Khóa học của tôi"
      subtitle="Theo dõi các lớp đã ghi danh và trạng thái học tập hiện tại."
    >
      <template #actions>
        <router-link to="/courses" class="student-primary-btn">Khám phá khóa học</router-link>
      </template>
    </PageHeader>

    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div class="student-mini-stat"><span>Tổng ghi danh</span><strong>{{ courses.length }}</strong></div>
      <div class="student-mini-stat"><span>Đang học</span><strong>{{ studyingCount }}</strong></div>
      <div class="student-mini-stat"><span>Đã hoàn thành</span><strong>{{ completedCount }}</strong></div>
    </div>

    <LoadingSpinner v-if="loading" size="lg" class="py-20" />
    <div v-else-if="error" class="student-empty">{{ error }}</div>
    <div v-else-if="courses.length === 0" class="student-card student-empty">
      Bạn chưa ghi danh khóa học nào.
    </div>
    <div v-else class="grid grid-cols-1 xl:grid-cols-2 gap-5">
      <article v-for="course in courses" :key="course.id" class="student-card">
        <div class="flex items-start justify-between gap-4">
          <div class="min-w-0">
            <p class="text-xs font-extrabold uppercase tracking-wider text-blue-600">Khóa học</p>
            <h2 class="text-xl font-black text-base-primary mt-1">{{ course.courseNameSnapshot }}</h2>
            <p class="text-sm text-base-secondary mt-2">{{ course.classNameSnapshot }}</p>
          </div>
          <span :class="statusClass(course.status)">{{ statusText(course.status) }}</span>
        </div>

        <div class="mt-6 grid grid-cols-2 gap-3">
          <div class="student-mini-stat">
            <span>Ngày ghi danh</span>
            <strong class="!text-base">{{ formatDate(course.enrolledAt) }}</strong>
          </div>
          <div class="student-mini-stat">
            <span>Tiến độ dự kiến</span>
            <strong class="!text-base">{{ progress(course.status) }}%</strong>
          </div>
        </div>

        <div class="mt-5">
          <ProgressBar :value="progress(course.status)" />
        </div>

        <div class="mt-6 flex flex-wrap gap-3">
          <router-link to="/student/schedule" class="student-secondary-btn">Xem lịch học</router-link>
          <router-link to="/student/results" class="student-secondary-btn">Xem kết quả</router-link>
        </div>
      </article>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import ProgressBar from '@/components/ProgressBar.vue'
import { studentApi } from '@/api/studentApi'
import { useAuthStore } from '@/stores/auth'
import { formatDate } from '@/lib/formatters'

const auth = useAuthStore()
const courses = ref([])
const loading = ref(true)
const error = ref('')

const studyingCount = computed(() => courses.value.filter(x => ['Confirmed', 'Studying'].includes(String(x.status))).length)
const completedCount = computed(() => courses.value.filter(x => x.status === 'Completed').length)

onMounted(loadData)

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    courses.value = auth.user?.referenceId ? await studentApi.getMyCourses(auth.user.referenceId) : []
  } catch (err) {
    error.value = err.message || 'Không tải được danh sách khóa học.'
  } finally {
    loading.value = false
  }
}

function statusText(status) {
  return ({ Pending: 'Chờ duyệt', Confirmed: 'Đã xác nhận', Studying: 'Đang học', Completed: 'Hoàn thành', Cancelled: 'Đã hủy' })[status] || status
}

function statusClass(status) {
  const base = 'student-status-pill '
  if (['Confirmed', 'Studying', 'Completed'].includes(String(status))) return base + 'is-green'
  if (status === 'Pending') return base + 'is-blue'
  return base + 'is-muted'
}

function progress(status) {
  return ({ Pending: 10, Confirmed: 25, Studying: 65, Completed: 100, Cancelled: 0 })[status] ?? 0
}
</script>
