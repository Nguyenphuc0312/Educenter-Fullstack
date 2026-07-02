<template>
  <div class="space-y-4">
    <!-- Header -->
    <div
      class="flex flex-col sm:flex-row sm:items-center gap-3"
      :class="title ? 'sm:justify-between' : 'sm:justify-end'"
    >
      <!-- Title block -->
      <div v-if="title">
        <h1 class="text-xl font-bold text-base-primary tracking-tight">{{ title }}</h1>
        <p v-if="subtitle" class="text-xs text-base-secondary mt-0.5">{{ subtitle }}</p>
      </div>

      <!-- Action buttons: Primary Add + Secondary dropdown -->
      <div class="flex items-center gap-2 flex-wrap shrink-0">
        <!-- Primary: Thêm mới -->
        <button
          v-if="api?.create"
          type="button"
          class="admin-btn admin-btn-primary h-9 px-4 shadow-sm"
          @click="openCreate"
        >
          <PlusOutlined style="font-size: 13px;" />
          Thêm mới
        </button>

        <!-- Secondary: Export/Import in dropdown -->
        <a-dropdown v-if="api?.export || api?.import" trigger="click" placement="bottomRight">
          <button type="button" class="admin-btn admin-btn-secondary h-9 px-3">
            <MoreOutlined style="font-size: 13px;" />
            Thao tác khác
          </button>
          <template #overlay>
            <a-menu class="min-w-[180px] shadow-lg rounded-xl p-1.5 dark:bg-slate-900 border border-slate-100 dark:border-slate-800">
              <a-menu-item
                v-if="api?.export"
                key="export"
                class="rounded-lg px-3 py-2 text-xs font-normal hover:bg-slate-50 dark:hover:bg-slate-800"
                @click="handleExport"
              >
                <div class="flex items-center gap-2">
                  <DownloadOutlined style="font-size: 13px; color: var(--admin-text-muted);" />
                  <span>Xuất dữ liệu (CSV)</span>
                </div>
              </a-menu-item>
              <a-menu-item
                v-if="api?.import"
                key="import"
                class="rounded-lg px-3 py-2 text-xs font-normal hover:bg-slate-50 dark:hover:bg-slate-800"
              >
                <a-upload
                  :before-upload="handleImport"
                  :show-upload-list="false"
                  accept=".json"
                >
                  <div class="flex items-center gap-2 w-full">
                    <UploadOutlined style="font-size: 13px; color: var(--admin-text-muted);" />
                    <span>Nhập dữ liệu (JSON)</span>
                  </div>
                </a-upload>
              </a-menu-item>
            </a-menu>
          </template>
        </a-dropdown>
      </div>
    </div>

    <!-- Filter Bar -->
    <div class="admin-toolbar">
      <div class="flex flex-col lg:flex-row lg:items-center gap-3">
        <!-- Search + custom filters -->
        <div class="flex items-center gap-2 flex-wrap flex-1 min-w-0">
          <a-input
            v-if="showSearch"
            v-model:value="searchText"
            :placeholder="searchPlaceholder"
            allow-clear
            class="admin-search w-full sm:w-64"
          >
            <template #prefix>
              <SearchOutlined style="color: var(--admin-text-subtle); font-size: 14px;" />
            </template>
          </a-input>

          <!-- Custom filter slot from parent -->
          <slot name="filters" />

          <!-- Status filter (default fallback if no filters slot) -->
          <template v-if="!$slots.filters && statusOptions.length">
            <a-select
              v-model:value="statusFilter"
              placeholder="Trạng thái"
              allow-clear
              size="small"
              class="w-36"
            >
              <a-select-option v-for="option in statusOptions" :key="option.value" :value="option.value">
                {{ option.label }}
              </a-select-option>
            </a-select>
          </template>

          <button
            type="button"
            class="admin-btn admin-btn-ghost h-9 px-3"
            @click="resetFilters"
          >
            <ReloadOutlined style="font-size: 13px;" />
            Đặt lại
          </button>
        </div>

        <!-- Actions slot (optional) -->
        <div v-if="$slots.actions" class="flex items-center gap-2 flex-wrap shrink-0">
          <slot name="actions" />
        </div>
      </div>

      <!-- Active filter chips -->
      <div v-if="$slots.filters && (searchText || statusFilter !== undefined)" class="mt-2 flex flex-wrap gap-1.5">
        <span
          v-if="searchText"
          class="admin-filter-chip"
        >
          "{{ searchText }}"
          <button @click="searchText = ''" class="hover:opacity-80 ml-0.5">×</button>
        </span>
        <span
          v-if="statusFilter !== undefined"
          class="admin-filter-chip"
        >
          Trạng thái: {{ statusOptions.find(o => o.value === statusFilter)?.label }}
          <button @click="statusFilter = undefined" class="hover:opacity-80 ml-0.5">×</button>
        </span>
      </div>
    </div>

    <!-- Bulk actions alert -->
    <transition name="slide-down">
      <div
        v-if="selectedRowKeys.length"
        class="rounded-xl px-4 py-2.5 flex items-center justify-between gap-3 flex-wrap"
        style="background: var(--admin-accent-soft); border: 1px solid var(--admin-accent-border);"
      >
        <div class="flex items-center gap-2.5">
          <span class="text-xs font-bold" style="color: var(--admin-accent);">
            Đã chọn {{ selectedRowKeys.length }} dòng
          </span>
          <button
            v-if="api?.bulkDelete"
            type="button"
            class="admin-btn admin-btn-danger h-8 px-3 text-xs"
            @click="handleBulkDelete"
          >
            <DeleteOutlined style="font-size: 12px;" />
            Xóa đã chọn
          </button>
          <slot name="bulkActions" :selected-row-keys="selectedRowKeys" :refresh="fetchItems" />
        </div>
        <button
          type="button"
          class="admin-btn admin-btn-ghost h-8 px-3 text-xs"
          style="color: var(--admin-accent);"
          @click="selectedRowKeys = []"
        >
          Bỏ chọn
        </button>
      </div>
    </transition>

    <!-- Table Container — overflow-x for horizontal scroll, NO fixed columns -->
    <div class="admin-card overflow-hidden">
      <div class="admin-table-scroll">
        <a-table
          :data-source="paginatedItems"
          :columns="tableColumns"
          :pagination="pagination"
          :loading="loading"
          :row-selection="rowSelection"
          :row-key="rowKey"
          size="small"
          class="admin-table w-full"
          @change="onTableChange"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.renderType === 'status'">
              <StatusBadge :value="record[statusField]" :options="statusOptions" />
            </template>

            <template v-else-if="column.renderType === 'money'">
              <span class="font-medium text-base-primary whitespace-nowrap">
                {{ formatVnd(getSortValue(record, column) || 0) }}
              </span>
            </template>

            <template v-else-if="column.renderType === 'date'">
              <span class="text-base-secondary whitespace-nowrap">
                {{ formatDate(getSortValue(record, column)) }}
              </span>
            </template>

            <template v-else-if="column.renderType === 'timeRange'">
              <div class="flex flex-col min-w-0 leading-tight">
                <span class="text-xs font-medium text-base-primary whitespace-nowrap">
                  {{ formatTimeRange(record, column) }}
                </span>
                <span v-if="column.dayField" class="text-[10px] text-base-muted mt-0.5 whitespace-nowrap">
                  {{ formatDayOfWeek(record[column.dayField]) }}
                </span>
              </div>
            </template>

            <template v-else-if="column.key === '__actions'">
              <ActionDropdown
                v-if="hasActionsFor(record)"
                :record="record"
                :show-edit="!!api?.update && canEdit(record)"
                :show-delete="!!api?.delete && canDelete(record)"
                :show-extra="props.hasRowActions(record)"
                @edit="openEdit"
                @delete="handleDelete"
              >
                <template #extra>
                  <slot name="rowActions" :record="record" :refresh="fetchItems" />
                </template>
              </ActionDropdown>
            </template>

            <template v-else>
              <slot name="bodyCell" :column="column" :record="record" />
            </template>
          </template>

          <template #emptyText>
            <EmptyTableState
              :show-action-button="false"
              title="Chưa có dữ liệu"
              description="Không tìm thấy bản ghi phù hợp hoặc danh sách trống."
            />
          </template>
        </a-table>
      </div>
    </div>

    <!-- Modal dialog Form -->
    <a-modal
      v-model:open="modalOpen"
      width="720px"
      :footer="null"
      :destroy-on-close="true"
      :centered="true"
    >
      <template #title>
        <div class="text-sm font-bold text-base-primary pb-3 border-b border-base">
          {{ editingRecord ? 'Cập nhật thông tin' : 'Thêm bản ghi mới' }}
        </div>
      </template>

      <a-form ref="formRef" :model="formState" layout="vertical" class="mt-4 space-y-4">
        <div
          v-if="!editingRecord && props.fields.some((field) => field.editOnly)"
          class="rounded-md border px-3 py-2 text-xs"
          style="background: var(--admin-accent-soft); border-color: var(--admin-accent-border); color: var(--admin-text-secondary);"
        >
          Mã bản ghi được hệ thống tự sinh sau khi lưu.
        </div>
        <div class="space-y-5">
          <div v-for="(group, gIdx) in computedGroups" :key="gIdx" class="space-y-3">
            <div v-if="group.title" class="text-[11px] font-bold uppercase tracking-wider text-slate-400 dark:text-slate-500 border-l-2 border-primary-500 pl-2">
              {{ group.title }}
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-x-4 gap-y-3">
              <a-form-item
                v-for="field in group.fields"
                :key="field.name"
                :label="field.label"
                :name="field.name"
                :rules="getFieldRules(field)"
                :class="field.fullWidth ? 'md:col-span-2' : ''"
                class="mb-0"
              >
                <a-textarea
                  v-if="field.type === 'textarea'"
                  v-model:value="formState[field.name]"
                  :rows="field.rows || 3"
                  :placeholder="resolveFieldPlaceholder(field)"
                  class="text-xs"
                  :disabled="resolveFieldDisabled(field)"
                />
                <a-input-number
                  v-else-if="field.type === 'number'"
                  v-model:value="formState[field.name]"
                  class="w-full text-xs"
                  :min="field.min ?? 0"
                  :disabled="resolveFieldDisabled(field)"
                />
                <a-date-picker
                  v-else-if="field.type === 'date'"
                  v-model:value="formState[field.name]"
                  class="w-full text-xs"
                  value-format="YYYY-MM-DD"
                  :disabled="resolveFieldDisabled(field)"
                />
                <a-select
                  v-else-if="field.type === 'select'"
                  v-model:value="formState[field.name]"
                  :placeholder="resolveFieldPlaceholder(field)"
                  class="text-xs w-full"
                  allow-clear
                  show-search
                  option-filter-prop="label"
                  :mode="field.mode"
                  :max-tag-count="field.maxTagCount ?? 2"
                  :disabled="resolveFieldDisabled(field)"
                  @change="handleFieldChange(field, $event)"
                >
                  <a-select-option
                    v-for="option in resolveFieldOptions(field)"
                    :key="option.value"
                    :value="option.value"
                    :label="option.label || option.text"
                  >
                    {{ option.label || option.text }}
                  </a-select-option>
                </a-select>
                <a-switch v-else-if="field.type === 'switch'" v-model:checked="formState[field.name]" />
                <a-input-password
                  v-else-if="field.type === 'password'"
                  v-model:value="formState[field.name]"
                  :placeholder="resolveFieldPlaceholder(field)"
                  class="text-xs"
                  :disabled="resolveFieldDisabled(field)"
                />
                <a-input
                  v-else
                  v-model:value="formState[field.name]"
                  :type="field.type || 'text'"
                  :placeholder="resolveFieldPlaceholder(field)"
                  class="text-xs"
                  :disabled="resolveFieldDisabled(field)"
                />
              </a-form-item>
            </div>
          </div>
        </div>

        <!-- Footer buttons: Cancel (secondary) + Save (primary) -->
        <div class="flex justify-end gap-2 pt-4 border-t border-base mt-5">
          <button
            type="button"
            class="admin-btn admin-btn-secondary h-10 px-5"
            :disabled="saving"
            @click="modalOpen = false"
          >
            Đóng
          </button>
          <button
            type="button"
            class="admin-btn admin-btn-primary h-10 px-5 shadow-sm"
            :disabled="saving"
            @click="handleSubmit"
          >
            <span v-if="saving" class="inline-block w-3.5 h-3.5 border-2 border-white/30 border-t-white rounded-full animate-spin"></span>
            {{ saving ? 'Đang lưu...' : 'Lưu lại' }}
          </button>
        </div>
      </a-form>
    </a-modal>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { message } from 'ant-design-vue'
