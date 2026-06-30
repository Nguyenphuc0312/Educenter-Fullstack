<template>
  <AdminResourceView
    title="Quản lý phòng học"
    subtitle="Danh mục phòng học dùng để chọn khi mở lớp mới."
    :api="roomApi"
    :columns="columns"
    :fields="fields"
    :searchable-fields="['code', 'name', 'note']"
    :form-groups="formGroups"
    :filter-fn="customFilter"
    :can-delete="canDeleteRoom"
    :has-row-actions="() => true"
    @reset="resetCustomFilters"
  >
    <template #filters>
      <a-select
        v-model:value="filterActive"
        placeholder="Trạng thái"
        allow-clear
        size="small"
        class="w-36"
      >
        <a-select-option :value="true">Đang dùng</a-select-option>
        <a-select-option :value="false">Tạm dừng</a-select-option>
      </a-select>
    </template>

    <template #bodyCell="{ column, record }">
      <template v-if="column.key === 'roomInfo'">
        <button type="button" class="room-info-button" @click="openRoomDetail(record)">
          <div class="flex items-center gap-2 min-w-0">
            <span class="text-[13px] font-semibold truncate" style="color: var(--admin-text);">
              {{ record.name || 'Chưa đặt tên' }}
            </span>
            <span class="text-[10px] font-mono px-1.5 py-0.5 rounded" style="background: var(--admin-surface-2); color: var(--admin-text-muted);">
              {{ record.code || '---' }}
            </span>
          </div>
          <span class="text-[11px] truncate mt-0.5 text-left" style="color: var(--admin-text-muted);">
            {{ record.note || 'Chưa có ghi chú' }}
          </span>
        </button>
      </template>

      <template v-else-if="column.key === 'capacity'">
        <span class="text-xs font-semibold" style="color: var(--admin-text);">
          {{ record.capacity || 0 }} chỗ
        </span>
      </template>

      <template v-else-if="column.key === 'classCount'">
        <button type="button" class="room-link-button" @click="openRoomDetail(record)">
          {{ record.classCount || 0 }} lớp
        </button>
      </template>

      <template v-else-if="column.key === 'isActive'">
        <span
          class="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[11px] font-semibold"
          :style="record.isActive ? activeStyle : inactiveStyle"
        >
          <span class="w-1.5 h-1.5 rounded-full" :style="{ background: record.isActive ? '#10b981' : '#94a3b8' }"></span>
          {{ record.isActive ? 'Đang dùng' : 'Tạm dừng' }}
        </span>
      </template>
    </template>

    <template #rowActions="{ record }">
      <button type="button" class="action-menu-item" @click="openRoomDetail(record)">
        Xem lớp đang dùng
      </button>
    </template>
  </AdminResourceView>

  <a-drawer
    v-model:open="detailOpen"
    width="620"
    placement="right"
    :destroy-on-close="true"
    class="room-detail-drawer"
  >
    <template #title>
      <span class="sr-only">Chi tiết phòng học</span>
    </template>

    <div v-if="detailLoading" class="room-detail-loading">
      <a-spin />
    </div>

    <div v-else class="room-drawer-content">
      <section class="room-hero">
        <div class="room-hero-main">
          <span class="room-code">{{ selectedRoom?.code || '---' }}</span>
          <h2>{{ selectedRoom?.name || 'Chi tiết phòng học' }}</h2>
          <p>{{ selectedRoom?.note || 'Chưa có ghi chú' }}</p>
        </div>
        <span :class="['room-status-pill', selectedRoom?.isActive ? 'is-active' : 'is-paused']">
          {{ selectedRoom?.isActive ? 'Đang sử dụng' : 'Tạm dừng' }}
        </span>
      </section>

      <section class="room-metric-grid">
        <div class="room-metric-card">
          <span>Sức chứa</span>
          <strong>{{ selectedRoom?.capacity || 0 }} chỗ</strong>
        </div>
        <div class="room-metric-card">
          <span>Lớp đang dùng</span>
          <strong>{{ usageClasses.length }} lớp</strong>
        </div>
        <div class="room-metric-card">
          <span>Đã đăng ký</span>
          <strong>{{ totalStudents }}/{{ totalClassCapacity }}</strong>
        </div>
        <div class="room-metric-card">
          <span>Buổi học</span>
          <strong>{{ totalSchedules }} buổi</strong>
        </div>
      </section>

      <section class="room-section">
        <div class="room-section-header">
          <div>
            <h3>Lớp đang sử dụng</h3>
            <p>Danh sách lớp và khung giờ đã đăng ký phòng này.</p>
          </div>
          <span>{{ usageClasses.length }}</span>
        </div>

        <div v-if="usageClasses.length === 0" class="room-empty-state">
          Phòng này chưa được gắn với lớp học nào.
        </div>

        <div v-else class="room-class-list">
          <article v-for="item in usageClasses" :key="item.classId" class="room-class-card">
            <div class="room-class-top">
              <div class="min-w-0">
                <h4>{{ item.className }}</h4>
                <p>{{ item.classCode }} · {{ item.teacherName || 'Chưa có giảng viên' }}</p>
              </div>
              <span class="room-seat-badge">
                {{ item.currentStudents || 0 }}/{{ item.maxStudents || 0 }}
              </span>
            </div>

            <div class="room-class-meta">
              <div>
                <span>Khai giảng</span>
                <strong>{{ formatDate(item.startDate) }}</strong>
              </div>
              <div>
                <span>Kết thúc</span>
                <strong>{{ formatDate(item.endDate) }}</strong>
              </div>
              <div>
                <span>Giảng viên</span>
                <strong>{{ item.teacherName || 'Chưa phân công' }}</strong>
              </div>
              <div>
                <span>Trạng thái</span>
                <strong>{{ formatClassStatus(item.status) }}</strong>
              </div>
            </div>

            <div v-if="item.schedules?.length" class="room-schedule-list">
              <div v-for="schedule in item.schedules" :key="schedule.id" class="room-schedule-row">
                <div class="room-schedule-day">
                  <strong>{{ formatDay(schedule.dayOfWeek) }}</strong>
                  <span>{{ formatShift(schedule.studyShift) }}</span>
                </div>
                <div class="room-schedule-topic">
                  <strong>{{ formatTime(schedule.startTime) }} - {{ formatTime(schedule.endTime) }}</strong>
                  <span>{{ schedule.topic || 'Buổi học' }}</span>
                </div>
              </div>
            </div>

            <div v-else class="room-no-schedule">
              Lớp này chưa có lịch học chi tiết.
            </div>
          </article>
        </div>
      </section>
    </div>
  </a-drawer>
