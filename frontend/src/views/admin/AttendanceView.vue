<template>
  <div class="space-y-6">
    <PageHeader title="Điểm danh" subtitle="Tạo buổi điểm danh, khóa phiên và cập nhật điểm danh hàng loạt." />

    <div class="attendance-control-card bg-card-base border border-base rounded-xl p-4 shadow-sm">
      <div class="flex flex-col xl:flex-row xl:items-end gap-4">
        <div class="flex-1 space-y-2 min-w-0">
          <label class="text-xs font-semibold text-base-secondary">Chọn lớp học</label>
          <a-select
            v-model:value="selectedClassId"
            placeholder="Chọn lớp để xem phiên điểm danh..."
            class="attendance-class-select w-full"
            :loading="loadingClasses"
            show-search
            option-filter-prop="children"
            @change="handleClassChange"
          >
            <a-select-option v-for="cls in classes" :key="cls.id" :value="cls.id">
              {{ cls.className }} - {{ cls.courseNameSnapshot }}
            </a-select-option>
          </a-select>
        </div>

        <a-button
          type="primary"
          class="attendance-create-btn"
          :loading="creatingSession"
          :disabled="!selectedClassId"
          @click="handleCreateSession"
        >
          <template #icon><PlusOutlined /></template>
          Tạo phiên điểm danh mới
        </a-button>
      </div>
    </div>

    <div v-if="selectedClassId">
      <div class="flex items-center justify-between mb-3">
        <h2 class="text-sm font-bold text-base-primary flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-blue-600 rounded-full"></span>
          Danh sách phiên điểm danh
        </h2>
        <a-button class="attendance-refresh-btn" :loading="loadingSessions" @click="loadSessions(selectedClassId)">
          <template #icon><SyncOutlined /></template>
          Làm mới
        </a-button>
      </div>

      <div v-if="loadingSessions" class="py-8 flex justify-center">
        <a-spin size="large" />
      </div>

      <div v-else-if="sessions.length === 0" class="bg-card-base border border-base rounded-xl p-4 shadow-sm">
        <EmptyTableState
          :show-action-button="true"
          title="Chưa có phiên điểm danh"
          description="Lớp học này chưa có phiên điểm danh nào. Nhấn Tạo phiên điểm danh mới để bắt đầu."
          action-button-text="Tạo phiên điểm danh"
          @action="handleCreateSession"
        />
      </div>

      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
        <div
          v-for="session in sessions"
          :key="session.id"
          class="bg-card-base border border-base rounded-xl p-4 shadow-sm cursor-pointer hover:border-primary-300 hover:-translate-y-0.5 transition-all duration-200"
          :class="{ 'border-blue-400 dark:border-blue-600 ring-1 ring-blue-300 dark:ring-blue-800': selectedSessionId === session.id }"
          @click="selectSession(session)"
        >
          <div class="flex items-start justify-between gap-2">
            <div class="flex-1 min-w-0">
              <p class="text-xs font-bold text-base-primary truncate">
                Buổi {{ session.sessionNumber }} - {{ formatDate(session.attendanceDate) }}
              </p>
              <p class="text-[11px] text-base-secondary truncate mt-0.5">
                {{ session.topic || 'Chưa có chủ đề' }}
              </p>
            </div>
            <StatusBadge :value="session.status" :options="sessionStatusOptions" />
          </div>

          <div class="flex items-center justify-between mt-3">
            <a-button
              v-if="session.status === 1"
              size="small"
              danger
              ghost
              class="text-[10px] h-6"
              @click.stop="handleLockSession(session.id)"
            >
              Khóa phiên
            </a-button>
            <a-button
              v-if="session.status === 1"
              size="small"
              class="text-[10px] h-6 ml-2"
              @click.stop="handleDeleteSession(session)"
            >
              Xóa
            </a-button>
            <span v-else class="text-[10px] text-base-muted">Đã khóa</span>

            <a-button size="small" type="link" class="text-[10px] h-6 px-0" @click.stop="selectSession(session)">
              Xem điểm danh →
            </a-button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="selectedSessionId" class="space-y-4">
      <div class="attendance-record-header">
        <div>
          <h2>
            <span></span>
            Điểm danh - Buổi {{ selectedSession?.sessionNumber }}
            <small v-if="selectedSession?.topic">({{ selectedSession?.topic }})</small>
          </h2>
          <p v-if="records.length && !isSessionLocked">
            Click vào dòng học viên để chọn. Nhấn <kbd>c</kbd> có mặt, <kbd>v</kbd> vắng, <kbd>m</kbd> đi muộn, <kbd>p</kbd> có phép.
          </p>
        </div>

        <div class="attendance-header-actions">
          <a-button size="small" type="primary" :loading="saving" :disabled="!records.length || isSessionLocked" @click="saveBulkRecords">
            Lưu điểm danh
          </a-button>
          <a-button size="small" :disabled="!records.length || isSessionLocked" @click="markAllPresent">
            Đánh dấu tất cả có mặt
          </a-button>
        </div>
      </div>

      <div v-if="records.length && !isSessionLocked" class="attendance-quickbar">
        <div>
          <strong>{{ selectedRecordIds.length }}</strong>
          <span>học viên đang chọn</span>
        </div>
        <div class="attendance-quick-actions">
          <button type="button" @click="applyStatusToSelection(1)">C Có mặt</button>
          <button type="button" @click="applyStatusToSelection(2)">V Vắng</button>
          <button type="button" @click="applyStatusToSelection(3)">M Đi muộn</button>
          <button type="button" @click="applyStatusToSelection(4)">P Có phép</button>
          <button type="button" class="is-muted" :disabled="!selectedRecordIds.length" @click="clearSelectedRecords">Bỏ chọn</button>
        </div>
      </div>

      <div v-if="loadingRecords" class="py-8 flex justify-center">
        <a-spin size="large" />
      </div>

      <div v-else class="bg-card-base border border-base rounded-xl shadow-sm overflow-hidden">
        <a-table
          :data-source="records"
          :columns="recordColumns"
          row-key="id"
          size="small"
          class="custom-table attendance-record-table"
          :row-selection="rowSelection"
          :custom-row="attendanceRowProps"
          :row-class-name="attendanceRowClassName"
          :pagination="{ pageSize: 15, size: 'small', showTotal: (total) => `Tổng ${total} học viên` }"
          :scroll="{ x: 760 }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'studentNameSnapshot'">
              <button type="button" class="attendance-student-button" @click.stop="selectSingleRecord(record)">
                <span class="attendance-student-avatar">{{ studentInitials(record.studentNameSnapshot) }}</span>
                <span class="attendance-student-main">
                  <strong>{{ record.studentNameSnapshot || 'Học viên' }}</strong>
                  <small>{{ shortenId(record.studentId) }}</small>
                </span>
              </button>
            </template>

            <template v-else-if="column.key === 'status'">
              <a-select
                v-model:value="record.status"
                size="small"
                class="w-36 attendance-status-select"
                :disabled="isSessionLocked"
                @click.stop
              >
                <a-select-option :value="1"><span class="text-emerald-600 font-medium">✓ Có mặt</span></a-select-option>
                <a-select-option :value="2"><span class="text-rose-600 font-medium">✕ Vắng mặt</span></a-select-option>
                <a-select-option :value="3"><span class="text-amber-600 font-medium">⏰ Đi muộn</span></a-select-option>
                <a-select-option :value="4"><span class="text-blue-600 font-medium">Có phép</span></a-select-option>
              </a-select>
            </template>

            <template v-else-if="column.key === 'note'">
              <a-input
                v-model:value="record.note"
                size="small"
                placeholder="Nhập lý do..."
                class="text-xs"
                :disabled="isSessionLocked"
                @click.stop
              />
            </template>

            <template v-else-if="column.key === 'markedAt'">
              <span class="text-base-muted text-[10px]">{{ record.markedAt ? formatDateTime(record.markedAt) : '—' }}</span>
            </template>
          </template>

          <template #emptyText>
            <EmptyTableState :show-action-button="false" description="Không có bản ghi điểm danh nào cho phiên này." />
          </template>
        </a-table>
      </div>

      <div v-if="records.length" class="attendance-summary-bar">
        <div><span class="dot present"></span><strong>{{ presentCount }}</strong><span>Có mặt</span></div>
        <div><span class="dot absent"></span><strong>{{ absentCount }}</strong><span>Vắng mặt</span></div>
        <div><span class="dot late"></span><strong>{{ lateCount }}</strong><span>Đi muộn</span></div>
        <div><span class="dot excused"></span><strong>{{ excusedCount }}</strong><span>Có phép</span></div>
        <div class="ml-auto"><span>Tỷ lệ chuyên cần:</span><strong>{{ attendanceRate }}%</strong></div>
      </div>
    </div>

    <div v-if="!selectedClassId" class="py-12">
      <EmptyTableState
        :show-action-button="false"
        title="Chọn một lớp học để bắt đầu"
        description="Hãy chọn lớp học ở trên để xem danh sách phiên điểm danh và quản lý điểm danh học viên."
      />
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { Modal, message } from 'ant-design-vue'
import { PlusOutlined, SyncOutlined } from '@ant-design/icons-vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import EmptyTableState from '@/components/admin/EmptyTableState.vue'
import StatusBadge from '@/components/admin/StatusBadge.vue'
import { attendanceApi } from '@/api/attendanceApi'
import { classApi } from '@/api/classApi'
import { scheduleApi } from '@/api/scheduleApi'
import { tuitionApi } from '@/api/tuitionApi'
import { SESSION_STATUS, toOptions } from '@/lib/constants'

