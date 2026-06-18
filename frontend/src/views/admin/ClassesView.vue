<template>
  <AdminResourceView
    title="Quản lý lớp học"
    subtitle="Mở lớp, gán giáo viên, phòng học và theo dõi sĩ số."
    :api="classApi"
    :columns="columns"
    :fields="fields"
    :searchable-fields="['classCode', 'className', 'courseNameSnapshot', 'teacherNameSnapshot', 'room']"
    :status-options="statusOptions"
    :form-groups="formGroups"
    :filter-fn="customFilter"
    @reset="resetCustomFilters"
  >
    <!-- Custom filters slot -->
    <template #filters>
      <a-select
        v-model:value="filterCourseId"
        placeholder="Khóa học"
        allow-clear
        size="small"
        class="w-40"
      >
        <a-select-option v-for="course in courses" :key="course.id" :value="course.id">
          {{ course.name }}
        </a-select-option>
      </a-select>

      <a-select
        v-model:value="filterTeacherId"
        placeholder="Giảng viên"
        allow-clear
        size="small"
        class="w-40"
      >
        <a-select-option v-for="teacher in teachers" :key="teacher.id" :value="teacher.id">
          {{ teacher.fullName }}
        </a-select-option>
      </a-select>

      <a-select
        v-model:value="filterLearningMode"
        placeholder="Hình thức"
        allow-clear
        size="small"
        class="w-28"
      >
        <a-select-option :value="0">Trực tiếp</a-select-option>
        <a-select-option :value="1">Trực tuyến</a-select-option>
        <a-select-option :value="2">Kết hợp</a-select-option>
      </a-select>

      <a-select
        v-model:value="filterStatus"
        placeholder="Trạng thái"
        allow-clear
        size="small"
        class="w-32"
      >
        <a-select-option :value="0">Đang mở</a-select-option>
        <a-select-option :value="1">Đã đầy</a-select-option>
        <a-select-option :value="2">Đang học</a-select-option>
        <a-select-option :value="3">Hoàn thành</a-select-option>
        <a-select-option :value="4">Đã hủy</a-select-option>
      </a-select>
    </template>

    <!-- Custom row actions -->
    <template #rowActions="{ record }">
      <a-menu-item
        class="rounded-md px-3 py-1.5 text-xs"
        @click="goToSchedule(record)"
      >
        <CalendarOutlined class="mr-1" /> Xem lịch học
      </a-menu-item>
      <a-menu-item
        class="rounded-md px-3 py-1.5 text-xs"
        @click="goToStudents(record)"
      >
        <TeamOutlined class="mr-1" /> Xem học viên
      </a-menu-item>
    </template>

    <!-- Custom body cells -->
    <template #bodyCell="{ column, record }">
      <!-- Cột lớp học: gộp mã + tên + level chip -->
      <template v-if="column.key === 'classInfo'">
        <div class="flex flex-col min-w-0">
          <div class="flex items-center gap-2">
            <span class="text-[13px] font-semibold admin-class-cell-ellipsis" style="color: var(--admin-text);">
              {{ record.className || '—' }}
            </span>
            <span
              v-if="classNeedsAttention(record)"
              class="inline-flex items-center gap-1 px-1.5 py-0.5 rounded text-[10px] font-semibold"
              :style="{ background: 'var(--admin-warn-soft)', color: 'var(--admin-warn)', border: '1px solid var(--admin-warn-border)' }"
              title="Ngày kết thúc đã qua nhưng trạng thái vẫn Đang mở hoặc Đang học"
            >
              Cần cập nhật
            </span>
          </div>
          <div class="flex items-center gap-1.5 mt-0.5">
            <span class="text-[11px] font-mono admin-class-cell-ellipsis" style="color: var(--admin-text-muted);">
              {{ record.classCode || '—' }}
            </span>
            <span v-if="record.courseNameSnapshot" class="text-[11px] admin-class-cell-ellipsis" style="color: var(--admin-text-muted);">
              <span class="mx-1 opacity-50">•</span>{{ record.courseNameSnapshot }}
            </span>
            <span
              v-if="record.level"
              class="inline-flex items-center px-1.5 py-0 rounded text-[9px] font-semibold uppercase tracking-wider"
              :style="{ background: 'var(--admin-accent-soft)', color: 'var(--admin-accent)' }"
            >
              {{ record.level }}
            </span>
          </div>
        </div>
      </template>

      <!-- Cột giáo viên: name + phòng -->
      <template v-else-if="column.key === 'teacherInfo'">
        <div class="flex items-center gap-2.5 min-w-0">
          <div
            class="w-8 h-8 rounded-full flex items-center justify-center text-[11px] font-bold shrink-0 ring-2 ring-white dark:ring-slate-900"
            :style="{ background: teacherAvatarBg(record.teacherNameSnapshot), color: '#fff' }"
          >
            {{ getInitials(record.teacherNameSnapshot || 'NA') }}
          </div>
          <div class="flex flex-col min-w-0">
            <span class="text-[13px] font-semibold admin-class-cell-ellipsis" style="color: var(--admin-text);">
              {{ record.teacherNameSnapshot || 'Chưa phân công' }}
            </span>
            <span
              v-if="record.room"
              class="text-[11px] admin-class-cell-ellipsis"
              style="color: var(--admin-text-muted);"
            >
              Phòng {{ record.room }}
            </span>
            <span
              v-else
              class="text-[11px] admin-class-cell-ellipsis italic"
              style="color: var(--admin-text-subtle);"
            >
              Chưa cập nhật phòng
            </span>
          </div>
        </div>
      </template>

      <!-- Cột sĩ số: progress bar -->
      <template v-else-if="column.key === 'enrollment'">
        <div class="space-y-1.5 min-w-[150px] max-w-[200px]">
          <div class="flex items-baseline justify-between gap-2">
            <span class="text-[13px] font-semibold font-variant-numeric" style="color: var(--admin-text);">
              {{ record.currentStudents ?? 0 }}<span style="color: var(--admin-text-muted);">/{{ record.maxStudents ?? 0 }}</span>
            </span>
            <span
              v-if="enrollmentPercent(record) >= 100"
              class="inline-flex items-center px-1.5 py-0 rounded text-[9px] font-bold uppercase tracking-wider"
              :style="{ background: 'var(--admin-danger-soft)', color: 'var(--admin-danger)', border: '1px solid var(--admin-danger-border)' }"
            >
              Đã đầy
            </span>
            <span
              v-else
              class="text-[11px] font-semibold font-variant-numeric"
              :style="{ color: enrollmentColor(record) }"
            >
              {{ enrollmentPercent(record) }}%
            </span>
          </div>
          <div class="w-full h-1.5 rounded-full overflow-hidden" style="background: var(--admin-surface-2);">
            <div
              class="h-full rounded-full transition-all"
              :style="{
                width: `${enrollmentPercent(record)}%`,
                background: enrollmentColor(record)
              }"
            ></div>
          </div>
        </div>
      </template>

      <!-- Cột hình thức học: badge nhỏ -->
      <template v-else-if="column.key === 'learningMode'">
        <span
          v-if="record.learningMode !== null && record.learningMode !== undefined"
          class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[11px] font-medium"
          :style="learningModeStyle(record.learningMode)"
        >
          <span class="w-1.5 h-1.5 rounded-full" :style="{ background: learningModeDotColor(record.learningMode) }"></span>
          {{ learningModeLabel(record.learningMode) }}
        </span>
        <span v-else class="text-[12px]" style="color: var(--admin-text-subtle);">Chưa cập nhật</span>
      </template>

      <!-- Cột ngày: gọn + warning nếu mâu thuẫn -->
      <template v-else-if="column.key === 'schedule'">
        <div class="flex flex-col text-[12px]">
          <span style="color: var(--admin-text);">{{ formatDateRange(record.startDate, record.endDate) }}</span>
          <span v-if="record.startDate" class="text-[10px] mt-0.5" style="color: var(--admin-text-muted);">
            {{ scheduleHint(record.startDate, record.endDate) }}
          </span>
          <div
            v-if="classNeedsAttention(record)"
            class="mt-1.5 px-2 py-1.5 rounded-md flex items-start gap-1.5"
            :style="{ background: 'var(--admin-warn-soft, #fffbeb)', border: '1px solid var(--admin-warn-border, #fde68a)' }"
          >
            <WarningOutlined style="font-size: 12px; color: var(--admin-warn, #d97706); margin-top: 1px; flex-shrink: 0;" />
            <div class="flex flex-col">
              <span class="text-[11px] font-semibold" style="color: var(--admin-warn, #d97706);">
                Cần cập nhật trạng thái
              </span>
              <span class="text-[10px]" style="color: var(--admin-warn, #d97706); opacity: 0.85;">
                Ngày kết thúc đã qua
              </span>
            </div>
          </div>
        </div>
      </template>
    </template>
  </AdminResourceView>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { CalendarOutlined, TeamOutlined, WarningOutlined } from '@ant-design/icons-vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import { classApi } from '@/api/classApi'
