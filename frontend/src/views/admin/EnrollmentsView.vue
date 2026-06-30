<template>
  <div>
    <AdminResourceView
      title="Quản lý ghi danh"
      subtitle="Theo dõi đăng ký khóa học, xác nhận, hủy hoặc hoàn thành ghi danh."
      :api="enrollmentApi"
      :columns="columns"
      :fields="fields"
      :searchable-fields="['studentNameSnapshot', 'courseNameSnapshot', 'classNameSnapshot']"
      :status-options="statusOptions"
      :form-groups="formGroups"
      :filter-fn="customFilter"
      @reset="resetCustomFilters"
    >
      <template #filters>
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

        <a-select
          v-model:value="filterClassId"
          placeholder="Lớp học"
          allow-clear
          size="small"
          class="w-40"
          :loading="loadingClasses"
          :disabled="!filterCourseId"
        >
          <a-select-option v-for="cls in filteredFilterClasses" :key="cls.id" :value="cls.id">
            {{ cls.className || cls.name }}
          </a-select-option>
        </a-select>

        <a-select
          v-model:value="filterStatusValue"
          placeholder="Trạng thái"
          allow-clear
          size="small"
          class="w-36"
        >
          <a-select-option :value="1">Chờ xác nhận</a-select-option>
          <a-select-option :value="2">Đã xác nhận</a-select-option>
          <a-select-option :value="3">Đang học</a-select-option>
          <a-select-option :value="4">Hoàn thành</a-select-option>
          <a-select-option :value="5">Đã hủy</a-select-option>
        </a-select>

        <a-range-picker
          v-model:value="filterDateRange"
          value-format="YYYY-MM-DD"
          size="small"
          class="w-56"
          :placeholder="['Ghi danh từ', 'Đến']"
        />
      </template>

      <template #bulkActions="{ selectedRowKeys, refresh }">
        <a-button
          size="small"
          type="primary"
          class="h-7 text-[11px]"
          :disabled="!selectedRowKeys.length"
          @click="triggerBulkConfirm(selectedRowKeys, refresh)"
        >
          Xác nhận
        </a-button>
        <a-button
          size="small"
          danger
          ghost
          class="h-7 text-[11px]"
          :disabled="!selectedRowKeys.length"
          @click="triggerBulkCancel(selectedRowKeys, refresh)"
        >
          Hủy ghi danh
        </a-button>
      </template>

      <template #rowActions="{ record, refresh }">
        <a-menu-item
          v-if="enrollmentStatusValue(record.status) === 1"
          class="rounded-lg px-3 py-2 text-xs"
          @click="triggerConfirmOne(record.id, refresh)"
        >
          Xác nhận ghi danh
        </a-menu-item>
        <a-menu-item
          v-if="[2, 3].includes(enrollmentStatusValue(record.status))"
          class="rounded-lg px-3 py-2 text-xs text-emerald-600"
          @click="triggerCompleteOne(record.id, refresh)"
        >
          Hoàn thành khóa học
        </a-menu-item>
        <a-menu-item
          v-if="![4, 5].includes(enrollmentStatusValue(record.status))"
          class="rounded-lg px-3 py-2 text-xs text-rose-600"
          @click="triggerCancelOne(record.id, refresh)"
        >
          Hủy ghi danh
        </a-menu-item>
      </template>

      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'studentNameSnapshot'">
          <a-popover trigger="hover" placement="rightTop" overlay-class-name="enrollment-student-popover">
            <template #content>
              <div class="enrollment-student-card">
                <div class="enrollment-student-header">
                  <div
                    class="enrollment-student-avatar"
                    :style="{ background: avatarColor(studentDisplayName(record)) }"
                  >
                    {{ initials(studentDisplayName(record)) }}
                  </div>
                  <div class="min-w-0">
                    <h3>{{ studentDisplayName(record) || 'Học viên' }}</h3>
                    <p>{{ studentDisplayCode(record) || 'Chưa có mã học viên' }}</p>
                  </div>
                </div>

                <div class="enrollment-student-grid">
                  <div><span>Email</span><strong>{{ studentDetail(record).email || '—' }}</strong></div>
                  <div><span>Điện thoại</span><strong>{{ studentDetail(record).phone || '—' }}</strong></div>
                  <div><span>Ngày sinh</span><strong>{{ formatDate(studentDetail(record).dateOfBirth) }}</strong></div>
                  <div><span>Tuổi</span><strong>{{ computeAge(studentDetail(record).dateOfBirth) }}</strong></div>
                  <div><span>Giới tính</span><strong>{{ genderLabel(studentDetail(record).gender) }}</strong></div>
                  <div><span>Trạng thái</span><strong>{{ studentStatusLabel(studentDetail(record).status) }}</strong></div>
                </div>

                <div class="enrollment-student-address">
                  <span>Địa chỉ</span>
                  <strong>{{ studentDetail(record).address || 'Chưa cập nhật' }}</strong>
                </div>

                <div class="enrollment-student-footer">
                  <div>
                    <span>Khóa học</span>
                    <strong>{{ record.courseNameSnapshot || '—' }}</strong>
                  </div>
                  <div>
                    <span>Lớp</span>
                    <strong>{{ record.classNameSnapshot || '—' }}</strong>
                  </div>
                </div>
              </div>
            </template>

            <div class="enrollment-student-cell">
              <div
                class="enrollment-student-cell-avatar"
                :style="{ background: avatarColor(studentDisplayName(record)) }"
              >
                {{ initials(studentDisplayName(record)) }}
              </div>
              <div class="min-w-0">
                <div class="text-xs font-semibold text-base-primary truncate max-w-[160px]" :title="studentDisplayName(record)">
                  {{ studentDisplayName(record) || '—' }}
                </div>
                <div v-if="studentDisplayCode(record)" class="text-[10px] text-base-muted font-mono">
                  {{ studentDisplayCode(record) }}
                </div>
              </div>
            </div>
          </a-popover>
        </template>

        <template v-else-if="column.key === 'courseNameSnapshot'">
          <div class="leading-tight">
            <div class="text-xs text-base-primary truncate max-w-[160px]" :title="record.courseNameSnapshot">
              {{ record.courseNameSnapshot || '—' }}
            </div>
            <div v-if="record.classNameSnapshot" class="text-[10px] text-base-muted">
              Lớp: {{ record.classNameSnapshot }}
            </div>
          </div>
        </template>

        <template v-else-if="column.key === 'enrolledAt'">
          <span class="text-xs text-base-secondary whitespace-nowrap">
            {{ formatDate(record.enrolledAt) }}
          </span>
        </template>

        <template v-else-if="column.key === 'note'">
          <span
            v-if="record.note"
            class="text-xs text-base-secondary truncate block max-w-[140px]"
            :title="record.note"
          >
            {{ record.note }}
          </span>
          <span v-else class="text-base-muted text-xs italic">—</span>
        </template>
      </template>
    </AdminResourceView>

    <ConfirmActionModal
      v-model:open="confirmOpen"
      :title="confirmTitle"
      :message="confirmMsg"
      :type="confirmType"
      :loading="confirmLoading"
      @confirm="handleExecuteAction"
    />
  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { message } from 'ant-design-vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import ConfirmActionModal from '@/components/admin/ConfirmActionModal.vue'
