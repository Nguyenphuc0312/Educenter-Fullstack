<template>
  <div class="space-y-6">
    <PageHeader
      title="Lịch dạy tuần"
      subtitle="Lịch học dạng tuần theo lớp, ca học và phòng học của giảng viên."
    >
      <template #actions>
        <a-button :loading="loading" @click="loadData">Làm mới</a-button>
      </template>
    </PageHeader>

    <section class="teacher-filter-bar">
      <a-select v-model:value="selectedClassId" allow-clear placeholder="Lọc theo lớp" class="min-w-[260px]">
        <a-select-option v-for="cls in classes" :key="cls.id" :value="cls.id">
          {{ cls.className }}
        </a-select-option>
      </a-select>
      <a-select v-model:value="selectedShift" allow-clear placeholder="Lọc theo ca" class="min-w-[160px]">
        <a-select-option :value="0">Sáng</a-select-option>
        <a-select-option :value="1">Chiều</a-select-option>
        <a-select-option :value="2">Tối</a-select-option>
      </a-select>
      <a-select v-model:value="selectedRoom" allow-clear placeholder="Lọc theo phòng" class="min-w-[160px]">
        <a-select-option v-for="room in rooms" :key="room" :value="room">{{ room }}</a-select-option>
      </a-select>
      <a-button @click="resetFilters">Đặt lại</a-button>
    </section>

    <section class="teacher-calendar">
      <div class="teacher-calendar-head" :style="{ gridTemplateColumns: '96px repeat(7, minmax(130px, 1fr))' }">
        <div class="teacher-calendar-head-cell">Ca học</div>
        <div v-for="day in days" :key="day.value" class="teacher-calendar-head-cell">
          <p>{{ day.label }}</p>
          <span>{{ day.hint }}</span>
        </div>
      </div>

      <div v-if="loading" class="teacher-calendar-loading"><a-spin /></div>

      <template v-else>
        <div
          v-for="shift in shifts"
          :key="shift.value"
          class="teacher-calendar-row"
          :style="{ gridTemplateColumns: '96px repeat(7, minmax(130px, 1fr))' }"
        >
          <div class="teacher-shift-cell">
            <strong :style="{ color: shift.color }">{{ shift.label }}</strong>
            <span>{{ shift.time }}</span>
          </div>
          <div v-for="day in days" :key="`${day.value}-${shift.value}`" class="teacher-day-cell">
            <div v-if="cellItems(day.value, shift.value).length" class="space-y-2">
              <button
                v-for="item in cellItems(day.value, shift.value)"
                :key="item.id"
                type="button"
                class="teacher-calendar-card"
                @click="selectedSchedule = item"
              >
                <div class="flex items-start justify-between gap-2">
                  <div class="min-w-0 text-left">
                    <p class="font-bold truncate">{{ item.classNameSnapshot }}</p>
                    <span>{{ item.topic || `Buổi ${item.sessionNumber}` }}</span>
                  </div>
                  <a-tag :color="statusColor(item.status)">{{ statusLabel(item.status) }}</a-tag>
                </div>
                <div class="teacher-card-meta">
                  <span>{{ formatTime(item.startTime) }} - {{ formatTime(item.endTime) }}</span>
                  <span>{{ item.room }}</span>
                </div>
              </button>
            </div>
          </div>
        </div>
      </template>
    </section>

    <a-drawer v-model:open="drawerOpen" title="Chi tiết lịch học" width="420">
      <div v-if="selectedSchedule" class="space-y-4">
        <div class="teacher-detail-hero">
          <p>{{ dayLabel(selectedSchedule.dayOfWeek) }} · {{ shiftLabel(selectedSchedule.studyShift) }}</p>
          <h2>{{ selectedSchedule.classNameSnapshot }}</h2>
          <span>{{ formatTime(selectedSchedule.startTime) }} - {{ formatTime(selectedSchedule.endTime) }}</span>
        </div>
        <div class="grid grid-cols-2 gap-3">
          <div class="teacher-info-box"><span>Phòng</span><strong>{{ selectedSchedule.room }}</strong></div>
          <div class="teacher-info-box"><span>Buổi</span><strong>{{ selectedSchedule.sessionNumber }}</strong></div>
          <div class="teacher-info-box col-span-2"><span>Chủ đề</span><strong>{{ selectedSchedule.topic || '-' }}</strong></div>
          <div class="teacher-info-box col-span-2"><span>Trạng thái</span><strong>{{ statusLabel(selectedSchedule.status) }}</strong></div>
        </div>
        <div class="flex gap-2">
          <router-link class="flex-1" :to="`/teacher/classes/${selectedSchedule.classId}/attendance`">
            <a-button type="primary" block>Điểm danh lớp này</a-button>
          </router-link>
          <router-link class="flex-1" :to="`/teacher/classes/${selectedSchedule.classId}`">
            <a-button block>Xem lớp</a-button>
          </router-link>
        </div>
      </div>
    </a-drawer>
  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import { classApi } from '@/api/classApi'
