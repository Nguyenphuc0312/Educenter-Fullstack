<template>
  <div class="space-y-6">
    <PageHeader 
      title="Nhập kết quả học tập" 
      subtitle="Đánh giá điểm thi, chuyên cần và kết xuất bảng điểm cho học viên."
    >
      <template #actions>
        <div class="hidden lg:flex items-center bg-white border border-slate-200 rounded-lg shadow-sm overflow-hidden mr-2">
          <a-tooltip title="Tải file mẫu (CSV)">
            <button @click="downloadTemplate" class="px-3 py-2 text-slate-500 hover:bg-slate-50 hover:text-blue-600 transition-colors border-r border-slate-200">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
            </button>
          </a-tooltip>
          <button @click="triggerImport" class="px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 transition-colors border-r border-slate-200 flex items-center gap-1.5" :disabled="!selectedClassId">
            <svg class="w-4 h-4 text-emerald-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" /></svg>
            Nhập CSV
          </button>
          <input type="file" ref="fileInput" accept=".csv" class="hidden" @change="handleImportCSV" />
          <button @click="exportToCSV" class="px-3 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50 transition-colors flex items-center gap-1.5" :disabled="!rows.length">
            <svg class="w-4 h-4 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" /></svg>
            Xuất CSV
          </button>
        </div>

        <button 
          class="px-4 py-2 bg-white border border-slate-200 text-slate-700 hover:bg-slate-50 font-medium rounded-lg transition-colors shadow-sm flex items-center gap-2 disabled:opacity-70 active:scale-95" 
          @click="loadBaseData"
          :disabled="loading"
        >
          <LoadingSpinner v-if="loading" size="sm" class="text-slate-500" />
          <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>
          Làm mới
        </button>
        <button 
          class="px-5 py-2 font-bold rounded-lg shadow-sm transition-all flex items-center gap-2 disabled:opacity-50 active:scale-95"
          :class="isDirty ? 'bg-amber-500 hover:bg-amber-600 text-white animate-pulse-fast' : 'bg-blue-600 hover:bg-blue-700 text-white'"
          :disabled="!rows.length || saving" 
          @click="saveAll"
        >
          <LoadingSpinner v-if="saving" size="sm" class="!text-white" />
          <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-3m-1 4l-3 3m0 0l-3-3m3 3V4" /></svg>
          {{ isDirty ? 'Lưu thay đổi (*)' : 'Lưu bảng điểm' }}
        </button>
      </template>
    </PageHeader>

    <section class="bg-white rounded-2xl p-5 border border-slate-200 shadow-sm flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div class="flex-1 max-w-xl">
        <label class="text-xs font-bold text-slate-600 uppercase tracking-wider mb-2 flex items-center gap-1.5">
          <svg class="w-4 h-4 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" /></svg>
          Chọn Lớp học
        </label>
        <a-select 
          v-model:value="selectedClassId" 
          placeholder="Tìm kiếm và chọn lớp học..." 
          class="w-full custom-select" 
          @change="loadClassData"
          show-search
          option-filter-prop="children"
        >
          <a-select-option v-for="cls in classes" :key="cls.id" :value="cls.id">
            <div class="font-medium text-slate-700">
              <span class="font-bold text-blue-600 mr-2">[{{ cls.classCode || 'LỚP' }}]</span> {{ cls.className }}
            </div>
          </a-select-option>
        </a-select>
      </div>

      <div v-if="isDirty" class="flex items-center gap-3 bg-amber-50 border border-amber-200 px-4 py-2.5 rounded-xl text-amber-700 text-xs font-medium md:max-w-xs animate-fade-in">
        <svg class="w-5 h-5 shrink-0 text-amber-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" /></svg>
        <p>Phát hiện thay đổi chưa được lưu. Hãy nhấn <strong>Lưu thay đổi</strong> trước khi thoát!</p>
      </div>
    </section>

    <div v-if="loading" class="py-24 flex justify-center">
      <LoadingSpinner size="lg" />
    </div>

    <div v-else-if="!selectedClassId" class="py-20 bg-slate-50/50 rounded-2xl border border-dashed border-slate-300 text-center flex flex-col items-center">
      <div class="w-20 h-20 bg-white rounded-full shadow-sm border border-slate-100 flex items-center justify-center text-slate-300 mb-5">
        <svg class="w-10 h-10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" /></svg>
      </div>
      <h2 class="text-xl font-black text-slate-700 mb-2">Chưa chọn lớp học</h2>
      <p class="text-slate-500 max-w-md mx-auto">Sử dụng hộp chọn phía trên để bắt đầu phiên nhập điểm.</p>
    </div>

    <div v-else-if="rows.length === 0" class="py-20 bg-slate-50/50 rounded-2xl border border-dashed border-slate-300 text-center flex flex-col items-center">
      <div class="w-20 h-20 bg-white rounded-full shadow-sm border border-slate-100 flex items-center justify-center text-slate-300 mb-5">
        <svg class="w-10 h-10" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" /></svg>
      </div>
      <h2 class="text-xl font-black text-slate-700 mb-2">Lớp chưa có học viên</h2>
      <p class="text-slate-500 max-w-md mx-auto">Không có học viên nào đạt điều kiện ghi danh (Đang học/Đã xác nhận) để nhập điểm.</p>
    </div>

    <template v-else>
      <section class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div class="bg-white rounded-xl p-4 border border-slate-200 shadow-sm flex items-center justify-between transition-shadow hover:shadow-md">
          <div>
            <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">Sĩ số lớp</p>
            <strong class="text-2xl font-black text-slate-800">{{ rows.length }}</strong>
          </div>
          <div class="w-10 h-10 bg-slate-50 text-slate-500 rounded-full flex items-center justify-center"><svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" /></svg></div>
        </div>
        <div class="bg-white rounded-xl p-4 border border-slate-200 shadow-sm flex items-center justify-between transition-shadow hover:shadow-md">
          <div>
            <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">Tỷ lệ Đạt</p>
            <strong class="text-2xl font-black text-emerald-600">{{ stats.passRate }}%</strong>
          </div>
          <div class="w-10 h-10 bg-emerald-50 text-emerald-500 rounded-full flex items-center justify-center"><svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg></div>
        </div>
        <div class="bg-white rounded-xl p-4 border border-slate-200 shadow-sm flex items-center justify-between transition-shadow hover:shadow-md">
          <div>
            <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">Điểm TB Lớp</p>
            <strong class="text-2xl font-black text-blue-600">{{ stats.avgScore }}</strong>
          </div>
          <div class="w-10 h-10 bg-blue-50 text-blue-500 rounded-full flex items-center justify-center"><svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" /></svg></div>
        </div>
        <div class="bg-white rounded-xl p-4 border border-slate-200 shadow-sm flex items-center justify-between transition-shadow hover:shadow-md">
          <div>
            <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">Chuyên cần TB</p>
            <strong class="text-2xl font-black text-indigo-600">{{ stats.avgAttendance }}%</strong>
          </div>
          <div class="w-10 h-10 bg-indigo-50 text-indigo-500 rounded-full flex items-center justify-center"><svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg></div>
        </div>
      </section>

      <section class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden flex flex-col relative">
        <div class="p-4 border-b border-slate-100 bg-slate-50/80 flex flex-col lg:flex-row lg:items-center justify-between gap-4">
          <div>
            <h2 class="text-base font-bold text-slate-800">Danh sách học viên</h2>
            <p class="text-[11px] font-medium text-slate-500 mt-0.5">TB = (Giữa kỳ × 0.4) + (Cuối kỳ × 0.6). Điều kiện Đạt: TB ≥ 5.0 & Chuyên cần ≥ 70%.</p>
          </div>
          
          <div class="flex items-center gap-3">
            <div class="relative">
              <svg class="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" /></svg>
              <input 
                v-model="searchQuery" 
                type="text" 
                placeholder="Tìm tên học viên..." 
                class="pl-9 pr-4 py-1.5 text-sm bg-white border border-slate-300 rounded-lg focus:ring-2 focus:ring-blue-100 focus:border-blue-400 outline-none transition-all w-full sm:w-64"
              />
            </div>
            <a-tooltip title="Đặt nhanh 100% chuyên cần cho học viên chưa nhập">
              <button @click="quickFillAttendance" class="p-2 bg-white border border-slate-200 rounded-lg text-slate-500 hover:text-emerald-600 hover:bg-emerald-50 transition-colors">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" /></svg>
              </button>
            </a-tooltip>
          </div>
        </div>

        <div class="p-0 overflow-x-auto custom-scrollbar">
          <a-table
            :data-source="filteredRows"
            :columns="columns"
            row-key="studentId"
            size="middle"
            class="enterprise-table"
            :pagination="false"
            :scroll="{ x: 1050 }"
          >
            <template #bodyCell="{ column, record, index }">
              
              <template v-if="column.key === 'index'">
                <span class="text-slate-400 font-semibold">{{ index + 1 }}</span>
              </template>

              <template v-else-if="column.key === 'student'">
                <div class="flex items-center gap-3 py-1">
                  <div class="w-9 h-9 rounded-xl flex items-center justify-center text-[10px] font-black text-white shrink-0 shadow-sm border border-white/20" :style="{ background: avatarBg(record.studentNameSnapshot) }">
                    {{ getInitials(record.studentNameSnapshot) }}
                  </div>
                  <div class="min-w-0 flex flex-col">
                    <span class="font-bold text-slate-800 text-[13px] truncate" :title="record.studentNameSnapshot">{{ record.studentNameSnapshot }}</span>
                    <span class="text-[10px] text-slate-500 font-mono mt-0.5">{{ record.studentId.substring(0,8).toUpperCase() }}</span>
                  </div>
                </div>
              </template>

              <template v-else-if="column.key === 'attendancePercent'">
                <div class="flex items-center gap-2">
                  <a-input-number 
                    v-model:value="record.attendancePercent" 
                    :min="0" :max="100" :step="1" 
                    class="w-[70px] custom-number-input" 
                    @change="markDirty"
                  />
                  <span class="text-xs font-bold text-slate-400">%</span>
                </div>
              </template>

              <template v-else-if="column.key === 'midtermScore'">
                <a-input-number 
                  v-model:value="record.midtermScore" 
                  :min="0" :max="10" :step="0.25" 
                  class="w-[85px] custom-number-input" 
                  placeholder="0.0"
                  @change="markDirty"
                />
              </template>

              <template v-else-if="column.key === 'finalScore'">
                <a-input-number 
                  v-model:value="record.finalScore" 
                  :min="0" :max="10" :step="0.25" 
                  class="w-[85px] custom-number-input" 
                  placeholder="0.0"
                  @change="markDirty"
                />
              </template>

              <template v-else-if="column.key === 'average'">
                <div :class="['inline-flex items-center justify-center px-3 py-1.5 rounded-lg border shadow-sm min-w-[50px]', getAvgBadgeClass(average(record))]">
                  <span class="text-[14px] font-black leading-none">
                    {{ average(record).toFixed(1) }}
                  </span>
                </div>
              </template>

              <template v-else-if="column.key === 'result'">
                <span class="whitespace-nowrap inline-flex">
                  <span :class="['px-2.5 py-1 text-[11px] font-black rounded-md border uppercase tracking-wider', passed(record) ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-rose-50 text-rose-700 border-rose-200']">
                    {{ passed(record) ? 'Đạt' : 'Không Đạt' }}
                  </span>
                </span>
              </template>

              <template v-else-if="column.key === 'feedback'">
                <a-textarea 
                  v-model:value="record.feedback" 
                  placeholder="Nhập nhận xét..." 
                  :auto-size="{ minRows: 1, maxRows: 3 }"
                  class="text-xs bg-slate-50 border-slate-200 focus:bg-white rounded-lg custom-scrollbar min-w-[180px]"
                  @change="markDirty"
                />
              </template>

            </template>
            <template #emptyText>
              <div class="py-10 text-slate-400">Không tìm thấy học viên phù hợp.</div>
            </template>
          </a-table>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup>
