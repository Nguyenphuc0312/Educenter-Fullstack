<template>
  <div class="space-y-6 printable-area">
    <PageHeader 
      title="Lịch giảng dạy" 
      subtitle="Quản lý lịch dạy theo tuần, theo dõi ca học, phòng học và xuất dữ liệu."
    >
      <template #actions>
        <div class="flex items-center bg-white border border-slate-200 rounded-lg shadow-sm overflow-hidden mr-2 hide-on-print">
          <button @click="prevWeek" class="px-3 py-2 text-slate-500 hover:bg-slate-50 hover:text-blue-600 transition-colors border-r border-slate-200" title="Tuần trước">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" /></svg>
          </button>
          <button @click="goToToday" class="px-4 py-2 text-sm font-bold text-slate-700 hover:bg-slate-50 transition-colors">
            Hôm nay
          </button>
          <button @click="nextWeek" class="px-3 py-2 text-slate-500 hover:bg-slate-50 hover:text-blue-600 transition-colors border-l border-slate-200" title="Tuần sau">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" /></svg>
          </button>
        </div>

        <button 
          class="px-4 py-2 bg-emerald-50 border border-emerald-200 text-emerald-700 hover:bg-emerald-100 font-bold rounded-lg transition-colors shadow-sm flex items-center gap-2 hide-on-print active:scale-95" 
          @click="exportSchedule"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" /></svg>
          Xuất PDF / In
        </button>

        <button 
          class="px-4 py-2 bg-blue-600 text-white font-bold rounded-lg shadow-sm hover:bg-blue-700 transition-colors flex items-center gap-2 disabled:opacity-50 hide-on-print active:scale-95" 
          @click="loadData"
          :disabled="loading"
        >
          <LoadingSpinner v-if="loading" size="sm" class="!text-white" />
          <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>
          Làm mới
        </button>
      </template>
    </PageHeader>

    <section class="flex flex-col xl:flex-row xl:items-center justify-between gap-4 hide-on-print">
      
      <div class="relative flex items-center bg-blue-50 border border-blue-200 rounded-xl hover:bg-blue-100 transition-colors shadow-sm w-max group px-1">
        <svg class="w-5 h-5 text-blue-500 ml-3 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
        <a-date-picker 
          v-model:value="currentDate" 
          picker="week" 
          :format="customWeekFormat"
          :allow-clear="false"
          class="!bg-transparent !border-none !shadow-none font-bold text-sm text-blue-800 cursor-pointer custom-week-picker w-[250px]"
        />
      </div>

      <div class="bg-slate-50 p-1.5 rounded-xl border border-slate-200 flex flex-wrap items-center gap-2 shadow-sm">
        <div class="flex items-center pl-2 pr-1 text-slate-400">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z"/></svg>
        </div>

        <a-select v-model:value="selectedClassId" allow-clear placeholder="Lọc theo Lớp học" class="min-w-[180px] !border-none custom-filter-select" :bordered="false">
          <a-select-option v-for="cls in classes" :key="cls.id" :value="cls.id">{{ cls.className }}</a-select-option>
        </a-select>

        <div class="w-px h-4 bg-slate-300 hidden sm:block"></div>

        <a-select v-model:value="selectedShift" allow-clear placeholder="Lọc theo Ca" class="min-w-[120px] !border-none custom-filter-select" :bordered="false">
          <a-select-option :value="0">Ca Sáng</a-select-option>
          <a-select-option :value="1">Ca Chiều</a-select-option>
          <a-select-option :value="2">Ca Tối</a-select-option>
        </a-select>

        <div class="w-px h-4 bg-slate-300 hidden md:block"></div>

        <a-select v-model:value="selectedRoom" allow-clear placeholder="Phòng" class="min-w-[100px] !border-none custom-filter-select" :bordered="false">
          <a-select-option v-for="room in rooms" :key="room" :value="room">P.{{ room }}</a-select-option>
        </a-select>

        <div class="w-px h-4 bg-slate-300 hidden md:block"></div>

        <a-tooltip title="Bật lên để xem cả những lớp đã kết thúc hoặc chưa khai giảng trong tuần này">
          <div class="flex items-center gap-2 px-2 border-l border-slate-200 cursor-pointer" @click="showExpiredClasses = !showExpiredClasses">
            <span class="text-[11px] font-bold text-slate-500 uppercase">Hiện tất cả</span>
            <a-switch v-model:checked="showExpiredClasses" size="small" />
          </div>
        </a-tooltip>

        <div class="ml-auto">
          <button v-if="selectedClassId || selectedShift !== undefined || selectedRoom" @click="resetFilters" class="px-3 py-1.5 text-xs font-bold text-rose-600 hover:bg-rose-50 rounded-lg transition-colors flex items-center gap-1">
            Bỏ lọc
          </button>
        </div>
      </div>
    </section>

    <section class="bg-white border border-slate-200 rounded-2xl shadow-sm overflow-hidden flex flex-col calendar-container print:border-black print:rounded-none">
      <div v-if="loading" class="py-32 flex justify-center bg-slate-50/50">
        <LoadingSpinner size="lg" />
      </div>

      <div v-else class="overflow-x-auto custom-scrollbar">
        <div class="min-w-[1100px] flex flex-col">
          
          <div class="grid grid-cols-[100px_repeat(7,minmax(0,1fr))] border-b border-slate-200 bg-slate-50 print:bg-white print:border-black">
            <div class="flex items-center justify-center border-r border-slate-200 print:border-black p-3">
              <span class="text-xs font-bold text-slate-400 print:text-black uppercase tracking-wider">Ca học</span>
            </div>
            
            <div 
              v-for="day in dynamicDays" 
              :key="day.dayValue" 
              class="flex flex-col items-center justify-center p-3 border-r border-slate-200 print:border-black last:border-r-0 transition-colors relative"
              :class="day.isToday ? 'bg-blue-50 print:bg-white' : ''"
            >
              <div v-if="day.isToday" class="absolute top-0 left-0 right-0 h-1 bg-blue-500 hide-on-print"></div>
              
              <span class="text-sm font-black mb-0.5 print:text-black" :class="day.isToday ? 'text-blue-700' : 'text-slate-700'">
                {{ day.label }}
              </span>
              <span class="text-[11px] font-bold print:text-black" :class="day.isToday ? 'text-blue-500' : 'text-slate-500'">
                {{ day.dateStr }}
              </span>
            </div>
          </div>

          <div class="flex flex-col">
            <div 
              v-for="shift in shifts" 
              :key="shift.value" 
              class="grid grid-cols-[100px_repeat(7,minmax(0,1fr))] border-b border-slate-200 print:border-black last:border-b-0"
            >
              <div class="flex flex-col items-center justify-center p-3 border-r border-slate-200 print:border-black bg-slate-50/30 print:bg-white">
                <span class="text-sm font-black print:text-black" :style="{ color: shift.color }">{{ shift.label }}</span>
                <span class="text-[10px] font-bold text-slate-400 print:text-black mt-1 text-center">{{ shift.time }}</span>
              </div>

              <div 
                v-for="day in dynamicDays" 
                :key="`${day.dayValue}-${shift.value}`" 
                class="p-2.5 border-r border-slate-200 print:border-black last:border-r-0 min-h-[140px] transition-colors"
                :class="day.isToday ? 'bg-blue-50/20 print:bg-white' : 'bg-white'"
              >
                <div v-if="cellItems(day.dayValue, shift.value).length" class="space-y-2.5 h-full">
                  <button
                    v-for="item in cellItems(day.dayValue, shift.value)"
                    :key="item.id"
                    type="button"
                    class="w-full text-left bg-white border rounded-xl print:rounded-none p-3 shadow-sm print:shadow-none hover:shadow-md hover:-translate-y-0.5 transition-all duration-200 relative overflow-hidden group schedule-card"
                    :class="selectedSchedule?.id === item.id ? 'ring-2 ring-blue-400 border-blue-400' : 'border-slate-200 hover:border-blue-300 print:border-black'"
                    @click="openScheduleDetail(item, day.fullDate)"
                  >
                    <div class="absolute left-0 top-0 bottom-0 w-1.5 transition-colors hide-on-print" :style="{ backgroundColor: shift.color }"></div>
                    
                    <div class="pl-2 print:pl-0">
                      <div class="flex items-start justify-between gap-1 mb-1.5">
                        <h4 class="font-bold text-[13px] text-slate-800 print:text-black leading-snug line-clamp-2 group-hover:text-blue-700 transition-colors" :title="item.classNameSnapshot">
                          {{ item.classNameSnapshot }}
                        </h4>
                      </div>
                      
                      <p class="text-[11px] font-medium text-slate-500 print:text-black mb-2 truncate" :title="item.topic">
                        {{ item.topic || `Buổi ${item.sessionNumber}` }}
                      </p>
                      
                      <div class="flex items-center justify-between mt-3 pt-2 border-t border-slate-100 print:border-black">
                        <div class="flex items-center gap-1 text-[10px] font-bold text-slate-600 print:text-black">
                          <svg class="w-3.5 h-3.5 text-slate-400 hide-on-print" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                          {{ formatTime(item.startTime) }}
                        </div>
                        <div class="flex items-center gap-1 text-[10px] font-black text-slate-600 print:text-black bg-slate-100 print:bg-white px-1.5 py-0.5 rounded border border-slate-200 print:border-black">
                          P.{{ item.room || 'NA' }}
                        </div>
                      </div>
                    </div>
                  </button>
                </div>
                <div v-else class="w-full h-full min-h-[120px] rounded-xl border border-dashed border-slate-200 print:border-none bg-slate-50/50 print:bg-white flex items-center justify-center opacity-0 hover:opacity-100 transition-opacity">
                  <span class="text-[10px] font-medium text-slate-400 hide-on-print">Trống</span>
                </div>
              </div>

            </div>
          </div>
        </div>
      </div>
    </section>

    <a-drawer v-model:open="drawerOpen" :title="null" placement="right" width="420" :closable="false" :padding="0" class="hide-on-print">
      <div v-if="selectedSchedule" class="flex flex-col h-full bg-slate-50">
        <div class="bg-gradient-to-br from-blue-600 to-indigo-700 p-6 text-white relative">
          <button @click="drawerOpen = false" class="absolute top-4 right-4 p-1.5 bg-white/10 hover:bg-white/20 rounded-full transition-colors">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>
          </button>
          
          <div class="flex items-center gap-2 text-blue-100 text-[11px] font-bold uppercase tracking-wider mb-2">
            <span>{{ dayLabel(selectedSchedule.dayOfWeek) }} ({{ selectedScheduleDateStr }})</span>
            <span class="w-1 h-1 rounded-full bg-blue-300"></span>
            <span>{{ shiftLabel(selectedSchedule.studyShift) }}</span>
          </div>
          <h2 class="text-xl font-black mb-1 leading-tight">{{ selectedSchedule.classNameSnapshot }}</h2>
          <p class="text-blue-100 text-sm font-medium flex items-center gap-1.5">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
            {{ formatTime(selectedSchedule.startTime) }} - {{ formatTime(selectedSchedule.endTime) }}
          </p>
        </div>

        <div class="p-6 flex-1 space-y-4">
          
          <div class="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex items-center justify-between">
            <div>
              <span class="text-[11px] font-bold text-slate-400 uppercase tracking-wider mb-1 block">Thời gian đào tạo (Khai giảng - Bế giảng)</span>
              <strong class="text-sm font-black text-slate-700">
                {{ formatDate(selectedScheduleClass?.startDate) }} <span class="text-slate-400 font-normal mx-1">→</span> {{ formatDate(selectedScheduleClass?.endDate) }}
              </strong>
            </div>
            <svg class="w-6 h-6 text-indigo-100" fill="currentColor" viewBox="0 0 24 24"><path d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex flex-col justify-center">
              <span class="text-[11px] font-bold text-slate-400 uppercase tracking-wider mb-1">Phòng học</span>
              <strong class="text-lg font-black text-slate-800">P.{{ selectedSchedule.room || 'Chưa xếp' }}</strong>
            </div>
            <div class="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex flex-col justify-center">
              <span class="text-[11px] font-bold text-slate-400 uppercase tracking-wider mb-1">Thứ tự buổi</span>
              <strong class="text-lg font-black text-blue-600">Buổi {{ selectedSchedule.sessionNumber }}</strong>
            </div>
          </div>

          <div class="bg-white p-4 rounded-xl border border-slate-200 shadow-sm">
            <span class="text-[11px] font-bold text-slate-400 uppercase tracking-wider mb-1 block">Chủ đề / Nội dung</span>
            <strong class="text-sm font-medium text-slate-700 leading-relaxed">{{ selectedSchedule.topic || 'Giáo án chưa cập nhật chủ đề cụ thể.' }}</strong>
          </div>

          <div class="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex items-center justify-between">
            <span class="text-[11px] font-bold text-slate-400 uppercase tracking-wider">Trạng thái</span>
            <span :class="['px-2.5 py-1 text-[11px] font-black uppercase tracking-wider rounded-md border', statusColorClass(selectedSchedule.status)]">
              {{ statusLabel(selectedSchedule.status) }}
            </span>
          </div>
        </div>

        <div class="p-6 bg-white border-t border-slate-200 flex flex-col gap-3">
          <button @click="goToAttendance(selectedSchedule.classId)" class="w-full py-3 bg-emerald-50 text-emerald-600 border border-emerald-200 hover:bg-emerald-100 font-bold rounded-xl text-sm transition-colors flex justify-center items-center gap-2 active:scale-95">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" /></svg>
            Tạo phiên Điểm danh
          </button>
          
          <button @click="goToClassDetail(selectedSchedule.classId)" class="w-full py-3 bg-blue-600 text-white hover:bg-blue-700 font-bold rounded-xl text-sm transition-colors flex justify-center items-center gap-2 shadow-sm active:scale-95">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
            Xem chi tiết Lớp
          </button>
        </div>
      </div>
    </a-drawer>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { message } from 'ant-design-vue'
