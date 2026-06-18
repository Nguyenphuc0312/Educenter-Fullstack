<template>
  <div class="space-y-6">
    <PageHeader 
      title="Điểm danh Lớp học" 
      subtitle="Tạo phiên điểm danh, cập nhật chuyên cần và khóa sổ sau khi kết thúc buổi học."
    >
      <template #actions>
        <button 
          class="px-4 py-2 bg-white border border-slate-200 text-slate-700 hover:bg-slate-50 font-medium rounded-lg transition-colors shadow-sm flex items-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed active:scale-95" 
          @click="loadBaseData"
          :disabled="loading"
        >
          <LoadingSpinner v-if="loading" size="sm" class="text-slate-500" />
          <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>
          Làm mới
        </button>
      </template>
    </PageHeader>

    <section class="bg-white rounded-2xl p-5 border border-slate-200 shadow-sm">
      <div class="flex flex-col mb-3">
        <h2 class="text-sm font-bold text-slate-800 flex items-center gap-2">
          <svg class="w-4 h-4 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" /></svg>
          Khởi tạo phiên điểm danh
        </h2>
        <p class="text-xs text-slate-500">Chọn lớp, lịch học và ngày để mở phiên điểm danh mới.</p>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-5 gap-4 items-end">
        <div class="xl:col-span-1 space-y-1.5">
          <label class="text-xs font-semibold text-slate-600">Lớp học</label>
          <a-select 
            v-model:value="selectedClassId" 
            placeholder="Chọn lớp học" 
            class="w-full custom-select" 
            @change="onClassChange"
            show-search
            option-filter-prop="children"
          >
            <a-select-option v-for="cls in classes" :key="cls.id" :value="cls.id">
              {{ cls.className }}
            </a-select-option>
          </a-select>
        </div>

        <div class="xl:col-span-2 space-y-1.5">
          <label class="text-xs font-semibold text-slate-600">Lịch trình / Buổi học</label>
          <a-select 
            v-model:value="selectedScheduleId" 
            placeholder="Chọn nội dung buổi học" 
            class="w-full custom-select"
            :disabled="!selectedClassId || schedules.length === 0"
          >
            <a-select-option v-for="sch in schedules" :key="sch.id" :value="sch.id">
              Buổi {{ sch.sessionNumber }} • {{ sch.topic || sch.classNameSnapshot }}
            </a-select-option>
          </a-select>
        </div>

        <div class="xl:col-span-1 space-y-1.5">
          <label class="text-xs font-semibold text-slate-600">Ngày điểm danh</label>
          <a-date-picker 
            v-model:value="attendanceDate" 
            class="w-full h-[38px] rounded-lg border-slate-300" 
            format="DD/MM/YYYY" 
            :allow-clear="false"
          />
        </div>

        <div class="xl:col-span-1">
          <button 
            class="w-full h-[38px] bg-blue-600 text-white font-bold rounded-lg shadow-sm hover:bg-blue-700 transition-colors flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed active:scale-95"
            :disabled="!canCreateSession || creatingSession" 
            @click="createSession"
          >
            <LoadingSpinner v-if="creatingSession" size="sm" class="!text-white" />
            <span v-else>Tạo buổi mới</span>
          </button>
        </div>
      </div>
    </section>

    <div class="grid grid-cols-1 xl:grid-cols-4 gap-6">
      
      <section class="xl:col-span-1 bg-white rounded-2xl border border-slate-200 shadow-sm flex flex-col h-[650px]">
        <div class="p-4 border-b border-slate-100 bg-slate-50/50 rounded-t-2xl">
          <h2 class="text-sm font-bold text-slate-800">Danh sách phiên</h2>
          <p class="text-[11px] text-slate-500 mt-0.5">Các buổi đã điểm danh của lớp</p>
        </div>
        
        <div class="flex-1 overflow-y-auto custom-scrollbar p-3 space-y-2">
          <div v-if="sessions.length === 0" class="h-full flex flex-col items-center justify-center text-center px-4">
            <svg class="w-10 h-10 text-slate-300 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" /></svg>
            <p class="text-sm font-semibold text-slate-500">Chưa có phiên nào</p>
          </div>
          
          <button
            v-for="session in sessions"
            :key="session.id"
            type="button"
            class="w-full text-left p-3 rounded-xl border transition-all duration-200 group flex items-start justify-between gap-2"
            :class="selectedSessionId === session.id ? 'bg-blue-50 border-blue-400 shadow-sm ring-1 ring-blue-100' : 'bg-white border-slate-200 hover:border-blue-300 hover:bg-slate-50'"
            @click="selectSession(session)"
          >
            <div class="min-w-0">
              <p class="text-xs font-black text-slate-800 truncate mb-1">
                Buổi {{ session.sessionNumber }} <span class="font-medium text-slate-500 mx-1">•</span> {{ formatDate(session.attendanceDate) }}
              </p>
              <p class="text-[11px] text-slate-500 truncate" :class="selectedSessionId === session.id ? 'text-blue-700' : ''">
                {{ session.topic }}
              </p>
            </div>
            <div class="shrink-0 mt-0.5">
              <span v-if="sessionLocked(session)" class="flex items-center justify-center w-5 h-5 rounded-full bg-rose-100 text-rose-600" title="Đã khóa">
                <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" /></svg>
              </span>
              <span v-else class="flex items-center justify-center w-5 h-5 rounded-full bg-emerald-100 text-emerald-600" title="Đang mở">
                <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 11V7a4 4 0 118 0m-4 8v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2z" /></svg>
              </span>
            </div>
          </button>
        </div>
      </section>

      <section class="xl:col-span-3 bg-white rounded-2xl border border-slate-200 shadow-sm flex flex-col h-[650px] relative overflow-hidden">
        
        <div class="p-4 border-b border-slate-200 bg-slate-50/80 flex flex-col lg:flex-row justify-between lg:items-center gap-4 relative z-10">
          <div>
            <h2 class="text-base font-bold text-slate-800">
              <template v-if="selectedSession">
                <span class="text-blue-600">Buổi {{ selectedSession.sessionNumber }}:</span> {{ selectedSession.topic }}
              </template>
              <template v-else>Chi tiết điểm danh</template>
            </h2>
            <p v-if="selectedSession" class="text-[11px] font-medium text-slate-500 mt-0.5">Lớp: {{ selectedSession.classNameSnapshot }}</p>
            <p v-else class="text-[11px] text-slate-500 mt-0.5">Vui lòng chọn một phiên ở cột bên trái để thao tác.</p>
          </div>

          <div class="flex flex-wrap items-center gap-2">
            <button 
              class="px-4 py-2 bg-white border border-slate-300 text-slate-700 font-semibold rounded-lg text-xs hover:bg-slate-50 transition-colors flex items-center gap-1.5 disabled:opacity-50"
              :disabled="!selectedSession || sessionLocked(selectedSession)"
              @click="markAllPresent"
            >
              <svg class="w-3.5 h-3.5 text-emerald-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" /></svg>
              Tất cả có mặt
            </button>
            <button 
              class="px-5 py-2 bg-blue-600 text-white font-bold rounded-lg text-xs hover:bg-blue-700 shadow-sm transition-colors flex items-center gap-1.5 disabled:opacity-50 disabled:cursor-not-allowed"
              :disabled="!selectedSession || sessionLocked(selectedSession) || saving"
              @click="saveRecords"
            >
              <LoadingSpinner v-if="saving" size="xs" class="!text-white" />
              <svg v-else class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4" /></svg>
              Lưu điểm danh
            </button>
            <button 
              class="px-4 py-2 bg-rose-50 text-rose-600 border border-rose-200 font-bold rounded-lg text-xs hover:bg-rose-100 transition-colors flex items-center gap-1.5 disabled:opacity-50 disabled:cursor-not-allowed"
              :disabled="!selectedSession || sessionLocked(selectedSession) || locking"
              @click="confirmLockSession"
            >
              <LoadingSpinner v-if="locking" size="xs" class="!text-rose-600" />
              <svg v-else class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" /></svg>
              Khóa buổi
            </button>
          </div>
        </div>

        <div v-if="selectedSession && records.length" class="bg-white border-b border-slate-100 px-4 py-2.5 flex flex-wrap items-center gap-4 text-xs">
          <div class="font-semibold text-slate-600 border-r pr-4 border-slate-200">Sĩ số: <span class="text-slate-800">{{ records.length }}</span></div>
          <div class="flex items-center gap-1.5 font-medium"><span class="w-2 h-2 rounded-full bg-emerald-500"></span> Có mặt: <span class="font-bold text-emerald-700">{{ stats.present }}</span></div>
          <div class="flex items-center gap-1.5 font-medium"><span class="w-2 h-2 rounded-full bg-rose-500"></span> Vắng: <span class="font-bold text-rose-700">{{ stats.absent }}</span></div>
          <div class="flex items-center gap-1.5 font-medium"><span class="w-2 h-2 rounded-full bg-amber-500"></span> Muộn: <span class="font-bold text-amber-700">{{ stats.late }}</span></div>
          <div class="flex items-center gap-1.5 font-medium"><span class="w-2 h-2 rounded-full bg-blue-500"></span> Phép: <span class="font-bold text-blue-700">{{ stats.excused }}</span></div>
          <div v-if="stats.locked" class="flex items-center gap-1.5 font-medium"><span class="w-2 h-2 rounded-full bg-rose-700"></span> Khóa học phí: <span class="font-bold text-rose-700">{{ stats.locked }}</span></div>
        </div>

        <div class="flex-1 overflow-auto custom-scrollbar relative">
          <div v-if="loadingRecords" class="absolute inset-0 flex items-center justify-center bg-white/80 z-20 backdrop-blur-sm">
            <LoadingSpinner size="lg" />
          </div>

          <a-table
            :data-source="records"
            :columns="columns"
            row-key="studentId"
            size="middle"
            class="enterprise-table"
            :pagination="false"
            :scroll="{ y: 460 }"
          >
            <template #bodyCell="{ column, record, index }">
              <template v-if="column.key === 'index'">
                <span class="text-slate-400 font-semibold">{{ index + 1 }}</span>
              </template>

              <template v-else-if="column.key === 'student'">
                <div class="flex items-center gap-3 py-0.5">
                  <div class="w-8 h-8 rounded-full flex items-center justify-center text-[10px] font-black text-white shrink-0 shadow-sm" :style="{ background: avatarBg(record.studentNameSnapshot) }">
                    {{ getInitials(record.studentNameSnapshot) }}
                  </div>
                  <div class="min-w-0 flex flex-col">
                    <span class="font-bold text-slate-800 text-[13px] truncate" :title="record.studentNameSnapshot">{{ record.studentNameSnapshot }}</span>
                    <span class="text-[10px] text-slate-500 font-mono">{{ record.studentId.substring(0,8).toUpperCase() }}...</span>
                    <span v-if="isAttendanceLocked(record)" class="mt-1 inline-flex w-fit items-center rounded-full border border-rose-200 bg-rose-50 px-2 py-0.5 text-[10px] font-bold text-rose-700">Khóa học phí</span>
                  </div>
                </div>
              </template>

              <template v-else-if="column.key === 'status'">
                <a-select 
                  v-model:value="record.status" 
                  class="w-[140px] custom-status-select" 
                  :class="getStatusSelectClass(record.status)"
                  :disabled="!selectedSession || sessionLocked(selectedSession) || isAttendanceLocked(record)"
                  :bordered="false"
                >
                  <a-select-option value="Present"><span class="font-bold text-emerald-700">✓ Có mặt</span></a-select-option>
                  <a-select-option value="Absent"><span class="font-bold text-rose-700">✗ Vắng mặt</span></a-select-option>
                  <a-select-option value="Late"><span class="font-bold text-amber-700">⏰ Đi muộn</span></a-select-option>
                  <a-select-option value="Excused"><span class="font-bold text-blue-700">📋 Có phép</span></a-select-option>
                </a-select>
              </template>

              <template v-else-if="column.key === 'note'">
                <a-input 
                  v-model:value="record.note" 
                  placeholder="Nhập lý do..." 
                  class="text-xs bg-slate-50 border-slate-200 focus:bg-white rounded-lg py-1.5"
                  :disabled="!selectedSession || sessionLocked(selectedSession) || isAttendanceLocked(record)" 
                />
              </template>
            </template>
            <template #emptyText>
              <div class="py-16 text-slate-400">Không có dữ liệu học viên.</div>
            </template>
          </a-table>
        </div>
      </section>
    </div>

    <a-modal
      v-model:open="isConfirmLockOpen"
      title="Xác nhận khóa buổi điểm danh"
      centered
      :closable="false"
      :maskClosable="false"
    >
      <div class="py-2">
        <div class="flex items-center gap-3 text-rose-600 mb-3 bg-rose-50 p-3 rounded-lg border border-rose-100">
          <svg class="w-6 h-6 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" /></svg>
          <span class="text-sm font-bold">Cảnh báo: Hành động không thể hoàn tác!</span>
        </div>
        <p class="text-sm text-slate-600 leading-relaxed">
          Sau khi khóa, toàn bộ dữ liệu điểm danh của <strong>Buổi {{ selectedSession?.sessionNumber }}</strong> sẽ được đóng băng và gửi lên hệ thống lưu trữ. <strong>Bạn sẽ không thể chỉnh sửa lại điểm danh hay ghi chú.</strong> Bạn có chắc chắn muốn khóa phiên này?
        </p>
      </div>
      <template #footer>
        <div class="flex gap-2 justify-end mt-2">
          <button @click="isConfirmLockOpen = false" class="px-4 py-2 bg-white border border-slate-300 rounded-lg text-sm font-semibold text-slate-700 hover:bg-slate-50 transition-colors">Hủy bỏ</button>
          <button @click="executeLockSession" class="px-5 py-2 bg-rose-600 text-white rounded-lg text-sm font-bold hover:bg-rose-700 transition-colors flex items-center gap-2">
            <LoadingSpinner v-if="locking" size="sm" class="!text-white" />
            Xác nhận Khóa
          </button>
        </div>
      </template>
    </a-modal>
  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import dayjs from 'dayjs'
