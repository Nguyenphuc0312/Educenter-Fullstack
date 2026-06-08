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
            allow-clear
            size="small"
            class="w-44"
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
            allow-clear
            size="small"
            class="w-40"
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
            allow-clear
            size="small"
            class="w-28"
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
            allow-clear
            size="small"
            class="w-28"
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
        <div v-if="hasActiveFilters" class="flex flex-wrap gap-1.5 mt-2">
          <span v-if="selectedClassId" class="admin-filter-chip">
            Lớp: {{ classes.find(c => c.id === selectedClassId)?.className || '—' }}
            <button @click="selectedClassId = undefined; onCalendarFilterChange()">×</button>
          </span>
          <span v-if="selectedTeacherId" class="admin-filter-chip">
            GV: {{ teachers.find(t => t.id === selectedTeacherId)?.fullName || '—' }}
            <button @click="selectedTeacherId = undefined; onCalendarFilterChange()">×</button>
          </span>
          <span v-if="selectedRoom" class="admin-filter-chip">
            Phòng: {{ selectedRoom }}
            <button @click="selectedRoom = undefined; onCalendarFilterChange()">×</button>
          </span>
          <span v-if="selectedShift !== undefined" class="admin-filter-chip">
            Ca: {{ ['Sáng','Chiều','Tối'][selectedShift] }}
            <button @click="selectedShift = undefined; onCalendarFilterChange()">×</button>
          </span>
        </div>
      </div>

      <!-- ── Schedule count ── -->
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

// ── State ──
const activeTab = ref('list')
const calendarLoading = ref(false)
const loadingClasses = ref(false)
const loadingTeachers = ref(false)
const selectedClassId = ref(undefined)
const selectedTeacherId = ref(undefined)
const selectedRoom = ref(undefined)
const selectedShift = ref(undefined)
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

const columns = [
  { title: 'Lớp', dataIndex: 'classNameSnapshot', key: 'classNameSnapshot', width: 180 },
  { title: 'Giảng viên', dataIndex: 'teacherNameSnapshot', key: 'teacherNameSnapshot', width: 160, ellipsis: true },
  { title: 'Phòng', dataIndex: 'room', key: 'room', width: 100 },
  { title: 'Buổi', dataIndex: 'sessionNumber', key: 'sessionNumber', width: 80 },
  { title: 'Thời gian', key: 'timeRange', width: 150 },
  { title: 'Chủ đề', dataIndex: 'topic', key: 'topic', ellipsis: true },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 140 },
]

const fields = [
  { name: 'classId', label: 'ID Lớp học', required: true, default: '' },
  { name: 'dayOfWeek', label: 'Thứ', type: 'select', options: DAY_OF_WEEK_VN.map((l, i) => ({ value: i, label: l })), default: 1 },
  { name: 'studyShift', label: 'Ca học', type: 'select', options: toOptions(STUDY_SHIFT), default: 0 },
  { name: 'startTime', label: 'Giờ bắt đầu', placeholder: '08:00:00', required: true, default: '08:00:00' },
  { name: 'endTime', label: 'Giờ kết thúc', placeholder: '10:00:00', required: true, default: '10:00:00' },
  { name: 'room', label: 'Phòng học', required: true, default: '' },
  { name: 'topic', label: 'Chủ đề', default: '' },
  { name: 'sessionNumber', label: 'Số buổi', type: 'number', required: true, default: 1 },
  { name: 'status', label: 'Trạng thái', type: 'select', options: statusOptions, default: 0 },
]

// ── Computed ──
const roomOptions = computed(() => {
  const rooms = new Set(schedules.value.map(s => s.room).filter(Boolean))
  return Array.from(rooms).sort()
})

const hasActiveFilters = computed(() =>
  selectedClassId.value || selectedTeacherId.value || selectedRoom.value || selectedShift.value !== undefined
)

const filteredSchedules = computed(() =>
  schedules.value.filter(item => {
    if (selectedClassId.value && item.classId !== selectedClassId.value) return false
    if (selectedTeacherId.value && item.teacherId !== selectedTeacherId.value) return false
    if (selectedRoom.value && item.room !== selectedRoom.value) return false
    if (selectedShift.value !== undefined && normalizeShift(item.studyShift) !== selectedShift.value) return false
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
  const colorMap = {
    0: { bg: 'bg-amber-50 dark:bg-amber-950/30', border: 'border-amber-200 dark:border-amber-800', text: 'text-amber-800 dark:text-amber-200' },
    1: { bg: 'bg-blue-50 dark:bg-blue-950/30', border: 'border-blue-200 dark:border-blue-800', text: 'text-blue-800 dark:text-blue-200' },
    2: { bg: 'bg-purple-50 dark:bg-purple-950/30', border: 'border-purple-200 dark:border-purple-800', text: 'text-purple-800 dark:text-purple-200' },
  }
  const c = colorMap[s] || colorMap[0]

  if (Number(item.status) === 2) {
    return [c.bg, 'border border-dashed opacity-50', 'text-slate-400 dark:text-slate-500']
  }
  return [c.bg, `border ${c.border}`, c.text]
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
  // Filter is reactive via computed, no extra action needed
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
</style>
