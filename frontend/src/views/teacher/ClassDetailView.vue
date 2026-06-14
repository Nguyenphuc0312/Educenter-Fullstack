<template>
  <div class="space-y-6">
    <PageHeader>
      <template #title>
        <div class="flex items-center gap-3">
          <h1 class="text-2xl font-black text-slate-800">{{ classInfo?.className || 'Đang tải thông tin lớp...' }}</h1>
          <span v-if="classInfo" class="px-2.5 py-0.5 text-[10px] font-black uppercase tracking-wider bg-blue-100 text-blue-700 rounded-lg border border-blue-200 shadow-sm">
            {{ classInfo?.classCode || '—' }}
          </span>
        </div>
      </template>
      <template #subtitle>
        <p class="text-slate-500 font-medium mt-1">{{ classInfo?.courseNameSnapshot || 'Quản lý toàn diện dữ liệu lớp học, lịch trình và kết quả.' }}</p>
      </template>
      <template #actions>
        <router-link to="/teacher/classes">
          <button class="px-4 py-2.5 bg-white border border-slate-200 text-slate-700 font-bold rounded-xl shadow-sm hover:bg-slate-50 transition-colors active:scale-95 text-sm">
            Quay lại
          </button>
        </router-link>

        <button v-if="classInfo" @click="goToResults" class="px-5 py-2.5 bg-purple-50 text-purple-700 border border-purple-200 font-bold rounded-xl shadow-sm hover:bg-purple-100 transition-colors flex items-center gap-2 active:scale-95 text-sm">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" /></svg>
          Nhập kết quả
        </button>

        <button v-if="classInfo" @click="goToAttendance" class="px-5 py-2.5 bg-blue-600 text-white font-bold rounded-xl shadow-sm hover:bg-blue-700 transition-colors flex items-center gap-2 active:scale-95 text-sm">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"/></svg>
          Mở điểm danh
        </button>
      </template>
    </PageHeader>

    <section class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="bg-white p-5 rounded-2xl border border-slate-100 shadow-sm flex flex-col justify-center transition-all hover:shadow-md hover:border-blue-200">
        <div class="flex items-center gap-4 mb-3">
          <div class="p-3 bg-blue-50 text-blue-600 rounded-xl"><svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"/></svg></div>
          <div>
            <span class="text-[11px] font-bold text-slate-400 uppercase tracking-wider block">Sĩ số lớp</span>
            <strong class="text-lg font-black text-slate-800">{{ classInfo?.currentStudents || 0 }} / {{ classInfo?.maxStudents || 0 }}</strong>
          </div>
        </div>
        <div class="w-full h-1.5 bg-slate-100 rounded-full overflow-hidden shadow-inner">
          <div class="h-full bg-blue-500 rounded-full transition-all" :style="{ width: `${Math.min(((classInfo?.currentStudents || 0) / (classInfo?.maxStudents || 1)) * 100, 100)}%` }"></div>
        </div>
      </div>

      <div class="bg-white p-5 rounded-2xl border border-slate-100 shadow-sm flex items-center gap-4 transition-all hover:shadow-md hover:border-emerald-200">
        <div class="p-3 bg-emerald-50 text-emerald-600 rounded-xl"><svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m3-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"/></svg></div>
        <div>
          <span class="text-[11px] font-bold text-slate-400 uppercase tracking-wider block">Địa điểm</span>
          <strong class="text-lg font-black text-slate-800">{{ classInfo?.room ? `Phòng ${classInfo.room}` : 'Chưa xếp phòng' }}</strong>
        </div>
      </div>

      <div class="bg-white p-5 rounded-2xl border border-slate-100 shadow-sm flex items-center gap-4 transition-all hover:shadow-md hover:border-indigo-200">
        <div class="p-3 bg-indigo-50 text-indigo-600 rounded-xl"><svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg></div>
        <div>
          <span class="text-[11px] font-bold text-slate-400 uppercase tracking-wider block">Khai giảng</span>
          <strong class="text-lg font-black text-slate-800">{{ formatDate(classInfo?.startDate) }}</strong>
        </div>
      </div>

      <div class="bg-white p-5 rounded-2xl border border-slate-100 shadow-sm flex items-center gap-4 transition-all hover:shadow-md hover:border-amber-200">
        <div class="p-3 bg-amber-50 text-amber-600 rounded-xl"><svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"/></svg></div>
        <div>
          <span class="text-[11px] font-bold text-slate-400 uppercase tracking-wider block">Dự kiến bế giảng</span>
          <strong class="text-lg font-black text-slate-800">{{ formatDate(classInfo?.endDate) }}</strong>
        </div>
      </div>
    </section>

    <section class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden min-h-[500px]">
      <div class="border-b border-slate-100 px-6 pt-4 bg-slate-50/50">
        <a-tabs v-model:activeKey="activeTab" class="custom-tabs">
          <a-tab-pane key="students" :tab="`Học viên (${enrollments.length})`" />
          <a-tab-pane key="schedule" :tab="`Lịch học (${schedules.length})`" />
          <a-tab-pane key="attendance" :tab="`Buổi điểm danh (${sessions.length})`" />
          <a-tab-pane key="results" :tab="`Kết quả (${results.length})`" />
        </a-tabs>
      </div>

      <div class="p-0">
        <div v-if="loading" class="py-24 flex justify-center">
          <LoadingSpinner size="lg" />
        </div>
        
        <a-table v-else-if="activeTab === 'students'" :data-source="enrollments" :columns="studentColumns" row-key="id" class="enterprise-table" :pagination="{ pageSize: 10 }" :scroll="{ x: 700 }">
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'student'">
              <div class="flex items-center gap-3">
                <div class="w-9 h-9 rounded-xl flex items-center justify-center text-[11px] font-black text-white shrink-0 shadow-sm border border-white/20" :style="{ background: avatarBg(record.studentNameSnapshot) }">
                  {{ getInitials(record.studentNameSnapshot) }}
                </div>
                <div>
                  <p class="font-bold text-slate-800 text-[13px]">{{ record.studentNameSnapshot || '—' }}</p>
                  <p class="text-[10px] text-slate-500 font-mono mt-0.5 bg-slate-100 px-1.5 py-0.5 rounded w-max border border-slate-200">{{ record.studentId }}</p>
                </div>
              </div>
            </template>
            <template v-else-if="column.key === 'status'">
              <span :class="['px-2.5 py-1 text-[11px] font-bold rounded-md border', enrollmentBadgeClass(record.status)]">
                {{ enrollmentLabel(record.status) }}
              </span>
            </template>
          </template>
          <template #emptyText><div class="py-12 text-slate-400">Chưa có học viên ghi danh vào lớp này.</div></template>
        </a-table>

        <a-table v-else-if="activeTab === 'schedule'" :data-source="schedules" :columns="scheduleColumns" row-key="id" class="enterprise-table" :pagination="{ pageSize: 10 }" :scroll="{ x: 700 }">
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'sessionNumber'">
              <span class="font-black text-slate-800">Buổi {{ record.sessionNumber }}</span>
            </template>
            
            <template v-if="column.key === 'time'">
              <div class="flex items-center gap-2 whitespace-nowrap">
                <div class="w-[90px] text-center px-2 py-1 bg-slate-100 border border-slate-200 text-slate-700 font-bold rounded-md text-[11px]">
                  {{ dayLabel(record.dayOfWeek) }}
                </div>
                <div class="w-[80px] text-center px-2 py-1 bg-blue-50 border border-blue-100 text-blue-700 font-bold rounded-md text-[11px]">
                  {{ shiftLabel(record.studyShift) }}
                </div>
                <div class="flex items-center gap-1.5 px-2 py-1 text-slate-600 font-mono text-[11px]">
                  <svg class="w-3.5 h-3.5 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
                  {{ formatTime(record.startTime) }} - {{ formatTime(record.endTime) }}
                </div>
              </div>
            </template>

            <template v-if="column.key === 'room'">
              <span class="font-bold text-slate-600">P.{{ record.room || '—' }}</span>
            </template>
            <template v-if="column.key === 'topic'">
              <span class="text-slate-600 text-[13px]">{{ record.topic || '—' }}</span>
            </template>
          </template>
          <template #emptyText><div class="py-12 text-slate-400">Chưa có lịch trình được thiết lập.</div></template>
        </a-table>

        <a-table v-else-if="activeTab === 'attendance'" :data-source="sessions" :columns="sessionColumns" row-key="id" class="enterprise-table" :pagination="{ pageSize: 10 }" :scroll="{ x: 700 }">
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'sessionNumber'">
              <span class="font-black text-slate-800">Buổi {{ record.sessionNumber }}</span>
            </template>
            <template v-if="column.key === 'topic'">
              <span class="text-slate-600 text-[13px]">{{ record.topic || '—' }}</span>
            </template>
            <template v-if="column.key === 'status'">
              <span :class="['px-2.5 py-1 text-[11px] font-bold rounded-md border flex items-center gap-1.5 w-max', sessionStatusClass(record.status)]">
                <span class="w-1.5 h-1.5 rounded-full" :class="String(record.status) === 'Locked' || Number(record.status) === 2 ? 'bg-rose-500' : 'bg-emerald-500'"></span>
                {{ sessionLabel(record.status) }}
              </span>
            </template>
          </template>
          <template #emptyText><div class="py-12 text-slate-400">Chưa có phiên điểm danh nào được tạo.</div></template>
        </a-table>

        <a-table v-else-if="activeTab === 'results'" :data-source="results" :columns="resultColumns" row-key="id" class="enterprise-table" :pagination="{ pageSize: 10 }" :scroll="{ x: 800 }">
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'studentNameSnapshot'">
               <span class="font-bold text-slate-800 text-[13px]">{{ record.studentNameSnapshot || '—' }}</span>
            </template>
            <template v-if="column.key === 'midtermScore' || column.key === 'finalScore'">
               <span :class="['font-black text-[13px]', getScoreColor(record[column.key])]">
                 {{ record[column.key] != null ? Number(record[column.key]).toFixed(1) : '—' }}
               </span>
            </template>
            <template v-else-if="column.key === 'averageScore'">
              <div class="inline-flex items-center justify-center px-2 py-1 rounded bg-slate-50 border border-slate-100 min-w-[40px]">
                <strong :class="['text-sm', getScoreColor(record.averageScore)]">{{ record.averageScore != null ? Number(record.averageScore).toFixed(1) : '—' }}</strong>
              </div>
            </template>
            <template v-else-if="column.key === 'resultStatus'">
              <span :class="['px-2.5 py-1 text-[11px] font-bold rounded-md border uppercase tracking-wider', resultBadgeClass(record.resultStatus)]">
                {{ resultLabel(record.resultStatus) }}
              </span>
            </template>
          </template>
          <template #emptyText><div class="py-12 text-slate-400">Chưa có dữ liệu kết quả học tập.</div></template>
        </a-table>
      </div>
    </section>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { classApi } from '@/api/classApi'
