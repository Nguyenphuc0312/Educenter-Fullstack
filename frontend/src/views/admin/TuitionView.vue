<template>
  <div>
    <AdminResourceView
      ref="resourceView"
      title="Học phí và hóa đơn"
      subtitle="Theo dõi học phí, số tiền đã đóng, công nợ và trạng thái hóa đơn từng học viên."
      :api="tuitionApi"
      :columns="columns"
      :fields="fields"
      :searchable-fields="['invoiceCode', 'studentNameSnapshot', 'courseNameSnapshot', 'classNameSnapshot']"
      :status-options="statusOptions"
      :form-groups="formGroups"
      :filter-fn="customFilter"
      :can-select="canMarkOverdue"
      :has-row-actions="hasTuitionActions"
      @reset="resetCustomFilters"
    >
      <!-- Custom Filters -->
      <template #filters>
        <!-- Trạng thái hóa đơn -->
        <a-select
          v-model:value="filterStatus"
          placeholder="Trạng thái"
          allow-clear
          size="small"
          class="w-40"
        >
          <a-select-option :value="1">Chưa thanh toán</a-select-option>
          <a-select-option :value="2">Một phần</a-select-option>
          <a-select-option :value="3">Đã thanh toán</a-select-option>
          <a-select-option :value="4">Quá hạn</a-select-option>
        </a-select>

        <!-- Khóa học -->
        <a-select
          v-model:value="filterCourseId"
          placeholder="Khóa học"
          allow-clear
          size="small"
          class="w-48"
          :loading="loadingCourses"
        >
          <a-select-option v-for="course in courses" :key="course.id" :value="course.id">
            {{ course.name }}
          </a-select-option>
        </a-select>

        <!-- Hạn thanh toán -->
        <a-range-picker
          v-model:value="filterDueDateRange"
          value-format="YYYY-MM-DD"
          size="small"
          class="w-56"
          :placeholder="['Hạn từ', 'Đến']"
        />
      </template>

      <template #actions>
        <button
          type="button"
          class="admin-btn admin-btn-secondary h-9 px-3"
          :disabled="upcomingReminderLoading"
          @click="sendUpcomingReminders"
        >
          <span v-if="upcomingReminderLoading" class="inline-block w-3.5 h-3.5 border-2 border-slate-300 border-t-slate-700 rounded-full animate-spin"></span>
          Nhắc sắp đến hạn
        </button>
        <button
          type="button"
          class="admin-btn admin-btn-secondary h-9 px-3"
          :disabled="scanLoading"
          @click="scanOverdueInvoices"
        >
          <span v-if="scanLoading" class="inline-block w-3.5 h-3.5 border-2 border-slate-300 border-t-slate-700 rounded-full animate-spin"></span>
          Quét quá hạn
        </button>
      </template>

      <!-- Bulk actions -->
      <template #bulkActions="{ selectedRowKeys, refresh }">
        <a-button
          size="small"
          class="h-8 px-3 border border-amber-200 dark:border-amber-800 text-amber-600 dark:text-amber-400 text-xs font-medium"
          :disabled="!selectedRowKeys.length"
          @click="triggerBulkOverdue(selectedRowKeys, refresh)"
        >
          Đánh dấu quá hạn
        </a-button>
      </template>

      <!-- Row Actions -->
      <template #rowActions="{ record, refresh }">
        <a-menu-item
          v-if="canMarkOverdue(record)"
          class="rounded-lg px-3 py-2 text-xs"
          @click="triggerMarkOverdue(record.id, refresh)"
        >
          Đánh dấu quá hạn
        </a-menu-item>
        <a-menu-item
          v-if="canSendReminder(record)"
          class="rounded-lg px-3 py-2 text-xs"
          @click="triggerSendReminder(record, refresh)"
        >
          Gửi cảnh báo học phí
        </a-menu-item>
      </template>

      <!-- Custom Body Cells -->
      <template #bodyCell="{ column, record }">
        <!-- Invoice code cell: readable short format -->
        <template v-if="column.key === 'invoiceCode'">
          <span class="font-mono text-[11px] font-semibold text-slate-600 dark:text-slate-300 bg-slate-100 dark:bg-slate-800 px-1.5 py-0.5 rounded">
            {{ shortCode(record.invoiceCode) }}
          </span>
        </template>

        <!-- Remaining amount cell -->
        <template v-else-if="column.key === 'remainingAmount'">
          <span
            v-if="getRemaining(record) === 0"
            class="text-xs font-semibold text-emerald-600 dark:text-emerald-400 whitespace-nowrap"
          >
            —
          </span>
          <span
            v-else
            class="text-xs font-bold text-rose-600 dark:text-rose-400 whitespace-nowrap"
          >
            {{ formatVnd(getRemaining(record)) }}
          </span>
        </template>

        <!-- Progress bar cell -->
        <template v-else-if="column.key === 'progress'">
          <div class="space-y-1 max-w-[160px]">
            <div class="flex items-center justify-between text-[10px] gap-2">
              <span class="text-emerald-600 dark:text-emerald-400 font-semibold whitespace-nowrap">
                {{ formatVnd(record.paidAmount || 0) }}
              </span>
              <span class="text-base-muted whitespace-nowrap">
                / {{ formatVnd(record.totalAmount || 0) }}
              </span>
            </div>
            <div class="w-full h-1.5 bg-slate-100 dark:bg-slate-800 rounded-full overflow-hidden">
              <div
                class="h-full rounded-full transition-all"
                :class="getProgressBarClass(record)"
                :style="{ width: `${getPaymentProgress(record)}%` }"
              ></div>
            </div>
          </div>
        </template>

        <!-- Student name cell with code -->
        <template v-else-if="column.key === 'studentNameSnapshot'">
          <a-popover trigger="hover" placement="rightTop" overlay-class-name="tuition-student-popover">
            <template #content>
              <div class="tuition-student-card">
                <div class="tuition-student-header">
                  <div
                    class="tuition-student-avatar"
                    :style="{ background: avatarColor(studentDisplayName(record)) }"
                  >
                    {{ initials(studentDisplayName(record)) }}
                  </div>
                  <div class="min-w-0">
                    <h3>{{ studentDisplayName(record) || 'Học viên' }}</h3>
                    <p>{{ studentDisplayCode(record) || 'Chưa có mã học viên' }}</p>
                  </div>
                </div>

                <div class="tuition-student-grid">
                  <div><span>Email</span><strong>{{ studentDetail(record).email || '—' }}</strong></div>
                  <div><span>Điện thoại</span><strong>{{ studentDetail(record).phone || '—' }}</strong></div>
                  <div><span>Ngày sinh</span><strong>{{ formatDate(studentDetail(record).dateOfBirth) }}</strong></div>
                  <div><span>Tuổi</span><strong>{{ computeAge(studentDetail(record).dateOfBirth) }}</strong></div>
                  <div><span>Giới tính</span><strong>{{ genderLabel(studentDetail(record).gender) }}</strong></div>
                  <div><span>Trạng thái</span><strong>{{ studentStatusLabel(studentDetail(record).status) }}</strong></div>
                </div>

                <div class="tuition-student-address">
                  <span>Địa chỉ</span>
                  <strong>{{ studentDetail(record).address || 'Chưa cập nhật' }}</strong>
                </div>

                <div class="tuition-student-footer">
                  <div>
                    <span>Khóa học</span>
                    <strong>{{ record.courseNameSnapshot || '—' }}</strong>
                  </div>
                  <div>
                    <span>Lớp học</span>
                    <strong>{{ record.classNameSnapshot || '—' }}</strong>
                  </div>
                </div>
              </div>
            </template>

            <div class="tuition-student-cell">
              <div
                class="tuition-student-cell-avatar"
                :style="{ background: avatarColor(studentDisplayName(record)) }"
              >
                {{ initials(studentDisplayName(record)) }}
              </div>
              <div class="min-w-0">
                <div class="text-xs font-semibold text-base-primary truncate max-w-[160px]" :title="studentDisplayName(record)">
                  {{ studentDisplayName(record) || '—' }}
                </div>
                <div v-if="studentDisplayCode(record)" class="text-[10px] text-base-muted font-mono truncate max-w-[160px]">
                  {{ studentDisplayCode(record) }}
                </div>
              </div>
            </div>
          </a-popover>
        </template>

        <template v-else-if="column.key === 'reminderStatus'">
          <div class="leading-tight">
            <span
              class="inline-flex items-center rounded-full px-2 py-0.5 text-[11px] font-bold"
              :class="record.lastReminderSentAt ? 'bg-blue-50 text-blue-700 border border-blue-100' : 'bg-slate-50 text-slate-500 border border-slate-100'"
            >
              {{ record.lastReminderSentAt ? `Đã cảnh báo ${record.reminderCount || 1} lần` : 'Chưa cảnh báo' }}
            </span>
            <div v-if="record.lastReminderSentAt" class="mt-1 text-[10px] text-base-muted">
              {{ formatDateTime(record.lastReminderSentAt) }}
            </div>
          </div>
        </template>
      </template>
    </AdminResourceView>

    <!-- Overdue Confirm Modal -->
    <ConfirmActionModal
      v-model:open="confirmOpen"
      :title="confirmTitle"
      :message="confirmMsg"
      type="warning"
      :loading="confirmLoading"
      ok-text="Đánh dấu quá hạn"
      @confirm="handleExecuteAction"
    />
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import ConfirmActionModal from '@/components/admin/ConfirmActionModal.vue'
import { tuitionApi } from '@/api/tuitionApi'
import { courseApi } from '@/api/courseApi'
import { classApi } from '@/api/classApi'
import { studentApi } from '@/api/studentApi'
import { INVOICE_STATUS, toOptions } from '@/lib/constants'
import { formatVnd } from '@/lib/formatters'
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

