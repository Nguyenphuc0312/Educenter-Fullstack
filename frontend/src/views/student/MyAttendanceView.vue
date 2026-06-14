<template>
  <div class="space-y-6">
    <PageHeader 
      title="Chuyên cần & Điểm danh" 
      subtitle="Theo dõi lịch sử điểm danh và tỷ lệ tham gia học tập các khóa học." 
    >
      <template #actions>
        <button 
          class="px-4 py-2 bg-white border border-slate-200 text-slate-700 hover:bg-slate-50 font-medium rounded-lg transition-colors shadow-sm flex items-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed active:scale-95" 
          @click="refreshData"
          :disabled="loading"
        >
          <LoadingSpinner v-if="loading" size="sm" class="text-slate-500" />
          <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>
          Làm mới
        </button>
        <button 
          class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors shadow-sm flex items-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed active:scale-95"
          @click="exportReport"
          :disabled="isExporting || loading"
        >
          <LoadingSpinner v-if="isExporting" size="sm" class="!text-white" />
          <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
          Xuất báo cáo
        </button>
      </template>
    </PageHeader>

    <LoadingSpinner v-if="loading" size="lg" class="py-24" />
    
    <div v-else-if="error" class="p-12 text-center text-red-500 font-medium bg-red-50 rounded-xl border border-red-100 max-w-2xl mx-auto mt-10">
      {{ error }}
    </div>
    
    <div v-else-if="records.length === 0" class="bg-white rounded-2xl shadow-sm border border-slate-100 p-16 text-center flex flex-col items-center justify-center">
      <svg class="w-16 h-16 text-slate-300 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" /></svg>
      <h3 class="text-lg font-bold text-slate-700">Chưa có dữ liệu điểm danh</h3>
      <p class="text-sm text-slate-500 mt-1">Giảng viên chưa cập nhật bản ghi điểm danh nào cho bạn.</p>
    </div>

    <template v-else>
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        <div class="lg:col-span-2 grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div class="sm:col-span-2 bg-gradient-to-r from-blue-600 to-indigo-600 rounded-xl p-6 border border-blue-700 shadow-sm flex items-center justify-between relative overflow-hidden">
            <div class="absolute right-0 top-0 bottom-0 w-64 bg-[url('https://www.transparenttextures.com/patterns/cubes.png')] opacity-20 pointer-events-none"></div>
            <div class="relative z-10 text-white">
              <span class="text-blue-100 text-sm font-semibold uppercase tracking-wider mb-1 block">Tỷ lệ chuyên cần tổng</span>
              <div class="flex items-baseline gap-2">
                <strong class="text-4xl font-black">{{ attendanceRate }}%</strong>
                <span v-if="attendanceRate >= 80" class="text-emerald-300 text-sm font-medium flex items-center gap-1"><svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" /></svg> Đạt chuẩn</span>
                <span v-else class="text-red-300 text-sm font-medium flex items-center gap-1"><svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd" /></svg> Cảnh báo</span>
              </div>
            </div>
            <div class="relative z-10 p-4 bg-white/10 rounded-full backdrop-blur-sm hidden sm:block">
              <svg class="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
            </div>
          </div>

          <div class="bg-white rounded-xl p-5 border border-slate-200 shadow-sm flex flex-col justify-center transition-all hover:border-blue-300">
            <span class="text-slate-500 text-xs font-bold uppercase tracking-wider mb-1 flex items-center gap-1.5">
              <svg class="w-4 h-4 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
              Tổng số buổi
            </span>
            <strong class="text-3xl font-black text-slate-800">{{ records.length }}</strong>
          </div>

          <div class="bg-emerald-50 rounded-xl p-5 border border-emerald-100 flex flex-col justify-center transition-all hover:shadow-md">
            <span class="text-emerald-600 text-xs font-bold uppercase tracking-wider mb-1 flex items-center gap-1.5">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" /></svg>
              Có mặt
            </span>
            <strong class="text-3xl font-black text-emerald-700">{{ countByStatus('Present') }}</strong>
          </div>
        </div>

        <div class="bg-white rounded-xl p-6 border border-slate-200 shadow-sm flex flex-col">
          <span class="text-slate-800 text-sm font-bold mb-4 block">Phân bổ chuyên cần</span>
          <div class="flex-1 flex items-center justify-center relative min-h-[160px]">
            <div class="w-40 h-40 relative">
              <Doughnut :data="chartData" :options="chartOptions" />
              <div class="absolute inset-0 flex items-center justify-center flex-col pointer-events-none">
                <span class="text-2xl font-black text-red-500">{{ countByStatus('Absent') }}</span>
                <span class="text-[10px] uppercase font-bold text-slate-400">Buổi vắng</span>
              </div>
            </div>
          </div>
          <div class="mt-4 grid grid-cols-2 gap-2 text-xs font-medium">
            <div class="flex items-center gap-2"><span class="w-3 h-3 rounded-full bg-emerald-500"></span> Có mặt</div>
            <div class="flex items-center gap-2"><span class="w-3 h-3 rounded-full bg-amber-500"></span> Muộn/Phép</div>
            <div class="flex items-center gap-2"><span class="w-3 h-3 rounded-full bg-red-500"></span> Vắng mặt</div>
          </div>
        </div>
      </div>

      <section class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden flex flex-col min-h-[400px]">
        <div class="border-b border-slate-200 px-6 pt-4 bg-slate-50/50 flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
          <a-tabs v-model:activeKey="activeTab" class="w-full">
            <a-tab-pane key="all" :tab="`Tất cả (${records.length})`" />
            <a-tab-pane key="Present" :tab="`Có mặt (${countByStatus('Present')})`" />
            <a-tab-pane key="Absent" :tab="`Vắng mặt (${countByStatus('Absent')})`" />
            <a-tab-pane key="Other" :tab="`Muộn / Phép (${countByStatus('Late') + countByStatus('Excused')})`" />
          </a-tabs>
        </div>

        <div class="p-0">
          <a-table
            :data-source="filteredRecords"
            :columns="columns"
            row-key="id"
            :pagination="{ pageSize: 10, showSizeChanger: true, showTotal: (total) => `Tổng số ${total} bản ghi` }"
            :scroll="{ x: 800 }"
            class="enterprise-table"
          >
            <template #bodyCell="{ column, record }">
              
              <template v-if="column.key === 'markedAt'">
                <div class="flex items-center gap-3">
                  <div class="p-2 bg-slate-50 rounded border border-slate-200 text-slate-500 hidden sm:block">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
                  </div>
                  <div>
                    <span class="font-bold text-slate-800 block">{{ formatDateTime(record.markedAt).split(' ')[0] }}</span>
                    <span class="text-xs text-slate-500">{{ formatDateTime(record.markedAt).split(' ')[1] || '' }}</span>
                  </div>
                </div>
              </template>
              
              <template v-else-if="column.key === 'studentNameSnapshot'">
                <span class="font-medium text-slate-700">{{ record.studentNameSnapshot || 'N/A' }}</span>
              </template>

              <template v-else-if="column.key === 'status'">
                <span :class="statusClass(record.status)">
                  <span class="mr-1.5 opacity-70 text-[10px]">●</span>
                  {{ statusText(record.status) }}
                </span>
              </template>

              <template v-else-if="column.key === 'note'">
                <span v-if="record.note" class="text-sm text-slate-600 bg-slate-50 px-3 py-1.5 rounded-lg border border-slate-100 inline-block max-w-[250px] truncate" :title="record.note">
                  {{ record.note }}
                </span>
                <span v-else class="text-slate-300 italic">-</span>
              </template>
              
            </template>
            <template #emptyText>
               <a-empty description="Không có bản ghi điểm danh nào khớp với bộ lọc." />
            </template>
          </a-table>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js'