import { onMounted, ref, watch, computed } from 'vue'
import { message } from 'ant-design-vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { classApi } from '@/api/classApi'
import { enrollmentApi } from '@/api/enrollmentApi'
import { resultApi } from '@/api/resultApi'
import { useAuthStore } from '@/stores/auth'

const props = defineProps({ classId: { type: String, default: '' } })
const auth = useAuthStore()

const loading = ref(false)
const saving = ref(false)
const isDirty = ref(false) // Trạng thái chưa lưu
const searchQuery = ref('')

const classes = ref([])
const selectedClassId = ref(props.classId || undefined)
const rows = ref([])

// Cấu trúc bảng (Cố định cột tên và fix độ rộng)
const columns = [
  { title: '#', key: 'index', width: 50, align: 'center', fixed: 'left' },
  { title: 'Thông tin Học viên', key: 'student', width: 240, fixed: 'left' },
  { title: 'Chuyên cần', key: 'attendancePercent', width: 130 },
  { title: 'Giữa kỳ (40%)', key: 'midtermScore', width: 120 },
  { title: 'Cuối kỳ (60%)', key: 'finalScore', width: 120 },
  { title: 'Trung bình', key: 'average', width: 100, align: 'center' },
  { title: 'Xếp loại', key: 'result', width: 130, align: 'center' },
  { title: 'Phản hồi từ Giảng viên', key: 'feedback', minWidth: 200 }
]

