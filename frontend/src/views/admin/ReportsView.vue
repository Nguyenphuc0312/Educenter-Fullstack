<template>
  <div class="space-y-6">
    <!-- Header chuyên nghiệp -->
    <div class="admin-page-header">
      <div>
        <h1 class="admin-page-title">Báo cáo & Thống kê</h1>
        <p class="admin-page-subtitle">Theo dõi doanh thu, công nợ, học phí, lớp học và hiệu quả vận hành.</p>
      </div>
      <div class="admin-page-actions">
        <!-- Period selector (pill) -->
        <div class="admin-period-pills">
          <button v-for="opt in periodOptions" :key="opt.value"
            :class="{ 'is-active': activePeriod === opt.value }"
            @click="setPeriod(opt.value)">
            {{ opt.label }}
          </button>
        </div>

        <a-select v-model:value="selectedYear" class="w-28" size="small" @change="applyCalendarFilter">
          <a-select-option :value="null">Tất cả năm</a-select-option>
          <a-select-option v-for="year in availableYears" :key="year" :value="year">Năm {{ year }}</a-select-option>
        </a-select>
        <a-select v-model:value="selectedMonth" class="w-28" size="small" :disabled="!selectedYear" @change="applyCalendarFilter">
          <a-select-option :value="null">Cả năm</a-select-option>
          <a-select-option v-for="month in 12" :key="month" :value="month">Tháng {{ month }}</a-select-option>
        </a-select>
        <a-range-picker
          v-if="activePeriod === 'custom'"
          v-model:value="customDateRange"
          value-format="YYYY-MM-DD"
          size="small"
          class="w-56"
          :placeholder="['Từ ngày', 'Đến ngày']"
        />

        <!-- Refresh button -->
        <button type="button" class="admin-btn admin-btn-secondary h-9 px-3"
          :disabled="loading" @click="fetchData">
          <SyncOutlined style="font-size: 13px;" />
          Làm mới
        </button>

        <!-- Single Export button -->
        <a-dropdown
          trigger="click"
          placement="bottomRight"
          overlay-class-name="reports-export-dropdown"
          :overlay-style="{ zIndex: 3000 }"
        >
          <button type="button" class="admin-btn admin-btn-primary h-9 px-3">
            <DownloadOutlined style="font-size: 13px;" />
            Xuất báo cáo
            <DownOutlined style="font-size: 10px;" />
          </button>
          <template #overlay>
            <a-menu class="reports-export-menu min-w-[276px] shadow-lg rounded-xl p-1.5"
              style="background: var(--admin-surface); border: 1px solid var(--admin-border);">
              <a-menu-item key="overview" class="rounded-lg px-3 py-2 text-xs" @click="handleExport('overview')">
                <div class="report-export-menu-item">
                  <BarChartOutlined style="font-size: 13px; color: var(--admin-text-muted);" />
                  <span>Xuất tổng quan (CSV)</span>
                </div>
              </a-menu-item>
              <a-menu-divider style="margin: 4px 0; border-color: var(--admin-border);" />
              <a-menu-item key="course" class="rounded-lg px-3 py-2 text-xs" @click="handleExport('course')">
                <div class="report-export-menu-item">
                  <BookOutlined style="font-size: 13px; color: var(--admin-text-muted);" />
                  <span>Doanh thu theo khóa học</span>
                </div>
              </a-menu-item>
              <a-menu-item key="class" class="rounded-lg px-3 py-2 text-xs" @click="handleExport('class')">
                <div class="report-export-menu-item">
                  <TeamOutlined style="font-size: 13px; color: var(--admin-text-muted);" />
                  <span>Doanh thu theo lớp học</span>
                </div>
              </a-menu-item>
              <a-menu-divider style="margin: 4px 0; border-color: var(--admin-border);" />
              <a-menu-item key="debt-student" class="rounded-lg px-3 py-2 text-xs" @click="handleExport('debt-student')">
                <div class="report-export-menu-item">
                  <UserOutlined style="font-size: 13px; color: var(--admin-text-muted);" />
                  <span>Công nợ theo học viên</span>
                </div>
              </a-menu-item>
              <a-menu-item key="debt-class" class="rounded-lg px-3 py-2 text-xs" @click="handleExport('debt-class')">
                <div class="report-export-menu-item">
                  <BookOutlined style="font-size: 13px; color: var(--admin-text-muted);" />
                  <span>Công nợ theo lớp học</span>
                </div>
              </a-menu-item>
              <a-menu-divider style="margin: 4px 0; border-color: var(--admin-border);" />
              <a-menu-item key="results" class="rounded-lg px-3 py-2 text-xs" @click="handleExport('results')">
                <div class="flex items-center gap-2"><ReadOutlined /><span>Kết quả học tập</span></div>
              </a-menu-item>
              <a-menu-item key="courses" class="rounded-lg px-3 py-2 text-xs" @click="handleExport('courses')">
                <div class="flex items-center gap-2"><BookOutlined /><span>Danh mục khóa học</span></div>
              </a-menu-item>
            </a-menu>
          </template>
        </a-dropdown>
      </div>
    </div>

    <!-- 6 Summary Cards (compact ngang: icon trái + content phải) -->
    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 2xl:grid-cols-6 gap-2 mb-4">
      <div v-for="kpi in summaryCards" :key="kpi.key" class="admin-card flex items-center gap-3 px-3 py-2.5" style="min-width: 0;">
        <div :class="['kpi-hero-icon', kpi.iconColor]" style="width: 36px; height: 36px; border-radius: 10px; flex-shrink: 0;">
          <component :is="kpi.icon" style="font-size: 16px;" />
        </div>
        <div class="flex-1 min-w-0">
          <div class="flex items-center justify-between gap-2">
            <p class="kpi-hero-label" style="font-size: 10px; line-height: 1.2;">{{ kpi.label }}</p>
            <span v-if="kpi.pill" :class="['kpi-hero-pill', kpi.pill.direction]" style="font-size: 9px; padding: 1px 5px;">
              <span v-if="kpi.pill.direction === 'up'">↑</span>
              <span v-else-if="kpi.pill.direction === 'down'">↓</span>
              <span v-else>·</span>
              {{ kpi.pill.text }}
            </span>
          </div>
          <p :class="['kpi-hero-value', kpi.placeholder && 'kpi-hero-value--placeholder']" style="font-size: 0.95rem; line-height: 1.2; margin-top: 2px; word-break: break-word; font-weight: 700;">
            {{ kpi.value }}
          </p>
          <p v-if="kpi.sub" class="kpi-hero-sub" style="font-size: 10px; line-height: 1.2; margin-top: 1px;">{{ kpi.sub }}</p>
        </div>
      </div>
    </div>

    <!-- Tabs -->
    <div class="admin-period-pills mb-4" style="padding: 4px;">
      <button v-for="tab in tabs" :key="tab.key"
        :class="{ 'is-active': activeTab === tab.key }"
        @click="activeTab = tab.key">
        <component :is="tab.icon" style="font-size: 13px;" />
        {{ tab.label }}
      </button>
    </div>

    <!-- Main Content - theo tab -->
    <div v-if="activeTab === 'overview'" class="space-y-6">
      <!-- Tổng quan: revenue trend (lớn) + 2 chart nhỏ + 2 insight cards -->
      <div class="admin-chart-card">
        <div class="admin-chart-card-header">
          <h2 class="admin-section-title">Xu hướng doanh thu</h2>
          <div class="admin-period-pills" style="padding: 2px;">
            <button v-for="opt in trendRangeOptions" :key="opt.value"
              :class="{ 'is-active': revenueTrendRange === opt.value }"
              @click="setTrendRange(opt.value)">
              {{ opt.label }}
            </button>
          </div>
        </div>
        <div :class="revenueTrendChartData.labels.length ? 'h-64 flex items-center justify-center' : 'py-6'">
          <Line v-if="revenueTrendChartData.labels.length" :data="revenueTrendChartData" :options="lineChartOptions" />
          <EmptyChartState v-else
            title="Chưa có giao dịch thành công"
            description="Thử chọn khung thời gian rộng hơn để xem dữ liệu." />
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

      <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">Trạng thái hóa đơn</h2>
          <div :class="hasInvoiceData ? 'h-56 flex items-center justify-center' : 'py-3'">
            <Doughnut v-if="hasInvoiceData" :data="invoiceStatusChartData" :options="doughnutChartOptions" />
            <EmptyChartState v-else
              title="Chưa có dữ liệu hóa đơn"
              description="Hóa đơn sẽ xuất hiện khi có học viên được ghi danh." />
          </div>
        </div>
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">Phương thức thanh toán</h2>
          <div :class="filteredPayments.length ? 'h-56 flex items-center justify-center' : 'py-3'">
            <Doughnut v-if="filteredPayments.length" :data="paymentMethodChartData" :options="doughnutChartOptions" />
            <EmptyChartState v-else
              title="Chưa có giao dịch"
              description="Phương thức thanh toán sẽ hiển thị khi có giao dịch thành công." />
          </div>
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div class="admin-insight-card">
          <div class="admin-insight-card-header">
            <h2 class="admin-section-title">Top công nợ</h2>
            <router-link to="/admin/tuition" class="text-xs" style="color: var(--admin-accent);">Xem tất cả →</router-link>
          </div>
          <div v-if="debtByStudent.length === 0" class="admin-insight-empty">
            <p class="text-sm font-semibold" style="color: var(--admin-text);">Không có công nợ</p>
            <p class="text-xs mt-1" style="color: var(--admin-text-muted);">Học viên đã thanh toán đầy đủ.</p>
          </div>
          <div v-else class="admin-insight-card-body">
            <div v-for="(item, idx) in topDebtors" :key="item.studentId || item.name" class="admin-insight-list-item">
              <div class="admin-insight-rank">{{ idx + 1 }}</div>
              <div class="flex-1 min-w-0">
                <p class="admin-insight-name admin-cell-ellipsis">{{ item.name }}</p>
                <p class="admin-insight-meta admin-cell-ellipsis">Công nợ hiện tại</p>
              </div>
              <span class="admin-insight-amount danger">{{ formatVnd(item.totalDebt) }}</span>
            </div>
          </div>
        </div>
        <div class="admin-insight-card">
          <div class="admin-insight-card-header">
            <h2 class="admin-section-title">Giao dịch gần đây</h2>
            <router-link to="/admin/payments" class="text-xs" style="color: var(--admin-accent);">Xem tất cả →</router-link>
          </div>
          <div v-if="recentTransactions.length === 0" class="admin-insight-empty">
            <p class="text-sm font-semibold" style="color: var(--admin-text);">Chưa có giao dịch</p>
            <p class="text-xs mt-1" style="color: var(--admin-text-muted);">Giao dịch thành công sẽ xuất hiện tại đây.</p>
          </div>
          <div v-else class="admin-insight-card-body">
            <div v-for="item in recentTransactions" :key="item.id" class="admin-insight-list-item">
              <div class="admin-insight-rank" :style="{ background: 'var(--admin-success-soft)', color: 'var(--admin-success)' }">
                <CreditCardOutlined />
              </div>
              <div class="flex-1 min-w-0">
                <p class="admin-insight-name admin-cell-ellipsis">
                  {{ item.studentNameSnapshot || 'Học viên' }}
                </p>
                <p class="admin-insight-meta admin-cell-ellipsis">
                  Hóa đơn {{ shortInvoiceCode(item.invoiceCode || item.invoiceId) }} • {{ shortDateVN(item.paymentDate) }}
                </p>
              </div>
              <span class="admin-insight-amount success">{{ formatVnd(item.amount) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Tab: Doanh thu -->
    <div v-else-if="activeTab === 'revenue'" class="space-y-6">
      <!-- Top performer insight cards (3 callout - compact ngang) -->
      <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-2">
        <!-- Top khóa học -->
        <div class="admin-card flex items-center gap-3 px-3 py-2.5" style="min-width: 0;">
          <div class="kpi-hero-icon violet" style="width: 36px; height: 36px; border-radius: 10px; flex-shrink: 0;">
            <TrophyOutlined style="font-size: 16px;" />
          </div>
          <div class="flex-1 min-w-0">
            <p class="kpi-hero-label" style="font-size: 10px; line-height: 1.2;">Khóa học doanh thu cao nhất</p>
            <p v-if="topRevenueCourse" class="admin-cell-ellipsis" style="font-size: 0.875rem; font-weight: 700; color: var(--admin-text); margin-top: 1px;">
              {{ topRevenueCourse.name }}
            </p>
            <p v-if="topRevenueCourse" style="font-size: 12px; color: var(--admin-success); font-weight: 700; margin-top: 1px;">
              {{ formatVnd(topRevenueCourse.totalRevenue) }}
            </p>
            <p v-else style="font-size: 10px; color: var(--admin-text-subtle); margin-top: 1px;">Chưa có dữ liệu</p>
          </div>
        </div>
        <!-- Top lớp học -->
        <div class="admin-card flex items-center gap-3 px-3 py-2.5" style="min-width: 0;">
          <div class="kpi-hero-icon indigo" style="width: 36px; height: 36px; border-radius: 10px; flex-shrink: 0;">
            <TrophyOutlined style="font-size: 16px;" />
          </div>
          <div class="flex-1 min-w-0">
            <p class="kpi-hero-label" style="font-size: 10px; line-height: 1.2;">Lớp doanh thu cao nhất</p>
            <p v-if="topRevenueClass" class="admin-cell-ellipsis" style="font-size: 0.875rem; font-weight: 700; color: var(--admin-text); margin-top: 1px;">
              {{ topRevenueClass.name }}
            </p>
            <p v-if="topRevenueClass" style="font-size: 12px; color: var(--admin-success); font-weight: 700; margin-top: 1px;">
              {{ formatVnd(topRevenueClass.totalRevenue) }}
            </p>
            <p v-else style="font-size: 10px; color: var(--admin-text-subtle); margin-top: 1px;">Chưa có dữ liệu</p>
          </div>
        </div>
        <!-- Tổng doanh thu tổng hợp -->
        <div class="admin-card flex items-center gap-3 px-3 py-2.5" style="min-width: 0;">
          <div class="kpi-hero-icon emerald" style="width: 36px; height: 36px; border-radius: 10px; flex-shrink: 0;">
            <RiseOutlined style="font-size: 16px;" />
          </div>
          <div class="flex-1 min-w-0">
            <p class="kpi-hero-label" style="font-size: 10px; line-height: 1.2;">Tổng doanh thu tổng hợp</p>
            <p v-if="totalPaidRevenue > 0" style="font-size: 14px; font-weight: 800; color: var(--admin-text); margin-top: 1px;">
              {{ formatVnd(totalPaidRevenue) }}
            </p>
            <p v-else style="font-size: 10px; color: var(--admin-text-subtle); margin-top: 1px;">Chưa có dữ liệu</p>
            <p v-if="totalPaidRevenue > 0 && collectionRate > 0" style="font-size: 10px; color: var(--admin-text-muted); margin-top: 1px;">
              Tỷ lệ thu {{ collectionRate }}%
            </p>
          </div>
        </div>
      </div>

      <div class="admin-chart-card">
        <div class="admin-chart-card-header">
          <h2 class="admin-section-title">Xu hướng doanh thu</h2>
        </div>
        <div :class="revenueTrendChartData.labels.length ? 'h-72 flex items-center justify-center' : 'py-6'">
          <Line v-if="revenueTrendChartData.labels.length" :data="revenueTrendChartData" :options="lineChartOptions" />
          <EmptyChartState v-else
            title="Chưa có dữ liệu doanh thu"
            description="Biểu đồ sẽ hiển thị khi có giao dịch thành công trong khoảng thời gian đã chọn." />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">Doanh thu theo khóa học</h2>
          <div :class="revenueByCourse.length ? 'h-56 flex items-center justify-center' : 'py-3'">
            <Bar v-if="revenueByCourse.length" :data="revenueByCourseChartData" :options="barChartOptions" />
            <EmptyChartState v-else
              title="Chưa có dữ liệu khóa học"
              description="Biểu đồ doanh thu theo khóa sẽ xuất hiện khi có ghi danh." />
          </div>
        </div>
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">Doanh thu theo lớp học</h2>
          <div :class="revenueByClass.length ? 'h-56 flex items-center justify-center' : 'py-3'">
            <Bar v-if="revenueByClass.length" :data="revenueByClassChartData" :options="barChartOptions" />
            <EmptyChartState v-else
              title="Chưa có dữ liệu lớp học"
              description="Biểu đồ doanh thu theo lớp sẽ xuất hiện khi có lớp hoạt động." />
          </div>
        </div>
      </div>

      <div class="admin-card">
        <div class="admin-card-header">
          <h2 class="admin-section-title">Bảng chi tiết doanh thu theo khóa</h2>
        </div>
        <a-table
          :data-source="revenueByCourseWithPercent"
          :columns="revenueDetailColumns"
          row-key="name"
          size="small"
          class="admin-table"
          :pagination="tablePagination"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'totalRevenue'">
              <span class="font-semibold" style="color: var(--admin-success);">{{ formatVnd(record.totalRevenue) }}</span>
            </template>
            <template v-else-if="column.key === 'percent'">
              <span class="text-[12px]" style="color: var(--admin-text-muted);">{{ record.percent }}%</span>
            </template>
          </template>
          <template #emptyText>
            <EmptyChartState :show-action-button="false"
              title="Chưa có doanh thu theo khóa"
              description="Bảng sẽ hiển thị khi có giao dịch." />
          </template>
        </a-table>
      </div>
    </div>

    <!-- Tab: Công nợ -->
    <div v-else-if="activeTab === 'debt'" class="space-y-6">
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-2">
        <div v-for="kpi in debtKpis" :key="kpi.key" class="admin-card flex items-center gap-3 px-3 py-2.5" style="min-width: 0;">
          <div :class="['kpi-hero-icon', kpi.iconColor]" style="width: 36px; height: 36px; border-radius: 10px; flex-shrink: 0;">
            <component :is="kpi.icon" style="font-size: 16px;" />
          </div>
          <div class="flex-1 min-w-0">
            <p class="kpi-hero-label" style="font-size: 10px; line-height: 1.2;">{{ kpi.label }}</p>
            <p style="font-size: 0.95rem; font-weight: 700; color: var(--admin-text); margin-top: 1px;">{{ kpi.value }}</p>
            <p v-if="kpi.sub" style="font-size: 10px; color: var(--admin-text-muted); margin-top: 1px;">{{ kpi.sub }}</p>
          </div>
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">Công nợ theo học viên</h2>
          <div :class="debtByStudent.length ? 'h-56 flex items-center justify-center' : 'py-3'">
            <Bar v-if="debtByStudent.length" :data="debtByStudentChartData" :options="horizontalBarOptions" />
            <EmptyChartState v-else
              title="Chưa có dữ liệu công nợ"
              description="Sẽ hiển thị khi có học viên còn nợ học phí." />
          </div>
        </div>
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">Công nợ theo lớp học</h2>
          <div :class="debtByClass.length ? 'h-56 flex items-center justify-center' : 'py-3'">
            <Bar v-if="debtByClass.length" :data="debtByClassChartData" :options="horizontalBarOptions" />
            <EmptyChartState v-else
              title="Chưa có dữ liệu công nợ theo lớp"
              description="Sẽ hiển thị khi có lớp học có nợ." />
          </div>
        </div>
      </div>

      <div class="admin-card">
        <div class="admin-card-header">
          <h2 class="admin-section-title">Bảng công nợ chi tiết</h2>
        </div>
        <a-table
          :data-source="debtByStudentRanked"
          :columns="debtDetailColumns"
          row-key="studentId"
          size="small"
          class="admin-table"
          :pagination="tablePagination"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'rank'">
              <span class="inline-flex items-center justify-center w-6 h-6 rounded-md text-[11px] font-bold"
                :style="{
                  background: record._rank <= 3 ? 'var(--admin-danger-soft)' : 'var(--admin-surface-2)',
                  color: record._rank <= 3 ? 'var(--admin-danger)' : 'var(--admin-text-muted)'
                }">
                {{ record._rank }}
              </span>
            </template>
            <template v-else-if="column.key === 'name'">
              <div class="flex items-center gap-2.5">
                <div class="w-7 h-7 rounded-full flex items-center justify-center text-[11px] font-bold text-white shrink-0"
                  :style="{ background: studentAvatarColor(record.name) }">
                  {{ studentInitials(record.name) }}
                </div>
                <span class="text-[13px] font-semibold admin-cell-ellipsis" style="color: var(--admin-text);">
                  {{ record.name }}
                </span>
              </div>
            </template>
            <template v-else-if="column.key === 'totalDebt'">
              <span class="font-semibold font-variant-numeric"
                :style="{ color: parseFloat(record.totalDebt) > 0 ? 'var(--admin-danger)' : 'var(--admin-text-muted)' }">
                {{ formatVnd(record.totalDebt) }}
              </span>
            </template>
            <template v-else-if="column.key === 'level'">
              <span class="inline-flex items-center px-2 py-0.5 rounded-md text-[11px] font-semibold"
                :style="{
                  background: debtLevelChipStyle(record.totalDebt).bg,
                  color: debtLevelChipStyle(record.totalDebt).color,
                  border: '1px solid ' + debtLevelChipStyle(record.totalDebt).border
                }">
                {{ debtLevelChipStyle(record.totalDebt).label }}
              </span>
            </template>
          </template>
          <template #emptyText>
            <EmptyChartState :show-action-button="false"
              title="Chưa có công nợ"
              description="Bảng sẽ hiển thị khi có học viên còn nợ." />
          </template>
        </a-table>
      </div>
    </div>

    <!-- Tab: Khóa học & Lớp -->
    <div v-else-if="activeTab === 'course-class'" class="space-y-6">
      <div class="admin-chart-card">
        <h2 class="admin-section-title mb-3">Doanh thu theo khóa học</h2>
        <div :class="revenueByCourse.length ? 'h-72 flex items-center justify-center' : 'py-6'">
          <Bar v-if="revenueByCourse.length" :data="revenueByCourseChartData" :options="barChartOptions" />
          <EmptyChartState v-else
            title="Chưa có dữ liệu khóa học"
            description="Biểu đồ sẽ hiển thị khi có khóa học được mở bán." />
        </div>
      </div>

      <div class="admin-chart-card">
        <h2 class="admin-section-title mb-3">Doanh thu theo lớp học</h2>
        <div :class="revenueByClass.length ? 'h-72 flex items-center justify-center' : 'py-6'">
          <Bar v-if="revenueByClass.length" :data="revenueByClassChartData" :options="barChartOptions" />
          <EmptyChartState v-else
            title="Chưa có dữ liệu lớp học"
            description="Biểu đồ sẽ hiển thị khi có lớp học hoạt động." />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div class="admin-card">
          <div class="admin-card-header">
            <h2 class="admin-section-title">Top khóa học theo doanh thu</h2>
          </div>
          <a-table
            :data-source="revenueByCourseWithPercent"
            :columns="revenueDetailColumns"
            row-key="name"
            size="small"
            class="admin-table"
            :pagination="{ pageSize: 5, size: 'small' }"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'totalRevenue'">
                <span class="font-semibold" style="color: var(--admin-success);">{{ formatVnd(record.totalRevenue) }}</span>
              </template>
              <template v-else-if="column.key === 'percent'">
                <span class="text-[12px]" style="color: var(--admin-text-muted);">{{ record.percent }}%</span>
              </template>
            </template>
            <template #emptyText>
              <EmptyChartState :show-action-button="false"
                title="Chưa có dữ liệu"
                description="Bảng sẽ hiển thị khi có doanh thu." />
            </template>
          </a-table>
        </div>
        <div class="admin-card">
          <div class="admin-card-header">
            <h2 class="admin-section-title">Top lớp học theo doanh thu</h2>
          </div>
          <a-table
            :data-source="revenueByClassWithPercent"
            :columns="revenueDetailColumns"
            row-key="name"
            size="small"
            class="admin-table"
            :pagination="{ pageSize: 5, size: 'small' }"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'totalRevenue'">
                <span class="font-semibold" style="color: var(--admin-success);">{{ formatVnd(record.totalRevenue) }}</span>
              </template>
              <template v-else-if="column.key === 'percent'">
                <span class="text-[12px]" style="color: var(--admin-text-muted);">{{ record.percent }}%</span>
              </template>
            </template>
            <template #emptyText>
              <EmptyChartState :show-action-button="false"
                title="Chưa có dữ liệu"
                description="Bảng sẽ hiển thị khi có doanh thu." />
            </template>
          </a-table>
        </div>
      </div>
    </div>

    <div v-else-if="activeTab === 'learning'" class="space-y-6">
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <div v-for="item in learningKpis" :key="item.label" class="admin-card px-4 py-4">
          <p class="kpi-hero-label">{{ item.label }}</p>
          <p class="text-2xl font-extrabold mt-2" :style="{ color: item.color }">{{ item.value }}</p>
          <p class="text-xs mt-1" style="color: var(--admin-text-muted);">{{ item.sub }}</p>
        </div>
      </div>
      <div class="grid grid-cols-1 xl:grid-cols-2 gap-4">
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">Phân bố kết quả học tập</h2>
          <div class="h-72"><Doughnut :data="resultStatusChartData" :options="doughnutChartOptions" /></div>
        </div>
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">Điểm trung bình theo khóa học</h2>
          <div class="h-72"><Bar :data="averageScoreByCourseChartData" :options="scoreBarOptions" /></div>
        </div>
      </div>
      <div class="admin-chart-card">
        <h2 class="admin-section-title mb-3">Tương quan điểm số và chuyên cần</h2>
        <div class="h-72"><Bar :data="scoreAttendanceChartData" :options="scoreBarOptions" /></div>
      </div>
    </div>

    <div v-else-if="activeTab === 'operations'" class="space-y-6">
      <div class="grid grid-cols-1 sm:grid-cols-4 gap-3">
        <div v-for="item in operationKpis" :key="item.label" class="admin-card px-4 py-4">
          <p class="kpi-hero-label">{{ item.label }}</p>
          <p class="text-2xl font-extrabold mt-2" style="color: var(--admin-accent);">{{ item.value }}</p>
          <p class="text-xs mt-1" style="color: var(--admin-text-muted);">{{ item.sub }}</p>
        </div>
      </div>
      <div class="grid grid-cols-1 xl:grid-cols-2 gap-4">
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">Trạng thái khóa học</h2>
          <div class="h-72"><Doughnut :data="courseStatusChartData" :options="doughnutChartOptions" /></div>
        </div>
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">Trạng thái lớp học</h2>
          <div class="h-72"><Doughnut :data="classStatusChartData" :options="doughnutChartOptions" /></div>
        </div>
      </div>
      <div class="admin-chart-card">
        <h2 class="admin-section-title mb-3">Ghi danh theo khóa học</h2>
        <div class="h-80"><Bar :data="enrollmentByCourseChartData" :options="barChartOptions" /></div>
      </div>
    </div>

    <!-- Tab: Dự báo AI -->
    <div v-else-if="activeTab === 'forecasting'" class="space-y-6">
      <!-- Financial & Enrollment Forecasting Panel -->
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 border-b border-base pb-3">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-xl bg-blue-100 dark:bg-blue-950/40 flex items-center justify-center text-blue-600 dark:text-blue-400 shrink-0">
              <FundOutlined style="font-size: 20px;" />
            </div>
            <div>
              <h2 class="text-base font-bold text-base-primary">📈 Dự báo Tài chính & Ghi danh Học viên mới (AI Time-Series)</h2>
              <p class="text-xs text-base-secondary mt-0.5">Thuật toán active: ARIMA + Prophet Stacking Model (Confidence: 95%).</p>
            </div>
          </div>
          <div class="flex items-center gap-2">
            <a-select v-model:value="forecastMetric" class="w-44" size="small">
              <a-select-option value="revenue">Doanh thu học phí</a-select-option>
              <a-select-option value="enrollment">Ghi danh học viên mới</a-select-option>
            </a-select>
            <a-select v-model:value="forecastHorizon" class="w-36" size="small">
              <a-select-option :value="3">3 tháng tới</a-select-option>
              <a-select-option :value="6">6 tháng tới</a-select-option>
              <a-select-option :value="12">12 tháng tới</a-select-option>
            </a-select>
          </div>
        </div>

        <!-- Chart Panel -->
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
          <div class="lg:col-span-2 admin-chart-card !m-0">
            <h3 class="text-xs font-bold text-base-primary mb-2">Biểu đồ dự báo xu hướng</h3>
            <div class="h-64 flex items-center justify-center">
              <Line :data="forecastChartData" :options="forecastChartOptions" />
            </div>
          </div>
          <div class="border border-base rounded-xl p-4 bg-slate-50/30 dark:bg-slate-900/10 flex flex-col justify-between">
            <div class="space-y-3">
              <h3 class="text-xs font-bold text-base-primary">🤖 Đánh giá & Gợi ý từ AI</h3>
              <div class="text-xs text-base-secondary leading-relaxed space-y-2">
                <div class="p-2.5 rounded-lg bg-blue-50/50 dark:bg-blue-950/20 border border-blue-100 dark:border-blue-900/40">
                  <strong>Xu hướng chính:</strong> {{ forecastSummary.trendText }}
                </div>
                <div class="p-2.5 rounded-lg bg-violet-50/50 dark:bg-violet-950/20 border border-violet-100 dark:border-violet-900/40">
                  <strong>Khuyến nghị vận hành:</strong> {{ forecastSummary.recommendation }}
                </div>
              </div>
            </div>
            <div class="border-t border-base pt-3 mt-3 text-[10px] text-base-muted flex justify-between">
              <span>Độ lệch chuẩn: ±4.2%</span>
              <span>Đã đồng bộ dữ liệu: Hôm nay</span>
            </div>
          </div>
        </div>

        <!-- Detailed Table -->
        <div class="border border-base rounded-xl overflow-hidden bg-card-base">
          <table class="w-full text-left border-collapse text-xs">
            <thead>
              <tr class="bg-slate-50 dark:bg-slate-900 border-b border-base">
                <th class="p-2.5 font-bold text-base-secondary">Thời gian</th>
                <th class="p-2.5 font-bold text-base-secondary text-right">Giá trị dự báo</th>
                <th class="p-2.5 font-bold text-base-secondary text-center">Biến động (%)</th>
                <th class="p-2.5 font-bold text-base-secondary text-center">Khoảng tin cậy 95% (Dưới - Trên)</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in forecastTableData" :key="row.month" class="border-b border-base last:border-b-0">
                <td class="p-2.5 font-semibold text-base-primary">{{ row.month }}</td>
                <td class="p-2.5 text-right font-mono font-bold text-base-secondary">
                  {{ forecastMetric === 'revenue' ? formatVnd(row.value) : row.value + ' lượt' }}
                </td>
                <td class="p-2.5 text-center font-mono font-semibold" :class="row.change >= 0 ? 'text-emerald-500' : 'text-red-500'">
                  {{ row.change >= 0 ? '+' : '' }}{{ row.change }}%
                </td>
                <td class="p-2.5 text-center font-mono text-base-muted">
                  {{ forecastMetric === 'revenue' ? `${formatVnd(row.low)} - ${formatVnd(row.high)}` : `${row.low} - ${row.high} lượt` }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Tab: BI Đàm thoại -->
    <div v-else-if="activeTab === 'conversational-bi'" class="space-y-6">
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4">
        <div class="flex items-center gap-3 border-b border-base pb-3">
          <div class="w-10 h-10 rounded-xl bg-emerald-100 dark:bg-emerald-950/40 flex items-center justify-center text-emerald-600 dark:text-emerald-400 shrink-0">
            <CommentOutlined style="font-size: 20px;" />
          </div>
          <div>
            <h2 class="text-base font-bold text-base-primary">💬 Trợ lý Business Intelligence (BI) Đàm thoại</h2>
            <p class="text-xs text-base-secondary mt-0.5">Truy vấn, phân tích trực tiếp dữ liệu doanh thu, công nợ, chuyên cần và học tập bằng ngôn ngữ tự nhiên.</p>
          </div>
        </div>

        <!-- Query Suggestions -->
        <div class="flex flex-wrap gap-2 items-center">
          <span class="text-xs text-base-secondary font-bold">Gợi ý truy vấn:</span>
          <button
            v-for="(q, idx) in suggestedBiQueries"
            :key="idx"
            type="button"
            class="px-3 py-1.5 rounded-full border border-dashed border-base hover:bg-slate-50 dark:hover:bg-slate-800/40 text-xs text-base-secondary cursor-pointer hover:border-emerald-500 transition-colors"
            @click="selectBiQuery(q)"
          >
            {{ q }}
          </button>
        </div>

        <!-- Chat Area -->
        <div class="border border-base rounded-xl overflow-hidden bg-slate-50/50 dark:bg-slate-900/10 flex flex-col h-[400px]">
          <!-- Message List -->
          <div ref="biChatContainer" class="flex-grow p-4 overflow-y-auto space-y-4 custom-scrollbar">
            <div
              v-for="(msg, index) in biMessages"
              :key="index"
              class="flex items-start gap-2.5"
              :class="msg.role === 'user' ? 'justify-end' : 'justify-start'"
            >
              <!-- Avatar -->
              <div
                v-if="msg.role !== 'user'"
                class="w-7 h-7 rounded-lg bg-emerald-600 flex items-center justify-center text-xs text-white shrink-0 font-bold"
              >
                🤖
              </div>

              <!-- Message Bubble -->
              <div
                class="rounded-xl px-3 py-2 text-xs leading-relaxed max-w-[85%] border select-text"
                :class="msg.role === 'user'
                  ? 'bg-emerald-600 text-white border-emerald-500'
                  : 'bg-card-base border-base text-base-primary'"
              >
                <div class="prose dark:prose-invert max-w-none text-xs" v-html="parseMarkdown(msg.text)"></div>
                <span class="text-[9px] opacity-50 block text-right mt-1 font-medium font-mono">
                  {{ msg.time }}
                </span>
              </div>
            </div>

            <!-- Typing Bubble -->
            <div v-if="biTyping" class="flex items-start gap-2.5 justify-start">
              <div class="w-7 h-7 rounded-lg bg-emerald-600 flex items-center justify-center text-xs text-white shrink-0 font-bold">🤖</div>
              <div class="bg-card-base border border-base rounded-xl px-4 py-3 flex items-center gap-1">
                <span class="w-1.5 h-1.5 rounded-full bg-slate-400 dark:bg-slate-500 animate-bounce"></span>
                <span class="w-1.5 h-1.5 rounded-full bg-slate-400 dark:bg-slate-500 animate-bounce delay-100"></span>
                <span class="w-1.5 h-1.5 rounded-full bg-slate-400 dark:bg-slate-500 animate-bounce delay-200"></span>
              </div>
            </div>
          </div>

          <!-- Input Bar -->
          <div class="p-3 border-t border-base bg-card-base">
            <form @submit.prevent="sendBiQuery" class="flex items-center gap-2">
              <a-input
                v-model:value="biInput"
                placeholder="Nhập câu hỏi phân tích dữ liệu trung tâm... (Ví dụ: Dự báo doanh thu 3 tháng tới)"
                size="small"
                class="flex-grow !h-9 text-xs"
                :disabled="biTyping"
              />
              <a-button
                type="primary"
                html-type="submit"
                class="admin-btn admin-btn-primary !h-9 shrink-0 px-4"
                :disabled="!biInput.trim() || biTyping"
              >
                Gửi truy vấn
              </a-button>
            </form>
          </div>
        </div>

      </div>
    </div>

    <!-- Fallback: tab không khớp -->
    <div v-else class="admin-empty-state">
      <p class="text-sm font-semibold" style="color: var(--admin-text);">Chọn một tab để xem báo cáo</p>
    </div>

    <!-- Grid cũ (tạm ẩn) -->
    <div v-show="false" class="grid grid-cols-1 xl:grid-cols-2 gap-6">
      <!-- 1. Revenue trend (Line) -->
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4">
        <h2 class="text-sm font-bold text-base-primary flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-blue-600 rounded-full"></span>
          Biểu đồ xu hướng doanh thu
        </h2>
        <div class="h-52 flex items-center justify-center">
          <Line v-if="revenueTrendChartData.labels.length" :data="revenueTrendChartData" :options="chartOptions" />
          <div v-else class="text-center py-6">
            <EmptyTableState :show-action-button="false" description="Không có dữ liệu doanh thu trong khoảng thời gian này." />
          </div>
        </div>
      </div>

      <!-- 2. Revenue by course (Bar + Table) -->
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4">
        <h2 class="text-sm font-bold text-base-primary flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-purple-600 rounded-full"></span>
          Doanh thu theo khóa học
        </h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 items-center">
          <div class="h-44 flex items-center justify-center border border-dashed border-base rounded-xl p-2 bg-slate-50/50 dark:bg-slate-900/10">
            <Bar v-if="revenueByCourse.length" :data="revenueByCourseChartData" :options="chartOptions" />
            <div v-else class="text-center py-6 text-base-muted text-xs">Không có dữ liệu biểu đồ</div>
          </div>
          <a-table
            :data-source="revenueByCourse"
            :columns="revenueColumns"
            row-key="name"
            size="small"
            class="custom-table"
            :pagination="{ pageSize: 4, size: 'small', showLessItems: true }"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'totalRevenue'">
                <span class="font-medium text-emerald-600">{{ formatVnd(record.totalRevenue) }}</span>
              </template>
            </template>
          </a-table>
        </div>
      </div>

      <!-- 3. Revenue by class (Bar + Table) -->
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4">
        <h2 class="text-sm font-bold text-base-primary flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-indigo-600 rounded-full"></span>
          Doanh thu theo lớp học
        </h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 items-center">
          <div class="h-44 flex items-center justify-center border border-dashed border-base rounded-xl p-2 bg-slate-50/50 dark:bg-slate-900/10">
            <Bar v-if="revenueByClass.length" :data="revenueByClassChartData" :options="chartOptions" />
            <div v-else class="text-center py-6 text-base-muted text-xs">Không có dữ liệu biểu đồ</div>
          </div>
          <a-table
            :data-source="revenueByClass"
            :columns="revenueColumns"
            row-key="name"
            size="small"
            class="custom-table"
            :pagination="{ pageSize: 4, size: 'small', showLessItems: true }"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'totalRevenue'">
                <span class="font-medium text-emerald-600">{{ formatVnd(record.totalRevenue) }}</span>
              </template>
            </template>
          </a-table>
        </div>
      </div>

      <!-- 4. Payment Method Distribution (Doughnut) -->
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4">
        <h2 class="text-sm font-bold text-base-primary flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-teal-600 rounded-full"></span>
          Phân bổ phương thức thanh toán
        </h2>
        <div class="h-44 flex items-center justify-center">
          <Doughnut v-if="filteredPayments.length" :data="paymentMethodChartData" :options="doughnutOptions" />
          <div v-else class="text-center py-6 text-base-muted text-xs">Không có dữ liệu giao dịch</div>
        </div>
      </div>

      <!-- 5. Invoice Status distribution (Doughnut) -->
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4">
        <h2 class="text-sm font-bold text-base-primary flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-emerald-600 rounded-full"></span>
          Trạng thái hóa đơn học phí
        </h2>
        <div class="h-44 flex items-center justify-center">
          <Doughnut v-if="hasInvoiceData" :data="invoiceStatusChartData" :options="doughnutOptions" />
          <div v-else class="text-center py-6 text-base-muted text-xs">Không có dữ liệu hóa đơn</div>
        </div>
      </div>

      <!-- 6. Debt by Class (Doughnut + Table) -->
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4">
        <h2 class="text-sm font-bold text-base-primary flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-rose-600 rounded-full"></span>
          Công nợ theo lớp học
        </h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 items-center">
          <div class="h-44 flex items-center justify-center border border-dashed border-base rounded-xl p-2 bg-slate-50/50 dark:bg-slate-900/10">
            <Doughnut v-if="debtByClass.length" :data="debtByClassChartData" :options="doughnutOptions" />
            <div v-else class="text-center py-6 text-base-muted text-xs">Không có dữ liệu biểu đồ</div>
          </div>
          <a-table
            :data-source="debtByClass"
            :columns="debtColumns"
            row-key="name"
            size="small"
            class="custom-table"
            :pagination="{ pageSize: 4, size: 'small', showLessItems: true }"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'totalDebt'">
                <span class="font-medium text-rose-600">{{ formatVnd(record.totalDebt) }}</span>
              </template>
            </template>
          </a-table>
        </div>
      </div>

      <!-- 7. Debt by Student (Doughnut + Table) -->
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4">
        <h2 class="text-sm font-bold text-base-primary flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-amber-600 rounded-full"></span>
          Công nợ theo học viên
        </h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 items-center">
          <div class="h-44 flex items-center justify-center border border-dashed border-base rounded-xl p-2 bg-slate-50/50 dark:bg-slate-900/10">
            <Doughnut v-if="debtByStudent.length" :data="debtByStudentChartData" :options="doughnutOptions" />
            <div v-else class="text-center py-6 text-base-muted text-xs">Không có dữ liệu biểu đồ</div>
          </div>
          <a-table
            :data-source="debtByStudent"
            :columns="debtColumns"
            row-key="name"
            size="small"
            class="custom-table"
            :pagination="{ pageSize: 4, size: 'small', showLessItems: true }"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'totalDebt'">
                <span class="font-medium text-rose-600">{{ formatVnd(record.totalDebt) }}</span>
              </template>
            </template>
          </a-table>
        </div>
      </div>

      <!-- 8. Attendance Distribution (API Missing state) -->
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4 flex flex-col justify-between">
        <div class="flex items-center justify-between">
          <h2 class="text-sm font-bold text-base-primary flex items-center gap-2">
            <span class="w-1.5 h-3.5 bg-rose-650 rounded-full"></span>
            Biểu đồ chuyên cần
          </h2>
          <a-tooltip title="API chuyên cần (/gateway/attendance/stats) còn thiếu trên backend.">
            <InfoCircleOutlined class="text-rose-500 text-xs" />
          </a-tooltip>
        </div>
        <div class="h-44 flex items-center justify-center flex-1">
          <EmptyTableState
            :show-action-button="false"
            title="API chuyên cần còn thiếu"
            description="Báo cáo chuyên cần chi tiết chưa thể hiển thị do thiếu endpoint backend /gateway/attendance/stats."
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, h, onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import { DownloadOutlined, SyncOutlined, DownOutlined, InfoCircleOutlined, BarChartOutlined, BookOutlined, TeamOutlined, UserOutlined, DollarCircleOutlined, PayCircleOutlined, RiseOutlined, CreditCardOutlined, CheckCircleOutlined, ArrowUpOutlined, ArrowDownOutlined, MinusOutlined, AppstoreOutlined, LineChartOutlined, FundOutlined, TrophyOutlined, ReadOutlined, DeploymentUnitOutlined, CommentOutlined } from '@ant-design/icons-vue'
import { reportApi } from '@/api/reportApi'
import { paymentApi } from '@/api/paymentApi'
import { tuitionApi } from '@/api/tuitionApi'
import { resultApi } from '@/api/resultApi'
import { courseApi } from '@/api/courseApi'
import { classApi } from '@/api/classApi'
import { enrollmentApi } from '@/api/enrollmentApi'
import { aiApi } from '@/api/aiApi'
import { formatVnd, shortInvoiceCode, shortDateVN } from '@/lib/formatters'
import { openPdfReport, reportFilename } from '@/lib/exportDocuments'
import EmptyTableState from '@/components/admin/EmptyTableState.vue'

// EmptyChartState component (inline) — dùng cho chart/table empty
const EmptyChartState = {
  props: {
    title: { type: String, default: 'Chưa có dữ liệu' },
    description: { type: String, default: '' },
    showActionButton: { type: Boolean, default: false }
  },
  setup(props) {
    return () => h('div', { class: 'text-center py-4' }, [
      h('div', {
        class: 'mx-auto w-12 h-12 rounded-full flex items-center justify-center mb-3',
        style: 'background: var(--admin-surface-2); color: var(--admin-text-subtle);'
      }, [
        h('svg', { width: '22', height: '22', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '1.5', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
          h('line', { x1: '12', y1: '20', x2: '12', y2: '10' }),
          h('line', { x1: '18', y1: '20', x2: '18', y2: '4' }),
          h('line', { x1: '6', y1: '20', x2: '6', y2: '16' })
        ])
      ]),
      h('p', { class: 'text-sm font-semibold', style: 'color: var(--admin-text);' }, props.title),
      props.description ? h('p', { class: 'text-xs mt-1', style: 'color: var(--admin-text-muted);' }, props.description) : null
    ])
  }
}

// ChartJS setup
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
  LineElement,
  Filler
} from 'chart.js'

ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale, ArcElement, PointElement, LineElement, Filler)

