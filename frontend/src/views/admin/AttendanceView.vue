<template>
  <div class="space-y-6">
    <PageHeader title="Điểm danh" subtitle="Tạo buổi điểm danh, khóa phiên và cập nhật điểm danh hàng loạt." />

    <!-- Class Selection Panel -->
    <div class="bg-card-base border border-base rounded-xl p-4 shadow-sm">
      <div class="flex flex-col md:flex-row md:items-end gap-3">
        <div class="flex-1 space-y-1">
          <label class="text-xs font-semibold text-base-secondary">Chọn lớp học</label>
          <a-select
            v-model:value="selectedClassId"
            placeholder="Chọn lớp để xem phiên điểm danh..."
            size="small"
            class="w-full"
            :loading="loadingClasses"
            show-search
            option-filter-prop="children"
            @change="handleClassChange"
          >
            <a-select-option v-for="cls in classes" :key="cls.id" :value="cls.id">
              {{ cls.className }} — {{ cls.courseNameSnapshot }}
            </a-select-option>
          </a-select>
        </div>

        <a-button
          size="small"
          type="primary"
          :loading="creatingSession"
          :disabled="!selectedClassId"
          @click="handleCreateSession"
        >
          <template #icon><PlusOutlined /></template>
          Tạo phiên điểm danh mới
        </a-button>
      </div>
    </div>

    <!-- Sessions Panel -->
    <div v-if="selectedClassId">
      <div class="flex items-center justify-between mb-3">
        <h2 class="text-sm font-bold text-base-primary flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-blue-600 rounded-full"></span>
          Danh sách phiên điểm danh
        </h2>
        <a-button
          size="small"
          :loading="loadingSessions"
          @click="loadSessions(selectedClassId)"
        >
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

      <!-- Session Cards -->
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
                Buổi {{ session.sessionNumber }} — {{ formatDate(session.attendanceDate) }}
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
            <span v-else class="text-[10px] text-base-muted">Đã khóa</span>

            <a-button
              size="small"
              type="link"
              class="text-[10px] h-6 px-0"
              @click.stop="selectSession(session)"
            >
              Xem điểm danh →
            </a-button>
          </div>
        </div>
      </div>
    </div>

    <!-- Attendance Records Panel -->
    <div v-if="selectedSessionId" class="space-y-4">
      <div class="flex items-center justify-between">
        <h2 class="text-sm font-bold text-base-primary flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-emerald-600 rounded-full"></span>
          Điểm danh — Buổi {{ selectedSession?.sessionNumber }}
          <span v-if="selectedSession?.topic" class="text-base-muted font-normal text-xs">({{ selectedSession?.topic }})</span>
        </h2>
        <a-button
          size="small"
          type="primary"
          :loading="saving"
          :disabled="!records.length || selectedSession?.status === 2"
          @click="saveBulkRecords"
        >
          Lưu điểm danh
        </a-button>
        <a-button
          size="small"
          :disabled="!records.length || selectedSession?.status === 2"
          @click="markAllPresent"
        >
          Đánh dấu tất cả có mặt
        </a-button>
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
          class="custom-table"
          :pagination="{ pageSize: 15, size: 'small', showTotal: (total) => `Tổng ${total} học viên` }"
          :scroll="{ x: 700 }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'status'">
              <a-select
                v-model:value="record.status"
                size="small"
                class="w-32"
                :disabled="selectedSession?.status === 2"
              >
                <a-select-option :value="1">
                  <span class="text-emerald-600 font-medium">✓ Có mặt</span>
                </a-select-option>
                <a-select-option :value="2">
                  <span class="text-rose-600 font-medium">✗ Vắng mặt</span>
                </a-select-option>
                <a-select-option :value="3">
                  <span class="text-amber-600 font-medium">⏰ Đi muộn</span>
                </a-select-option>
                <a-select-option :value="4">
                  <span class="text-blue-600 font-medium">📋 Có phép</span>
                </a-select-option>
              </a-select>
            </template>
            <template v-else-if="column.key === 'note'">
              <a-input
                v-model:value="record.note"
                size="small"
                placeholder="Ghi chú..."
                class="text-xs"
                :disabled="selectedSession?.status === 2"
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

      <!-- Quick summary bar -->
      <div v-if="records.length" class="flex flex-wrap items-center gap-4 text-xs p-3 bg-slate-50 dark:bg-slate-900/50 rounded-xl border border-base">
        <div class="flex items-center gap-1.5">
          <span class="w-2 h-2 rounded-full bg-emerald-500"></span>
          <span class="font-bold text-emerald-700 dark:text-emerald-400">{{ presentCount }}</span>
          <span class="text-base-secondary">Có mặt</span>
        </div>
        <div class="flex items-center gap-1.5">
          <span class="w-2 h-2 rounded-full bg-rose-500"></span>
          <span class="font-bold text-rose-700 dark:text-rose-400">{{ absentCount }}</span>
          <span class="text-base-secondary">Vắng mặt</span>
        </div>
        <div class="flex items-center gap-1.5">
          <span class="w-2 h-2 rounded-full bg-amber-500"></span>
          <span class="font-bold text-amber-700 dark:text-amber-400">{{ lateCount }}</span>
          <span class="text-base-secondary">Đi muộn</span>
        </div>
        <div class="flex items-center gap-1.5">
          <span class="w-2 h-2 rounded-full bg-blue-500"></span>
          <span class="font-bold text-blue-700 dark:text-blue-400">{{ excusedCount }}</span>
          <span class="text-base-secondary">Có phép</span>
        </div>
        <div class="flex items-center gap-1.5 ml-auto">
          <span class="text-base-secondary">Tỷ lệ chuyên cần:</span>
          <span class="font-bold text-base-primary">{{ attendanceRate }}%</span>
        </div>
      </div>
    </div>

    <!-- Initial empty state when no class is selected -->
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
import { computed, onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import { PlusOutlined, SyncOutlined } from '@ant-design/icons-vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import EmptyTableState from '@/components/admin/EmptyTableState.vue'
import StatusBadge from '@/components/admin/StatusBadge.vue'
import { attendanceApi } from '@/api/attendanceApi'
import { classApi } from '@/api/classApi'
import { SESSION_STATUS, ATTENDANCE_STATUS, toOptions } from '@/lib/constants'

const sessionStatusOptions = toOptions(SESSION_STATUS, { 1: 'green', 2: 'red' })

// Data refs
const classes = ref([])
const sessions = ref([])
const records = ref([])
const selectedClassId = ref(undefined)
const selectedSessionId = ref(undefined)
const selectedSession = ref(null)

// Loading states
const loadingClasses = ref(false)
const loadingSessions = ref(false)
const loadingRecords = ref(false)
const creatingSession = ref(false)
const saving = ref(false)

// Table columns
const recordColumns = [
  { title: 'Học viên', dataIndex: 'studentNameSnapshot', key: 'studentNameSnapshot', sorter: (a, b) => String(a.studentNameSnapshot).localeCompare(String(b.studentNameSnapshot)) },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', width: 150 },
  { title: 'Ghi chú', dataIndex: 'note', key: 'note', width: 220 },
  { title: 'Đánh dấu lúc', dataIndex: 'markedAt', key: 'markedAt', width: 160 },
]

// Computed statistics
const presentCount = computed(() => records.value.filter(r => r.status === 1).length)
const absentCount = computed(() => records.value.filter(r => r.status === 2).length)
const lateCount = computed(() => records.value.filter(r => r.status === 3).length)
const excusedCount = computed(() => records.value.filter(r => r.status === 4).length)
const attendanceRate = computed(() => {
  if (!records.value.length) return 0
  return Math.round((presentCount.value / records.value.length) * 100)
})

// Load all classes on mount
async function loadClasses() {
  loadingClasses.value = true
  try {
    const result = await classApi.getAll()
    classes.value = result?.items || result?.data || result || []
  } catch (error) {
    message.error('Không thể tải danh sách lớp học')
  } finally {
    loadingClasses.value = false
  }
}

// Mark all students as present
function markAllPresent() {
  if (!records.value.length || selectedSession.value?.status === 2) return
  records.value.forEach(r => {
    r.status = 1
  })
  message.success('Đã đánh dấu tất cả có mặt')
}

// Load sessions for a class
async function loadSessions(classId) {
  if (!classId) return
  loadingSessions.value = true
  sessions.value = []
  selectedSessionId.value = undefined
  selectedSession.value = null
  records.value = []
  try {
    const result = await attendanceApi.getSessionsByClass(classId)
    sessions.value = result || []
  } catch (error) {
    message.error('Không thể tải phiên điểm danh')
  } finally {
    loadingSessions.value = false
  }
}

// Load records for a session
async function loadRecords(sessionId) {
  if (!sessionId) return
  loadingRecords.value = true
  records.value = []
  try {
    const result = await attendanceApi.getRecordsBySession(sessionId)
    records.value = result || []
  } catch (error) {
    message.error('Không thể tải bản ghi điểm danh')
  } finally {
    loadingRecords.value = false
  }
}

// Handler: class selection changed
function handleClassChange(classId) {
  selectedSessionId.value = undefined
  selectedSession.value = null
  records.value = []
  loadSessions(classId)
}

// Handler: select session + load records
function selectSession(session) {
  selectedSessionId.value = session.id
  selectedSession.value = session
  loadRecords(session.id)
}

// Handler: create a new session
async function handleCreateSession() {
  if (!selectedClassId.value) return
  creatingSession.value = true
  try {
    const cls = classes.value.find(c => c.id === selectedClassId.value)
    const sessionNumber = sessions.value.length + 1
    await attendanceApi.createSession({
      classId: selectedClassId.value,
      sessionNumber,
      topic: `Buổi ${sessionNumber}`,
    })
    message.success('Đã tạo phiên điểm danh mới')
    await loadSessions(selectedClassId.value)
  } catch (error) {
    message.error(error.message || 'Không thể tạo phiên điểm danh')
  } finally {
    creatingSession.value = false
  }
}

// Handler: lock session
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

// Handler: save attendance records
async function saveBulkRecords() {
  if (!selectedSessionId.value) return
  saving.value = true
  try {
    await attendanceApi.bulkUpdateRecords({
      attendanceSessionId: selectedSessionId.value,
      records: records.value.map(r => ({
        studentId: r.studentId,
        status: r.status,
        note: r.note || ''
      }))
    })
    message.success('Đã lưu điểm danh thành công')
    await loadRecords(selectedSessionId.value)
  } catch (error) {
    message.error(error.message || 'Không thể lưu điểm danh')
  } finally {
    saving.value = false
  }
}

// Formatters
function formatDate(value) {
  if (!value) return '—'
  return new Date(value).toLocaleDateString('vi-VN')
}

function formatDateTime(value) {
  if (!value) return '—'
  return new Date(value).toLocaleString('vi-VN')
}

onMounted(loadClasses)
</script>
