<template>
  <div class="space-y-8">
    <LoadingSpinner v-if="isLoading" size="lg" class="py-20" />
    <ErrorState
      v-else-if="error"
      title="Lỗi tải dữ liệu"
      :description="error"
      :onRetry="fetchDashboardData"
    />

    <template v-else>
      <PageHeader
        :title="`Xin chào, ${student?.fullName || user?.fullName || 'học viên'}`"
        :subtitle="`Mã học viên: ${student?.studentCode || 'N/A'} · Theo dõi khóa học, lịch học, chuyên cần, điểm số và học phí của bạn.`"
      >
        <template #actions>
          <router-link to="/courses" class="student-secondary-btn">Tìm khóa học</router-link>
          <router-link to="/student/schedule" class="student-primary-btn">Xem lịch tuần</router-link>
        </template>
      </PageHeader>

      <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
        <StatCard label="Khóa đang học" :value="`${activeCourses.length}`" hint="Lớp đã xác nhận" tone="blue" />
        <StatCard label="Chuyên cần" :value="formatPercent(attendancePercent, 0)" hint="Theo bản ghi điểm danh" tone="green" />
        <StatCard label="Điểm trung bình" :value="averageScoreText" hint="Từ kết quả đã nhập" tone="purple" />
        <StatCard label="Công nợ học phí" :value="formatVnd(tuitionSummary.debt)" hint="Hóa đơn còn phải thanh toán" tone="orange" />
      </div>

      <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">
        <section class="xl:col-span-2 student-card">
          <div class="student-section-head">
            <div>
              <h2>Khóa học của tôi</h2>
              <p>Trạng thái ghi danh và tiến độ học tập hiện tại.</p>
            </div>
            <router-link to="/student/courses" class="student-link">Xem tất cả</router-link>
          </div>

          <div v-if="courses.length === 0" class="student-empty">
            Bạn chưa có khóa học nào. Hãy chọn một khóa phù hợp để bắt đầu.
          </div>
          <div v-else class="space-y-4">
            <article v-for="course in courses.slice(0, 4)" :key="course.id" class="student-course-row">
              <div class="min-w-0">
                <div class="flex flex-wrap items-center gap-2">
                  <h3>{{ course.courseNameSnapshot || 'Khóa học' }}</h3>
                  <span :class="statusClass(course.status)">{{ statusText(course.status) }}</span>
                </div>
                <p>{{ course.classNameSnapshot || 'Chưa có lớp' }} · Ghi danh {{ formatDate(course.enrolledAt) }}</p>
              </div>
              <div class="w-full sm:w-44">
                <div class="flex justify-between text-xs font-semibold text-base-secondary mb-2">
                  <span>Tiến độ</span>
                  <span>{{ progressForCourse(course) }}%</span>
                </div>
                <ProgressBar :value="progressForCourse(course)" />
              </div>
            </article>
          </div>
        </section>

        <section class="student-card">
          <div class="student-section-head">
            <div>
              <h2>Lịch học gần nhất</h2>
              <p>Các buổi học theo lớp đang học.</p>
            </div>
          </div>

          <div v-if="upcomingSchedules.length === 0" class="student-empty">
            Chưa có lịch học sắp tới.
          </div>
          <div v-else class="space-y-3">
            <article v-for="schedule in upcomingSchedules.slice(0, 4)" :key="schedule.id" class="student-schedule-item">
              <div class="student-schedule-date">{{ shiftText(schedule.studyShift) }}</div>
              <div class="min-w-0">
                <h3>{{ schedule.classNameSnapshot || schedule.className || 'Lớp học' }}</h3>
                <p>{{ dayText(schedule.dayOfWeek) }} · {{ formatTime(schedule.startTime) }} - {{ formatTime(schedule.endTime) }}</p>
                <p>{{ schedule.room || 'Phòng học chưa cập nhật' }}</p>
              </div>
            </article>
          </div>
        </section>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <section class="student-card">
          <div class="student-section-head">
            <div>
              <h2>Kết quả học tập</h2>
              <p>Điểm mới nhất theo từng khóa.</p>
            </div>
            <router-link to="/student/results" class="student-link">Chi tiết</router-link>
          </div>
          <div v-if="results.length === 0" class="student-empty">Chưa có kết quả học tập.</div>
          <div v-else class="space-y-3">
            <article v-for="result in results.slice(0, 3)" :key="result.id" class="student-result-row">
              <div>
                <h3>{{ result.courseNameSnapshot }}</h3>
                <p>{{ result.classNameSnapshot }}</p>
              </div>
              <div class="text-right">
                <strong>{{ formatScore(result.averageScore) }}</strong>
                <span :class="statusClass(result.resultStatus)">{{ statusText(result.resultStatus) }}</span>
              </div>
            </article>
          </div>
        </section>

        <section class="student-card">
          <div class="student-section-head">
            <div>
              <h2>Học phí</h2>
              <p>Tình trạng hóa đơn và thanh toán.</p>
            </div>
            <router-link to="/student/tuition" class="student-link">Xem hóa đơn</router-link>
          </div>
          <div class="grid grid-cols-2 gap-3">
            <div class="student-mini-stat">
              <span>Đã thanh toán</span>
              <strong>{{ formatVnd(tuitionSummary.paid) }}</strong>
            </div>
            <div class="student-mini-stat">
              <span>Còn nợ</span>
              <strong>{{ formatVnd(tuitionSummary.debt) }}</strong>
            </div>
          </div>
        </section>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, defineComponent, h, onMounted, ref } from 'vue'