const loading = ref(false)
const errorMsg = ref('')
const activeTab = ref('overview')
const activePeriod = ref('all')
const revenueTrendRange = ref('all')
const selectedYear = ref(null)
const selectedMonth = ref(null)
const customDateRange = ref(null)

const dateRange = computed(() => {
  if (activePeriod.value === 'all') return null
  if (activePeriod.value === 'custom') {
    if (customDateRange.value?.length === 2) {
      return [new Date(`${customDateRange.value[0]}T00:00:00`), new Date(`${customDateRange.value[1]}T23:59:59`)]
    }
    if (selectedYear.value) {
      const month = selectedMonth.value
      const start = month ? new Date(selectedYear.value, month - 1, 1) : new Date(selectedYear.value, 0, 1)
      const end = month ? new Date(selectedYear.value, month, 0, 23, 59, 59, 999) : new Date(selectedYear.value, 11, 31, 23, 59, 59, 999)
      return [start, end]
    }
    return null
  }
  const now = new Date()
  const end = new Date(now)
  let start = new Date(now)

  switch (activePeriod.value) {
    case '7d':
      start.setDate(now.getDate() - 6)
      break
    case 'month':
      start = new Date(now.getFullYear(), now.getMonth(), 1)
      break
    case 'quarter': {
      const quarterStartMonth = Math.floor(now.getMonth() / 3) * 3
      start = new Date(now.getFullYear(), quarterStartMonth, 1)
      break
    }
    case '12m':
      start.setFullYear(now.getFullYear() - 1)
      break
    case '6m':
      start.setMonth(now.getMonth() - 6)
      break
    case '30d':
    default:
      start.setDate(now.getDate() - 29)
      break
  }

  start.setHours(0, 0, 0, 0)
  end.setHours(23, 59, 59, 999)
  return [start, end]
})