import dayjs from 'dayjs'
import isoWeek from 'dayjs/plugin/isoWeek'
import isSameOrBefore from 'dayjs/plugin/isSameOrBefore'
import isSameOrAfter from 'dayjs/plugin/isSameOrAfter'

import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { classApi } from '@/api/classApi'
import { scheduleApi } from '@/api/scheduleApi'
import { useAuthStore } from '@/stores/auth'

// Setup DayJS Plugins
dayjs.extend(isoWeek)
dayjs.extend(isSameOrBefore)
dayjs.extend(isSameOrAfter)

const router = useRouter()
const auth = useAuthStore()

const loading = ref(false)
const classes = ref([])
const schedules = ref([])

// Bộ lọc
const selectedClassId = ref(undefined)
const selectedShift = ref(undefined)
const selectedRoom = ref(undefined)
const showExpiredClasses = ref(false) // Toggle: Cho phép hiện lịch của những lớp không nằm trong khoảng Start/End Date của tuần đang chọn

// Quản lý Drawer
const selectedSchedule = ref(null)
const selectedScheduleDateStr = ref('')
const drawerOpen = computed({
  get: () => !!selectedSchedule.value,
  set: (value) => { if (!value) selectedSchedule.value = null }
})

// Tính toán Lớp học của Schedule đang chọn (Để hiển thị ngày Khai giảng/Bế giảng)
const selectedScheduleClass = computed(() => {
  if (!selectedSchedule.value) return null
  return classes.value.find(c => c.id === selectedSchedule.value.classId)
})

