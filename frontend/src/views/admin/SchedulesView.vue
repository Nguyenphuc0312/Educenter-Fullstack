<template>
  <div class="space-y-4">
    <!-- Header with Tab Selector -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
      <div>
        <h1 class="text-xl font-bold text-base-primary tracking-tight">Quản lý lịch học</h1>
        <p class="text-xs text-base-secondary mt-0.5">Lịch học theo lớp, ca học, phòng học và chủ đề từng buổi.</p>
      </div>
      <div class="flex items-center gap-2">
        <a-radio-group v-model:value="activeTab" size="small">
          <a-radio-button value="list">Danh sách</a-radio-button>
          <a-radio-button value="calendar">Lịch tuần</a-radio-button>
        </a-radio-group>
      </div>
    </div>

    <!-- Tab 1: List View -->
    <div v-show="activeTab === 'list'">
      <AdminResourceView
        title=""
        subtitle=""
        :api="scheduleApi"
        :columns="columns"
        :fields="fields"
        :searchable-fields="['classNameSnapshot', 'room', 'topic', 'teacherNameSnapshot']"
        :status-options="statusOptions"
      />
    </div>

    <!-- Tab 2: Weekly Calendar View -->
    <div v-show="activeTab === 'calendar'" class="space-y-3">

      <!-- ── Toolbar ── -->
      <div class="bg-card-base border border-base rounded-xl px-4 py-3 shadow-sm">
        <div class="flex flex-wrap items-center gap-2">
          <!-- Lớp -->
          <a-select
            v-model:value="selectedClassId"
            placeholder="Lớp học"
            mode="multiple"
            allow-clear
            :max-tag-count="1"
            class="schedule-filter-select w-56"
            :loading="loadingClasses"
            @change="onCalendarFilterChange"
          >
            <a-select-option v-for="cls in classes" :key="cls.id" :value="cls.id">
              {{ cls.className || cls.name }}
            </a-select-option>
          </a-select>

          <!-- Giảng viên -->
          <a-select
            v-model:value="selectedTeacherId"
            placeholder="Giảng viên"
            mode="multiple"
            allow-clear
            :max-tag-count="1"
            class="schedule-filter-select w-52"
            :loading="loadingTeachers"
            @change="onCalendarFilterChange"
          >
            <a-select-option v-for="t in teachers" :key="t.id" :value="t.id">
              {{ t.fullName }}
            </a-select-option>
          </a-select>

          <!-- Phòng -->
          <a-select
            v-model:value="selectedRoom"
            placeholder="Phòng"
            mode="multiple"
            allow-clear
            :max-tag-count="1"
            class="schedule-filter-select w-36"
            @change="onCalendarFilterChange"
          >
            <a-select-option v-for="room in roomOptions" :key="room" :value="room">
              {{ room }}
            </a-select-option>
          </a-select>

          <!-- Ca -->
          <a-select
            v-model:value="selectedShift"
            placeholder="Ca học"
            mode="multiple"
            allow-clear
            :max-tag-count="1"
            class="schedule-filter-select w-36"
            @change="onCalendarFilterChange"
          >
            <a-select-option :value="0">Sáng</a-select-option>
            <a-select-option :value="1">Chiều</a-select-option>
            <a-select-option :value="2">Tối</a-select-option>
          </a-select>

          <!-- Refresh — nằm cùng hàng toolbar, bên phải -->
          <a-button
            size="small"
            class="admin-btn admin-btn-secondary h-9 px-3 ml-auto shrink-0"
            :loading="calendarLoading"
            @click="fetchCalendarData"
          >
            <template #icon><SyncOutlined :spin="calendarLoading" /></template>
            Làm mới
          </a-button>
        </div>

        <!-- Active filter chips -->
        <div v-if="false && hasActiveFilters" class="flex flex-wrap gap-1.5 mt-2">
          <span v-if="selectedClassId.length" class="admin-filter-chip">
            Lớp: {{ classes.find(c => c.id === selectedClassId)?.className || '—' }}
            <button @click="selectedClassId = undefined; onCalendarFilterChange()">×</button>
          </span>
          <span v-if="selectedTeacherId.length" class="admin-filter-chip">
            GV: {{ teachers.find(t => t.id === selectedTeacherId)?.fullName || '—' }}
            <button @click="selectedTeacherId = undefined; onCalendarFilterChange()">×</button>
          </span>
          <span v-if="selectedRoom.length" class="admin-filter-chip">
            Phòng: {{ selectedRoom }}
            <button @click="selectedRoom = undefined; onCalendarFilterChange()">×</button>
          </span>
          <span v-if="selectedShift.length" class="admin-filter-chip">
            Ca: {{ ['Sáng','Chiều','Tối'][selectedShift] }}
            <button @click="selectedShift = undefined; onCalendarFilterChange()">×</button>
          </span>
        </div>
        <div v-if="false && hasActiveFilters" class="flex flex-wrap gap-1.5 mt-2">
          <span v-if="selectedClassId.length" class="admin-filter-chip">
            Lớp: {{ selectedClassId.length }} lựa chọn
            <button @click="selectedClassId = []; onCalendarFilterChange()">×</button>
          </span>
          <span v-if="selectedTeacherId.length" class="admin-filter-chip">
            Giảng viên: {{ selectedTeacherId.length }} lựa chọn
            <button @click="selectedTeacherId = []; onCalendarFilterChange()">×</button>
          </span>
          <span v-if="selectedRoom.length" class="admin-filter-chip">
            Phòng: {{ selectedRoom.join(', ') }}
            <button @click="selectedRoom = []; onCalendarFilterChange()">×</button>
          </span>
          <span v-if="selectedShift.length" class="admin-filter-chip">
            Ca: {{ selectedShift.map(s => STUDY_SHIFT[s]).join(', ') }}
            <button @click="selectedShift = []; onCalendarFilterChange()">×</button>
          </span>
        </div>
      </div>

      <!-- ── Schedule count ── -->
      <div v-if="hasActiveFilters" class="schedule-selected-filters mt-3">
        <button
          v-for="chip in activeFilterChips"
          :key="`${chip.type}-${chip.value}`"
          type="button"
          class="schedule-filter-chip"
          @click="removeFilterChip(chip)"
          :title="`Bỏ lọc ${chip.label}`"
        >
          <span class="schedule-filter-chip-type">{{ chip.typeLabel }}</span>
          <span class="schedule-filter-chip-label">{{ chip.label }}</span>
          <span class="schedule-filter-chip-remove">×</span>
        </button>
      </div>

      <div v-if="!calendarLoading && filteredSchedules.length > 0" class="flex items-center gap-2 text-xs text-base-secondary px-1">
        <span class="font-semibold">{{ filteredSchedules.length }}</span>
        <span>buổi học</span>
        <span class="text-base-muted">· Kéo thả thẻ để di chuyển lịch</span>
      </div>

      <!-- ── Debug banner ── -->
      <div v-if="false" class="bg-blue-50 dark:bg-blue-950/20 border border-blue-200 dark:border-blue-800 rounded-xl px-4 py-2 text-xs text-blue-700 dark:text-blue-300 font-mono">
        <div class="flex items-center gap-4 flex-wrap">
          <span>Tổng: <strong>{{ schedules.length }}</strong> buổi</span>
          <span>·</span>
          <span>Lọc thấy: <strong>{{ filteredSchedules.length }}</strong></span>
          <span>·</span>
          <span>Ngày mẫu: <strong>{{ sortedDays.map(d => d.label).join(', ') }}</strong></span>
          <span>·</span>
          <span>Ngày có trong data:
            <strong>{{ [...new Set(schedules.map(s => s.dayOfWeek))].sort().join(', ') }}</strong>
          </span>
          <span>·</span>
          <span>Ca có trong data:
            <strong>{{ [...new Set(schedules.map(s => s.studyShift))].sort().join(', ') }}</strong>
          </span>
        </div>
      </div>

      <!-- ── Loading ── -->
      <div v-if="calendarLoading" class="py-16 flex flex-col justify-center items-center gap-3 bg-card-base border border-base rounded-xl">
        <a-spin size="large" />
        <span class="text-xs text-base-secondary">Đang tải lịch học...</span>
      </div>

      <!-- ── Empty ── -->
      <div v-else-if="filteredSchedules.length === 0 && schedules.length > 0" class="bg-card-base border border-base rounded-xl p-8 shadow-sm">
        <EmptyTableState
          :show-action-button="false"
          title="Không tìm thấy lịch học"
          description="Hãy thử thay đổi bộ lọc hoặc kiểm tra dữ liệu."
        />
      </div>

      <!-- ── No data at all ── -->
      <div v-else-if="!calendarLoading && schedules.length === 0" class="bg-card-base border border-base rounded-xl p-8 shadow-sm">
        <EmptyTableState
          :show-action-button="false"
          title="Chưa có lịch học"
          description="Không tìm thấy buổi học nào."
        />
      </div>

      <!-- ── Calendar Grid ── -->
      <div v-else class="bg-card-base border border-base rounded-xl shadow-sm overflow-hidden">

        <!-- Header row -->
        <div class="grid bg-slate-50 dark:bg-slate-900 border-b border-base"
          :style="{ gridTemplateColumns: '80px repeat(7, 1fr)' }">
          <div class="p-2 text-center text-xs font-bold text-base-secondary border-r border-base">Ca</div>
          <div v-for="day in sortedDays" :key="day.value"
            class="p-2 text-center text-xs font-bold text-base-secondary border-r border-base last:border-r-0">
            <div>{{ day.label }}</div>
            <div class="text-[10px] font-normal opacity-60">{{ day.date }}</div>
          </div>
        </div>

        <!-- Shift rows -->
        <div v-for="shift in shifts" :key="shift.value">
          <!-- Shift header -->
          <div class="grid border-b border-base last:border-b-0"
            :style="{ gridTemplateColumns: '80px repeat(7, 1fr)' }">
            <!-- Shift label -->
            <div class="p-2 border-r border-base flex flex-col items-center justify-center gap-0.5">
              <span class="text-xs font-bold" :style="{ color: shift.color }">{{ shift.label }}</span>
              <span class="text-[10px] text-base-muted">{{ shift.time }}</span>
            </div>

            <!-- Day cells -->
            <div v-for="day in sortedDays" :key="day.value"
              class="p-1.5 border-r border-base last:border-r-0 align-top min-h-[80px]"
              :class="{ 'bg-slate-50/50 dark:bg-slate-800/30': dragOverCell === `${day.value}-${shift.value}` }"
              @dragover.prevent="onDragOver(day.value, shift.value)"
              @dragleave="onDragLeave"
              @drop="onDrop(day.value, shift.value)">

              <!-- Cards: stacked vertically, full width, content wraps -->
              <div class="space-y-1">
                <div v-for="item in getCellItems(day.value, shift.value).slice(0, 2)"
                  :key="item.id"
                  :class="['schedule-card rounded-lg p-2 cursor-grab active:cursor-grabbing border select-none transition-all hover:scale-[1.02] min-h-[72px]',
                    getCardClasses(item, shift)]"
                  draggable="true"
                  @dragstart="onDragStart(item, day.value, shift.value)"
                  @dragend="onDragEnd"
                  @click.stop="openDetailDrawer(item)">

                  <!-- Header: status bar (left) + class name -->
                  <div class="flex items-start gap-1.5 mb-1.5">
                    <!-- Colored status bar on the left edge -->
                    <div class="w-1 self-stretch rounded-full shrink-0" :class="getStatusBarClass(item.status)"></div>
                    <div class="flex-1 min-w-0">
                      <div class="font-bold text-[11px] leading-snug text-wrap" :title="item.classNameSnapshot">
                        {{ item.classNameSnapshot }}
                      </div>
                    </div>
                  </div>

                  <!-- Giảng viên -->
                  <div v-if="item.teacherNameSnapshot" class="text-[10px] leading-snug opacity-80 pl-2.5 truncate" :title="item.teacherNameSnapshot">
                    👨‍🏫 {{ item.teacherNameSnapshot }}
                  </div>

                  <!-- Phòng + giờ -->
                  <div class="text-[10px] leading-snug opacity-70 pl-2.5">
                    📍 {{ item.room || '—' }} · {{ formatTime(item.startTime) }}
                  </div>
                </div>
              </div>

              <!-- Overflow pills: scroll horizontally if too many -->
              <div v-if="getCellItems(day.value, shift.value).length > 2"
                class="mt-1 overflow-x-auto scrollbar-hide flex gap-1">
                <div
                  v-for="item in getCellItems(day.value, shift.value).slice(2)"
                  :key="item.id"
                  :class="['schedule-card rounded px-2 py-1 border text-[9px] leading-snug cursor-grab active:cursor-grabbing shrink-0 transition-all hover:scale-105 min-w-0 max-w-[100px]',
                    getCardClasses(item, shift)]"
                  draggable="true"
                  @dragstart="onDragStart(item, day.value, shift.value)"
                  @dragend="onDragEnd"
                  @click.stop="openDetailDrawer(item)">
                  <div class="truncate font-semibold">{{ item.classNameSnapshot }}</div>
                  <div class="opacity-60 truncate">{{ formatTime(item.startTime) }}</div>
                </div>
                <!-- +N more -->
                <div class="shrink-0 flex items-center px-1 text-[9px] text-base-muted font-medium self-center">
                  +{{ getCellItems(day.value, shift.value).length - 2 }}
                </div>
              </div>

              <!-- Drop hint when dragging -->
              <div v-if="isDragging && dragOverCell === `${day.value}-${shift.value}`"
                class="mt-1 text-center text-[9px] font-bold text-primary-500 border border-dashed border-primary-300 dark:border-primary-700 rounded py-1 bg-primary-50 dark:bg-primary-950/20">
                Thả để di chuyển
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ── Detail Drawer ── -->
    <a-drawer
      v-model:open="drawerOpen"
      :title="null"
      placement="right"
      width="400"
      :body-style="{ padding: '0' }"
    >
      <template v-if="selectedSchedule">
        <!-- Drawer header with colored bar -->
        <div class="h-2 w-full" :style="{ background: getShiftColor(selectedSchedule.studyShift) }"></div>
        <div class="p-5 space-y-4">
          <div>
            <h2 class="text-base font-bold text-base-primary">{{ selectedSchedule.classNameSnapshot }}</h2>
            <p class="text-xs text-base-secondary mt-0.5">{{ selectedSchedule.topic || 'Chưa có chủ đề' }}</p>
          </div>

          <!-- Info grid -->
          <div class="grid grid-cols-2 gap-2">
            <div class="bg-slate-50 dark:bg-slate-800 rounded-lg p-2.5">
              <div class="text-[10px] text-base-muted mb-1">Phòng học</div>
              <div class="text-xs font-semibold text-base-primary">{{ selectedSchedule.room || '—' }}</div>
            </div>
            <div class="bg-slate-50 dark:bg-slate-800 rounded-lg p-2.5">
              <div class="text-[10px] text-base-muted mb-1">Buổi thứ</div>
              <div class="text-xs font-semibold text-base-primary">{{ selectedSchedule.sessionNumber }}</div>
            </div>
            <div class="bg-slate-50 dark:bg-slate-800 rounded-lg p-2.5">
              <div class="text-[10px] text-base-muted mb-1">Ca học</div>
              <div class="text-xs font-semibold text-base-primary">
                {{ ['Sáng','Chiều','Tối'][selectedSchedule.studyShift] }}
              </div>
            </div>
            <div class="bg-slate-50 dark:bg-slate-800 rounded-lg p-2.5">
              <div class="text-[10px] text-base-muted mb-1">Thứ</div>
              <div class="text-xs font-semibold text-base-primary">{{ getDayLabel(selectedSchedule.dayOfWeek) }}</div>
            </div>
            <div class="bg-slate-50 dark:bg-slate-800 rounded-lg p-2.5 col-span-2">
              <div class="text-[10px] text-base-muted mb-1">Thời gian</div>
              <div class="text-xs font-semibold text-base-primary">
                {{ formatTime(selectedSchedule.startTime) }} — {{ formatTime(selectedSchedule.endTime) }}
              </div>
            </div>
            <div class="bg-slate-50 dark:bg-slate-800 rounded-lg p-2.5 col-span-2">
              <div class="text-[10px] text-base-muted mb-1">Giảng viên</div>
              <div class="text-xs font-semibold text-base-primary">{{ selectedSchedule.teacherNameSnapshot || '—' }}</div>
            </div>
            <div class="bg-slate-50 dark:bg-slate-800 rounded-lg p-2.5 col-span-2">
              <div class="text-[10px] text-base-muted mb-1">Trạng thái</div>
              <div class="text-xs font-semibold">
                <StatusBadge :value="selectedSchedule.status" :options="statusOptions" />
              </div>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex gap-2 pt-2">
            <a-button class="flex-1" size="small" @click="drawerOpen = false">Đóng</a-button>
            <a-button type="primary" class="flex-1" size="small" @click="drawerOpen = false">
              Sửa lịch
            </a-button>
          </div>
        </div>
      </template>
    </a-drawer>
  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { message } from 'ant-design-vue'