const sessionStatusOptions = toOptions(SESSION_STATUS, { 1: 'green', 2: 'red' })

const classes = ref([])
const sessions = ref([])
const records = ref([])
const selectedClassId = ref(undefined)
const selectedSessionId = ref(undefined)
const selectedSession = ref(null)
const selectedRecordIds = ref([])
const learningHolds = ref([])

const loadingClasses = ref(false)
const loadingSessions = ref(false)
const loadingRecords = ref(false)
const creatingSession = ref(false)
const saving = ref(false)

const isSessionLocked = computed(() => Number(selectedSession.value?.status) === 2 || selectedSession.value?.status === 'Locked')

const recordColumns = [
  { title: 'Học viên', dataIndex: 'studentNameSnapshot', key: 'studentNameSnapshot', sorter: (a, b) => String(a.studentNameSnapshot || '').localeCompare(String(b.studentNameSnapshot || ''), 'vi') },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', width: 180 },
  { title: 'Ghi chú', dataIndex: 'note', key: 'note', width: 300 },
  { title: 'Đánh dấu lúc', dataIndex: 'markedAt', key: 'markedAt', width: 160 },
]

const rowSelection = computed(() => ({
  selectedRowKeys: selectedRecordIds.value,
  hideSelectAll: false,
  getCheckboxProps: (record) => ({ disabled: isSessionLocked.value || isAttendanceLocked(record) }),
  onChange: (keys) => {
    selectedRecordIds.value = keys
  },
}))

