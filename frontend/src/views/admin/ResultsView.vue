<template>
  <AdminResourceView
    title="Kết quả học tập"
    subtitle="Nhập và quản lý điểm giữa kỳ, cuối kỳ, chuyên cần và kết quả tổng kết."
    :api="resultApi"
    :columns="columns"
    :fields="fields"
    :searchable-fields="['studentNameSnapshot', 'courseNameSnapshot', 'classNameSnapshot']"
    status-field="resultStatus"
    :status-options="statusOptions"
    :form-groups="formGroups"
    :filter-fn="customFilter"
    @reset="resetCustomFilters"
  >
    <!-- Custom Filters -->
    <template #filters>
      <!-- Course Selector -->
      <a-select
        v-model:value="filterCourseId"
        placeholder="Khóa học"
        allow-clear
        size="small"
        class="w-44"
        :loading="loadingCourses"
      >
        <a-select-option v-for="course in courses" :key="course.id" :value="course.id">
          {{ course.name }}
        </a-select-option>
      </a-select>

      <!-- Status filter -->
      <a-select
        v-model:value="filterStatus"
        placeholder="Kết quả"
        allow-clear
        size="small"
        class="w-32"
      >
        <a-select-option :value="1">Đang học</a-select-option>
        <a-select-option :value="2">Đạt</a-select-option>
        <a-select-option :value="3">Không đạt</a-select-option>
      </a-select>

      <!-- Score Range Filter -->
      <a-select
        v-model:value="filterScoreRange"
        placeholder="Khoảng điểm"
        allow-clear
        size="small"
        class="w-40"
      >
        <a-select-option value="fail">Không đạt (0–4.9)</a-select-option>
        <a-select-option value="avg">Trung bình (5–6.9)</a-select-option>
        <a-select-option value="good">Khá – Giỏi (7–10)</a-select-option>
      </a-select>
    </template>

    <!-- Custom cells for score display -->
    <template #bodyCell="{ column, record }">
      <!-- Student name with avatar -->
      <template v-if="column.key === 'studentNameSnapshot'">
        <div class="flex items-center gap-2.5 min-w-0">
          <div
            class="w-8 h-8 rounded-full flex items-center justify-center text-[11px] font-bold text-white flex-shrink-0"
            :style="{ background: avatarColor(record.studentNameSnapshot) }"
          >
            {{ initials(record.studentNameSnapshot) }}
          </div>
          <div class="min-w-0">
            <div class="text-xs font-semibold text-base-primary truncate max-w-[160px]" :title="record.studentNameSnapshot">
              {{ record.studentNameSnapshot || '—' }}
            </div>
            <div v-if="record.studentCodeSnapshot" class="text-[10px] text-base-muted font-mono">
              {{ record.studentCodeSnapshot }}
            </div>
          </div>
        </div>
      </template>

      <!-- Course + class combined -->
      <template v-else-if="column.key === 'courseNameSnapshot'">
        <div class="leading-tight">
          <div class="text-xs text-base-primary truncate max-w-[160px]" :title="record.courseNameSnapshot">
            {{ record.courseNameSnapshot || '—' }}
          </div>
          <div v-if="record.classNameSnapshot" class="text-[10px] text-base-muted">
            {{ record.classNameSnapshot }}
          </div>
        </div>
      </template>

      <!-- Midterm score with mini progress bar -->
      <template v-else-if="column.key === 'midtermScore'">
        <ScoreCell :score="record.midtermScore" />
      </template>

      <!-- Final score with mini progress bar -->
      <template v-else-if="column.key === 'finalScore'">
        <ScoreCell :score="record.finalScore" />
      </template>

      <!-- Average score colored -->
      <template v-else-if="column.key === 'averageScore'">
        <span
          class="text-xs font-bold"
          :class="record.averageScore >= 5 ? 'text-emerald-600 dark:text-emerald-400' : 'text-rose-600 dark:text-rose-400'"
        >
          {{ record.averageScore != null ? record.averageScore.toFixed(1) : '—' }}
        </span>
      </template>

      <!-- Attendance with mini bar -->
      <template v-else-if="column.key === 'attendancePercent'">
        <div class="flex items-center gap-2">
          <span class="text-xs font-medium text-base-primary">
            {{ record.attendancePercent != null ? record.attendancePercent : '—' }}{{ record.attendancePercent != null ? '%' : '' }}
          </span>
          <div v-if="record.attendancePercent != null" class="w-12 h-1 bg-slate-100 dark:bg-slate-800 rounded-full overflow-hidden">
            <div
              class="h-full rounded-full bg-blue-400"
              :style="{ width: `${Math.min(record.attendancePercent, 100)}%` }"
            ></div>
          </div>
        </div>
      </template>

      <!-- Result status badge -->
      <template v-else-if="column.key === 'resultStatus'">
        <StatusBadge :value="record.resultStatus" :options="statusOptions" />
      </template>
    </template>
  </AdminResourceView>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import StatusBadge from '@/components/admin/StatusBadge.vue'
