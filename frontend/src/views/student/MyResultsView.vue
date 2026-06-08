<template>
  <div class="space-y-6">
    <PageHeader title="Bảng điểm" subtitle="Điểm giữa kỳ, cuối kỳ, chuyên cần và kết quả học tập theo từng lớp." />

    <LoadingSpinner v-if="loading" size="lg" class="py-20" />
    <div v-else-if="error" class="student-empty">{{ error }}</div>
    <div v-else-if="results.length === 0" class="student-card student-empty">Chưa có điểm học tập.</div>
    <div v-else class="grid grid-cols-1 xl:grid-cols-2 gap-5">
      <article v-for="result in results" :key="result.id" class="student-card">
        <div class="flex items-start justify-between gap-4">
          <div>
            <p class="text-xs font-extrabold uppercase tracking-wider text-violet-600">Kết quả học tập</p>
            <h2 class="text-xl font-black text-base-primary mt-1">{{ result.courseNameSnapshot }}</h2>
            <p class="text-sm text-base-secondary mt-2">{{ result.classNameSnapshot }}</p>
          </div>
          <span :class="statusClass(result.resultStatus)">{{ statusText(result.resultStatus) }}</span>
        </div>

        <div class="grid grid-cols-2 sm:grid-cols-4 gap-3 mt-6">
          <ScoreBox label="Giữa kỳ" :value="formatScore(result.midtermScore)" />
          <ScoreBox label="Cuối kỳ" :value="formatScore(result.finalScore)" />
          <ScoreBox label="Trung bình" :value="formatScore(result.averageScore)" highlight />
          <ScoreBox label="Chuyên cần" :value="formatPercent(result.attendancePercent, 0)" />
        </div>

        <div class="mt-5 p-4 rounded-2xl bg-slate-50 dark:bg-slate-900/40 border border-slate-200/70 dark:border-slate-700/60">
          <p class="text-xs font-bold text-base-muted uppercase tracking-wider">Nhận xét</p>
          <p class="text-sm text-base-secondary mt-1">{{ result.feedback || 'Giảng viên chưa nhập nhận xét.' }}</p>
        </div>
      </article>
    </div>
  </div>
</template>

<script setup>
import { defineComponent, h, onMounted, ref } from 'vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { studentApi } from '@/api/studentApi'
import { useAuthStore } from '@/stores/auth'
import { formatPercent, formatScore } from '@/lib/formatters'

const ScoreBox = defineComponent({
  props: { label: String, value: String, highlight: Boolean },
  setup(props) {
    return () => h('div', { class: ['student-mini-stat', props.highlight ? 'ring-2 ring-blue-500/20' : ''] }, [
      h('span', props.label),
      h('strong', props.value),
    ])
  },
})

const auth = useAuthStore()
const results = ref([])
const loading = ref(true)
const error = ref('')

onMounted(loadData)

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    results.value = auth.user?.referenceId ? await studentApi.getMyResults(auth.user.referenceId) : []
  } catch (err) {
    error.value = err.message || 'Không tải được bảng điểm.'
  } finally {
    loading.value = false
  }
}

function statusText(status) {
  return ({ Passed: 'Đạt', Failed: 'Chưa đạt', InProgress: 'Đang học' })[status] || status
}
function statusClass(status) {
  const base = 'student-status-pill '
  if (status === 'Passed') return base + 'is-green'
  if (status === 'Failed') return base + 'is-red'
  return base + 'is-blue'
}
</script>