import { enrollmentApi } from '@/api/enrollmentApi'
import { courseApi } from '@/api/courseApi'
import { classApi } from '@/api/classApi'
import { studentApi } from '@/api/studentApi'
import { ENROLLMENT_STATUS, toOptions } from '@/lib/constants'
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

const statusOptions = toOptions(ENROLLMENT_STATUS, { 1: 'orange', 2: 'blue', 3: 'purple', 4: 'green', 5: 'red' })

const courses = ref([])
const classes = ref([])
const students = ref([])
const loadingCourses = ref(false)
const loadingClasses = ref(false)
const loadingStudents = ref(false)

const filterCourseId = ref(undefined)
const filterClassId = ref(undefined)
const filterStatusValue = ref(undefined)
const filterDateRange = ref(null)

const filteredFilterClasses = computed(() => {
  if (!filterCourseId.value) return []
  return classes.value.filter((cls) => String(cls.courseId) === String(filterCourseId.value))
})

const confirmOpen = ref(false)
const confirmTitle = ref('')
const confirmMsg = ref('')
const confirmType = ref('warning')
const confirmLoading = ref(false)
let confirmActionCallback = null

const ENROLLMENT_STATUS_VALUE = {
  Pending: 1,
  Confirmed: 2,
  Studying: 3,
  Completed: 4,
  Cancelled: 5,
}

