<template>
  <div class="space-y-6">
    <PageHeader 
      title="Lớp đang phụ trách" 
      subtitle="Quản lý danh sách lớp học, điểm danh, nhập điểm và theo dõi tiến độ đào tạo."
    >
      <template #actions>
        <button 
          class="px-4 py-2 bg-emerald-50 border border-emerald-200 text-emerald-700 hover:bg-emerald-100 font-bold rounded-lg transition-colors shadow-sm flex items-center gap-2 active:scale-95" 
          @click="exportToCSV"
          :disabled="filteredClasses.length === 0"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" /></svg>
          Xuất danh sách
        </button>
        <button 
          class="px-4 py-2 bg-blue-600 text-white font-bold rounded-lg transition-colors shadow-sm flex items-center gap-2 hover:bg-blue-700 disabled:opacity-70 disabled:cursor-not-allowed active:scale-95" 
          @click="loadData"
          :disabled="loading"
        >
          <LoadingSpinner v-if="loading" size="sm" class="!text-white" />
          <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>
          Làm mới
        </button>
      </template>
    </PageHeader>

    <section class="bg-white rounded-2xl p-4 border border-slate-200 shadow-sm flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div class="flex-1 max-w-md relative">
        <svg class="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" /></svg>
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="Tìm theo tên lớp, mã lớp..." 
          class="w-full pl-9 pr-4 py-2 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:ring-2 focus:ring-blue-100 focus:border-blue-400 outline-none transition-all"
        />
      </div>

      <div class="flex items-center gap-3">
        <label class="text-xs font-bold text-slate-500 uppercase tracking-wider hidden sm:block">Trạng thái:</label>
        <a-select 
          v-model:value="filterStatus" 
          allow-clear 
          placeholder="Tất cả trạng thái" 
          class="w-40 custom-select"
        >
          <a-select-option value="Open">Đang mở</a-select-option>
          <a-select-option value="InProgress">Đang học</a-select-option>
          <a-select-option value="Completed">Hoàn thành</a-select-option>
          <a-select-option value="Cancelled">Đã hủy</a-select-option>
        </a-select>
      </div>
    </section>

    <div v-if="loading" class="py-24 flex justify-center">
      <LoadingSpinner size="lg" />
    </div>

    <div v-else-if="classes.length === 0" class="bg-white border border-slate-200 rounded-2xl p-16 text-center flex flex-col items-center shadow-sm">
      <div class="w-20 h-20 bg-slate-50 text-slate-300 rounded-full flex items-center justify-center mb-4">
        <svg class="w-10 h-10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" /></svg>
      </div>
      <h3 class="text-lg font-bold text-slate-700">Chưa được phân công</h3>
      <p class="text-sm text-slate-500 mt-1 max-w-sm mx-auto">Bạn chưa được gắn vào lớp học nào trong hệ thống. Vui lòng liên hệ Giáo vụ hoặc Admin.</p>
    </div>

    <div v-else-if="filteredClasses.length === 0" class="bg-white border border-slate-200 rounded-2xl p-16 text-center flex flex-col items-center shadow-sm">
      <div class="w-20 h-20 bg-slate-50 text-slate-300 rounded-full flex items-center justify-center mb-4">
        <svg class="w-10 h-10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" /></svg>
      </div>
      <h3 class="text-lg font-bold text-slate-700">Không tìm thấy kết quả</h3>
      <p class="text-sm text-slate-500 mt-1 max-w-sm mx-auto">Không có lớp học nào khớp với từ khóa hoặc bộ lọc của bạn.</p>
      <button @click="searchQuery = ''; filterStatus = undefined" class="mt-4 px-4 py-2 text-blue-600 bg-blue-50 font-bold rounded-lg hover:bg-blue-100 transition-colors">Xóa bộ lọc</button>
    </div>

    <template v-else>
      <section class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-5">
        <article v-for="cls in filteredClasses" :key="cls.id" class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden flex flex-col transition-all duration-300 hover:shadow-lg hover:border-blue-300 group">
          
          <div class="p-5 flex-1 relative">
            <div class="flex items-start justify-between gap-3 mb-3">
              <span class="px-2.5 py-1 rounded-md text-[10px] font-black uppercase tracking-wider bg-slate-100 text-slate-600 border border-slate-200">
                {{ cls.classCode }}
              </span>
              <span :class="['px-2.5 py-1 rounded-md text-[10px] font-black uppercase tracking-wider border whitespace-nowrap', statusColorClass(cls.status)]">
                {{ statusLabel(cls.status) }}
              </span>
            </div>

            <div class="mb-4 cursor-pointer" @click="goToDetail(cls.id)">
              <h2 class="text-lg font-black text-slate-800 leading-snug line-clamp-2 group-hover:text-blue-700 transition-colors" :title="cls.className">{{ cls.className }}</h2>
              <p class="text-xs font-semibold text-blue-600 mt-1 line-clamp-1" :title="cls.courseNameSnapshot">{{ cls.courseNameSnapshot }}</p>
            </div>

            <div class="mb-5 bg-slate-50 p-3 rounded-xl border border-slate-100">
              <div class="flex justify-between text-xs font-bold text-slate-600 mb-1.5 font-variant-numeric">
                <span class="flex items-center gap-1.5">
                  <svg class="w-3.5 h-3.5 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"/></svg>
                  Sĩ số lớp
                </span>
                <span :class="getPercentColorText(capacityPercent(cls))">{{ cls.currentStudents }}/{{ cls.maxStudents }} ({{ capacityPercent(cls) }}%)</span>
              </div>
              <div class="w-full h-1.5 bg-slate-200 rounded-full overflow-hidden shadow-inner">
                <div class="h-full rounded-full transition-all duration-500" :class="getPercentBgColor(capacityPercent(cls))" :style="{ width: `${capacityPercent(cls)}%` }"></div>
              </div>
            </div>

            <div class="grid grid-cols-2 gap-3 text-xs">
              <div class="flex items-center gap-1.5 text-slate-600">
                <svg class="w-4 h-4 text-emerald-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m3-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"/></svg>
                <span class="truncate font-medium">P. {{ cls.room || 'Trống' }}</span>
              </div>
              <div class="flex items-center gap-1.5 text-slate-600">
                <svg class="w-4 h-4 text-purple-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9"/></svg>
                <span class="truncate font-medium">{{ modeLabel(cls.learningMode) }}</span>
              </div>
              <div class="col-span-2 flex items-center gap-1.5 text-slate-600 mt-1 border-t border-slate-100 pt-3">
                <svg class="w-4 h-4 text-amber-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>
                <span class="truncate font-medium">{{ formatDate(cls.startDate) }} <span class="text-slate-400 mx-0.5">→</span> {{ formatDate(cls.endDate) }}</span>
              </div>
            </div>
          </div>

          <div class="grid grid-cols-3 divide-x divide-slate-100 border-t border-slate-100 bg-slate-50/50 mt-auto">
            <button @click="goToDetail(cls.id)" class="py-3 text-center text-xs font-bold text-slate-600 hover:text-blue-600 hover:bg-white transition-colors flex flex-col items-center gap-1 whitespace-nowrap">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
              Chi tiết
            </button>
            <button @click="goToAttendance(cls.id)" class="py-3 text-center text-xs font-bold text-slate-600 hover:text-emerald-600 hover:bg-white transition-colors flex flex-col items-center gap-1 whitespace-nowrap">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
              Điểm danh
            </button>
            <button @click="goToResults(cls.id)" class="py-3 text-center text-xs font-bold text-slate-600 hover:text-purple-600 hover:bg-white transition-colors flex flex-col items-center gap-1 whitespace-nowrap">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" /></svg>
              Nhập điểm
            </button>
          </div>
        </article>
      </section>

      <section class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden mt-6">
        <div class="p-5 border-b border-slate-100 bg-slate-50/50 flex justify-between items-center">
          <h2 class="text-base font-bold text-slate-800">Danh sách tổng hợp</h2>
          <span class="text-xs font-semibold text-slate-500 bg-slate-200 px-2 py-1 rounded-md">{{ filteredClasses.length }} Lớp</span>
        </div>
        
        <div class="p-0 overflow-x-auto custom-scrollbar">
          <a-table
            :data-source="filteredClasses"
            :columns="columns"
            row-key="id"
            class="enterprise-table"
            :pagination="{ pageSize: 10, showSizeChanger: false }"
            :scroll="{ x: 1000 }"
          >
            <template #bodyCell="{ column, record }">
              
              <template v-if="column.key === 'className'">
                <div class="py-1 min-w-[200px]">
                  <p class="font-bold text-slate-800 text-sm hover:text-blue-600 transition-colors cursor-pointer line-clamp-1" @click="goToDetail(record.id)">
                    {{ record.className }}
                  </p>
                  <div class="flex items-center gap-2 mt-1">
                    <span class="px-1.5 py-0.5 bg-slate-100 border border-slate-200 text-slate-500 rounded text-[10px] font-mono">{{ record.classCode }}</span>
                    <span class="text-[11px] text-slate-500 font-medium truncate max-w-[200px]">{{ record.courseNameSnapshot }}</span>
                  </div>
                </div>
              </template>

              <template v-else-if="column.key === 'capacity'">
                <div class="min-w-[120px] max-w-[160px] space-y-1.5 py-1">
                  <div class="flex justify-between text-[11px] font-bold text-slate-600 font-variant-numeric">
                    <span>{{ record.currentStudents }}/{{ record.maxStudents }}</span>
                    <span :class="getPercentColorText(capacityPercent(record))">{{ capacityPercent(record) }}%</span>
                  </div>
                  <div class="w-full h-1.5 bg-slate-100 rounded-full overflow-hidden shadow-inner">
                    <div class="h-full rounded-full transition-all duration-500" :class="getPercentBgColor(capacityPercent(record))" :style="{ width: `${capacityPercent(record)}%` }"></div>
                  </div>
                </div>
              </template>

              <template v-else-if="column.key === 'status'">
                <span :class="['px-2.5 py-1 text-[11px] font-bold rounded-md border whitespace-nowrap', statusColorClass(record.status)]">
                  {{ statusLabel(record.status) }}
                </span>
              </template>

              <template v-else-if="column.key === 'room'">
                <span class="font-medium text-slate-700 whitespace-nowrap">P.{{ record.room || '—' }}</span>
              </template>

              <template v-else-if="column.key === 'actions'">
                <div class="flex items-center gap-2 justify-end whitespace-nowrap">
                  <button @click="goToDetail(record.id)" class="px-3 py-1.5 bg-white border border-slate-300 text-slate-700 hover:text-blue-600 hover:border-blue-300 hover:bg-blue-50 font-bold rounded-lg text-xs transition-colors shadow-sm active:scale-95 flex items-center gap-1.5">
                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>
                    Chi tiết
                  </button>
                  <button @click="goToAttendance(record.id)" class="px-3 py-1.5 bg-emerald-50 text-emerald-600 border border-emerald-200 hover:bg-emerald-100 hover:border-emerald-300 font-bold rounded-lg text-xs transition-colors shadow-sm active:scale-95 flex items-center gap-1.5">
                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" /></svg>
                    Điểm danh
                  </button>
                  <button @click="goToResults(record.id)" class="px-3 py-1.5 bg-purple-50 text-purple-600 border border-purple-200 hover:bg-purple-100 hover:border-purple-300 font-bold rounded-lg text-xs transition-colors shadow-sm active:scale-95 flex items-center gap-1.5">
                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" /></svg>
                    Nhập điểm
                  </button>
                </div>
              </template>
              
            </template>
          </a-table>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup>
