<template>
  <div class="space-y-6">
    <PageHeader title="Điểm danh lớp học" subtitle="Tạo buổi điểm danh, cập nhật trạng thái hàng loạt và khóa buổi học.">
      <template #actions>
        <a-button :loading="loading" @click="loadBaseData">Làm mới</a-button>
      </template>
    </PageHeader>

    <section class="teacher-panel">
      <div class="grid grid-cols-1 lg:grid-cols-4 gap-3">
        <a-select v-model:value="selectedClassId" placeholder="Chọn lớp" class="w-full" @change="onClassChange">
          <a-select-option v-for="cls in classes" :key="cls.id" :value="cls.id">{{ cls.className }}</a-select-option>
        </a-select>
        <a-select v-model:value="selectedScheduleId" placeholder="Lịch học" class="w-full">
          <a-select-option v-for="sch in schedules" :key="sch.id" :value="sch.id">
            Buổi {{ sch.sessionNumber }} · {{ sch.topic || sch.classNameSnapshot }}
          </a-select-option>
        </a-select>
        <a-date-picker v-model:value="attendanceDate" class="w-full" format="DD/MM/YYYY" />
        <a-button type="primary" :disabled="!canCreateSession" :loading="creatingSession" @click="createSession">Tạo buổi điểm danh</a-button>
      </div>
    </section>

    <section class="grid grid-cols-1 xl:grid-cols-3 gap-4">
      <div class="teacher-panel">
        <div class="teacher-panel-head">
          <div>
            <h2>Buổi điểm danh</h2>
            <p>Chọn một buổi để tải danh sách học viên.</p>
          </div>
        </div>
        <div v-if="sessions.length === 0" class="teacher-empty">Chưa có buổi điểm danh cho lớp này.</div>
        <button
          v-for="session in sessions"
          :key="session.id"
          type="button"
          class="teacher-session-row"
          :class="{ 'is-active': selectedSessionId === session.id }"
          @click="selectSession(session)"
        >
          <div>
            <p>Buổi {{ session.sessionNumber }} · {{ session.topic }}</p>
            <span>{{ formatDate(session.attendanceDate) }}</span>
          </div>
          <a-tag :color="sessionLocked(session) ? 'red' : 'green'">{{ sessionLocked(session) ? 'Đã khóa' : 'Đang mở' }}</a-tag>
        </button>
      </div>

      <div class="teacher-panel xl:col-span-2">
        <div class="teacher-panel-head">
          <div>
            <h2>Danh sách điểm danh</h2>
            <p v-if="selectedSession">Lớp {{ selectedSession.classNameSnapshot }} · {{ selectedSession.topic }}</p>
            <p v-else>Chọn buổi điểm danh để thao tác.</p>
          </div>
          <div class="flex gap-2">
            <a-button :disabled="!selectedSession || sessionLocked(selectedSession)" :loading="saving" @click="saveRecords">Lưu điểm danh</a-button>
            <a-button danger :disabled="!selectedSession || sessionLocked(selectedSession)" :loading="locking" @click="lockSession">Khóa buổi</a-button>
          </div>
        </div>

        <a-table
          :data-source="records"
          :columns="columns"
          :loading="loadingRecords"
          row-key="studentId"
          size="small"
          :pagination="{ pageSize: 10, showSizeChanger: false }"
          :scroll="{ x: 760 }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'student'">
              <p class="font-semibold text-base-primary">{{ record.studentNameSnapshot }}</p>
              <p class="text-xs text-base-secondary">{{ record.studentId }}</p>
            </template>
            <template v-else-if="column.key === 'status'">
              <a-select v-model:value="record.status" class="w-[140px]" :disabled="!selectedSession || sessionLocked(selectedSession)">
                <a-select-option value="Present">Có mặt</a-select-option>
                <a-select-option value="Absent">Vắng</a-select-option>
                <a-select-option value="Late">Đi muộn</a-select-option>
                <a-select-option value="Excused">Có phép</a-select-option>
              </a-select>
            </template>
            <template v-else-if="column.key === 'note'">
              <a-input v-model:value="record.note" placeholder="Ghi chú" :disabled="!selectedSession || sessionLocked(selectedSession)" />
            </template>
          </template>
        </a-table>
      </div>
    </section>
  </div>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import dayjs from 'dayjs'
import { message } from 'ant-design-vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import { classApi } from '@/api/classApi'
import { scheduleApi } from '@/api/scheduleApi'
import { enrollmentApi } from '@/api/enrollmentApi'
import { attendanceApi } from '@/api/attendanceApi'
import { useAuthStore } from '@/stores/auth'
import { formatDate } from '@/lib/formatters'

const props = defineProps({ classId: { type: String, default: '' } })
const auth = useAuthStore()
const loading = ref(false)
const loadingRecords = ref(false)
const creatingSession = ref(false)
const saving = ref(false)
const locking = ref(false)
const classes = ref([])
const schedules = ref([])
const enrollments = ref([])
const sessions = ref([])
const records = ref([])
const selectedClassId = ref(props.classId || undefined)
const selectedScheduleId = ref(undefined)
const selectedSessionId = ref(undefined)
const attendanceDate = ref(dayjs())

const selectedClass = computed(() => classes.value.find(x => x.id === selectedClassId.value))
const selectedSchedule = computed(() => schedules.value.find(x => x.id === selectedScheduleId.value))
const selectedSession = computed(() => sessions.value.find(x => x.id === selectedSessionId.value))
const canCreateSession = computed(() => selectedClass.value && selectedSchedule.value && attendanceDate.value)
const columns = [
  { title: 'Học viên', key: 'student', width: 260 },
  { title: 'Trạng thái', key: 'status', width: 170 },
  { title: 'Ghi chú', key: 'note' }
]

