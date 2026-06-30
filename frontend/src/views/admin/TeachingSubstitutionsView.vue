<template>
  <section class="sub-page">
    <!-- ── Page Header ── -->
    <div class="sub-header">
      <div class="sub-header-left">
        <div class="sub-header-icon">
          <UserSwitchOutlined />
        </div>
        <div>
          <h1 class="sub-title">Duyệt dạy thay</h1>
          <p class="sub-subtitle">Quản lý yêu cầu giảng viên dạy thay từng buổi học</p>
        </div>
      </div>

      <button class="sub-refresh-btn" :class="{ loading }" @click="loadItems">
        <SyncOutlined :spin="loading" />
        <span>Làm mới</span>
      </button>
    </div>

    <!-- ── Table Card ── -->
    <div class="sub-table-card">
      <a-table
        :data-source="sortedItems"
        :columns="columns"
        :loading="loading"
        row-key="id"
        size="middle"
        :scroll="{ x: 1500 }"
        :pagination="pagination"
        :row-class-name="rowClassName"
        class="sub-table"
      >
        <template #bodyCell="{ column, record }">
          <!-- Buổi học -->
          <template v-if="column.key === 'schedule'">
            <div class="schedule-cell">
              <div class="schedule-top">
                <span class="class-badge">{{ record.classNameSnapshot || 'Lớp học' }}</span>
                <span v-if="isToday(record)" class="today-badge">HÔM NAY</span>
              </div>
              <div class="schedule-meta">
                <span><CalendarOutlined /> {{ dayLabel(record.dayOfWeek) }}</span>
                <span><ClockCircleOutlined /> {{ formatTime(record.startTime) }} – {{ formatTime(record.endTime) }}</span>
                <span><HomeOutlined /> {{ record.room || 'Chưa xếp phòng' }}</span>
              </div>
            </div>
          </template>

          <!-- Giảng viên -->
          <template v-else-if="column.key === 'teachers'">
            <div class="teacher-flow">
              <!-- GV xin nghỉ -->
              <a-popover
                trigger="hover"
                placement="rightTop"
                overlay-class-name="teacher-detail-popover"
              >
                <template #content>
                  <div class="teacher-popover-card">
                    <div class="teacher-popover-header">
                      <div
                        class="teacher-popover-avatar"
                        :style="{ background: avatarColor(record.requestingTeacherNameSnapshot) }"
                      >
                        {{ initials(record.requestingTeacherNameSnapshot) }}
                      </div>
                      <div class="min-w-0">
                        <h3>{{ record.requestingTeacherNameSnapshot || 'Giảng viên' }}</h3>
                        <p class="popover-role popover-role--leave">Xin nghỉ buổi này</p>
                      </div>
                    </div>
                    <div class="teacher-popover-info">
                      <div><span>Buổi học</span><strong>{{ record.classNameSnapshot || '—' }}</strong></div>
                      <div><span>Thứ</span><strong>{{ dayLabel(record.dayOfWeek) }}</strong></div>
                      <div><span>Thời gian</span><strong>{{ formatTime(record.startTime) }} – {{ formatTime(record.endTime) }}</strong></div>
                      <div><span>Phòng</span><strong>{{ record.room || 'Chưa xếp' }}</strong></div>
                    </div>
                    <div class="teacher-popover-reason" v-if="record.reason">
                      <span>Lý do</span>
                      <strong>{{ record.reason }}</strong>
                    </div>
                  </div>
                </template>

                <div class="teacher-node teacher-node--leave">
                  <div
                    class="teacher-avatar"
                    :style="{ background: avatarColor(record.requestingTeacherNameSnapshot) }"
                  >
                    {{ initials(record.requestingTeacherNameSnapshot) }}
                  </div>
                  <div class="teacher-info">
                    <strong>{{ record.requestingTeacherNameSnapshot || 'Chưa rõ' }}</strong>
                    <span class="role-tag role-tag--leave">Xin nghỉ</span>
                  </div>
                </div>
              </a-popover>

              <div class="flow-arrow"><ArrowRightOutlined /></div>

              <!-- GV dạy thay -->
              <a-popover
                trigger="hover"
                placement="rightTop"
                overlay-class-name="teacher-detail-popover"
              >
                <template #content>
                  <div class="teacher-popover-card">
                    <div class="teacher-popover-header">
                      <div
                        class="teacher-popover-avatar"
                        :style="{ background: avatarColor(record.substituteTeacherNameSnapshot) }"
                      >
                        {{ initials(record.substituteTeacherNameSnapshot) }}
                      </div>
                      <div class="min-w-0">
                        <h3>{{ record.substituteTeacherNameSnapshot || 'Giảng viên' }}</h3>
                        <p class="popover-role popover-role--sub">Dạy thay buổi này</p>
                      </div>
                    </div>
                    <div class="teacher-popover-info">
                      <div><span>Buổi học</span><strong>{{ record.classNameSnapshot || '—' }}</strong></div>
                      <div><span>Thứ</span><strong>{{ dayLabel(record.dayOfWeek) }}</strong></div>
                      <div><span>Thời gian</span><strong>{{ formatTime(record.startTime) }} – {{ formatTime(record.endTime) }}</strong></div>
                      <div><span>Phòng</span><strong>{{ record.room || 'Chưa xếp' }}</strong></div>
                    </div>
                    <div class="teacher-popover-status">
                      <span>Trạng thái yêu cầu</span>
                      <span :class="['status-pill-modern', statusTone(record.status)]">
                        <span class="status-dot"></span>
                        {{ statusLabel(record.status) }}
                      </span>
                    </div>
                  </div>
                </template>

                <div class="teacher-node teacher-node--sub">
                  <div
                    class="teacher-avatar teacher-avatar--sub"
                    :style="{ background: avatarColor(record.substituteTeacherNameSnapshot) }"
                  >
                    {{ initials(record.substituteTeacherNameSnapshot) }}
                  </div>
                  <div class="teacher-info">
                    <strong>{{ record.substituteTeacherNameSnapshot || 'Chưa rõ' }}</strong>
                    <span class="role-tag role-tag--sub">Dạy thay</span>
                  </div>
                </div>
              </a-popover>
            </div>
          </template>

          <!-- Lý do -->
          <template v-else-if="column.key === 'reason'">
            <div class="reason-bubble">
              <span class="reason-quote">"</span>
              {{ record.reason || 'Không có lý do chi tiết' }}
              <span class="reason-quote">"</span>
            </div>
          </template>

          <!-- Trạng thái -->
          <template v-else-if="column.key === 'status'">
            <span :class="['status-pill-modern', statusTone(record.status)]">
              <span class="status-dot"></span>
              {{ statusLabel(record.status) }}
            </span>
          </template>

          <!-- Thao tác -->
          <template v-else-if="column.key === 'actions'">
            <div v-if="isPending(record.status)" class="action-cell">
              <a-popconfirm
                title="Duyệt yêu cầu dạy thay này?"
                ok-text="Duyệt"
                cancel-text="Hủy"
                @confirm="review(record, true)"
              >
                <button class="action-btn action-btn--approve">
                  <CheckOutlined />
                  Duyệt
                </button>
              </a-popconfirm>

              <a-popconfirm
                title="Từ chối yêu cầu dạy thay này?"
                ok-text="Từ chối"
                ok-type="danger"
                cancel-text="Hủy"
                @confirm="review(record, false)"
              >
                <button class="action-btn action-btn--reject">
                  <CloseOutlined />
                  Từ chối
                </button>
              </a-popconfirm>
            </div>

            <span v-else class="processed-label">
              <CheckCircleOutlined />
              Đã xử lý
            </span>
          </template>
        </template>
      </a-table>
    </div>
  </section>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import {
  CheckOutlined,
  CloseOutlined,
  ClockCircleOutlined,
  SyncOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ArrowRightOutlined,
  CalendarOutlined,
  HomeOutlined,
  UserSwitchOutlined,
} from '@ant-design/icons-vue'
import { teachingSubstitutionApi } from '@/api/teachingSubstitutionApi'
import { DAY_OF_WEEK_VN } from '@/lib/constants'