import { useAuthStore } from '../../stores/auth'
import { studentApi } from '../../api/studentApi'
import { tuitionApi } from '../../api/tuitionApi'
import { scheduleApi } from '../../api/scheduleApi'
import PageHeader from '../../components/ui/PageHeader.vue'
import LoadingSpinner from '../../components/ui/LoadingSpinner.vue'
import ErrorState from '../../components/ui/ErrorState.vue'
import ProgressBar from '../../components/ProgressBar.vue'
import { formatDate, formatPercent, formatScore, formatTime, formatVnd } from '../../lib/formatters'

const StatCard = defineComponent({
  props: { label: String, value: String, hint: String, tone: String },
  setup(props) {
    const tones = {
      blue: 'from-blue-500/12 to-cyan-500/8 text-blue-600',
      green: 'from-emerald-500/12 to-teal-500/8 text-emerald-600',
      purple: 'from-violet-500/12 to-fuchsia-500/8 text-violet-600',
      orange: 'from-amber-500/14 to-orange-500/8 text-orange-600',
    }
    return () => h('article', { class: 'student-stat-card' }, [
      h('div', { class: ['student-stat-icon', tones[props.tone] || tones.blue] }, '●'),
      h('div', {}, [
        h('p', { class: 'student-stat-label' }, props.label),
        h('strong', { class: 'student-stat-value' }, props.value),
        h('span', { class: 'student-stat-hint' }, props.hint),
      ]),
    ])
  },
})

const authStore = useAuthStore()
const user = computed(() => authStore.user)

const profile = ref(null)
const invoices = ref([])
const schedules = ref([])
const isLoading = ref(true)
const error = ref(null)

const student = computed(() => profile.value?.student || null)
const courses = computed(() => profile.value?.courses || [])
const results = computed(() => profile.value?.results || [])
const activeCourses = computed(() => courses.value.filter(x => ['Confirmed', 'Studying'].includes(String(x.status))))
const attendancePercent = computed(() => profile.value?.attendanceSummary?.attendancePercent ?? averageAttendanceFromResults.value)
const averageAttendanceFromResults = computed(() => {
  if (!results.value.length) return 0
  return results.value.reduce((sum, item) => sum + Number(item.attendancePercent || 0), 0) / results.value.length
})
const averageScoreText = computed(() => {
  if (!results.value.length) return '-'
  const avg = results.value.reduce((sum, item) => sum + Number(item.averageScore || 0), 0) / results.value.length
  return formatScore(avg)
})
const tuitionSummary = computed(() => invoices.value.reduce((sum, invoice) => ({
  paid: sum.paid + Number(invoice.paidAmount || 0),
  debt: sum.debt + Number(invoice.debtAmount || 0),
}), { paid: 0, debt: 0 }))
const upcomingSchedules = computed(() => schedules.value.slice().sort((a, b) => Number(a.sessionNumber || 0) - Number(b.sessionNumber || 0)))

onMounted(fetchDashboardData)

async function fetchDashboardData() {
  if (!user.value?.referenceId) {
    isLoading.value = false
    return
  }
  isLoading.value = true
  error.value = null
  try {
    const studentId = user.value.referenceId
    const [profileData, invoiceData] = await Promise.all([
      studentApi.getLearningProfile(studentId),
      tuitionApi.getByStudent(studentId).catch(() => []),
    ])
    profile.value = profileData
    invoices.value = Array.isArray(invoiceData) ? invoiceData : []

    const classIds = [...new Set((profileData?.courses || []).map(x => x.classId).filter(Boolean))]
    const scheduleGroups = await Promise.all(classIds.slice(0, 4).map(id => scheduleApi.getByClass(id).catch(() => [])))
    schedules.value = scheduleGroups.flat()
  } catch (err) {
    console.error(err)
    error.value = 'Không thể tải dữ liệu tổng quan học viên. Vui lòng kiểm tra API Gateway và đăng nhập lại.'
  } finally {
    isLoading.value = false
  }
}

function progressForCourse(course) {
  const result = results.value.find(x => String(x.classId) === String(course.classId))
  if (result?.resultStatus === 'Passed') return 100
  if (result?.resultStatus === 'Failed') return 100
  if (course.status === 'Completed') return 100
  if (course.status === 'Studying') return 65
  if (course.status === 'Confirmed') return 25
  return 10
}

function statusText(status) {
  const map = {
    Pending: 'Chờ duyệt',
    Confirmed: 'Đã xác nhận',
    Studying: 'Đang học',
    Completed: 'Hoàn thành',
    Cancelled: 'Đã hủy',
    Passed: 'Đạt',
    Failed: 'Chưa đạt',
    InProgress: 'Đang học',
    Paid: 'Đã thanh toán',
    Partial: 'Thanh toán một phần',
    Unpaid: 'Chưa thanh toán',
    Overdue: 'Quá hạn',
  }
  return map[status] || status || '-'
}

function statusClass(status) {
  const base = 'student-status-pill '
  if (['Studying', 'Confirmed', 'Paid', 'Passed'].includes(String(status))) return base + 'is-green'
  if (['Pending', 'Partial', 'InProgress'].includes(String(status))) return base + 'is-blue'
  if (['Overdue', 'Failed'].includes(String(status))) return base + 'is-red'
  return base + 'is-muted'
}

function shiftText(shift) {
  return ({ Morning: 'Sáng', Afternoon: 'Chiều', Evening: 'Tối' })[shift] || shift || 'Ca học'
}

function dayText(day) {
  return ({ Monday: 'Thứ 2', Tuesday: 'Thứ 3', Wednesday: 'Thứ 4', Thursday: 'Thứ 5', Friday: 'Thứ 6', Saturday: 'Thứ 7', Sunday: 'Chủ nhật' })[day] || day || 'Ngày học'
}
</script>