import { enrollmentApi } from '@/api/enrollmentApi'
import { scheduleApi } from '@/api/scheduleApi'
import { attendanceApi } from '@/api/attendanceApi'
import { resultApi } from '@/api/resultApi'

const router = useRouter()
const route = useRoute()

const props = defineProps({ classId: { type: String, required: true } })
const loading = ref(false)
const activeTab = ref('students')
const classInfo = ref(null)
const enrollments = ref([])
const schedules = ref([])
const sessions = ref([])
const results = ref([])

// Column definition
const studentColumns = [
  { title: 'Thông tin Học viên', key: 'student', width: 280 }, 
  { title: 'Khóa học', dataIndex: 'courseNameSnapshot', key: 'courseNameSnapshot' }, 
  { title: 'Ngày ghi danh', dataIndex: 'enrolledAt', key: 'enrolledAt', customRender: ({ text }) => formatDate(text) }, 
  { title: 'Trạng thái', key: 'status' }
]

// Mở rộng cột thời gian học để chứa các Badge fix width
const scheduleColumns = [
  { title: 'Buổi học', dataIndex: 'sessionNumber', key: 'sessionNumber', width: 100, align: 'center' }, 
  { title: 'Thời gian học (Thứ - Ca - Giờ)', key: 'time', width: 340 }, 
  { title: 'Phòng', dataIndex: 'room', key: 'room', width: 100 }, 
  { title: 'Nội dung (Chủ đề)', dataIndex: 'topic', key: 'topic' }
]