import { resultApi } from '@/api/resultApi'
import { courseApi } from '@/api/courseApi'
import { classApi } from '@/api/classApi'
import { studentApi } from '@/api/studentApi'
import { RESULT_STATUS, toOptions } from '@/lib/constants'
import {
  applyClassSnapshot,
  applyCourseSnapshot,
  applyStudentSnapshot,
  asList,
  classOptions,
  courseOptions,
  findById,
  studentOptions,
} from '@/lib/adminRelationOptions'

// Inline ScoreCell component
const ScoreCell = {
  props: ['score'],
  template: `
    <div class="space-y-0.5">
      <span class="text-xs font-semibold" :class="scoreClass">
        {{ score != null ? score : '—' }}
      </span>
      <div v-if="score != null" class="w-14 h-1 bg-slate-100 dark:bg-slate-800 rounded-full overflow-hidden">
        <div class="h-full rounded-full transition-all" :class="barClass" :style="{ width: Math.min((score / 10) * 100, 100) + '%' }"></div>
      </div>
    </div>
  `,
  computed: {
    scoreClass() {
      if (this.score == null) return 'text-base-muted'
      if (this.score >= 8) return 'text-emerald-600 dark:text-emerald-400'
      if (this.score >= 5) return 'text-blue-600 dark:text-blue-400'
      return 'text-rose-600 dark:text-rose-400'
    },
    barClass() {
      if (this.score == null) return ''
      if (this.score >= 8) return 'bg-emerald-500'
      if (this.score >= 5) return 'bg-blue-400'
      return 'bg-rose-400'
    }
  }
}

const statusOptions = toOptions(RESULT_STATUS, { 1: 'blue', 2: 'green', 3: 'red' })

// Filter states
const filterCourseId = ref(undefined)
const filterScoreRange = ref(undefined)
const filterStatus = ref(undefined)
const courses = ref([])
const classes = ref([])
const students = ref([])
const loadingCourses = ref(false)
const loadingClasses = ref(false)
const loadingStudents = ref(false)

const columns = [
  { title: 'Học viên', key: 'studentNameSnapshot', width: 220 },
  { title: 'Khóa học / Lớp', key: 'courseNameSnapshot', width: 190 },
  { title: 'Giữa kỳ', key: 'midtermScore', width: 100, sorter: (a, b) => (a.midtermScore || 0) - (b.midtermScore || 0) },
  { title: 'Cuối kỳ', key: 'finalScore', width: 100, sorter: (a, b) => (a.finalScore || 0) - (b.finalScore || 0) },
  { title: 'TB', key: 'averageScore', width: 80, sorter: (a, b) => (a.averageScore || 0) - (b.averageScore || 0) },
  { title: 'Chuyên cần', key: 'attendancePercent', width: 120 },
  { title: 'Kết quả', key: 'resultStatus', width: 120 },
]

// Form fields — clean, no raw IDs in labels
const legacyFields = [
  { name: 'studentNameSnapshot', label: 'Tên học viên', required: true, default: '' },
  { name: 'studentCodeSnapshot', label: 'Mã học viên', default: '' },
  { name: 'studentId', label: 'ID Học viên', required: true, default: '' },
  { name: 'courseNameSnapshot', label: 'Tên khóa học', required: true, default: '' },
  { name: 'courseId', label: 'ID Khóa học', required: true, default: '' },
  { name: 'classNameSnapshot', label: 'Tên lớp', default: '' },
  { name: 'classId', label: 'ID Lớp học', default: '' },
  { name: 'midtermScore', label: 'Điểm giữa kỳ', type: 'number', default: null },
  { name: 'finalScore', label: 'Điểm cuối kỳ', type: 'number', default: null },
  { name: 'attendancePercent', label: 'Chuyên cần (%)', type: 'number', default: 100 },
  { name: 'evaluatedByTeacherName', label: 'Giáo viên chấm', default: '' },
  { name: 'feedback', label: 'Nhận xét', type: 'textarea', fullWidth: true, default: '' },
]