import { SyncOutlined } from '@ant-design/icons-vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import EmptyTableState from '@/components/admin/EmptyTableState.vue'
import StatusBadge from '@/components/admin/StatusBadge.vue'
import { scheduleApi } from '@/api/scheduleApi'
import { classApi } from '@/api/classApi'
import { teacherApi } from '@/api/teacherApi'
import { SCHEDULE_STATUS, STUDY_SHIFT, DAY_OF_WEEK_VN, toOptions } from '@/lib/constants'
import { classOptions } from '@/lib/adminRelationOptions'

// ── State ──
const activeTab = ref('list')
const calendarLoading = ref(false)
const loadingClasses = ref(false)
const loadingTeachers = ref(false)
const selectedClassId = ref([])
const selectedTeacherId = ref([])
const selectedRoom = ref([])
const selectedShift = ref([])
const drawerOpen = ref(false)
const selectedSchedule = ref(null)

// Drag state
const isDragging = ref(false)
const draggedItem = ref(null)
const draggedFromDay = ref(null)
const draggedFromShift = ref(null)
const dragOverCell = ref(null)

// Data
const schedules = ref([])
const classes = ref([])
const teachers = ref([])

// ── Config ──
const statusOptions = toOptions(SCHEDULE_STATUS, { 0: 'blue', 1: 'green', 2: 'red' })