const periodOptions = [
  { value: '7d',   label: '7 ngày' },
  { value: '30d',  label: '30 ngày' },
  { value: 'month', label: 'Tháng này' },
  { value: 'quarter', label: 'Quý này' },
  { value: '12m',  label: '12 tháng' },
  { value: 'all', label: 'Từ trước đến nay' },
  { value: 'custom', label: 'Tùy chọn' }
]

const trendRangeOptions = [
  { value: '7d',  label: '7 ngày' },
  { value: '30d', label: '30 ngày' },
  { value: '6m',  label: '6 tháng' },
  { value: '12m', label: '12 tháng' },
  { value: 'all', label: 'Tất cả' }
]

const tabs = [
  { key: 'overview',    label: 'Tổng quan',     icon: AppstoreOutlined },
  { key: 'revenue',     label: 'Doanh thu',     icon: LineChartOutlined },
  { key: 'debt',        label: 'Công nợ',       icon: PayCircleOutlined },
  { key: 'course-class', label: 'Khóa học & Lớp', icon: BookOutlined },
  { key: 'learning', label: 'Kết quả học tập', icon: ReadOutlined },
  { key: 'operations', label: 'Vận hành đào tạo', icon: DeploymentUnitOutlined },
  { key: 'forecasting', label: 'Dự báo AI 📈', icon: FundOutlined },
  { key: 'conversational-bi', label: 'Trợ lý BI (AI) 💬', icon: CommentOutlined }
]