const loading = ref(false)
const items = ref([])

const columns = [
  { title: 'Buổi học', key: 'schedule', width: 280 },
  { title: 'Giảng viên chuyển giao', key: 'teachers', width: 500 },
  { title: 'Lý do xin nghỉ', key: 'reason', width: 360 },
  { title: 'Trạng thái', key: 'status', width: 150, align: 'center' },
  { title: 'Thao tác', key: 'actions', width: 210, align: 'center', fixed: 'right' },
]

const pagination = computed(() => ({
  pageSize: 10,
  showSizeChanger: false,
  hideOnSinglePage: false,
}))

const DAY_NAMES = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']
const todayIdx  = new Date().getDay()
const todayStr  = DAY_NAMES[todayIdx]

function isToday(record) { return record.dayOfWeek === todayStr }

function dayOrder(dayOfWeek) {
  const idx = DAY_NAMES.indexOf(dayOfWeek)
  return idx === -1 ? 7 : (idx - todayIdx + 7) % 7
}

const sortedItems = computed(() => {
  const pending = items.value.filter(i => isPending(i.status))
  const rest    = items.value.filter(i => !isPending(i.status))
  const sortByDay = (a, b) => dayOrder(a.dayOfWeek) - dayOrder(b.dayOfWeek)
  return [...pending.slice().sort(sortByDay), ...rest.slice().sort(sortByDay)]
})

