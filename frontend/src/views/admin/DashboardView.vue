<template>
  <div>
    <!-- HERO strip -->
    <div class="admin-hero mb-6">
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div class="min-w-0">
          <div class="admin-hero-meta mb-2">
            <span>Bảng điều khiển</span>
            <span style="opacity: 0.6">•</span>
            <span>{{ todayLabel }}</span>
          </div>
          <h1 class="admin-hero-title">Tổng quan vận hành trung tâm</h1>
          <p class="admin-hero-sub">Theo dõi học viên, doanh thu, công nợ và hoạt động đào tạo trong thời gian thực.</p>
        </div>
        <div class="admin-hero-actions">
          <span v-if="lastUpdated" class="admin-hero-meta">Cập nhật {{ lastUpdated }}</span>
          <button
            type="button"
            class="admin-hero-btn"
            :disabled="globalLoading"
            @click="fetchData"
          >
            <SyncOutlined />
            Làm mới
          </button>
          <router-link to="/admin/reports" class="admin-hero-btn solid">
            Báo cáo →
          </router-link>
        </div>
      </div>
    </div>

    <!-- 4 Hero KPI cards (top) -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-4">
      <div
        v-for="kpi in kpiHeroes"
        :key="kpi.key"
        :class="['kpi-hero', kpi.accent && 'kpi-hero--accent']"
      >
        <div class="flex items-start justify-between gap-3 mb-3">
          <div :class="['kpi-hero-icon', kpi.iconColor]">
            <component :is="kpi.icon" style="font-size: 22px;" />
          </div>
          <span v-if="kpi.pill" :class="['kpi-hero-pill', kpi.pill.direction]">
            <span v-if="kpi.pill.direction === 'up'">↑</span>
            <span v-else-if="kpi.pill.direction === 'down'">↓</span>
            <span v-else>·</span>
            {{ kpi.pill.text }}
          </span>
        </div>
        <p class="kpi-hero-label">{{ kpi.label }}</p>
        <p :class="['kpi-hero-value', kpi.placeholder && 'kpi-hero-value--placeholder']">
          {{ kpi.value }}
        </p>
        <p v-if="kpi.sub" class="kpi-hero-sub">{{ kpi.sub }}</p>
      </div>
    </div>

    <!-- 4 KPI mini (bottom) -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
      <div
        v-for="kpi in kpiMinis"
        :key="kpi.key"
        class="admin-card flex items-center gap-3 px-4 py-3.5"
      >
        <div :class="['kpi-hero-icon', kpi.iconColor]" style="width: 40px; height: 40px; border-radius: 12px;">
          <component :is="kpi.icon" style="font-size: 18px;" />
        </div>
        <div class="min-w-0 flex-1">
          <p class="kpi-card-label" style="font-size: 10px;">{{ kpi.label }}</p>
          <p class="text-base font-bold mt-0.5 font-variant-numeric" style="color: var(--admin-text);">
            {{ kpi.value || 0 }}
          </p>
          <p v-if="kpi.sub" class="text-[10px] mt-0.5 truncate" style="color: var(--admin-text-subtle);">
            {{ kpi.sub }}
          </p>
        </div>
      </div>
    </div>

    <!-- Bottom row: 2 insight cards + activity feed -->
    <div class="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-4">
      <!-- Lớp sắp khai giảng -->
      <div class="admin-insight-card">
        <div class="admin-insight-card-header">
          <h2 class="admin-section-title">Lớp sắp khai giảng</h2>
          <router-link to="/admin/classes" class="text-xs" style="color: var(--admin-accent);">Quản lý →</router-link>
        </div>
        <div v-if="upcomingClasses.length === 0" class="admin-insight-empty">
          <p class="text-[13px] font-semibold" style="color: var(--admin-text);">Không có lớp sắp khai giảng</p>
          <p class="text-xs mt-0.5" style="color: var(--admin-text-muted);">Mọi lớp hiện tại đã bắt đầu hoặc kết thúc.</p>
        </div>
        <div v-else class="admin-insight-card-body">
          <div
            v-for="(item, idx) in upcomingClasses"
            :key="item.id"
            class="admin-insight-list-item"
          >
            <div class="admin-insight-rank">{{ idx + 1 }}</div>
            <div class="flex-1 min-w-0">
              <p class="admin-insight-name admin-cell-ellipsis">{{ item.className || 'Lớp học' }}</p>
              <p class="admin-insight-meta admin-cell-ellipsis">
                <span class="font-medium">{{ shortDateVN(item.startDate) }}</span>
                <span v-if="item.courseNameSnapshot" class="opacity-70"> • {{ item.courseNameSnapshot }}</span>
              </p>
              <p
                v-if="item.teacherNameSnapshot || item.room"
                class="admin-insight-meta admin-cell-ellipsis"
                style="opacity: 0.85;"
              >
                <span v-if="item.teacherNameSnapshot">GV: {{ item.teacherNameSnapshot }}</span>
                <span v-if="item.teacherNameSnapshot && item.room" class="mx-1 opacity-50">•</span>
                <span v-if="item.room">Phòng {{ item.room }}</span>
              </p>
            </div>
            <ClockCircleOutlined style="color: var(--admin-text-subtle); font-size: 14px;" />
          </div>
        </div>
      </div>

      <!-- Học viên còn nợ học phí -->
      <div class="admin-insight-card">
        <div class="admin-insight-card-header">
          <h2 class="admin-section-title">Học viên còn nợ học phí</h2>
          <router-link to="/admin/tuition" class="text-xs" style="color: var(--admin-accent);">Tất cả →</router-link>
        </div>
        <div v-if="topDebtors.length === 0" class="admin-insight-empty">
          <p class="text-sm font-semibold" style="color: var(--admin-text);">Tuyệt vời!</p>
          <p class="text-xs mt-1" style="color: var(--admin-text-muted);">Hiện không có khoản công nợ nào.</p>
        </div>
        <div v-else class="admin-insight-card-body">
          <div
            v-for="(item, idx) in topDebtors"
            :key="item.studentId || item.name"
            class="admin-insight-list-item"
          >
            <div class="admin-insight-rank">{{ idx + 1 }}</div>
            <div class="flex-1 min-w-0">
              <p class="admin-insight-name admin-cell-ellipsis">{{ item.name }}</p>
              <p class="admin-insight-meta admin-cell-ellipsis">
                Công nợ hiện tại
                <span v-if="item.courseNameSnapshot" class="opacity-70"> • {{ item.courseNameSnapshot }}</span>
                <span v-else-if="item.classNameSnapshot" class="opacity-70"> • {{ item.classNameSnapshot }}</span>
              </p>
            </div>
            <span class="admin-insight-amount danger">{{ formatVnd(item.totalDebt) }}</span>
          </div>
        </div>
      </div>

      <!-- Hoạt động gần đây -->
      <div class="admin-insight-card lg:col-span-2 xl:col-span-1">
        <div class="admin-insight-card-header">
          <h2 class="admin-section-title">Hoạt động gần đây</h2>
          <router-link to="/admin/payments" class="text-xs" style="color: var(--admin-accent);">Xem tất cả →</router-link>
        </div>
        <div v-if="recentActivities.length === 0" class="admin-insight-empty">
          <div class="admin-insight-empty-icon">
            <ClockCircleOutlined style="font-size: 18px;" />
          </div>
          <p class="text-sm font-semibold" style="color: var(--admin-text);">Chưa có hoạt động nào</p>
          <p class="text-xs mt-1" style="color: var(--admin-text-muted);">Thanh toán và ghi danh mới sẽ xuất hiện tại đây.</p>
        </div>
        <div v-else class="admin-insight-card-body">
          <div
            v-for="item in recentActivities"
            :key="item.key"
            class="admin-insight-list-item"
          >
            <div
              class="admin-insight-icon"
              :style="{ background: item.bg, color: item.color, width: '36px', height: '36px', borderRadius: '10px', display: 'flex', alignItems: 'center', justifyContent: 'center' }"
            >
              <component :is="item.icon" style="font-size: 16px;" />
            </div>
            <div class="flex-1 min-w-0">
              <p class="admin-insight-name admin-cell-ellipsis" style="font-size: 13px;">{{ item.title }}</p>
              <p class="admin-insight-meta admin-cell-ellipsis">{{ item.sub }}</p>
              <p class="admin-insight-meta" style="margin-top: 2px; opacity: 0.7;">{{ item.time }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Chart row: revenue (lớn) + class status (compact) -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-4 mt-4">
      <!-- Revenue chart (2/3) -->
      <div class="admin-chart-card xl:col-span-2">
        <div class="admin-chart-card-header">
          <div>
            <h2 class="admin-section-title">Xu hướng doanh thu</h2>
            <p class="text-xs mt-1" style="color: var(--admin-text-muted);">
              Tổng doanh thu từ giao dịch thành công theo khoảng thời gian đã chọn.
            </p>
          </div>
          <div class="admin-period-pills">
            <button
              v-for="opt in periodOptions"
              :key="opt.value"
              :class="{ 'is-active': revenuePeriod === opt.value }"
              @click="setRevenuePeriod(opt.value)"
            >
              {{ opt.label }}
            </button>
          </div>
        </div>
        <div :class="revenueTrendData.labels.length ? 'h-56 flex items-center justify-center' : 'py-2'">
          <Line
            v-if="revenueTrendData.labels.length"
            :data="revenueTrendData"
            :options="lineChartOptions"
          />
          <div v-else class="text-center max-w-sm mx-auto">
            <div class="mx-auto w-12 h-12 rounded-full flex items-center justify-center mb-3" style="background: var(--admin-surface-2); color: var(--admin-text-subtle);">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
                <line x1="12" y1="20" x2="12" y2="10" />
                <line x1="18" y1="20" x2="18" y2="4" />
                <line x1="6" y1="20" x2="6" y2="16" />
              </svg>
            </div>
            <p class="text-sm font-semibold" style="color: var(--admin-text);">Chưa có giao dịch thành công</p>
            <p class="text-xs mt-1" style="color: var(--admin-text-muted);">Thử chọn khung thời gian rộng hơn để xem dữ liệu.</p>
            <div class="flex items-center justify-center gap-2 mt-3">
              <button
                v-if="revenuePeriod !== '30d'"
                type="button"
                class="admin-btn admin-btn-secondary h-8 px-3 text-xs"
                @click="setRevenuePeriod('30d')"
              >
                Xem 30 ngày
              </button>
              <button
                v-if="revenuePeriod !== '6m'"
                type="button"
                class="admin-btn admin-btn-secondary h-8 px-3 text-xs"
                @click="setRevenuePeriod('6m')"
              >
                Xem 6 tháng
              </button>
              <button
                v-if="revenuePeriod !== '12m'"
                type="button"
                class="admin-btn admin-btn-secondary h-8 px-3 text-xs"
                @click="setRevenuePeriod('12m')"
              >
                Xem 12 tháng
              </button>
            </div>
          </div>
        </div>
        <div v-if="revenueInsight" class="admin-insight">
          <div class="admin-insight-icon">
            <ArrowUpOutlined v-if="revenueInsight.direction === 'up'" />
            <ArrowDownOutlined v-else-if="revenueInsight.direction === 'down'" />
            <MinusOutlined v-else />
          </div>
          <div>
            <span class="font-semibold">{{ revenueInsight.headline }}</span>
            <span class="ml-1" style="color: var(--admin-text-muted);">{{ revenueInsight.detail }}</span>
          </div>
        </div>
      </div>

      <!-- Class status (1/3) -->
      <div class="admin-chart-card">
        <div class="admin-chart-card-header">
          <h2 class="admin-section-title">Trạng thái lớp học</h2>
        </div>
        <div :class="hasClassStatusData ? 'h-44 flex items-center justify-center' : 'py-2'">
          <Doughnut v-if="hasClassStatusData" :data="classStatusChartData" :options="doughnutChartOptions" />
          <div v-else class="text-center px-4 py-3">
            <p class="text-sm font-semibold" style="color: var(--admin-text);">Chưa có dữ liệu trạng thái lớp học</p>
            <p class="text-xs mt-1" style="color: var(--admin-text-muted);">Dữ liệu sẽ hiển thị khi lớp học được cập nhật trạng thái.</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import {
  DollarCircleOutlined,
  TeamOutlined,
  UserOutlined,
  BookOutlined,
  CalendarOutlined,
  SyncOutlined,
  ArrowUpOutlined,
  ArrowDownOutlined,
  MinusOutlined,
  PayCircleOutlined,
  CheckCircleOutlined,
  CreditCardOutlined,
  RiseOutlined,
  ClockCircleOutlined,
  FileTextOutlined
} from '@ant-design/icons-vue'
import { reportApi } from '@/api/reportApi'
import { studentApi } from '@/api/studentApi'
import { courseApi } from '@/api/courseApi'
import { classApi } from '@/api/classApi'
import { paymentApi } from '@/api/paymentApi'
import { enrollmentApi } from '@/api/enrollmentApi'
import { formatVnd, formatRelative, formatDate, shortInvoiceCode, shortDateVN } from '@/lib/formatters'
import PageHeader from '@/components/ui/PageHeader.vue'

import { Bar, Doughnut, Line } from 'vue-chartjs'
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
  ArcElement,
  PointElement,
  LineElement
} from 'chart.js'

ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale, ArcElement, PointElement, LineElement)

