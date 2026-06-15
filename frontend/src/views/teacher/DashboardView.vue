<template>
  <div class="space-y-6">
    <div class="relative bg-gradient-to-r from-blue-700 to-indigo-800 rounded-2xl p-6 md:p-8 shadow-lg overflow-hidden flex flex-col md:flex-row md:items-center justify-between gap-6">
      <div class="absolute inset-0 opacity-10 bg-[url('https://www.transparenttextures.com/patterns/cubes.png')] pointer-events-none"></div>
      
      <div class="relative z-10 text-white min-w-0">
        <div class="flex items-center gap-2 text-blue-100 text-sm font-semibold mb-2 uppercase tracking-wider">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
          <span>{{ currentDateTime }}</span>
        </div>
        <h1 class="text-3xl md:text-4xl font-black mb-2 leading-tight">Xin chào, {{ teacherName }}!</h1>
        <p class="text-blue-100 max-w-xl text-sm md:text-base">Chào mừng trở lại bảng điều khiển. Theo dõi lịch giảng dạy, quản lý học viên và cập nhật điểm danh ngay tại đây.</p>
      </div>

      <div class="relative z-10 flex gap-3 shrink-0 w-full md:w-auto">
        <button
          class="flex-1 md:flex-none px-4 py-2.5 bg-white/10 hover:bg-white/20 text-white font-semibold rounded-xl backdrop-blur-sm transition-all border border-white/20 flex items-center justify-center gap-2 disabled:opacity-50"
          :disabled="loading"
          @click="loadData"
        >
          <LoadingSpinner v-if="loading" size="sm" class="!text-white" />
          <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>
          Làm mới
        </button>
        <router-link to="/teacher/schedule" class="flex-1 md:flex-none">
          <button class="w-full px-5 py-2.5 bg-white text-blue-700 hover:bg-blue-50 font-bold rounded-xl transition-all shadow-md flex items-center justify-center gap-2">
            Lịch tuần <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" /></svg>
          </button>
        </router-link>
      </div>
    </div>

    <section class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
      <div v-for="card in kpiCards" :key="card.label" class="bg-white p-5 rounded-2xl border border-slate-100 shadow-sm hover:shadow-md transition-all group overflow-hidden relative">
        <div class="absolute -right-4 -top-4 w-20 h-20 rounded-full blur-2xl opacity-20 transition-transform group-hover:scale-110" :style="{ background: card.color }"></div>
        <div class="flex items-start justify-between gap-3 relative z-10">
          <div class="w-12 h-12 rounded-xl flex items-center justify-center text-xl shrink-0 border" :style="{ background: card.bg, color: card.color, borderColor: card.borderColor }">
            <component :is="card.icon" />
          </div>
        </div>
        <div class="mt-4 relative z-10">
          <p class="text-[11px] font-bold text-slate-400 uppercase tracking-wider mb-1">{{ card.label }}</p>
          <p class="text-3xl font-black text-slate-800">{{ card.value }}</p>
          <p class="text-xs text-slate-500 mt-1 truncate">{{ card.sub }}</p>
        </div>
      </div>
    </section>

    <section class="grid grid-cols-1 xl:grid-cols-3 gap-6">
      
      <div class="xl:col-span-2 bg-white rounded-2xl border border-slate-100 shadow-sm flex flex-col h-[380px]">
        <div class="p-5 border-b border-slate-50 flex items-center justify-between">
          <div class="flex items-center gap-2">
            <span class="w-1.5 h-4 bg-blue-500 rounded-full"></span>
            <h2 class="text-base font-bold text-slate-800">Lịch dạy tuần này</h2>
          </div>
          <router-link to="/teacher/schedule" class="text-xs font-semibold text-blue-600 hover:text-blue-700 transition-colors">Xem toàn bộ lịch →</router-link>
        </div>

        <div class="flex-1 overflow-y-auto custom-scrollbar p-5 bg-slate-50/30">
          <div v-if="loading" class="h-full flex items-center justify-center"><LoadingSpinner size="lg" /></div>
          <div v-else-if="weekSchedules.length === 0" class="h-full flex flex-col items-center justify-center text-center">
            <div class="w-16 h-16 bg-blue-50 text-blue-300 rounded-full flex items-center justify-center mb-3">
              <CalendarOutlined class="text-2xl" />
            </div>
            <p class="text-sm font-bold text-slate-600">Tuyệt vời, tuần này bạn được nghỉ!</p>
            <p class="text-xs text-slate-400 mt-1">Chưa có buổi học nào được phân công trong tuần này.</p>
          </div>
          <div v-else class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div v-for="item in weekSchedules" :key="item.id" class="bg-white p-4 rounded-xl border border-slate-200 shadow-sm hover:border-blue-300 hover:shadow-md transition-all group relative overflow-hidden">
              <div class="absolute top-0 left-0 w-1 h-full bg-blue-500 opacity-0 group-hover:opacity-100 transition-opacity"></div>
              
              <div class="flex justify-between items-start mb-2">
                <span class="px-2 py-0.5 rounded text-[10px] font-black uppercase tracking-wider bg-blue-50 text-blue-700 border border-blue-100">
                  {{ dayLabel(item.dayOfWeek) }} · {{ shiftLabel(item.studyShift) }}
                </span>
                <span class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Buổi {{ item.sessionNumber }}</span>
              </div>
              
              <h3 class="font-bold text-slate-800 text-sm truncate" :title="item.classNameSnapshot">{{ item.classNameSnapshot || 'Đang cập nhật lớp' }}</h3>
              <p class="text-xs text-slate-500 mt-1 truncate" :title="item.topic">{{ item.topic || 'Giáo án đang cập nhật' }}</p>
              
              <div class="mt-4 pt-3 border-t border-slate-100 flex items-center justify-between text-xs font-medium text-slate-600">
                <span class="flex items-center gap-1.5">
                  <svg class="w-3.5 h-3.5 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                  {{ formatTime(item.startTime) }} - {{ formatTime(item.endTime) }}
                </span>
                <span class="flex items-center gap-1.5 px-2 py-1 bg-slate-50 rounded-md border border-slate-200">
                  <svg class="w-3.5 h-3.5 text-emerald-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" /></svg>
                  P.{{ item.room || 'Trống' }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-2xl border border-slate-100 shadow-sm flex flex-col h-[380px]">
        <div class="p-5 border-b border-slate-50 flex items-center justify-between">
          <div class="flex items-center gap-2">
            <span class="w-1.5 h-4 bg-emerald-500 rounded-full"></span>
            <h2 class="text-base font-bold text-slate-800">Thao tác nhanh</h2>
          </div>
        </div>
        
        <div class="flex-1 p-5 space-y-3">
          <router-link 
            v-for="(item, idx) in actionItems" 
            :key="idx" 
            :to="item.to" 
            class="group block p-4 rounded-xl border border-slate-200 hover:border-emerald-300 hover:bg-emerald-50/30 hover:shadow-md transition-all"
          >
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-3">
                <div class="w-10 h-10 rounded-lg flex items-center justify-center shrink-0 border" :class="item.iconClass">
                  <component :is="item.icon" />
                </div>
                <div>
                  <h4 class="font-bold text-slate-800 text-sm group-hover:text-emerald-700 transition-colors">{{ item.title }}</h4>
                  <p class="text-xs text-slate-500 mt-0.5">{{ item.sub }}</p>
                </div>
              </div>
              <svg class="w-4 h-4 text-slate-300 group-hover:text-emerald-500 transition-transform group-hover:translate-x-1" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" /></svg>
            </div>
          </router-link>
        </div>
      </div>

    </section>

    <section class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden">
      <div class="p-5 border-b border-slate-50 flex items-center justify-between bg-slate-50/50">
        <div>
          <h2 class="text-base font-bold text-slate-800">Lớp đang phụ trách</h2>
          <p class="text-xs text-slate-500 mt-1">Danh sách lớp được phân công giảng dạy.</p>
        </div>
        <router-link to="/teacher/classes" class="px-4 py-2 text-xs font-bold bg-white border border-slate-200 text-slate-600 hover:text-blue-600 hover:border-blue-300 rounded-lg shadow-sm transition-colors">Tất cả lớp học</router-link>
      </div>

      <div class="p-0">
        <a-table
          :data-source="classes"
          :columns="classColumns"
          :loading="loading"
          row-key="id"
          class="enterprise-table"
          :pagination="{ pageSize: 5, showSizeChanger: false }"
          :scroll="{ x: 900 }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'className'">
              <div class="flex items-center gap-3 py-1">
                <div class="w-10 h-10 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center font-black text-sm border border-blue-100 shadow-sm">
                  {{ getInitials(record.className) }}
                </div>
                <div>
                  <p class="font-bold text-slate-800 text-sm hover:text-blue-600 transition-colors cursor-pointer" @click="$router.push(`/teacher/classes/${record.id}`)">{{ record.className }}</p>
                  <p class="text-xs text-slate-500 font-medium truncate max-w-[250px] mt-0.5">{{ record.courseNameSnapshot }}</p>
                </div>
              </div>
            </template>
            
            <template v-else-if="column.key === 'capacity'">
              <div class="min-w-[140px] max-w-[180px] space-y-1.5">
                <div class="flex justify-between text-[11px] font-bold text-slate-600 font-variant-numeric">
                  <span class="flex items-center gap-1.5"><TeamOutlined class="text-slate-400" /> {{ record.currentStudents }}/{{ record.maxStudents }}</span>
                  <span :class="getPercentColorText(percent(record))">{{ percent(record) }}%</span>
                </div>
                <div class="w-full h-1.5 bg-slate-100 rounded-full overflow-hidden shadow-inner">
                  <div class="h-full rounded-full transition-all duration-500" :class="getPercentBgColor(percent(record))" :style="{ width: `${percent(record)}%` }"></div>
                </div>
              </div>
            </template>
            
            <template v-else-if="column.key === 'time'">
              <div class="text-xs font-semibold text-slate-700 bg-slate-50 px-2.5 py-1.5 rounded-lg border border-slate-100 inline-flex items-center gap-1.5">
                <CalendarOutlined class="text-slate-400" />
                {{ formatDate(record.startDate) }} - {{ formatDate(record.endDate) }}
              </div>
            </template>
            
            <template v-else-if="column.key === 'actions'">
              <div class="flex items-center gap-2">
                <router-link :to="`/teacher/classes/${record.id}`">
                  <button class="px-3 py-1.5 bg-white border border-slate-200 text-slate-600 hover:text-blue-600 hover:border-blue-300 font-semibold rounded text-[11px] transition-colors shadow-sm">Chi tiết</button>
                </router-link>
                <router-link :to="`/teacher/classes/${record.id}/attendance`">
                  <button class="px-3 py-1.5 bg-emerald-50 text-emerald-600 border border-emerald-200 hover:bg-emerald-100 font-bold rounded text-[11px] transition-colors shadow-sm">Điểm danh</button>
                </router-link>
              </div>
            </template>
          </template>

          <template #emptyText>
            <div class="py-12 flex flex-col items-center">
              <BookOutlined class="text-4xl text-slate-300 mb-3" />
              <p class="font-bold text-slate-600">Bạn chưa được phân công lớp nào</p>
            </div>
          </template>
        </a-table>
      </div>
    </section>
  </div>
</template>

<script setup>
import { computed, h, onMounted, ref } from 'vue'
import { BookOutlined, CalendarOutlined, CheckCircleOutlined, TeamOutlined, SyncOutlined, CheckSquareOutlined } from '@ant-design/icons-vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { classApi } from '@/api/classApi'
import { scheduleApi } from '@/api/scheduleApi'
import { enrollmentApi } from '@/api/enrollmentApi'
import { attendanceApi } from '@/api/attendanceApi'
import { resultApi } from '@/api/resultApi'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const loading = ref(false)
const classes = ref([])
const schedules = ref([])
const enrollments = ref([])
const sessions = ref([])
const results = ref([])

const teacherName = computed(() => auth.user?.fullName || auth.user?.username || 'Giảng viên')

// Current DateTime formatting
const currentDateTime = ref('')
const updateTime = () => {
  currentDateTime.value = new Date().toLocaleString('vi-VN', { weekday: 'long', day: '2-digit', month: 'long', year: 'numeric', hour: '2-digit', minute:'2-digit' })
}
setInterval(updateTime, 60000)
updateTime()

// Cột Table 
const classColumns = [
  { title: 'Thông tin Lớp học', key: 'className', width: 280 },
  { title: 'Phòng', dataIndex: 'room', key: 'room', width: 100, align: 'center' },
  { title: 'Sĩ số', key: 'capacity', width: 200 },
  { title: 'Thời gian đào tạo', key: 'time', width: 240 },
  { title: 'Thao tác', key: 'actions', width: 180, align: 'right' }
]

const weekSchedules = computed(() => schedules.value.slice().sort(sortSchedule).slice(0, 4)) // Hiển thị tối đa 4 buổi

const kpiCards = computed(() => [
  { label: 'Lớp phụ trách', value: classes.value.length, sub: 'Đã xếp lịch trong học kỳ', icon: () => h(BookOutlined), color: '#2563eb', bg: '#eff6ff', borderColor: '#bfdbfe' },
  { label: 'Học viên', value: enrollments.value.length, sub: 'Tổng học viên đang quản lý', icon: () => h(TeamOutlined), color: '#059669', bg: '#ecfdf5', borderColor: '#a7f3d0' },
  { label: 'Phiên điểm danh', value: sessions.value.length, sub: 'Đã tạo trong suốt quá trình', icon: () => h(CheckCircleOutlined), color: '#7c3aed', bg: '#f5f3ff', borderColor: '#ddd6fe' },
  { label: 'Lịch dạy', value: schedules.value.length, sub: 'Buổi học / Ca thực tế', icon: () => h(CalendarOutlined), color: '#d97706', bg: '#fffbeb', borderColor: '#fde68a' }
])

const actionItems = computed(() => [
  { title: 'Điểm danh lớp học', sub: `${sessions.value.filter(x => String(x.status) === 'Open' || Number(x.status) === 1).length} buổi đang chờ điểm danh`, to: '/teacher/attendance', icon: () => h(CheckCircleOutlined), iconClass: 'bg-emerald-50 text-emerald-600 border-emerald-100' },
  { title: 'Nhập kết quả học tập', sub: `${results.value.length} bảng điểm đã ghi nhận`, to: '/teacher/results', icon: () => h(CheckSquareOutlined), iconClass: 'bg-blue-50 text-blue-600 border-blue-100' },
  { title: 'Tra cứu Lịch tuần', sub: `Xem chi tiết thời gian, phòng học`, to: '/teacher/schedule', icon: () => h(CalendarOutlined), iconClass: 'bg-amber-50 text-amber-600 border-amber-100' }
])

// Helpers
function getInitials(name) {
  if (!name) return 'L'
  const parts = String(name).trim().split(/\s+/)
  if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

function percent(record) {
  if (!record.maxStudents) return 0
  return Math.min(100, Math.round((Number(record.currentStudents || 0) / Number(record.maxStudents)) * 100))
}

function getPercentBgColor(pct) {
  if (pct >= 100) return 'bg-rose-500'
  if (pct >= 80) return 'bg-amber-400'
  return 'bg-emerald-500'
}

function getPercentColorText(pct) {
  if (pct >= 100) return 'text-rose-600'
  if (pct >= 80) return 'text-amber-600'
  return 'text-emerald-600'
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

function formatTime(t) {
  if (!t) return '—'
  return String(t).substring(0, 5)
}

function formatDate(d) {
  if (!d) return '—'
  const date = new Date(d)
  if (Number.isNaN(date.getTime())) return d
  return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

function shortDateVN(d) {
  if (!d) return '—'
  const date = new Date(d)
  if (Number.isNaN(date.getTime())) return d
  return `${String(date.getDate()).padStart(2,'0')}/${String(date.getMonth()+1).padStart(2,'0')}`
}

function sortSchedule(a, b) {
  return dayValue(a.dayOfWeek) - dayValue(b.dayOfWeek) || shiftValue(a.studyShift) - shiftValue(b.studyShift) || String(a.startTime).localeCompare(String(b.startTime))
}

// Data Fetching
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
/* Scrollbar */
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #cbd5e1; }

/* Table Enterprise Styles */
:deep(.enterprise-table .ant-table-thead > tr > th) {
  background: #f8fafc;
  color: #475569;
  font-weight: 700;
  text-transform: uppercase;
  font-size: 0.75rem;
  letter-spacing: 0.025em;
  padding: 12px 16px;
  border-bottom: 2px solid #e2e8f0;
}
:deep(.enterprise-table .ant-table-tbody > tr > td) {
  padding: 14px 16px;
  vertical-align: middle;
  border-bottom: 1px solid #f1f5f9;
}
:deep(.enterprise-table .ant-table-tbody > tr:hover > td) {
  background-color: #f8fafc !important;
}
</style>