import {
  DeleteOutlined,
  DownloadOutlined,
  PlusOutlined,
  UploadOutlined,
  MoreOutlined,
  ReloadOutlined,
  SearchOutlined,
} from '@ant-design/icons-vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import StatusBadge from '@/components/admin/StatusBadge.vue'
import ActionDropdown from '@/components/admin/ActionDropdown.vue'
import EmptyTableState from '@/components/admin/EmptyTableState.vue'
import { formatVnd } from '@/lib/formatters'

const props = defineProps({
  title: { type: String, required: true },
  subtitle: { type: String, default: '' },
  api: { type: Object, required: true },
  columns: { type: Array, default: () => [] },
  fields: { type: Array, default: () => [] },
  searchableFields: { type: Array, default: () => [] },
  statusField: { type: String, default: 'status' },
  statusOptions: { type: Array, default: () => [] },
  rowKey: { type: String, default: 'id' },
  searchPlaceholder: { type: String, default: 'Tìm kiếm...' },
  normalizeIn: { type: Function, default: (item) => item },
  normalizeOut: { type: Function, default: (item) => item },
  formGroups: { type: Array, default: () => [] },
  filterFn: { type: Function, default: null },
  showSearch: { type: Boolean, default: true },
  canEdit: { type: Function, default: () => true },
  canDelete: { type: Function, default: () => true },
  hasRowActions: { type: Function, default: () => true },
  canSelect: { type: Function, default: () => true },
})