// ============ State ============
const globalLoading = ref(false)
const lastUpdated = ref('')
const revenuePeriod = ref('6m')

const periodOptions = [
  { value: '7d', label: '7 ngày' },
  { value: '30d', label: '30 ngày' },
  { value: '6m', label: '6 tháng' },
  { value: '12m', label: '12 tháng' }
]

const todayLabel = computed(() => {
  const d = new Date()
  return d.toLocaleDateString('vi-VN', { weekday: 'long', day: '2-digit', month: 'long', year: 'numeric' })
})

const dashboardData = ref(null)
const studentsList = ref([])
const coursesList = ref([])
const classesList = ref([])
const paymentsList = ref([])
const enrollmentsList = ref([])
const debtList = ref([])

const overviewData = computed(() => dashboardData.value?.overview || {})

const activeStudentsCount = computed(() =>
  studentsList.value.filter(s => Number(s.status) === 1).length
)

const activeClassesCount = computed(() =>
  classesList.value.filter(c => Number(c.status) === 2).length
)

const totalDebtAmount = computed(() => {
  const v = overviewData.value?.totalDebt
  if (typeof v === 'number') return v
  return debtList.value.reduce((sum, d) => sum + parseFloat(d.totalDebt || 0), 0)
})

const totalRevenueAmount = computed(() => {
  const v = overviewData.value?.totalRevenue
  if (typeof v === 'number') return v
  return 0
})

