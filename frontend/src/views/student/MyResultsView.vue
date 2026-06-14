<template>
  <div class="space-y-6">
    <PageHeader 
      title="Bảng điểm & Kết quả" 
      subtitle="Quản lý điểm số, đánh giá chuyên cần và nhận xét chi tiết từ giảng viên." 
    >
      <template #actions>
        <button 
          class="px-4 py-2 bg-white border border-slate-200 text-slate-700 hover:bg-slate-50 font-medium rounded-lg transition-colors shadow-sm flex items-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed" 
          @click="refreshData"
          :disabled="loading"
        >
          <LoadingSpinner v-if="loading" size="sm" class="text-slate-500" />
          <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>
          Làm mới
        </button>
        <button 
          class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors shadow-sm flex items-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed active:scale-95"
          @click="exportTranscript"
          :disabled="isExporting || loading"
        >
          <LoadingSpinner v-if="isExporting" size="sm" class="!text-white" />
          <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
          Xuất bảng điểm
        </button>
      </template>
    </PageHeader>

    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="bg-white shadow-sm rounded-xl p-5 border border-slate-200 flex items-center gap-4 hover:border-blue-300 transition-colors">
        <div class="p-3 bg-blue-50 rounded-lg text-blue-600">
          <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" /></svg>
        </div>
        <div>
          <p class="text-slate-500 text-xs font-bold uppercase tracking-wider">Trung bình tích lũy</p>
          <strong class="text-2xl text-slate-800 font-black">{{ formatScore(averageTotal) }}</strong>
        </div>
      </div>
      
      <div class="bg-white shadow-sm rounded-xl p-5 border border-slate-200 flex items-center gap-4 hover:border-emerald-300 transition-colors">
        <div class="p-3 bg-emerald-50 rounded-lg text-emerald-600">
          <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
        </div>
        <div>
          <p class="text-slate-500 text-xs font-bold uppercase tracking-wider">Môn đã Đạt</p>
          <strong class="text-2xl text-emerald-600 font-black">{{ passedCount }}</strong>
        </div>
      </div>
      
      <div class="bg-white shadow-sm rounded-xl p-5 border border-slate-200 flex items-center gap-4 hover:border-red-300 transition-colors">
        <div class="p-3 bg-red-50 rounded-lg text-red-600">
          <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
        </div>
        <div>
          <p class="text-slate-500 text-xs font-bold uppercase tracking-wider">Môn chưa Đạt</p>
          <strong class="text-2xl text-red-600 font-black">{{ failedCount }}</strong>
        </div>
      </div>

      <div class="bg-gradient-to-br from-slate-800 to-slate-900 shadow-sm rounded-xl p-5 border border-slate-700 flex items-center gap-4 relative overflow-hidden">
        <div class="absolute -right-4 -bottom-4 w-20 h-20 bg-white opacity-5 rounded-full blur-xl"></div>
        <div class="p-3 bg-white/10 rounded-lg text-white">
          <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" /></svg>
        </div>
        <div class="relative z-10">
          <p class="text-slate-300 text-xs font-bold uppercase tracking-wider">Tỷ lệ hoàn thành</p>
          <strong class="text-2xl text-white font-black">{{ passRate }}%</strong>
        </div>
      </div>
    </div>

    <section class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden flex flex-col min-h-[500px]">
      
      <div class="border-b border-slate-200 px-6 pt-4 bg-slate-50/50">
        <a-tabs v-model:activeKey="activeTab">
          <a-tab-pane key="all" :tab="`Tất cả (${results.length})`" />
          <a-tab-pane key="Passed" :tab="`Đã đạt (${passedCount})`" />
          <a-tab-pane key="Failed" :tab="`Chưa đạt (${failedCount})`" />
          <a-tab-pane key="InProgress" tab="Đang học" />
        </a-tabs>
      </div>

      <div class="p-6 flex-1 bg-slate-50/30">
        <LoadingSpinner v-if="loading" size="lg" class="py-24" />
        
        <div v-else-if="error" class="p-12 text-center text-red-500 font-medium bg-red-50 rounded-xl border border-red-100 max-w-2xl mx-auto mt-10">
          {{ error }}
        </div>
        
        <div v-else-if="filteredResults.length === 0" class="flex flex-col items-center justify-center py-20 text-center">
          <svg class="w-16 h-16 text-slate-300 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
          <h3 class="text-lg font-bold text-slate-700">Không có dữ liệu</h3>
          <p class="text-sm text-slate-500 mt-1">Chưa có kết quả học tập nào phù hợp với bộ lọc hiện tại.</p>
        </div>

        <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <article 
            v-for="result in filteredResults" 
            :key="result.id" 
            :class="['bg-white rounded-2xl border-x border-b border-slate-200 shadow-sm overflow-hidden flex flex-col transition-all hover:shadow-lg border-t-4 group hover:-translate-y-1', getBorderTopColor(result.resultStatus)]"
          >
            <div class="p-5 pb-4 border-b border-slate-100 flex items-start justify-between gap-4">
              <div class="min-w-0">
                <div class="flex items-center gap-2 mb-1.5">
                  <span class="text-[10px] font-black uppercase tracking-wider px-2 py-0.5 rounded bg-slate-100 text-slate-500">Môn học</span>
                  <span :class="statusClass(result.resultStatus)">{{ statusText(result.resultStatus) }}</span>
                </div>
                <h2 class="text-lg font-bold text-slate-800 line-clamp-1 group-hover:text-blue-600 transition-colors" :title="result.courseNameSnapshot">
                  {{ result.courseNameSnapshot || 'Đang cập nhật môn học' }}
                </h2>
                <p class="text-sm text-slate-500 mt-0.5 font-medium">{{ result.classNameSnapshot || 'Lớp chưa được phân' }}</p>
              </div>
              
              <div class="shrink-0 text-right">
                <span class="block text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">Xếp loại</span>
                <span :class="['px-3 py-1 text-xs font-bold rounded-lg border', getClassificationClass(result.averageScore)]">
                  {{ getClassificationText(result.averageScore, result.resultStatus) }}
                </span>
              </div>
            </div>

            <div class="p-5 grid grid-cols-4 gap-3">
              <div class="bg-slate-50 rounded-xl p-3 text-center border border-slate-100 flex flex-col justify-center">
                <span class="block text-[11px] font-bold uppercase tracking-wide text-slate-400 mb-1">Giữa kỳ</span>
                <strong :class="['text-lg font-black', getScoreColor(result.midtermScore)]">{{ formatScore(result.midtermScore) }}</strong>
              </div>
              
              <div class="bg-slate-50 rounded-xl p-3 text-center border border-slate-100 flex flex-col justify-center">
                <span class="block text-[11px] font-bold uppercase tracking-wide text-slate-400 mb-1">Cuối kỳ</span>
                <strong :class="['text-lg font-black', getScoreColor(result.finalScore)]">{{ formatScore(result.finalScore) }}</strong>
              </div>
              
              <div class="bg-slate-50 rounded-xl p-3 text-center border border-slate-100 flex flex-col justify-center">
                <span class="block text-[11px] font-bold uppercase tracking-wide text-slate-400 mb-1">Chuyên cần</span>
                <strong :class="['text-lg font-black', getAttendanceColor(result.attendancePercent)]">{{ formatPercent(result.attendancePercent, 0) }}</strong>
              </div>

              <div class="bg-gradient-to-br from-blue-50 to-indigo-50 rounded-xl p-3 text-center border border-blue-200 flex flex-col justify-center shadow-inner">
                <span class="block text-[11px] font-bold uppercase tracking-wide text-blue-500 mb-1">Trung bình</span>
                <strong :class="['text-2xl font-black', getScoreColor(result.averageScore, true)]">{{ formatScore(result.averageScore) }}</strong>
              </div>
            </div>

            <div class="px-5 pb-5 mt-auto">
              <div class="bg-amber-50/60 rounded-xl p-4 border border-amber-200/60 relative group-hover:bg-amber-50 transition-colors">
                <svg class="absolute top-3 right-3 h-8 w-8 text-amber-500/10" fill="currentColor" viewBox="0 0 24 24"><path d="M14.017 21v-7.391c0-5.704 3.731-9.57 8.983-10.609l.995 2.151c-2.432.917-3.995 3.638-3.995 5.849h4v10h-9.983zm-14.017 0v-7.391c0-5.704 3.748-9.57 9-10.609l.996 2.151c-2.433.917-3.996 3.638-3.996 5.849h3.983v10h-9.983z" /></svg>
                <div class="flex items-center gap-1.5 mb-2">
                  <span class="w-1.5 h-1.5 rounded-full bg-amber-400"></span>
                  <p class="text-[11px] font-bold text-amber-800 uppercase tracking-wider">Nhận xét từ Giảng viên</p>
                </div>
                <p class="text-sm text-slate-700 italic leading-relaxed relative z-10 pl-3 border-l-2 border-amber-300">
                  {{ result.feedback || 'Chưa có nhận xét nào được ghi nhận cho học viên trong môn học này.' }}
                </p>
              </div>
            </div>
          </article>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { studentApi } from '@/api/studentApi'