const statusOptions = toOptions(INVOICE_STATUS, { 1: 'amber', 2: 'orange', 3: 'green', 4: 'red' })

// Filter states
const filterStatus = ref(undefined)
const filterCourseId = ref(undefined)
const filterDueDateRange = ref(null)
const courses = ref([])
const classes = ref([])
const students = ref([])
const loadingCourses = ref(false)
const loadingClasses = ref(false)
const loadingStudents = ref(false)

// Confirm modal states
const confirmOpen = ref(false)
const confirmTitle = ref('')
const confirmMsg = ref('')
const confirmLoading = ref(false)
const scanLoading = ref(false)
const upcomingReminderLoading = ref(false)
const resourceView = ref(null)
let confirmActionCallback = null

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

// Table columns
const columns = [
  { title: 'Cảnh báo', key: 'reminderStatus', width: 150 },
  { title: 'Mã hóa đơn', key: 'invoiceCode', width: 140 },
  { title: 'Học viên', key: 'studentNameSnapshot', width: 180 },
  { title: 'Khóa học', dataIndex: 'courseNameSnapshot', key: 'courseNameSnapshot', width: 160, ellipsis: true },
  { title: 'Tổng tiền', dataIndex: 'totalAmount', key: 'totalAmount', type: 'money', width: 130, sorter: (a, b) => (a.totalAmount || 0) - (b.totalAmount || 0) },
  { title: 'Đã đóng', dataIndex: 'paidAmount', key: 'paidAmount', type: 'money', width: 130, sorter: (a, b) => (a.paidAmount || 0) - (b.paidAmount || 0) },
  { title: 'Còn nợ', key: 'remainingAmount', width: 110, sortValue: (record) => Number(record.totalAmount || 0) - Number(record.paidAmount || 0) },
  { title: 'Tiến độ', key: 'progress', width: 200, sortValue: (record) => Number(record.totalAmount) > 0 ? Number(record.paidAmount || 0) / Number(record.totalAmount) : 0 },
  { title: 'Hạn đóng', dataIndex: 'dueDate', key: 'dueDate', type: 'date', width: 120 },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 160 },
]