const sessionColumns = [
  { title: 'Phiên điểm danh', dataIndex: 'sessionNumber', key: 'sessionNumber', width: 150, align: 'center' }, 
  { title: 'Ngày diễn ra', dataIndex: 'attendanceDate', key: 'attendanceDate', width: 150, customRender: ({ text }) => formatDate(text) }, 
  { title: 'Nội dung (Chủ đề)', dataIndex: 'topic', key: 'topic' }, 
  { title: 'Trạng thái khóa sổ', key: 'status', width: 180 }
]

const resultColumns = [
  { title: 'Học viên', dataIndex: 'studentNameSnapshot', key: 'studentNameSnapshot', width: 220 }, 
  { title: 'Giữa kỳ', dataIndex: 'midtermScore', key: 'midtermScore', width: 100 }, 
  { title: 'Cuối kỳ', dataIndex: 'finalScore', key: 'finalScore', width: 100 }, 
  { title: 'Tổng kết', key: 'averageScore', width: 100 }, 
  { title: 'Xếp loại', key: 'resultStatus', width: 150 }
]

// ================= HÀM ĐIỀU HƯỚNG BÊN NGOÀI (DEEP LINKING FIX) =================
function goToAttendance() {
  if (!classInfo.value?.id) return;
  router.push({
    path: '/teacher/attendance',
    query: { classId: classInfo.value.id }
  })
}

function goToResults() {
  if (!classInfo.value?.id) return;
  router.push({
    path: '/teacher/results',
    query: { classId: classInfo.value.id }
  })
}

// Formatting Helpers
function formatDate(d) {
  if (!d) return '—'
  const date = new Date(d)
  if (Number.isNaN(date.getTime())) return d
  return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}