const sortedDays = computed(() => {
  const today = new Date()
  return [1, 2, 3, 4, 5, 6, 0].map(d => {
    const date = new Date(today)
    const diff = (d - today.getDay() + 7) % 7
    date.setDate(today.getDate() + diff)
    return {
      value: d,
      label: DAY_OF_WEEK_VN[d],
      date: date.toLocaleDateString('vi-VN', { day: 'numeric', month: 'short' }),
    }
  })
})

const shifts = [
  { value: 0, label: 'Sáng', time: '08:00 – 12:00', color: '#f59e0b' },
  { value: 1, label: 'Chiều', time: '13:30 – 17:30', color: '#3b82f6' },
  { value: 2, label: 'Tối', time: '18:00 – 22:00', color: '#8b5cf6' },
]

const dayOptions = [
  { value: 0, label: DAY_OF_WEEK_VN[0] },
  { value: 1, label: DAY_OF_WEEK_VN[1] },
  { value: 2, label: DAY_OF_WEEK_VN[2] },
  { value: 3, label: DAY_OF_WEEK_VN[3] },
  { value: 4, label: DAY_OF_WEEK_VN[4] },
  { value: 5, label: DAY_OF_WEEK_VN[5] },
  { value: 6, label: DAY_OF_WEEK_VN[6] },
  { value: 'Sunday', label: DAY_OF_WEEK_VN[0] },
  { value: 'Monday', label: DAY_OF_WEEK_VN[1] },
  { value: 'Tuesday', label: DAY_OF_WEEK_VN[2] },
  { value: 'Wednesday', label: DAY_OF_WEEK_VN[3] },
  { value: 'Thursday', label: DAY_OF_WEEK_VN[4] },
  { value: 'Friday', label: DAY_OF_WEEK_VN[5] },
  { value: 'Saturday', label: DAY_OF_WEEK_VN[6] },
]

