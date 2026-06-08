<template>
  <div class="space-y-6">
    <PageHeader title="Lớp phụ trách" subtitle="Danh sách lớp học đang gắn với tài khoản giảng viên.">
      <template #actions>
        <a-button :loading="loading" @click="loadData">Làm mới</a-button>
      </template>
    </PageHeader>

    <section class="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-4">
      <article v-for="cls in classes" :key="cls.id" class="teacher-class-card">
        <div class="flex items-start justify-between gap-3">
          <div class="min-w-0">
            <a-tag color="blue">{{ cls.classCode }}</a-tag>
            <h2 class="mt-3 text-base font-extrabold text-base-primary truncate">{{ cls.className }}</h2>
            <p class="text-xs text-base-secondary mt-1 truncate">{{ cls.courseNameSnapshot }}</p>
          </div>
          <a-tag :color="statusColor(cls.status)">{{ statusLabel(cls.status) }}</a-tag>
        </div>

        <div class="grid grid-cols-2 gap-3 mt-5">
          <div class="teacher-mini-box">
            <span>Phòng</span>
            <strong>{{ cls.room || '-' }}</strong>
          </div>
          <div class="teacher-mini-box">
            <span>Hình thức</span>
            <strong>{{ modeLabel(cls.learningMode) }}</strong>
          </div>
          <div class="teacher-mini-box col-span-2">
            <span>Thời gian</span>
            <strong>{{ formatDate(cls.startDate) }} - {{ formatDate(cls.endDate) }}</strong>
          </div>
        </div>

        <div class="mt-5">
          <div class="flex justify-between text-xs text-base-secondary mb-1">
            <span>Sĩ số</span>
            <span>{{ cls.currentStudents }}/{{ cls.maxStudents }} · {{ capacityPercent(cls) }}%</span>
          </div>
          <a-progress :percent="capacityPercent(cls)" :show-info="false" />
        </div>

        <div class="grid grid-cols-3 gap-2 mt-5">
          <router-link :to="`/teacher/classes/${cls.id}`"><a-button block>Chi tiết</a-button></router-link>
          <router-link :to="`/teacher/classes/${cls.id}/attendance`"><a-button block>Điểm danh</a-button></router-link>
          <router-link :to="`/teacher/classes/${cls.id}/results`"><a-button block>Nhập điểm</a-button></router-link>
        </div>
      </article>
    </section>

    <section class="teacher-panel">
      <a-table
        :data-source="classes"
        :columns="columns"
        :loading="loading"
        row-key="id"
        size="small"
        :pagination="{ pageSize: 8, showSizeChanger: false }"
        :scroll="{ x: 900 }"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'className'">
            <div>
              <p class="font-semibold text-base-primary">{{ record.className }}</p>
              <p class="text-xs text-base-secondary">{{ record.courseNameSnapshot }}</p>
            </div>
          </template>
          <template v-else-if="column.key === 'status'">
            <a-tag :color="statusColor(record.status)">{{ statusLabel(record.status) }}</a-tag>
          </template>
          <template v-else-if="column.key === 'capacity'">
            {{ record.currentStudents }}/{{ record.maxStudents }}
          </template>
          <template v-else-if="column.key === 'actions'">
            <router-link :to="`/teacher/classes/${record.id}`"><a-button size="small">Mở lớp</a-button></router-link>
          </template>
        </template>
      </a-table>
    </section>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import { classApi } from '@/api/classApi'
import { useAuthStore } from '@/stores/auth'
import { formatDate } from '@/lib/formatters'

const auth = useAuthStore()
const loading = ref(false)
const classes = ref([])

const columns = [
  { title: 'Lớp học', key: 'className', width: 280 },
  { title: 'Phòng', dataIndex: 'room', key: 'room', width: 100 },
  { title: 'Sĩ số', key: 'capacity', width: 120 },
  { title: 'Trạng thái', key: 'status', width: 130 },
  { title: 'Thao tác', key: 'actions', width: 120, fixed: 'right' }
]

function capacityPercent(cls) {
  if (!cls.maxStudents) return 0
  return Math.min(100, Math.round((Number(cls.currentStudents || 0) / Number(cls.maxStudents)) * 100))
}
function statusLabel(value) {
  const map = { Open: 'Đang mở', Full: 'Đã đầy', InProgress: 'Đang học', Completed: 'Hoàn thành', Cancelled: 'Đã hủy', 0: 'Đang mở', 1: 'Đã đầy', 2: 'Đang học', 3: 'Hoàn thành', 4: 'Đã hủy' }
  return map[value] || value
}
function statusColor(value) {
  const map = { Open: 'blue', Full: 'orange', InProgress: 'purple', Completed: 'green', Cancelled: 'red', 0: 'blue', 1: 'orange', 2: 'purple', 3: 'green', 4: 'red' }
  return map[value] || 'blue'
}
function modeLabel(value) {
  const map = { Offline: 'Offline', Online: 'Online', Hybrid: 'Hybrid', 0: 'Offline', 1: 'Online', 2: 'Hybrid' }
  return map[value] || value
}
async function loadData() {
  if (!auth.user?.referenceId) return
  loading.value = true
  try {
    classes.value = await classApi.getByTeacher(auth.user.referenceId)
  } finally {
    loading.value = false
  }
}
onMounted(loadData)
</script>

<style scoped>
.teacher-class-card,.teacher-panel { background: var(--admin-surface); border: 1px solid var(--admin-border); border-radius: 18px; box-shadow: var(--admin-shadow-sm); }
.teacher-class-card { padding: 18px; transition: transform .2s ease, box-shadow .2s ease; }
.teacher-class-card:hover { transform: translateY(-2px); box-shadow: var(--admin-shadow-md); }
.teacher-panel { padding: 16px; overflow: hidden; }
.teacher-mini-box { background: var(--admin-surface-2); border: 1px solid var(--admin-border); border-radius: 14px; padding: 12px; }
.teacher-mini-box span { display: block; color: var(--admin-text-muted); font-size: 11px; margin-bottom: 4px; }
.teacher-mini-box strong { color: var(--admin-text); font-size: 13px; }
</style>