// ================= LOGIC & CALCULATION =================
function activeEnrollments(items) {
  return items.filter(x => ['Confirmed', 'Studying', 2, 3, '2', '3'].includes(x.status))
}

function average(row) {
  return Number(row.midtermScore || 0) * 0.4 + Number(row.finalScore || 0) * 0.6
}

function passed(row) {
  return average(row) >= 5 && Number(row.attendancePercent || 0) >= 70
}

function markDirty() {
  isDirty.value = true
}

const filteredRows = computed(() => {
  if (!searchQuery.value) return rows.value
  const q = searchQuery.value.toLowerCase()
  return rows.value.filter(r => r.studentNameSnapshot?.toLowerCase().includes(q))
})

function quickFillAttendance() {
  rows.value.forEach(r => {
    if (r.attendancePercent == null || r.attendancePercent === 0) {
      r.attendancePercent = 100
      isDirty.value = true
    }
  })
  message.success('Đã điền nhanh 100% chuyên cần cho các ô trống.')
}

// ================= THỐNG KÊ REAL-TIME =================
const stats = computed(() => {
  if (rows.value.length === 0) return { passRate: 0, avgScore: '0.0', avgAttendance: 0 }
  const totalStudents = rows.value.length
  const passedStudents = rows.value.filter(r => passed(r)).length
  const totalScore = rows.value.reduce((acc, curr) => acc + average(curr), 0)
  const totalAttendance = rows.value.reduce((acc, curr) => acc + Number(curr.attendancePercent || 0), 0)

  return {
    passRate: Math.round((passedStudents / totalStudents) * 100),
    avgScore: (totalScore / totalStudents).toFixed(1),
    avgAttendance: Math.round(totalAttendance / totalStudents)
  }
})