function sessionLocked(session) { return String(session?.status) === 'Locked' || Number(session?.status) === 2 }
function normalizedStatus(value) {
  const map = { 1: 'Present', 2: 'Absent', 3: 'Late', 4: 'Excused', Present: 'Present', Absent: 'Absent', Late: 'Late', Excused: 'Excused' }
  return map[value] || 'Present'
}
function classNameForPayload() { return selectedClass.value?.className || selectedSchedule.value?.classNameSnapshot || '' }
function activeEnrollments() {
  return enrollments.value.filter(x => ['Confirmed', 'Studying', 2, 3, '2', '3'].includes(x.status))
}
async function loadBaseData() {
  if (!auth.user?.referenceId) return
  loading.value = true
  try {
    classes.value = await classApi.getByTeacher(auth.user.referenceId)
    if (!selectedClassId.value && classes.value.length) selectedClassId.value = classes.value[0].id
    await onClassChange()
  } finally {
    loading.value = false
  }
}
async function onClassChange() {
  records.value = []
  selectedSessionId.value = undefined
  if (!selectedClassId.value) return
  const [sch, en, ses] = await Promise.all([
    scheduleApi.getByClass(selectedClassId.value).catch(() => []),
    enrollmentApi.getByClass(selectedClassId.value).catch(() => []),
    attendanceApi.getSessionsByClass(selectedClassId.value).catch(() => [])
  ])
  schedules.value = sch || []
  enrollments.value = en || []
  sessions.value = ses || []
  selectedScheduleId.value = schedules.value[0]?.id
}
async function createSession() {
  creatingSession.value = true
  try {
    const payload = {
      classId: selectedClassId.value,
      classNameSnapshot: classNameForPayload(),
      scheduleId: selectedScheduleId.value,
      sessionNumber: selectedSchedule.value?.sessionNumber || sessions.value.length + 1,
      attendanceDate: attendanceDate.value?.toISOString?.() || new Date().toISOString(),
      topic: selectedSchedule.value?.topic || `Buổi ${selectedSchedule.value?.sessionNumber || sessions.value.length + 1}`,
      createdByTeacherId: auth.user.referenceId,
      createdByTeacherName: auth.user.fullName || auth.user.username
    }
    const created = await attendanceApi.createSession(payload)
    sessions.value = [created, ...sessions.value]
    await selectSession(created)
    message.success('Đã tạo buổi điểm danh')
  } catch (error) {
    message.error(error.message || 'Không tạo được buổi điểm danh')
  } finally {
    creatingSession.value = false
  }
}
async function selectSession(session) {
  selectedSessionId.value = session.id
  loadingRecords.value = true
  try {
    const data = await attendanceApi.getRecordsBySession(session.id)
    if (data?.length) {
      records.value = data.map(x => ({ ...x, status: normalizedStatus(x.status) }))
    } else {
      records.value = activeEnrollments().map(x => ({
        id: null,
        attendanceSessionId: session.id,
        studentId: x.studentId,
        studentNameSnapshot: x.studentNameSnapshot,
        status: 'Present',
        note: ''
      }))
    }
  } finally {
    loadingRecords.value = false
  }
}
async function saveRecords() {
  if (!selectedSession.value) return
  saving.value = true
  try {
    const saved = await attendanceApi.bulkUpdateRecords({
      attendanceSessionId: selectedSession.value.id,
      records: records.value.map(x => ({ studentId: x.studentId, status: x.status, note: x.note }))
    })
    records.value = saved.map(x => ({ ...x, status: normalizedStatus(x.status) }))
    message.success('Đã lưu điểm danh')
  } catch (error) {
    message.error(error.message || 'Không lưu được điểm danh')
  } finally {
    saving.value = false
  }
}
async function lockSession() {
  if (!selectedSession.value) return
  locking.value = true
  try {
    const locked = await attendanceApi.lockSession(selectedSession.value.id)
    sessions.value = sessions.value.map(x => x.id === locked.id ? locked : x)
    message.success('Đã khóa buổi điểm danh')
  } finally {
    locking.value = false
  }
}

watch(() => props.classId, (value) => { if (value) selectedClassId.value = value })
onMounted(loadBaseData)
</script>

<style scoped>
.teacher-panel { background: var(--admin-surface); border: 1px solid var(--admin-border); border-radius: 18px; padding: 16px; box-shadow: var(--admin-shadow-sm); }
.teacher-panel-head { display: flex; justify-content: space-between; align-items: flex-start; gap: 16px; margin-bottom: 16px; }
.teacher-panel-head h2 { color: var(--admin-text); font-weight: 800; font-size: 16px; }
.teacher-panel-head p { color: var(--admin-text-muted); font-size: 12px; }
.teacher-empty { min-height: 120px; display: grid; place-items: center; color: var(--admin-text-muted); font-size: 13px; }
.teacher-session-row { width: 100%; display: flex; justify-content: space-between; align-items: center; gap: 12px; padding: 12px; border: 1px solid var(--admin-border); border-radius: 14px; background: var(--admin-surface-2); margin-bottom: 10px; text-align: left; transition: all .2s ease; }
.teacher-session-row:hover,.teacher-session-row.is-active { border-color: rgba(37,99,235,.45); box-shadow: var(--admin-shadow-sm); }
.teacher-session-row p { color: var(--admin-text); font-weight: 700; font-size: 13px; }
.teacher-session-row span { color: var(--admin-text-muted); font-size: 12px; }
</style>
