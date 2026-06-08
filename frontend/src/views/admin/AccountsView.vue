<template>
  <div>
    <AdminResourceView
      title="Quản lý tài khoản"
      subtitle="Tài khoản đăng nhập cho admin, giảng viên và học viên."
      :api="accountApi"
      :columns="columns"
      :fields="fields"
      :searchable-fields="['username', 'fullName', 'email', 'phone']"
      :status-options="statusOptions"
      status-field="status"
      :form-groups="formGroups"
      :filter-fn="customFilter"
      @reset="resetCustomFilters"
    >
      <!-- Custom Filters -->
      <template #filters>
        <!-- Role Filter -->
        <a-select
          v-model:value="filterRole"
          placeholder="Vai trò"
          allow-clear
          size="small"
          class="w-32"
        >
          <a-select-option :value="1">Admin</a-select-option>
          <a-select-option :value="2">Giảng viên</a-select-option>
          <a-select-option :value="3">Học viên</a-select-option>
        </a-select>

        <!-- Status Filter -->
        <a-select
          v-model:value="filterStatusValue"
          placeholder="Trạng thái"
          allow-clear
          size="small"
          class="w-36"
        >
          <a-select-option :value="1">Hoạt động</a-select-option>
          <a-select-option :value="2">Bị khóa</a-select-option>
        </a-select>
      </template>

      <!-- Bulk actions -->
      <template #bulkActions="{ selectedRowKeys, refresh }">
        <a-button
          size="small"
          class="h-7 text-[11px] border border-amber-200 dark:border-amber-800 text-amber-600 dark:text-amber-400"
          :disabled="!selectedRowKeys.length"
          @click="triggerBulkLock(selectedRowKeys, refresh)"
        >
          <template #icon><LockOutlined /></template>
          Khóa tất cả
        </a-button>
        <a-button
          size="small"
          class="h-7 text-[11px] border border-blue-200 dark:border-blue-800 text-blue-600 dark:text-blue-400"
          :disabled="!selectedRowKeys.length"
          @click="triggerBulkUnlock(selectedRowKeys, refresh)"
        >
          <template #icon><UnlockOutlined /></template>
          Mở khóa tất cả
        </a-button>
      </template>

      <!-- Row actions -->
      <template #rowActions="{ record, refresh }">
        <a-menu-item
          v-if="record.status === 1"
          class="rounded-lg px-3 py-2 text-xs text-amber-600"
          @click="triggerLock(record.id, refresh)"
        >
          <LockOutlined /> Khóa tài khoản
        </a-menu-item>
        <a-menu-item
          v-else
          class="rounded-lg px-3 py-2 text-xs text-blue-600"
          @click="triggerUnlock(record.id, refresh)"
        >
          <UnlockOutlined /> Mở khóa tài khoản
        </a-menu-item>
      </template>

      <!-- Custom cells -->
      <template #bodyCell="{ column, record }">
        <!-- Account username cell with role badge -->
        <template v-if="column.key === 'username'">
          <div class="flex items-center gap-2.5">
            <!-- Role avatar -->
            <div
              class="w-8 h-8 rounded-full flex items-center justify-center text-[11px] font-bold text-white flex-shrink-0"
              :style="{ background: roleColor(record.role) }"
            >
              {{ roleInitials(record.role) }}
            </div>
            <div class="min-w-0">
              <div class="text-xs font-semibold text-base-primary truncate max-w-[140px]" :title="record.username">
                {{ record.username || '—' }}
              </div>
              <div class="flex items-center gap-1">
                <span
                  class="inline-flex items-center text-[9px] font-semibold px-1.5 py-0.5 rounded-full"
                  :class="roleBadgeClass(record.role)"
                >
                  {{ roleLabel(record.role) }}
                </span>
              </div>
            </div>
          </div>
        </template>

        <!-- Full name -->
        <template v-else-if="column.key === 'fullName'">
          <div class="leading-tight">
            <div class="text-xs font-medium text-base-primary truncate max-w-[180px]" :title="record.fullName">
              {{ record.fullName || '—' }}
            </div>
            <div v-if="record.email" class="text-[10px] text-base-muted truncate max-w-[180px]">
              {{ record.email }}
            </div>
          </div>
        </template>

        <!-- Role badge in column -->
        <template v-else-if="column.key === 'role'">
          <span
            class="inline-flex items-center text-[10px] font-semibold px-2 py-0.5 rounded-full"
            :class="roleBadgeClass(record.role)"
          >
            {{ roleLabel(record.role) }}
          </span>
        </template>

        <!-- Created date -->
        <template v-else-if="column.key === 'createdAt'">
          <span class="text-xs text-base-secondary whitespace-nowrap">
            {{ formatDate(record.createdAt) }}
          </span>
        </template>
      </template>
    </AdminResourceView>

    <!-- Confirmation Modal -->
    <ConfirmActionModal
      v-model:open="confirmOpen"
      :title="confirmTitle"
      :message="confirmMsg"
      :type="confirmType"
      :loading="confirmLoading"
      @confirm="handleExecuteAction"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { message } from 'ant-design-vue'
import { LockOutlined, UnlockOutlined } from '@ant-design/icons-vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import ConfirmActionModal from '@/components/admin/ConfirmActionModal.vue'
import { accountApi } from '@/api/accountApi'
import { ACCOUNT_STATUS, USER_ROLE, toOptions } from '@/lib/constants'