import { courseApi } from '@/api/courseApi'
import { teacherApi } from '@/api/teacherApi'
import { classroomApi } from '@/api/classroomApi'
import { CLASS_STATUS, LEARNING_MODE, toOptions } from '@/lib/constants'
import { getInitials } from '@/lib/formatters'

const router = useRouter()

// Navigate tới trang Schedule đã lọc theo class
function goToSchedule(record) {
  if (!record?.id) return
  router.push({ path: '/admin/schedules', query: { classId: record.id } })
}

// Navigate tới danh sách học viên của lớp
function goToStudents(record) {
  if (!record?.id) return
  // Hiện chưa có route /admin/students với filter — dùng tab list
  router.push({ path: '/admin/enrollments', query: { classId: record.id } })
}

const statusOptions = toOptions(CLASS_STATUS, { 0: 'green', 1: 'orange', 2: 'blue', 3: 'purple', 4: 'red' })
const learningModeOptions = toOptions(LEARNING_MODE)

const courses = ref([])
const teachers = ref([])
const classrooms = ref([])

const filterCourseId = ref(undefined)
const filterTeacherId = ref(undefined)
const filterLearningMode = ref(undefined)
const filterStatus = ref(undefined)

const CLASS_STATUS_VALUE = {
  Open: 0,
  Full: 1,
  InProgress: 2,
  Completed: 3,
  Cancelled: 4,
}

