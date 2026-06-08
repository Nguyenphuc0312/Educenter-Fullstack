<template>
  <div class="space-y-6">
    <PageHeader :title="classInfo?.className || 'Chi tiết lớp học'" :subtitle="classInfo?.courseNameSnapshot || 'Thông tin lớp, học viên, lịch học và kết quả.'">
      <template #actions>
        <router-link to="/teacher/classes"><a-button>Quay lại</a-button></router-link>
        <router-link v-if="classInfo" :to="`/teacher/classes/${classInfo.id}/attendance`"><a-button type="primary">Điểm danh</a-button></router-link>
      </template>
    </PageHeader>

    <section class="grid grid-cols-1 lg:grid-cols-4 gap-4">
      <div class="teacher-info-card">
        <span>Mã lớp</span>
        <strong>{{ classInfo?.classCode || '-' }}</strong>
      </div>
      <div class="teacher-info-card">
        <span>Sĩ số</span>
        <strong>{{ classInfo?.currentStudents || 0 }}/{{ classInfo?.maxStudents || 0 }}</strong>
      </div>
      <div class="teacher-info-card">
        <span>Phòng</span>
        <strong>{{ classInfo?.room || '-' }}</strong>
      </div>
      <div class="teacher-info-card">
        <span>Thời gian</span>
        <strong>{{ formatDate(classInfo?.startDate) }} - {{ formatDate(classInfo?.endDate) }}</strong>
      </div>
    </section>

    <section class="teacher-panel">
      <a-tabs v-model:activeKey="activeTab">
        <a-tab-pane key="students" tab="Học viên">
          <a-table :data-source="enrollments" :columns="studentColumns" :loading="loading" row-key="id" size="small" :pagination="{ pageSize: 8 }">
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'student'">
                <p class="font-semibold text-base-primary">{{ record.studentNameSnapshot }}</p>
                <p class="text-xs text-base-secondary">{{ record.studentId }}</p>
              </template>
              <template v-else-if="column.key === 'status'">
                <a-tag :color="enrollmentColor(record.status)">{{ enrollmentLabel(record.status) }}</a-tag>
              </template>
            </template>
          </a-table>
        </a-tab-pane>
        <a-tab-pane key="schedule" tab="Lịch học">
          <a-table :data-source="schedules" :columns="scheduleColumns" :loading="loading" row-key="id" size="small" :pagination="{ pageSize: 8 }">
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'time'">{{ dayLabel(record.dayOfWeek) }} · {{ shiftLabel(record.studyShift) }} · {{ formatTime(record.startTime) }} - {{ formatTime(record.endTime) }}</template>
            </template>
          </a-table>
        </a-tab-pane>
        <a-tab-pane key="attendance" tab="Buổi điểm danh">
          <a-table :data-source="sessions" :columns="sessionColumns" :loading="loading" row-key="id" size="small" :pagination="{ pageSize: 8 }">
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'status'"><a-tag :color="String(record.status) === 'Locked' || Number(record.status) === 2 ? 'red' : 'green'">{{ sessionLabel(record.status) }}</a-tag></template>
            </template>
          </a-table>
        </a-tab-pane>
        <a-tab-pane key="results" tab="Kết quả">
          <a-table :data-source="results" :columns="resultColumns" :loading="loading" row-key="id" size="small" :pagination="{ pageSize: 8 }">
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'resultStatus'"><a-tag :color="resultColor(record.resultStatus)">{{ resultLabel(record.resultStatus) }}</a-tag></template>
            </template>
          </a-table>
        </a-tab-pane>
      </a-tabs>
    </section>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import { classApi } from '@/api/classApi'
import { enrollmentApi } from '@/api/enrollmentApi'
import { scheduleApi } from '@/api/scheduleApi'
import { attendanceApi } from '@/api/attendanceApi'
import { resultApi } from '@/api/resultApi'
import { formatDate, formatTime, formatScore } from '@/lib/formatters'

const props = defineProps({ classId: { type: String, required: true } })
const loading = ref(false)
const activeTab = ref('students')
const classInfo = ref(null)
const enrollments = ref([])
const schedules = ref([])
const sessions = ref([])
const results = ref([])