const fields = computed(() => [
  {
    name: 'studentId',
    label: 'Học viên',
    type: 'select',
    options: studentOptions(students.value),
    required: true,
    default: '',
    placeholder: loadingStudents.value ? 'Đang tải học viên...' : 'Chọn học viên',
    onChange: (_value, formState, { option }) => applyStudentSnapshot(formState, option?.item),
  },
  {
    name: 'courseId',
    label: 'Khóa học',
    type: 'select',
    options: courseOptions(courses.value),
    required: true,
    default: '',
    placeholder: loadingCourses.value ? 'Đang tải khóa học...' : 'Chọn khóa học',
    onChange: (value, formState, { option }) => {
      applyCourseSnapshot(formState, option?.item)
      const currentClass = findById(classes.value, formState.classId)
      if (currentClass?.courseId && currentClass.courseId !== value) {
        formState.classId = ''
        formState.classNameSnapshot = ''
      }
    },
  },
  {
    name: 'classId',
    label: 'Lớp học',
    type: 'select',
    options: classOptions(classes.value),
    required: true,
    default: '',
    placeholder: loadingClasses.value ? 'Đang tải lớp học...' : 'Chọn lớp học',
    onChange: (_value, formState, { option }) => applyClassSnapshot(formState, option?.item, courses.value),
  },
  { name: 'studentNameSnapshot', label: 'Tên học viên', hidden: true, required: true, default: '' },
  { name: 'studentCodeSnapshot', label: 'Mã học viên', hidden: true, default: '' },
  { name: 'courseNameSnapshot', label: 'Tên khóa học', hidden: true, required: true, default: '' },
  { name: 'classNameSnapshot', label: 'Tên lớp', hidden: true, default: '' },
  { name: 'midtermScore', label: 'Điểm giữa kỳ', type: 'number', default: null },
  { name: 'finalScore', label: 'Điểm cuối kỳ', type: 'number', default: null },
  { name: 'attendancePercent', label: 'Chuyên cần (%)', type: 'number', default: 100 },
  { name: 'evaluatedByTeacherName', label: 'Giáo viên chấm', default: '' },
  { name: 'feedback', label: 'Nhận xét', type: 'textarea', fullWidth: true, default: '' },
])

const formGroups = [
  { title: 'Học viên', fields: ['studentNameSnapshot', 'studentCodeSnapshot', 'studentId'] },
  { title: 'Khóa học & Lớp học', fields: ['courseNameSnapshot', 'courseId', 'classNameSnapshot', 'classId'] },
  { title: 'Điểm số & Chuyên cần', fields: ['midtermScore', 'finalScore', 'attendancePercent'] },
  { title: 'Đánh giá', fields: ['evaluatedByTeacherName', 'feedback'] },
]

function customFilter(item) {
  const matchCourse = !filterCourseId.value || item.courseId === filterCourseId.value
  const matchStatus = filterStatus.value === undefined || Number(item.resultStatus) === Number(filterStatus.value)

  let matchScore = true
  const avg = item.averageScore ?? item.finalScore ?? null
  if (filterScoreRange.value === 'fail' && avg !== null) {
    matchScore = avg < 5
  } else if (filterScoreRange.value === 'avg' && avg !== null) {
    matchScore = avg >= 5 && avg < 7
  } else if (filterScoreRange.value === 'good' && avg !== null) {
    matchScore = avg >= 7
  }

  return matchCourse && matchScore && matchStatus
}

function resetCustomFilters() {
  filterCourseId.value = undefined
  filterScoreRange.value = undefined
  filterStatus.value = undefined
}

// Avatar helpers
function initials(name) {
  if (!name) return '?'
  const parts = String(name).trim().split(/\s+/)
  if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

const AVATAR_COLORS = [
  '#4f46e5', '#7c3aed', '#db2777', '#0891b2',
  '#059669', '#d97706', '#dc2626', '#65a30d',
]

function avatarColor(name) {
  if (!name) return AVATAR_COLORS[0]
  let hash = 0
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length]
}

async function loadDependencies() {
  loadingCourses.value = true
  loadingClasses.value = true
  loadingStudents.value = true
  try {
    const [coursesRes, classesRes, studentsRes] = await Promise.all([
      courseApi.getAll(),
      classApi.getAll(),
      studentApi.getAll(),
    ])
    courses.value = asList(coursesRes)
    classes.value = asList(classesRes)
    students.value = asList(studentsRes)
  } catch (error) {}
  finally {
    loadingCourses.value = false
    loadingClasses.value = false
    loadingStudents.value = false
  }
}

onMounted(loadDependencies)
</script>