const tuitionRate = computed(() => {
  const total = totalRevenueAmount.value + totalDebtAmount.value
  if (total === 0) return 0
  return Math.round((totalRevenueAmount.value / total) * 100)
})

// ============ KPI Hero Cards (4 lớn) ============
// Ưu tiên: KHÔNG dùng "—", chỉ dùng khi thật sự 0. Khi 0 thì value = "Chưa có dữ liệu"
// (nhỏ + muted), sub vẫn hiển thị rõ ý nghĩa
const kpiHeroes = computed(() => {
  const hasStudents = studentsList.value.length > 0
  const hasCourses = coursesList.value.length > 0
  const hasRevenue = totalRevenueAmount.value > 0
  const hasDebt = totalDebtAmount.value > 0
  const hasTuition = (totalRevenueAmount.value + totalDebtAmount.value) > 0

  return [
    {
      key: 'students',
      label: 'Tổng học viên',
      value: hasStudents ? studentsList.value.length : 'Chưa có dữ liệu',
      placeholder: !hasStudents,
      sub: hasStudents
        ? `${studentsList.value.length} hồ sơ học viên trong hệ thống`
        : 'Chưa có hồ sơ học viên nào',
      pill: hasStudents && activeStudentsCount.value > 0
        ? { direction: 'up', text: 'Đang hoạt động' }
        : null,
      icon: TeamOutlined,
      iconColor: 'indigo',
      accent: true
    },
    {
      key: 'revenue',
      label: 'Doanh thu',
      value: hasRevenue ? formatVnd(totalRevenueAmount.value) : 'Chưa có dữ liệu',
      placeholder: !hasRevenue,
      sub: hasRevenue
        ? 'Từ các giao dịch thanh toán thành công'
        : 'Chưa có giao dịch thành công nào được ghi nhận',
      pill: hasRevenue ? { direction: 'up', text: 'Thành công' } : null,
      icon: DollarCircleOutlined,
      iconColor: 'emerald',
      accent: false
    },
    {
      key: 'tuition',
      label: 'Tỷ lệ thu học phí',
      value: hasTuition ? `${tuitionRate.value}%` : 'Chưa có dữ liệu',
      placeholder: !hasTuition,
      sub: hasTuition
        ? `${formatVnd(totalRevenueAmount.value)} đã thu trên tổng ${formatVnd(totalRevenueAmount.value + totalDebtAmount.value)} phải thu`
        : 'Chờ dữ liệu học phí để tính tỷ lệ',
      pill: hasTuition
        ? (tuitionRate.value >= 80
            ? { direction: 'up', text: 'Tốt' }
            : tuitionRate.value >= 50
              ? { direction: 'neutral', text: 'Trung bình' }
              : { direction: 'down', text: 'Cần chú ý' })
        : null,
      icon: RiseOutlined,
      iconColor: 'sky',
      accent: false
    },
    {
      key: 'debt',
      label: 'Công nợ',
      value: hasDebt ? formatVnd(totalDebtAmount.value) : 'Chưa có dữ liệu',
      placeholder: false,
      sub: hasDebt
        ? 'Học phí chưa được thanh toán'
        : 'Tuyệt vời, không có khoản nợ nào',
      pill: hasDebt ? { direction: 'down', text: 'Cần thu hồi' } : { direction: 'up', text: 'Sạch nợ' },
      icon: PayCircleOutlined,
      iconColor: 'rose',
      accent: false
    }
  ]
})