// Form fields — clean, no raw IDs
const legacyFields = [
  { name: 'invoiceCode', label: 'Mã hóa đơn', required: true, editOnly: true, default: '' },
  { name: 'studentId', label: 'ID Học viên', required: true, default: '' },
  { name: 'studentNameSnapshot', label: 'Tên học viên', required: true, default: '' },
  { name: 'studentCodeSnapshot', label: 'Mã học viên', default: '' },
  { name: 'courseId', label: 'ID Khóa học', required: true, default: '' },
  { name: 'courseNameSnapshot', label: 'Tên khóa học', required: true, default: '' },
  { name: 'classId', label: 'ID Lớp học', default: '' },
  { name: 'classNameSnapshot', label: 'Tên lớp', default: '' },
  { name: 'totalAmount', label: 'Tổng tiền', type: 'number', required: true, default: 0 },
  { name: 'paidAmount', label: 'Đã đóng', type: 'number', default: 0 },
  { name: 'dueDate', label: 'Hạn đóng', type: 'date', default: '' },
  { name: 'status', label: 'Trạng thái', type: 'select', options: statusOptions, default: 1 },
]

const fields = computed(() => [
  { name: 'invoiceCode', label: 'Mã hóa đơn', required: true, editOnly: true, default: '' },
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
    placeholder: loadingClasses.value ? 'Đang tải lớp học...' : 'Chọn lớp học',
    onChange: (_value, formState, { option }) => applyClassSnapshot(formState, option?.item, courses.value),
  },
  { name: 'studentNameSnapshot', label: 'Tên học viên', hidden: true, required: true, default: '' },
  { name: 'studentCodeSnapshot', label: 'Mã học viên', hidden: true, default: '' },
  { name: 'courseNameSnapshot', label: 'Tên khóa học', hidden: true, required: true, default: '' },
  { name: 'classNameSnapshot', label: 'Tên lớp', hidden: true, default: '' },
  { name: 'totalAmount', label: 'Tổng tiền', type: 'number', required: true, default: 0 },
  { name: 'paidAmount', label: 'Đã đóng', type: 'number', default: 0 },
  { name: 'dueDate', label: 'Hạn đóng', type: 'date', default: '' },
  { name: 'status', label: 'Trạng thái', type: 'select', options: statusOptions, default: 1 },
])

