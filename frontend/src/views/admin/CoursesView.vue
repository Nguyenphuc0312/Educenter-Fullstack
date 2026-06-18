<template>
  <AdminResourceView
    title="Quản lý khóa học"
    subtitle="Quản lý danh mục khóa học, học phí, trạng thái mở lớp và dữ liệu public."
    :api="courseApi"
    :columns="columns"
    :fields="fields"
    :searchable-fields="['code', 'name', 'category', 'level']"
    :status-options="courseStatusOptions"
    :normalize-out="normalizeCourse"
    :form-groups="formGroups"
    :filter-fn="customFilter"
    @reset="resetCustomFilters"
  >
    <!-- Custom filters slot -->
    <template #filters>
      <!-- Category Filter -->
      <a-select
        v-model:value="filterCategory"
        placeholder="Danh mục"
        allow-clear
        size="small"
        class="w-36"
      >
        <a-select-option value="Frontend">Frontend</a-select-option>
        <a-select-option value="Backend">Backend</a-select-option>
        <a-select-option value="Mobile">Mobile</a-select-option>
        <a-select-option value="DevOps">DevOps</a-select-option>
      </a-select>

      <!-- Level Filter -->
      <a-select
        v-model:value="filterLevel"
        placeholder="Cấp độ"
        allow-clear
        size="small"
        class="w-32"
      >
        <a-select-option value="Cơ bản">Cơ bản</a-select-option>
        <a-select-option value="Trung cấp">Trung cấp</a-select-option>
        <a-select-option value="Nâng cao">Nâng cao</a-select-option>
      </a-select>

      <!-- Tuition Range Filter -->
      <a-select
        v-model:value="filterTuitionRange"
        placeholder="Mức học phí"
        allow-clear
        size="small"
        class="w-36"
      >
        <a-select-option value="under_5m">Dưới 5 triệu</a-select-option>
        <a-select-option value="5m_10m">5 - 10 triệu</a-select-option>
        <a-select-option value="above_10m">Trên 10 triệu</a-select-option>
      </a-select>

      <!-- Best Seller Filter -->
      <a-select
        v-model:value="filterBestSeller"
        placeholder="Bán chạy"
        allow-clear
        size="small"
        class="w-28"
      >
        <a-select-option :value="true">Bán chạy</a-select-option>
        <a-select-option :value="false">Thường</a-select-option>
      </a-select>
    </template>

    <!-- Custom cells -->
    <template #bodyCell="{ column, record }">
      <!-- Course cell: thumbnail/icon + name + code -->
      <template v-if="column.key === 'name'">
        <div class="flex items-center gap-2.5 min-w-0">
          <!-- Thumbnail or category icon -->
          <div
            v-if="record.thumbnailUrl"
            class="w-10 h-10 rounded-lg overflow-hidden flex-shrink-0 bg-slate-100 dark:bg-slate-800"
          >
            <img
              :src="record.thumbnailUrl"
              :alt="record.name"
              class="w-full h-full object-cover"
              loading="lazy"
            />
          </div>
          <div
            v-else
            class="w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0 text-base leading-none"
            :style="{ background: categoryColor(record.category) }"
          >
            {{ categoryEmoji(record.category) }}
          </div>

          <div class="min-w-0">
            <div class="text-xs font-semibold text-base-primary truncate max-w-[180px]" :title="record.name">
              {{ record.name || '—' }}
            </div>
            <div class="text-[10px] text-base-muted font-mono">
              {{ record.code || '—' }}
            </div>
          </div>
        </div>
      </template>

      <!-- Tuition fee with currency format -->
      <template v-else-if="column.key === 'tuitionFee'">
        <span class="font-bold text-base-primary whitespace-nowrap">
          {{ formatVnd(record.tuitionFee || 0) }}
        </span>
      </template>

      <!-- Sessions count -->
      <template v-else-if="column.key === 'totalSessions'">
        <span class="text-xs text-base-secondary whitespace-nowrap">
          {{ record.totalSessions || 0 }} buổi
        </span>
      </template>

      <!-- Category + Level -->
      <template v-else-if="column.key === 'category'">
        <div class="flex flex-col gap-0.5">
          <span class="text-xs font-medium text-base-primary">{{ record.category || '—' }}</span>
          <span class="text-[10px] text-base-muted">{{ record.level || '—' }}</span>
        </div>
      </template>
    </template>

    <!-- Custom row actions -->
    <template #rowActions="{ record }">
      <a-menu-item
        class="rounded-md px-3 py-1.5 text-xs"
        @click="openOpenClassModal(record)"
      >
        <PlusOutlined class="mr-1" /> Mở lớp học mới
      </a-menu-item>
      <a-menu-item
        class="rounded-md px-3 py-1.5 text-xs"
        @click="openAssignStudentsModal(record)"
      >
        <UsergroupAddOutlined class="mr-1" /> Xếp lớp học viên
      </a-menu-item>
    </template>
  </AdminResourceView>

  <!-- Modal: Mở lớp học mới -->
  <a-modal
    v-model:open="openClassModalOpen"
    title="Mở lớp học mới"
    width="640px"
    :centered="true"
    :destroy-on-close="true"
    @ok="handleCreateClass"
    :confirm-loading="creatingClass"
    ok-text="Mở lớp"
    cancel-text="Hủy"
  >
    <a-form :model="classFormState" layout="vertical" class="mt-4 space-y-4">
      <div class="grid grid-cols-2 gap-4 text-xs">
        <a-form-item label="Khóa học" class="col-span-2 mb-2">
          <a-input :value="selectedCourse?.name" disabled class="bg-slate-50 text-xs font-semibold" />
        </a-form-item>
        <a-form-item label="Mã lớp học" required class="mb-2">
          <a-input v-model:value="classFormState.classCode" placeholder="Ví dụ: CLASS-FE-01" class="text-xs" />
        </a-form-item>
        <a-form-item label="Tên lớp học" required class="mb-2">
          <a-input v-model:value="classFormState.className" placeholder="Ví dụ: Frontend cơ bản K01" class="text-xs" />
        </a-form-item>
        <a-form-item label="Giảng viên" required class="mb-2">
          <a-select
            v-model:value="classFormState.teacherId"
            placeholder="Chọn giảng viên"
            :loading="loadingTeachers"
            show-search
            option-filter-prop="label"
            class="text-xs"
          >
            <a-select-option v-for="t in teachers" :key="t.id" :value="t.id" :label="t.fullName">
              {{ t.fullName }}
            </a-select-option>
          </a-select>
        </a-form-item>
        <a-form-item label="Phòng học" class="mb-2">
          <a-select
            v-model:value="classFormState.classroomId"
            placeholder="Chọn phòng học"
            :loading="loadingClassrooms"
            allow-clear
            show-search
            option-filter-prop="label"
            class="text-xs"
          >
            <a-select-option v-for="c in classrooms" :key="c.id" :value="c.id" :label="`${c.code} - ${c.name}`">
              {{ c.code }} - {{ c.name }} ({{ c.capacity }} chỗ)
            </a-select-option>
          </a-select>
        </a-form-item>
        <a-form-item label="Sĩ số tối thiểu" required class="mb-2">
          <a-input-number v-model:value="classFormState.minStudents" :min="1" class="w-full text-xs" />
        </a-form-item>
        <a-form-item label="Sĩ số tối đa" required class="mb-2">
          <a-input-number v-model:value="classFormState.maxStudents" :min="1" class="w-full text-xs" />
        </a-form-item>
        <a-form-item label="Hình thức học" required class="mb-2">
          <a-select v-model:value="classFormState.learningMode" placeholder="Hình thức học" class="text-xs">
            <a-select-option :value="0">Trực tiếp</a-select-option>
            <a-select-option :value="1">Trực tuyến</a-select-option>
            <a-select-option :value="2">Kết hợp</a-select-option>
          </a-select>
        </a-form-item>
        <a-form-item label="Ca học" class="mb-2">
          <a-select v-model:value="classFormState.studyShift" placeholder="Chọn ca học" class="text-xs">
            <a-select-option :value="0">Sáng (Ca 1)</a-select-option>
            <a-select-option :value="1">Chiều (Ca 2)</a-select-option>
            <a-select-option :value="2">Tối (Ca 3)</a-select-option>
          </a-select>
        </a-form-item>
        <a-form-item label="Giờ bắt đầu" class="mb-2">
          <a-input v-model:value="classFormState.startTime" placeholder="08:00" class="text-xs" />
        </a-form-item>
        <a-form-item label="Giờ kết thúc" class="mb-2">
          <a-input v-model:value="classFormState.endTime" placeholder="10:00" class="text-xs" />
        </a-form-item>
        <a-form-item label="Ngày bắt đầu" required class="mb-2">
          <a-date-picker v-model:value="classFormState.startDate" value-format="YYYY-MM-DD" class="w-full text-xs" />
        </a-form-item>
        <a-form-item label="Ngày kết thúc" class="mb-2">
          <a-date-picker v-model:value="classFormState.endDate" value-format="YYYY-MM-DD" class="w-full text-xs" placeholder="Tự động tính nếu xếp lịch" />
        </a-form-item>
        <a-form-item label="Lịch học trong tuần" class="col-span-2 mb-2">
          <a-checkbox-group v-model:value="classFormState.daysOfWeek" class="w-full text-xs flex flex-wrap gap-2">
            <a-checkbox :value="1">Thứ 2</a-checkbox>
            <a-checkbox :value="2">Thứ 3</a-checkbox>
            <a-checkbox :value="3">Thứ 4</a-checkbox>
            <a-checkbox :value="4">Thứ 5</a-checkbox>
            <a-checkbox :value="5">Thứ 6</a-checkbox>
            <a-checkbox :value="6">Thứ 7</a-checkbox>
            <a-checkbox :value="0">Chủ nhật</a-checkbox>
          </a-checkbox-group>
        </a-form-item>
      </div>
    </a-form>
  </a-modal>

  <!-- Modal: Xếp lớp học viên -->
  <a-modal
    v-model:open="assignStudentsModalOpen"
    title="Xếp lớp học viên đăng ký"
    width="720px"
    :centered="true"
    :destroy-on-close="true"
    @ok="handleBulkAssign"
    :confirm-loading="assigningStudents"
    ok-text="Xếp lớp & Xác nhận"
    cancel-text="Hủy"
  >
    <div class="space-y-4 py-2 text-xs">
      <div class="grid grid-cols-2 gap-4">
        <div>
          <span class="font-semibold">Khóa học:</span> {{ selectedCourse?.name }}
        </div>
        <div class="text-right">
          <span class="font-semibold text-rose-600">
            Có {{ pendingEnrollments.length }} học viên chờ xếp lớp
          </span>
        </div>
      </div>

      <!-- Class selection -->
      <a-form-item label="Chọn lớp học xếp vào" required class="mb-2">
        <a-select
          v-model:value="selectedTargetClassId"
          placeholder="Chọn lớp học trống..."
          class="w-full text-xs"
          :loading="loadingClasses"
        >
          <a-select-option
            v-for="cls in availableClassesForCourse"
            :key="cls.id"
            :value="cls.id"
          >
            {{ cls.className || cls.name }} (Sĩ số: {{ cls.currentStudents }}/{{ cls.maxStudents }})
          </a-select-option>
        </a-select>
      </a-form-item>

      <!-- Pending registrations table -->
      <div>
        <span class="font-semibold block mb-2">Danh sách học viên đăng ký chờ xếp lớp:</span>
        <div v-if="pendingEnrollments.length === 0" class="text-center py-8 border border-dashed rounded-lg bg-slate-50 dark:bg-slate-800">
          Không có học viên nào đang chờ xếp lớp cho khóa học này.
        </div>
        <div v-else class="max-h-[300px] overflow-y-auto border border-base rounded-lg">
          <table class="w-full min-w-full divide-y divide-base">
            <thead class="bg-slate-50 dark:bg-slate-900 sticky top-0">
              <tr>
                <th class="px-4 py-2.5 text-left text-xs font-semibold text-base-secondary w-12">
                  <a-checkbox
                    :checked="isAllSelected"
                    :indeterminate="isIndeterminate"
                    @change="handleSelectAll"
                  />
                </th>
                <th class="px-4 py-2.5 text-left text-xs font-semibold text-base-secondary">Học viên</th>
                <th class="px-4 py-2.5 text-left text-xs font-semibold text-base-secondary">Mã học viên</th>
                <th class="px-4 py-2.5 text-left text-xs font-semibold text-base-secondary">Ngày đăng ký</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-base bg-card-base">
              <tr v-for="item in pendingEnrollments" :key="item.id">
                <td class="px-4 py-2 text-left text-xs">
                  <a-checkbox
                    :checked="selectedEnrollmentIds.includes(item.id)"
                    @change="toggleSelect(item.id)"
                  />
                </td>
                <td class="px-4 py-2 text-left text-xs font-semibold text-base-primary">
                  {{ item.studentNameSnapshot }}
                </td>
                <td class="px-4 py-2 text-left text-xs font-mono text-base-secondary">
                  {{ item.studentCodeSnapshot || '—' }}
                </td>
                <td class="px-4 py-2 text-left text-xs text-base-muted">
                  {{ formatDate(item.enrolledAt) }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </a-modal>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { PlusOutlined, UsergroupAddOutlined } from '@ant-design/icons-vue'
import { Modal, message } from 'ant-design-vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import { courseApi } from '@/api/courseApi'
import { teacherApi } from '@/api/teacherApi'
import { classroomApi } from '@/api/classroomApi'
import { classApi } from '@/api/classApi'
import { enrollmentApi } from '@/api/enrollmentApi'
import { COURSE_STATUS, toOptions } from '@/lib/constants'
import { formatVnd } from '@/lib/formatters'

const router = useRouter()

const courseStatusOptions = toOptions(COURSE_STATUS, { 0: 'default', 1: 'blue', 2: 'slate', 3: 'orange' })

// Custom filter states
const filterCategory = ref(undefined)
const filterLevel = ref(undefined)
const filterTuitionRange = ref(undefined)
const filterBestSeller = ref(undefined)

const columns = [
  { title: 'Khóa học', key: 'name', width: 280 },
  { title: 'Danh mục / Cấp độ', key: 'category', width: 160 },
  { title: 'Học phí', key: 'tuitionFee', type: 'money', width: 140, sorter: (a, b) => (a.tuitionFee || 0) - (b.tuitionFee || 0) },
  { title: 'Số buổi', key: 'totalSessions', width: 100 },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 140 },
]