// ============ KPI Mini Cards (4 nhỏ — phụ) ============
const kpiMinis = computed(() => [
  {
    key: 'courses',
    label: 'Khóa học',
    value: coursesList.value.length,
    sub: coursesList.value.length > 0 ? `${coursesList.value.length} chương trình đào tạo` : 'Chưa có dữ liệu',
    icon: BookOutlined,
    iconColor: 'violet'
  },
  {
    key: 'classes',
    label: 'Lớp đang diễn ra',
    value: activeClassesCount.value,
    sub: activeClassesCount.value > 0
      ? `${activeClassesCount.value} lớp đang diễn ra`
      : 'Chưa có lớp nào đang diễn ra',
    icon: CalendarOutlined,
    iconColor: 'amber'
  },
  {
    key: 'enrollments',
    label: 'Lượt ghi danh',
    value: enrollmentsList.value.length,
    sub: enrollmentsList.value.length > 0
      ? `${enrollmentsList.value.length} lượt ghi danh được ghi nhận`
      : 'Chưa có lượt ghi danh',
    icon: FileTextOutlined,
    iconColor: 'sky'
  },
  {
    key: 'active-students',
    label: 'Học viên đang học',
    value: activeStudentsCount.value,
    sub: activeStudentsCount.value > 0
      ? `${activeStudentsCount.value} học viên đang theo học`
      : 'Chưa có học viên đang theo học',
    icon: UserOutlined,
    iconColor: 'emerald'
  }
])

