<template>
  <div class="space-y-6">
    <PageHeader
      title="Lịch học theo tuần"
      subtitle="Theo dõi và quản lý thời khóa biểu các lớp học đang tham gia."
    />

    <div class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden flex flex-col">
      <div class="p-4 sm:p-6">
        
        <div class="flex flex-col lg:flex-row justify-between items-start lg:items-center mb-6 gap-4">
          
          <div class="inline-flex w-full sm:w-auto items-center bg-white border border-slate-200 rounded-lg shadow-sm hover:border-blue-400 hover:shadow-md transition-all duration-300">
            <button 
              @click="changeWeek(-1)" 
              class="flex-shrink-0 px-3.5 py-2 text-slate-500 hover:text-blue-600 hover:bg-blue-50 active:bg-blue-100 transition-colors border-r border-slate-200 rounded-l-lg"
              title="Tuần trước"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" /></svg>
            </button>
            
            <div
              class="relative flex flex-1 sm:flex-none items-center justify-center min-w-0 sm:w-[300px]"
            >
              <a-date-picker 
                v-model:value="currentDate" 
                v-model:open="weekPickerOpen"
                :format="formatWeekRange"
                @change="weekPickerOpen = false"
                picker="week" 
                placement="bottomLeft"
                class="schedule-week-picker w-full" 
                :allowClear="false"
                :inputReadOnly="true"
              />
            </div>

            <button 
              @click="changeWeek(1)" 
              class="flex-shrink-0 px-3.5 py-2 text-slate-500 hover:text-blue-600 hover:bg-blue-50 active:bg-blue-100 transition-colors border-l border-slate-200 rounded-r-lg"
              title="Tuần tiếp theo"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" /></svg>
            </button>
          </div>

          <div class="flex flex-wrap items-center gap-3">
            <button 
              @click="currentDate = dayjs()" 
              class="px-4 py-2 border border-slate-200 rounded-lg shadow-sm text-sm font-bold text-slate-600 hover:text-blue-600 hover:bg-blue-50 hover:border-blue-300 active:scale-95 transition-all duration-200"
            >
              Hôm nay
            </button>

            <a-dropdown :trigger="['click']">
              <button class="px-4 py-2 border border-slate-200 rounded-lg shadow-sm flex items-center gap-2 text-sm font-bold text-slate-700 hover:text-blue-600 hover:bg-slate-50 hover:border-blue-300 active:scale-95 transition-all duration-200">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z" /></svg>
                {{ filterLabel }}
                <svg class="w-3 h-3 ml-1 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
              </button>
              <template #overlay>
                <a-menu @click="handleFilterChange" class="!rounded-lg !shadow-lg !p-2 !w-52">
                  <div class="px-3 py-2 text-xs font-bold text-slate-400 uppercase tracking-wider">Hiển thị theo ca</div>
                  <a-menu-item key="all">
                    <span :class="['font-medium block py-1', activeFilter === 'all' ? 'text-blue-600 font-bold' : 'text-slate-700']">Tất cả môn học</span>
                  </a-menu-item>
                  <a-menu-item key="Morning">
                    <span :class="['font-medium block py-1', activeFilter === 'Morning' ? 'text-emerald-700 font-bold' : 'text-emerald-600']">Chỉ hiện Ca Sáng</span>
                  </a-menu-item>
                  <a-menu-item key="Afternoon">
                    <span :class="['font-medium block py-1', activeFilter === 'Afternoon' ? 'text-amber-700 font-bold' : 'text-amber-600']">Chỉ hiện Ca Chiều</span>
                  </a-menu-item>
                  <a-menu-item key="Evening">
                    <span :class="['font-medium block py-1', activeFilter === 'Evening' ? 'text-slate-900 font-bold' : 'text-slate-600']">Chỉ hiện Ca Tối</span>
                  </a-menu-item>
                </a-menu>
              </template>
            </a-dropdown>

            <button 
              @click="downloadSchedule" 
              class="px-4 py-2 bg-blue-600 text-white rounded-lg shadow-sm flex items-center gap-2 text-sm font-bold hover:bg-blue-700 hover:shadow-md active:scale-95 transition-all duration-200"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" /></svg>
              Xuất lịch
            </button>
          </div>
        </div>

        <LoadingSpinner v-if="loading" size="lg" class="py-24" />
        <div v-else-if="error" class="p-12 text-center text-red-500 font-medium bg-red-50 rounded-xl border border-red-100">
          {{ error }}
        </div>

        <div v-else class="overflow-x-auto pb-4 custom-scrollbar">
          <div class="min-w-[1100px] flex flex-col">
            
            <div class="grid grid-cols-[110px_repeat(7,minmax(0,1fr))] gap-3 mb-3">
              <div class="flex items-center justify-center">
                <button 
                  @click="viewMode = viewMode === 'week' ? 'month' : 'week'" 
                  class="text-xs font-bold text-blue-600 bg-blue-50 hover:bg-blue-100 px-3 py-1.5 rounded-lg transition-colors border border-blue-200"
                >
                  {{ viewMode === 'week' ? 'Xem theo Tháng' : 'Xem theo Tuần' }}
                </button>
              </div>
              <div 
                v-for="day in currentWeekDays" 
                :key="`header-${day.format('DD')}`" 
                :class="['text-center py-3 rounded-xl transition-colors', day.isSame(dayjs(), 'day') ? 'bg-blue-50 border border-blue-200 shadow-sm' : 'bg-transparent border border-transparent']"
              >
                <div class="text-sm font-bold text-slate-800 mb-0.5">{{ dayText(day.day()) }}</div>
                <div :class="['text-[11px] font-semibold', day.isSame(dayjs(), 'day') ? 'text-blue-600' : 'text-slate-500']">
                  {{ day.format('DD/MM/YYYY') }}
                </div>
              </div>
            </div>

            <div v-if="viewMode === 'week'" class="space-y-3">
              <div v-for="shift in displayedShifts" :key="shift.id" class="grid grid-cols-[110px_repeat(7,minmax(0,1fr))] gap-3">
                
                <div class="flex flex-col items-center justify-center p-3 rounded-xl bg-slate-50 border border-slate-100 min-h-[160px] hover:border-slate-300 hover:shadow-sm transition-all duration-300">
                  <div v-html="shift.icon" :class="['mb-2', shift.iconClass]"></div>
                  <span class="font-bold text-[13px] text-slate-800">{{ shift.label }}</span>
                  <span class="text-[11px] text-slate-500 font-medium mt-1 text-center bg-white px-2 py-0.5 rounded border border-slate-200">{{ shift.time }}</span>
                </div>

                <div v-for="day in currentWeekDays" :key="`cell-${day.format('DD')}-${shift.id}`" class="h-full">
                  
                  <div v-if="getSchedulesForSlot(day, shift.id).length > 0" class="h-full flex flex-col gap-2">
                    <div 
                      v-for="schedule in getSchedulesForSlot(day, shift.id)" 
                      :key="schedule.id"
                      :class="['p-4 rounded-xl border flex flex-col h-full shadow-sm hover:-translate-y-1 hover:shadow-md transition-all duration-300 cursor-pointer', shift.cardClass]"
                    >
                      <h4 class="font-bold text-[13px] leading-snug mb-3 line-clamp-2" :title="schedule.courseNameSnapshot || schedule.classNameSnapshot">
                        {{ schedule.courseNameSnapshot || schedule.classNameSnapshot || 'Môn học' }}
                      </h4>
                      
                      <div class="mt-auto space-y-2">
                        <div class="flex items-center gap-2 text-[11px] font-medium opacity-90">
                          <svg class="w-3.5 h-3.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                          <span>{{ formatTime(schedule.startTime) }} - {{ formatTime(schedule.endTime) }}</span>
                        </div>
                        
                        <div class="flex items-center gap-2 text-[11px] font-medium opacity-90">
                          <svg class="w-3.5 h-3.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" /></svg>
                          <span class="truncate" :title="schedule.teacherName">{{ schedule.teacherName || 'Đang cập nhật' }}</span>
                        </div>
                        
                        <div class="flex items-center gap-2 text-[11px] font-medium opacity-90">
                          <svg class="w-3.5 h-3.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" /><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" /></svg>
                          <span class="truncate">Phòng {{ schedule.room || 'Trống' }}</span>
                        </div>
                      </div>
                    </div>
                  </div>

                  <div v-else class="h-full rounded-xl border border-dashed border-slate-200 bg-slate-50/50 hover:bg-slate-100 transition-colors flex flex-col items-center justify-center p-4 min-h-[160px]">
                    <svg class="w-6 h-6 text-slate-300 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>
                    <span class="text-[11px] font-medium text-slate-400 text-center">Không có lớp học</span>
                  </div>
                  
                </div>
              </div>
            </div>

            <div v-else class="enterprise-calendar mt-2">
              <a-calendar v-model:value="currentDate">
                <template #dateCellRender="{ current }">
                  <ul class="m-0 p-0 list-none space-y-1 mt-1">
                    <li v-for="schedule in getSchedulesForDay(current)" :key="schedule.id">
                      <div :class="['px-1.5 py-1 text-xs font-medium rounded border truncate transition-all', getShiftColorClass(schedule.studyShift)]">
                        <span class="font-bold opacity-80">{{ formatTime(schedule.startTime) }}</span>
                        <span class="ml-1">{{ schedule.courseNameSnapshot || schedule.classNameSnapshot }}</span>
                      </div>
                    </li>
                  </ul>
                </template>
              </a-calendar>
            </div>

          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import { message } from "ant-design-vue";