// ================= LOGIC THỜI GIAN (TIME MACHINE) =================
const currentDate = ref(dayjs())

const startOfWeek = computed(() => currentDate.value.startOf('isoWeek'))
const endOfWeek = computed(() => currentDate.value.endOf('isoWeek'))

const customWeekFormat = (value) => {
  return `${value.startOf('isoWeek').format('DD/MM/YYYY')} - ${value.endOf('isoWeek').format('DD/MM/YYYY')}`
}

const weekDisplayRange = computed(() => {
  return `${startOfWeek.value.format('DD/MM/YYYY')} - ${endOfWeek.value.format('DD/MM/YYYY')}`
})

const dynamicDays = computed(() => {
  // Trả về mảng 7 ngày tính từ thứ 2
  return Array.from({ length: 7 }).map((_, i) => {
    const d = startOfWeek.value.add(i, 'day')
    return {
      dayValue: d.day(), // 0 (Sun) to 6 (Sat)
      dateStr: d.format('DD/MM/YYYY'),
      fullDate: d,
      isToday: d.isSame(dayjs(), 'day'),
      label: ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'][d.day()],
      hint: ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT'][d.day()]
    }
  })
})

function prevWeek() { currentDate.value = currentDate.value.subtract(1, 'week') }
function nextWeek() { currentDate.value = currentDate.value.add(1, 'week') }
function goToToday() { currentDate.value = dayjs() }