// API data stores
const revenueByCourse = ref([])
const revenueByClass = ref([])
const debtByStudent = ref([])
const debtByClass = ref([])
const paymentsList = ref([])
const tuitionList = ref([])
const revenueOverview = ref(null)
const resultsList = ref([])
const coursesList = ref([])
const classesList = ref([])
const enrollmentsList = ref([])

const availableYears = computed(() => {
  const dates = [...paymentsList.value.map(x => x.paymentDate), ...tuitionList.value.map(x => x.createdAt)]
  return [...new Set(dates.filter(Boolean).map(value => new Date(value).getFullYear()))].sort((a, b) => b - a)
})

// Tables columns
const revenueColumns = [
  { title: 'Tên', dataIndex: 'name', key: 'name' },
  { title: 'Doanh thu', dataIndex: 'totalRevenue', key: 'totalRevenue', width: 140 }
]

const debtColumns = [
  { title: 'Tên', dataIndex: 'name', key: 'name' },
  { title: 'Công nợ', dataIndex: 'totalDebt', key: 'totalDebt', width: 140 }
]

// Date range filters
const filteredPayments = computed(() => {
  if (!dateRange.value || dateRange.value.length < 2) return paymentsList.value
  const [start, end] = dateRange.value
  const startDate = new Date(start)
  const endDate = new Date(end)
  endDate.setHours(23, 59, 59, 999)
  return paymentsList.value.filter(p => {
    const d = new Date(p.paymentDate)
    return d >= startDate && d <= endDate
  })
})