import dayjs from "dayjs";
import PageHeader from "@/components/ui/PageHeader.vue";
import LoadingSpinner from "@/components/ui/LoadingSpinner.vue";
import { studentApi } from "@/api/studentApi";
import { scheduleApi } from "@/api/scheduleApi";
import { useAuthStore } from "@/stores/auth";
import { formatTime } from "@/lib/formatters";
import { downloadIcs, downloadExcelReport, reportFilename } from "@/lib/exportDocuments";

const auth = useAuthStore();
const schedules = ref([]);
const enrollments = ref([]);
const loading = ref(true);
const error = ref("");

const viewMode = ref("week"); // 'week' hoặc 'month'
const currentDate = ref(dayjs());
const weekPickerOpen = ref(false);
const activeFilter = ref("all"); // 'all', 'Morning', 'Afternoon', 'Evening'

onMounted(loadData);

// GỌI DỮ LIỆU TỪ BACKEND
async function loadData() {
  loading.value = true;
  error.value = "";
  try {
    enrollments.value = auth.user?.referenceId ? await studentApi.getMyCourses(auth.user.referenceId) : [];
    const classIds = [...new Set(enrollments.value.map((x) => x.classId).filter(Boolean))];
    const groups = await Promise.all(
      classIds.map((id) => scheduleApi.getByClass(id).catch(() => []))
    );
    schedules.value = groups.flat();
  } catch (err) {
    error.value = err.message || "Lỗi kết nối Backend. Không thể tải dữ liệu thời khóa biểu.";
  } finally {
    loading.value = false;
  }
}