const statusOptions = toOptions(ACCOUNT_STATUS, { 1: 'green', 2: 'red' })

// Custom filter states
const filterRole = ref(undefined)
const filterStatusValue = ref(undefined)

// Confirmation modal states
const confirmOpen = ref(false)
const confirmTitle = ref('')
const confirmMsg = ref('')
const confirmType = ref('warning')
const confirmLoading = ref(false)
let confirmActionCallback = null

const columns = [
  { title: 'Tài khoản', key: 'username', width: 200 },
  { title: 'Họ tên', key: 'fullName', width: 200 },
  { title: 'Email', dataIndex: 'email', key: 'email', width: 220, ellipsis: true },
  { title: 'Vai trò', key: 'role', width: 120 },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 120 },
  { title: 'Ngày tạo', key: 'createdAt', width: 120 },
]

const fields = [
  { name: 'username', label: 'Username', required: true, default: '' },
  { name: 'password', label: 'Mật khẩu', type: 'password', default: 'User@123' },
  { name: 'fullName', label: 'Họ tên', required: true, default: '' },
  { name: 'email', label: 'Email', required: true, default: '' },
  { name: 'phone', label: 'Điện thoại', default: '' },
  { name: 'role', label: 'Vai trò', type: 'select', options: toOptions(USER_ROLE), default: 3 },
  { name: 'referenceId', label: 'Reference ID', fullWidth: true, default: '' },
]

const formGroups = [
  {
    title: 'Thông tin đăng nhập',
    fields: ['username', 'password']
  },
  {
    title: 'Thông tin cá nhân',
    fields: ['fullName', 'email', 'phone']
  },
  {
    title: 'Vai trò & Liên kết',
    fields: ['role', 'referenceId']
  }
]

function customFilter(item) {
  const matchRole = filterRole.value === undefined || Number(item.role) === Number(filterRole.value)
  const matchStatus = filterStatusValue.value === undefined || Number(item.status) === Number(filterStatusValue.value)
  return matchRole && matchStatus
}

function resetCustomFilters() {
  filterRole.value = undefined
  filterStatusValue.value = undefined
}

// Role helpers
function roleLabel(role) {
  const labels = { 1: 'Admin', 2: 'Giảng viên', 3: 'Học viên' }
  return labels[Number(role)] || '—'
}

function roleInitials(role) {
  const labels = { 1: 'A', 2: 'T', 3: 'S' }
  return labels[Number(role)] || '?'
}

const ROLE_COLORS = { 1: '#4f46e5', 2: '#059669', 3: '#0891b2' }

function roleColor(role) {
  return ROLE_COLORS[Number(role)] || '#6366f1'
}

const ROLE_BADGE_CLASSES = {
  1: 'bg-indigo-100 text-indigo-700 dark:bg-indigo-900/30 dark:text-indigo-400',
  2: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400',
  3: 'bg-sky-100 text-sky-700 dark:bg-sky-900/30 dark:text-sky-400',
}

function roleBadgeClass(role) {
  return ROLE_BADGE_CLASSES[Number(role)] || 'bg-slate-100 text-slate-600'
}

function formatDate(value) {
  if (!value) return '—'
  return new Date(value).toLocaleDateString('vi-VN')
}

// Confirm trigger helpers
function triggerLock(id, refresh) {
  confirmTitle.value = 'Khóa tài khoản?'
  confirmMsg.value = 'Tài khoản này sẽ không thể đăng nhập cho đến khi được mở khóa lại.'
  confirmType.value = 'danger'
  confirmActionCallback = async () => {
    await accountApi.lock(id)
    message.success('Đã khóa tài khoản')
    refresh()
  }
  confirmOpen.value = true
}

function triggerUnlock(id, refresh) {
  confirmTitle.value = 'Mở khóa tài khoản?'
  confirmMsg.value = 'Tài khoản sẽ được kích hoạt trở lại và có thể đăng nhập bình thường.'
  confirmType.value = 'info'
  confirmActionCallback = async () => {
    await accountApi.unlock(id)
    message.success('Đã mở khóa tài khoản')
    refresh()
  }
  confirmOpen.value = true
}

function triggerBulkLock(ids, refresh) {
  confirmTitle.value = `Khóa ${ids.length} tài khoản?`
  confirmMsg.value = `Tất cả ${ids.length} tài khoản đã chọn sẽ không thể đăng nhập.`
  confirmType.value = 'danger'
  confirmActionCallback = async () => {
    await accountApi.bulkLock(ids)
    message.success('Đã khóa các tài khoản đã chọn')
    refresh()
  }
  confirmOpen.value = true
}

function triggerBulkUnlock(ids, refresh) {
  confirmTitle.value = `Mở khóa ${ids.length} tài khoản?`
  confirmMsg.value = `Tất cả ${ids.length} tài khoản đã chọn sẽ được kích hoạt trở lại.`
  confirmType.value = 'info'
  confirmActionCallback = async () => {
    await accountApi.bulkUnlock(ids)
    message.success('Đã mở khóa các tài khoản đã chọn')
    refresh()
  }
  confirmOpen.value = true
}

async function handleExecuteAction() {
  if (!confirmActionCallback) return
  confirmLoading.value = true
  try {
    await confirmActionCallback()
    confirmOpen.value = false
  } catch (error) {
    message.error(error.message || 'Không thể thực hiện hành động')
  } finally {
    confirmLoading.value = false
  }
}
</script>