const studyShiftOptions = [
  { value: 0, label: STUDY_SHIFT[0] },
  { value: 1, label: STUDY_SHIFT[1] },
  { value: 2, label: STUDY_SHIFT[2] },
  { value: 'Morning', label: STUDY_SHIFT[0] },
  { value: 'Afternoon', label: STUDY_SHIFT[1] },
  { value: 'Evening', label: STUDY_SHIFT[2] },
]

const columns = [
  { title: 'Lớp', dataIndex: 'classNameSnapshot', key: 'classNameSnapshot', width: 180 },
  { title: 'Giảng viên', dataIndex: 'teacherNameSnapshot', key: 'teacherNameSnapshot', width: 160, ellipsis: true },
  { title: 'Phòng', dataIndex: 'room', key: 'room', width: 100 },
  { title: 'Buổi', dataIndex: 'sessionNumber', key: 'sessionNumber', width: 80 },
  { title: 'Thời gian', key: 'timeRange', type: 'timeRange', startField: 'startTime', endField: 'endTime', dayField: 'dayOfWeek', width: 150, sortValue: (record) => `${record.dayOfWeek ?? ''}-${record.startTime ?? ''}` },
  { title: 'Chủ đề', dataIndex: 'topic', key: 'topic', ellipsis: true },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 140 },
]