import { scheduleApi } from '@/api/scheduleApi'
import { useAuthStore } from '@/stores/auth'
import { formatTime } from '@/lib/formatters'

const auth = useAuthStore()
const loading = ref(false)
const classes = ref([])
const schedules = ref([])
const selectedClassId = ref(undefined)
const selectedShift = ref(undefined)
const selectedRoom = ref(undefined)
const selectedSchedule = ref(null)

const drawerOpen = computed({
  get: () => !!selectedSchedule.value,
  set: (value) => { if (!value) selectedSchedule.value = null }
})

const days = [
  { value: 1, label: 'Thứ 2', hint: 'Monday' },
  { value: 2, label: 'Thứ 3', hint: 'Tuesday' },
  { value: 3, label: 'Thứ 4', hint: 'Wednesday' },
  { value: 4, label: 'Thứ 5', hint: 'Thursday' },
  { value: 5, label: 'Thứ 6', hint: 'Friday' },
  { value: 6, label: 'Thứ 7', hint: 'Saturday' },
  { value: 0, label: 'Chủ nhật', hint: 'Sunday' }
]
const shifts = [
  { value: 0, label: 'Sáng', time: '08:00 - 12:00', color: '#f59e0b' },
  { value: 1, label: 'Chiều', time: '13:30 - 17:30', color: '#2563eb' },
  { value: 2, label: 'Tối', time: '18:00 - 22:00', color: '#7c3aed' }
]

const rooms = computed(() => [...new Set(schedules.value.map(x => x.room).filter(Boolean))])

const filteredSchedules = computed(() => schedules.value.filter(item => {
  const matchClass = !selectedClassId.value || item.classId === selectedClassId.value
  const matchShift = selectedShift.value === undefined || shiftValue(item.studyShift) === selectedShift.value
  const matchRoom = !selectedRoom.value || item.room === selectedRoom.value
  return matchClass && matchShift && matchRoom
}))

function cellItems(day, shift) {
  return filteredSchedules.value
    .filter(item => dayValue(item.dayOfWeek) === day && shiftValue(item.studyShift) === shift)
    .sort((a, b) => String(a.startTime).localeCompare(String(b.startTime)))
}