const STUDENT_STATUS_LABEL = {
  1: 'Hoạt động',
  2: 'Không hoạt động',
  3: 'Tạm dừng',
  Active: 'Hoạt động',
  Inactive: 'Không hoạt động',
  Suspended: 'Tạm dừng',
}

const GENDER_LABEL = {
  0: 'Không rõ',
  1: 'Nam',
  2: 'Nữ',
  3: 'Khác',
  Unknown: 'Không rõ',
  Male: 'Nam',
  Female: 'Nữ',
  Other: 'Khác',
}

const AVATAR_COLORS = [
  '#4f46e5', '#7c3aed', '#db2777', '#0891b2',
  '#059669', '#d97706', '#dc2626', '#65a30d',
]

const columns = [
  { title: 'Học viên', key: 'studentNameSnapshot', width: 200 },
  { title: 'Khóa học / Lớp', key: 'courseNameSnapshot', width: 200 },
  { title: 'Ngày ghi danh', key: 'enrolledAt', width: 140 },
  { title: 'Ghi chú', key: 'note', width: 160 },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 140 },
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
    options: (formState) => filteredClassOptions(formState.courseId),
    required: true,
    default: '',
    disabled: (formState) => !formState.courseId,
    placeholder: (formState) => {
      if (loadingClasses.value) return 'Đang tải lớp học...'
      return formState.courseId ? 'Chọn lớp học' : 'Chọn khóa học trước'
    },
    onChange: (_value, formState, { option }) => applyClassSnapshot(formState, option?.item, courses.value),
  },
  { name: 'studentNameSnapshot', label: 'Tên học viên', hidden: true, required: true, default: '' },
  { name: 'studentCodeSnapshot', label: 'Mã học viên', hidden: true, default: '' },
  { name: 'courseNameSnapshot', label: 'Tên khóa học', hidden: true, required: true, default: '' },
  { name: 'classNameSnapshot', label: 'Tên lớp', hidden: true, default: '' },
  { name: 'status', label: 'Trạng thái', type: 'select', options: statusOptions, default: 1 },
  { name: 'note', label: 'Ghi chú', type: 'textarea', fullWidth: true, default: '' },
])

const formGroups = [
  { title: 'Học viên', fields: ['studentNameSnapshot', 'studentCodeSnapshot', 'studentId'] },
  { title: 'Khóa học & Lớp học', fields: ['courseNameSnapshot', 'courseId', 'classNameSnapshot', 'classId'] },
  { title: 'Trạng thái & Ghi chú', fields: ['status', 'note'] },
]

function enrollmentStatusValue(status) {
  const numeric = Number(status)
  if (Number.isInteger(numeric)) return numeric
  return ENROLLMENT_STATUS_VALUE[status]
}

function customFilter(item) {
  const matchCourse = !filterCourseId.value || item.courseId === filterCourseId.value
  const matchClass = !filterClassId.value || item.classId === filterClassId.value
  const matchStatus = filterStatusValue.value === undefined || enrollmentStatusValue(item.status) === Number(filterStatusValue.value)

  let matchDate = true
  if (filterDateRange.value && filterDateRange.value.length >= 2) {
    const [start, end] = filterDateRange.value
    const startDate = new Date(start)
    const endDate = new Date(end)
    endDate.setHours(23, 59, 59, 999)
    if (item.enrolledAt) {
      const enrolledDate = new Date(item.enrolledAt)
      matchDate = enrolledDate >= startDate && enrolledDate <= endDate
    }
  }

  return matchCourse && matchClass && matchDate && matchStatus
}