const fields = [
  { name: 'code', label: 'Mã khóa học', required: true, editOnly: true, default: '' },
  { name: 'name', label: 'Tên khóa học', required: true, default: '' },
  { name: 'slug', label: 'Slug', required: true, editOnly: true, default: '' },
  { name: 'category', label: 'Danh mục', required: true, default: 'Frontend' },
  { name: 'level', label: 'Cấp độ', required: true, default: 'Cơ bản' },
  { name: 'tuitionFee', label: 'Học phí', type: 'number', required: true, default: 0 },
  { name: 'totalSessions', label: 'Tổng số buổi', type: 'number', required: true, default: 12 },
  { name: 'durationText', label: 'Thời lượng', default: '8 tuần' },
  { name: 'status', label: 'Trạng thái', type: 'select', options: courseStatusOptions, default: 1 },
  { name: 'shortDescription', label: 'Mô tả ngắn', type: 'textarea', fullWidth: true, default: '' },
  { name: 'description', label: 'Mô tả chi tiết', type: 'textarea', fullWidth: true, default: '' },
  { name: 'thumbnailUrl', label: 'Ảnh đại diện URL', fullWidth: true, default: '' },
  { name: 'isBestSeller', label: 'Bán chạy', type: 'switch', default: false },
  { name: 'isPopularThisWeek', label: 'Phổ biến tuần này', type: 'switch', default: false },
]