</template>

<script setup>
import { computed, ref } from 'vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import { roomApi } from '@/api/roomApi'

const filterActive = ref(undefined)
const detailOpen = ref(false)
const detailLoading = ref(false)
const selectedRoom = ref(null)
const usageClasses = ref([])

const activeStyle = {
  background: 'rgba(16, 185, 129, 0.1)',
  color: '#059669',
  border: '1px solid rgba(16, 185, 129, 0.25)'
}

const inactiveStyle = {
  background: 'rgba(148, 163, 184, 0.12)',
  color: '#64748b',
  border: '1px solid rgba(148, 163, 184, 0.3)'
}

const columns = [
  { title: 'Phòng học', dataIndex: 'name', key: 'roomInfo', minWidth: 260 },
  { title: 'Sức chứa', dataIndex: 'capacity', key: 'capacity', width: 120 },
  { title: 'Lớp đang dùng', dataIndex: 'classCount', key: 'classCount', width: 130 },
  { title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', width: 130 },
]

const fields = [
  { name: 'name', label: 'Tên phòng', required: true, default: '' },
  { name: 'capacity', label: 'Sức chứa', type: 'number', required: true, default: 30, min: 1 },
  { name: 'isActive', label: 'Đang sử dụng', type: 'switch', default: true },
  { name: 'note', label: 'Ghi chú', type: 'textarea', fullWidth: true, default: '' },
]

const formGroups = [
  {
    title: 'Thông tin phòng học',
    fields: ['name', 'capacity', 'isActive']
  },
  {
    title: 'Ghi chú',
    fields: ['note']
  }
]

const dayLabels = {
  0: 'Chủ nhật',
  1: 'Thứ 2',
  2: 'Thứ 3',
  3: 'Thứ 4',
  4: 'Thứ 5',
  5: 'Thứ 6',
  6: 'Thứ 7',
  Sunday: 'Chủ nhật',
  Monday: 'Thứ 2',
  Tuesday: 'Thứ 3',
  Wednesday: 'Thứ 4',
  Thursday: 'Thứ 5',
  Friday: 'Thứ 6',
  Saturday: 'Thứ 7'
}

const shiftLabels = {
  0: 'Sáng',
  1: 'Chiều',
  2: 'Tối',
  Morning: 'Sáng',
  Afternoon: 'Chiều',
  Evening: 'Tối'
}

const classStatusLabels = {
  Open: 'Sắp mở',
  InProgress: 'Đang học',
  Completed: 'Hoàn thành',
  Cancelled: 'Đã hủy',
  Full: 'Đã đủ sĩ số'
}

const totalStudents = computed(() =>
  usageClasses.value.reduce((sum, item) => sum + Number(item.currentStudents || 0), 0)
)

const totalClassCapacity = computed(() =>
  usageClasses.value.reduce((sum, item) => sum + Number(item.maxStudents || 0), 0)
)

const totalSchedules = computed(() =>
  usageClasses.value.reduce((sum, item) => sum + Number(item.schedules?.length || 0), 0)
)

function customFilter(item) {
  return filterActive.value === undefined || item.isActive === filterActive.value
}

function resetCustomFilters() {
  filterActive.value = undefined
}

function canDeleteRoom(record) {
  return Number(record?.classCount || 0) === 0
}

async function openRoomDetail(record) {
  selectedRoom.value = record
  usageClasses.value = []
  detailOpen.value = true
  detailLoading.value = true
  try {
    const detail = await roomApi.getUsage(record.id)
    selectedRoom.value = detail?.room || record
    usageClasses.value = detail?.classes || []
  } finally {
    detailLoading.value = false
  }
}

function formatDay(value) {
  return dayLabels[value] || String(value || 'Chưa chọn')
}

function formatShift(value) {
  return shiftLabels[value] || 'Ca học'
}

function formatClassStatus(value) {
  return classStatusLabels[value] || value || 'Chưa rõ'
}

function formatTime(value) {
  if (!value) return '--:--'
  return String(value).slice(0, 5)
}

function formatDate(value) {
  if (!value) return '--/--/----'
  return new Intl.DateTimeFormat('vi-VN').format(new Date(value))
}
</script>

<style>
.room-info-button {
  display: flex;
  width: 100%;
  min-width: 0;
  flex-direction: column;
  border: 0;
  background: transparent;
  padding: 0;
  cursor: pointer;
}

.room-info-button:hover span:first-child {
  color: var(--admin-accent) !important;
}

.room-link-button {
  border: 0;
  background: transparent;
  padding: 0;
  color: var(--admin-accent);
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
}

.action-menu-item {
  width: 100%;
  border: 0;
  background: transparent;
  padding: 4px 0;
  text-align: left;
  font-size: 12px;
  color: var(--admin-text);
  cursor: pointer;
}

.room-detail-drawer .ant-drawer-header {
  min-height: 56px;
  border-bottom: 1px solid #eef2f7;
  padding: 0 26px;
}

.room-detail-drawer .ant-drawer-body {
  padding: 0;
  background: #ffffff;
}

.room-detail-loading {
  display: flex;
  min-height: 280px;
  align-items: center;
  justify-content: center;
}

.room-drawer-content {
  padding: 26px 30px 32px;
}

.room-hero {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 18px;
}

.room-hero-main {
  min-width: 0;
}

.room-code {
  display: inline-flex;
  width: fit-content;
  border-radius: 999px;
  background: #eef2ff;
  color: #2563eb;
  padding: 6px 12px;
  font-size: 15px;
  font-weight: 800;
  letter-spacing: 0;
}

.room-hero h2 {
  margin: 12px 0 6px;
  color: #0f172a;
  font-size: 27px;
  font-weight: 800;
  line-height: 1.25;
}

.room-hero p {
  margin: 0;
  color: #64748b;
  font-size: 12px;
  line-height: 1.5;
}

.room-status-pill,
.room-seat-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  border-radius: 999px;
  padding: 7px 14px;
  font-size: 13px;
  font-weight: 800;
  white-space: nowrap;
}

