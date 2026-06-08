<template>
  <div class="space-y-6">
    <PageHeader title="Hồ sơ cá nhân" subtitle="Thông tin tài khoản và hồ sơ học viên đang liên kết với hệ thống." />

    <LoadingSpinner v-if="loading" size="lg" class="py-20" />
    <div v-else class="grid grid-cols-1 xl:grid-cols-3 gap-6">
      <section class="student-card xl:col-span-1">
        <div class="flex flex-col items-center text-center">
          <div class="w-24 h-24 rounded-3xl gradient-primary text-white flex items-center justify-center text-3xl font-black shadow-lg">
            {{ initials }}
          </div>
          <h2 class="mt-5 text-2xl font-black text-base-primary">{{ student?.fullName || auth.user?.fullName || 'Học viên' }}</h2>
          <p class="text-base-secondary">{{ auth.user?.username }}</p>
          <span :class="statusClass(student?.status)" class="mt-4">{{ statusText(student?.status || 'Active') }}</span>
        </div>
      </section>

      <section class="student-card xl:col-span-2">
        <div class="student-section-head">
          <div>
            <h2>Thông tin học viên</h2>
            <p>Dữ liệu hồ sơ được lấy từ StudentAttendanceService.</p>
          </div>
        </div>
        <div v-if="error" class="student-empty">{{ error }}</div>
        <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-3">
          <InfoItem label="Mã học viên" :value="student?.studentCode" />
          <InfoItem label="Email" :value="student?.email || auth.user?.email" />
          <InfoItem label="Số điện thoại" :value="student?.phone" />
          <InfoItem label="Ngày sinh" :value="formatDate(student?.dateOfBirth)" />
          <InfoItem label="Giới tính" :value="genderText(student?.gender)" />
          <InfoItem label="Reference ID" :value="auth.user?.referenceId" />
          <InfoItem class="md:col-span-2" label="Địa chỉ" :value="student?.address" />
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import { computed, defineComponent, h, onMounted, ref } from 'vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { useAuthStore } from '@/stores/auth'
import { studentApi } from '@/api/studentApi'
import { formatDate, getInitials } from '@/lib/formatters'

const InfoItem = defineComponent({
  props: { label: String, value: [String, Number] },
  setup(props) {
    return () => h('div', { class: 'student-profile-row !items-start !justify-start !block' }, [
      h('span', { class: 'block text-xs font-extrabold uppercase tracking-wider text-base-muted' }, props.label),
      h('strong', { class: 'block mt-2 text-base-primary break-words' }, props.value || '-'),
    ])
  },
})

const auth = useAuthStore()
const student = ref(null)
const loading = ref(true)
const error = ref('')
const initials = computed(() => getInitials(student.value?.fullName || auth.user?.fullName || auth.user?.username || 'HV'))

onMounted(loadData)

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    student.value = auth.user?.referenceId ? await studentApi.getById(auth.user.referenceId) : null
  } catch (err) {
    error.value = err.message || 'Không tải được hồ sơ học viên.'
  } finally {
    loading.value = false
  }
}

function statusText(status) {
  return ({ Active: 'Đang hoạt động', Inactive: 'Ngừng hoạt động', Suspended: 'Tạm khóa' })[status] || status
}
function statusClass(status) {
  const base = 'student-status-pill '
  if (status === 'Active') return base + 'is-green'
  if (status === 'Suspended') return base + 'is-red'
  return base + 'is-muted'
}
function genderText(gender) {
  return ({ Male: 'Nam', Female: 'Nữ', Other: 'Khác' })[gender] || gender || '-'
}
</script>
