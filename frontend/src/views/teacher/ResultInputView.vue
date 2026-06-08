<template>
  <div class="space-y-6">
    <PageHeader title="Nhập kết quả học tập" subtitle="Cập nhật điểm giữa kỳ, cuối kỳ và phản hồi cho học viên trong lớp phụ trách.">
      <template #actions>
        <a-button :loading="loading" @click="loadBaseData">Làm mới</a-button>
        <a-button type="primary" :disabled="!rows.length" :loading="saving" @click="saveAll">Lưu thay đổi</a-button>
      </template>
    </PageHeader>

    <section class="teacher-panel">
      <a-select v-model:value="selectedClassId" placeholder="Chọn lớp" class="w-full max-w-md" @change="loadClassData">
        <a-select-option v-for="cls in classes" :key="cls.id" :value="cls.id">{{ cls.className }}</a-select-option>
      </a-select>
    </section>

    <section class="teacher-panel">
      <a-table
        :data-source="rows"
        :columns="columns"
        :loading="loading"
        row-key="studentId"
        size="small"
        :pagination="{ pageSize: 10, showSizeChanger: false }"
        :scroll="{ x: 980 }"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'student'">
            <p class="font-semibold text-base-primary">{{ record.studentNameSnapshot }}</p>
            <p class="text-xs text-base-secondary">{{ record.courseNameSnapshot }}</p>
          </template>
          <template v-else-if="column.key === 'midtermScore'">
            <a-input-number v-model:value="record.midtermScore" :min="0" :max="10" :step="0.25" class="w-[110px]" />
          </template>
          <template v-else-if="column.key === 'finalScore'">
            <a-input-number v-model:value="record.finalScore" :min="0" :max="10" :step="0.25" class="w-[110px]" />
          </template>
          <template v-else-if="column.key === 'attendancePercent'">
            <a-input-number v-model:value="record.attendancePercent" :min="0" :max="100" :step="1" class="w-[110px]" />
          </template>
          <template v-else-if="column.key === 'average'">
            <strong>{{ average(record).toFixed(2) }}</strong>
          </template>
          <template v-else-if="column.key === 'result'">
            <a-tag :color="passed(record) ? 'green' : 'red'">{{ passed(record) ? 'Đạt' : 'Không đạt' }}</a-tag>
          </template>
          <template v-else-if="column.key === 'feedback'">
            <a-input v-model:value="record.feedback" placeholder="Nhận xét" />
          </template>
        </template>
      </a-table>
    </section>
  </div>
</template>

<script setup>
import { onMounted, ref, watch } from 'vue'
import { message } from 'ant-design-vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import { classApi } from '@/api/classApi'
import { enrollmentApi } from '@/api/enrollmentApi'
import { resultApi } from '@/api/resultApi'
import { useAuthStore } from '@/stores/auth'

const props = defineProps({ classId: { type: String, default: '' } })
const auth = useAuthStore()
const loading = ref(false)
const saving = ref(false)
const classes = ref([])
const selectedClassId = ref(props.classId || undefined)
const rows = ref([])

const columns = [
  { title: 'Học viên', key: 'student', width: 260 },
  { title: 'Giữa kỳ', key: 'midtermScore', width: 130 },
  { title: 'Cuối kỳ', key: 'finalScore', width: 130 },
  { title: 'Chuyên cần %', key: 'attendancePercent', width: 140 },
  { title: 'Trung bình', key: 'average', width: 110 },
  { title: 'Kết quả', key: 'result', width: 120 },
  { title: 'Phản hồi', key: 'feedback', width: 260 }
]

function activeEnrollments(items) {
  return items.filter(x => ['Confirmed', 'Studying', 2, 3, '2', '3'].includes(x.status))
}
function average(row) {
  return Number(row.midtermScore || 0) * 0.4 + Number(row.finalScore || 0) * 0.6
}
function passed(row) {
  return average(row) >= 5 && Number(row.attendancePercent || 0) >= 70
}
async function loadBaseData() {
  if (!auth.user?.referenceId) return
  loading.value = true
  try {
    classes.value = await classApi.getByTeacher(auth.user.referenceId)
    if (!selectedClassId.value && classes.value.length) selectedClassId.value = classes.value[0].id
    await loadClassData()
  } finally {
    loading.value = false
  }
}
async function loadClassData() {
  if (!selectedClassId.value) return
  loading.value = true
  try {
    const selectedClass = classes.value.find(x => x.id === selectedClassId.value) || await classApi.getById(selectedClassId.value)
    const [enrollments, results] = await Promise.all([
      enrollmentApi.getByClass(selectedClassId.value),
      resultApi.getByClass(selectedClassId.value)
    ])
    const resultByStudent = new Map((results || []).map(x => [x.studentId, x]))
    rows.value = activeEnrollments(enrollments || []).map(en => {
      const existing = resultByStudent.get(en.studentId)
      return {
        id: existing?.id || null,
        studentId: en.studentId,
        studentNameSnapshot: en.studentNameSnapshot,
        courseId: en.courseId || selectedClass.courseId,
        courseNameSnapshot: en.courseNameSnapshot || selectedClass.courseNameSnapshot,
        classId: en.classId || selectedClass.id,
        classNameSnapshot: en.classNameSnapshot || selectedClass.className,
        midtermScore: Number(existing?.midtermScore || 0),
        finalScore: Number(existing?.finalScore || 0),
        attendancePercent: Number(existing?.attendancePercent || 80),
        feedback: existing?.feedback || ''
      }
    })
  } finally {
    loading.value = false
  }
}
async function saveAll() {
  saving.value = true
  try {
    for (const row of rows.value) {
      const payload = {
        studentId: row.studentId,
        courseId: row.courseId,
        courseNameSnapshot: row.courseNameSnapshot,
        classId: row.classId,
        classNameSnapshot: row.classNameSnapshot,
        midtermScore: Number(row.midtermScore || 0),
        finalScore: Number(row.finalScore || 0),
        attendancePercent: Number(row.attendancePercent || 0),
        feedback: row.feedback,
        evaluatedByTeacherId: auth.user.referenceId,
        evaluatedByTeacherName: auth.user.fullName || auth.user.username
      }
      const saved = row.id ? await resultApi.update(row.id, payload) : await resultApi.create(payload)
      row.id = saved.id
    }
    message.success('Đã lưu kết quả học tập')
    await loadClassData()
  } catch (error) {
    message.error(error.message || 'Không lưu được kết quả')
  } finally {
    saving.value = false
  }
}

watch(() => props.classId, (value) => { if (value) selectedClassId.value = value })
onMounted(loadBaseData)
</script>

<style scoped>
.teacher-panel { background: var(--admin-surface); border: 1px solid var(--admin-border); border-radius: 18px; padding: 16px; box-shadow: var(--admin-shadow-sm); }
</style>