const legacyFields = [
  { name: 'classId', label: 'ID Lớp học', required: true, default: '' },
  { name: 'dayOfWeek', label: 'Thứ', type: 'select', options: dayOptions, default: 1 },
  { name: 'studyShift', label: 'Ca học', type: 'select', options: studyShiftOptions, default: 0 },
  { name: 'startTime', label: 'Giờ bắt đầu', placeholder: '08:00:00', required: true, default: '08:00:00' },
  { name: 'endTime', label: 'Giờ kết thúc', placeholder: '10:00:00', required: true, default: '10:00:00' },
  { name: 'room', label: 'Phòng học', required: true, default: '' },
  { name: 'topic', label: 'Chủ đề', default: '' },
  { name: 'sessionNumber', label: 'Số buổi', type: 'number', required: true, default: 1 },
  { name: 'status', label: 'Trạng thái', type: 'select', options: statusOptions, default: 0 },
]

// ── Computed ──
const fields = computed(() => [
  {
    name: 'classId',
    label: 'Lớp học',
    type: 'select',
    options: classOptions(classes.value),
    required: true,
    default: '',
    placeholder: loadingClasses.value ? 'Đang tải lớp học...' : 'Chọn lớp học',
  },
  { name: 'dayOfWeek', label: 'Thứ', type: 'select', options: dayOptions, default: 1 },
  { name: 'studyShift', label: 'Ca học', type: 'select', options: studyShiftOptions, default: 0 },
  { name: 'startTime', label: 'Giờ bắt đầu', placeholder: '08:00:00', required: true, default: '08:00:00' },
  { name: 'endTime', label: 'Giờ kết thúc', placeholder: '10:00:00', required: true, default: '10:00:00' },
  { name: 'room', label: 'Phòng học', required: true, default: '' },
  { name: 'topic', label: 'Chủ đề', default: '' },
  { name: 'sessionNumber', label: 'Số buổi', type: 'number', required: true, default: 1 },
  { name: 'status', label: 'Trạng thái', type: 'select', options: statusOptions, default: 0 },
])

