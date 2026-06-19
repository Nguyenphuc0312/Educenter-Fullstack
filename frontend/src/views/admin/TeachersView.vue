<template>
  <AdminResourceView
    title="Quản lý giảng viên"
    subtitle="Hồ sơ giảng viên, chuyên môn, kinh nghiệm và trạng thái hoạt động."
    :api="teacherApi"
    :columns="columns"
    :fields="fields"
    :searchable-fields="['fullName', 'email', 'phone', 'specialization']"
    :status-options="statusOptions"
    :form-groups="formGroups"
    :filter-fn="customFilter"
    @reset="resetCustomFilters"
  >
    <!-- Custom filters slot -->
    <template #filters>
      <!-- Specialization Filter -->
      <a-select
        v-model:value="filterSpecialization"
        placeholder="Chuyên môn"
        allow-clear
        size="small"
        class="w-40"
      >
        <a-select-option value="Frontend">Frontend</a-select-option>
        <a-select-option value="Backend">Backend</a-select-option>
        <a-select-option value="Fullstack">Fullstack</a-select-option>
        <a-select-option value="DevOps">DevOps</a-select-option>
      </a-select>

      <!-- Status Filter -->
      <a-select
        v-model:value="filterStatusValue"
        placeholder="Trạng thái"
        allow-clear
        size="small"
        class="w-36"
      >
        <a-select-option :value="0">Hoạt động</a-select-option>
        <a-select-option :value="1">Không hoạt động</a-select-option>
      </a-select>

      <!-- Experience Years Range Filter -->
      <a-select
        v-model:value="filterExpRange"
        placeholder="Kinh nghiệm"
        allow-clear
        size="small"
        class="w-36"
      >
        <a-select-option value="under_3">Dưới 3 năm</a-select-option>
        <a-select-option value="3_5">3 - 5 năm</a-select-option>
        <a-select-option value="above_5">Trên 5 năm</a-select-option>
      </a-select>
    </template>

    <!-- Custom cells -->
    <template #bodyCell="{ column, record }">
      <!-- Teacher cell: avatar + name + email -->
      <template v-if="column.key === 'fullName'">
        <div class="flex items-center gap-2.5 min-w-0">
          <!-- Avatar -->
          <div
            v-if="record.avatarUrl"
            class="w-9 h-9 rounded-full overflow-hidden flex-shrink-0 bg-slate-100"
          >
            <img :src="record.avatarUrl" :alt="record.fullName" class="w-full h-full object-cover" />
          </div>
          <div
            v-else
            class="w-9 h-9 rounded-full flex items-center justify-center text-[11px] font-bold text-white flex-shrink-0"
            :style="{ background: avatarColor(record.fullName) }"
          >
            {{ initials(record.fullName) }}
          </div>

          <div class="min-w-0">
            <div class="text-xs font-semibold text-base-primary truncate max-w-[160px]" :title="record.fullName">
              {{ record.fullName || '—' }}
            </div>
            <div class="text-[10px] text-base-muted truncate max-w-[160px]" :title="record.email">
              {{ record.email || '—' }}
            </div>
          </div>
        </div>
      </template>

      <!-- Phone -->
      <template v-else-if="column.key === 'phone'">
        <span class="text-xs text-base-secondary whitespace-nowrap">
          {{ record.phone || '—' }}
        </span>
      </template>

      <!-- Experience -->
      <template v-else-if="column.key === 'experienceYears'">
        <span class="text-xs text-base-secondary whitespace-nowrap">
          {{ record.experienceYears || 0 }} năm
        </span>
      </template>

      <!-- Rating -->
      <template v-else-if="column.key === 'rating'">
        <div v-if="record.rating != null" class="flex items-center gap-1">
          <span class="text-xs font-bold text-amber-500">★</span>
          <span class="text-xs font-semibold text-base-primary">{{ Number(record.rating).toFixed(1) }}</span>
        </div>
        <span v-else class="text-base-muted text-xs">—</span>
      </template>
    </template>
  </AdminResourceView>
</template>

<script setup>
import { ref } from 'vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import { teacherApi } from '@/api/teacherApi'
import { TEACHER_STATUS, toOptions } from '@/lib/constants'

const statusOptions = toOptions(TEACHER_STATUS, { 0: 'green', 1: 'default' })
const phoneRules = [
  {
    pattern: /^0\d{9}$/,
    message: 'Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0'
  }
]

// Custom filter states
const filterSpecialization = ref(undefined)
const filterStatusValue = ref(undefined)
const filterExpRange = ref(undefined)

const columns = [
  { title: 'Giảng viên', key: 'fullName', width: 240 },
  { title: 'Điện thoại', key: 'phone', width: 130 },
  { title: 'Chuyên môn', dataIndex: 'specialization', key: 'specialization', width: 150 },
  { title: 'Kinh nghiệm', key: 'experienceYears', width: 120 },
  { title: 'Đánh giá', key: 'rating', width: 90 },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 130 },
]

const fields = [
  { name: 'fullName', label: 'Họ tên', required: true, default: '' },
  { name: 'email', label: 'Email', required: true, default: '' },
  { name: 'phone', label: 'Điện thoại', required: true, default: '', rules: phoneRules },
  { name: 'specialization', label: 'Chuyên môn', default: '' },
  { name: 'experienceYears', label: 'Số năm kinh nghiệm', type: 'number', default: 0 },
  { name: 'rating', label: 'Đánh giá', type: 'number', default: 5 },
  { name: 'avatarUrl', label: 'Ảnh đại diện URL', fullWidth: true, default: '' },
  { name: 'bio', label: 'Giới thiệu bản thân', type: 'textarea', fullWidth: true, default: '' },
  { name: 'status', label: 'Trạng thái', type: 'select', options: statusOptions, default: 0 },
]

// Sectioned form group configuration
const formGroups = [
  {
    title: 'Thông tin cá nhân',
    fields: ['fullName', 'email', 'phone', 'avatarUrl']
  },
  {
    title: 'Hồ sơ chuyên môn',
    fields: ['specialization', 'experienceYears', 'rating', 'bio']
  },
  {
    title: 'Trạng thái',
    fields: ['status']
  }
]

// Custom filtration handler
function customFilter(item) {
  const spec = item.specialization || ''
  const matchSpecialization = !filterSpecialization.value || spec.toLowerCase().includes(filterSpecialization.value.toLowerCase())
  const matchStatus = filterStatusValue.value === undefined || Number(item.status) === Number(filterStatusValue.value)

  let matchExp = true
  const exp = Number(item.experienceYears || 0)
  if (filterExpRange.value === 'under_3') matchExp = exp < 3
  else if (filterExpRange.value === '3_5') matchExp = exp >= 3 && exp <= 5
  else if (filterExpRange.value === 'above_5') matchExp = exp > 5

  return matchSpecialization && matchExp && matchStatus
}

function resetCustomFilters() {
  filterSpecialization.value = undefined
  filterStatusValue.value = undefined
  filterExpRange.value = undefined
}

// Avatar helpers
function initials(name) {
  if (!name) return '?'
  const parts = String(name).trim().split(/\s+/)
  if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

const AVATAR_COLORS = [
  '#4f46e5', '#7c3aed', '#db2777', '#0891b2',
  '#059669', '#d97706', '#dc2626', '#65a30d',
]

function avatarColor(name) {
  if (!name) return AVATAR_COLORS[0]
  let hash = 0
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length]
}
</script>