function classStatusValue(status) {
  const numeric = Number(status)
  if (Number.isInteger(numeric)) return numeric
  return CLASS_STATUS_VALUE[status]
}

// Cột mới: gộp classCode + className + courseNameSnapshot vào 1 cell
// AdminResourceView sẽ tự map status (key='status', type='status' → StatusBadge tiếng Việt từ CLASS_STATUS)
// Tên cột key mới: classInfo, teacherInfo, enrollment, schedule
// Bỏ cột learningMode khỏi main table (hiển thị dưới dạng chip nhỏ trong schedule hint)
const columns = [
  { title: 'Lớp học', dataIndex: 'className', key: 'classInfo', minWidth: 240 },
  { title: 'Giảng viên', dataIndex: 'teacherNameSnapshot', key: 'teacherInfo', width: 220 },
  { title: 'Sĩ số', dataIndex: 'currentStudents', key: 'enrollment', width: 170 },
  { title: 'Thời gian', dataIndex: 'startDate', key: 'schedule', width: 190 },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 140 }
]

const fields = [
  { name: 'courseId', label: 'Khóa học', type: 'select', options: [], required: true, default: '' },
  { name: 'classCode', label: 'Mã lớp', required: true, editOnly: true, default: '' },
  { name: 'className', label: 'Tên lớp', required: true, default: '' },
  { name: 'teacherId', label: 'Giảng viên', type: 'select', options: [], required: true, default: '' },
  { name: 'classroomId', label: 'Phòng học', type: 'select', options: [], required: false, default: null },
  { name: 'maxStudents', label: 'Sĩ số tối đa', type: 'number', required: true, default: 30 },
  { name: 'currentStudents', label: 'Sĩ số hiện tại', type: 'number', default: 0 },
  { name: 'startDate', label: 'Ngày bắt đầu', type: 'date', default: '' },
  { name: 'endDate', label: 'Ngày kết thúc', type: 'date', default: '' },
  { name: 'learningMode', label: 'Hình thức học', type: 'select', options: learningModeOptions, default: 0 },
  { name: 'status', label: 'Trạng thái', type: 'select', options: statusOptions, default: 0 }
]

const formGroups = [
  {
    title: 'Thông tin khóa học & Lớp',
    fields: ['courseId', 'classCode', 'className']
  },
  {
    title: 'Giáo viên & Địa điểm học',
    fields: ['teacherId', 'classroomId', 'learningMode']
  },
  {
    title: 'Quy mô học viên',
    fields: ['maxStudents', 'currentStudents']
  },
  {
    title: 'Thời hạn & Trạng thái',
    fields: ['startDate', 'endDate', 'status']
  }
]

function customFilter(item) {
  const matchCourse = !filterCourseId.value || item.courseId === filterCourseId.value
  const matchTeacher = !filterTeacherId.value || item.teacherId === filterTeacherId.value
  const matchMode = filterLearningMode.value === undefined || Number(item.learningMode) === Number(filterLearningMode.value)
  const matchStatus = filterStatus.value === undefined || classStatusValue(item.status) === Number(filterStatus.value)
  return matchCourse && matchTeacher && matchMode && matchStatus
}

function resetCustomFilters() {
  filterCourseId.value = undefined
  filterTeacherId.value = undefined
  filterLearningMode.value = undefined
  filterStatus.value = undefined
}

// ============ Helpers cho cell rendering ============
const avatarColors = ['#3b82f6', '#8b5cf6', '#ec4899', '#f59e0b', '#10b981', '#06b6d4', '#ef4444', '#6366f1']