import { useAuthStore } from '@/stores/auth'
import { formatPercent, formatScore } from '@/lib/formatters'
import { downloadExcelReport, reportFilename } from '@/lib/exportDocuments'

const auth = useAuthStore()
const results = ref([])
const loading = ref(true)
const error = ref('')
const isExporting = ref(false)
const activeTab = ref('all')

onMounted(loadData)

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    results.value = auth.user?.referenceId ? await studentApi.getMyResults(auth.user.referenceId) : []
  } catch (err) {
    error.value = err.message || 'Hệ thống đang gián đoạn. Không tải được bảng điểm lúc này.'
  } finally {
    loading.value = false
  }
}

// Làm mới dữ liệu
async function refreshData() {
  await loadData()
  message.success('Đã cập nhật bảng điểm mới nhất.')
}

// Xuất file
async function exportTranscript() {
  if (results.value.length === 0) {
    message.warning('Không có dữ liệu điểm để xuất.')
    return
  }
  isExporting.value = true
  message.loading({ content: 'Đang tạo file bảng điểm...', key: 'export' })
  try {
    downloadExcelReport({
      title: 'Bảng điểm học tập',
      subtitle: 'Tổng hợp điểm giữa kỳ, cuối kỳ, chuyên cần và đánh giá của giảng viên.',
      filename: reportFilename('bang-diem-hoc-tap'),
      user: auth.user,
      summary: [
        { label: 'Trung bình tích lũy', value: formatScore(averageTotal.value) },
        { label: 'Môn đã đạt', value: passedCount.value },
        { label: 'Môn chưa đạt', value: failedCount.value },
        { label: 'Tỷ lệ hoàn thành', value: `${passRate.value}%` },
      ],
      columns: [
        { label: 'Khóa học', value: (x) => x.courseNameSnapshot || '-' },
        { label: 'Lớp', value: (x) => x.classNameSnapshot || '-' },
        { label: 'Giữa kỳ', value: (x) => formatScore(x.midtermScore) },
        { label: 'Cuối kỳ', value: (x) => formatScore(x.finalScore) },
        { label: 'Chuyên cần', value: (x) => formatPercent(x.attendancePercent, 0) },
        { label: 'Trung bình', value: (x) => formatScore(x.averageScore) },
        { label: 'Trạng thái', value: (x) => statusText(x.resultStatus) },
        { label: 'Nhận xét', value: (x) => x.feedback || '-' },
      ],
      rows: filteredResults.value,
      notes: [
        'Bảng điểm được tạo từ dữ liệu đang hiển thị theo tab lọc hiện tại.',
        'Điểm số và trạng thái phụ thuộc dữ liệu đã được giảng viên/admin cập nhật trên hệ thống.',
      ],
    })
    message.success({ content: 'Xuất bảng điểm thành công.', key: 'export', duration: 2 })
  } catch (err) {
    message.error({ content: 'Lỗi xuất file. Vui lòng thử lại.', key: 'export' })
  } finally {
    isExporting.value = false
  }
}
// === COMPUTED THỐNG KÊ ===
const averageTotal = computed(() => {
  const validResults = results.value.filter(r => r.averageScore !== null && r.averageScore !== undefined)
  if (validResults.length === 0) return 0
  const sum = validResults.reduce((acc, curr) => acc + Number(curr.averageScore), 0)
  return sum / validResults.length
})