const presentCount = computed(() => records.value.filter(r => r.status === 1).length)
const absentCount = computed(() => records.value.filter(r => r.status === 2).length)
const lateCount = computed(() => records.value.filter(r => r.status === 3).length)
const excusedCount = computed(() => records.value.filter(r => r.status === 4).length)
const attendanceRate = computed(() => {
  if (!records.value.length) return 0
  return Math.round((presentCount.value / records.value.length) * 100)
})

async function loadClasses() {
  loadingClasses.value = true
  try {
    const result = await classApi.getAll()
    classes.value = result?.items || result?.data || result || []
  } catch {
    message.error('Không thể tải danh sách lớp học')
  } finally {
    loadingClasses.value = false
  }
}

function normalizeAttendanceStatus(status) {
  const map = { Present: 1, Absent: 2, Late: 3, Excused: 4 }
  if (map[status]) return map[status]
  const numeric = Number(status)
  return Number.isNaN(numeric) ? 1 : numeric
}

function markAllPresent() {
  if (!records.value.length || isSessionLocked.value) return
  records.value.forEach(r => { if (!isAttendanceLocked(r)) r.status = 1 })
  selectedRecordIds.value = records.value.filter(r => !isAttendanceLocked(r)).map(r => r.id)
  message.success('Đã đánh dấu tất cả có mặt')
}