const roomOptions = computed(() => {
  const rooms = new Set(schedules.value.map(s => s.room).filter(Boolean))
  return Array.from(rooms).sort()
})

const hasActiveFilters = computed(() =>
  Boolean(selectedClassId.value.length || selectedTeacherId.value.length || selectedRoom.value.length || selectedShift.value.length)
)

const activeFilterChips = computed(() => [
  ...selectedClassId.value.map(value => ({
    type: 'class',
    typeLabel: 'Lớp',
    value,
    label: classes.value.find(c => c.id === value)?.className || classes.value.find(c => c.id === value)?.name || value,
  })),
  ...selectedTeacherId.value.map(value => ({
    type: 'teacher',
    typeLabel: 'Giảng viên',
    value,
    label: teachers.value.find(t => t.id === value)?.fullName || value,
  })),
  ...selectedRoom.value.map(value => ({
    type: 'room',
    typeLabel: 'Phòng',
    value,
    label: value,
  })),
  ...selectedShift.value.map(value => ({
    type: 'shift',
    typeLabel: 'Ca',
    value,
    label: STUDY_SHIFT[value] || value,
  })),
])

const filteredSchedules = computed(() =>
  schedules.value.filter(item => {
    if (selectedClassId.value.length && !selectedClassId.value.includes(item.classId)) return false
    if (selectedTeacherId.value.length && !selectedTeacherId.value.includes(item.teacherId)) return false
    if (selectedRoom.value.length && !selectedRoom.value.includes(item.room)) return false
    if (selectedShift.value.length && !selectedShift.value.includes(normalizeShift(item.studyShift))) return false
    return true
  })
)

// ── Helpers ──
const dayMap = {
  sunday: 0,
  monday: 1,
  tuesday: 2,
  wednesday: 3,
  thursday: 4,
  friday: 5,
  saturday: 6,
}

const shiftMap = {
  morning: 0,
  afternoon: 1,
  evening: 2,
}

function normalizeDay(value) {
  if (value === null || value === undefined) return -1
  if (typeof value === 'number') return value
  const numeric = Number(value)
  if (!Number.isNaN(numeric)) return numeric
  return dayMap[String(value).trim().toLowerCase()] ?? -1
}

function normalizeShift(value) {
  if (value === null || value === undefined) return -1
  if (typeof value === 'number') return value
  const numeric = Number(value)
  if (!Number.isNaN(numeric)) return numeric
  return shiftMap[String(value).trim().toLowerCase()] ?? -1
}

function getCellItems(day, shift) {
  return filteredSchedules.value.filter(s =>
    normalizeDay(s.dayOfWeek) === day && normalizeShift(s.studyShift) === shift
  )
}

function getCardClasses(item, shift) {
  const s = normalizeShift(item.studyShift ?? shift?.value ?? shift)
  const shiftClass = {
    0: 'schedule-card-morning',
    1: 'schedule-card-afternoon',
    2: 'schedule-card-evening',
  }[s] || 'schedule-card-morning'

  if (Number(item.status) === 2) {
    return [shiftClass, 'schedule-card-cancelled']
  }
  return [shiftClass]
}

function getStatusBarClass(status) {
  if (Number(status) === 1) return 'bg-emerald-400'
  if (Number(status) === 2) return 'bg-slate-300 dark:bg-slate-600'
  return 'bg-blue-400'
}

function getStatusDotClass(status) {
  if (Number(status) === 1) return 'bg-emerald-400'
  if (Number(status) === 2) return 'bg-slate-400'
  return 'bg-blue-400'
}

function getShiftColor(shift) {
  return shifts[Number(shift)]?.color || '#6366f1'
}

function getDayLabel(day) {
  return DAY_OF_WEEK_VN[Number(day)] || '—'
}