function resetCustomFilters() {
  filterCourseId.value = undefined
  filterClassId.value = undefined
  filterStatusValue.value = undefined
  filterDateRange.value = null
}

watch(filterCourseId, (courseId) => {
  if (!courseId) {
    filterClassId.value = undefined
    return
  }

  const currentClass = classes.value.find((cls) => cls.id === filterClassId.value)
  if (currentClass && String(currentClass.courseId) !== String(courseId)) {
    filterClassId.value = undefined
  }
})

function filteredClassOptions(courseId) {
  if (!courseId) return []
  return classOptions(classes.value.filter((cls) => String(cls.courseId) === String(courseId)))
}

function triggerConfirmOne(id, refresh) {
  confirmTitle.value = 'Xác nhận ghi danh?'
  confirmMsg.value = 'Học viên sẽ được chính thức xếp lớp và bắt đầu quá trình học tập.'
  confirmType.value = 'info'
  confirmActionCallback = async () => {
    await enrollmentApi.confirm(id)
    message.success('Đã xác nhận ghi danh thành công')
    refresh()
  }
  confirmOpen.value = true
}

function triggerCompleteOne(id, refresh) {
  confirmTitle.value = 'Hoàn thành khóa học?'
  confirmMsg.value = 'Xác nhận học viên đã hoàn tất chương trình đào tạo của lớp này.'
  confirmType.value = 'success'
  confirmActionCallback = async () => {
    await enrollmentApi.complete(id)
    message.success('Đã hoàn thành khóa học')
    refresh()
  }
  confirmOpen.value = true
}

function triggerCancelOne(id, refresh) {
  confirmTitle.value = 'Hủy ghi danh học viên?'
  confirmMsg.value = 'Lưu ý: Hành động này sẽ rút học viên khỏi danh sách lớp học và không thể hoàn tác.'
  confirmType.value = 'danger'
  confirmActionCallback = async () => {
    await enrollmentApi.cancel(id)
    message.success('Đã hủy ghi danh')
    refresh()
  }
  confirmOpen.value = true
}

function triggerBulkConfirm(ids, refresh) {
  confirmTitle.value = 'Xác nhận ghi danh hàng loạt?'
  confirmMsg.value = `Bạn đang xác nhận ghi danh cho ${ids.length} học viên đã chọn.`
  confirmType.value = 'info'
  confirmActionCallback = async () => {
    await enrollmentApi.bulkConfirm(ids)
    message.success('Đã xác nhận ghi danh hàng loạt')
    refresh()
  }
  confirmOpen.value = true
}

function triggerBulkCancel(ids, refresh) {
  confirmTitle.value = 'Hủy ghi danh hàng loạt?'
  confirmMsg.value = `Bạn đang thực hiện hủy ghi danh cho ${ids.length} học viên đã chọn. Hành động này không thể hoàn tác.`
  confirmType.value = 'danger'
  confirmActionCallback = async () => {
    await enrollmentApi.bulkCancel(ids)
    message.success('Đã hủy ghi danh hàng loạt')
    refresh()
  }
  confirmOpen.value = true
}

async function handleExecuteAction() {
  if (!confirmActionCallback) return
  confirmLoading.value = true
  try {
    await confirmActionCallback()
    confirmOpen.value = false
  } catch (error) {
    message.error(error.message || 'Không thể thực hiện hành động')
  } finally {
    confirmLoading.value = false
  }
}

async function loadFilterDependencies() {
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
  } catch (error) {
    // Trang vẫn dùng được với snapshot ghi danh nếu danh mục liên kết tải lỗi.
  } finally {
    loadingCourses.value = false
    loadingClasses.value = false
    loadingStudents.value = false
  }
}