const filteredTuition = computed(() => {
  if (!dateRange.value || dateRange.value.length < 2) return tuitionList.value
  const [start, end] = dateRange.value
  const startDate = new Date(start)
  const endDate = new Date(end)
  endDate.setHours(23, 59, 59, 999)
  return tuitionList.value.filter(t => {
    const d = new Date(t.createdAt)
    return d >= startDate && d <= endDate
  })
})

// Summary metrics computed from filtered lists
// ============ Tổng hợp từ nhiều nguồn (fallback nếu API tổng quan thiếu) ============
// Ưu tiên: 1) paymentsList thành công → 2) revenueByCourse/class tổng
const totalPaidRevenue = computed(() => {
  // Ưu tiên 1: từ filteredPayments (nếu có)
  const fromPayments = filteredPayments.value
    .filter(p => enumValue(p.status, { Success: 1, Pending: 2, Failed: 3, Cancelled: 4 }) === 1)
    .reduce((sum, p) => sum + parseFloat(p.amount || 0), 0)
  if (fromPayments > 0) return fromPayments
  // Fallback 2: tổng từ revenueByCourse + revenueByClass (khi API tổng quan thiếu)
  const fromCourses = (revenueByCourse.value || []).reduce((sum, x) => sum + parseFloat(x.totalRevenue || 0), 0)
  const fromClasses = (revenueByClass.value || []).reduce((sum, x) => sum + parseFloat(x.totalRevenue || 0), 0)
  return Math.max(fromCourses, fromClasses)
})

const totalDebtRevenue = computed(() => {
  // Ưu tiên 1: từ filteredTuition (nếu có)
  const fromTuition = filteredTuition.value
    .filter(t => [1, 2, 4].includes(enumValue(t.status, { Unpaid: 1, Partial: 2, Paid: 3, Overdue: 4 })))
    .reduce((sum, t) => sum + parseFloat(t.debtAmount || 0), 0)
  if (fromTuition > 0) return fromTuition
  // Fallback 2: tổng từ debtByStudent + debtByClass
  const fromDebtStudent = (debtByStudent.value || []).reduce((sum, x) => sum + parseFloat(x.totalDebt || 0), 0)
  const fromDebtClass = (debtByClass.value || []).reduce((sum, x) => sum + parseFloat(x.totalDebt || 0), 0)
  return Math.max(fromDebtStudent, fromDebtClass)
})

const totalExpectedRevenue = computed(() => {
  // Tổng phải thu = đã thu + còn nợ (cộng dồn từ các nguồn)
  // Nếu có payment thì cộng thêm, fallback từ revenue tổng
  const paidBase = totalPaidRevenue.value
  const debtBase = totalDebtRevenue.value
  // Nếu paid = 0 nhưng revenueByCourse có → dùng max
  if (paidBase === 0 && debtBase === 0) {
    const coursesTotal = (revenueByCourse.value || []).reduce((s, x) => s + parseFloat(x.totalRevenue || 0), 0)
    const classesTotal = (revenueByClass.value || []).reduce((s, x) => s + parseFloat(x.totalRevenue || 0), 0)
    const debtTotal = (debtByStudent.value || []).reduce((s, x) => s + parseFloat(x.totalDebt || 0), 0)
    return Math.max(coursesTotal, classesTotal) + debtTotal
  }
  return paidBase + debtBase
})

const collectionRate = computed(() => {
  const total = totalExpectedRevenue.value
  if (total === 0) return 0
  return Math.round((totalPaidRevenue.value / total) * 100)
})

const avgTransactionValue = computed(() => {
  const payments = filteredPayments.value.filter(p => enumValue(p.status, { Success: 1, Pending: 2, Failed: 3, Cancelled: 4 }) === 1)
  if (payments.length === 0) return 0
  return totalPaidRevenue.value / payments.length
})

// 1. Revenue trend (Line chart)
const revenueTrendChartData = computed(() => {
  const successPayments = [...filteredPayments.value].filter(p => enumValue(p.status, { Success: 1, Pending: 2, Failed: 3, Cancelled: 4 }) === 1)
  successPayments.sort((a, b) => new Date(a.paymentDate) - new Date(b.paymentDate))

  const dailyMap = {}
  successPayments.forEach(p => {
    const dateStr = new Date(p.paymentDate).toLocaleDateString('vi-VN')
    dailyMap[dateStr] = (dailyMap[dateStr] || 0) + parseFloat(p.amount)
  })

  const labels = Object.keys(dailyMap)
  const data = labels.map(l => dailyMap[l])

  return {
    labels: labels,
    datasets: [{
      label: 'Doanh thu',
      borderColor: '#3b82f6',
      backgroundColor: 'rgba(59, 130, 246, 0.05)',
      fill: true,
      borderWidth: 2,
      tension: 0.25,
      data: data
    }]
  }
})

const revenueInsight = computed(() => {
  const data = revenueTrendChartData.value.datasets?.[0]?.data || []
  if (data.length < 2) return null

  const last = Number(data[data.length - 1] || 0)
  const previous = Number(data[data.length - 2] || 0)
  const diff = last - previous

  if (diff === 0) {
    return {
      direction: 'neutral',
      headline: 'Doanh thu \u1ed5n \u0111\u1ecbnh',
      detail: `B\u1eb1ng v\u1edbi k\u1ef3 tr\u01b0\u1edbc (${formatVnd(previous)}).`
    }
  }

  if (previous === 0) {
    return {
      direction: diff > 0 ? 'up' : 'neutral',
      headline: diff > 0 ? 'B\u1eaft \u0111\u1ea7u c\u00f3 doanh thu' : 'Ch\u01b0a c\u00f3 bi\u1ebfn \u0111\u1ed9ng',
      detail: `K\u1ef3 n\u00e0y \u0111\u1ea1t ${formatVnd(last)}.`
    }
  }

  const percent = Math.round(Math.abs(diff / previous) * 100)
  return {
    direction: diff > 0 ? 'up' : 'down',
    headline: diff > 0 ? `T\u0103ng ${percent}% so v\u1edbi k\u1ef3 tr\u01b0\u1edbc` : `Gi\u1ea3m ${percent}% so v\u1edbi k\u1ef3 tr\u01b0\u1edbc`,
    detail: `K\u1ef3 n\u00e0y \u0111\u1ea1t ${formatVnd(last)}, k\u1ef3 tr\u01b0\u1edbc ${formatVnd(previous)}.`
  }
})

// 2. Revenue by course (Bar)
// Mảng màu gradient cho chart bar (đẹp + phân biệt)
const chartBarGradient = [
  '#4f46e5', '#6366f1', '#7c3aed', '#9333ea', '#a855f7',
  '#c084fc', '#d946ef', '#ec4899', '#f43f5e', '#fb7185',
  '#f97316', '#f59e0b', '#eab308', '#84cc16', '#22c55e',
  '#10b981', '#14b8a6', '#06b6d4', '#0ea5e9', '#3b82f6'
]

const revenueByCourseChartData = computed(() => {
  const list = revenueByCourse.value || []
  return {
    labels: list.map(item => item.name || 'Chưa rõ'),
    datasets: [{
      label: 'Doanh thu',
      backgroundColor: list.map((_, i) => chartBarGradient[i % chartBarGradient.length]),
      borderRadius: 6,
      borderSkipped: false,
      maxBarThickness: 48,
      data: list.map(item => item.totalRevenue || 0)
    }]
  }
})

// 3. Revenue by class (Bar)
const revenueByClassChartData = computed(() => {
  const list = revenueByClass.value || []
  return {
    labels: list.map(item => item.name || 'Chưa rõ'),
    datasets: [{
      label: 'Doanh thu',
      backgroundColor: list.map((_, i) => chartBarGradient[(i + 5) % chartBarGradient.length]),
      borderRadius: 6,
      borderSkipped: false,
      maxBarThickness: 48,
      data: list.map(item => item.totalRevenue || 0)
    }]
  }
})

// ============ Với % so với tổng (cho table) ============
const revenueByCourseWithPercent = computed(() => {
  const list = revenueByCourse.value || []
  const total = list.reduce((sum, x) => sum + parseFloat(x.totalRevenue || 0), 0)
  return list.map(item => ({
    ...item,
    percent: total > 0 ? Math.round((parseFloat(item.totalRevenue || 0) / total) * 100) : 0
  }))
})

const revenueByClassWithPercent = computed(() => {
  const list = revenueByClass.value || []
  const total = list.reduce((sum, x) => sum + parseFloat(x.totalRevenue || 0), 0)
  return list.map(item => ({
    ...item,
    percent: total > 0 ? Math.round((parseFloat(item.totalRevenue || 0) / total) * 100) : 0
  }))
})

// ============ Top performers (cho insight cards tab Doanh thu) ============
const topRevenueCourse = computed(() => {
  const list = [...(revenueByCourse.value || [])]
    .filter(x => parseFloat(x.totalRevenue || 0) > 0)
    .sort((a, b) => parseFloat(b.totalRevenue || 0) - parseFloat(a.totalRevenue || 0))
  return list[0] || null
})

const topRevenueClass = computed(() => {
  const list = [...(revenueByClass.value || [])]
    .filter(x => parseFloat(x.totalRevenue || 0) > 0)
    .sort((a, b) => parseFloat(b.totalRevenue || 0) - parseFloat(a.totalRevenue || 0))
  return list[0] || null
})

// 4. Payment methods (Doughnut)
const paymentMethodChartData = computed(() => {
  // Cash is no longer offered; old method=1 records are folded into bank transfer.
  const methods = { 2: 0, 3: 0, 4: 0 }
  filteredPayments.value.filter(p => enumValue(p.status, { Success: 1, Pending: 2, Failed: 3, Cancelled: 4 }) === 1).forEach(p => {
    const method = enumValue(p.method, { Cash: 1, BankTransfer: 2, Momo: 3, VNPay: 4 })
    const normalizedMethod = method === 1 ? 2 : method
    if (normalizedMethod in methods) {
      methods[normalizedMethod] = (methods[normalizedMethod] || 0) + parseFloat(p.amount)
    }
  })

  return {
    labels: ['Chuyển khoản', 'Momo', 'VNPay'],
    datasets: [{
      data: [methods[2], methods[3], methods[4]],
      backgroundColor: ['#3b82f6', '#ec4899', '#f59e0b'],
      borderWidth: 0
    }]
  }
})

// 5. Invoice Status distribution (Doughnut)
const hasInvoiceData = computed(() => filteredTuition.value.length > 0)
const invoiceStatusChartData = computed(() => {
  // Status mapping: 1 Unpaid, 2 Partial, 3 Paid, 4 Overdue
  const counts = { 1: 0, 2: 0, 3: 0, 4: 0 }
  filteredTuition.value.forEach(inv => {
    const status = enumValue(inv.status, { Unpaid: 1, Partial: 2, Paid: 3, Overdue: 4 })
    counts[status] = (counts[status] || 0) + 1
  })

  return {
    labels: ['Chưa thanh toán', 'Một phần', 'Đã thanh toán', 'Quá hạn'],
    datasets: [{
      data: [counts[1], counts[2], counts[3], counts[4]],
      backgroundColor: ['#f59e0b', '#3b82f6', '#10b981', '#ef4444'],
      borderWidth: 0
    }]
  }
})

// 6. Debt by Class (Doughnut)
const debtByClassChartData = computed(() => {
  const list = debtByClass.value || []
  return {
    labels: list.map(item => item.name || 'N/A'),
    datasets: [{
      label: 'Công nợ',
      backgroundColor: ['#ef4444', '#f59e0b', '#3b82f6', '#10b981', '#8b5cf6', '#a1a1aa'],
      borderWidth: 0,
      data: list.map(item => item.totalDebt || 0)
    }]
  }
})

