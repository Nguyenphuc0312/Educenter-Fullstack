<template>
  <div class="space-y-6">
    <PageHeader
      title="Tổng quan giảng viên"
      :subtitle="`Xin chào ${teacherName}. Theo dõi lớp phụ trách, lịch học, điểm danh và kết quả học tập.`"
    >
      <template #actions>
        <a-button :loading="loading" @click="loadData">Làm mới</a-button>
        <router-link to="/teacher/schedule">
          <a-button type="primary">Xem lịch tuần</a-button>
        </router-link>
      </template>
    </PageHeader>

    <section class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
      <div v-for="card in kpiCards" :key="card.label" class="teacher-stat-card">
        <div class="flex items-start justify-between gap-3">
          <div>
            <p class="text-xs font-semibold text-base-secondary">{{ card.label }}</p>
            <p class="text-2xl font-extrabold text-base-primary mt-2">{{ card.value }}</p>
            <p class="text-xs text-base-muted mt-1">{{ card.sub }}</p>
          </div>
          <div class="teacher-stat-icon" :style="{ background: card.bg, color: card.color }">
            <component :is="card.icon" />
          </div>
        </div>
      </div>
    </section>

    <section class="grid grid-cols-1 xl:grid-cols-3 gap-4">
      <div class="teacher-panel xl:col-span-2">
        <div class="teacher-panel-head">
          <div>
            <h2>Lịch dạy tuần này</h2>
            <p>Các buổi học được lấy từ CourseScheduleService theo mã giảng viên.</p>
          </div>
          <router-link to="/teacher/schedule" class="teacher-link">Chi tiết</router-link>
        </div>

        <div v-if="loading" class="teacher-loading"><a-spin /></div>
        <div v-else-if="weekSchedules.length === 0" class="teacher-empty">Tuần này chưa có lịch dạy.</div>
        <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-3">
          <div v-for="item in weekSchedules" :key="item.id" class="teacher-schedule-card">
            <div class="flex items-start justify-between gap-3">
              <div class="min-w-0">
                <p class="text-xs font-bold text-blue-600">{{ dayLabel(item.dayOfWeek) }} · {{ shiftLabel(item.studyShift) }}</p>
                <h3 class="font-bold text-base-primary truncate mt-1">{{ item.classNameSnapshot }}</h3>
                <p class="text-xs text-base-secondary mt-1">{{ item.topic || 'Chưa có chủ đề' }}</p>
              </div>
              <a-tag color="blue">{{ item.room || '-' }}</a-tag>
            </div>
            <div class="flex items-center justify-between text-xs text-base-secondary mt-3">
              <span>{{ formatTime(item.startTime) }} - {{ formatTime(item.endTime) }}</span>
              <span>Buổi {{ item.sessionNumber }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="teacher-panel">
        <div class="teacher-panel-head">
          <div>
            <h2>Công việc gần nhất</h2>
            <p>Điểm danh và nhập kết quả cần theo dõi.</p>
          </div>
        </div>
        <div class="space-y-3">
          <router-link v-for="item in actionItems" :key="item.to" :to="item.to" class="teacher-action-row">
            <div>
              <p class="font-semibold text-base-primary">{{ item.title }}</p>
              <p class="text-xs text-base-secondary mt-0.5">{{ item.sub }}</p>
            </div>
            <span>→</span>
          </router-link>
        </div>
      </div>
    </section>

    <section class="teacher-panel">
      <div class="teacher-panel-head">
        <div>
          <h2>Lớp đang phụ trách</h2>
          <p>Chọn lớp để xem học viên, lịch học, điểm danh hoặc nhập điểm.</p>
        </div>
        <router-link to="/teacher/classes" class="teacher-link">Tất cả lớp</router-link>
      </div>
      <a-table
        :data-source="classes"
        :columns="classColumns"
        :loading="loading"
        row-key="id"
        size="small"
        :pagination="{ pageSize: 5, showSizeChanger: false }"
        :scroll="{ x: 900 }"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'className'">
            <div>
              <p class="font-semibold text-base-primary">{{ record.className }}</p>
              <p class="text-xs text-base-secondary">{{ record.courseNameSnapshot }}</p>
            </div>
          </template>
          <template v-else-if="column.key === 'capacity'">
            <div class="min-w-[120px]">
              <div class="flex justify-between text-xs mb-1">
                <span>{{ record.currentStudents }}/{{ record.maxStudents }}</span>
                <span>{{ percent(record) }}%</span>
              </div>
              <a-progress :percent="percent(record)" :show-info="false" size="small" />
            </div>
          </template>
          <template v-else-if="column.key === 'time'">
            <span>{{ formatDate(record.startDate) }} - {{ formatDate(record.endDate) }}</span>
          </template>
          <template v-else-if="column.key === 'actions'">
            <div class="flex gap-2">
              <router-link :to="`/teacher/classes/${record.id}`"><a-button size="small">Chi tiết</a-button></router-link>
              <router-link :to="`/teacher/classes/${record.id}/attendance`"><a-button size="small">Điểm danh</a-button></router-link>
            </div>
          </template>
        </template>
      </a-table>
    </section>
  </div>
</template>

<script setup>
import { computed, h, onMounted, ref } from 'vue'
import { BookOutlined, CalendarOutlined, CheckCircleOutlined, TeamOutlined } from '@ant-design/icons-vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import { classApi } from '@/api/classApi'
import { scheduleApi } from '@/api/scheduleApi'
import { enrollmentApi } from '@/api/enrollmentApi'
import { attendanceApi } from '@/api/attendanceApi'
import { resultApi } from '@/api/resultApi'
import { useAuthStore } from '@/stores/auth'
import { formatDate, formatTime } from '@/lib/formatters'

const auth = useAuthStore()
const loading = ref(false)
const classes = ref([])
const schedules = ref([])
const enrollments = ref([])
const sessions = ref([])
const results = ref([])

const teacherName = computed(() => auth.user?.fullName || auth.user?.username || 'Giảng viên')

const classColumns = [
  { title: 'Lớp học', key: 'className', width: 260 },
  { title: 'Phòng', dataIndex: 'room', key: 'room', width: 110 },
  { title: 'Sĩ số', key: 'capacity', width: 180 },
  { title: 'Thời gian', key: 'time', width: 220 },
  { title: 'Thao tác', key: 'actions', width: 210, fixed: 'right' }
]

const weekSchedules = computed(() => schedules.value.slice().sort(sortSchedule).slice(0, 8))

const kpiCards = computed(() => [
  { label: 'Lớp phụ trách', value: classes.value.length, sub: 'Lớp gắn với tài khoản giảng viên', icon: () => h(BookOutlined), color: '#2563eb', bg: 'rgba(37,99,235,.12)' },
  { label: 'Học viên đang học', value: enrollments.value.length, sub: 'Tổng ghi danh trong các lớp', icon: () => h(TeamOutlined), color: '#059669', bg: 'rgba(5,150,105,.12)' },
  { label: 'Buổi điểm danh', value: sessions.value.length, sub: 'Session đã tạo trong lớp phụ trách', icon: () => h(CheckCircleOutlined), color: '#7c3aed', bg: 'rgba(124,58,237,.12)' },
  { label: 'Lịch dạy', value: schedules.value.length, sub: 'Buổi học theo tuần/ca', icon: () => h(CalendarOutlined), color: '#ea580c', bg: 'rgba(234,88,12,.12)' }
])

const actionItems = computed(() => [
  { title: 'Điểm danh lớp học', sub: `${sessions.value.filter(x => String(x.status) === 'Open' || Number(x.status) === 1).length} buổi đang mở`, to: '/teacher/attendance' },
  { title: 'Nhập kết quả học tập', sub: `${results.value.length} kết quả đã ghi nhận`, to: '/teacher/results' },
  { title: 'Xem lịch tuần', sub: `${schedules.value.length} buổi trong lịch phụ trách`, to: '/teacher/schedule' }
])

function percent(record) {
  if (!record.maxStudents) return 0
  return Math.min(100, Math.round((Number(record.currentStudents || 0) / Number(record.maxStudents)) * 100))
}

function dayValue(value) {
  const map = { Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3, Thursday: 4, Friday: 5, Saturday: 6 }
  return typeof value === 'number' ? value : map[value] ?? Number(value) ?? 0
}

function dayLabel(value) {
  return ['Chủ nhật', 'Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7'][dayValue(value)] || value
}

function shiftValue(value) {
  const map = { Morning: 0, Afternoon: 1, Evening: 2 }
  return typeof value === 'number' ? value : map[value] ?? Number(value) ?? 0
}

function shiftLabel(value) {
  return ['Sáng', 'Chiều', 'Tối'][shiftValue(value)] || value
}

function sortSchedule(a, b) {
  return dayValue(a.dayOfWeek) - dayValue(b.dayOfWeek) || shiftValue(a.studyShift) - shiftValue(b.studyShift) || String(a.startTime).localeCompare(String(b.startTime))
}

async function loadData() {
  if (!auth.user?.referenceId) return
  loading.value = true
  try {
    classes.value = await classApi.getByTeacher(auth.user.referenceId)
    schedules.value = await scheduleApi.getByTeacher(auth.user.referenceId)
    const classIds = classes.value.map(x => x.id)
    const enrollmentGroups = await Promise.all(classIds.map(id => enrollmentApi.getByClass(id).catch(() => [])))
    const sessionGroups = await Promise.all(classIds.map(id => attendanceApi.getSessionsByClass(id).catch(() => [])))
    const resultGroups = await Promise.all(classIds.map(id => resultApi.getByClass(id).catch(() => [])))
    enrollments.value = enrollmentGroups.flat()
    sessions.value = sessionGroups.flat()
    results.value = resultGroups.flat()
  } finally {
    loading.value = false
  }
}

onMounted(loadData)
</script>

<style scoped>
.teacher-stat-card,
.teacher-panel {
  background: var(--admin-surface);
  border: 1px solid var(--admin-border);
  border-radius: 18px;
  box-shadow: var(--admin-shadow-sm);
}
.teacher-stat-card { padding: 20px; transition: transform .2s ease, box-shadow .2s ease; }
.teacher-stat-card:hover { transform: translateY(-2px); box-shadow: var(--admin-shadow-md); }
.teacher-stat-icon { width: 42px; height: 42px; border-radius: 14px; display: grid; place-items: center; font-size: 20px; }
.teacher-panel { padding: 18px; }
.teacher-panel-head { display: flex; align-items: flex-start; justify-content: space-between; gap: 16px; margin-bottom: 16px; }
.teacher-panel-head h2 { color: var(--admin-text); font-weight: 800; font-size: 16px; }
.teacher-panel-head p { color: var(--admin-text-muted); font-size: 12px; margin-top: 2px; }
.teacher-link { color: var(--admin-accent); font-weight: 700; font-size: 12px; }
.teacher-loading,.teacher-empty { min-height: 140px; display: grid; place-items: center; color: var(--admin-text-muted); font-size: 13px; }
.teacher-schedule-card,.teacher-action-row { display: block; border: 1px solid var(--admin-border); background: var(--admin-surface-2); border-radius: 14px; padding: 14px; transition: transform .2s ease, border-color .2s ease; }
.teacher-schedule-card:hover,.teacher-action-row:hover { transform: translateY(-1px); border-color: rgba(37, 99, 235, .35); }
.teacher-action-row { display: flex; align-items: center; justify-content: space-between; }
</style>