function rowClassName(record) {
  if (isPending(record.status) && isToday(record)) return 'row-today-pending'
  if (isPending(record.status)) return 'row-pending'
  return ''
}

async function loadItems() {
  loading.value = true
  try { items.value = await teachingSubstitutionApi.getAll() }
  finally { loading.value = false }
}

async function review(record, approved) {
  try {
    if (approved) {
      await teachingSubstitutionApi.approve(record.id, { adminNote: 'Admin đã duyệt yêu cầu dạy thay.' })
      message.success('Đã duyệt yêu cầu dạy thay')
    } else {
      await teachingSubstitutionApi.reject(record.id, { adminNote: 'Admin đã từ chối yêu cầu dạy thay.' })
      message.success('Đã từ chối yêu cầu dạy thay')
    }
    await loadItems()
  } catch (error) {
    message.error(error.message || 'Không thể xử lý yêu cầu')
  }
}

function formatTime(value) { return value ? String(value).slice(0, 5) : '--:--' }

function dayLabel(value) {
  const mapped = { Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3, Thursday: 4, Friday: 5, Saturday: 6 }[value]
  return DAY_OF_WEEK_VN[value] || DAY_OF_WEEK_VN[mapped] || value || 'Chưa rõ'
}

function isPending(status) { return status === 0 || status === 'Pending' }

function statusLabel(status) {
  return { 0: 'Chờ duyệt', 1: 'Đã duyệt', 2: 'Từ chối', 3: 'Đã hủy', Pending: 'Chờ duyệt', Approved: 'Đã duyệt', Rejected: 'Từ chối', Cancelled: 'Đã hủy' }[status] || status
}

function statusTone(status) {
  if (status === 1 || status === 'Approved')   return 'is-approved'
  if (status === 2 || status === 'Rejected')   return 'is-rejected'
  if (status === 3 || status === 'Cancelled')  return 'is-cancelled'
  return 'is-pending'
}

function initials(name) {
  const parts = String(name || '?').trim().split(/\s+/)
  if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

const AVATAR_COLORS = ['#4f46e5','#7c3aed','#db2777','#0891b2','#059669','#d97706','#dc2626','#65a30d']

function avatarColor(name) {
  if (!name) return AVATAR_COLORS[0]
  let hash = 0
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length]
}

onMounted(loadItems)
</script>

<style scoped>
/* ═══════════════════════════════════════
   PAGE LAYOUT
═══════════════════════════════════════ */
.sub-page {
  padding: 4px 0 32px;
  min-height: calc(100vh - 64px);
}

/* ═══════════════════════════════════════
   HEADER
═══════════════════════════════════════ */
.sub-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.sub-header-left {
  display: flex;
  align-items: center;
  gap: 14px;
}