const emit = defineEmits(['reset'])

const items = ref([])
const loading = ref(false)
const saving = ref(false)
const exporting = ref(false)
const selectedRowKeys = ref([])
const searchText = ref('')
const statusFilter = ref(undefined)
const modalOpen = ref(false)
const editingRecord = ref(null)
const formRef = ref()
const formState = reactive({})

// Pagination — reactive state so Ant Design Vue 4 updates properly
const currentPage = ref(1)
const pageSize = ref(10)
const sortState = reactive({
  columnKey: null,
  order: null,
})

// Group form fields for modular rendering
const computedGroups = computed(() => {
  const visibleFields = props.fields.filter((field) => {
    const hidden = typeof field.hidden === 'function' ? field.hidden(formState, editingRecord.value) : field.hidden
    return !hidden && (!field.editOnly || editingRecord.value)
  })
  if (props.formGroups && props.formGroups.length > 0) {
    return props.formGroups.map((group) => ({
      title: group.title,
      fields: visibleFields.filter((f) => group.fields.includes(f.name))
    })).filter((group) => group.fields.length > 0)
  }
  return [{ title: '', fields: visibleFields }]
})

function columnKey(column) {
  return column.key || column.dataIndex
}

function isSortableColumn(column) {
  if (column.type === 'status') return false
  return column.sortable !== false && Boolean(column.sortValue || column.sortField || column.dataIndex || column.key)
}