const studentColumns = [{ title: 'Học viên', key: 'student' }, { title: 'Khóa học', dataIndex: 'courseNameSnapshot', key: 'courseNameSnapshot' }, { title: 'Ngày ghi danh', dataIndex: 'enrolledAt', key: 'enrolledAt', customRender: ({ text }) => formatDate(text) }, { title: 'Trạng thái', key: 'status' }]
const scheduleColumns = [{ title: 'Buổi', dataIndex: 'sessionNumber', key: 'sessionNumber', width: 90 }, { title: 'Thời gian', key: 'time' }, { title: 'Phòng', dataIndex: 'room', key: 'room' }, { title: 'Chủ đề', dataIndex: 'topic', key: 'topic' }]
const sessionColumns = [{ title: 'Buổi', dataIndex: 'sessionNumber', key: 'sessionNumber' }, { title: 'Ngày', dataIndex: 'attendanceDate', key: 'attendanceDate', customRender: ({ text }) => formatDate(text) }, { title: 'Chủ đề', dataIndex: 'topic', key: 'topic' }, { title: 'Trạng thái', key: 'status' }]
const resultColumns = [{ title: 'Học viên', dataIndex: 'studentNameSnapshot', key: 'studentNameSnapshot' }, { title: 'Giữa kỳ', dataIndex: 'midtermScore', key: 'midtermScore', customRender: ({ text }) => formatScore(text) }, { title: 'Cuối kỳ', dataIndex: 'finalScore', key: 'finalScore', customRender: ({ text }) => formatScore(text) }, { title: 'TB', dataIndex: 'averageScore', key: 'averageScore', customRender: ({ text }) => formatScore(text) }, { title: 'Kết quả', key: 'resultStatus' }]

function dayValue(value) { return ({ Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3, Thursday: 4, Friday: 5, Saturday: 6 }[value] ?? Number(value) ?? 0) }
function shiftValue(value) { return ({ Morning: 0, Afternoon: 1, Evening: 2 }[value] ?? Number(value) ?? 0) }
function dayLabel(value) { return ['Chủ nhật', 'Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7'][dayValue(value)] || value }
function shiftLabel(value) { return ['Sáng', 'Chiều', 'Tối'][shiftValue(value)] || value }
function enrollmentLabel(value) { return ({ Pending: 'Chờ xác nhận', Confirmed: 'Đã xác nhận', Studying: 'Đang học', Completed: 'Hoàn thành', Cancelled: 'Đã hủy', 1: 'Chờ xác nhận', 2: 'Đã xác nhận', 3: 'Đang học', 4: 'Hoàn thành', 5: 'Đã hủy' }[value] || value) }
function enrollmentColor(value) { return ({ Pending: 'orange', Confirmed: 'blue', Studying: 'purple', Completed: 'green', Cancelled: 'red', 1: 'orange', 2: 'blue', 3: 'purple', 4: 'green', 5: 'red' }[value] || 'blue') }
function sessionLabel(value) { return ({ Open: 'Đang mở', Locked: 'Đã khóa', 1: 'Đang mở', 2: 'Đã khóa' }[value] || value) }
function resultLabel(value) { return ({ InProgress: 'Đang học', Passed: 'Đạt', Failed: 'Không đạt', 1: 'Đang học', 2: 'Đạt', 3: 'Không đạt' }[value] || value) }
function resultColor(value) { return ({ InProgress: 'blue', Passed: 'green', Failed: 'red', 1: 'blue', 2: 'green', 3: 'red' }[value] || 'blue') }

async function loadData() {
  loading.value = true
  try {
    const [cls, en, sch, ses, res] = await Promise.all([
      classApi.getById(props.classId),
      enrollmentApi.getByClass(props.classId),
      scheduleApi.getByClass(props.classId),
      attendanceApi.getSessionsByClass(props.classId),
      resultApi.getByClass(props.classId)
    ])
    classInfo.value = cls
    enrollments.value = en || []
    schedules.value = sch || []
    sessions.value = ses || []
    results.value = res || []
  } finally {
    loading.value = false
  }
}
onMounted(loadData)
</script>

<style scoped>
.teacher-info-card,.teacher-panel { background: var(--admin-surface); border: 1px solid var(--admin-border); border-radius: 18px; box-shadow: var(--admin-shadow-sm); }
.teacher-info-card { padding: 16px; }
.teacher-info-card span { display: block; font-size: 11px; color: var(--admin-text-muted); margin-bottom: 5px; }
.teacher-info-card strong { color: var(--admin-text); font-size: 15px; }
.teacher-panel { padding: 16px; }
</style>