// TÍNH TOÁN NGÀY TRONG TUẦN (Từ Thứ 2 -> Chủ Nhật)
const currentWeekDays = computed(() => {
  let d = currentDate.value;
  let dayOfWeek = d.day(); 
  let diffToMonday = dayOfWeek === 0 ? 6 : dayOfWeek - 1; 
  let monday = d.subtract(diffToMonday, 'day');
  return Array.from({ length: 7 }).map((_, i) => monday.add(i, "day"));
});

function changeWeek(direction) {
  currentDate.value = currentDate.value.add(direction, "week");
}

function formatWeekRange(value) {
  if (!value) return "";
  const dayOfWeek = value.day();
  const diffToMonday = dayOfWeek === 0 ? 6 : dayOfWeek - 1;
  const monday = value.subtract(diffToMonday, "day");
  return `${monday.format("DD/MM/YYYY")} - ${monday.add(6, "day").format("DD/MM/YYYY")}`;
}

function dayText(dayIndex) {
  const map = { 1: 'Thứ 2', 2: 'Thứ 3', 3: 'Thứ 4', 4: 'Thứ 5', 5: 'Thứ 6', 6: 'Thứ 7', 0: 'Chủ nhật' };
  return map[dayIndex];
}

// LOGIC BỘ LỌC CA HỌC
function handleFilterChange({ key }) {
  activeFilter.value = key;
}