// Table columns — sorting is controlled so it runs before client-side pagination.
const tableColumns = computed(() => {
  const mapped = props.columns.map((column) => {
    const key = columnKey(column)
    const mappedColumn = {
      ...column,
      key,
      renderType: column.type,
    }

    if (isSortableColumn(column)) {
      mappedColumn.sorter = true
      mappedColumn.sortOrder = sortState.columnKey === key ? sortState.order : null
      mappedColumn.sortDirections = ['ascend', 'descend']
    }

    if (column.type === 'status') {
      mappedColumn.filters = props.statusOptions
      mappedColumn.onFilter = (value, record) => normalizeStatusValue(record[props.statusField]) === normalizeStatusValue(value)
    }

    return mappedColumn
  })

  return [
    ...mapped,
    // Action column — NOT fixed, let it scroll naturally with table
    { title: 'Thao tác', key: '__actions', width: 130, fixed: undefined },
  ]
})

const rowSelection = computed(() => ({
  selectedRowKeys: selectedRowKeys.value,
  getCheckboxProps: (record) => ({ disabled: !props.canSelect(record) }),
  onChange: (keys) => {
    selectedRowKeys.value = keys
  },
}))

function hasActionsFor(record) {
  return (!!props.api?.update && props.canEdit(record))
    || (!!props.api?.delete && props.canDelete(record))
    || props.hasRowActions(record)
}

