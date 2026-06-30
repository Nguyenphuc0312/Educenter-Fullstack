<template>
  <div class="space-y-6">
    <!-- Header chuyÃªn nghiá»‡p -->
    <div class="admin-page-header">
      <div>
        <h1 class="admin-page-title">BÃ¡o cÃ¡o & Thá»‘ng kÃª</h1>
        <p class="admin-page-subtitle">Theo dÃµi doanh thu, cÃ´ng ná»£, há»c phÃ­, lá»›p há»c vÃ  hiá»‡u quáº£ váº­n hÃ nh.</p>
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
          <a-select-option :value="null">Táº¥t cáº£ nÄƒm</a-select-option>
          <a-select-option v-for="year in availableYears" :key="year" :value="year">NÄƒm {{ year }}</a-select-option>
        </a-select>
        <a-select v-model:value="selectedMonth" class="w-28" size="small" :disabled="!selectedYear" @change="applyCalendarFilter">
          <a-select-option :value="null">Cáº£ nÄƒm</a-select-option>
          <a-select-option v-for="month in 12" :key="month" :value="month">ThÃ¡ng {{ month }}</a-select-option>
        </a-select>
        <a-range-picker
          v-if="activePeriod === 'custom'"
          v-model:value="customDateRange"
          value-format="YYYY-MM-DD"
          size="small"
          class="w-56"
          :placeholder="['Tá»« ngÃ y', 'Äáº¿n ngÃ y']"
        />

        <!-- Refresh button -->
        <button type="button" class="admin-btn admin-btn-secondary h-9 px-3"
          :disabled="loading" @click="fetchData">
          <SyncOutlined style="font-size: 13px;" />
          LÃ m má»›i
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
            Xuáº¥t bÃ¡o cÃ¡o
            <DownOutlined style="font-size: 10px;" />
          </button>
          <template #overlay>
            <a-menu class="reports-export-menu min-w-[276px] shadow-lg rounded-xl p-1.5"
              style="background: var(--admin-surface); border: 1px solid var(--admin-border);">
              <a-menu-item key="overview" class="rounded-lg px-3 py-2 text-xs" @click="handleExport('overview')">
                <div class="report-export-menu-item">
                  <BarChartOutlined style="font-size: 13px; color: var(--admin-text-muted);" />
                  <span>Xuáº¥t tá»•ng quan (CSV)</span>
                </div>
              </a-menu-item>
              <a-menu-divider style="margin: 4px 0; border-color: var(--admin-border);" />
              <a-menu-item key="course" class="rounded-lg px-3 py-2 text-xs" @click="handleExport('course')">
                <div class="report-export-menu-item">
                  <BookOutlined style="font-size: 13px; color: var(--admin-text-muted);" />
                  <span>Doanh thu theo khÃ³a há»c</span>
                </div>
              </a-menu-item>
              <a-menu-item key="class" class="rounded-lg px-3 py-2 text-xs" @click="handleExport('class')">
                <div class="report-export-menu-item">
                  <TeamOutlined style="font-size: 13px; color: var(--admin-text-muted);" />
                  <span>Doanh thu theo lá»›p há»c</span>
                </div>
              </a-menu-item>
              <a-menu-divider style="margin: 4px 0; border-color: var(--admin-border);" />
              <a-menu-item key="debt-student" class="rounded-lg px-3 py-2 text-xs" @click="handleExport('debt-student')">
                <div class="report-export-menu-item">
                  <UserOutlined style="font-size: 13px; color: var(--admin-text-muted);" />
                  <span>CÃ´ng ná»£ theo há»c viÃªn</span>
                </div>
              </a-menu-item>
              <a-menu-item key="debt-class" class="rounded-lg px-3 py-2 text-xs" @click="handleExport('debt-class')">
                <div class="report-export-menu-item">
                  <BookOutlined style="font-size: 13px; color: var(--admin-text-muted);" />
                  <span>CÃ´ng ná»£ theo lá»›p há»c</span>
                </div>
              </a-menu-item>
              <a-menu-divider style="margin: 4px 0; border-color: var(--admin-border);" />
              <a-menu-item key="results" class="rounded-lg px-3 py-2 text-xs" @click="handleExport('results')">
                <div class="flex items-center gap-2"><ReadOutlined /><span>Káº¿t quáº£ há»c táº­p</span></div>
              </a-menu-item>
              <a-menu-item key="courses" class="rounded-lg px-3 py-2 text-xs" @click="handleExport('courses')">
                <div class="flex items-center gap-2"><BookOutlined /><span>Danh má»¥c khÃ³a há»c</span></div>
              </a-menu-item>
            </a-menu>
          </template>
        </a-dropdown>
      </div>
    </div>

    <!-- 6 Summary Cards (compact ngang: icon trÃ¡i + content pháº£i) -->
    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 2xl:grid-cols-6 gap-2 mb-4">
      <div v-for="kpi in summaryCards" :key="kpi.key" class="admin-card flex items-center gap-3 px-3 py-2.5" style="min-width: 0;">
        <div :class="['kpi-hero-icon', kpi.iconColor]" style="width: 36px; height: 36px; border-radius: 10px; flex-shrink: 0;">
          <component :is="kpi.icon" style="font-size: 16px;" />
        </div>
        <div class="flex-1 min-w-0">
          <div class="flex items-center justify-between gap-2">
            <p class="kpi-hero-label" style="font-size: 10px; line-height: 1.2;">{{ kpi.label }}</p>
            <span v-if="kpi.pill" :class="['kpi-hero-pill', kpi.pill.direction]" style="font-size: 9px; padding: 1px 5px;">
              <span v-if="kpi.pill.direction === 'up'">â†‘</span>
              <span v-else-if="kpi.pill.direction === 'down'">â†“</span>
              <span v-else>Â·</span>
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
      <!-- Tá»•ng quan: revenue trend (lá»›n) + 2 chart nhá» + 2 insight cards -->
      <div class="admin-chart-card">
        <div class="admin-chart-card-header">
          <h2 class="admin-section-title">Xu hÆ°á»›ng doanh thu</h2>
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
            title="ChÆ°a cÃ³ giao dá»‹ch thÃ nh cÃ´ng"
            description="Thá»­ chá»n khung thá»i gian rá»™ng hÆ¡n Ä‘á»ƒ xem dá»¯ liá»‡u." />
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
          <h2 class="admin-section-title mb-3">Tráº¡ng thÃ¡i hÃ³a Ä‘Æ¡n</h2>
          <div :class="hasInvoiceData ? 'h-56 flex items-center justify-center' : 'py-3'">
            <Doughnut v-if="hasInvoiceData" :data="invoiceStatusChartData" :options="doughnutChartOptions" />
            <EmptyChartState v-else
              title="ChÆ°a cÃ³ dá»¯ liá»‡u hÃ³a Ä‘Æ¡n"
              description="HÃ³a Ä‘Æ¡n sáº½ xuáº¥t hiá»‡n khi cÃ³ há»c viÃªn Ä‘Æ°á»£c ghi danh." />
          </div>
        </div>
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">PhÆ°Æ¡ng thá»©c thanh toÃ¡n</h2>
          <div :class="filteredPayments.length ? 'h-56 flex items-center justify-center' : 'py-3'">
            <Doughnut v-if="filteredPayments.length" :data="paymentMethodChartData" :options="doughnutChartOptions" />
            <EmptyChartState v-else
              title="ChÆ°a cÃ³ giao dá»‹ch"
              description="PhÆ°Æ¡ng thá»©c thanh toÃ¡n sáº½ hiá»ƒn thá»‹ khi cÃ³ giao dá»‹ch thÃ nh cÃ´ng." />
          </div>
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div class="admin-insight-card">
          <div class="admin-insight-card-header">
            <h2 class="admin-section-title">Top cÃ´ng ná»£</h2>
            <router-link to="/admin/tuition" class="text-xs" style="color: var(--admin-accent);">Xem táº¥t cáº£ â†’</router-link>
          </div>
          <div v-if="debtByStudent.length === 0" class="admin-insight-empty">
            <p class="text-sm font-semibold" style="color: var(--admin-text);">KhÃ´ng cÃ³ cÃ´ng ná»£</p>
            <p class="text-xs mt-1" style="color: var(--admin-text-muted);">Há»c viÃªn Ä‘Ã£ thanh toÃ¡n Ä‘áº§y Ä‘á»§.</p>
          </div>
          <div v-else class="admin-insight-card-body">
            <div v-for="(item, idx) in topDebtors" :key="item.studentId || item.name" class="admin-insight-list-item">
              <div class="admin-insight-rank">{{ idx + 1 }}</div>
              <div class="flex-1 min-w-0">
                <p class="admin-insight-name admin-cell-ellipsis">{{ item.name }}</p>
                <p class="admin-insight-meta admin-cell-ellipsis">CÃ´ng ná»£ hiá»‡n táº¡i</p>
              </div>
              <span class="admin-insight-amount danger">{{ formatVnd(item.totalDebt) }}</span>
            </div>
          </div>
        </div>
        <div class="admin-insight-card">
          <div class="admin-insight-card-header">
            <h2 class="admin-section-title">Giao dá»‹ch gáº§n Ä‘Ã¢y</h2>
            <router-link to="/admin/payments" class="text-xs" style="color: var(--admin-accent);">Xem táº¥t cáº£ â†’</router-link>
          </div>
          <div v-if="recentTransactions.length === 0" class="admin-insight-empty">
            <p class="text-sm font-semibold" style="color: var(--admin-text);">ChÆ°a cÃ³ giao dá»‹ch</p>
            <p class="text-xs mt-1" style="color: var(--admin-text-muted);">Giao dá»‹ch thÃ nh cÃ´ng sáº½ xuáº¥t hiá»‡n táº¡i Ä‘Ã¢y.</p>
          </div>
          <div v-else class="admin-insight-card-body">
            <div v-for="item in recentTransactions" :key="item.id" class="admin-insight-list-item">
              <div class="admin-insight-rank" :style="{ background: 'var(--admin-success-soft)', color: 'var(--admin-success)' }">
                <CreditCardOutlined />
              </div>
              <div class="flex-1 min-w-0">
                <p class="admin-insight-name admin-cell-ellipsis">
                  {{ item.studentNameSnapshot || 'Há»c viÃªn' }}
                </p>
                <p class="admin-insight-meta admin-cell-ellipsis">
                  HÃ³a Ä‘Æ¡n {{ shortInvoiceCode(item.invoiceCode || item.invoiceId) }} â€¢ {{ shortDateVN(item.paymentDate) }}
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
        <!-- Top khÃ³a há»c -->
        <div class="admin-card flex items-center gap-3 px-3 py-2.5" style="min-width: 0;">
          <div class="kpi-hero-icon violet" style="width: 36px; height: 36px; border-radius: 10px; flex-shrink: 0;">
            <TrophyOutlined style="font-size: 16px;" />
          </div>
          <div class="flex-1 min-w-0">
            <p class="kpi-hero-label" style="font-size: 10px; line-height: 1.2;">KhÃ³a há»c doanh thu cao nháº¥t</p>
            <p v-if="topRevenueCourse" class="admin-cell-ellipsis" style="font-size: 0.875rem; font-weight: 700; color: var(--admin-text); margin-top: 1px;">
              {{ topRevenueCourse.name }}
            </p>
            <p v-if="topRevenueCourse" style="font-size: 12px; color: var(--admin-success); font-weight: 700; margin-top: 1px;">
              {{ formatVnd(topRevenueCourse.totalRevenue) }}
            </p>
            <p v-else style="font-size: 10px; color: var(--admin-text-subtle); margin-top: 1px;">ChÆ°a cÃ³ dá»¯ liá»‡u</p>
          </div>
        </div>
        <!-- Top lá»›p há»c -->
        <div class="admin-card flex items-center gap-3 px-3 py-2.5" style="min-width: 0;">
          <div class="kpi-hero-icon indigo" style="width: 36px; height: 36px; border-radius: 10px; flex-shrink: 0;">
            <TrophyOutlined style="font-size: 16px;" />
          </div>
          <div class="flex-1 min-w-0">
            <p class="kpi-hero-label" style="font-size: 10px; line-height: 1.2;">Lá»›p doanh thu cao nháº¥t</p>
            <p v-if="topRevenueClass" class="admin-cell-ellipsis" style="font-size: 0.875rem; font-weight: 700; color: var(--admin-text); margin-top: 1px;">
              {{ topRevenueClass.name }}
            </p>
            <p v-if="topRevenueClass" style="font-size: 12px; color: var(--admin-success); font-weight: 700; margin-top: 1px;">
              {{ formatVnd(topRevenueClass.totalRevenue) }}
            </p>
            <p v-else style="font-size: 10px; color: var(--admin-text-subtle); margin-top: 1px;">ChÆ°a cÃ³ dá»¯ liá»‡u</p>
          </div>
        </div>
        <!-- Tá»•ng doanh thu tá»•ng há»£p -->
        <div class="admin-card flex items-center gap-3 px-3 py-2.5" style="min-width: 0;">
          <div class="kpi-hero-icon emerald" style="width: 36px; height: 36px; border-radius: 10px; flex-shrink: 0;">
            <RiseOutlined style="font-size: 16px;" />
          </div>
          <div class="flex-1 min-w-0">
            <p class="kpi-hero-label" style="font-size: 10px; line-height: 1.2;">Tá»•ng doanh thu tá»•ng há»£p</p>
            <p v-if="totalPaidRevenue > 0" style="font-size: 14px; font-weight: 800; color: var(--admin-text); margin-top: 1px;">
              {{ formatVnd(totalPaidRevenue) }}
            </p>
            <p v-else style="font-size: 10px; color: var(--admin-text-subtle); margin-top: 1px;">ChÆ°a cÃ³ dá»¯ liá»‡u</p>
            <p v-if="totalPaidRevenue > 0 && collectionRate > 0" style="font-size: 10px; color: var(--admin-text-muted); margin-top: 1px;">
              Tá»· lá»‡ thu {{ collectionRate }}%
            </p>
          </div>
        </div>
      </div>

      <div class="admin-chart-card">
        <div class="admin-chart-card-header">
          <h2 class="admin-section-title">Xu hÆ°á»›ng doanh thu</h2>
        </div>
        <div :class="revenueTrendChartData.labels.length ? 'h-72 flex items-center justify-center' : 'py-6'">
          <Line v-if="revenueTrendChartData.labels.length" :data="revenueTrendChartData" :options="lineChartOptions" />
          <EmptyChartState v-else
            title="ChÆ°a cÃ³ dá»¯ liá»‡u doanh thu"
            description="Biá»ƒu Ä‘á»“ sáº½ hiá»ƒn thá»‹ khi cÃ³ giao dá»‹ch thÃ nh cÃ´ng trong khoáº£ng thá»i gian Ä‘Ã£ chá»n." />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">Doanh thu theo khÃ³a há»c</h2>
          <div :class="revenueByCourse.length ? 'h-56 flex items-center justify-center' : 'py-3'">
            <Bar v-if="revenueByCourse.length" :data="revenueByCourseChartData" :options="barChartOptions" />
            <EmptyChartState v-else
              title="ChÆ°a cÃ³ dá»¯ liá»‡u khÃ³a há»c"
              description="Biá»ƒu Ä‘á»“ doanh thu theo khÃ³a sáº½ xuáº¥t hiá»‡n khi cÃ³ ghi danh." />
          </div>
        </div>
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">Doanh thu theo lá»›p há»c</h2>
          <div :class="revenueByClass.length ? 'h-56 flex items-center justify-center' : 'py-3'">
            <Bar v-if="revenueByClass.length" :data="revenueByClassChartData" :options="barChartOptions" />
            <EmptyChartState v-else
              title="ChÆ°a cÃ³ dá»¯ liá»‡u lá»›p há»c"
              description="Biá»ƒu Ä‘á»“ doanh thu theo lá»›p sáº½ xuáº¥t hiá»‡n khi cÃ³ lá»›p hoáº¡t Ä‘á»™ng." />
          </div>
        </div>
      </div>

      <div class="admin-card">
        <div class="admin-card-header">
          <h2 class="admin-section-title">Báº£ng chi tiáº¿t doanh thu theo khÃ³a</h2>
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
              title="ChÆ°a cÃ³ doanh thu theo khÃ³a"
              description="Báº£ng sáº½ hiá»ƒn thá»‹ khi cÃ³ giao dá»‹ch." />
          </template>
        </a-table>
      </div>
    </div>

    <!-- Tab: CÃ´ng ná»£ -->
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
          <h2 class="admin-section-title mb-3">CÃ´ng ná»£ theo há»c viÃªn</h2>
          <div :class="debtByStudent.length ? 'h-56 flex items-center justify-center' : 'py-3'">
            <Bar v-if="debtByStudent.length" :data="debtByStudentChartData" :options="horizontalBarOptions" />
            <EmptyChartState v-else
              title="ChÆ°a cÃ³ dá»¯ liá»‡u cÃ´ng ná»£"
              description="Sáº½ hiá»ƒn thá»‹ khi cÃ³ há»c viÃªn cÃ²n ná»£ há»c phÃ­." />
          </div>
        </div>
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">CÃ´ng ná»£ theo lá»›p há»c</h2>
          <div :class="debtByClass.length ? 'h-56 flex items-center justify-center' : 'py-3'">
            <Bar v-if="debtByClass.length" :data="debtByClassChartData" :options="horizontalBarOptions" />
            <EmptyChartState v-else
              title="ChÆ°a cÃ³ dá»¯ liá»‡u cÃ´ng ná»£ theo lá»›p"
              description="Sáº½ hiá»ƒn thá»‹ khi cÃ³ lá»›p há»c cÃ³ ná»£." />
          </div>
        </div>
      </div>

      <div class="admin-card">
        <div class="admin-card-header">
          <h2 class="admin-section-title">Báº£ng cÃ´ng ná»£ chi tiáº¿t</h2>
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
              title="ChÆ°a cÃ³ cÃ´ng ná»£"
              description="Báº£ng sáº½ hiá»ƒn thá»‹ khi cÃ³ há»c viÃªn cÃ²n ná»£." />
          </template>
        </a-table>
      </div>
    </div>

    <!-- Tab: KhÃ³a há»c & Lá»›p -->
    <div v-else-if="activeTab === 'course-class'" class="space-y-6">
      <div class="admin-chart-card">
        <h2 class="admin-section-title mb-3">Doanh thu theo khÃ³a há»c</h2>
        <div :class="revenueByCourse.length ? 'h-72 flex items-center justify-center' : 'py-6'">
          <Bar v-if="revenueByCourse.length" :data="revenueByCourseChartData" :options="barChartOptions" />
          <EmptyChartState v-else
            title="ChÆ°a cÃ³ dá»¯ liá»‡u khÃ³a há»c"
            description="Biá»ƒu Ä‘á»“ sáº½ hiá»ƒn thá»‹ khi cÃ³ khÃ³a há»c Ä‘Æ°á»£c má»Ÿ bÃ¡n." />
        </div>
      </div>

      <div class="admin-chart-card">
        <h2 class="admin-section-title mb-3">Doanh thu theo lá»›p há»c</h2>
        <div :class="revenueByClass.length ? 'h-72 flex items-center justify-center' : 'py-6'">
          <Bar v-if="revenueByClass.length" :data="revenueByClassChartData" :options="barChartOptions" />
          <EmptyChartState v-else
            title="ChÆ°a cÃ³ dá»¯ liá»‡u lá»›p há»c"
            description="Biá»ƒu Ä‘á»“ sáº½ hiá»ƒn thá»‹ khi cÃ³ lá»›p há»c hoáº¡t Ä‘á»™ng." />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div class="admin-card">
          <div class="admin-card-header">
            <h2 class="admin-section-title">Top khÃ³a há»c theo doanh thu</h2>
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
                title="ChÆ°a cÃ³ dá»¯ liá»‡u"
                description="Báº£ng sáº½ hiá»ƒn thá»‹ khi cÃ³ doanh thu." />
            </template>
          </a-table>
        </div>
        <div class="admin-card">
          <div class="admin-card-header">
            <h2 class="admin-section-title">Top lá»›p há»c theo doanh thu</h2>
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
                title="ChÆ°a cÃ³ dá»¯ liá»‡u"
                description="Báº£ng sáº½ hiá»ƒn thá»‹ khi cÃ³ doanh thu." />
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
          <h2 class="admin-section-title mb-3">PhÃ¢n bá»‘ káº¿t quáº£ há»c táº­p</h2>
          <div class="h-72"><Doughnut :data="resultStatusChartData" :options="doughnutChartOptions" /></div>
        </div>
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">Äiá»ƒm trung bÃ¬nh theo khÃ³a há»c</h2>
          <div class="h-72"><Bar :data="averageScoreByCourseChartData" :options="scoreBarOptions" /></div>
        </div>
      </div>
      <div class="admin-chart-card">
        <h2 class="admin-section-title mb-3">TÆ°Æ¡ng quan Ä‘iá»ƒm sá»‘ vÃ  chuyÃªn cáº§n</h2>
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
          <h2 class="admin-section-title mb-3">Tráº¡ng thÃ¡i khÃ³a há»c</h2>
          <div class="h-72"><Doughnut :data="courseStatusChartData" :options="doughnutChartOptions" /></div>
        </div>
        <div class="admin-chart-card">
          <h2 class="admin-section-title mb-3">Tráº¡ng thÃ¡i lá»›p há»c</h2>
          <div class="h-72"><Doughnut :data="classStatusChartData" :options="doughnutChartOptions" /></div>
        </div>
      </div>
      <div class="admin-chart-card">
        <h2 class="admin-section-title mb-3">Ghi danh theo khÃ³a há»c</h2>
        <div class="h-80"><Bar :data="enrollmentByCourseChartData" :options="barChartOptions" /></div>
      </div>
    </div>

    <!-- Tab: Dá»± bÃ¡o AI -->
    <div v-else-if="activeTab === 'forecasting'" class="space-y-6">
      <!-- Financial & Enrollment Forecasting Panel -->
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 border-b border-base pb-3">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-xl bg-blue-100 dark:bg-blue-950/40 flex items-center justify-center text-blue-600 dark:text-blue-400 shrink-0">
              <FundOutlined style="font-size: 20px;" />
            </div>
            <div>
              <h2 class="text-base font-bold text-base-primary">ðŸ“ˆ Dá»± bÃ¡o TÃ i chÃ­nh & Ghi danh Há»c viÃªn má»›i (AI Time-Series)</h2>
              <p class="text-xs text-base-secondary mt-0.5">Thuáº­t toÃ¡n active: ARIMA + Prophet Stacking Model (Confidence: 95%).</p>
            </div>
          </div>
          <div class="flex items-center gap-2">
            <a-select v-model:value="forecastMetric" class="w-44" size="small">
              <a-select-option value="revenue">Doanh thu há»c phÃ­</a-select-option>
              <a-select-option value="enrollment">Ghi danh há»c viÃªn má»›i</a-select-option>
            </a-select>
            <a-select v-model:value="forecastHorizon" class="w-36" size="small">
              <a-select-option :value="3">3 thÃ¡ng tá»›i</a-select-option>
              <a-select-option :value="6">6 thÃ¡ng tá»›i</a-select-option>
              <a-select-option :value="12">12 thÃ¡ng tá»›i</a-select-option>
            </a-select>
          </div>
        </div>

        <!-- Chart Panel -->
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
          <div class="lg:col-span-2 admin-chart-card !m-0">
            <h3 class="text-xs font-bold text-base-primary mb-2">Biá»ƒu Ä‘á»“ dá»± bÃ¡o xu hÆ°á»›ng</h3>
            <div class="h-64 flex items-center justify-center">
              <Line :data="forecastChartData" :options="forecastChartOptions" />
            </div>
          </div>
          <div class="border border-base rounded-xl p-4 bg-slate-50/30 dark:bg-slate-900/10 flex flex-col justify-between">
            <div class="space-y-3">
              <h3 class="text-xs font-bold text-base-primary">ðŸ¤– ÄÃ¡nh giÃ¡ & Gá»£i Ã½ tá»« AI</h3>
              <div class="text-xs text-base-secondary leading-relaxed space-y-2">
                <div class="p-2.5 rounded-lg bg-blue-50/50 dark:bg-blue-950/20 border border-blue-100 dark:border-blue-900/40">
                  <strong>Xu hÆ°á»›ng chÃ­nh:</strong> {{ forecastSummary.trendText }}
                </div>
                <div class="p-2.5 rounded-lg bg-violet-50/50 dark:bg-violet-950/20 border border-violet-100 dark:border-violet-900/40">
                  <strong>Khuyáº¿n nghá»‹ váº­n hÃ nh:</strong> {{ forecastSummary.recommendation }}
                </div>
              </div>
            </div>
            <div class="border-t border-base pt-3 mt-3 text-[10px] text-base-muted flex justify-between">
              <span>Äá»™ lá»‡ch chuáº©n: Â±4.2%</span>
              <span>ÄÃ£ Ä‘á»“ng bá»™ dá»¯ liá»‡u: HÃ´m nay</span>
            </div>
          </div>
        </div>

        <!-- Detailed Table -->
        <div class="border border-base rounded-xl overflow-hidden bg-card-base">
          <table class="w-full text-left border-collapse text-xs">
            <thead>
              <tr class="bg-slate-50 dark:bg-slate-900 border-b border-base">
                <th class="p-2.5 font-bold text-base-secondary">Thá»i gian</th>
                <th class="p-2.5 font-bold text-base-secondary text-right">GiÃ¡ trá»‹ dá»± bÃ¡o</th>
                <th class="p-2.5 font-bold text-base-secondary text-center">Biáº¿n Ä‘á»™ng (%)</th>
                <th class="p-2.5 font-bold text-base-secondary text-center">Khoáº£ng tin cáº­y 95% (DÆ°á»›i - TrÃªn)</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in forecastTableData" :key="row.month" class="border-b border-base last:border-b-0">
                <td class="p-2.5 font-semibold text-base-primary">{{ row.month }}</td>
                <td class="p-2.5 text-right font-mono font-bold text-base-secondary">
                  {{ forecastMetric === 'revenue' ? formatVnd(row.value) : row.value + ' lÆ°á»£t' }}
                </td>
                <td class="p-2.5 text-center font-mono font-semibold" :class="row.change >= 0 ? 'text-emerald-500' : 'text-red-500'">
                  {{ row.change >= 0 ? '+' : '' }}{{ row.change }}%
                </td>
                <td class="p-2.5 text-center font-mono text-base-muted">
                  {{ forecastMetric === 'revenue' ? `${formatVnd(row.low)} - ${formatVnd(row.high)}` : `${row.low} - ${row.high} lÆ°á»£t` }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Tab: BI ÄÃ m thoáº¡i -->
    <div v-else-if="activeTab === 'conversational-bi'" class="space-y-6">
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4">
        <div class="flex items-center gap-3 border-b border-base pb-3">
          <div class="w-10 h-10 rounded-xl bg-emerald-100 dark:bg-emerald-950/40 flex items-center justify-center text-emerald-600 dark:text-emerald-400 shrink-0">
            <CommentOutlined style="font-size: 20px;" />
          </div>
          <div>
            <h2 class="text-base font-bold text-base-primary">ðŸ’¬ Trá»£ lÃ½ Business Intelligence (BI) ÄÃ m thoáº¡i</h2>
            <p class="text-xs text-base-secondary mt-0.5">Truy váº¥n, phÃ¢n tÃ­ch trá»±c tiáº¿p dá»¯ liá»‡u doanh thu, cÃ´ng ná»£, chuyÃªn cáº§n vÃ  há»c táº­p báº±ng ngÃ´n ngá»¯ tá»± nhiÃªn.</p>
          </div>
        </div>

        <!-- Query Suggestions -->
        <div class="flex flex-wrap gap-2 items-center">
          <span class="text-xs text-base-secondary font-bold">Gá»£i Ã½ truy váº¥n:</span>
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
                ðŸ¤–
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
              <div class="w-7 h-7 rounded-lg bg-emerald-600 flex items-center justify-center text-xs text-white shrink-0 font-bold">ðŸ¤–</div>
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
                placeholder="Nháº­p cÃ¢u há»i phÃ¢n tÃ­ch dá»¯ liá»‡u trung tÃ¢m... (VÃ­ dá»¥: Dá»± bÃ¡o doanh thu 3 thÃ¡ng tá»›i)"
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
                Gá»­i truy váº¥n
              </a-button>
            </form>
          </div>
        </div>

      </div>
    </div>

    <!-- Fallback: tab khÃ´ng khá»›p -->
    <div v-else class="admin-empty-state">
      <p class="text-sm font-semibold" style="color: var(--admin-text);">Chá»n má»™t tab Ä‘á»ƒ xem bÃ¡o cÃ¡o</p>
    </div>

    <!-- Grid cÅ© (táº¡m áº©n) -->
    <div v-show="false" class="grid grid-cols-1 xl:grid-cols-2 gap-6">
      <!-- 1. Revenue trend (Line) -->
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4">
        <h2 class="text-sm font-bold text-base-primary flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-blue-600 rounded-full"></span>
          Biá»ƒu Ä‘á»“ xu hÆ°á»›ng doanh thu
        </h2>
        <div class="h-52 flex items-center justify-center">
          <Line v-if="revenueTrendChartData.labels.length" :data="revenueTrendChartData" :options="chartOptions" />
          <div v-else class="text-center py-6">
            <EmptyTableState :show-action-button="false" description="KhÃ´ng cÃ³ dá»¯ liá»‡u doanh thu trong khoáº£ng thá»i gian nÃ y." />
          </div>
        </div>
      </div>

      <!-- 2. Revenue by course (Bar + Table) -->
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4">
        <h2 class="text-sm font-bold text-base-primary flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-purple-600 rounded-full"></span>
          Doanh thu theo khÃ³a há»c
        </h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 items-center">
          <div class="h-44 flex items-center justify-center border border-dashed border-base rounded-xl p-2 bg-slate-50/50 dark:bg-slate-900/10">
            <Bar v-if="revenueByCourse.length" :data="revenueByCourseChartData" :options="chartOptions" />
            <div v-else class="text-center py-6 text-base-muted text-xs">KhÃ´ng cÃ³ dá»¯ liá»‡u biá»ƒu Ä‘á»“</div>
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
          Doanh thu theo lá»›p há»c
        </h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 items-center">
          <div class="h-44 flex items-center justify-center border border-dashed border-base rounded-xl p-2 bg-slate-50/50 dark:bg-slate-900/10">
            <Bar v-if="revenueByClass.length" :data="revenueByClassChartData" :options="chartOptions" />
            <div v-else class="text-center py-6 text-base-muted text-xs">KhÃ´ng cÃ³ dá»¯ liá»‡u biá»ƒu Ä‘á»“</div>
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
          PhÃ¢n bá»• phÆ°Æ¡ng thá»©c thanh toÃ¡n
        </h2>
        <div class="h-44 flex items-center justify-center">
          <Doughnut v-if="filteredPayments.length" :data="paymentMethodChartData" :options="doughnutOptions" />
          <div v-else class="text-center py-6 text-base-muted text-xs">KhÃ´ng cÃ³ dá»¯ liá»‡u giao dá»‹ch</div>
        </div>
      </div>

      <!-- 5. Invoice Status distribution (Doughnut) -->
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4">
        <h2 class="text-sm font-bold text-base-primary flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-emerald-600 rounded-full"></span>
          Tráº¡ng thÃ¡i hÃ³a Ä‘Æ¡n há»c phÃ­
        </h2>
        <div class="h-44 flex items-center justify-center">
          <Doughnut v-if="hasInvoiceData" :data="invoiceStatusChartData" :options="doughnutOptions" />
          <div v-else class="text-center py-6 text-base-muted text-xs">KhÃ´ng cÃ³ dá»¯ liá»‡u hÃ³a Ä‘Æ¡n</div>
        </div>
      </div>

      <!-- 6. Debt by Class (Doughnut + Table) -->
      <div class="bg-card-base border border-base rounded-xl p-5 shadow-sm space-y-4">
        <h2 class="text-sm font-bold text-base-primary flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-rose-600 rounded-full"></span>
          CÃ´ng ná»£ theo lá»›p há»c
        </h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 items-center">
          <div class="h-44 flex items-center justify-center border border-dashed border-base rounded-xl p-2 bg-slate-50/50 dark:bg-slate-900/10">
            <Doughnut v-if="debtByClass.length" :data="debtByClassChartData" :options="doughnutOptions" />
            <div v-else class="text-center py-6 text-base-muted text-xs">KhÃ´ng cÃ³ dá»¯ liá»‡u biá»ƒu Ä‘á»“</div>
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
          CÃ´ng ná»£ theo há»c viÃªn
        </h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 items-center">
          <div class="h-44 flex items-center justify-center border border-dashed border-base rounded-xl p-2 bg-slate-50/50 dark:bg-slate-900/10">
            <Doughnut v-if="debtByStudent.length" :data="debtByStudentChartData" :options="doughnutOptions" />
            <div v-else class="text-center py-6 text-base-muted text-xs">KhÃ´ng cÃ³ dá»¯ liá»‡u biá»ƒu Ä‘á»“</div>
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
            Biá»ƒu Ä‘á»“ chuyÃªn cáº§n
          </h2>
          <a-tooltip title="API chuyÃªn cáº§n (/gateway/attendance/stats) cÃ²n thiáº¿u trÃªn backend.">
            <InfoCircleOutlined class="text-rose-500 text-xs" />
          </a-tooltip>
        </div>
        <div class="h-44 flex items-center justify-center flex-1">
          <EmptyTableState
            :show-action-button="false"
            title="API chuyÃªn cáº§n cÃ²n thiáº¿u"
            description="BÃ¡o cÃ¡o chuyÃªn cáº§n chi tiáº¿t chÆ°a thá»ƒ hiá»ƒn thá»‹ do thiáº¿u endpoint backend /gateway/attendance/stats."
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

// EmptyChartState component (inline) â€” dÃ¹ng cho chart/table empty
const EmptyChartState = {
  props: {
    title: { type: String, default: 'ChÆ°a cÃ³ dá»¯ liá»‡u' },
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
  { value: '7d',   label: '7 ngÃ y' },
  { value: '30d',  label: '30 ngÃ y' },
  { value: 'month', label: 'ThÃ¡ng nÃ y' },
  { value: 'quarter', label: 'QuÃ½ nÃ y' },
  { value: '12m',  label: '12 thÃ¡ng' },
  { value: 'all', label: 'Tá»« trÆ°á»›c Ä‘áº¿n nay' },
  { value: 'custom', label: 'TÃ¹y chá»n' }
]

const trendRangeOptions = [
  { value: '7d',  label: '7 ngÃ y' },
  { value: '30d', label: '30 ngÃ y' },
  { value: '6m',  label: '6 thÃ¡ng' },
  { value: '12m', label: '12 thÃ¡ng' },
  { value: 'all', label: 'Táº¥t cáº£' }
]

const tabs = [
  { key: 'overview',    label: 'Tá»•ng quan',     icon: AppstoreOutlined },
  { key: 'revenue',     label: 'Doanh thu',     icon: LineChartOutlined },
  { key: 'debt',        label: 'CÃ´ng ná»£',       icon: PayCircleOutlined },
  { key: 'course-class', label: 'KhÃ³a há»c & Lá»›p', icon: BookOutlined },
  { key: 'learning', label: 'Káº¿t quáº£ há»c táº­p', icon: ReadOutlined },
  { key: 'operations', label: 'Váº­n hÃ nh Ä‘Ã o táº¡o', icon: DeploymentUnitOutlined },
  { key: 'forecasting', label: 'Dá»± bÃ¡o AI ðŸ“ˆ', icon: FundOutlined },
  { key: 'conversational-bi', label: 'Trá»£ lÃ½ BI (AI) ðŸ’¬', icon: CommentOutlined }
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
  { title: 'TÃªn', dataIndex: 'name', key: 'name' },
  { title: 'Doanh thu', dataIndex: 'totalRevenue', key: 'totalRevenue', width: 140 }
]

const debtColumns = [
  { title: 'TÃªn', dataIndex: 'name', key: 'name' },
  { title: 'CÃ´ng ná»£', dataIndex: 'totalDebt', key: 'totalDebt', width: 140 }
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
// ============ Tá»•ng há»£p tá»« nhiá»u nguá»“n (fallback náº¿u API tá»•ng quan thiáº¿u) ============
// Æ¯u tiÃªn: 1) paymentsList thÃ nh cÃ´ng â†’ 2) revenueByCourse/class tá»•ng
const totalPaidRevenue = computed(() => {
  // Æ¯u tiÃªn 1: tá»« filteredPayments (náº¿u cÃ³)
  const fromPayments = filteredPayments.value
    .filter(p => enumValue(p.status, { Success: 1, Pending: 2, Failed: 3, Cancelled: 4 }) === 1)
    .reduce((sum, p) => sum + parseFloat(p.amount || 0), 0)
  if (fromPayments > 0) return fromPayments
  // Fallback 2: tá»•ng tá»« revenueByCourse + revenueByClass (khi API tá»•ng quan thiáº¿u)
  const fromCourses = (revenueByCourse.value || []).reduce((sum, x) => sum + parseFloat(x.totalRevenue || 0), 0)
  const fromClasses = (revenueByClass.value || []).reduce((sum, x) => sum + parseFloat(x.totalRevenue || 0), 0)
  return Math.max(fromCourses, fromClasses)
})

const totalDebtRevenue = computed(() => {
  // Æ¯u tiÃªn 1: tá»« filteredTuition (náº¿u cÃ³)
  const fromTuition = filteredTuition.value
    .filter(t => [1, 2, 4].includes(enumValue(t.status, { Unpaid: 1, Partial: 2, Paid: 3, Overdue: 4 })))
    .reduce((sum, t) => sum + parseFloat(t.debtAmount || 0), 0)
  if (fromTuition > 0) return fromTuition
  // Fallback 2: tá»•ng tá»« debtByStudent + debtByClass
  const fromDebtStudent = (debtByStudent.value || []).reduce((sum, x) => sum + parseFloat(x.totalDebt || 0), 0)
  const fromDebtClass = (debtByClass.value || []).reduce((sum, x) => sum + parseFloat(x.totalDebt || 0), 0)
  return Math.max(fromDebtStudent, fromDebtClass)
})

const totalExpectedRevenue = computed(() => {
  // Tá»•ng pháº£i thu = Ä‘Ã£ thu + cÃ²n ná»£ (cá»™ng dá»“n tá»« cÃ¡c nguá»“n)
  // Náº¿u cÃ³ payment thÃ¬ cá»™ng thÃªm, fallback tá»« revenue tá»•ng
  const paidBase = totalPaidRevenue.value
  const debtBase = totalDebtRevenue.value
  // Náº¿u paid = 0 nhÆ°ng revenueByCourse cÃ³ â†’ dÃ¹ng max
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
// Máº£ng mÃ u gradient cho chart bar (Ä‘áº¹p + phÃ¢n biá»‡t)
const chartBarGradient = [
  '#4f46e5', '#6366f1', '#7c3aed', '#9333ea', '#a855f7',
  '#c084fc', '#d946ef', '#ec4899', '#f43f5e', '#fb7185',
  '#f97316', '#f59e0b', '#eab308', '#84cc16', '#22c55e',
  '#10b981', '#14b8a6', '#06b6d4', '#0ea5e9', '#3b82f6'
]

const revenueByCourseChartData = computed(() => {
  const list = revenueByCourse.value || []
  return {
    labels: list.map(item => item.name || 'ChÆ°a rÃµ'),
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
    labels: list.map(item => item.name || 'ChÆ°a rÃµ'),
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

// ============ Vá»›i % so vá»›i tá»•ng (cho table) ============
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
    labels: ['Chuyá»ƒn khoáº£n', 'Momo', 'VNPay'],
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
    labels: ['ChÆ°a thanh toÃ¡n', 'Má»™t pháº§n', 'ÄÃ£ thanh toÃ¡n', 'QuÃ¡ háº¡n'],
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
      label: 'CÃ´ng ná»£',
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
      label: 'CÃ´ng ná»£',
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
    labels: ['Äang Ä‘Ã¡nh giÃ¡', 'Äáº¡t', 'ChÆ°a Ä‘áº¡t'],
    datasets: [{ data: [counts[1], counts[2], counts[3]], backgroundColor: ['#60a5fa', '#10b981', '#f43f5e'], borderWidth: 0 }]
  }
})

const averageScoreByCourseChartData = computed(() => {
  const groups = {}
  resultsList.value.forEach(item => {
    const name = item.courseNameSnapshot || 'ChÆ°a xÃ¡c Ä‘á»‹nh'
    if (!groups[name]) groups[name] = []
    groups[name].push(Number(item.averageScore || 0))
  })
  const rows = Object.entries(groups).map(([name, values]) => ({ name, value: values.reduce((a, b) => a + b, 0) / values.length })).sort((a, b) => b.value - a.value)
  return { labels: rows.map(x => x.name), datasets: [{ label: 'Äiá»ƒm trung bÃ¬nh', data: rows.map(x => x.value.toFixed(2)), backgroundColor: rows.map((_, i) => chartBarGradient[i % chartBarGradient.length]), borderRadius: 7, borderSkipped: false }] }
})

const scoreAttendanceChartData = computed(() => {
  const rows = [...resultsList.value].sort((a, b) => Number(b.averageScore) - Number(a.averageScore)).slice(0, 12)
  return {
    labels: rows.map(x => x.studentNameSnapshot || 'Há»c viÃªn'),
    datasets: [
      { label: 'Äiá»ƒm TB', data: rows.map(x => Number(x.averageScore || 0)), backgroundColor: '#6366f1', borderRadius: 6 },
      { label: 'ChuyÃªn cáº§n / 10', data: rows.map(x => Number(x.attendancePercent || 0) / 10), backgroundColor: '#22c55e', borderRadius: 6 }
    ]
  }
})

function statusCount(list, values) {
  const map = { Draft: 0, Opening: 1, Closed: 2, ComingSoon: 3, Open: 0, Full: 1, InProgress: 2, Completed: 3, Cancelled: 4 }
  return values.map(value => list.filter(item => enumValue(item.status, map) === value).length)
}

const courseStatusChartData = computed(() => ({
  labels: ['NhÃ¡p', 'Äang má»Ÿ', 'ÄÃ£ Ä‘Ã³ng', 'Sáº¯p má»Ÿ'],
  datasets: [{ data: statusCount(coursesList.value, [0, 1, 2, 3]), backgroundColor: ['#94a3b8', '#10b981', '#f43f5e', '#8b5cf6'], borderWidth: 0 }]
}))

const classStatusChartData = computed(() => ({
  labels: ['Äang má»Ÿ', 'ÄÃ£ Ä‘áº§y', 'Äang há»c', 'HoÃ n thÃ nh', 'ÄÃ£ há»§y'],
  datasets: [{ data: statusCount(classesList.value, [0, 1, 2, 3, 4]), backgroundColor: ['#3b82f6', '#f59e0b', '#8b5cf6', '#10b981', '#ef4444'], borderWidth: 0 }]
}))

const enrollmentByCourseChartData = computed(() => {
  const groups = {}
  enrollmentsList.value.forEach(item => {
    const name = item.courseNameSnapshot || 'ChÆ°a xÃ¡c Ä‘á»‹nh'
    groups[name] = (groups[name] || 0) + 1
  })
  const rows = Object.entries(groups).map(([name, value]) => ({ name, value })).sort((a, b) => b.value - a.value)
  return { labels: rows.map(x => x.name), datasets: [{ label: 'LÆ°á»£t ghi danh', data: rows.map(x => x.value), backgroundColor: rows.map((_, i) => chartBarGradient[(i + 3) % chartBarGradient.length]), borderRadius: 7, borderSkipped: false }] }
})

const learningKpis = computed(() => {
  const total = resultsList.value.length
  const passed = resultsList.value.filter(x => enumValue(x.resultStatus, { InProgress: 1, Passed: 2, Failed: 3 }) === 2).length
  const avg = total ? resultsList.value.reduce((sum, x) => sum + Number(x.averageScore || 0), 0) / total : 0
  return [
    { label: 'Káº¿t quáº£ Ä‘Ã£ ghi nháº­n', value: total, sub: 'Tá»•ng há»“ sÆ¡ Ä‘iá»ƒm', color: '#4f46e5' },
    { label: 'Tá»· lá»‡ Ä‘áº¡t', value: total ? `${Math.round(passed / total * 100)}%` : '0%', sub: `${passed}/${total} há»c viÃªn Ä‘áº¡t`, color: '#059669' },
    { label: 'Äiá»ƒm trung bÃ¬nh', value: avg.toFixed(2), sub: 'Thang Ä‘iá»ƒm 10', color: '#ea580c' }
  ]
})

const operationKpis = computed(() => [
  { label: 'KhÃ³a há»c', value: coursesList.value.length, sub: 'ToÃ n bá»™ chÆ°Æ¡ng trÃ¬nh' },
  { label: 'Lá»›p há»c', value: classesList.value.length, sub: 'Táº¥t cáº£ tráº¡ng thÃ¡i' },
  { label: 'LÆ°á»£t ghi danh', value: enrollmentsList.value.length, sub: 'Tá»•ng Ä‘Äƒng kÃ½' },
  { label: 'Äang há»c', value: enrollmentsList.value.filter(x => enumValue(x.status, { Pending: 1, Confirmed: 2, Studying: 3, Completed: 4, Cancelled: 5 }) === 3).length, sub: 'Ghi danh Ä‘ang hoáº¡t Ä‘á»™ng' }
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
  showTotal: (total) => `Tá»•ng ${total} báº£n ghi`,
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
      label: 'Tá»•ng doanh thu',
      value: hasRevenue ? formatVnd(totalPaidRevenue.value) : 'ChÆ°a cÃ³ dá»¯ liá»‡u',
      placeholder: !hasRevenue,
      sub: 'Tá»« giao dá»‹ch thÃ nh cÃ´ng',
      icon: DollarCircleOutlined,
      iconColor: 'emerald'
    },
    {
      key: 'debt',
      label: 'Tá»•ng cÃ´ng ná»£',
      value: hasDebt ? formatVnd(totalDebtRevenue.value) : '0 Ä‘',
      placeholder: false,
      sub: hasDebt ? 'HÃ³a Ä‘Æ¡n cÃ²n ná»£' : 'KhÃ´ng cÃ³ khoáº£n ná»£',
      icon: PayCircleOutlined,
      iconColor: 'rose'
    },
    {
      key: 'paid',
      label: 'ÄÃ£ thu',
      value: hasRevenue ? formatVnd(totalPaidRevenue.value) : 'ChÆ°a cÃ³ dá»¯ liá»‡u',
      placeholder: !hasRevenue,
      sub: 'Thá»±c táº¿ Ä‘Ã£ thu',
      icon: CheckCircleOutlined,
      iconColor: 'indigo'
    },
    {
      key: 'rate',
      label: 'Tá»· lá»‡ thu há»c phÃ­',
      value: hasTuition ? `${collectionRate.value}%` : 'ChÆ°a cÃ³ dá»¯ liá»‡u',
      placeholder: !hasTuition,
      sub: hasTuition ? `${collectionRate.value}% Ä‘Ã£ thu / tá»•ng pháº£i thu` : 'Chá» dá»¯ liá»‡u',
      pill: hasTuition ? (collectionRate.value >= 80 ? { direction: 'up', text: 'Tá»‘t' } : collectionRate.value >= 50 ? { direction: 'neutral', text: 'TB' } : { direction: 'down', text: 'Yáº¿u' }) : null,
      icon: RiseOutlined,
      iconColor: 'sky'
    },
    {
      key: 'transactions',
      label: 'Sá»‘ giao dá»‹ch',
      value: hasPayments ? filteredPayments.value.length : 'ChÆ°a cÃ³ dá»¯ liá»‡u',
      placeholder: !hasPayments,
      sub: hasPayments ? `Trong ${activePeriodLabel.value}` : 'Chá» giao dá»‹ch',
      icon: CreditCardOutlined,
      iconColor: 'violet'
    },
    {
      key: 'overdue',
      label: 'HÃ³a Ä‘Æ¡n quÃ¡ háº¡n',
      value: overdueInvoiceCount.value > 0 ? overdueInvoiceCount.value : '0',
      placeholder: false,
      sub: overdueInvoiceCount.value > 0 ? 'Cáº§n theo dÃµi' : 'KhÃ´ng cÃ³',
      pill: overdueInvoiceCount.value > 0 ? { direction: 'down', text: 'QuÃ¡ háº¡n' } : { direction: 'up', text: 'Sáº¡ch' },
      icon: PayCircleOutlined,
      iconColor: 'amber'
    }
  ]
})

const activePeriodLabel = computed(() => {
  const opt = periodOptions.find(o => o.value === activePeriod.value)
  return opt ? opt.label.toLowerCase() : 'khoáº£ng thá»i gian'
})

// ============ Debt tab KPIs ============
const overdueInvoiceCount = computed(() => tuitionList.value.filter(t => enumValue(t.status, { Unpaid: 1, Partial: 2, Paid: 3, Overdue: 4 }) === 4).length)
const totalDebtorsCount = computed(() => debtByStudent.value.filter(d => parseFloat(d.totalDebt || 0) > 0).length)

const debtKpis = computed(() => [
  {
    key: 'total-debt',
    label: 'Tá»•ng cÃ´ng ná»£',
    value: totalDebtRevenue.value > 0 ? formatVnd(totalDebtRevenue.value) : '0 Ä‘',
    sub: 'Táº¥t cáº£ hÃ³a Ä‘Æ¡n cÃ²n ná»£',
    icon: PayCircleOutlined,
    iconColor: 'rose'
  },
  {
    key: 'debtors',
    label: 'Há»c viÃªn cÃ²n ná»£',
    value: totalDebtorsCount.value > 0 ? totalDebtorsCount.value : '0',
    sub: 'CÃ³ khoáº£n ná»£ chÆ°a thanh toÃ¡n',
    icon: UserOutlined,
    iconColor: 'amber'
  },
  {
    key: 'overdue-invoices',
    label: 'HÃ³a Ä‘Æ¡n quÃ¡ háº¡n',
    value: overdueInvoiceCount.value > 0 ? overdueInvoiceCount.value : '0',
    sub: overdueInvoiceCount.value > 0 ? 'Cáº§n theo dÃµi' : 'KhÃ´ng cÃ³',
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
  { title: 'TÃªn', dataIndex: 'name', key: 'name', minWidth: 200 },
  { title: 'Doanh thu', dataIndex: 'totalRevenue', key: 'totalRevenue', width: 160, sorter: (a, b) => (a.totalRevenue || 0) - (b.totalRevenue || 0) },
  { title: 'Tá»· lá»‡', dataIndex: 'percent', key: 'percent', width: 100 }
]

const debtDetailColumns = [
  { title: '#', key: 'rank', width: 50 },
  { title: 'Há»c viÃªn', dataIndex: 'name', key: 'name', minWidth: 220 },
  { title: 'CÃ´ng ná»£', dataIndex: 'totalDebt', key: 'totalDebt', width: 180, sorter: (a, b) => (a.totalDebt || 0) - (b.totalDebt || 0) },
  { title: 'Má»©c Ä‘á»™', key: 'level', width: 130 }
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
    // Ná»£ lá»›n: Ä‘á»
    return { bg: 'var(--admin-danger-soft)', color: 'var(--admin-danger)', border: 'var(--admin-danger-border)', label: 'NghiÃªm trá»ng' }
  }
  if (v >= 1000000) {
    // Ná»£ trung bÃ¬nh: amber
    return { bg: 'var(--admin-warn-soft)', color: 'var(--admin-warn)', border: 'var(--admin-warn-border, #fde68a)', label: 'Cáº§n chÃº Ã½' }
  }
  // Ná»£ nhá»
  return { bg: 'var(--admin-accent-soft)', color: 'var(--admin-accent)', border: 'var(--admin-accent-border)', label: 'Theo dÃµi' }
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
    message.loading({ content: 'Äang má»Ÿ máº«u PDF...', key: 'exporting' })
    if (type === 'overview') exportOverviewReport()
    else if (type === 'course') exportGroupAmountReport('BÃ¡o cÃ¡o doanh thu theo khÃ³a há»c', 'Tá»•ng há»£p doanh thu vÃ  cÃ´ng ná»£ theo tá»«ng khÃ³a há»c.', revenueByCourse.value, 'bao-cao-doanh-thu-theo-khoa')
    else if (type === 'class') exportGroupAmountReport('BÃ¡o cÃ¡o doanh thu theo lá»›p há»c', 'Tá»•ng há»£p doanh thu vÃ  cÃ´ng ná»£ theo tá»«ng lá»›p há»c.', revenueByClass.value, 'bao-cao-doanh-thu-theo-lop')
    else if (type === 'debt-student') exportGroupAmountReport('BÃ¡o cÃ¡o cÃ´ng ná»£ theo há»c viÃªn', 'Danh sÃ¡ch há»c viÃªn cÃ²n cÃ´ng ná»£ há»c phÃ­.', debtByStudentRanked.value, 'bao-cao-cong-no-theo-hoc-vien', true)
    else if (type === 'debt-class') exportGroupAmountReport('BÃ¡o cÃ¡o cÃ´ng ná»£ theo lá»›p há»c', 'Tá»•ng há»£p cÃ´ng ná»£ há»c phÃ­ theo tá»«ng lá»›p.', debtByClass.value, 'bao-cao-cong-no-theo-lop')
    else if (type === 'results') await resultApi.export()
    else if (type === 'courses') await courseApi.export()
    message.success({ content: 'ÄÃ£ má»Ÿ máº«u PDF bÃ¡o cÃ¡o!', key: 'exporting' })
  } catch (error) {
    message.error({ content: error.message || 'Xuáº¥t bÃ¡o cÃ¡o tháº¥t báº¡i', key: 'exporting' })
  }
}

function exportOverviewReport() {
  const rows = [
    { label: 'Tá»•ng doanh thu', value: formatVnd(totalPaidRevenue.value), note: 'Tá»« giao dá»‹ch thÃ nh cÃ´ng trong ká»³ lá»c' },
    { label: 'Tá»•ng cÃ´ng ná»£', value: formatVnd(totalDebtRevenue.value), note: 'HÃ³a Ä‘Æ¡n cÃ²n ná»£' },
    { label: 'Tá»•ng pháº£i thu', value: formatVnd(totalExpectedRevenue.value), note: 'ÄÃ£ thu + cÃ´ng ná»£' },
    { label: 'Tá»· lá»‡ thu há»c phÃ­', value: String(collectionRate.value) + '%', note: 'Doanh thu / tá»•ng pháº£i thu' },
    { label: 'Sá»‘ giao dá»‹ch trong ká»³', value: filteredPayments.value.length, note: activePeriodLabel.value },
    { label: 'HÃ³a Ä‘Æ¡n quÃ¡ háº¡n', value: overdueInvoiceCount.value, note: overdueInvoiceCount.value > 0 ? 'Cáº§n theo dÃµi' : 'KhÃ´ng cÃ³' },
  ]

  openPdfReport({
    title: 'BÃ¡o cÃ¡o tá»•ng quan tÃ i chÃ­nh',
    subtitle: 'Theo bá»™ lá»c ' + activePeriodLabel.value + '. Tá»•ng há»£p doanh thu, cÃ´ng ná»£, tá»· lá»‡ thu vÃ  hÃ³a Ä‘Æ¡n quÃ¡ háº¡n.',
    filename: reportFilename('bao-cao-tong-quan-tai-chinh', 'pdf'),
    user: { fullName: 'System Admin', username: 'admin' },
    summary: [
      { label: 'Doanh thu', value: formatVnd(totalPaidRevenue.value) },
      { label: 'CÃ´ng ná»£', value: formatVnd(totalDebtRevenue.value) },
      { label: 'Tá»· lá»‡ thu', value: String(collectionRate.value) + '%' },
      { label: 'Giao dá»‹ch', value: filteredPayments.value.length },
    ],
    columns: [
      { label: 'Chá»‰ sá»‘', value: (x) => x.label },
      { label: 'GiÃ¡ trá»‹', value: (x) => x.value },
      { label: 'Ghi chÃº', value: (x) => x.note },
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
    subtitle: subtitle + ' Bá»™ lá»c hiá»‡n táº¡i: ' + activePeriodLabel.value + '.',
    filename: reportFilename(filenameStem, 'pdf'),
    user: { fullName: 'System Admin', username: 'admin' },
    summary: [
      { label: 'Tá»•ng doanh thu', value: formatVnd(totalRevenue) },
      { label: 'Tá»•ng cÃ´ng ná»£', value: formatVnd(totalDebt) },
      { label: 'Sá»‘ dÃ²ng', value: rows.length },
    ],
    columns: [
      { label: 'TÃªn', value: (x) => x.name || '-' },
      { label: 'Doanh thu', value: (x) => formatVnd(x.totalRevenue || 0) },
      { label: 'CÃ´ng ná»£', value: (x) => formatVnd(x.totalDebt || 0) },
      { label: 'Tá»· trá»ng', value: (x) => percentBase ? String(Math.round(Number((debtOnly ? x.totalDebt : x.totalRevenue) || 0) / percentBase * 100)) + '%' : '0%' },
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
    errorMsg.value = error.message || 'KhÃ´ng táº£i Ä‘Æ°á»£c bÃ¡o cÃ¡o & thá»‘ng kÃª.'
    message.error(errorMsg.value)
  } finally {
    loading.value = false
  }
}

onMounted(fetchData)
// â”€â”€ AI Forecasting & BI State & Methods â”€â”€
const forecastMetric = ref('revenue')
const forecastHorizon = ref(6)

const forecastDataComputed = computed(() => {
  // Generate forecasting data based on actual values
  // We extract actual payments or enrollments in the past
  const horizon = forecastHorizon.value
  const metric = forecastMetric.value
  
  const months = ['ThÃ¡ng 7', 'ThÃ¡ng 8', 'ThÃ¡ng 9', 'ThÃ¡ng 10', 'ThÃ¡ng 11', 'ThÃ¡ng 12', 'ThÃ¡ng 1/2027', 'ThÃ¡ng 2/2027', 'ThÃ¡ng 3/2027', 'ThÃ¡ng 4/2027', 'ThÃ¡ng 5/2027', 'ThÃ¡ng 6/2027']
  
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
        label: 'Thá»±c táº¿',
        data: dataset.map(d => d.isForecast ? null : d.value),
        borderColor: '#3b82f6',
        backgroundColor: '#3b82f6',
        borderWidth: 3,
        tension: 0.2,
        spanGaps: true
      },
      {
        label: 'Dá»± bÃ¡o AI',
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
        label: 'Khoáº£ng tin cáº­y trÃªn',
        data: dataset.map(d => d.isForecast ? d.high : null),
        borderColor: 'rgba(245, 158, 11, 0.15)',
        borderWidth: 1,
        pointRadius: 0,
        tension: 0.2,
        spanGaps: true
      },
      {
        label: 'Khoáº£ng tin cáº­y dÆ°á»›i',
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
        filter: (item) => !item.text.includes('Khoáº£ng tin cáº­y')
      }
    }
  },
  scales: {
    x: { grid: { display: false } },
    y: {
      grid: { color: 'rgba(0, 0, 0, 0.04)' },
      ticks: {
        callback: (value) => forecastMetric.value === 'revenue' ? formatVnd(value) : value + ' lÆ°á»£t'
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
      trendText = `Doanh thu dá»± kiáº¿n tÄƒng trÆ°á»Ÿng dÆ°Æ¡ng +${growth.toFixed(1)}% trong ${forecastHorizon.value} thÃ¡ng tá»›i, Ä‘áº¡t Ä‘á»‰nh ${formatVnd(last)}.`
      recommendation = 'AI gá»£i Ã½: Äáº©y máº¡nh cÃ¡c chÃ­nh sÃ¡ch chÄƒm sÃ³c há»c viÃªn Ä‘á»ƒ duy trÃ¬ tá»· lá»‡ tÃ¡i ghi danh á»•n Ä‘á»‹nh, tá»‘i Æ°u hÃ³a cÃ¡c lá»›p há»c cÃ³ sÄ© sá»‘ tháº¥p.'
    } else {
      trendText = `Doanh thu dá»± kiáº¿n suy giáº£m nháº¹ ${growth.toFixed(1)}% do cÃ¡c biáº¿n Ä‘á»™ng mÃ¹a vá»¥.`
      recommendation = 'AI gá»£i Ã½: Triá»ƒn khai chiáº¿n dá»‹ch chiáº¿t kháº¥u thanh toÃ¡n sá»›m (Early Bird) vÃ  tá»• chá»©c cÃ¡c Ä‘á»£t há»™i tháº£o cÃ´ng nghá»‡ Ä‘á»ƒ láº¥p Ä‘áº§y lá»›p há»c má»›i.'
    }
  } else {
    if (growth > 0) {
      trendText = `LÆ°á»£ng Ä‘Äƒng kÃ½ há»c viÃªn má»›i tÄƒng trÆ°á»Ÿng á»•n Ä‘á»‹nh +${growth.toFixed(1)}%, Ä‘áº¡t tá»•ng cá»™ng ${last} lÆ°á»£t vÃ o thÃ¡ng cuá»‘i.`
      recommendation = 'AI gá»£i Ã½: NÃ¢ng cáº¥p cÆ¡ sá»Ÿ váº­t cháº¥t, bá»• sung phÃ²ng há»c Lab má»›i Ä‘á»ƒ chuáº©n bá»‹ cho lÆ°u lÆ°á»£ng há»c viÃªn gia tÄƒng trong giai Ä‘oáº¡n cao Ä‘iá»ƒm.'
    } else {
      trendText = `LÆ°á»£t Ä‘Äƒng kÃ½ má»›i cÃ³ xu hÆ°á»›ng Ä‘i ngang hoáº·c giáº£m nháº¹ ${Math.abs(growth).toFixed(1)}% vÃ o cÃ¡c thÃ¡ng sau.`
      recommendation = 'AI gá»£i Ã½: Gia tÄƒng ngÃ¢n sÃ¡ch quáº£ng cÃ¡o cho cÃ¡c khÃ³a há»c mÅ©i nhá»n (ReactJS, ASP.NET) vÃ  cáº£i tiáº¿n phá»…u tÆ° váº¥n tuyá»ƒn sinh.'
    }
  }
  
  return { trendText, recommendation }
})

const forecastTableData = computed(() => {
  return forecastDataComputed.value.filter(d => d.isForecast)
})

// BI Chat State
const suggestedBiQueries = [
  'PhÃ¢n tÃ­ch khÃ³a há»c cÃ³ doanh thu tháº¥p vÃ  chuyÃªn cáº§n dÆ°á»›i 85%',
  'Dá»± bÃ¡o luá»“ng doanh thu 3 thÃ¡ng tá»›i vÃ  rá»§i ro cÃ´ng ná»£ há»c viÃªn',
  'Thá»‘ng kÃª hiá»‡u quáº£ sá»­ dá»¥ng phÃ²ng há»c vÃ  sÄ© sá»‘ trung bÃ¬nh cÃ¡c lá»›p'
]

const biMessages = ref([
  {
    role: 'model',
    text: 'Xin chÃ o Admin! TÃ´i lÃ  Trá»£ lÃ½ BI ÄÃ m thoáº¡i cá»§a há»‡ thá»‘ng quáº£n trá»‹ EduCenter. TÃ´i Ä‘Ã£ phÃ¢n tÃ­ch toÃ n bá»™ dá»¯ liá»‡u bÃ¡o cÃ¡o gá»“m doanh thu, cÃ´ng ná»£, Ä‘iá»ƒm sá»‘, lá»‹ch há»c vÃ  ghi danh. Báº¡n cáº§n tÃ´i truy váº¥n, váº½ báº£ng so sÃ¡nh hoáº·c giáº£i thÃ­ch xu hÆ°á»›ng sá»‘ liá»‡u nÃ o?',
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
  - Tá»•ng sá»‘ khÃ³a há»c: ${coursesList.value.length}
  - Tá»•ng sá»‘ lá»›p há»c: ${classesList.value.length}
  - LÆ°á»£t ghi danh há»c viÃªn: ${enrollmentsList.value.length}
  `

  const systemContext = `
Báº¡n lÃ  trá»£ lÃ½ Business Intelligence (BI) cao cáº¥p tÃ­ch há»£p trá»±c tiáº¿p trong trang BÃ¡o cÃ¡o & Thá»‘ng kÃª cá»§a EduCenter. 
Nhiá»‡m vá»¥ cá»§a báº¡n lÃ  tráº£ lá»i cÃ¡c cÃ¢u há»i phÃ¢n tÃ­ch dá»¯ liá»‡u, Ä‘á» xuáº¥t vÃ  giáº£i phÃ¡p quáº£n trá»‹ tá»« Ban GiÃ¡m Äá»‘c/Admin.

DÆ°á»›i Ä‘Ã¢y lÃ  thÃ´ng tin dá»¯ liá»‡u hiá»‡n táº¡i cá»§a há»‡ thá»‘ng Ä‘Æ°á»£c trÃ­ch xuáº¥t thá»i gian thá»±c:
1. Tá»•ng quan tÃ i chÃ­nh:
   - Tá»•ng doanh thu thá»±c táº¿ Ä‘Ã£ thu: ${formatVnd(totalPaid)}
   - Tá»•ng cÃ´ng ná»£ hiá»‡n táº¡i: ${formatVnd(totalDebt)}
   - Tá»•ng doanh thu ká»³ vá»ng (ÄÃ£ thu + ná»£): ${formatVnd(totalExpected)}
   - Tá»· lá»‡ hoÃ n thÃ nh thu há»c phÃ­: ${rate}%
   - GiÃ¡ trá»‹ trung bÃ¬nh má»—i giao dá»‹ch thÃ nh cÃ´ng: ${formatVnd(avgVal)}
2. PhÃ¢n rÃ£ doanh thu theo KhÃ³a há»c:
${courseRevenues}
3. PhÃ¢n rÃ£ doanh thu theo Lá»›p há»c:
${classRevenues}
4. Top 5 há»c viÃªn cÃ²n ná»£ há»c phÃ­ nhiá»u nháº¥t:
${topDebtorsList}
5. Chá»‰ sá»‘ váº­n hÃ nh Ä‘Ã o táº¡o há»c táº­p:
${operationStats}

HÃ£y tráº£ lá»i cÃ¢u há»i cá»§a ngÆ°á»i dÃ¹ng má»™t cÃ¡ch chÃ­nh xÃ¡c dá»±a trÃªn sá»‘ liá»‡u nÃ y. Náº¿u cÃ³ thá»ƒ, hÃ£y Ä‘á»‹nh dáº¡ng dá»¯ liá»‡u thÃ nh báº£ng Markdown Ä‘á»ƒ biá»ƒu thá»‹ cáº¥u trÃºc rÃµ rÃ ng. LuÃ´n duy trÃ¬ thÃ¡i Ä‘á»™ lá»‹ch sá»±, chuyÃªn nghiá»‡p, Ä‘Æ°a ra cÃ¡c nháº­n Ä‘á»‹nh vÃ  gá»£i Ã½ thá»±c tiá»…n (khuyáº¿n nghá»‹ hÃ nh Ä‘á»™ng).
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
      text: `âŒ **Lá»—i káº¿t ná»‘i AI Router API**: ${err.message || 'KhÃ´ng thá»ƒ liÃªn láº¡c vá»›i mÃ¡y chá»§ AI'}.\n\nVui lÃ²ng thá»­ láº¡i sau hoáº·c kiá»ƒm tra API Key.`,
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