import { message } from 'ant-design-vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { classApi } from '@/api/classApi'
import { scheduleApi } from '@/api/scheduleApi'
import { enrollmentApi } from '@/api/enrollmentApi'
import { attendanceApi } from '@/api/attendanceApi'
import { tuitionApi } from '@/api/tuitionApi'
import { useAuthStore } from '@/stores/auth'
import { formatDate } from '@/lib/formatters'

const props = defineProps({ classId: { type: String, default: '' } })
const auth = useAuthStore()
const loading = ref(false)
const loadingRecords = ref(false)
const creatingSession = ref(false)
const saving = ref(false)
const locking = ref(false)
const isConfirmLockOpen = ref(false)

const classes = ref([])
const schedules = ref([])
const enrollments = ref([])
const sessions = ref([])
const records = ref([])
const learningHolds = ref([])

const selectedClassId = ref(props.classId || undefined)
const selectedScheduleId = ref(undefined)
const selectedSessionId = ref(undefined)
const attendanceDate = ref(dayjs())

const selectedClass = computed(() => classes.value.find(x => x.id === selectedClassId.value))
const selectedSchedule = computed(() => schedules.value.find(x => x.id === selectedScheduleId.value))
const selectedSession = computed(() => sessions.value.find(x => x.id === selectedSessionId.value))
const canCreateSession = computed(() => selectedClass.value && selectedSchedule.value && attendanceDate.value)