async function loadSessions(classId) {
  if (!classId) return
  loadingSessions.value = true
  sessions.value = []
  selectedSessionId.value = undefined
  selectedSession.value = null
  records.value = []
  selectedRecordIds.value = []
  try {
    sessions.value = await attendanceApi.getSessionsByClass(classId) || []
  } catch {
    message.error('Không thể tải phiên điểm danh')
  } finally {
    loadingSessions.value = false
  }
}

async function loadRecords(sessionId) {
  if (!sessionId) return
  loadingRecords.value = true
  records.value = []
  selectedRecordIds.value = []
  try {
    await loadLearningHolds(selectedSession.value?.classId || selectedClassId.value)
    const result = await attendanceApi.getRecordsBySession(sessionId)
    records.value = (result || []).map((record) => ({
      ...record,
      status: normalizeAttendanceStatus(record.status),
      isLearningHeld: learningHolds.value.some(x => String(x.studentId).toLowerCase() === String(record.studentId).toLowerCase()),
    }))
  } catch {
    message.error('Không thể tải bản ghi điểm danh')
  } finally {
    loadingRecords.value = false
  }
}

async function loadLearningHolds(classId) {
  learningHolds.value = []
  if (!classId) return
  try {
    learningHolds.value = await tuitionApi.getLearningHolds({ classId })
  } catch (error) {
    message.warning(error.message || 'Không thể kiểm tra trạng thái khóa học phí')
  }
}

function isAttendanceLocked(record) {
  return record?.isLearningHeld || learningHolds.value.some(x => String(x.studentId).toLowerCase() === String(record?.studentId).toLowerCase())
}

function handleClassChange(classId) {
  selectedSessionId.value = undefined
  selectedSession.value = null
  records.value = []
  selectedRecordIds.value = []
  loadSessions(classId)
}

function selectSession(session) {
  selectedSessionId.value = session.id
  selectedSession.value = session
  loadRecords(session.id)
}

function handleCreateSession() {
  if (!selectedClassId.value) return
  Modal.confirm({
    title: 'Tạo phiên điểm danh mới?',
    content: 'Hệ thống sẽ tạo bản ghi điểm danh cho toàn bộ học viên đang học trong lớp. Bạn có chắc muốn tạo phiên này?',
    okText: 'Tạo phiên',
    cancelText: 'Hủy',
    async onOk() {
      creatingSession.value = true
      try {
        const cls = classes.value.find(x => x.id === selectedClassId.value)
        const schedules = await scheduleApi.getByClass(selectedClassId.value)
        const schedule = schedules?.[0]
        if (!schedule) throw new Error('Lớp chưa có lịch học nên chưa thể tạo phiên điểm danh')
        const sessionNumber = nextSessionNumber()
        await attendanceApi.createSession({
          classId: selectedClassId.value,
          classNameSnapshot: cls?.className || '',
          scheduleId: schedule.id,
          sessionNumber,
          attendanceDate: cls?.startDate || new Date().toISOString(),
          topic: schedule.topic || `Buổi ${sessionNumber}`,
          createdByTeacherId: schedule.assignedTeacherId || schedule.teacherId,
          createdByTeacherName: schedule.assignedTeacherNameSnapshot || schedule.teacherNameSnapshot || 'Admin',
        })
        message.success('Đã tạo phiên điểm danh mới')
        await loadSessions(selectedClassId.value)
      } catch (error) {
        message.error(error.message || 'Không thể tạo phiên điểm danh')
      } finally {
        creatingSession.value = false
      }
    },
  })
}