// Sectioned form group configuration
const formGroups = [
  {
    title: 'Thông tin cơ bản',
    fields: ['code', 'name', 'slug', 'category', 'level']
  },
  {
    title: 'Học phí & Thời lượng',
    fields: ['tuitionFee', 'totalSessions', 'durationText']
  },
  {
    title: 'Mô tả khóa học',
    fields: ['shortDescription', 'description', 'thumbnailUrl']
  },
  {
    title: 'Trạng thái & Phân loại',
    fields: ['status', 'isBestSeller', 'isPopularThisWeek']
  }
]

// Custom filtration handler
function customFilter(item) {
  const matchCategory = !filterCategory.value || item.category === filterCategory.value
  const matchLevel = !filterLevel.value || item.level === filterLevel.value

  let matchTuition = true
  if (filterTuitionRange.value === 'under_5m') {
    matchTuition = item.tuitionFee < 5000000
  } else if (filterTuitionRange.value === '5m_10m') {
    matchTuition = item.tuitionFee >= 5000000 && item.tuitionFee <= 10000000
  } else if (filterTuitionRange.value === 'above_10m') {
    matchTuition = item.tuitionFee > 10000000
  }

  const matchBestSeller = filterBestSeller.value === undefined || item.isBestSeller === filterBestSeller.value

  return matchCategory && matchLevel && matchTuition && matchBestSeller
}