const passedCount = computed(() => results.value.filter(r => r.resultStatus === 'Passed').length)
const failedCount = computed(() => results.value.filter(r => r.resultStatus === 'Failed').length)

const passRate = computed(() => {
  const totalCompleted = passedCount.value + failedCount.value
  if (totalCompleted === 0) return 0
  return Math.round((passedCount.value / totalCompleted) * 100)
})

const filteredResults = computed(() => {
  if (activeTab.value === 'all') return results.value
  return results.value.filter(r => r.resultStatus === activeTab.value)
})

// === LOGIC XẾP LOẠI & HIỂN THỊ ===
function getClassificationText(score, status) {
  if (status === 'InProgress' || score === null || score === undefined) return 'Đang học'
  const numScore = Number(score)
  if (numScore >= 8.5) return 'Giỏi'
  if (numScore >= 7.0) return 'Khá'
  if (numScore >= 5.5) return 'Trung bình'
  return 'Yếu'
}

function getClassificationClass(score) {
  const numScore = Number(score)
  if (numScore >= 8.5) return 'bg-emerald-50 text-emerald-700 border-emerald-200'
  if (numScore >= 7.0) return 'bg-blue-50 text-blue-700 border-blue-200'
  if (numScore >= 5.5) return 'bg-amber-50 text-amber-700 border-amber-200'
  if (numScore > 0) return 'bg-red-50 text-red-700 border-red-200'
  return 'bg-slate-50 text-slate-500 border-slate-200' // Chưa có điểm
}