// Bảng chi tiết
const columns = [
  { title: '#', key: 'index', width: 50, align: 'center' },
  { title: 'Thông tin Học viên', key: 'student', minWidth: 220 },
  { title: 'Đánh giá Trạng thái', key: 'status', width: 180 },
  { title: 'Ghi chú / Lý do', key: 'note' }
]

// Real-time Stats
const stats = computed(() => {
  return {
    present: records.value.filter(r => r.status === 'Present').length,
    absent: records.value.filter(r => r.status === 'Absent').length,
    late: records.value.filter(r => r.status === 'Late').length,
    excused: records.value.filter(r => r.status === 'Excused').length,
    locked: records.value.filter(r => isAttendanceLocked(r)).length,
  }
})

// Avatar Helpers
function getInitials(name) {
  if (!name) return 'HV'
  const parts = String(name).trim().split(/\s+/)
  if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

const AVATAR_COLORS = [
  'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)', // Blue
  'linear-gradient(135deg, #10b981 0%, #047857 100%)', // Emerald
  'linear-gradient(135deg, #f59e0b 0%, #b45309 100%)', // Amber
  'linear-gradient(135deg, #8b5cf6 0%, #5b21b6 100%)', // Violet
  'linear-gradient(135deg, #ec4899 0%, #be185d 100%)', // Pink
]

function avatarBg(name) {
  if (!name) return 'linear-gradient(135deg, #94a3b8 0%, #475569 100%)'
  let hash = 0
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length]
}