function dayValue(value) {
  const map = { Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3, Thursday: 4, Friday: 5, Saturday: 6 }
  return typeof value === 'number' ? value : map[value] ?? Number(value) ?? 0
}
function shiftValue(value) {
  const map = { Morning: 0, Afternoon: 1, Evening: 2 }
  return typeof value === 'number' ? value : map[value] ?? Number(value) ?? 0
}
function dayLabel(value) { return days.find(d => d.value === dayValue(value))?.label || value }
function shiftLabel(value) { return shifts.find(s => s.value === shiftValue(value))?.label || value }
function statusLabel(value) {
  const map = { Scheduled: 'Đã lên lịch', Completed: 'Hoàn thành', Cancelled: 'Đã hủy', 0: 'Đã lên lịch', 1: 'Hoàn thành', 2: 'Đã hủy' }
  return map[value] || value
}
function statusColor(value) {
  const map = { Scheduled: 'blue', Completed: 'green', Cancelled: 'red', 0: 'blue', 1: 'green', 2: 'red' }
  return map[value] || 'blue'
}
function resetFilters() {
  selectedClassId.value = undefined
  selectedShift.value = undefined
  selectedRoom.value = undefined
}
async function loadData() {
  if (!auth.user?.referenceId) return
  loading.value = true
  try {
    const [classData, scheduleData] = await Promise.all([
      classApi.getByTeacher(auth.user.referenceId),
      scheduleApi.getByTeacher(auth.user.referenceId)
    ])
    classes.value = classData || []
    schedules.value = scheduleData || []
  } finally {
    loading.value = false
  }
}

watch(selectedClassId, () => { selectedSchedule.value = null })
onMounted(loadData)
</script>

<style scoped>
.teacher-filter-bar { display: flex; flex-wrap: wrap; gap: 12px; padding: 16px; background: var(--admin-surface); border: 1px solid var(--admin-border); border-radius: 18px; box-shadow: var(--admin-shadow-sm); }
.teacher-calendar { background: var(--admin-surface); border: 1px solid var(--admin-border); border-radius: 18px; overflow: hidden; box-shadow: var(--admin-shadow-sm); min-width: 980px; }
.teacher-calendar-head,.teacher-calendar-row { display: grid; }
.teacher-calendar-head { background: var(--admin-surface-2); border-bottom: 1px solid var(--admin-border); }
.teacher-calendar-head-cell { min-height: 64px; padding: 12px; display: grid; place-items: center; text-align: center; border-right: 1px solid var(--admin-border); color: var(--admin-text); }
.teacher-calendar-head-cell p { font-weight: 800; font-size: 13px; }
.teacher-calendar-head-cell span { font-size: 11px; color: var(--admin-text-muted); }
.teacher-calendar-row { border-bottom: 1px solid var(--admin-border); }
.teacher-shift-cell { padding: 14px 10px; border-right: 1px solid var(--admin-border); display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 3px; }
.teacher-shift-cell span { font-size: 11px; color: var(--admin-text-muted); text-align: center; }
.teacher-day-cell { min-height: 150px; padding: 8px; border-right: 1px solid var(--admin-border); background: color-mix(in srgb, var(--admin-surface) 92%, transparent); }
.teacher-calendar-loading { min-height: 360px; display: grid; place-items: center; grid-column: 1 / -1; }
.teacher-calendar-card { width: 100%; border: 1px solid rgba(37,99,235,.18); border-radius: 14px; padding: 10px; background: linear-gradient(135deg, rgba(37,99,235,.08), rgba(20,184,166,.08)); color: var(--admin-text); transition: transform .2s ease, box-shadow .2s ease; }
.teacher-calendar-card:hover { transform: translateY(-1px); box-shadow: var(--admin-shadow-sm); }
.teacher-calendar-card span { font-size: 11px; color: var(--admin-text-muted); }
.teacher-card-meta { display: flex; justify-content: space-between; gap: 8px; margin-top: 10px; font-size: 11px; color: var(--admin-text-muted); }
.teacher-detail-hero { padding: 16px; border-radius: 16px; background: linear-gradient(135deg, rgba(37,99,235,.12), rgba(20,184,166,.12)); }
.teacher-detail-hero p,.teacher-detail-hero span { color: var(--admin-text-muted); font-size: 12px; }
.teacher-detail-hero h2 { color: var(--admin-text); font-weight: 800; font-size: 18px; margin: 4px 0; }
.teacher-info-box { background: var(--admin-surface-2); border: 1px solid var(--admin-border); border-radius: 14px; padding: 12px; }
.teacher-info-box span { display: block; color: var(--admin-text-muted); font-size: 11px; margin-bottom: 4px; }
.teacher-info-box strong { color: var(--admin-text); font-size: 13px; }
</style>