function resetCustomFilters() {
  filterCategory.value = undefined
  filterLevel.value = undefined
  filterTuitionRange.value = undefined
  filterBestSeller.value = undefined
}

function normalizeCourse(data) {
  const slug = data.slug || data.name?.toLowerCase().normalize('NFD').replace(/[̀-ͯ]/g, '').replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '')
  return { ...data, slug }
}

// Category visual helpers
const CATEGORY_COLORS = {
  Frontend: '#4f46e5',
  Backend: '#059669',
  Mobile: '#0891b2',
  DevOps: '#d97706',
  Fullstack: '#7c3aed',
  Database: '#dc2626',
  Security: '#65a30d',
}

const CATEGORY_EMOJI = {
  Frontend: '⚛️',
  Backend: '⚙️',
  Mobile: '📱',
  DevOps: '🚀',
  Fullstack: '🧩',
  Database: '💾',
  Security: '🔒',
}

function categoryColor(category) {
  return CATEGORY_COLORS[category] || '#6366f1'
}

function categoryEmoji(category) {
  return CATEGORY_EMOJI[category] || '📚'
}

// ================= Modals State & Logic =================
const teachers = ref([])
const classrooms = ref([])
const loadingTeachers = ref(false)
const loadingClassrooms = ref(false)

const openClassModalOpen = ref(false)
const creatingClass = ref(false)
const selectedCourse = ref(null)
const classFormState = ref({
  classCode: '',
  className: '',
  teacherId: undefined,
  classroomId: undefined,
  minStudents: 5,
  maxStudents: 30,
  learningMode: 0,
  startDate: '',
  endDate: '',
  daysOfWeek: [],
  studyShift: 0,
  startTime: '08:00',
  endTime: '10:00',
})