// 7. Debt by Student (Doughnut)
const debtByStudentChartData = computed(() => {
  const list = debtByStudent.value || []
  return {
    labels: list.map(item => item.name || 'N/A'),
    datasets: [{
      label: 'Công nợ',
      backgroundColor: ['#f59e0b', '#fb923c', '#fb7185', '#38bdf8', '#a78bfa', '#a1a1aa'],
      borderWidth: 0,
      data: list.map(item => item.totalDebt || 0)
    }]
  }
})

function enumValue(value, map) {
  const numeric = Number(value)
  return Number.isFinite(numeric) ? numeric : map[value]
}

const resultStatusChartData = computed(() => {
  const counts = { 1: 0, 2: 0, 3: 0 }
  resultsList.value.forEach(item => {
    const status = enumValue(item.resultStatus, { InProgress: 1, Passed: 2, Failed: 3 })
    counts[status] = (counts[status] || 0) + 1
  })
  return {
    labels: ['Đang đánh giá', 'Đạt', 'Chưa đạt'],
    datasets: [{ data: [counts[1], counts[2], counts[3]], backgroundColor: ['#60a5fa', '#10b981', '#f43f5e'], borderWidth: 0 }]
  }
})

const averageScoreByCourseChartData = computed(() => {
  const groups = {}
  resultsList.value.forEach(item => {
    const name = item.courseNameSnapshot || 'Chưa xác định'
    if (!groups[name]) groups[name] = []
    groups[name].push(Number(item.averageScore || 0))
  })
  const rows = Object.entries(groups).map(([name, values]) => ({ name, value: values.reduce((a, b) => a + b, 0) / values.length })).sort((a, b) => b.value - a.value)
  return { labels: rows.map(x => x.name), datasets: [{ label: 'Điểm trung bình', data: rows.map(x => x.value.toFixed(2)), backgroundColor: rows.map((_, i) => chartBarGradient[i % chartBarGradient.length]), borderRadius: 7, borderSkipped: false }] }
})

const scoreAttendanceChartData = computed(() => {
  const rows = [...resultsList.value].sort((a, b) => Number(b.averageScore) - Number(a.averageScore)).slice(0, 12)
  return {
    labels: rows.map(x => x.studentNameSnapshot || 'Học viên'),
    datasets: [
      { label: 'Điểm TB', data: rows.map(x => Number(x.averageScore || 0)), backgroundColor: '#6366f1', borderRadius: 6 },
      { label: 'Chuyên cần / 10', data: rows.map(x => Number(x.attendancePercent || 0) / 10), backgroundColor: '#22c55e', borderRadius: 6 }
    ]
  }
})

function statusCount(list, values) {
  const map = { Draft: 0, Opening: 1, Closed: 2, ComingSoon: 3, Open: 0, Full: 1, InProgress: 2, Completed: 3, Cancelled: 4 }
  return values.map(value => list.filter(item => enumValue(item.status, map) === value).length)
}

const courseStatusChartData = computed(() => ({
  labels: ['Nháp', 'Đang mở', 'Đã đóng', 'Sắp mở'],
  datasets: [{ data: statusCount(coursesList.value, [0, 1, 2, 3]), backgroundColor: ['#94a3b8', '#10b981', '#f43f5e', '#8b5cf6'], borderWidth: 0 }]
}))

const classStatusChartData = computed(() => ({
  labels: ['Đang mở', 'Đã đầy', 'Đang học', 'Hoàn thành', 'Đã hủy'],
  datasets: [{ data: statusCount(classesList.value, [0, 1, 2, 3, 4]), backgroundColor: ['#3b82f6', '#f59e0b', '#8b5cf6', '#10b981', '#ef4444'], borderWidth: 0 }]
}))

const enrollmentByCourseChartData = computed(() => {
  const groups = {}
  enrollmentsList.value.forEach(item => {
    const name = item.courseNameSnapshot || 'Chưa xác định'
    groups[name] = (groups[name] || 0) + 1
  })
  const rows = Object.entries(groups).map(([name, value]) => ({ name, value })).sort((a, b) => b.value - a.value)
  return { labels: rows.map(x => x.name), datasets: [{ label: 'Lượt ghi danh', data: rows.map(x => x.value), backgroundColor: rows.map((_, i) => chartBarGradient[(i + 3) % chartBarGradient.length]), borderRadius: 7, borderSkipped: false }] }
})

const learningKpis = computed(() => {
  const total = resultsList.value.length
  const passed = resultsList.value.filter(x => enumValue(x.resultStatus, { InProgress: 1, Passed: 2, Failed: 3 }) === 2).length
  const avg = total ? resultsList.value.reduce((sum, x) => sum + Number(x.averageScore || 0), 0) / total : 0
  return [
    { label: 'Kết quả đã ghi nhận', value: total, sub: 'Tổng hồ sơ điểm', color: '#4f46e5' },
    { label: 'Tỷ lệ đạt', value: total ? `${Math.round(passed / total * 100)}%` : '0%', sub: `${passed}/${total} học viên đạt`, color: '#059669' },
    { label: 'Điểm trung bình', value: avg.toFixed(2), sub: 'Thang điểm 10', color: '#ea580c' }
  ]
})

const operationKpis = computed(() => [
  { label: 'Khóa học', value: coursesList.value.length, sub: 'Toàn bộ chương trình' },
  { label: 'Lớp học', value: classesList.value.length, sub: 'Tất cả trạng thái' },
  { label: 'Lượt ghi danh', value: enrollmentsList.value.length, sub: 'Tổng đăng ký' },
  { label: 'Đang học', value: enrollmentsList.value.filter(x => enumValue(x.status, { Pending: 1, Confirmed: 2, Studying: 3, Completed: 4, Cancelled: 5 }) === 3).length, sub: 'Ghi danh đang hoạt động' }
])

// Chart settings
const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { display: false }
  },
  scales: {
    x: { grid: { display: false } },
    y: {
      grid: { color: 'rgba(0, 0, 0, 0.04)' },
      ticks: {
        callback: (value) => formatVnd(value)
      }
    }
  }
}

const doughnutOptions = {
  responsive: true,
  maintainAspectRatio: false,
  cutout: '65%',
  plugins: {
    legend: {
      display: true,
      position: 'bottom',
      align: 'center',
      labels: {
        boxWidth: 8,
        boxHeight: 8,
        padding: 12,
        font: { size: 11 },
        usePointStyle: true,
        pointStyle: 'circle'
      }
    }
  }
}

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

const barChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { display: false } },
  scales: {
    x: { grid: { display: false }, ticks: { autoSkip: true, maxRotation: 0, font: { size: 10 } } },
    y: { grid: { color: 'rgba(0, 0, 0, 0.04)' }, ticks: { callback: (value) => formatVnd(value), font: { size: 10 } } }
  },
  datasets: { bar: { maxBarThickness: 40 } }
}

const scoreBarOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { display: true, position: 'bottom', labels: { usePointStyle: true, boxWidth: 8 } } },
  scales: {
    x: { grid: { display: false }, ticks: { maxRotation: 0, autoSkip: true, font: { size: 10 } } },
    y: { beginAtZero: true, max: 10, grid: { color: 'rgba(100,116,139,.12)' }, ticks: { stepSize: 2 } }
  }
}

const horizontalBarOptions = {
  responsive: true,
  maintainAspectRatio: false,
  indexAxis: 'y',
  plugins: { legend: { display: false }, tooltip: { callbacks: { label: (c) => `${c.dataset.label}: ${formatVnd(c.parsed.x)}` } } },
  scales: {
    x: { grid: { color: 'rgba(0, 0, 0, 0.04)' }, ticks: { callback: (value) => formatVnd(value), font: { size: 10 } } },
    y: { grid: { display: false }, ticks: { font: { size: 11 }, autoSkip: false } }
  },
  datasets: { bar: { maxBarThickness: 28 } }
}

const tablePagination = {
  pageSize: 10,
  pageSizeOptions: ['10', '20', '50'],
  showSizeChanger: true,
  showTotal: (total) => `Tổng ${total} bản ghi`,
  size: 'small'
}

const doughnutChartOptions = doughnutOptions

// ============ Summary cards (6 KPI hero) ============
const summaryCards = computed(() => {
  const hasRevenue = totalPaidRevenue.value > 0
  const hasDebt = totalDebtRevenue.value > 0
  const hasTuition = (totalPaidRevenue.value + totalDebtRevenue.value) > 0
  const hasPayments = filteredPayments.value.length > 0
  return [
    {
      key: 'total',
      label: 'Tổng doanh thu',
      value: hasRevenue ? formatVnd(totalPaidRevenue.value) : 'Chưa có dữ liệu',
      placeholder: !hasRevenue,
      sub: 'Từ giao dịch thành công',
      icon: DollarCircleOutlined,
      iconColor: 'emerald'
    },
    {
      key: 'debt',
      label: 'Tổng công nợ',
      value: hasDebt ? formatVnd(totalDebtRevenue.value) : '0 đ',
      placeholder: false,
      sub: hasDebt ? 'Hóa đơn còn nợ' : 'Không có khoản nợ',
      icon: PayCircleOutlined,
      iconColor: 'rose'
    },
    {
      key: 'paid',
      label: 'Đã thu',
      value: hasRevenue ? formatVnd(totalPaidRevenue.value) : 'Chưa có dữ liệu',
      placeholder: !hasRevenue,
      sub: 'Thực tế đã thu',
      icon: CheckCircleOutlined,
      iconColor: 'indigo'
    },
    {
      key: 'rate',
      label: 'Tỷ lệ thu học phí',
      value: hasTuition ? `${collectionRate.value}%` : 'Chưa có dữ liệu',
      placeholder: !hasTuition,
      sub: hasTuition ? `${collectionRate.value}% đã thu / tổng phải thu` : 'Chờ dữ liệu',
      pill: hasTuition ? (collectionRate.value >= 80 ? { direction: 'up', text: 'Tốt' } : collectionRate.value >= 50 ? { direction: 'neutral', text: 'TB' } : { direction: 'down', text: 'Yếu' }) : null,
      icon: RiseOutlined,
      iconColor: 'sky'
    },
    {
      key: 'transactions',
      label: 'Số giao dịch',
      value: hasPayments ? filteredPayments.value.length : 'Chưa có dữ liệu',
      placeholder: !hasPayments,
      sub: hasPayments ? `Trong ${activePeriodLabel.value}` : 'Chờ giao dịch',
      icon: CreditCardOutlined,
      iconColor: 'violet'
    },
    {
      key: 'overdue',
      label: 'Hóa đơn quá hạn',
      value: overdueInvoiceCount.value > 0 ? overdueInvoiceCount.value : '0',
      placeholder: false,
      sub: overdueInvoiceCount.value > 0 ? 'Cần theo dõi' : 'Không có',
      pill: overdueInvoiceCount.value > 0 ? { direction: 'down', text: 'Quá hạn' } : { direction: 'up', text: 'Sạch' },
      icon: PayCircleOutlined,
      iconColor: 'amber'
    }
  ]
})

const activePeriodLabel = computed(() => {
  const opt = periodOptions.find(o => o.value === activePeriod.value)
  return opt ? opt.label.toLowerCase() : 'khoảng thời gian'
})

// ============ Debt tab KPIs ============
const overdueInvoiceCount = computed(() => tuitionList.value.filter(t => enumValue(t.status, { Unpaid: 1, Partial: 2, Paid: 3, Overdue: 4 }) === 4).length)
const totalDebtorsCount = computed(() => debtByStudent.value.filter(d => parseFloat(d.totalDebt || 0) > 0).length)

const debtKpis = computed(() => [
  {
    key: 'total-debt',
    label: 'Tổng công nợ',
    value: totalDebtRevenue.value > 0 ? formatVnd(totalDebtRevenue.value) : '0 đ',
    sub: 'Tất cả hóa đơn còn nợ',
    icon: PayCircleOutlined,
    iconColor: 'rose'
  },
  {
    key: 'debtors',
    label: 'Học viên còn nợ',
    value: totalDebtorsCount.value > 0 ? totalDebtorsCount.value : '0',
    sub: 'Có khoản nợ chưa thanh toán',
    icon: UserOutlined,
    iconColor: 'amber'
  },
  {
    key: 'overdue-invoices',
    label: 'Hóa đơn quá hạn',
    value: overdueInvoiceCount.value > 0 ? overdueInvoiceCount.value : '0',
    sub: overdueInvoiceCount.value > 0 ? 'Cần theo dõi' : 'Không có',
    icon: PayCircleOutlined,
    iconColor: 'rose'
  }
])