.sub-header-icon {
  width: 44px;
  height: 44px;
  border-radius: 14px;
  background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  flex-shrink: 0;
  box-shadow: 0 4px 12px rgba(79,70,229,0.35);
}

.sub-title {
  margin: 0;
  font-size: 22px;
  font-weight: 800;
  color: var(--text-primary);
  line-height: 1.25;
  letter-spacing: -0.3px;
}

.sub-subtitle {
  margin: 2px 0 0;
  font-size: 12px;
  color: var(--text-secondary);
}

.sub-refresh-btn {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  height: 36px;
  padding: 0 16px;
  border-radius: 10px;
  border: 1.5px solid var(--border-color);
  background: var(--bg-card);
  color: #4f46e5;
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.18s ease;
}
.sub-refresh-btn:hover {
  background: rgba(79,70,229,0.07);
  border-color: #4f46e5;
  box-shadow: 0 2px 8px rgba(79,70,229,0.12);
}
.sub-refresh-btn.loading {
  opacity: 0.65;
  pointer-events: none;
}

/* ═══════════════════════════════════════
   STATS STRIP
═══════════════════════════════════════ */
.stats-strip {
  display: flex;
  align-items: center;
  gap: 0;
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: 16px;
  padding: 14px 24px;
  margin-bottom: 20px;
  box-shadow: 0 2px 12px rgba(15,23,42,0.04);
  flex-wrap: wrap;
  row-gap: 12px;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
  min-width: 120px;
}

.stat-divider {
  width: 1px;
  height: 36px;
  background: var(--border-color);
  margin: 0 8px;
  flex-shrink: 0;
}

.stat-icon {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  flex-shrink: 0;
}