const formGroups = [
  { title: 'Thông tin hóa đơn', fields: ['invoiceCode', 'status', 'dueDate'] },
  { title: 'Học viên', fields: ['studentId', 'studentNameSnapshot', 'studentCodeSnapshot'] },
  { title: 'Khóa học & Lớp học', fields: ['courseId', 'courseNameSnapshot', 'classId', 'classNameSnapshot'] },
  { title: 'Thanh toán', fields: ['totalAmount', 'paidAmount'] },
]

const invoiceStatusValues = { Unpaid: 1, Partial: 2, Paid: 3, Overdue: 4 }

function enumNumber(value, values) {
  if (typeof value === 'number') return value
  if (typeof value === 'string' && /^\d+$/.test(value)) return Number(value)
  return values[value] ?? value
}

function sameId(left, right) {
  return String(left || '').toLowerCase() === String(right || '').toLowerCase()
}

function customFilter(item) {
  const itemStatus = enumNumber(item.status, invoiceStatusValues)
  const matchStatus = filterStatus.value === undefined || Number(itemStatus) === Number(filterStatus.value)
  const matchCourse = filterCourseId.value === undefined || sameId(item.courseId, filterCourseId.value)

  let matchDueDate = true
  if (filterDueDateRange.value && filterDueDateRange.value.length >= 2) {
    const [start, end] = filterDueDateRange.value
    const startDate = new Date(start)
    const endDate = new Date(end)
    endDate.setHours(23, 59, 59, 999)
    if (!item.dueDate) matchDueDate = false
    else {
      const dueDate = new Date(item.dueDate)
      matchDueDate = !Number.isNaN(dueDate.getTime()) && dueDate >= startDate && dueDate <= endDate
    }
  }

  return matchStatus && matchCourse && matchDueDate
}
function resetCustomFilters() {
  filterStatus.value = undefined
  filterCourseId.value = undefined
  filterDueDateRange.value = null
}

function filteredClassOptions(courseId) {
  if (!courseId) return []
  return classOptions(classes.value.filter((cls) => String(cls.courseId) === String(courseId)))
}

function getRemaining(record) {
  return Math.max((record.totalAmount || 0) - (record.paidAmount || 0), 0)
}

function isPaid(status) {
  return Number(status) === 3 || status === 'Paid'
}

function canMarkOverdue(record) {
  if (!record || isPaid(record.status)) return false
  const debt = getRemaining(record)
  if (debt <= 0) return false
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  const partialDue = record.partialPaymentDueDate ? new Date(record.partialPaymentDueDate) : null
  if (partialDue) {
    partialDue.setHours(0, 0, 0, 0)
    return partialDue < today
  }
  if (!record.dueDate) return false
  const dueDate = new Date(record.dueDate)
  dueDate.setHours(0, 0, 0, 0)
  return dueDate < today
}

function canSendReminder(record) {
  return Boolean(record) && !isPaid(record.status) && getRemaining(record) > 0
}

function hasTuitionActions(record) {
  return canMarkOverdue(record) || canSendReminder(record)
}

function getPaymentProgress(record) {
  if (!record.totalAmount || record.totalAmount === 0) return 0
  return Math.min(Math.round(((record.paidAmount || 0) / record.totalAmount) * 100), 100)
}

function getProgressBarClass(record) {
  const progress = getPaymentProgress(record)
  if (progress >= 100) return 'bg-emerald-500'
  if (progress >= 50) return 'bg-blue-400'
  if (progress > 0) return 'bg-amber-400'
  return 'bg-rose-400'
}

function shortCode(code) {
  if (!code) return '—'
  if (code.length <= 12) return code
  return code.substring(0, 8) + '…'
}