const filterLabel = computed(() => {
  const labels = { all: "Bộ lọc", Morning: "Ca Sáng", Afternoon: "Ca Chiều", Evening: "Ca Tối" };
  return labels[activeFilter.value];
});

// CẤU HÌNH GIAO DIỆN CA HỌC
const shiftConfigs = [
  { 
    id: 'Morning', 
    label: 'Sáng', 
    time: '07:30 - 11:30',
    iconClass: 'text-emerald-500',
    cardClass: 'bg-emerald-50/70 border-emerald-200 text-emerald-900 dark:bg-emerald-500/15 dark:border-emerald-400/35 dark:text-emerald-100',
    icon: `<svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" /></svg>`
  },
  { 
    id: 'Afternoon', 
    label: 'Chiều', 
    time: '13:30 - 17:30',
    iconClass: 'text-amber-500',
    cardClass: 'bg-amber-50/70 border-amber-200 text-amber-900 dark:bg-amber-500/15 dark:border-amber-400/35 dark:text-amber-100',
    icon: `<svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 15a4 4 0 004 4h9a5 5 0 10-.1-9.999 5.002 5.002 0 10-9.78 2.096A4.001 4.001 0 003 15z" /></svg>`
  },
  { 
    id: 'Evening', 
    label: 'Tối', 
    time: '18:00 - 21:00',
    iconClass: 'text-slate-600',
    cardClass: 'bg-slate-700 border-slate-600 text-slate-50 dark:bg-blue-500/20 dark:border-blue-400/35 dark:text-blue-100',
    icon: `<svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" /></svg>`
  }
];

// Danh sách Ca hiển thị dựa trên bộ lọc
const displayedShifts = computed(() => {
  if (activeFilter.value === 'all') return shiftConfigs;
  return shiftConfigs.filter(shift => shift.id === activeFilter.value);
});

const dayToNumberMap = { Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3, Thursday: 4, Friday: 5, Saturday: 6 };

// Lọc lịch hiển thị theo Ca & Ngày (Lưới Tuần)
function getSchedulesForSlot(date, shiftId) {
  const dayOfWeekNumber = date.day();
  return schedules.value.filter(
    (s) => dayToNumberMap[s.dayOfWeek] === dayOfWeekNumber && s.studyShift === shiftId
  ).sort((a, b) => String(a.startTime).localeCompare(String(b.startTime)));
}

// Lọc lịch hiển thị theo Ngày (Lịch Tháng)
function getSchedulesForDay(date) {
  const dayOfWeekNumber = date.day();
  // Áp dụng luôn bộ lọc ca học cho lịch tháng nếu đang bật
  return schedules.value.filter(
    (s) => dayToNumberMap[s.dayOfWeek] === dayOfWeekNumber && (activeFilter.value === 'all' || s.studyShift === activeFilter.value)
  ).sort((a, b) => String(a.startTime).localeCompare(String(b.startTime)));
}

// Màu dùng cho Lịch tháng cũ
function getShiftColorClass(shift) {
  switch (shift) {
    case "Morning": return "bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-500/15 dark:text-emerald-100 dark:border-emerald-400/35";
    case "Afternoon": return "bg-amber-50 text-amber-700 border-amber-200 dark:bg-amber-500/15 dark:text-amber-100 dark:border-amber-400/35";
    case "Evening": return "bg-slate-700 text-white border-slate-600 dark:bg-blue-500/20 dark:text-blue-100 dark:border-blue-400/35";
    default: return "bg-slate-50 text-slate-700 border-slate-200 dark:bg-slate-800/70 dark:text-slate-100 dark:border-slate-700";
  }
}

// Xuất lịch
function buildScheduleRows() {
  return currentWeekDays.value.flatMap((day) =>
    displayedShifts.value.flatMap((shift) =>
      getSchedulesForSlot(day, shift.id).map((schedule) => ({
        ...schedule,
        dateLabel: day.format('DD/MM/YYYY'),
        dayLabel: dayText(day.day()),
        shiftLabel: shift.label,
      })),
    ),
  )
}

function scheduleDateTime(dayLabel, timeValue) {
  const date = dayjs(dayLabel, 'DD/MM/YYYY')
  const [hour = 0, minute = 0] = String(timeValue || '00:00').split(':').map(Number)
  return date.hour(hour).minute(minute).second(0).millisecond(0).toDate()
}