.stat-icon--total    { background: rgba(79,70,229,0.1);  color: #4f46e5; }
.stat-icon--pending  { background: rgba(217,119,6,0.1);  color: #d97706; }
.stat-icon--approved { background: rgba(5,150,105,0.1);  color: #059669; }
.stat-icon--rejected { background: rgba(220,38,38,0.1);  color: #dc2626; }

.stat-body {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.stat-value {
  font-size: 22px;
  font-weight: 900;
  color: var(--text-primary);
  line-height: 1;
}
.stat-value--pending  { color: #d97706; }
.stat-value--approved { color: #059669; }
.stat-value--rejected { color: #dc2626; }

.stat-label {
  font-size: 11px;
  font-weight: 600;
  color: var(--text-secondary);
  white-space: nowrap;
}

/* ═══════════════════════════════════════
   TABLE CARD
═══════════════════════════════════════ */
.sub-table-card {
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: 18px;
  overflow: hidden;
  box-shadow: 0 4px 24px rgba(15,23,42,0.05);
}

/* Ant Design overrides */
.sub-table :deep(.ant-table) {
  background: var(--bg-card) !important;
  color: var(--text-primary) !important;
}
.sub-table :deep(.ant-table-thead > tr > th) {
  background: var(--bg-card) !important;
  border-bottom: 1.5px solid var(--border-color) !important;
  color: var(--text-secondary) !important;
  font-size: 12px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}
.sub-table :deep(.ant-table-tbody > tr > td) {
  border-bottom: 1px solid var(--border-color) !important;
  color: var(--text-primary) !important;
  vertical-align: top;
  transition: background 0.15s;
}
.sub-table :deep(.ant-table-cell-fix-left),
.sub-table :deep(.ant-table-cell-fix-right) {
  background: var(--bg-card) !important;
}
.sub-table :deep(.ant-table-tbody > tr:hover > td),
.sub-table :deep(.ant-table-tbody > tr:hover > .ant-table-cell-fix-right) {
  background: var(--bg-secondary) !important;
}
.sub-table :deep(.row-today-pending > td) {
  background: rgba(251,191,36,0.06) !important;
  border-left: 3px solid #f59e0b !important;
}
.sub-table :deep(.row-pending > td) {
  background: rgba(251,191,36,0.025) !important;
}
.sub-table :deep(.ant-pagination) { margin: 16px !important; }
.sub-table :deep(.ant-pagination-item),
.sub-table :deep(.ant-pagination-prev .ant-pagination-item-link),
.sub-table :deep(.ant-pagination-next .ant-pagination-item-link) {
  border-radius: 8px !important;
  border-color: var(--border-color) !important;
  background: var(--bg-card) !important;
  color: var(--text-primary) !important;
}
.sub-table :deep(.ant-pagination-item-active) {
  border-color: #4f46e5 !important;
  background: #4f46e5 !important;
}
.sub-table :deep(.ant-pagination-item-active a) { color: #fff !important; }

/* ═══════════════════════════════════════
   SCHEDULE CELL
═══════════════════════════════════════ */
.schedule-cell { display: flex; flex-direction: column; gap: 6px; padding: 6px 0; }
.schedule-top  { display: flex; align-items: center; gap: 6px; flex-wrap: wrap; }

.class-badge {
  display: inline-flex;
  align-items: center;
  padding: 2px 8px;
  border-radius: 6px;
  font-size: 11px;
  font-weight: 800;
  background: rgba(79,70,229,0.08);
  color: #4f46e5;
}

.today-badge {
  display: inline-flex;
  align-items: center;
  padding: 2px 7px;
  border-radius: 6px;
  font-size: 10px;
  font-weight: 900;
  letter-spacing: 0.06em;
  background: rgba(220,38,38,0.1);
  color: #dc2626;
}

.schedule-meta {
  display: flex;
  flex-direction: column;
  gap: 3px;
  font-size: 11.5px;
  color: var(--text-secondary);
}
.schedule-meta span {
  display: flex;
  align-items: center;
  gap: 5px;
}

/* ═══════════════════════════════════════
   TEACHER FLOW CELL
═══════════════════════════════════════ */
.teacher-flow {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 6px 0;
  flex-wrap: nowrap;
  white-space: nowrap;
  min-width: 460px;
}

.flow-arrow {
  color: var(--text-muted);
  font-size: 13px;
  flex-shrink: 0;
}

.teacher-node {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  padding: 5px 7px 5px 5px;
  border-radius: 10px;
  cursor: default;
  transition: background 0.16s, box-shadow 0.16s;
  flex: 0 0 auto;
  min-width: 0;
}
.teacher-node:hover {
  background: rgba(79,70,229,0.06);
  box-shadow: inset 0 0 0 1.5px rgba(79,70,229,0.15);
}

.teacher-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 900;
  color: #fff;
  flex-shrink: 0;
}
.teacher-avatar--sub {
  box-shadow: 0 0 0 2px rgba(79,70,229,0.25);
}

.teacher-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}
.teacher-info strong {
  font-size: 12px;
  font-weight: 700;
  color: var(--text-primary);
  white-space: nowrap;
  max-width: 150px;
  overflow: hidden;
  text-overflow: ellipsis;
}

.role-tag {
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}
.role-tag--leave { color: #dc2626; }
.role-tag--sub   { color: #059669; }

/* ═══════════════════════════════════════
   REASON BUBBLE
═══════════════════════════════════════ */
.reason-bubble {
  font-size: 12px;
  color: var(--text-secondary);
  line-height: 1.55;
  background: var(--bg-secondary);
  border: 1px solid var(--border-color);
  border-radius: 10px;
  padding: 8px 12px;
  font-style: italic;
  margin: 6px 6px 6px 0;
}
.reason-quote {
  font-style: normal;
  font-weight: 900;
  font-size: 14px;
  color: var(--text-muted);
  line-height: 1;
}

/* ═══════════════════════════════════════
   STATUS PILL
═══════════════════════════════════════ */
.status-pill-modern {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 700;
  border: 1px solid transparent;
  white-space: nowrap;
}
.status-dot {
  width: 6px; height: 6px;
  border-radius: 999px;
  flex-shrink: 0;
}

.status-pill-modern.is-pending  { background: #fffbeb; border-color: #fde68a; color: #d97706; }
.status-pill-modern.is-pending .status-dot { background: #d97706; }
.status-pill-modern.is-approved { background: #ecfdf5; border-color: #a7f3d0; color: #059669; }
.status-pill-modern.is-approved .status-dot { background: #059669; }
.status-pill-modern.is-rejected { background: #fff1f2; border-color: #fecdd3; color: #e11d48; }
.status-pill-modern.is-rejected .status-dot { background: #e11d48; }
.status-pill-modern.is-cancelled { background: #f8fafc; border-color: #e2e8f0; color: #64748b; }
.status-pill-modern.is-cancelled .status-dot { background: #64748b; }

.dark .status-pill-modern.is-pending  { background: rgba(217,119,6,0.12);  border-color: rgba(217,119,6,0.3);  color: #fbbf24; }
.dark .status-pill-modern.is-approved { background: rgba(5,150,105,0.12);  border-color: rgba(5,150,105,0.3);  color: #34d399; }
.dark .status-pill-modern.is-rejected { background: rgba(225,29,72,0.12);  border-color: rgba(225,29,72,0.3);  color: #f87171; }
.dark .status-pill-modern.is-cancelled{ background: rgba(100,116,139,0.12);border-color: rgba(100,116,139,0.3);color: #94a3b8; }

/* ═══════════════════════════════════════
   ACTION BUTTONS
═══════════════════════════════════════ */
.action-cell {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.action-btn {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  height: 32px;
  padding: 0 12px;
  border-radius: 8px;
  font-size: 12px;
  font-weight: 700;
  border: none;
  cursor: pointer;
  transition: all 0.18s ease;
}
.action-btn--approve {
  background: #10b981;
  color: #fff;
  box-shadow: 0 2px 6px rgba(16,185,129,0.3);
}
.action-btn--approve:hover {
  background: #059669;
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(16,185,129,0.35);
}
.action-btn--reject {
  background: #f43f5e;
  color: #fff;
  box-shadow: 0 2px 6px rgba(244,63,94,0.3);
}
.action-btn--reject:hover {
  background: #e11d48;
  transform: translateY(-1px);
  box-shadow: 0 4px 10px rgba(244,63,94,0.35);
}

.processed-label {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 11.5px;
  font-weight: 600;
  color: var(--text-muted);
}

/* ═══════════════════════════════════════
   TEACHER POPOVER
═══════════════════════════════════════ */
:global(.teacher-detail-popover .ant-popover-inner) {
  padding: 0;
  border-radius: 14px;
  overflow: hidden;
  box-shadow: 0 18px 45px rgba(15,23,42,0.18);
}

.teacher-popover-card {
  width: 320px;
  background: #ffffff;
  color: #0f172a;
}

.teacher-popover-header {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  border-bottom: 1px solid #e2e8f0;
  background: linear-gradient(180deg, #f8fbff 0%, #ffffff 100%);
}

.teacher-popover-avatar {
  width: 44px;
  height: 44px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-size: 14px;
  font-weight: 900;
  flex-shrink: 0;
}

.teacher-popover-header h3 {
  margin: 0;
  font-size: 15px;
  font-weight: 800;
  color: #0f172a;
  line-height: 1.25;
}

.popover-role {
  margin: 3px 0 0;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}
.popover-role--leave { color: #dc2626; }
.popover-role--sub   { color: #059669; }

.teacher-popover-info {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  padding: 14px 16px 10px;
}

.teacher-popover-info div {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.teacher-popover-info span {
  font-size: 11px;
  font-weight: 700;
  color: #64748b;
}

.teacher-popover-info strong {
  font-size: 12px;
  font-weight: 800;
  color: #0f172a;
  line-height: 1.35;
}

.teacher-popover-reason {
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 10px 16px 14px;
  border-top: 1px solid #e2e8f0;
}

.teacher-popover-reason span {
  font-size: 11px;
  font-weight: 700;
  color: #64748b;
}

.teacher-popover-reason strong {
  font-size: 12px;
  font-weight: 600;
  color: #0f172a;
  font-style: italic;
  line-height: 1.45;
}

.teacher-popover-status {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 16px 14px;
  border-top: 1px solid #e2e8f0;
  background: #f8fafc;
}

.teacher-popover-status span:first-child {
  font-size: 11px;
  font-weight: 700;
  color: #64748b;
}
</style>