import { Doughnut } from 'vue-chartjs'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { studentApi } from '@/api/studentApi'
import { useAuthStore } from '@/stores/auth'
import { formatDateTime } from '@/lib/formatters'
import { downloadExcelReport, reportFilename } from '@/lib/exportDocuments'

// Đăng ký module Chart.js
ChartJS.register(ArcElement, Tooltip, Legend)

const auth = useAuthStore()
const records = ref([])
const loading = ref(true)
const error = ref('')
const isExporting = ref(false)
const activeTab = ref('all')

// Cấu hình cột Table
const columns = [
  { title: 'Thời gian điểm danh', dataIndex: 'markedAt', key: 'markedAt', sorter: (a, b) => new Date(a.markedAt) - new Date(b.markedAt) },
  { title: 'Học viên', dataIndex: 'studentNameSnapshot', key: 'studentNameSnapshot' },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', width: 180 },
  { title: 'Ghi chú của Giảng viên', dataIndex: 'note', key: 'note' },
]

onMounted(loadData)

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    records.value = auth.user?.referenceId ? await studentApi.getMyAttendance(auth.user.referenceId) : []
  } catch (err) {
    error.value = err.message || 'Hệ thống đang gián đoạn. Không tải được dữ liệu chuyên cần.'
  } finally {
    loading.value = false
  }
}

async function refreshData() {
  await loadData()
  message.success('Đã làm mới dữ liệu điểm danh!')
}