function getStatusSelectClass(status) {
  const map = {
    'Present': 'bg-emerald-50 border-emerald-200 text-emerald-800',
    'Absent': 'bg-rose-50 border-rose-200 text-rose-800',
    'Late': 'bg-amber-50 border-amber-200 text-amber-800',
    'Excused': 'bg-blue-50 border-blue-200 text-blue-800'
  }
  return `border rounded-lg transition-colors ${map[status] || 'bg-slate-50 border-slate-200'}`
}

function sessionLocked(session) { return String(session?.status) === 'Locked' || Number(session?.status) === 2 }
function normalizedStatus(value) {
  const map = { 1: 'Present', 2: 'Absent', 3: 'Late', 4: 'Excused', Present: 'Present', Absent: 'Absent', Late: 'Late', Excused: 'Excused' }
  return map[value] || 'Present'
}
function classNameForPayload() { return selectedClass.value?.className || selectedSchedule.value?.classNameSnapshot || '' }
function activeEnrollments() {
  return enrollments.value.filter(x => ['Confirmed', 'Studying', 2, 3, '2', '3'].includes(x.status))
}

function isAttendanceLocked(record) {
  return learningHolds.value.some(x => String(x.studentId).toLowerCase() === String(record.studentId).toLowerCase())
}

async function loadLearningHolds(classId) {
  learningHolds.value = []
  if (!classId) return
  try {
    learningHolds.value = await tuitionApi.getLearningHolds({ classId })
  } catch (error) {
    message.warning(error.message || 'Không thể kiểm tra trạng thái khóa học phí')
  }
}