const assignStudentsModalOpen = ref(false)
const assigningStudents = ref(false)
const pendingEnrollments = ref([])
const availableClassesForCourse = ref([])
const selectedTargetClassId = ref(undefined)
const selectedEnrollmentIds = ref([])
const loadingClasses = ref(false)

const isAllSelected = computed(() => {
  return pendingEnrollments.value.length > 0 && selectedEnrollmentIds.value.length === pendingEnrollments.value.length
})

const isIndeterminate = computed(() => {
  return selectedEnrollmentIds.value.length > 0 && selectedEnrollmentIds.value.length < pendingEnrollments.value.length
})

function handleSelectAll(e) {
  if (e.target.checked) {
    selectedEnrollmentIds.value = pendingEnrollments.value.map(item => item.id)
  } else {
    selectedEnrollmentIds.value = []
  }
}

function toggleSelect(id) {
  const index = selectedEnrollmentIds.value.indexOf(id)
  if (index > -1) {
    selectedEnrollmentIds.value.splice(index, 1)
  } else {
    selectedEnrollmentIds.value.push(id)
  }
}

async function fetchClassroomAndTeacherOptions() {
  loadingTeachers.value = true
  loadingClassrooms.value = true
  try {
    const [teachersRes, classroomsRes] = await Promise.all([
      teacherApi.getAll(),
      classroomApi.getAll()
    ])
    teachers.value = teachersRes?.items || teachersRes?.data || teachersRes || []
    classrooms.value = classroomsRes?.items || classroomsRes?.data || classroomsRes || []
  } catch (err) {
    console.error(err)
  } finally {
    loadingTeachers.value = false
    loadingClassrooms.value = false
  }
}

function openOpenClassModal(course) {
  selectedCourse.value = course
  const todayStr = new Date().toISOString().substring(0, 10)
  classFormState.value = {
    classCode: `LOP-${course.code}-${Math.floor(100 + Math.random() * 900)}`,
    className: `Lớp ${course.name} - K${Math.floor(1 + Math.random() * 10)}`,
    teacherId: undefined,
    classroomId: undefined,
    minStudents: 5,
    maxStudents: 30,
    learningMode: 0,
    startDate: todayStr,
    endDate: '',
    daysOfWeek: [],
    studyShift: 0,
    startTime: '08:00',
    endTime: '10:00',
  }
  openClassModalOpen.value = true
}