function formatTime(time) {
  if (!time) return '—'
  return String(time).substring(0, 5)
}

// ── Drag & Drop ──
function onDragStart(item, day, shift) {
  isDragging.value = true
  draggedItem.value = item
  draggedFromDay.value = day
  draggedFromShift.value = shift
}

function onDragEnd() {
  isDragging.value = false
  draggedItem.value = null
  draggedFromDay.value = null
  draggedFromShift.value = null
  dragOverCell.value = null
}

function onDragOver(day, shift) {
  if (isDragging.value) {
    dragOverCell.value = `${day}-${shift}`
  }
}

function onDragLeave() {
  dragOverCell.value = null
}

async function onDrop(day, shift) {
  dragOverCell.value = null
  if (!draggedItem.value) return

  const item = draggedItem.value
  // Skip if dropped on same cell
  if (Number(item.dayOfWeek) === day && Number(item.studyShift) === shift) {
    isDragging.value = false
    return
  }

  try {
    await scheduleApi.update(item.id, {
      ...item,
      dayOfWeek: day,
      studyShift: shift,
    })
    message.success('Đã cập nhật lịch học')
    await fetchCalendarData()
  } catch (err) {
    message.error('Không thể cập nhật lịch học: ' + (err.message || ''))
  } finally {
    onDragEnd()
  }
}

// ── Drawer ──
function openDetailDrawer(item) {
  selectedSchedule.value = item
  drawerOpen.value = true
}

// ── Filter change ──
function onCalendarFilterChange() {
  selectedClassId.value = Array.isArray(selectedClassId.value) ? selectedClassId.value : []
  selectedTeacherId.value = Array.isArray(selectedTeacherId.value) ? selectedTeacherId.value : []
  selectedRoom.value = Array.isArray(selectedRoom.value) ? selectedRoom.value : []
  selectedShift.value = Array.isArray(selectedShift.value) ? selectedShift.value : []
  // Filter is reactive via computed, no extra action needed
}

function removeFilterChip(chip) {
  const removeValue = (list) => list.filter(value => value !== chip.value)
  if (chip.type === 'class') selectedClassId.value = removeValue(selectedClassId.value)
  if (chip.type === 'teacher') selectedTeacherId.value = removeValue(selectedTeacherId.value)
  if (chip.type === 'room') selectedRoom.value = removeValue(selectedRoom.value)
  if (chip.type === 'shift') selectedShift.value = removeValue(selectedShift.value)
  onCalendarFilterChange()
}

// ── Data fetch ──
async function fetchCalendarData() {
  calendarLoading.value = true
  try {
    const [schedsRes, classesRes, teachersRes] = await Promise.all([
      scheduleApi.getAll(),
      classApi.getAll(),
      teacherApi.getAll()
    ])
    schedules.value = schedsRes?.items || schedsRes?.data || schedsRes || []
    classes.value = classesRes?.items || classesRes?.data || classesRes || []
    teachers.value = teachersRes?.items || teachersRes?.data || teachersRes || []
  } catch (err) {
    message.error('Không thể tải dữ liệu lịch học')
  } finally {
    calendarLoading.value = false
  }
}

// Auto-load when switching to calendar tab
watch(activeTab, (val) => {
  if (val === 'calendar' && schedules.value.length === 0) {
    fetchCalendarData()
  }
})
</script>

<style scoped>
/* Hide horizontal scrollbar in overflow rows */
.scrollbar-hide::-webkit-scrollbar { display: none; }
.scrollbar-hide { -ms-overflow-style: none; scrollbar-width: none; }

/* Schedule card: subtle shadow, readable text */
.schedule-card {
  box-shadow: 0 1px 3px rgba(0,0,0,0.08);
}