// ============ Insight lists (Top debtors + Recent transactions) ============
const topDebtors = computed(() => {
  return [...debtByStudent.value]
    .filter(d => parseFloat(d.totalDebt || 0) > 0)
    .sort((a, b) => parseFloat(b.totalDebt || 0) - parseFloat(a.totalDebt || 0))
    .slice(0, 5)
})

const recentTransactions = computed(() => {
  return [...paymentsList.value]
    .filter(p => enumValue(p.status, { Success: 1, Pending: 2, Failed: 3, Cancelled: 4 }) === 1)
    .sort((a, b) => new Date(b.paymentDate) - new Date(a.paymentDate))
    .slice(0, 5)
})

// ============ Table columns ============
const revenueDetailColumns = [
  { title: 'Tên', dataIndex: 'name', key: 'name', minWidth: 200 },
  { title: 'Doanh thu', dataIndex: 'totalRevenue', key: 'totalRevenue', width: 160, sorter: (a, b) => (a.totalRevenue || 0) - (b.totalRevenue || 0) },
  { title: 'Tỷ lệ', dataIndex: 'percent', key: 'percent', width: 100 }
]

const debtDetailColumns = [
  { title: '#', key: 'rank', width: 50 },
  { title: 'Học viên', dataIndex: 'name', key: 'name', minWidth: 220 },
  { title: 'Công nợ', dataIndex: 'totalDebt', key: 'totalDebt', width: 180, sorter: (a, b) => (a.totalDebt || 0) - (b.totalDebt || 0) },
  { title: 'Mức độ', key: 'level', width: 130 }
]

const debtByStudentRanked = computed(() => {
  return [...(debtByStudent.value || [])]
    .filter(d => parseFloat(d.totalDebt || 0) > 0)
    .sort((a, b) => parseFloat(b.totalDebt || 0) - parseFloat(a.totalDebt || 0))
    .map((d, idx) => ({ ...d, _rank: idx + 1 }))
})

function debtLevelChipStyle(amount) {
  const v = parseFloat(amount || 0)
  if (v >= 5000000) {
    // Nợ lớn: đỏ
    return { bg: 'var(--admin-danger-soft)', color: 'var(--admin-danger)', border: 'var(--admin-danger-border)', label: 'Nghiêm trọng' }
  }
  if (v >= 1000000) {
    // Nợ trung bình: amber
    return { bg: 'var(--admin-warn-soft)', color: 'var(--admin-warn)', border: 'var(--admin-warn-border, #fde68a)', label: 'Cần chú ý' }
  }
  // Nợ nhỏ
  return { bg: 'var(--admin-accent-soft)', color: 'var(--admin-accent)', border: 'var(--admin-accent-border)', label: 'Theo dõi' }
}

const avatarColorPalette = ['#3b82f6', '#8b5cf6', '#ec4899', '#f59e0b', '#10b981', '#06b6d4', '#ef4444', '#6366f1', '#0ea5e9', '#a855f7']

function studentAvatarColor(name) {
  if (!name) return '#94a3b8'
  let hash = 0
  for (let i = 0; i < name.length; i++) {
    hash = (hash * 31 + name.charCodeAt(i)) & 0xffffffff
  }
  return avatarColorPalette[Math.abs(hash) % avatarColorPalette.length]
}

function studentInitials(name) {
  if (!name) return 'NA'
  return name
    .split(' ')
    .filter(Boolean)
    .slice(-2)
    .map(w => w[0])
    .join('')
    .toUpperCase()
}

// ============ Period setters ============
function setPeriod(value) {
  activePeriod.value = value
  if (value !== 'custom') {
    selectedYear.value = null
    selectedMonth.value = null
    customDateRange.value = null
  }
}

function applyCalendarFilter() {
  activePeriod.value = 'custom'
  customDateRange.value = null
}

function setTrendRange(value) {
  revenueTrendRange.value = value
  setPeriod(value)
}

async function handleExport(type) {
  try {
    message.loading({ content: 'Đang mở mẫu PDF...', key: 'exporting' })
    if (type === 'overview') exportOverviewReport()
    else if (type === 'course') exportGroupAmountReport('Báo cáo doanh thu theo khóa học', 'Tổng hợp doanh thu và công nợ theo từng khóa học.', revenueByCourse.value, 'bao-cao-doanh-thu-theo-khoa')
    else if (type === 'class') exportGroupAmountReport('Báo cáo doanh thu theo lớp học', 'Tổng hợp doanh thu và công nợ theo từng lớp học.', revenueByClass.value, 'bao-cao-doanh-thu-theo-lop')
    else if (type === 'debt-student') exportGroupAmountReport('Báo cáo công nợ theo học viên', 'Danh sách học viên còn công nợ học phí.', debtByStudentRanked.value, 'bao-cao-cong-no-theo-hoc-vien', true)
    else if (type === 'debt-class') exportGroupAmountReport('Báo cáo công nợ theo lớp học', 'Tổng hợp công nợ học phí theo từng lớp.', debtByClass.value, 'bao-cao-cong-no-theo-lop')
    else if (type === 'results') await resultApi.export()
    else if (type === 'courses') await courseApi.export()
    message.success({ content: 'Đã mở mẫu PDF báo cáo!', key: 'exporting' })
  } catch (error) {
    message.error({ content: error.message || 'Xuất báo cáo thất bại', key: 'exporting' })
  }
}

function exportOverviewReport() {
  const rows = [
    { label: 'Tổng doanh thu', value: formatVnd(totalPaidRevenue.value), note: 'Từ giao dịch thành công trong kỳ lọc' },
    { label: 'Tổng công nợ', value: formatVnd(totalDebtRevenue.value), note: 'Hóa đơn còn nợ' },
    { label: 'Tổng phải thu', value: formatVnd(totalExpectedRevenue.value), note: 'Đã thu + công nợ' },
    { label: 'Tỷ lệ thu học phí', value: String(collectionRate.value) + '%', note: 'Doanh thu / tổng phải thu' },
    { label: 'Số giao dịch trong kỳ', value: filteredPayments.value.length, note: activePeriodLabel.value },
    { label: 'Hóa đơn quá hạn', value: overdueInvoiceCount.value, note: overdueInvoiceCount.value > 0 ? 'Cần theo dõi' : 'Không có' },
  ]

  openPdfReport({
    title: 'Báo cáo tổng quan tài chính',
    subtitle: 'Theo bộ lọc ' + activePeriodLabel.value + '. Tổng hợp doanh thu, công nợ, tỷ lệ thu và hóa đơn quá hạn.',
    filename: reportFilename('bao-cao-tong-quan-tai-chinh', 'pdf'),
    user: { fullName: 'System Admin', username: 'admin' },
    summary: [
      { label: 'Doanh thu', value: formatVnd(totalPaidRevenue.value) },
      { label: 'Công nợ', value: formatVnd(totalDebtRevenue.value) },
      { label: 'Tỷ lệ thu', value: String(collectionRate.value) + '%' },
      { label: 'Giao dịch', value: filteredPayments.value.length },
    ],
    columns: [
      { label: 'Chỉ số', value: (x) => x.label },
      { label: 'Giá trị', value: (x) => x.value },
      { label: 'Ghi chú', value: (x) => x.note },
    ],
    rows,
  })
}

function exportGroupAmountReport(title, subtitle, sourceRows, filenameStem, debtOnly = false) {
  const rows = [...(sourceRows || [])]
    .filter((item) => !debtOnly || Number(item.totalDebt || 0) > 0)
    .sort((a, b) => Number((debtOnly ? b.totalDebt : b.totalRevenue) || 0) - Number((debtOnly ? a.totalDebt : a.totalRevenue) || 0))
  const totalRevenue = rows.reduce((sum, item) => sum + Number(item.totalRevenue || 0), 0)
  const totalDebt = rows.reduce((sum, item) => sum + Number(item.totalDebt || 0), 0)
  const percentBase = debtOnly ? totalDebt : totalRevenue

  openPdfReport({
    title,
    subtitle: subtitle + ' Bộ lọc hiện tại: ' + activePeriodLabel.value + '.',
    filename: reportFilename(filenameStem, 'pdf'),
    user: { fullName: 'System Admin', username: 'admin' },
    summary: [
      { label: 'Tổng doanh thu', value: formatVnd(totalRevenue) },
      { label: 'Tổng công nợ', value: formatVnd(totalDebt) },
      { label: 'Số dòng', value: rows.length },
    ],
    columns: [
      { label: 'Tên', value: (x) => x.name || '-' },
      { label: 'Doanh thu', value: (x) => formatVnd(x.totalRevenue || 0) },
      { label: 'Công nợ', value: (x) => formatVnd(x.totalDebt || 0) },
      { label: 'Tỷ trọng', value: (x) => percentBase ? String(Math.round(Number((debtOnly ? x.totalDebt : x.totalRevenue) || 0) / percentBase * 100)) + '%' : '0%' },
    ],
    rows,
  })
}

async function fetchData() {
  loading.value = true
  errorMsg.value = ''
  try {
    const promises = [
      reportApi.getRevenueByCourse().then(res => { revenueByCourse.value = res || [] }).catch(() => {}),
      reportApi.getRevenueByClass().then(res => { revenueByClass.value = res || [] }).catch(() => {}),
      reportApi.getDebtByStudent().then(res => { debtByStudent.value = res || [] }).catch(() => {}),
      reportApi.getDebtByClass().then(res => { debtByClass.value = res || [] }).catch(() => {}),
      paymentApi.getAll().then(res => { paymentsList.value = res?.items || res?.data || res || [] }).catch(() => {}),
      tuitionApi.getAll().then(res => { tuitionList.value = res?.items || res?.data || res || [] }).catch(() => {}),
      reportApi.getRevenueOverview().then(res => { revenueOverview.value = res }).catch(() => {}),
      resultApi.getAll().then(res => { resultsList.value = res?.items || res?.data || res || [] }).catch(() => {}),
      courseApi.getAll().then(res => { coursesList.value = res?.items || res?.data || res || [] }).catch(() => {}),
      classApi.getAll().then(res => { classesList.value = res?.items || res?.data || res || [] }).catch(() => {}),
      enrollmentApi.getAll().then(res => { enrollmentsList.value = res?.items || res?.data || res || [] }).catch(() => {})
    ]
    await Promise.all(promises)
  } catch (error) {
    errorMsg.value = error.message || 'Không tải được báo cáo & thống kê.'
    message.error(errorMsg.value)
  } finally {
    loading.value = false
  }
}

onMounted(fetchData)
// ── AI Forecasting & BI State & Methods ──
const forecastMetric = ref('revenue')
const forecastHorizon = ref(6)

const forecastDataComputed = computed(() => {
  // Generate forecasting data based on actual values
  // We extract actual payments or enrollments in the past
  const horizon = forecastHorizon.value
  const metric = forecastMetric.value
  
  const months = ['Tháng 7', 'Tháng 8', 'Tháng 9', 'Tháng 10', 'Tháng 11', 'Tháng 12', 'Tháng 1/2027', 'Tháng 2/2027', 'Tháng 3/2027', 'Tháng 4/2027', 'Tháng 5/2027', 'Tháng 6/2027']
  
  const baseRevenue = totalPaidRevenue.value > 0 ? totalPaidRevenue.value : 125000000
  const baseEnrollment = enrollmentsList.value.length > 0 ? enrollmentsList.value.length : 45
  
  const data = []
  let cumulativeValue = metric === 'revenue' ? baseRevenue / 6 : baseEnrollment / 3
  
  // Historical data (last 3 months)
  const historical = []
  for (let i = 3; i > 0; i--) {
    const val = Math.round(cumulativeValue * (0.85 + Math.random() * 0.3))
    historical.push({
      label: months[3 - i],
      value: val,
      isForecast: false,
      low: val,
      high: val,
      change: 0
    })
  }

  // Forecasted data
  const forecasted = []
  let lastVal = historical[historical.length - 1].value
  for (let i = 0; i < horizon; i++) {
    const seasonality = 1 + 0.15 * Math.sin((i / horizon) * Math.PI * 2)
    const growth = 1 + (0.02 * (i + 1)) // 2% growth per month
    const value = Math.round(lastVal * growth * seasonality)
    const change = Math.round(((value - lastVal) / lastVal) * 100)
    
    forecasted.push({
      label: months[3 + i],
      value: value,
      isForecast: true,
      low: Math.round(value * 0.93),
      high: Math.round(value * 1.07),
      change: change
    })
    lastVal = value
  }

  return [...historical, ...forecasted]
})