function formatTime(t) {
  if (!t) return '—'
  return String(t).substring(0, 5) // "13:30:00" -> "13:30"
}
function getInitials(name) {
  if (!name) return 'HV'
  const parts = String(name).trim().split(/\s+/)
  if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

// Avatar Colors
const AVATAR_COLORS = [
  'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)', 
  'linear-gradient(135deg, #10b981 0%, #047857 100%)', 
  'linear-gradient(135deg, #f59e0b 0%, #b45309 100%)', 
  'linear-gradient(135deg, #8b5cf6 0%, #5b21b6 100%)', 
  'linear-gradient(135deg, #ec4899 0%, #be185d 100%)', 
  'linear-gradient(135deg, #0ea5e9 0%, #0369a1 100%)', 
]
function avatarBg(name) {
  if (!name) return 'linear-gradient(135deg, #94a3b8 0%, #475569 100%)'
  let hash = 0
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length]
}

// Label Helpers
function dayLabel(value) { return ({ 0: 'Chủ nhật', 1: 'Thứ Hai', 2: 'Thứ Ba', 3: 'Thứ Tư', 4: 'Thứ Năm', 5: 'Thứ Sáu', 6: 'Thứ Bảy' }[value] || value) }
function shiftLabel(value) { return ({ 0: 'Ca Sáng', 1: 'Ca Chiều', 2: 'Ca Tối' }[value] || value) }

function enrollmentLabel(v) { return ({ 1: 'Chờ xác nhận', 2: 'Đã xác nhận', 3: 'Đang học', 4: 'Hoàn thành', 5: 'Đã hủy' }[v] || v) }
function enrollmentBadgeClass(v) { return ({ 1: 'bg-amber-50 text-amber-700 border-amber-200', 2: 'bg-blue-50 text-blue-700 border-blue-200', 3: 'bg-purple-50 text-purple-700 border-purple-200', 4: 'bg-emerald-50 text-emerald-700 border-emerald-200', 5: 'bg-slate-50 text-slate-600 border-slate-200' }[v] || 'bg-slate-50 text-slate-600') }

function sessionLabel(v) { return (String(v) === 'Locked' || Number(v) === 2) ? 'Đã khóa' : 'Đang mở' }
function sessionStatusClass(v) { return (String(v) === 'Locked' || Number(v) === 2) ? 'bg-rose-50 text-rose-700 border-rose-200' : 'bg-emerald-50 text-emerald-700 border-emerald-200' }

function resultLabel(v) { return ({ 1: 'Đang học', 2: 'Đạt', 3: 'Không đạt' }[v] || v) }
function resultBadgeClass(v) { return ({ 1: 'bg-blue-50 text-blue-700 border-blue-200', 2: 'bg-emerald-50 text-emerald-700 border-emerald-200', 3: 'bg-rose-50 text-rose-700 border-rose-200' }[v] || 'bg-slate-50 text-slate-600') }

function getScoreColor(score) {
  if (score == null) return 'text-slate-400'
  if (score >= 8) return 'text-emerald-600'
  if (score >= 5) return 'text-blue-600'
  return 'text-rose-600'
}

async function loadData() {
  loading.value = true
  try {
    const [cls, en, sch, ses, res] = await Promise.all([
      classApi.getById(props.classId),
      enrollmentApi.getByClass(props.classId),
      scheduleApi.getByClass(props.classId),
      attendanceApi.getSessionsByClass(props.classId),
      resultApi.getByClass(props.classId)
    ])
    classInfo.value = cls
    enrollments.value = en || []
    
    // Sort schedules by session number
    schedules.value = (sch || []).sort((a,b) => a.sessionNumber - b.sessionNumber)
    
    // Sort sessions by number
    sessions.value = (ses || []).sort((a,b) => b.sessionNumber - a.sessionNumber)
    results.value = res || []
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  // Đọc params tab từ URL để mở sẵn Tab phù hợp
  if (route.query.tab) {
    activeTab.value = route.query.tab
  }
  loadData()
})
</script>

<style scoped>
:deep(.enterprise-table .ant-table-thead > tr > th) {
  background: #f8fafc;
  color: #475569;
  font-weight: 700;
  text-transform: uppercase;
  font-size: 0.75rem;
  padding: 12px 16px;
  border-bottom: 2px solid #e2e8f0;
}
:deep(.enterprise-table .ant-table-tbody > tr > td) {
  padding: 14px 16px;
  border-bottom: 1px solid #f1f5f9;
}
:deep(.enterprise-table .ant-table-tbody > tr:hover > td) {
  background-color: #f8fafc !important;
}
.custom-tabs :deep(.ant-tabs-nav) {
  margin-bottom: 0 !important;
}
.custom-tabs :deep(.ant-tabs-tab) {
  padding: 12px 0 16px 0;
  font-weight: 700;
  font-size: 14px;
  color: #64748b;
}
.custom-tabs :deep(.ant-tabs-tab-active) {
  color: #2563eb !important;
}
</style>