// Data Fetching
async function loadBaseData() {
  if (!auth.user?.referenceId) return
  loading.value = true
  try {
    classes.value = await classApi.getByTeacher(auth.user.referenceId)
    if (!selectedClassId.value && classes.value.length) selectedClassId.value = classes.value[0].id
    await onClassChange()
  } catch (err) {
    message.error('Lỗi khi tải dữ liệu lớp học.')
  } finally {
    loading.value = false
  }
}

async function onClassChange() {
  records.value = []
  learningHolds.value = []
  selectedSessionId.value = undefined
  if (!selectedClassId.value) return
  const [sch, en, ses] = await Promise.all([
    scheduleApi.getByClass(selectedClassId.value).catch(() => []),
    enrollmentApi.getByClass(selectedClassId.value).catch(() => []),
    attendanceApi.getSessionsByClass(selectedClassId.value).catch(() => [])
  ])
  schedules.value = sch || []
  enrollments.value = en || []
  await loadLearningHolds(selectedClassId.value)
  // Xếp phiên mới nhất lên đầu
  sessions.value = (ses || []).sort((a,b) => b.sessionNumber - a.sessionNumber)
  selectedScheduleId.value = schedules.value[0]?.id
}

async function createSession() {
  creatingSession.value = true
  try {
    const payload = {
      classId: selectedClassId.value,
      classNameSnapshot: classNameForPayload(),
      scheduleId: selectedScheduleId.value,
      sessionNumber: selectedSchedule.value?.sessionNumber || sessions.value.length + 1,
      attendanceDate: attendanceDate.value?.toISOString?.() || new Date().toISOString(),
      topic: selectedSchedule.value?.topic || `Nội dung buổi ${selectedSchedule.value?.sessionNumber || sessions.value.length + 1}`,
      createdByTeacherId: auth.user.referenceId,
      createdByTeacherName: auth.user.fullName || auth.user.username
    }
    const created = await attendanceApi.createSession(payload)
    sessions.value = [created, ...sessions.value].sort((a,b) => b.sessionNumber - a.sessionNumber)
    await selectSession(created)
    message.success('Đã tạo thành công phiên điểm danh mới.')
  } catch (error) {
    message.error(error.message || 'Hệ thống từ chối yêu cầu tạo phiên.')
  } finally {
    creatingSession.value = false
  }
}