const filteredItems = computed(() => {
  const keyword = searchText.value.trim().toLowerCase()
  return items.value.filter((item) => {
    const matchesKeyword = !keyword || props.searchableFields.some((field) => String(item[field] ?? '').toLowerCase().includes(keyword))
    const matchesStatus = statusFilter.value === undefined || normalizeStatusValue(item[props.statusField]) === normalizeStatusValue(statusFilter.value)

    let matchesCustom = true
    if (props.filterFn) {
      matchesCustom = props.filterFn(item)
    }

    return matchesKeyword && matchesStatus && matchesCustom
  })
})

const STATUS_VALUE_ALIASES = {
  Draft: 0,
  Open: 1,
  Closed: 2,
  ComingSoon: 3,
  Full: 1,
  InProgress: 2,
  Completed: 3,
  Cancelled: 4,
  Pending: 1,
  Confirmed: 2,
  Unpaid: 1,
  Partial: 2,
  Paid: 3,
  Overdue: 4,
  Success: 1,
  Processing: 2,
  Failed: 3,
  CancelledPayment: 4,
  Active: 1,
  Inactive: 2,
  Locked: 2,
  Suspended: 3,
}

function normalizeStatusValue(value) {
  if (value === null || value === undefined || value === '') return value
  if (typeof value === 'number') return value
  if (typeof value === 'string' && /^\d+$/.test(value)) return Number(value)
  return STATUS_VALUE_ALIASES[value] ?? value
}

function getSortColumn() {
  return props.columns.find((column) => columnKey(column) === sortState.columnKey)
}

function getSortValue(record, column) {
  if (typeof column?.sortValue === 'function') return column.sortValue(record)
  const field = column?.sortField || column?.dataIndex || column?.key
  return field ? record?.[field] : null
}

function isDateColumn(column) {
  const field = String(column?.sortField || column?.dataIndex || column?.key || '')
  return column?.type === 'date' || /(date|at|start|end|due)$/i.test(field)
}

function compareValues(left, right, column) {
  const leftEmpty = left === null || left === undefined || left === ''
  const rightEmpty = right === null || right === undefined || right === ''
  if (leftEmpty || rightEmpty) {
    if (leftEmpty && rightEmpty) return 0
    return leftEmpty ? 1 : -1
  }

  if (isDateColumn(column)) {
    const leftTime = new Date(left).getTime()
    const rightTime = new Date(right).getTime()
    if (!Number.isNaN(leftTime) && !Number.isNaN(rightTime)) return leftTime - rightTime
  }

  const leftNumber = typeof left === 'number' ? left : Number(left)
  const rightNumber = typeof right === 'number' ? right : Number(right)
  if (!Number.isNaN(leftNumber) && !Number.isNaN(rightNumber)) return leftNumber - rightNumber

  return String(left).localeCompare(String(right), 'vi', {
    numeric: true,
    sensitivity: 'base',
  })
}

const sortedItems = computed(() => {
  if (!sortState.columnKey || !sortState.order) return filteredItems.value
  const column = getSortColumn()
  if (!column) return filteredItems.value

  const direction = sortState.order === 'descend' ? -1 : 1
  return [...filteredItems.value].sort((left, right) => {
    if (typeof column.sorter === 'function') return direction * column.sorter(left, right)
    return direction * compareValues(getSortValue(left, column), getSortValue(right, column), column)
  })
})

// Client-side pagination — sort the complete filtered list before slicing pages.
const totalItems = computed(() => filteredItems.value.length)

const paginatedItems = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return sortedItems.value.slice(start, start + pageSize.value)
})