async function exportReport() {
  if (records.value.length === 0) {
    message.warning('Không có dữ liệu để xuất báo cáo.')
    return
  }
  isExporting.value = true
  message.loading({ content: 'Đang tạo báo cáo chuyên cần...', key: 'exportReport' })
  try {
    downloadExcelReport({
      title: 'Báo cáo chuyên cần',
      subtitle: 'Tổng hợp lịch sử điểm danh và tỷ lệ tham gia học tập.',
      filename: reportFilename('bao-cao-chuyen-can'),
      user: auth.user,
      summary: [
        { label: 'Tổng số buổi', value: records.value.length },
        { label: 'Tỷ lệ chuyên cần', value: `${attendanceRate.value}%` },
        { label: 'Có mặt', value: countByStatus('Present') },
        { label: 'Vắng mặt', value: countByStatus('Absent') },
      ],
      columns: [
        { label: 'Thời gian điểm danh', value: (x) => formatDateTime(x.markedAt) },
        { label: 'Học viên', value: (x) => x.studentNameSnapshot || auth.user?.fullName || '-' },
        { label: 'Trạng thái', value: (x) => statusText(x.status) },
        { label: 'Ghi chú', value: (x) => x.note || '-' },
      ],
      rows: filteredRecords.value,
      notes: [
        'Báo cáo được tạo từ dữ liệu chuyên cần đang hiển thị theo bộ lọc hiện tại.',
        'Trạng thái Đi muộn/Có phép được tính một phần trong tỷ lệ chuyên cần tổng.',
      ],
    })
    message.success({ content: 'Xuất báo cáo chuyên cần thành công.', key: 'exportReport', duration: 3 })
  } catch (err) {
    message.error({ content: 'Đã xảy ra lỗi khi tạo báo cáo.', key: 'exportReport', duration: 3 })
  } finally {
    isExporting.value = false
  }
}
// === LOGIC TÍNH TOÁN & LỌC ===
const attendanceRate = computed(() => {
  if (!records.value.length) return 0
  const point = records.value.reduce((sum, item) => {
    if (item.status === 'Present') return sum + 1
    if (['Late', 'Excused'].includes(item.status)) return sum + 0.5
    return sum
  }, 0)
  return Math.round((point / records.value.length) * 100)
})

function countByStatus(status) {
  return records.value.filter(x => x.status === status).length
}

const filteredRecords = computed(() => {
  if (activeTab.value === 'all') return records.value
  if (activeTab.value === 'Other') return records.value.filter(r => ['Late', 'Excused'].includes(r.status))
  return records.value.filter(r => r.status === activeTab.value)
})

// === CẤU HÌNH BIỂU ĐỒ DOUGHNUT ===
const chartData = computed(() => ({
  labels: ['Có mặt', 'Đi muộn/Phép', 'Vắng mặt'],
  datasets: [
    {
      data: [
        countByStatus('Present'),
        countByStatus('Late') + countByStatus('Excused'),
        countByStatus('Absent')
      ],
      backgroundColor: ['#10b981', '#f59e0b', '#ef4444'], // emerald, amber, red
      borderWidth: 0,
      hoverOffset: 6
    }
  ]
}))

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  cutout: '80%', // Làm mảnh vòng tròn
  plugins: {
    legend: { display: false }, // Ẩn legend mặc định
    tooltip: {
      backgroundColor: 'rgba(15, 23, 42, 0.9)',
      titleFont: { size: 13, family: 'Inter' },
      bodyFont: { size: 13, family: 'Inter', weight: 'bold' },
      padding: 12,
      cornerRadius: 8,
      callbacks: {
        label: function(context) {
          return ` ${context.label}: ${context.raw} buổi`
        }
      }
    }
  }
}

// === HELPERS GIAO DIỆN ===
function statusText(status) {
  return ({ Present: 'Có mặt', Absent: 'Vắng mặt', Late: 'Đi muộn', Excused: 'Có phép' })[status] || status
}

function statusClass(status) {
  const base = 'inline-flex items-center px-3 py-1 rounded-full text-xs font-bold border uppercase tracking-wider '
  if (status === 'Present') return base + 'bg-emerald-50 text-emerald-700 border-emerald-200'
  if (['Late', 'Excused'].includes(status)) return base + 'bg-amber-50 text-amber-700 border-amber-200'
  if (status === 'Absent') return base + 'bg-red-50 text-red-700 border-red-200'
  return base + 'bg-slate-50 text-slate-600 border-slate-200'
}
</script>

<style scoped>
/* Tuỳ chỉnh Ant Design Table chuẩn Enterprise */
:deep(.enterprise-table .ant-table-thead > tr > th) {
  background: #f8fafc;
  color: #475569;
  font-weight: 600;
  text-transform: uppercase;
  font-size: 0.75rem;
  letter-spacing: 0.025em;
  padding: 12px 16px;
  border-bottom: 2px solid #e2e8f0;
}
:deep(.enterprise-table .ant-table-tbody > tr > td) {
  padding: 16px;
  vertical-align: middle;
  border-bottom: 1px solid #f1f5f9;
}
:deep(.enterprise-table .ant-table-tbody > tr:hover > td) {
  background-color: #f8fafc !important;
}
:deep(.ant-tabs-nav) {
  margin-bottom: 0 !important;
}
:deep(.ant-tabs-tab) {
  padding: 16px 0;
  font-weight: 600;
  color: #64748b;
  font-size: 14px;
}
:deep(.ant-tabs-tab-active) {
  color: #2563eb !important;
}
</style>