// Cảnh báo khi ngày kết thúc đã qua mà trạng thái vẫn là "Đang mở" (0) hoặc "Đang học" (2)
function classNeedsAttention(record) {
  if (!record?.endDate) return false
  const end = new Date(record.endDate)
  if (Number.isNaN(end.getTime())) return false
  if (end >= new Date()) return false
  const status = classStatusValue(record.status)
  return status === 0 || status === 2
}

function teacherAvatarBg(name) {
  if (!name) return '#94a3b8'
  let hash = 0
  for (let i = 0; i < name.length; i++) {
    hash = (hash * 31 + name.charCodeAt(i)) & 0xffffffff
  }
  return avatarColors[Math.abs(hash) % avatarColors.length]
}

function enrollmentPercent(record) {
  const max = Number(record?.maxStudents || 0)
  const cur = Number(record?.currentStudents || 0)
  if (max <= 0) return 0
  return Math.min(100, Math.round((cur / max) * 100))
}

function enrollmentColor(record) {
  const pct = enrollmentPercent(record)
  if (pct >= 100) return '#ef4444'   // đỏ: đầy
  if (pct >= 80) return '#f59e0b'    // cam: sắp đầy
  if (pct >= 30) return '#10b981'    // xanh: tốt
  return '#3b82f6'                   // xanh dương: còn ít
}

const modeLabels = LEARNING_MODE
const modeDotColors = { 0: '#64748b', 1: '#3b82f6', 2: '#8b5cf6' }
const modeBg = {
  0: { bg: 'rgba(100, 116, 139, 0.1)', color: '#475569' },
  1: { bg: 'rgba(59, 130, 246, 0.1)', color: '#2563eb' },
  2: { bg: 'rgba(139, 92, 246, 0.1)', color: '#7c3aed' }
}

function learningModeLabel(mode) {
  if (mode === null || mode === undefined) return '—'
  return modeLabels[Number(mode)] || '—'
}
function learningModeDotColor(mode) {
  return modeDotColors[Number(mode)] || '#94a3b8'
}
function learningModeStyle(mode) {
  const s = modeBg[Number(mode)] || modeBg[0]
  return { background: s.bg, color: s.color }
}

function formatDateRange(start, end) {
  if (!start && !end) return '—'
  if (start && !end) return formatShort(start)
  if (!start && end) return `→ ${formatShort(end)}`
  return `${formatShort(start)} – ${formatShort(end)}`
}

function formatShort(d) {
  if (!d) return ''
  const date = new Date(d)
  if (Number.isNaN(date.getTime())) return d
  const dd = String(date.getDate()).padStart(2, '0')
  const mm = String(date.getMonth() + 1).padStart(2, '0')
  const yy = date.getFullYear()
  return `${dd}/${mm}/${yy}`
}

function scheduleHint(start, end) {
  if (!start) return ''
  const s = new Date(start)
  const now = new Date()
  if (Number.isNaN(s.getTime())) return ''
  const diffMs = s - now
  const diffDays = Math.round(diffMs / (1000 * 60 * 60 * 24))
  if (diffDays > 0) return `Còn ${diffDays} ngày nữa`
  if (diffDays === 0) return 'Khai giảng hôm nay'
  if (end) {
    const e = new Date(end)
    if (!Number.isNaN(e.getTime()) && e > now) {
      const total = Math.round((e - s) / (1000 * 60 * 60 * 24))
      const passed = Math.round((now - s) / (1000 * 60 * 60 * 24))
      return `Đang diễn ra (${passed}/${total} ngày)`
    }
  }
  return 'Đã kết thúc'
}

async function loadFilterDependencies() {
  try {
    const [coursesRes, teachersRes, classroomsRes] = await Promise.all([
      courseApi.getAll(),
      teacherApi.getAll(),
      classroomApi.getAll()
    ])
    courses.value = coursesRes?.items || coursesRes?.data || coursesRes || []
    teachers.value = teachersRes?.items || teachersRes?.data || teachersRes || []
    classrooms.value = classroomsRes?.items || classroomsRes?.data || classroomsRes || []
    fields.find(field => field.name === 'courseId').options = courses.value.map(course => ({
      value: course.id,
      label: `${course.code || '---'} - ${course.name}`,
    }))
    fields.find(field => field.name === 'teacherId').options = teachers.value.map(teacher => ({
      value: teacher.id,
      label: teacher.fullName,
    }))
    fields.find(field => field.name === 'classroomId').options = classrooms.value.map(c => ({
      value: c.id,
      label: `${c.code} - ${c.name} (${c.capacity} chỗ)`
    }))
  } catch (error) {
    // Fail silently
  }
}

onMounted(loadFilterDependencies)
</script>

<style scoped>
.admin-class-cell-ellipsis {
  display: block;
  min-width: 0;
  max-width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