function onPageChange(page, size) {
  currentPage.value = page
  pageSize.value = size
  // Reset to page 1 when page size changes
  if (size !== pageSize.value) currentPage.value = 1
}

const pagination = computed(() => ({
  current: currentPage.value,
  pageSize: pageSize.value,
  total: totalItems.value,
  pageSizeOptions: ['10', '20', '50', '100'],
  showSizeChanger: true,
  showQuickJumper: false,
  showTotal: (total) => `${totalItems.value} bản ghi`,
  size: 'small',
  locale: {
    jump_to: 'Đến trang',
    page: '',
    items_per_page: '/ trang',
  },
}))

function getListFromResponse(data) {
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.data)) return data.data
  return []
}

async function fetchItems() {
  if (!props.api?.getAll) return
  loading.value = true
  try {
    const data = await props.api.getAll()
    items.value = getListFromResponse(data).map(props.normalizeIn)
  } catch (error) {
    message.error(error.message || 'Không thể tải dữ liệu')
    items.value = []
  } finally {
    loading.value = false
  }
}

function resetFilters() {
  searchText.value = ''
  statusFilter.value = undefined
  sortState.columnKey = null
  sortState.order = null
  currentPage.value = 1
  emit('reset')
}

function onTableChange(paginationInfo, _filters, sorterInfo) {
  if (paginationInfo.current) currentPage.value = paginationInfo.current
  if (paginationInfo.pageSize) pageSize.value = paginationInfo.pageSize

  const sorter = Array.isArray(sorterInfo) ? sorterInfo[0] : sorterInfo
  const nextColumnKey = sorter?.order ? sorter.columnKey || sorter.field : null
  const sortChanged = sortState.columnKey !== nextColumnKey || sortState.order !== (sorter?.order || null)
  sortState.columnKey = nextColumnKey
  sortState.order = sorter?.order || null
  if (sortChanged) currentPage.value = 1
}

// Reset to page 1 when filters/search change
watch([searchText, statusFilter], () => {
  currentPage.value = 1
})

watch(totalItems, () => {
  const maxPage = Math.max(1, Math.ceil(totalItems.value / pageSize.value))
  if (currentPage.value > maxPage) currentPage.value = maxPage
})

function resetForm(record = null) {
  Object.keys(formState).forEach((key) => delete formState[key])
  props.fields.forEach((field) => {
    const fallback = record && Object.prototype.hasOwnProperty.call(field, 'editDefault') ? field.editDefault : field.default
    formState[field.name] = normalizeSelectValue(field, record?.[field.name] ?? fallback ?? null)
  })
}

function normalizeSelectValue(field, value) {
  if (field.type !== 'select' || value === null || value === undefined) return value
  if (field.mode === 'multiple') {
    return Array.isArray(value) ? value : []
  }
  const options = resolveFieldOptions(field)
  if (options.some((option) => option.value === value)) return value
  const englishEnumMap = {
    Sunday: 0,
    Monday: 1,
    Tuesday: 2,
    Wednesday: 3,
    Thursday: 4,
    Friday: 5,
    Saturday: 6,
    Morning: 0,
    Afternoon: 1,
    Evening: 2,
    Offline: 0,
    Online: 1,
    Hybrid: 2,
    Open: 0,
    Full: 1,
    InProgress: 2,
    Completed: 3,
    Cancelled: 4,
    Draft: 0,
    Closed: 2,
    ComingSoon: 3,
    Scheduled: 0,
    Admin: 1,
    Teacher: 2,
    Student: 3,
  }
  const mapped = englishEnumMap[value]
  return options.some((option) => option.value === mapped) ? mapped : value
}

function resolveFieldOptions(field) {
  return typeof field.options === 'function' ? field.options(formState, editingRecord.value) : field.options || []
}

function resolveFieldDisabled(field) {
  return typeof field.disabled === 'function' ? field.disabled(formState, editingRecord.value) : !!field.disabled
}

function resolveFieldPlaceholder(field) {
  return typeof field.placeholder === 'function'
    ? field.placeholder(formState, editingRecord.value)
    : field.placeholder || field.label
}