async function selectSession(session) {
  selectedSessionId.value = session.id
  loadingRecords.value = true
  try {
    await loadLearningHolds(session.classId || selectedClassId.value)
    const data = await attendanceApi.getRecordsBySession(session.id)
    if (data?.length) {
      records.value = data.map(x => ({ ...x, status: normalizedStatus(x.status) }))
    } else {
      records.value = activeEnrollments().map(x => ({
        id: null,
        attendanceSessionId: session.id,
        studentId: x.studentId,
        studentNameSnapshot: x.studentNameSnapshot,
        status: 'Present',
        note: ''
      }))
    }
  } catch (err) {
    message.error('Không thể tải danh sách học viên.')
  } finally {
    loadingRecords.value = false
  }
}

function markAllPresent() {
  if (!records.value.length || sessionLocked(selectedSession.value)) return
  records.value.forEach(r => { if (!isAttendanceLocked(r)) r.status = 'Present' })
  message.success('Đã thiết lập nhanh tất cả "Có mặt".')
}

async function saveRecords() {
  if (!selectedSession.value) return
  const unlockedRecords = records.value.filter(x => !isAttendanceLocked(x))
  const lockedCount = records.value.length - unlockedRecords.length
  if (!unlockedRecords.length) {
    message.warning('Tất cả học viên trong buổi này đang bị khóa điểm danh do quá hạn học phí.')
    return
  }
  saving.value = true
  try {
    const saved = await attendanceApi.bulkUpdateRecords({
      attendanceSessionId: selectedSession.value.id,
      records: unlockedRecords.map(x => ({ studentId: x.studentId, status: x.status, note: x.note }))
    })
    records.value = saved.map(x => ({ ...x, status: normalizedStatus(x.status) }))
    message.success(lockedCount ? `Đã lưu điểm danh. Bỏ qua ${lockedCount} học viên bị khóa do quá hạn học phí.` : 'Đã lưu dữ liệu điểm danh thành công.')
  } catch (error) {
    message.error(error.message || 'Có lỗi xảy ra trong quá trình lưu dữ liệu.')
  } finally {
    saving.value = false
  }
}

function confirmLockSession() {
  if (!selectedSession.value) return
  isConfirmLockOpen.value = true
}

async function executeLockSession() {
  if (!selectedSession.value) return
  locking.value = true
  try {
    const locked = await attendanceApi.lockSession(selectedSession.value.id)
    sessions.value = sessions.value.map(x => x.id === locked.id ? locked : x)
    message.success('Đã khóa phiên điểm danh vĩnh viễn.')
    isConfirmLockOpen.value = false
  } catch (error) {
    message.error(error.message || 'Không thể thực thi lệnh khóa phiên.')
  } finally {
    locking.value = false
  }
}

watch(() => props.classId, (value) => { if (value) selectedClassId.value = value })
onMounted(loadBaseData)
</script>

<style scoped>
/* Scrollbar */
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #e2e8f0; border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #cbd5e1; }

/* Tùy chỉnh Select */
:deep(.custom-select .ant-select-selector) {
  border-radius: 8px !important;
  height: 38px !important;
  align-items: center;
  border-color: #cbd5e1 !important;
}

/* Tùy chỉnh Select Trạng thái trong bảng */
:deep(.custom-status-select .ant-select-selector) {
  background-color: transparent !important;
  border: none !important;
  box-shadow: none !important;
  font-weight: 700;
  height: 34px !important;
  display: flex;
  align-items: center;
}
:deep(.custom-status-select .ant-select-selection-item) {
  display: flex;
  align-items: center;
}
:deep(.custom-status-select.ant-select-disabled .ant-select-selector) {
  color: inherit !important;
  opacity: 0.8;
}

/* Tuỳ chỉnh Ant Table */
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
  padding: 10px 16px;
  vertical-align: middle;
  border-bottom: 1px solid #f1f5f9;
}
:deep(.enterprise-table .ant-table-tbody > tr:hover > td) {
  background-color: #f8fafc !important;
}
</style>