function nextSessionNumber() {
  const used = new Set(sessions.value.map(x => Number(x.sessionNumber)))
  for (let i = 1; i <= 300; i += 1) if (!used.has(i)) return i
  return sessions.value.length + 1
}

async function handleDeleteSession(session) {
  Modal.confirm({
    title: 'Xóa phiên điểm danh?',
    content: `Phiên buổi ${session.sessionNumber} sẽ bị xóa cùng toàn bộ bản ghi điểm danh liên quan. Chỉ nên xóa khi tạo nhầm hoặc dữ liệu test.`,
    okText: 'Xóa phiên',
    okType: 'danger',
    cancelText: 'Hủy',
    async onOk() {
      try {
        await attendanceApi.deleteSession(session.id)
        message.success('Đã xóa phiên điểm danh')
        if (selectedSessionId.value === session.id) {
          selectedSessionId.value = undefined
          selectedSession.value = null
          records.value = []
          selectedRecordIds.value = []
        }
        await loadSessions(selectedClassId.value)
      } catch (error) {
        message.error(error.message || 'Không thể xóa phiên điểm danh')
      }
    },
  })
}

async function handleLockSession(sessionId) {
  try {
    await attendanceApi.lockSession(sessionId)
    message.success('Đã khóa phiên điểm danh')
    await loadSessions(selectedClassId.value)
    if (selectedSessionId.value === sessionId) {
      const updated = sessions.value.find(s => s.id === sessionId)
      if (updated) selectedSession.value = updated
    }
  } catch (error) {
    message.error(error.message || 'Không thể khóa phiên')
  }
}

async function saveBulkRecords() {
  if (!selectedSessionId.value) return
  const editableRecords = records.value.filter(r => !isAttendanceLocked(r))
  const skippedCount = records.value.length - editableRecords.length
  if (!editableRecords.length) {
    message.warning('Tất cả học viên trong phiên này đang bị khóa điểm danh do quá hạn học phí')
    return
  }
  saving.value = true
  try {
    await attendanceApi.bulkUpdateRecords({
      attendanceSessionId: selectedSessionId.value,
      records: editableRecords.map(r => ({
        studentId: r.studentId,
        status: r.status,
        note: r.note || '',
      })),
    })
    message.success(skippedCount ? `Đã lưu điểm danh. Bỏ qua ${skippedCount} học viên bị khóa học phí.` : 'Đã lưu điểm danh thành công')
    await loadRecords(selectedSessionId.value)
  } catch (error) {
    message.error(error.message || 'Không thể lưu điểm danh')
  } finally {
    saving.value = false
  }
}

function attendanceRowProps(record) {
  return {
    onClick: (event) => {
      if (isInteractiveTarget(event.target)) return
      selectSingleRecord(record)
    },
  }
}

function attendanceRowClassName(record) {
  return isAttendanceLocked(record) ? 'attendance-row-locked' : (selectedRecordIds.value.includes(record.id) ? 'attendance-row-selected' : 'attendance-row-clickable')
}

function selectSingleRecord(record) {
  if (isSessionLocked.value || isAttendanceLocked(record)) return
  selectedRecordIds.value = [record.id]
}

function clearSelectedRecords() {
  selectedRecordIds.value = []
}