async function handleCreateClass() {
  const form = classFormState.value
  if (!form.classCode || !form.className || !form.teacherId || !form.startDate || (!form.endDate && (!form.daysOfWeek || form.daysOfWeek.length === 0))) {
    message.warning('Vui lòng điền đầy đủ các thông tin bắt buộc (Mã, Tên lớp, Giảng viên, Ngày bắt đầu, và Hạn kết thúc/Lịch học)')
    return
  }

  creatingClass.value = true
  try {
    const payload = {
      courseId: selectedCourse.value.id,
      classCode: form.classCode,
      className: form.className,
      teacherId: form.teacherId,
      classroomId: form.classroomId || null,
      room: form.classroomId ? classrooms.value.find(c => c.id === form.classroomId)?.code || classrooms.value.find(c => c.id === form.classroomId)?.name || '' : '',
      minStudents: form.minStudents,
      maxStudents: form.maxStudents,
      currentStudents: 0,
      startDate: form.startDate,
      endDate: form.endDate || form.startDate,
      learningMode: form.learningMode,
      status: 0, // Open
      daysOfWeek: form.daysOfWeek.length > 0 ? form.daysOfWeek : null,
      studyShift: form.daysOfWeek.length > 0 ? form.studyShift : null,
      startTime: form.daysOfWeek.length > 0 ? form.startTime : null,
      endTime: form.daysOfWeek.length > 0 ? form.endTime : null,
    }

    const res = await classApi.create(payload)
    const newClassId = res?.id || res?.data?.id
    openClassModalOpen.value = false

    Modal.confirm({
      title: 'Lớp học đã được mở thành công',
      content: 'Bạn có muốn đi xếp lịch học cho lớp này ngay bây giờ không?',
      okText: 'Xếp lịch ngay',
      cancelText: 'Để sau',
      onOk() {
        if (newClassId) {
          router.push({ path: '/admin/schedules', query: { classId: newClassId } })
        } else {
          router.push({ path: '/admin/schedules' })
        }
      }
    })
  } catch (err) {
    message.error(err.message || 'Không thể mở lớp học')
  } finally {
    creatingClass.value = false
  }
}

async function openAssignStudentsModal(course) {
  selectedCourse.value = course
  selectedTargetClassId.value = undefined
  selectedEnrollmentIds.value = []
  pendingEnrollments.value = []
  availableClassesForCourse.value = []

  assignStudentsModalOpen.value = true
  loadingClasses.value = true

  try {
    const [enrollmentsRes, classesRes] = await Promise.all([
      enrollmentApi.getAll(),
      classApi.getByCourse(course.id)
    ])

    const enrolls = enrollmentsRes?.items || enrollmentsRes?.data || enrollmentsRes || []
    const clss = classesRes?.items || classesRes?.data || classesRes || []

    pendingEnrollments.value = enrolls.filter(e =>
      e.courseId === course.id &&
      (Number(e.status) === 1 || e.status === 'Pending')
    )

    availableClassesForCourse.value = clss.filter(c =>
      Number(c.status) !== 4 && c.status !== 'Cancelled'
    )
  } catch (err) {
    message.error('Không thể tải danh sách lớp học hoặc học viên')
  } finally {
    loadingClasses.value = false
  }
}

async function handleBulkAssign() {
  if (!selectedTargetClassId.value) {
    message.warning('Vui lòng chọn lớp học')
    return
  }
  if (selectedEnrollmentIds.value.length === 0) {
    message.warning('Vui lòng chọn ít nhất một học viên')
    return
  }

  const selectedClass = availableClassesForCourse.value.find(c => c.id === selectedTargetClassId.value)
  if (!selectedClass) return

  assigningStudents.value = true
  try {
    await enrollmentApi.bulkAssign({
      enrollmentIds: selectedEnrollmentIds.value,
      classId: selectedTargetClassId.value,
      classNameSnapshot: selectedClass.className || selectedClass.name
    })

    message.success(`Đã xếp lớp và xác nhận ghi danh cho ${selectedEnrollmentIds.value.length} học viên thành công`)
    assignStudentsModalOpen.value = false
  } catch (err) {
    message.error(err.message || 'Không thể xếp lớp học viên')
  } finally {
    assigningStudents.value = false
  }
}

function formatDate(value) {
  if (!value) return '—'
  return new Date(value).toLocaleDateString('vi-VN')
}

onMounted(() => {
  fetchClassroomAndTeacherOptions()
})
</script>