.room-status-pill.is-active::before {
  content: "";
  width: 7px;
  height: 7px;
  margin-right: 8px;
  border-radius: 999px;
  background: #10b981;
}

.room-status-pill.is-active {
  background: #ecfdf5;
  color: #059669;
  border: 1px solid #bbf7d0;
}

.room-status-pill.is-paused {
  background: #f1f5f9;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.room-metric-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 14px;
  margin-top: 22px;
}

.room-metric-card,
.room-class-meta > div,
.room-schedule-row {
  border-radius: 8px;
  background: #f8fafc;
}

.room-metric-card {
  border: 1px solid #eef2f7;
  min-height: 126px;
  padding: 18px;
  box-shadow: 0 10px 26px rgba(15, 23, 42, 0.04);
}

.room-metric-card::before,
.room-class-card::before {
  content: "";
  display: flex;
  align-items: center;
  justify-content: center;
  width: 34px;
  height: 34px;
  margin-bottom: 15px;
  border-radius: 10px;
  background: #eff6ff;
  color: #2563eb;
  font-size: 17px;
  font-weight: 900;
}

.room-metric-card:nth-child(1)::before { content: "2"; background: #eff6ff; color: #2563eb; }
.room-metric-card:nth-child(2)::before { content: "□"; background: #f5f3ff; color: #7c3aed; }
.room-metric-card:nth-child(3)::before { content: "✓"; background: #ecfdf5; color: #16a34a; }
.room-metric-card:nth-child(4)::before { content: "▣"; background: #fff7ed; color: #f97316; }

.room-metric-card span,
.room-class-meta span,
.room-section-header p,
.room-class-top p,
.room-schedule-row span,
.room-no-schedule,
.room-empty-state {
  color: #64748b;
  font-size: 11px;
}

.room-metric-card strong {
  display: block;
  margin-top: 5px;
  color: #0f172a;
  font-size: 20px;
  font-weight: 800;
}

.room-section {
  margin-top: 28px;
}

.room-section-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;
}

.room-section-header h3 {
  margin: 0;
  color: #0f172a;
  font-size: 17px;
  font-weight: 800;
}

.room-section-header p {
  margin: 4px 0 0;
}

.room-section-header > span {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 36px;
  height: 32px;
  border-radius: 999px;
  background: #eff6ff;
  color: #2563eb;
  font-size: 15px;
  font-weight: 800;
}

.room-empty-state {
  border: 1px dashed #cbd5e1;
  border-radius: 8px;
  background: #f8fafc;
  padding: 18px;
}

.room-class-list {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.room-class-card {
  position: relative;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  background: #ffffff;
  box-shadow: 0 14px 30px rgba(15, 23, 42, 0.07);
  padding: 20px 20px 20px 86px;
  overflow: hidden;
}

.room-class-card::after {
  content: "";
  position: absolute;
  inset: 0 auto 0 0;
  width: 4px;
  background: #2563eb;
}

.room-class-card:nth-child(even)::after {
  background: #7c3aed;
}

.room-class-card::before {
  position: absolute;
  top: 22px;
  left: 22px;
  content: "□";
  width: 46px;
  height: 46px;
  margin: 0;
  border-radius: 10px;
  background: #eff6ff;
  color: #2563eb;
}

.room-class-card:nth-child(even)::before {
  background: #f5f3ff;
  color: #7c3aed;
}

.room-class-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.room-class-top h4 {
  margin: 0;
  color: #0f172a;
  font-size: 17px;
  font-weight: 800;
  line-height: 1.35;
}

.room-class-top p {
  margin: 4px 0 0;
}

.room-seat-badge {
  background: #eef2ff;
  color: #2563eb;
  border: 1px solid #93c5fd;
  padding: 5px 13px;
  font-size: 14px;
}

.room-class-meta {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px;
  margin-top: 20px;
}

.room-class-meta > div {
  padding: 13px 15px;
  border: 1px solid #e5edf7;
}

.room-class-meta strong {
  display: block;
  margin-top: 4px;
  color: #0f172a;
  font-size: 12px;
  font-weight: 800;
  line-height: 1.35;
}

.room-schedule-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-top: 13px;
}

.room-schedule-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  border: 0;
  padding: 12px 15px;
  background: #eff6ff;
  color: #2563eb;
}

.room-class-card:nth-child(even) .room-schedule-row {
  background: #f5f3ff;
  color: #7c3aed;
}

.room-schedule-row strong {
  display: inline;
  color: inherit;
  font-size: 13px;
  font-weight: 800;
  line-height: 1.35;
}

.room-schedule-day {
  min-width: 0;
}

.room-schedule-day span {
  margin-left: 4px;
  color: inherit;
  font-weight: 800;
}

.room-schedule-topic {
  min-width: 0;
  text-align: right;
}

.room-schedule-topic span {
  display: block;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: inherit;
  font-weight: 600;
}

.room-no-schedule {
  margin-top: 13px;
  border: 1px dashed #cbd5e1;
  border-radius: 8px;
  background: #f8fafc;
  padding: 12px;
}

.dark .room-detail-drawer .ant-drawer-body {
  background: #0f172a;
}

.dark .room-hero h2,
.dark .room-metric-card strong,
.dark .room-section-header h3,
.dark .room-class-top h4,
.dark .room-class-meta strong,
.dark .room-schedule-row strong {
  color: #e5e7eb;
}

.dark .room-metric-card,
.dark .room-class-meta > div,
.dark .room-schedule-row,
.dark .room-empty-state,
.dark .room-no-schedule {
  background: #111827;
  border-color: #1f2937;
}

.dark .room-class-card {
  background: #0b1120;
  border-color: #1f2937;
}

@media (max-width: 640px) {
  .room-detail-drawer .ant-drawer-content-wrapper {
    width: 100% !important;
  }

  .room-drawer-content {
    padding: 20px 18px 24px;
  }

  .room-class-meta,
  .room-metric-grid {
    grid-template-columns: 1fr;
  }

  .room-schedule-row {
    grid-template-columns: 1fr;
  }

  .room-schedule-topic {
    text-align: left;
  }
}
</style>