function applyStatusToSelection(status) {
  if (isSessionLocked.value) return
  if (!selectedRecordIds.value.length) {
    message.info('Hãy click chọn một hoặc nhiều học viên trước')
    return
  }
  const selected = new Set(selectedRecordIds.value)
  records.value.forEach((record) => {
    if (selected.has(record.id) && !isAttendanceLocked(record)) record.status = status
  })
}

function handleKeyboardShortcut(event) {
  if (isInteractiveTarget(event.target) || !records.value.length || isSessionLocked.value) return
  const status = { c: 1, v: 2, m: 3, p: 4 }[event.key?.toLowerCase()]
  if (!status) return
  event.preventDefault()
  applyStatusToSelection(status)
}

function isInteractiveTarget(target) {
  const tagName = target?.tagName?.toLowerCase()
  return ['input', 'textarea', 'select', 'button'].includes(tagName) || target?.closest?.('.ant-select, .ant-input, .ant-checkbox-wrapper, .ant-pagination')
}

function studentInitials(name) {
  const parts = String(name || 'HV').trim().split(/\s+/)
  return parts.slice(-2).map(x => x[0]).join('').toUpperCase()
}

function shortenId(value) {
  if (!value) return '---'
  return `${String(value).slice(0, 8)}...`
}

function formatDate(value) {
  if (!value) return '—'
  return new Date(value).toLocaleDateString('vi-VN')
}

function formatDateTime(value) {
  if (!value) return '—'
  return new Date(value).toLocaleString('vi-VN')
}

onMounted(() => {
  loadClasses()
  window.addEventListener('keydown', handleKeyboardShortcut)
})

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeyboardShortcut)
})
</script>

<style scoped>
.attendance-control-card {
  background:
    linear-gradient(180deg, rgba(59, 130, 246, 0.035), transparent 72%),
    var(--bg-card);
}

.attendance-control-card label {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  margin: 0;
  color: var(--text-primary);
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.01em;
}

.attendance-control-card label::before {
  content: "";
  width: 4px;
  height: 14px;
  border-radius: 999px;
  background: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.attendance-class-select :deep(.ant-select-selector) {
  min-height: 40px !important;
  padding: 4px 12px !important;
  border-radius: 10px !important;
}

.attendance-class-select :deep(.ant-select-selection-search-input) {
  height: 38px !important;
}

.attendance-class-select :deep(.ant-select-selection-placeholder),
.attendance-class-select :deep(.ant-select-selection-item) {
  display: flex;
  align-items: center;
  min-height: 30px;
  font-size: 13px;
}

.attendance-create-btn,
.attendance-refresh-btn {
  display: inline-flex !important;
  align-items: center !important;
  justify-content: center !important;
  gap: 8px !important;
  border-radius: 10px !important;
  font-size: 13px !important;
  font-weight: 700 !important;
}

.attendance-create-btn {
  height: 40px !important;
  min-width: 220px;
}

.attendance-refresh-btn {
  height: 34px !important;
  border-color: #cbd5e1 !important;
  background: #ffffff !important;
  color: #0f172a !important;
}

.attendance-record-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.attendance-record-header h2 {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 0;
  color: var(--text-primary);
  font-size: 14px;
  font-weight: 800;
}

.attendance-record-header h2 > span {
  width: 6px;
  height: 15px;
  border-radius: 999px;
  background: #059669;
}

.attendance-record-header small {
  color: var(--text-muted);
  font-size: 12px;
  font-weight: 500;
}

.attendance-record-header p,
.attendance-hotkey-hint {
  margin: 7px 0 0;
  color: #64748b;
  font-size: 12px;
  line-height: 1.45;
}

kbd {
  display: inline-flex;
  min-width: 19px;
  height: 19px;
  align-items: center;
  justify-content: center;
  border: 1px solid #cbd5e1;
  border-bottom-width: 2px;
  border-radius: 5px;
  background: #ffffff;
  color: #0f172a;
  font-size: 11px;
  font-weight: 800;
  line-height: 1;
}

.attendance-header-actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 8px;
}

