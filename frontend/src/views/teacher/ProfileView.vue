<template>
  <div class="space-y-6">
    <PageHeader title="Hồ sơ giảng viên" subtitle="Thông tin tài khoản và hồ sơ chuyên môn đang liên kết với hệ thống." />

    <section class="grid grid-cols-1 lg:grid-cols-3 gap-4">
      <div class="teacher-profile-card lg:col-span-1">
        <div class="w-20 h-20 rounded-3xl gradient-primary text-white grid place-items-center text-2xl font-extrabold">
          {{ initials }}
        </div>
        <h2 class="mt-4 text-lg font-extrabold text-base-primary">{{ teacher?.fullName || auth.user?.fullName || auth.user?.username }}</h2>
        <p class="text-sm text-base-secondary">{{ teacher?.specialization || 'Giảng viên EduCenter' }}</p>
        <a-rate class="mt-4" :value="Number(teacher?.rating || 0)" disabled allow-half />
      </div>

      <div class="teacher-profile-card lg:col-span-2">
        <h3 class="font-extrabold text-base-primary mb-4">Thông tin chi tiết</h3>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
          <div class="teacher-info-box"><span>Email</span><strong>{{ teacher?.email || auth.user?.email || '-' }}</strong></div>
          <div class="teacher-info-box"><span>Điện thoại</span><strong>{{ teacher?.phone || auth.user?.phone || '-' }}</strong></div>
          <div class="teacher-info-box"><span>Kinh nghiệm</span><strong>{{ teacher?.experienceYears ?? '-' }} năm</strong></div>
          <div class="teacher-info-box"><span>Trạng thái</span><strong>{{ statusLabel(teacher?.status) }}</strong></div>
          <div class="teacher-info-box md:col-span-2"><span>Giới thiệu</span><strong>{{ teacher?.bio || 'Chưa có mô tả chuyên môn.' }}</strong></div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import { teacherApi } from '@/api/teacherApi'
import { useAuthStore } from '@/stores/auth'
import { getInitials } from '@/lib/formatters'

const auth = useAuthStore()
const teacher = ref(null)
const initials = computed(() => getInitials(teacher.value?.fullName || auth.user?.fullName || auth.user?.username || 'GV'))

function statusLabel(value) {
  return ({ Active: 'Hoạt động', Inactive: 'Ngừng hoạt động', 0: 'Hoạt động', 1: 'Ngừng hoạt động' }[value] || value || '-')
}
onMounted(async () => {
  if (auth.user?.referenceId) {
    teacher.value = await teacherApi.getById(auth.user.referenceId).catch(() => null)
  }
})
</script>

<style scoped>
.teacher-profile-card { background: var(--admin-surface); border: 1px solid var(--admin-border); border-radius: 20px; padding: 22px; box-shadow: var(--admin-shadow-sm); }
.teacher-info-box { background: var(--admin-surface-2); border: 1px solid var(--admin-border); border-radius: 14px; padding: 13px; }
.teacher-info-box span { display: block; color: var(--admin-text-muted); font-size: 11px; margin-bottom: 5px; }
.teacher-info-box strong { color: var(--admin-text); font-size: 13px; line-height: 1.5; }
</style>