// ================= UI HELPERS =================
function getInitials(name) {
  if (!name) return 'HV'
  const parts = String(name).trim().split(/\s+/)
  if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

const AVATAR_COLORS = [
  'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)',
  'linear-gradient(135deg, #10b981 0%, #047857 100%)',
  'linear-gradient(135deg, #f59e0b 0%, #b45309 100%)',
  'linear-gradient(135deg, #8b5cf6 0%, #5b21b6 100%)',
  'linear-gradient(135deg, #ec4899 0%, #be185d 100%)',
]

function avatarBg(name) {
  if (!name) return 'linear-gradient(135deg, #94a3b8 0%, #475569 100%)'
  let hash = 0
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length]
}

function getAvgBadgeClass(score) {
  if (score == null || score === 0) return 'bg-slate-50 border-slate-200 text-slate-400'
  if (score >= 8) return 'bg-emerald-50 border-emerald-200 text-emerald-700'
  if (score >= 5) return 'bg-blue-50 border-blue-200 text-blue-700'
  return 'bg-rose-50 border-rose-200 text-rose-700'
}

// ================= IMPORT / EXPORT CSV =================
const fileInput = ref(null)

function triggerImport() {
  fileInput.value.click()
}

function handleImportCSV(event) {
  const file = event.target.files[0]
  if (!file) return

  // Frontend Mock CSV Parser for demonstration
  const reader = new FileReader()
  reader.onload = (e) => {
    try {
      const text = e.target.result
      const lines = text.split('\n')
      let count = 0
      
      // Simple parser (assuming ID is first column, skip header)
      for (let i = 1; i < lines.length; i++) {
        const parts = lines[i].split(',')
        if (parts.length >= 4) {
          const sId = parts[0].trim()
          const rowMatch = rows.value.find(r => r.studentId === sId)
          if (rowMatch) {
            rowMatch.attendancePercent = parseFloat(parts[2]) || 0
            rowMatch.midtermScore = parseFloat(parts[3]) || 0
            rowMatch.finalScore = parseFloat(parts[4]) || 0
            if (parts[7]) rowMatch.feedback = parts[7].replace(/['"]+/g, '').trim()
            count++
            isDirty.value = true
          }
        }
      }
      message.success(`Đã nhập điểm thành công cho ${count} học viên. Đừng quên bấm Lưu!`)
    } catch(err) {
      message.error('Định dạng file không hợp lệ.')
    }
  }
  reader.readAsText(file)
  event.target.value = '' // reset
}

function downloadTemplate() {
  const header = "Mã Học Viên,Tên Học Viên,Chuyên Cần (%),Điểm Giữa Kỳ,Điểm Cuối Kỳ,Điểm TB,Xếp Loại,Nhận Xét\n"
  let content = header
  
  if (rows.value.length > 0) {
    content += rows.value.map(r => `${r.studentId},"${r.studentNameSnapshot}",${r.attendancePercent||0},${r.midtermScore||0},${r.finalScore||0},0,"",""`).join('\n')
  } else {
    content += 'HV001,"Nguyễn Văn A",100,8.5,9.0,0,"","Học tốt"'
  }

  downloadBlob(content, 'Template_NhapDiem.csv')
}

function exportToCSV() {
  const header = "Mã Học Viên,Tên Học Viên,Chuyên Cần (%),Điểm Giữa Kỳ,Điểm Cuối Kỳ,Điểm TB,Xếp Loại,Nhận Xét\n"
  const content = header + rows.value.map(r => {
    return `${r.studentId},"${r.studentNameSnapshot}",${r.attendancePercent||0},${r.midtermScore||0},${r.finalScore||0},${average(r).toFixed(1)},"${passed(r)?'Đạt':'Không đạt'}","${r.feedback||''}"`
  }).join('\n')
  
  downloadBlob(content, `BangDiem_${classes.value.find(c => c.id === selectedClassId.value)?.classCode || 'Lop'}.csv`)
}

function downloadBlob(content, filename) {
  // Use BOM for UTF-8 Excel support
  const blob = new Blob(["\uFEFF" + content], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement("a")
  link.setAttribute("href", url)
  link.setAttribute("download", filename)
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

// ================= DATA FETCHING =================
async function loadBaseData() {
  if (!auth.user?.referenceId) return
  loading.value = true
  try {
    classes.value = await classApi.getByTeacher(auth.user.referenceId)
    if (!selectedClassId.value && classes.value.length) selectedClassId.value = classes.value[0].id
    await loadClassData()
  } catch(err) {
    message.error('Lỗi khi tải danh sách lớp.')
  } finally {
    loading.value = false
  }
}

async function loadClassData() {
  if (!selectedClassId.value) return
  loading.value = true
  searchQuery.value = ''
  isDirty.value = false
  try {
    const selectedClass = classes.value.find(x => x.id === selectedClassId.value) || await classApi.getById(selectedClassId.value)
    const [enrollments, results] = await Promise.all([
      enrollmentApi.getByClass(selectedClassId.value),
      resultApi.getByClass(selectedClassId.value)
    ])
    
    const resultByStudent = new Map((results || []).map(x => [x.studentId, x]))
    
    rows.value = activeEnrollments(enrollments || []).map(en => {
      const existing = resultByStudent.get(en.studentId)
      return {
        id: existing?.id || null,
        studentId: en.studentId,
        studentNameSnapshot: en.studentNameSnapshot,
        courseId: en.courseId || selectedClass.courseId,
        courseNameSnapshot: en.courseNameSnapshot || selectedClass.courseNameSnapshot,
        classId: en.classId || selectedClass.id,
        classNameSnapshot: en.classNameSnapshot || selectedClass.className,
        midtermScore: existing?.midtermScore !== undefined ? Number(existing.midtermScore) : null,
        finalScore: existing?.finalScore !== undefined ? Number(existing.finalScore) : null,
        attendancePercent: existing?.attendancePercent !== undefined ? Number(existing.attendancePercent) : 100,
        feedback: existing?.feedback || ''
      }
    })
  } catch(err) {
    message.error('Không thể tải danh sách học viên.')
  } finally {
    loading.value = false
  }
}

async function saveAll() {
  saving.value = true
  try {
    for (const row of rows.value) {
      const payload = {
        studentId: row.studentId,
        courseId: row.courseId,
        courseNameSnapshot: row.courseNameSnapshot,
        classId: row.classId,
        classNameSnapshot: row.classNameSnapshot,
        midtermScore: Number(row.midtermScore || 0),
        finalScore: Number(row.finalScore || 0),
        attendancePercent: Number(row.attendancePercent || 0),
        resultStatus: passed(row) ? 2 : 3, // 2: Passed, 3: Failed
        averageScore: average(row),
        feedback: row.feedback,
        evaluatedByTeacherId: auth.user.referenceId,
        evaluatedByTeacherName: auth.user.fullName || auth.user.username
      }
      const saved = row.id ? await resultApi.update(row.id, payload) : await resultApi.create(payload)
      row.id = saved.id
    }
    isDirty.value = false // Tắt cờ cảnh báo
    message.success('Toàn bộ bảng điểm đã được lưu thành công.')
    await loadClassData()
  } catch (error) {
    message.error(error.message || 'Hệ thống gặp lỗi, không lưu được kết quả.')
  } finally {
    saving.value = false
  }
}

// Cảnh báo người dùng khi reload page nếu có thay đổi chưa lưu
window.addEventListener('beforeunload', (e) => {
  if (isDirty.value) {
    e.preventDefault()
    e.returnValue = ''
  }
})

watch(() => props.classId, (value) => { if (value) selectedClassId.value = value })
onMounted(loadBaseData)
</script>

<style scoped>
/* Hiệu ứng chớp nháy cảnh báo (Màu cam) */
.animate-pulse-fast {
  animation: pulse-border 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}
@keyframes pulse-border {
  0%, 100% { box-shadow: 0 0 0 0 rgba(245, 158, 11, 0.4); }
  50% { box-shadow: 0 0 0 4px rgba(245, 158, 11, 0); }
}

.animate-fade-in {
  animation: fadeIn 0.3s ease-in-out forwards;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(-5px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Custom Input Number cho bảng điểm */
:deep(.custom-number-input) {
  border-radius: 8px;
  border-color: #e2e8f0;
}
:deep(.custom-number-input:hover),
:deep(.custom-number-input:focus-within) {
  border-color: #3b82f6;
  box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.1);
}
:deep(.custom-number-input .ant-input-number-input) {
  font-weight: 700;
  color: #1e293b;
  text-align: center;
}

/* Custom Select cho Toolbar */
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

/* Custom Table Enterprise */
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
  padding: 10px 16px;
  vertical-align: middle;
  border-bottom: 1px solid #f1f5f9;
}
:deep(.enterprise-table .ant-table-tbody > tr:hover > td) {
  background-color: #f8fafc !important;
}

/* Scrollbar */
.custom-scrollbar::-webkit-scrollbar { width: 6px; height: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #94a3b8; }
</style>