<template>
  <AdminResourceView
    title="Quản lý học viên"
    subtitle="Thông tin học viên, trạng thái học tập và hồ sơ liên hệ."
    :api="studentApi"
    :columns="columns"
    :fields="fields"
    :searchable-fields="['studentCode', 'fullName', 'email', 'phone']"
    :status-options="statusOptions"
    :form-groups="formGroups"
    :filter-fn="customFilter"
    @reset="resetCustomFilters"
  >
    <!-- Custom filters slot -->
    <template #filters>
      <!-- Gender Filter -->
      <a-select
        v-model:value="filterGender"
        placeholder="Giới tính"
        allow-clear
        size="small"
        class="w-28"
      >
        <a-select-option :value="1">Nam</a-select-option>
        <a-select-option :value="2">Nữ</a-select-option>
        <a-select-option :value="3">Khác</a-select-option>
      </a-select>

      <!-- Status Filter -->
      <a-select
        v-model:value="filterStatus"
        placeholder="Trạng thái"
        allow-clear
        size="small"
        class="w-36"
      >
        <a-select-option :value="1">Hoạt động</a-select-option>
        <a-select-option :value="2">Không hoạt động</a-select-option>
        <a-select-option :value="3">Tạm dừng</a-select-option>
      </a-select>

      <!-- Age Range Filter -->
      <a-select
        v-model:value="filterAgeRange"
        placeholder="Độ tuổi"
        allow-clear
        size="small"
        class="w-36"
      >
        <a-select-option value="under_18">Dưới 18 tuổi</a-select-option>
        <a-select-option value="18_25">18 - 25 tuổi</a-select-option>
        <a-select-option value="above_25">Trên 25 tuổi</a-select-option>
      </a-select>
    </template>

    <!-- Custom cells -->
    <template #bodyCell="{ column, record }">
      <!-- Avatar + Name cell -->
      <template v-if="column.key === 'fullName'">
        <div class="flex items-center gap-2.5 min-w-0">
          <!-- Avatar initials -->
          <div
            class="w-8 h-8 rounded-full flex items-center justify-center text-[11px] font-bold text-white flex-shrink-0"
            :style="{ background: avatarColor(record.fullName) }"
          >
            {{ initials(record.fullName) }}
          </div>
          <div class="min-w-0">
            <div class="text-xs font-semibold text-base-primary truncate max-w-[180px]" :title="record.fullName">
              {{ record.fullName || '—' }}
            </div>
            <div class="text-[10px] text-base-muted font-mono">
              {{ record.studentCode || '—' }}
            </div>
          </div>
        </div>
      </template>

      <!-- Contact cell -->
      <template v-else-if="column.key === 'email'">
        <div class="leading-tight">
          <div class="text-xs text-base-primary truncate max-w-[200px]" :title="record.email">
            {{ record.email || '—' }}
          </div>
          <div v-if="record.phone" class="text-[10px] text-base-muted">
            {{ record.phone }}
          </div>
        </div>
      </template>

      <!-- Birth date + Gender -->
      <template v-else-if="column.key === 'dateOfBirth'">
        <div class="leading-tight">
          <span class="text-xs text-base-secondary whitespace-nowrap">
            {{ formatDate(record.dateOfBirth) }}
          </span>
          <div v-if="record.gender" class="text-[10px] text-base-muted">
            {{ genderLabel(record.gender) }}
          </div>
        </div>
      </template>

      <!-- Age (computed, not stored) -->
      <template v-else-if="column.key === '__age'">
        <span class="text-xs text-base-secondary">
          {{ computeAge(record.dateOfBirth) }}
        </span>
      </template>
    </template>
  </AdminResourceView>
</template>

<script setup>
import { ref } from 'vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import { studentApi } from '@/api/studentApi'
import { STUDENT_STATUS, GENDER, toOptions } from '@/lib/constants'

const statusOptions = toOptions(STUDENT_STATUS, { 1: 'green', 2: 'default', 3: 'red' })
const genderOptions = toOptions(GENDER)
const phoneRules = [
  {
    pattern: /^0\d{9}$/,
    message: 'Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0'
  }
]

// Custom filter states
const filterGender = ref(undefined)
const filterStatus = ref(undefined)
const filterAgeRange = ref(undefined)

const columns = [
  { title: 'Họ tên', key: 'fullName', width: 240 },
  { title: 'Liên hệ', key: 'email', width: 220 },
  { title: 'Ngày sinh', key: 'dateOfBirth', width: 140 },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 130 },
  { title: 'Ngày tạo', dataIndex: 'createdAt', key: 'createdAt', type: 'date', width: 120 },
]

const fields = [
  { name: 'studentCode', label: 'Mã học viên', required: true, editOnly: true, default: '' },
  { name: 'fullName', label: 'Họ tên', required: true, default: '' },
  { name: 'email', label: 'Email', required: true, default: '' },
  { name: 'phone', label: 'Điện thoại', required: true, default: '', rules: phoneRules },
  { name: 'dateOfBirth', label: 'Ngày sinh', type: 'date', default: '' },
  { name: 'gender', label: 'Giới tính', type: 'select', options: genderOptions, default: 0 },
  { name: 'address', label: 'Địa chỉ', fullWidth: true, default: '' },
  { name: 'avatarUrl', label: 'Ảnh đại diện', fullWidth: true, default: '' },
  { name: 'status', label: 'Trạng thái', type: 'select', options: statusOptions, default: 1 },
]

// Sectioned form group configuration
const formGroups = [
  {
    title: 'Hồ sơ cá nhân',
    fields: ['studentCode', 'fullName', 'dateOfBirth', 'gender', 'avatarUrl']
  },
  {
    title: 'Thông tin liên hệ & Địa chỉ',
    fields: ['email', 'phone', 'address']
  },
  {
    title: 'Trạng thái',
    fields: ['status']
  }
]

// Custom filtration handler
function customFilter(item) {
  const matchGender = filterGender.value === undefined || Number(item.gender) === Number(filterGender.value)
  const matchStatus = filterStatus.value === undefined || Number(item.status) === Number(filterStatus.value)

  let matchAge = true
  if (filterAgeRange.value && item.dateOfBirth) {
    const dob = new Date(item.dateOfBirth)
    const today = new Date()
    let age = today.getFullYear() - dob.getFullYear()
    const m = today.getMonth() - dob.getMonth()
    if (m < 0 || (m === 0 && today.getDate() < dob.getDate())) age--

    if (filterAgeRange.value === 'under_18') matchAge = age < 18
    else if (filterAgeRange.value === '18_25') matchAge = age >= 18 && age <= 25
    else if (filterAgeRange.value === 'above_25') matchAge = age > 25
  }

  return matchGender && matchStatus && matchAge
}

function resetCustomFilters() {
  filterGender.value = undefined
  filterStatus.value = undefined
  filterAgeRange.value = undefined
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

function genderLabel(gender) {
  const labels = { 0: 'Không rõ', 1: 'Nam', 2: 'Nữ', 3: 'Khác' }
  return labels[gender] || '—'
}

function computeAge(dob) {
  if (!dob) return '—'
  const birth = new Date(dob)
  const today = new Date()
  let age = today.getFullYear() - birth.getFullYear()
  const m = today.getMonth() - birth.getMonth()
  if (m < 0 || (m === 0 && today.getDate() < birth.getDate())) age--
  return age > 0 ? `${age} tuổi` : '—'
}

function formatDate(value) {
  if (!value) return '—'
  return new Date(value).toLocaleDateString('vi-VN')
}
</script>