function getFieldRules(field) {
  const rules = []
  if (field.required) rules.push({ required: true, message: `Vui lòng nhập ${String(field.label || '').toLowerCase()}` })
  if (Array.isArray(field.rules)) rules.push(...field.rules)
  return rules
}

function handleFieldChange(field, value) {
  if (typeof field.onChange !== 'function') return
  const option = resolveFieldOptions(field).find((item) => item.value === value)
  field.onChange(value, formState, { option, editingRecord: editingRecord.value })
}

function openCreate() {
  editingRecord.value = null
  resetForm()
  modalOpen.value = true
}

function openEdit(record) {
  editingRecord.value = record
  resetForm(record)
  modalOpen.value = true
}

async function handleSubmit() {
  try {
    await formRef.value?.validate()
    saving.value = true
    const payload = props.normalizeOut({ ...formState })
    if (editingRecord.value) {
      await props.api.update(editingRecord.value[props.rowKey], payload)
      message.success('Cập nhật thành công')
    } else {
      await props.api.create(payload)
      message.success('Tạo mới thành công')
    }
    modalOpen.value = false
    await fetchItems()
  } catch (error) {
    if (error?.errorFields) return
    message.error(error.message || 'Không thể lưu dữ liệu')
  } finally {
    saving.value = false
  }
}

async function handleDelete(record) {
  const id = record[props.rowKey]
  try {
    await props.api.delete(id)
    message.success('Đã xóa bản ghi')
    await fetchItems()
  } catch (error) {
    message.error(error.message || 'Không thể xóa bản ghi')
  }
}

async function handleBulkDelete() {
  try {
    await props.api.bulkDelete(selectedRowKeys.value)
    message.success('Đã xóa các bản ghi đã chọn')
    selectedRowKeys.value = []
    await fetchItems()
  } catch (error) {
    message.error(error.message || 'Không thể xóa hàng loạt')
  }
}

async function handleImport(file) {
  try {
    await props.api.import(file)
    message.success('Import thành công')
    await fetchItems()
  } catch (error) {
    message.error(error.message || 'Import thất bại')
  }
  return false
}

async function handleExport() {
  exporting.value = true
  try {
    await props.api.export()
  } catch (error) {
    message.error(error.message || 'Export thất bại')
  } finally {
    exporting.value = false
  }
}

function formatDate(value) {
  if (!value) return '-'
  return new Date(value).toLocaleDateString('vi-VN')
}

function formatTimeValue(value) {
  if (!value) return '--:--'
  return String(value).slice(0, 5)
}

function formatTimeRange(record, column) {
  return `${formatTimeValue(record[column.startField || 'startTime'])} - ${formatTimeValue(record[column.endField || 'endTime'])}`
}

function formatDayOfWeek(value) {
  const labels = {
    0: 'Chủ nhật', 1: 'Thứ Hai', 2: 'Thứ Ba', 3: 'Thứ Tư',
    4: 'Thứ Năm', 5: 'Thứ Sáu', 6: 'Thứ Bảy',
    Sunday: 'Chủ nhật', Monday: 'Thứ Hai', Tuesday: 'Thứ Ba', Wednesday: 'Thứ Tư',
    Thursday: 'Thứ Năm', Friday: 'Thứ Sáu', Saturday: 'Thứ Bảy',
  }
  return labels[value] || value || '-'
}

defineExpose({ fetchItems })

onMounted(fetchItems)
</script>

<style>
/* Admin table base styles — delegated to scoped CSS in [data-role="admin"] via .admin-table class.
   Keep ONLY the per-cell nowrap default and the opt-in ellipsis handling here. */
.admin-table .ant-table-cell-ellipsis {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 0;
}
/* Cells marked with .admin-cell-ellipsis (parent opted in) — clamp with ellipsis */
.admin-table td.admin-cell-ellipsis {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: initial;
}
/* Cells marked with .admin-cell-wrap (parent opted in) — allow multi-line wrap */
.admin-table td.admin-cell-wrap {
  white-space: normal;
  word-break: break-word;
}
/* Default numeric cells (money, date, action) should stay on one line */
.admin-table td.col-numeric,
.admin-table td.col-action {
  white-space: nowrap;
}
</style>