// ================= DỮ LIỆU TĨNH =================
const shifts = [
  { value: 0, label: 'Ca Sáng', time: '08:00 - 12:00', color: '#10b981' }, 
  { value: 1, label: 'Ca Chiều', time: '13:30 - 17:30', color: '#3b82f6' }, 
  { value: 2, label: 'Ca Tối', time: '18:00 - 22:00', color: '#8b5cf6' }    
]

const rooms = computed(() => [...new Set(schedules.value.map(x => x.room).filter(Boolean))])

// ================= LỌC LỊCH TRÌNH THÔNG MINH =================
const filteredSchedules = computed(() => {
  return schedules.value.filter(item => {
    const cls = classes.value.find(c => c.id === item.classId)
    
    // 1. Lọc theo Khoảng thời gian Lớp học (Bảo vệ dữ liệu không bị mất)
    if (!showExpiredClasses.value) { 
      if (cls) {
        // Lọc những lớp đã Hủy hoặc Hoàn thành (không hiện trên lịch hiện tại)
        if (cls.status === 'Completed' || cls.status === 'Cancelled' || cls.status === 3 || cls.status === 4) {
          return false
        }

        // Lọc theo khoảng ngày (Nếu lớp có cấu hình ngày Khai giảng/Bế giảng)
        if (cls.startDate && cls.endDate) {
          const classStart = dayjs(cls.startDate).startOf('day')
          const classEnd = dayjs(cls.endDate).endOf('day')
          const weekStart = startOfWeek.value.startOf('day')
          const weekEnd = endOfWeek.value.endOf('day')
          
          if (classStart.isValid() && classEnd.isValid()) {
            // Ẩn nếu: Lớp kết thúc trước tuần hiện tại HOẶC Lớp khai giảng sau tuần hiện tại
            if (classEnd.isBefore(weekStart) || classStart.isAfter(weekEnd)) {
              return false
            }
          }
        }
      }
    }

    // 2. Lọc theo Bộ lọc Dropdown của Admin
    const matchClass = !selectedClassId.value || item.classId === selectedClassId.value
    const matchShift = selectedShift.value === undefined || shiftValue(item.studyShift) === selectedShift.value
    const matchRoom = !selectedRoom.value || item.room === selectedRoom.value
    
    return matchClass && matchShift && matchRoom
  })
})