.attendance-quickbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 12px;
  border: 1px solid #dbeafe;
  border-radius: 12px;
  background: #eff6ff;
}

.attendance-quickbar > div:first-child {
  display: flex;
  align-items: baseline;
  gap: 6px;
  color: #1e3a8a;
  font-size: 12px;
}

.attendance-quickbar strong {
  font-size: 18px;
  font-weight: 900;
}

.attendance-quick-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.attendance-quick-actions button {
  height: 30px;
  padding: 0 10px;
  border: 1px solid #bfdbfe;
  border-radius: 8px;
  background: #ffffff;
  color: #1d4ed8;
  font-size: 12px;
  font-weight: 800;
  cursor: pointer;
}

.attendance-quick-actions button:hover {
  border-color: #2563eb;
  background: #dbeafe;
}

.attendance-quick-actions button.is-muted {
  border-color: #cbd5e1;
  color: #64748b;
}

.attendance-quick-actions button:disabled {
  cursor: not-allowed;
  opacity: 0.55;
}

.attendance-record-table :deep(.attendance-row-clickable),
.attendance-record-table :deep(.attendance-row-selected) {
  cursor: pointer;
}

.attendance-record-table :deep(.attendance-row-selected > td) {
  background: #eef4ff !important;
}

.attendance-record-table :deep(.ant-table-tbody > tr:hover > td) {
  background: #f8fbff;
}

.attendance-student-button {
  display: flex;
  width: 100%;
  min-width: 260px;
  align-items: center;
  gap: 12px;
  border: 0;
  background: transparent;
  color: inherit;
  text-align: left;
  cursor: pointer;
}

.attendance-student-avatar {
  display: inline-flex;
  width: 34px;
  height: 34px;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  border-radius: 999px;
  background: #f59e0b;
  color: #ffffff;
  font-size: 11px;
  font-weight: 900;
}

.attendance-student-main {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 2px;
}

.attendance-student-main strong {
  color: #0f172a;
  font-size: 13px;
  font-weight: 800;
}

.attendance-student-main small {
  color: #64748b;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-size: 10px;
}

.attendance-summary-bar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 16px;
  padding: 12px;
  border: 1px solid var(--border-base);
  border-radius: 12px;
  background: #f8fafc;
  font-size: 12px;
}

.attendance-summary-bar > div {
  display: flex;
  align-items: center;
  gap: 6px;
}

.attendance-summary-bar strong {
  color: var(--text-primary);
  font-weight: 900;
}

.dot {
  width: 8px;
  height: 8px;
  border-radius: 999px;
}

.dot.present { background: #10b981; }
.dot.absent { background: #f43f5e; }
.dot.late { background: #f59e0b; }
.dot.excused { background: #3b82f6; }

.dark .attendance-refresh-btn {
  background: #111b2f !important;
  border-color: #334155 !important;
  color: #e5edf8 !important;
}

.dark .attendance-quickbar {
  border-color: rgba(96, 165, 250, 0.3);
  background: rgba(37, 99, 235, 0.12);
}

.dark .attendance-quick-actions button,
.dark kbd {
  border-color: #334155;
  background: #111827;
  color: #bfdbfe;
}

.dark .attendance-record-table :deep(.attendance-row-selected > td) {
  background: rgba(37, 99, 235, 0.18) !important;
}

.dark .attendance-student-main strong {
  color: #e5edf8;
}

.dark .attendance-summary-bar {
  background: rgba(15, 23, 42, 0.55);
}

@media (max-width: 767px) {
  .attendance-create-btn {
    width: 100%;
    min-width: 0;
  }

  .attendance-record-header,
  .attendance-quickbar {
    align-items: stretch;
    flex-direction: column;
  }

  .attendance-header-actions {
    justify-content: flex-start;
  }
}
</style>