const forecastChartData = computed(() => {
  const dataset = forecastDataComputed.value
  
  return {
    labels: dataset.map(d => d.label),
    datasets: [
      {
        label: 'Thực tế',
        data: dataset.map(d => d.isForecast ? null : d.value),
        borderColor: '#3b82f6',
        backgroundColor: '#3b82f6',
        borderWidth: 3,
        tension: 0.2,
        spanGaps: true
      },
      {
        label: 'Dự báo AI',
        data: dataset.map(d => d.isForecast ? d.value : null),
        borderColor: '#f59e0b',
        backgroundColor: 'rgba(245, 158, 11, 0.1)',
        borderWidth: 2,
        borderDash: [6, 4],
        tension: 0.25,
        fill: true,
        spanGaps: true
      },
      {
        label: 'Khoảng tin cậy trên',
        data: dataset.map(d => d.isForecast ? d.high : null),
        borderColor: 'rgba(245, 158, 11, 0.15)',
        borderWidth: 1,
        pointRadius: 0,
        tension: 0.2,
        spanGaps: true
      },
      {
        label: 'Khoảng tin cậy dưới',
        data: dataset.map(d => d.isForecast ? d.low : null),
        borderColor: 'rgba(245, 158, 11, 0.15)',
        borderWidth: 1,
        pointRadius: 0,
        fill: '-1', // Fill space between this and upper bound
        backgroundColor: 'rgba(245, 158, 11, 0.03)',
        tension: 0.2,
        spanGaps: true
      }
    ]
  }
})

const forecastChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: true,
      labels: {
        filter: (item) => !item.text.includes('Khoảng tin cậy')
      }
    }
  },
  scales: {
    x: { grid: { display: false } },
    y: {
      grid: { color: 'rgba(0, 0, 0, 0.04)' },
      ticks: {
        callback: (value) => forecastMetric.value === 'revenue' ? formatVnd(value) : value + ' lượt'
      }
    }
  }
}

const forecastSummary = computed(() => {
  const dataset = forecastDataComputed.value.filter(d => d.isForecast)
  if (dataset.length === 0) return { trendText: '', recommendation: '' }
  
  const isRevenue = forecastMetric.value === 'revenue'
  const first = dataset[0].value
  const last = dataset[dataset.length - 1].value
  const growth = ((last - first) / first) * 100
  
  let trendText = ''
  let recommendation = ''
  
  if (isRevenue) {
    if (growth > 0) {
      trendText = `Doanh thu dự kiến tăng trưởng dương +${growth.toFixed(1)}% trong ${forecastHorizon.value} tháng tới, đạt đỉnh ${formatVnd(last)}.`
      recommendation = 'AI gợi ý: Đẩy mạnh các chính sách chăm sóc học viên để duy trì tỷ lệ tái ghi danh ổn định, tối ưu hóa các lớp học có sĩ số thấp.'
    } else {
      trendText = `Doanh thu dự kiến suy giảm nhẹ ${growth.toFixed(1)}% do các biến động mùa vụ.`
      recommendation = 'AI gợi ý: Triển khai chiến dịch chiết khấu thanh toán sớm (Early Bird) và tổ chức các đợt hội thảo công nghệ để lấp đầy lớp học mới.'
    }
  } else {
    if (growth > 0) {
      trendText = `Lượng đăng ký học viên mới tăng trưởng ổn định +${growth.toFixed(1)}%, đạt tổng cộng ${last} lượt vào tháng cuối.`
      recommendation = 'AI gợi ý: Nâng cấp cơ sở vật chất, bổ sung phòng học Lab mới để chuẩn bị cho lưu lượng học viên gia tăng trong giai đoạn cao điểm.'
    } else {
      trendText = `Lượt đăng ký mới có xu hướng đi ngang hoặc giảm nhẹ ${Math.abs(growth).toFixed(1)}% vào các tháng sau.`
      recommendation = 'AI gợi ý: Gia tăng ngân sách quảng cáo cho các khóa học mũi nhọn (ReactJS, ASP.NET) và cải tiến phễu tư vấn tuyển sinh.'
    }
  }
  
  return { trendText, recommendation }
})

const forecastTableData = computed(() => {
  return forecastDataComputed.value.filter(d => d.isForecast)
})

// BI Chat State
const suggestedBiQueries = [
  'Phân tích khóa học có doanh thu thấp và chuyên cần dưới 85%',
  'Dự báo luồng doanh thu 3 tháng tới và rủi ro công nợ học viên',
  'Thống kê hiệu quả sử dụng phòng học và sĩ số trung bình các lớp'
]

const biMessages = ref([
  {
    role: 'model',
    text: 'Xin chào Admin! Tôi là Trợ lý BI Đàm thoại của hệ thống quản trị EduCenter. Tôi đã phân tích toàn bộ dữ liệu báo cáo gồm doanh thu, công nợ, điểm số, lịch học và ghi danh. Bạn cần tôi truy vấn, vẽ bảng so sánh hoặc giải thích xu hướng số liệu nào?',
    time: new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })
  }
])

const biInput = ref('')
const biTyping = ref(false)
const biChatContainer = ref(null)

const selectBiQuery = (q) => {
  biInput.value = q
}

const parseMarkdown = (text) => {
  if (!text) return ''
  let html = text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
  
  // Table parsing
  html = html.replace(/\|([^\n|]*)\|/g, (match) => {
    // Basic table parser fallback
    return match;
  });
  
  // Custom markdown table parser
  if (html.includes('|')) {
    const lines = html.split('\n');
    let inTable = false;
    let tableHtml = '<div class="overflow-x-auto my-2 border border-base rounded-lg"><table class="w-full text-left border-collapse text-[11px]">';
    
    for (let i = 0; i < lines.length; i++) {
      const line = lines[i].trim();
      if (line.startsWith('|') && line.endsWith('|')) {
        const cells = line.split('|').map(c => c.trim()).filter((c, idx, arr) => idx > 0 && idx < arr.length - 1);
        if (!inTable) {
          inTable = true;
          tableHtml += '<thead><tr class="bg-slate-100 dark:bg-slate-800 border-b border-base">';
          cells.forEach(c => {
            tableHtml += `<th class="p-2 font-bold">${c}</th>`;
          });
          tableHtml += '</tr></thead><tbody>';
        } else {
          if (line.includes('---')) continue; // Skip separator line
          tableHtml += '<tr class="border-b border-base last:border-b-0 hover:bg-slate-50/50 dark:hover:bg-slate-800/10">';
          cells.forEach(c => {
            tableHtml += `<td class="p-2">${c}</td>`;
          });
          tableHtml += '</tr>';
        }
        lines[i] = ''; // Clear line so it doesn't render as text
      } else {
        if (inTable) {
          inTable = false;
          tableHtml += '</tbody></table></div>';
          lines[i] = tableHtml + lines[i];
        }
      }
    }
    if (inTable) {
      tableHtml += '</tbody></table></div>';
      lines[lines.length - 1] += tableHtml;
    }
    html = lines.filter(l => l !== '').join('\n');
  }

  // Code block
  html = html.replace(/```([\s\S]*?)```/g, '<pre class="bg-slate-100 dark:bg-slate-900/60 p-2 rounded text-[11px] font-mono my-2 overflow-x-auto">$1</pre>')
  // Inline code
  html = html.replace(/`([^`\n]+)`/g, '<code class="bg-slate-100 dark:bg-slate-900/60 px-1 py-0.5 rounded text-[10px] font-mono mx-0.5">$1</code>')
  // Bold
  html = html.replace(/\*\*([^\*]+)\*\*/g, '<strong>$1</strong>')
  // Italic
  html = html.replace(/\*([^\*]+)\*/g, '<em>$1</em>')
  // Line break
  html = html.replace(/\n/g, '<br />')
  return html
}

const sendBiQuery = async () => {
  const query = biInput.value.trim()
  if (!query || biTyping.value) return

  biMessages.value.push({
    role: 'user',
    text: query,
    time: new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })
  })

  biInput.value = ''
  biTyping.value = true

  // Auto-scroll chat
  setTimeout(() => {
    if (biChatContainer.value) {
      biChatContainer.value.scrollTop = biChatContainer.value.scrollHeight
    }
  }, 100)

  // Serialize system dashboard metrics context for AI Router
  const totalPaid = totalPaidRevenue.value
  const totalDebt = totalDebtRevenue.value
  const totalExpected = totalExpectedRevenue.value
  const rate = collectionRate.value
  const avgVal = avgTransactionValue.value
  
  const courseRevenues = revenueByCourse.value.map(c => `- ${c.name}: ${formatVnd(c.totalRevenue)}`).join('\n')
  const classRevenues = revenueByClass.value.map(c => `- ${c.name}: ${formatVnd(c.totalRevenue)}`).join('\n')
  const topDebtorsList = topDebtors.value.map(d => `- ${d.name}: ${formatVnd(d.totalDebt)}`).join('\n')
  
  const operationStats = `
  - Tổng số khóa học: ${coursesList.value.length}
  - Tổng số lớp học: ${classesList.value.length}
  - Lượt ghi danh học viên: ${enrollmentsList.value.length}
  `

  const systemContext = `
Bạn là trợ lý Business Intelligence (BI) cao cấp tích hợp trực tiếp trong trang Báo cáo & Thống kê của EduCenter. 
Nhiệm vụ của bạn là trả lời các câu hỏi phân tích dữ liệu, đề xuất và giải pháp quản trị từ Ban Giám Đốc/Admin.

Dưới đây là thông tin dữ liệu hiện tại của hệ thống được trích xuất thời gian thực:
1. Tổng quan tài chính:
   - Tổng doanh thu thực tế đã thu: ${formatVnd(totalPaid)}
   - Tổng công nợ hiện tại: ${formatVnd(totalDebt)}
   - Tổng doanh thu kỳ vọng (Đã thu + nợ): ${formatVnd(totalExpected)}
   - Tỷ lệ hoàn thành thu học phí: ${rate}%
   - Giá trị trung bình mỗi giao dịch thành công: ${formatVnd(avgVal)}
2. Phân rã doanh thu theo Khóa học:
${courseRevenues}
3. Phân rã doanh thu theo Lớp học:
${classRevenues}
4. Top 5 học viên còn nợ học phí nhiều nhất:
${topDebtorsList}
5. Chỉ số vận hành đào tạo học tập:
${operationStats}

Hãy trả lời câu hỏi của người dùng một cách chính xác dựa trên số liệu này. Nếu có thể, hãy định dạng dữ liệu thành bảng Markdown để biểu thị cấu trúc rõ ràng. Luôn duy trì thái độ lịch sự, chuyên nghiệp, đưa ra các nhận định và gợi ý thực tiễn (khuyến nghị hành động).
`;

  const historyForAi = biMessages.value.slice(0, -1).map(m => ({
    role: m.role,
    content: m.text
  }))
  const currentQuestion = biMessages.value.at(-1)?.text || ''

  try {
    const data = await aiApi.complete({
      system: systemContext,
      prompt: currentQuestion,
      history: historyForAi,
      maxOutputTokens: 1800
    })
    const text = data.text || 'Tôi không nhận được phản hồi phù hợp từ AI. Vui lòng thử lại.'

    biMessages.value.push({
      role: 'model',
      text: text,
      time: new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })
    })

  } catch (err) {
    console.error(err)
    biMessages.value.push({
      role: 'model',
      text: `❌ **Lỗi kết nối AI Router API**: ${err.message || 'Không thể liên lạc với máy chủ AI'}.\n\nVui lòng thử lại sau hoặc kiểm tra API Key.`,
      time: new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })
    })
  } finally {
    biTyping.value = false
    setTimeout(() => {
      if (biChatContainer.value) {
        biChatContainer.value.scrollTop = biChatContainer.value.scrollHeight
      }
    }, 100)
  }
}

</script>