:deep(.ant-radio-group) {
  display: inline-flex;
  padding: 3px;
  border: 1px solid var(--admin-border, #e2e8f0);
  border-radius: 12px;
  background: var(--admin-surface-2, #f8fafc);
  box-shadow: 0 1px 2px rgba(15, 23, 42, 0.05);
}

:deep(.ant-radio-button-wrapper) {
  height: 30px !important;
  padding: 0 13px !important;
  display: inline-flex !important;
  align-items: center !important;
  border: 0 !important;
  border-radius: 9px !important;
  background: transparent !important;
  color: var(--admin-text-muted, #64748b) !important;
  font-size: 12px !important;
  font-weight: 700 !important;
  box-shadow: none !important;
}

:deep(.ant-radio-button-wrapper::before) {
  display: none !important;
}

:deep(.ant-radio-button-wrapper-checked) {
  background: var(--admin-accent, #4f46e5) !important;
  color: #ffffff !important;
  box-shadow: 0 8px 16px -12px rgba(79, 70, 229, 0.9) !important;
}

.dark :deep(.ant-radio-group) {
  background: #0b1728;
  border-color: #263653;
  box-shadow: none;
}

.dark :deep(.ant-radio-button-wrapper) {
  color: #cbd5e1 !important;
}

.dark :deep(.ant-radio-button-wrapper-checked) {
  background: #2563eb !important;
  color: #ffffff !important;
}

.schedule-filter-select :deep(.ant-select-selector) {
  min-height: 34px !important;
  border-radius: 10px !important;
}

.schedule-filter-select :deep(.ant-select-selection-placeholder),
.schedule-filter-select :deep(.ant-select-selection-item),
.schedule-filter-select :deep(.ant-select-selection-overflow-item) {
  font-size: 12px;
  font-weight: 600;
}

.schedule-filter-select :deep(.ant-select-selection-overflow-item .ant-select-selection-item) {
  border-radius: 8px !important;
  border-color: rgba(79, 70, 229, 0.24) !important;
  background: rgba(79, 70, 229, 0.1) !important;
  color: var(--admin-accent, #4f46e5) !important;
}

.dark .schedule-filter-select :deep(.ant-select-selection-overflow-item .ant-select-selection-item) {
  border-color: rgba(96, 165, 250, 0.36) !important;
  background: rgba(37, 99, 235, 0.18) !important;
  color: #dbeafe !important;
}

.schedule-selected-filters {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

.schedule-filter-chip {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  max-width: 260px;
  min-height: 30px;
  padding: 4px 8px 4px 10px;
  border: 1px solid var(--admin-accent-border, #c7d2fe);
  border-radius: 999px;
  background: var(--admin-accent-soft, #eef2ff);
  color: var(--admin-accent, #4f46e5);
  font-size: 12px;
  font-weight: 700;
  line-height: 1;
  transition: all 0.15s ease;
}

.schedule-filter-chip:hover {
  border-color: var(--admin-accent, #4f46e5);
  background: #ffffff;
}

.schedule-filter-chip-type {
  color: var(--admin-text-muted, #64748b);
  font-weight: 700;
}

.schedule-filter-chip-label {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.schedule-filter-chip-remove {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 16px;
  height: 16px;
  border-radius: 999px;
  background: rgba(79, 70, 229, 0.12);
  color: currentColor;
  font-size: 13px;
  line-height: 1;
}

.dark .schedule-filter-chip {
  border-color: rgba(96, 165, 250, 0.36);
  background: rgba(37, 99, 235, 0.18);
  color: #dbeafe;
}

.dark .schedule-filter-chip:hover {
  border-color: #60a5fa;
  background: rgba(59, 130, 246, 0.26);
}

.dark .schedule-filter-chip-type {
  color: #93c5fd;
}

.dark .schedule-filter-chip-remove {
  background: rgba(191, 219, 254, 0.14);
  color: #ffffff;
}

.schedule-card {
  background: #ffffff;
  border-color: #dbe4f0;
  color: #0f172a;
  box-shadow: 0 8px 18px -16px rgba(15, 23, 42, 0.45);
}

.schedule-card-morning {
  background: #fffbeb;
  border-color: #fde68a;
  color: #92400e;
}

.schedule-card-afternoon {
  background: #eff6ff;
  border-color: #bfdbfe;
  color: #1d4ed8;
}

.schedule-card-evening {
  background: #f5f3ff;
  border-color: #ddd6fe;
  color: #6d28d9;
}

.schedule-card-cancelled {
  opacity: 0.55;
  border-style: dashed;
  color: #64748b;
}

.dark .schedule-card {
  background: linear-gradient(180deg, #13213a 0%, #101b30 100%) !important;
  border-color: #263653 !important;
  color: #e5edf8 !important;
  box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.04), 0 10px 24px -20px rgba(0, 0, 0, 0.9);
}

.dark .schedule-card-morning {
  border-color: rgba(251, 191, 36, 0.45) !important;
  color: #fde68a !important;
}

.dark .schedule-card-afternoon {
  border-color: rgba(96, 165, 250, 0.48) !important;
  color: #93c5fd !important;
}

.dark .schedule-card-evening {
  border-color: rgba(167, 139, 250, 0.5) !important;
  color: #c4b5fd !important;
}

.dark .schedule-card-cancelled {
  background: #111827 !important;
  border-color: #475569 !important;
  color: #94a3b8 !important;
}

.dark .schedule-card .opacity-80,
.dark .schedule-card .opacity-70,
.dark .schedule-card .opacity-60 {
  opacity: 1 !important;
  color: #9fb2cf !important;
}
</style>
