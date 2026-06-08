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
      <!-- Custom filters slot -->
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

        <!-- Class Selector -->
        <a-select
          v-model:value="filterClassId"
          placeholder="Lớp học"
          allow-clear
          size="small"
          class="w-40"
          :loading="loadingClasses"
        >
          <a-select-option v-for="cls in classes" :key="cls.id" :value="cls.id">
            {{ cls.className || cls.name }}
          </a-select-option>
        </a-select>

        <!-- Status filter -->
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

        <!-- Enrolled Date Range Picker -->
        <a-range-picker
          v-model:value="filterDateRange"
          value-format="YYYY-MM-DD"
          size="small"
          class="w-56"
          :placeholder="['Ghi danh từ', 'Đến']"
        />
      </template>

      <!-- Custom Bulk Actions -->
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

      <!-- Row Actions Dropdown items -->
      <template #rowActions="{ record, refresh }">
        <a-menu-item
          v-if="record.status === 1"
          class="rounded-lg px-3 py-2 text-xs"
          @click="triggerConfirmOne(record.id, refresh)"
        >
          Xác nhận ghi danh
        </a-menu-item>
        <a-menu-item
          v-if="record.status === 2 || record.status === 3"
          class="rounded-lg px-3 py-2 text-xs text-emerald-600"
          @click="triggerCompleteOne(record.id, refresh)"
        >
          Hoàn thành khóa học
        </a-menu-item>
        <a-menu-item
          v-if="record.status !== 4 && record.status !== 5"
          class="rounded-lg px-3 py-2 text-xs text-rose-600"
          @click="triggerCancelOne(record.id, refresh)"
        >
          Hủy ghi danh
        </a-menu-item>
      </template>

      <!-- Custom cells -->
      <template #bodyCell="{ column, record }">
        <!-- Student name cell with code -->
        <template v-if="column.key === 'studentNameSnapshot'">
          <div class="leading-tight">
            <div class="text-xs font-semibold text-base-primary truncate max-w-[160px]" :title="record.studentNameSnapshot">
              {{ record.studentNameSnapshot || '—' }}
            </div>
            <div v-if="record.studentCodeSnapshot" class="text-[10px] text-base-muted font-mono">
              {{ record.studentCodeSnapshot }}
            </div>
          </div>
        </template>

        <!-- Course + class cell -->
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

        <!-- Enrollment date -->
        <template v-else-if="column.key === 'enrolledAt'">
          <span class="text-xs text-base-secondary whitespace-nowrap">
            {{ formatDate(record.enrolledAt) }}
          </span>
        </template>

        <!-- Note preview -->
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

    <!-- Confirmation Modal Dialog -->
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
import { onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import ConfirmActionModal from '@/components/admin/ConfirmActionModal.vue'
import { enrollmentApi } from '@/api/enrollmentApi'
import { courseApi } from '@/api/courseApi'
import { classApi } from '@/api/classApi'
import { ENROLLMENT_STATUS, toOptions } from '@/lib/constants'

const statusOptions = toOptions(ENROLLMENT_STATUS, { 1: 'orange', 2: 'blue', 3: 'purple', 4: 'green', 5: 'red' })

// Dynamic drop-down list states
const courses = ref([])
const classes = ref([])
const loadingCourses = ref(false)
const loadingClasses = ref(false)

// Custom filter variables
const filterCourseId = ref(undefined)
const filterClassId = ref(undefined)
const filterStatusValue = ref(undefined)
const filterDateRange = ref(null)

// Confirmation Modal States
const confirmOpen = ref(false)
const confirmTitle = ref('')
const confirmMsg = ref('')
const confirmType = ref('warning')
const confirmLoading = ref(false)
let confirmActionCallback = null

// Table columns
const columns = [
  { title: 'Học viên', key: 'studentNameSnapshot', width: 200 },
  { title: 'Khóa học / Lớp', key: 'courseNameSnapshot', width: 200 },
  { title: 'Ngày ghi danh', key: 'enrolledAt', width: 140 },
  { title: 'Ghi chú', key: 'note', width: 160 },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 140 },
]

// Form fields — clean, no raw IDs exposed in labels
const fields = [
  { name: 'studentNameSnapshot', label: 'Tên học viên', required: true, default: '' },
  { name: 'studentCodeSnapshot', label: 'Mã học viên', default: '' },
  { name: 'studentId', label: 'ID Học viên', required: true, default: '' },
  { name: 'courseNameSnapshot', label: 'Tên khóa học', required: true, default: '' },
  { name: 'courseId', label: 'ID Khóa học', required: true, default: '' },
  { name: 'classNameSnapshot', label: 'Tên lớp', default: '' },
  { name: 'classId', label: 'ID Lớp học', default: '' },
  { name: 'status', label: 'Trạng thái', type: 'select', options: statusOptions, default: 1 },
  { name: 'note', label: 'Ghi chú', type: 'textarea', fullWidth: true, default: '' },
]

const formGroups = [
  {
    title: 'Học viên',
    fields: ['studentNameSnapshot', 'studentCodeSnapshot', 'studentId']
  },
  {
    title: 'Khóa học & Lớp',
    fields: ['courseNameSnapshot', 'courseId', 'classNameSnapshot', 'classId']
  },
  {
    title: 'Trạng thái & Ghi chú',
    fields: ['status', 'note']
  }
]

// Custom filter checks
function customFilter(item) {
  const matchCourse = !filterCourseId.value || item.courseId === filterCourseId.value
  const matchClass = !filterClassId.value || item.classId === filterClassId.value
  const matchStatus = filterStatusValue.value === undefined || Number(item.status) === Number(filterStatusValue.value)

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

// Confirmation helper triggers
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

// Load filter dependencies
async function loadFilterDependencies() {
  loadingCourses.value = true
  loadingClasses.value = true
  try {
    const [coursesRes, classesRes] = await Promise.all([
      courseApi.getAll(),
      classApi.getAll()
    ])
    courses.value = coursesRes?.items || coursesRes?.data || coursesRes || []
    classes.value = classesRes?.items || classesRes?.data || classesRes || []
  } catch (error) {
    // Fail silently
  } finally {
    loadingCourses.value = false
    loadingClasses.value = false
  }
}

function formatDate(value) {
  if (!value) return '—'
  return new Date(value).toLocaleDateString('vi-VN')
}

onMounted(loadFilterDependencies)
</script>