function downloadSchedule() {
  const rows = buildScheduleRows()
  if (rows.length === 0) {
    message.warning('Không có lịch học trong tuần hiện tại để xuất.')
    return
  }

  message.loading({ content: 'Đang tạo file lịch học...', key: 'export' })
  try {
    downloadExcelReport({
      title: 'Lịch học tuần',
      subtitle: `${currentWeekDays.value[0].format('DD/MM/YYYY')} - ${currentWeekDays.value[6].format('DD/MM/YYYY')}`,
      filename: reportFilename('lich-hoc-tuan'),
      user: auth.user,
      summary: [
        { label: 'Tuần học', value: `${currentWeekDays.value[0].format('DD/MM/YYYY')} - ${currentWeekDays.value[6].format('DD/MM/YYYY')}` },
        { label: 'Số buổi học', value: rows.length },
        { label: 'Bộ lọc', value: filterLabel.value },
      ],
      columns: [
        { label: 'Thứ', value: (x) => x.dayLabel },
        { label: 'Ngày', value: (x) => x.dateLabel },
        { label: 'Ca', value: (x) => x.shiftLabel },
        { label: 'Môn học', value: (x) => x.courseNameSnapshot || x.classNameSnapshot || '-' },
        { label: 'Lớp', value: (x) => x.classNameSnapshot || '-' },
        { label: 'Giờ học', value: (x) => `${formatTime(x.startTime)} - ${formatTime(x.endTime)}` },
        { label: 'Giảng viên', value: (x) => x.teacherName || '-' },
        { label: 'Phòng', value: (x) => x.room || '-' },
      ],
      rows,
      notes: ['File này được tạo từ lịch học tuần đang hiển thị trên màn hình.'],
    })

    downloadIcs({
      filename: reportFilename('lich-hoc-educenter', 'ics'),
      events: rows.map((schedule) => ({
        uid: `${schedule.id}-${schedule.dateLabel}@educenter.local`,
        title: schedule.courseNameSnapshot || schedule.classNameSnapshot || 'Lịch học EduCenter',
        start: scheduleDateTime(schedule.dateLabel, schedule.startTime),
        end: scheduleDateTime(schedule.dateLabel, schedule.endTime),
        location: schedule.room ? `Phòng ${schedule.room}` : '',
        description: `${schedule.classNameSnapshot || ''} - ${schedule.teacherName || ''}`,
      })),
    })

    message.success({ content: 'Đã tải xuống bảng Excel lịch học và file lịch .ics.', key: 'export', duration: 3 })
  } catch (err) {
    message.error({ content: 'Không thể tạo file lịch học. Vui lòng thử lại.', key: 'export', duration: 3 })
  }
}</script>

<style scoped>
/* Scrollbar tinh chỉnh cho Lịch lưới Matrix */
.custom-scrollbar::-webkit-scrollbar {
  height: 8px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: #f8fafc; 
  border-radius: 4px;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #cbd5e1; 
  border-radius: 4px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #94a3b8; 
}

/* Ant Design Calendar Overrides */
.enterprise-calendar :deep(.ant-picker-calendar-full .ant-picker-panel) {
  background: transparent;
}
.enterprise-calendar :deep(.ant-picker-calendar-header) {
  padding-bottom: 16px;
  padding-top: 4px;
}
.enterprise-calendar :deep(.ant-picker-calendar-date) {
  min-height: 110px !important;
  margin: 0 !important;
  border-radius: 8px;
  transition: background-color 0.2s;
}
.enterprise-calendar :deep(.ant-picker-cell-in-view.ant-picker-cell-today .ant-picker-calendar-date) {
  background: #f0f9ff; 
  border: 1px solid #bae6fd; 
}
.enterprise-calendar :deep(.ant-picker-cell-in-view.ant-picker-cell-today .ant-picker-calendar-date-value) {
  color: #0284c7; 
  font-weight: 800;
}
.enterprise-calendar :deep(.ant-picker-cell-in-view.ant-picker-cell-selected .ant-picker-calendar-date) {
  background: #f8fafc; 
}
.enterprise-calendar :deep(.ant-picker-calendar-date-value) {
  font-weight: 600;
  color: #475569; 
}
</style>