// Đổi màu text điểm số
function getScoreColor(score, isAverage = false) {
  if (score === null || score === undefined) return 'text-slate-400'
  const numScore = Number(score)
  if (numScore < 5.0) return 'text-red-500' // Yếu
  if (numScore >= 8.0) return 'text-emerald-600' // Khá/Giỏi
  return isAverage ? 'text-blue-700' : 'text-slate-800'
}

function getAttendanceColor(percent) {
  if (percent === null || percent === undefined) return 'text-slate-400'
  const num = Number(percent)
  if (num < 80) return 'text-red-500' // Cảnh báo cấm thi
  return 'text-slate-800'
}

function statusText(status) {
  return ({ Passed: 'Đã Đạt', Failed: 'Chưa Đạt', InProgress: 'Đang Học' })[status] || status
}

function statusClass(status) {
  const base = 'px-2.5 py-1 text-[10px] font-black uppercase tracking-wider rounded-md border '
  if (status === 'Passed') return base + 'bg-emerald-50 text-emerald-700 border-emerald-200'
  if (status === 'Failed') return base + 'bg-red-50 text-red-700 border-red-200'
  return base + 'bg-blue-50 text-blue-700 border-blue-200' // InProgress
}

function getBorderTopColor(status) {
  if (status === 'Passed') return 'border-t-emerald-500'
  if (status === 'Failed') return 'border-t-red-500'
  return 'border-t-blue-400'
}
</script>

<style scoped>
:deep(.ant-tabs-nav) {
  margin-bottom: 0 !important;
}
:deep(.ant-tabs-tab) {
  padding: 16px 0;
  font-weight: 600;
  color: #64748b;
  font-size: 15px;
}
:deep(.ant-tabs-tab-active) {
  color: #2563eb !important;
}
</style>