import { onMounted, ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { message } from 'ant-design-vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { classApi } from '@/api/classApi'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()
const loading = ref(false)
const classes = ref([])

// Cấu hình Bảng (Table Columns)
const columns = [
  { title: 'Thông tin Lớp học', key: 'className', minWidth: 260 },
  { title: 'Phòng học', dataIndex: 'room', key: 'room', width: 110, align: 'center' },
  { title: 'Sĩ số', key: 'capacity', width: 170 },
  { title: 'Trạng thái', key: 'status', width: 140 },
  { title: 'Thao tác nhanh', key: 'actions', width: 340, align: 'right' }
]

// ================= BỘ LỌC VÀ TÌM KIẾM =================
const searchQuery = ref('')
const filterStatus = ref(undefined)

const filteredClasses = computed(() => {
  return classes.value.filter(c => {
    const q = searchQuery.value.toLowerCase()
    const matchSearch = c.className.toLowerCase().includes(q) || c.classCode.toLowerCase().includes(q)
    const matchStatus = filterStatus.value ? c.status == filterStatus.value : true
    return matchSearch && matchStatus
  })
})

// ================= HÀM ĐIỀU HƯỚNG ĐỘC LẬP =================
function goToDetail(classId) {
  router.push(`/teacher/classes/${classId}`)
}

function goToAttendance(classId) {
  router.push({ 
    path: '/teacher/attendance', 
    query: { classId: classId } 
  })
}

function goToResults(classId) {
  router.push({ 
    path: '/teacher/results', 
    query: { classId: classId } 
  })
}

// ================= TÍNH NĂNG XUẤT CSV (EXPORT) =================
function exportToCSV() {
  message.loading({ content: 'Đang chuẩn bị file CSV...', key: 'export' })
  
  const header = "Mã Lớp,Tên Lớp,Khóa Học,Sĩ Số Hiện Tại,Sĩ Số Tối Đa,Phòng Học,Ngày Khai Giảng,Ngày Bế Giảng,Trạng Thái\n"
  const content = header + filteredClasses.value.map(c => {
    return `"${c.classCode}","${c.className}","${c.courseNameSnapshot}",${c.currentStudents},${c.maxStudents},"${c.room || 'Chưa xếp'}","${formatDate(c.startDate)}","${formatDate(c.endDate)}","${statusLabel(c.status)}"`
  }).join('\n')
  
  // Use BOM for UTF-8 Excel support
  const blob = new Blob(["\uFEFF" + content], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement("a")
  link.setAttribute("href", url)
  link.setAttribute("download", `DanhSachLopPhuTrach_${new Date().getTime()}.csv`)
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  
  setTimeout(() => {
    message.success({ content: 'Xuất danh sách thành công!', key: 'export', duration: 2 })
  }, 500)
}

// ================= UI HELPERS =================
function capacityPercent(cls) {
  if (!cls.maxStudents) return 0
  return Math.min(100, Math.round((Number(cls.currentStudents || 0) / Number(cls.maxStudents)) * 100))
}

function getPercentBgColor(pct) {
  if (pct >= 100) return 'bg-rose-500'
  if (pct >= 80) return 'bg-amber-400'
  if (pct >= 30) return 'bg-emerald-500'
  return 'bg-blue-500'
}

function getPercentColorText(pct) {
  if (pct >= 100) return 'text-rose-600'
  if (pct >= 80) return 'text-amber-600'
  if (pct >= 30) return 'text-emerald-600'
  return 'text-blue-600'
}

function statusLabel(value) {
  const map = { Open: 'Đang mở', Full: 'Đã đầy', InProgress: 'Đang học', Completed: 'Hoàn thành', Cancelled: 'Đã hủy', 0: 'Đang mở', 1: 'Đã đầy', 2: 'Đang học', 3: 'Hoàn thành', 4: 'Đã hủy' }
  return map[value] || value
}

function statusColorClass(value) {
  const map = { 
    Open: 'bg-blue-50 text-blue-700 border-blue-200', 
    Full: 'bg-amber-50 text-amber-700 border-amber-200', 
    InProgress: 'bg-purple-50 text-purple-700 border-purple-200', 
    Completed: 'bg-emerald-50 text-emerald-700 border-emerald-200', 
    Cancelled: 'bg-rose-50 text-rose-700 border-rose-200',
    0: 'bg-blue-50 text-blue-700 border-blue-200', 
    1: 'bg-amber-50 text-amber-700 border-amber-200', 
    2: 'bg-purple-50 text-purple-700 border-purple-200', 
    3: 'bg-emerald-50 text-emerald-700 border-emerald-200', 
    4: 'bg-rose-50 text-rose-700 border-rose-200' 
  }
  return map[value] || 'bg-slate-50 text-slate-600 border-slate-200'
}

function modeLabel(value) {
  const map = { Offline: 'Trực tiếp (Offline)', Online: 'Trực tuyến (Online)', Hybrid: 'Kết hợp (Hybrid)', 0: 'Trực tiếp (Offline)', 1: 'Trực tuyến (Online)', 2: 'Kết hợp (Hybrid)' }
  return map[value] || value
}

function formatDate(d) {
  if (!d) return 'Chưa xác định'
  const date = new Date(d)
  if (Number.isNaN(date.getTime())) return d
  return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

// ================= DATA FETCHING =================
async function loadData() {
  if (!auth.user?.referenceId) return
  loading.value = true
  try {
    classes.value = await classApi.getByTeacher(auth.user.referenceId)
  } finally {
    loading.value = false
  }
}
onMounted(loadData)
</script>

<style scoped>
/* Scrollbar */
.custom-scrollbar::-webkit-scrollbar { width: 6px; height: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #94a3b8; }

/* Tùy chỉnh Select */
:deep(.custom-select .ant-select-selector) {
  border-radius: 12px !important;
  padding: 8px 16px !important;
  height: auto !important;
  border-color: #e2e8f0 !important;
  box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
}
:deep(.custom-select.ant-select-focused .ant-select-selector) {
  border-color: #3b82f6 !important;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1) !important;
}

/* Table Enterprise Styles */
:deep(.enterprise-table .ant-table-thead > tr > th) {
  background: #f8fafc;
  color: #475569;
  font-weight: 700;
  text-transform: uppercase;
  font-size: 0.75rem;
  letter-spacing: 0.025em;
  padding: 12px 16px;
  border-bottom: 2px solid #e2e8f0;
}
:deep(.enterprise-table .ant-table-tbody > tr > td) {
  padding: 14px 16px;
  vertical-align: middle;
  border-bottom: 1px solid #f1f5f9;
}
:deep(.enterprise-table .ant-table-tbody > tr:hover > td) {
  background-color: #f8fafc !important;
}
</style>