function studentDetail(record) {
  const matched = students.value.find((student) => student.id === record.studentId)
  return matched || {
    fullName: record.studentNameSnapshot,
    studentCode: record.studentCodeSnapshot,
  }
}

function studentDisplayName(record) {
  return studentDetail(record).fullName || record.studentNameSnapshot || ''
}

function studentDisplayCode(record) {
  return studentDetail(record).studentCode || record.studentCodeSnapshot || ''
}

function initials(name) {
  if (!name) return '?'
  const parts = String(name).trim().split(/\s+/)
  if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

function avatarColor(name) {
  if (!name) return AVATAR_COLORS[0]
  let hash = 0
  for (let i = 0; i < name.length; i += 1) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length]
}

function genderLabel(gender) {
  return GENDER_LABEL[gender] || '—'
}

function studentStatusLabel(status) {
  return STUDENT_STATUS_LABEL[status] || '—'
}

function ageFromDate(dob) {
  if (!dob) return 0
  const birth = new Date(dob)
  const today = new Date()
  let age = today.getFullYear() - birth.getFullYear()
  const monthOffset = today.getMonth() - birth.getMonth()
  if (monthOffset < 0 || (monthOffset === 0 && today.getDate() < birth.getDate())) age -= 1
  return Math.max(age, 0)
}

function computeAge(dob) {
  const age = ageFromDate(dob)
  return age > 0 ? `${age} tuổi` : '—'
}

function formatDate(value) {
  if (!value) return '—'
  return new Date(value).toLocaleDateString('vi-VN')
}

onMounted(loadFilterDependencies)
</script>

<style scoped>
.enrollment-student-cell {
  display: inline-flex;
  max-width: 100%;
  align-items: center;
  gap: 10px;
  min-width: 0;
  padding: 4px 6px;
  margin: -4px -6px;
  border-radius: 10px;
  cursor: default;
  transition: background 0.16s ease, box-shadow 0.16s ease;
}

.enrollment-student-cell:hover {
  background: rgba(37, 99, 235, 0.06);
  box-shadow: inset 0 0 0 1px rgba(37, 99, 235, 0.12);
}

.enrollment-student-cell-avatar {
  display: inline-flex;
  width: 32px;
  height: 32px;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  border-radius: 999px;
  color: #ffffff;
  font-size: 11px;
  font-weight: 800;
}

:global(.enrollment-student-popover .ant-popover-inner) {
  padding: 0;
  border-radius: 14px;
  overflow: hidden;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.18);
}

.enrollment-student-card {
  width: 380px;
  background: #ffffff;
  color: #0f172a;
}

.enrollment-student-header {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  border-bottom: 1px solid #e2e8f0;
  background: linear-gradient(180deg, #f8fbff 0%, #ffffff 100%);
}

.enrollment-student-avatar {
  display: inline-flex;
  width: 46px;
  height: 46px;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  border-radius: 999px;
  color: #ffffff;
  font-size: 13px;
  font-weight: 900;
}

.enrollment-student-header h3 {
  margin: 0;
  color: #0f172a;
  font-size: 15px;
  font-weight: 800;
  line-height: 1.25;
}

.enrollment-student-header p {
  margin: 3px 0 0;
  color: #64748b;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-size: 11px;
}

.enrollment-student-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  padding: 14px 16px 8px;
}

.enrollment-student-grid div,
.enrollment-student-address,
.enrollment-student-footer div {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 3px;
}

.enrollment-student-grid span,
.enrollment-student-address span,
.enrollment-student-footer span {
  color: #64748b;
  font-size: 11px;
  font-weight: 700;
}

.enrollment-student-grid strong,
.enrollment-student-address strong,
.enrollment-student-footer strong {
  color: #0f172a;
  font-size: 12px;
  font-weight: 800;
  line-height: 1.35;
  overflow-wrap: anywhere;
}

.enrollment-student-address {
  padding: 8px 16px 14px;
}

.enrollment-student-footer {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  padding: 10px 16px;
  border-top: 1px solid #e2e8f0;
  background: #f8fafc;
}
</style>