function cellItems(dayVal, shiftVal) {
  return filteredSchedules.value
    .filter(item => dayValue(item.dayOfWeek) === dayVal && shiftValue(item.studyShift) === shiftVal)
    .sort((a, b) => String(a.startTime).localeCompare(String(b.startTime)))
}

function openScheduleDetail(item, dateObj) {
  selectedSchedule.value = item
  selectedScheduleDateStr.value = dateObj.format('DD/MM/YYYY')
}

// ================= ĐIỀU HƯỚNG BÊN NGOÀI (DEEP LINKING FIX) =================
function goToAttendance(classId) {
  drawerOpen.value = false
  router.push({ path: '/teacher/attendance', query: { classId } })
}

function goToClassDetail(classId) {
  drawerOpen.value = false
  router.push(`/teacher/classes/${classId}`)
}

// ================= UI HELPERS =================
function isToday(dayVal) {
  // Chỉ hightlight ngày hôm nay nếu tuần đang chọn là tuần hiện tại
  if (!currentDate.value.isSame(dayjs(), 'week')) return false;
  return dayjs().day() === dayVal
}

function dayValue(value) {
  const map = { Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3, Thursday: 4, Friday: 5, Saturday: 6 }
  return typeof value === 'number' ? value : map[value] ?? Number(value) ?? 0
}