// ============ Revenue trend chart ============
function groupKey(date, period) {
  const d = new Date(date)
  if (period === '7d' || period === '30d') {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
  }
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`
}

function formatLabel(key, period) {
  if (period === '7d' || period === '30d') {
    const [y, m, d] = key.split('-')
    return `${d}/${m}`
  }
  const [y, m] = key.split('-')
  return `T${m}/${y.slice(2)}`
}

const cutoffDate = computed(() => {
  const d = new Date()
  if (revenuePeriod.value === '7d') d.setDate(d.getDate() - 7)
  else if (revenuePeriod.value === '30d') d.setDate(d.getDate() - 30)
  else if (revenuePeriod.value === '6m') d.setMonth(d.getMonth() - 6)
  else d.setMonth(d.getMonth() - 12)
  return d
})

function isSuccessPayment(payment) {
  return Number(payment?.status) === 1 || payment?.status === 'Success'
}

const revenueTrendData = computed(() => {
  const successfulPayments = paymentsList.value
    .filter(isSuccessPayment)
    .filter(p => p.paymentDate && new Date(p.paymentDate) >= cutoffDate.value)
    .sort((a, b) => new Date(a.paymentDate) - new Date(b.paymentDate))

  const grouped = {}
  successfulPayments.forEach(pay => {
    const key = groupKey(pay.paymentDate, revenuePeriod.value)
    grouped[key] = (grouped[key] || 0) + parseFloat(pay.amount || 0)
  })

  const keys = Object.keys(grouped).sort()
  return {
    labels: keys.map(k => formatLabel(k, revenuePeriod.value)),
    datasets: [
      {
        label: 'Doanh thu',
        borderColor: '#2563eb',
        backgroundColor: 'rgba(37, 99, 235, 0.08)',
        fill: true,
        tension: 0.35,
        borderWidth: 2,
        pointBackgroundColor: '#2563eb',
        pointRadius: 3,
        pointHoverRadius: 5,
        data: keys.map(k => grouped[k])
      }
    ]
  }
})

const revenueInsight = computed(() => {
  const data = revenueTrendData.value.datasets[0].data
  if (data.length < 2) return null
  const last = data[data.length - 1]
  const prev = data[data.length - 2]
  if (!prev) return null
  const diff = last - prev
  if (diff === 0) {
    return {
      direction: 'neutral',
      headline: 'Doanh thu ổn định',
      detail: `Bằng với kỳ trước (${formatVnd(prev)}).`
    }
  }
  const pct = Math.round((diff / prev) * 100)
  return {
    direction: diff > 0 ? 'up' : 'down',
    headline: diff > 0 ? `Tăng ${Math.abs(pct)}% so với kỳ trước` : `Giảm ${Math.abs(pct)}% so với kỳ trước`,
    detail: `Kỳ này đạt ${formatVnd(last)}, kỳ trước ${formatVnd(prev)}.`
  }
})

const lineChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false },
    tooltip: {
      callbacks: {
        label: (context) => `Doanh thu: ${formatVnd(context.parsed.y)}`
      }
    }
  },
  scales: {
    x: { grid: { display: false } },
    y: {
      grid: { color: 'rgba(0, 0, 0, 0.04)' },
      ticks: { callback: (value) => formatVnd(value) }
    }
  }
}

const doughnutChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: true,
      position: 'bottom',
      labels: {
        boxWidth: 8,
        padding: 10,
        font: { size: 10 }
      }
    }
  }
}

const horizontalBarChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  indexAxis: 'y',
  plugins: { legend: { display: false } },
  scales: {
    x: { grid: { color: 'rgba(0, 0, 0, 0.04)' }, ticks: { precision: 0 } },
    y: { grid: { display: false } }
  }
}

// Chỉ hiển thị chart khi CÓ data trong tất cả status; nếu chỉ 1 status = 0 mà các status khác = 0
// thì tổng vẫn bằng 0 → empty. Điều này tránh chart trống hiển thị legend.
const hasClassStatusData = computed(() => {
  if (!classesList.value.length) return false
  return classStatusChartData.value.datasets[0].data.some(v => v > 0)
})
const classStatusChartData = computed(() => {
  const statusCounts = { 0: 0, 1: 0, 2: 0, 3: 0, 4: 0 }
  classesList.value.forEach(cls => {
    const s = Number(cls.status)
    if (s in statusCounts) statusCounts[s]++
  })
  return {
    labels: ['Đang mở', 'Đã đầy', 'Đang học', 'Hoàn thành', 'Đã hủy'],
    datasets: [{
      data: [statusCounts[0], statusCounts[1], statusCounts[2], statusCounts[3], statusCounts[4]],
      backgroundColor: ['#3b82f6', '#64748b', '#6366f1', '#10b981', '#ef4444'],
      borderWidth: 0
    }]
  }
})

const studentsByCourseChartData = computed(() => {
  const counts = {}
  enrollmentsList.value.forEach(en => {
    const name = en.courseNameSnapshot || 'Khác'
    counts[name] = (counts[name] || 0) + 1
  })
  const sorted = Object.entries(counts).sort((a, b) => b[1] - a[1]).slice(0, 6)
  return {
    labels: sorted.map(s => s[0]),
    datasets: [{
      label: 'Số lượng học viên',
      backgroundColor: '#8b5cf6',
      borderRadius: 4,
      data: sorted.map(s => s[1])
    }]
  }
})

// ============ Upcoming classes (từ classesList thật) ============
const upcomingClasses = computed(() => {
  const now = new Date()
  return [...classesList.value]
    .filter(c => (Number(c.status) === 0 || Number(c.status) === 1) && c.startDate && new Date(c.startDate) > now)
    .sort((a, b) => new Date(a.startDate) - new Date(b.startDate))
    .slice(0, 5)
})

// ============ Top debtors (từ reportApi.getDebtByStudent) ============
const topDebtors = computed(() => {
  return [...debtList.value]
    .filter(d => parseFloat(d.totalDebt || 0) > 0)
    .sort((a, b) => parseFloat(b.totalDebt || 0) - parseFloat(a.totalDebt || 0))
    .slice(0, 5)
    .map(d => ({
      ...d,
      courseNameSnapshot: d.courseNameSnapshot || d.classNameSnapshot || null
    }))
})

// ============ Activity feed (chỉ từ dữ liệu thật, câu tự nhiên) ============
const recentActivities = computed(() => {
  const items = []

  // Payments — dòng chính: "Ghi nhận thanh toán X", dòng phụ: "Hóa đơn INV-XXXX • 21/01/2026"
  paymentsList.value.filter(isSuccessPayment).slice(0, 6).forEach(p => {
    if (!p.paymentDate) return
    const amount = parseFloat(p.amount || 0)
    if (!amount) return
    const ref = shortInvoiceCode(p.invoiceCode || p.invoiceId)
    const subParts = []
    if (ref) subParts.push(`Hóa đơn ${ref}`)
    subParts.push(shortDateVN(p.paymentDate))
    items.push({
      key: `pay-${p.id}`,
      title: `Ghi nhận thanh toán ${formatVnd(amount)}`,
      sub: subParts.join(' • '),
      time: formatRelative(p.paymentDate),
      date: new Date(p.paymentDate),
      icon: CreditCardOutlined,
      color: '#10b981',
      bg: 'rgba(16, 185, 129, 0.12)'
    })
  })

  // Enrollments — dòng chính: tên học viên, dòng phụ: lớp/khóa học + ngày
  enrollmentsList.value.slice(0, 6).forEach(en => {
    if (!en.enrolledAt) return
    const name = en.studentNameSnapshot || 'Học viên'
    const course = en.courseNameSnapshot || null
    const cls = en.classNameSnapshot || null
    const target = cls ? `Lớp ${cls}` : (course ? `Khóa ${course}` : 'Khóa học')
    items.push({
      key: `enr-${en.id}`,
      title: `${name} ghi danh`,
      sub: `${target} • ${shortDateVN(en.enrolledAt)}`,
      time: formatRelative(en.enrolledAt),
      date: new Date(en.enrolledAt),
      icon: FileTextOutlined,
      color: '#6366f1',
      bg: 'rgba(99, 102, 241, 0.12)'
    })
  })

  return items
    .sort((a, b) => b.date - a.date)
    .slice(0, 8)
})

// ============ Fetch ============
async function fetchData() {
  globalLoading.value = true
  try {
    const promises = [
      reportApi.getDashboard().then(d => { dashboardData.value = d }).catch(() => {}),
      studentApi.getAll().then(d => { studentsList.value = d?.items || d?.data || d || [] }).catch(() => {}),
      courseApi.getAll().then(d => { coursesList.value = d?.items || d?.data || d || [] }).catch(() => {}),
      classApi.getAll().then(d => { classesList.value = d?.items || d?.data || d || [] }).catch(() => {}),
      paymentApi.getAll().then(d => { paymentsList.value = d?.items || d?.data || d || [] }).catch(() => {}),
      enrollmentApi.getAll().then(d => { enrollmentsList.value = d?.items || d?.data || d || [] }).catch(() => {}),
      reportApi.getDebtByStudent().then(d => { debtList.value = d || [] }).catch(() => {})
    ]
    await Promise.all(promises)
    lastUpdated.value = new Date().toLocaleTimeString('vi-VN')
  } catch (e) {
    // silent — UI handles empty states
  } finally {
    globalLoading.value = false
  }
}

function setRevenuePeriod(value) {
  revenuePeriod.value = value
}

onMounted(fetchData)
</script>