function formatDateTime(value) {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '—'
  return date.toLocaleString('vi-VN', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit', year: 'numeric' })
}

function formatDate(value) {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '—'
  return date.toLocaleDateString('vi-VN')
}

function studentDetail(record) {
  const matched = students.value.find((student) => sameId(student.id, record.studentId))
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
  if (Number.isNaN(birth.getTime())) return 0
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

function triggerSendReminder(record, refresh) {
  confirmTitle.value = 'Gửi cảnh báo học phí?'
  confirmMsg.value = `Hệ thống sẽ gửi email cảnh báo học phí tới học viên ${record.studentNameSnapshot || ''}.`
  confirmActionCallback = async () => {
    await tuitionApi.sendReminder(record.id)
    message.success('Đã gửi cảnh báo học phí')
    refresh()
  }
  confirmOpen.value = true
}

function triggerMarkOverdue(id, refresh) {
  confirmTitle.value = 'Đánh dấu hóa đơn quá hạn?'
  confirmMsg.value = 'Trạng thái hóa đơn sẽ được cập nhật thành Quá hạn.'
  confirmActionCallback = async () => {
    await tuitionApi.markOverdue(id)
    message.success('Đã đánh dấu quá hạn')
    refresh()
  }
  confirmOpen.value = true
}

function triggerBulkOverdue(ids, refresh) {
  confirmTitle.value = `Đánh dấu ${ids.length} hóa đơn quá hạn?`
  confirmMsg.value = `Trạng thái của ${ids.length} hóa đơn đã chọn sẽ được cập nhật thành Quá hạn.`
  confirmActionCallback = async () => {
    await tuitionApi.bulkMarkOverdue(ids)
    message.success('Đã đánh dấu quá hạn hàng loạt')
    refresh()
  }
  confirmOpen.value = true
}

async function scanOverdueInvoices() {
  scanLoading.value = true
  try {
    const result = await tuitionApi.scanOverdue()
    message.success(`Đã quét ${result.scannedInvoices || 0} hóa đơn, cập nhật ${result.updatedInvoices || 0} hóa đơn quá hạn`)
    await resourceView.value?.fetchItems?.()
  } catch (error) {
    message.error(error.message || 'Không thể quét hóa đơn quá hạn')
  } finally {
    scanLoading.value = false
  }
}

async function sendUpcomingReminders() {
  upcomingReminderLoading.value = true
  try {
    const result = await tuitionApi.sendUpcomingReminders(3)
    message.success(`Đã gửi cảnh báo cho ${result.succeeded || 0} hóa đơn sắp đến hạn`)
    await resourceView.value?.fetchItems?.()
  } catch (error) {
    message.error(error.message || 'Không thể gửi cảnh báo học phí sắp đến hạn')
  } finally {
    upcomingReminderLoading.value = false
  }
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
  } catch (e) {}
  finally {
    loadingCourses.value = false
    loadingClasses.value = false
    loadingStudents.value = false
  }
}

onMounted(loadDependencies)
</script>

<style scoped>
.tuition-student-cell {
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

.tuition-student-cell:hover {
  background: rgba(37, 99, 235, 0.06);
  box-shadow: inset 0 0 0 1px rgba(37, 99, 235, 0.12);
}

.tuition-student-cell-avatar {
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

:global(.tuition-student-popover .ant-popover-inner) {
  padding: 0;
  border-radius: 14px;
  overflow: hidden;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.18);
}

.tuition-student-card {
  width: 380px;
  background: #ffffff;
  color: #0f172a;
}

.tuition-student-header {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  border-bottom: 1px solid #e2e8f0;
  background: linear-gradient(180deg, #f8fbff 0%, #ffffff 100%);
}

.tuition-student-avatar {
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

.tuition-student-header h3 {
  margin: 0;
  color: #0f172a;
  font-size: 15px;
  font-weight: 800;
  line-height: 1.25;
}

.tuition-student-header p {
  margin: 3px 0 0;
  color: #64748b;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-size: 11px;
}

.tuition-student-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  padding: 14px 16px 8px;
}

.tuition-student-grid div,
.tuition-student-address,
.tuition-student-footer div {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 3px;
}

.tuition-student-grid span,
.tuition-student-address span,
.tuition-student-footer span {
  color: #64748b;
  font-size: 11px;
  font-weight: 700;
}

.tuition-student-grid strong,
.tuition-student-address strong,
.tuition-student-footer strong {
  color: #0f172a;
  font-size: 12px;
  font-weight: 800;
  line-height: 1.35;
  overflow-wrap: anywhere;
}

.tuition-student-address {
  padding: 8px 16px 14px;
}

.tuition-student-footer {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  padding: 10px 16px;
  border-top: 1px solid #e2e8f0;
  background: #f8fafc;
}
</style>