function shiftValue(value) {
  const map = { Morning: 0, Afternoon: 1, Evening: 2 }
  return typeof value === 'number' ? value : map[value] ?? Number(value) ?? 0
}

function dayLabel(value) { return dynamicDays.value.find(d => d.dayValue === dayValue(value))?.label || value }
function shiftLabel(value) { return shifts.find(s => s.value === shiftValue(value))?.label || value }

function statusLabel(value) {
  const map = { Scheduled: 'Đã lên lịch', Completed: 'Hoàn thành', Cancelled: 'Đã hủy', 0: 'Đã lên lịch', 1: 'Hoàn thành', 2: 'Đã hủy' }
  return map[value] || value
}

function statusColorClass(value) {
  const map = { 
    Scheduled: 'bg-blue-50 text-blue-700 border-blue-200', 
    Completed: 'bg-emerald-50 text-emerald-700 border-emerald-200', 
    Cancelled: 'bg-rose-50 text-rose-700 border-rose-200', 
    0: 'bg-blue-50 text-blue-700 border-blue-200', 
    1: 'bg-emerald-50 text-emerald-700 border-emerald-200', 
    2: 'bg-rose-50 text-rose-700 border-rose-200' 
  }
  return map[value] || 'bg-slate-50 text-slate-600 border-slate-200'
}

function formatTime(t) {
  if (!t) return '—'
  return String(t).substring(0, 5) // "08:00:00" -> "08:00"
}

function formatDate(d) {
  if (!d) return 'Chưa xác định'
  const date = dayjs(d)
  if (!date.isValid()) return d
  return date.format('DD/MM/YYYY')
}

function resetFilters() {
  selectedClassId.value = undefined
  selectedShift.value = undefined
  selectedRoom.value = undefined
}

// ================= TÍNH NĂNG EXPORT =================
function exportSchedule() {
  message.loading({ content: 'Đang chuẩn bị file In/PDF...', key: 'print' })
  setTimeout(() => {
    message.destroy('print')
    window.print() 
  }, 800)
}

// ================= DATA FETCH =================
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
  } catch(e) {
    message.error('Lỗi khi tải dữ liệu lịch học.')
  } finally {
    loading.value = false
  }
}

onMounted(loadData)
</script>

<style scoped>
/* Tuỳ chỉnh Input Date Picker của Ant Design */
:deep(.custom-week-picker .ant-picker-input > input) {
  font-weight: 700;
  color: #1e40af; /* blue-800 */
  text-align: center;
  cursor: pointer;
}

/* Ẩn viền focus mặc định của Ant Design Select trong bộ lọc */
:deep(.custom-filter-select .ant-select-selector) {
  border: none !important;
  box-shadow: none !important;
  background-color: transparent !important;
  font-weight: 600;
  color: #475569;
}
:deep(.custom-filter-select .ant-select-arrow) {
  color: #94a3b8;
}

/* Scrollbar mượt mà */
.custom-scrollbar::-webkit-scrollbar { height: 8px; width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #94a3b8; }

/* IN ẤN (EXPORT PDF) - Tối ưu giao diện giấy A4 */
@media print {
  body {
    background-color: white !important;
  }
  .hide-on-print {
    display: none !important;
  }
  .printable-area {
    padding: 0 !important;
    margin: 0 !important;
  }
  .calendar-container {
    border: 2px solid #000 !important;
    border-radius: 0 !important;
    box-shadow: none !important;
  }
  .schedule-card {
    border: 1px solid #94a3b8 !important;
    box-shadow: none !important;
    page-break-inside: avoid;
    break-inside: avoid;
  }
}
</style>