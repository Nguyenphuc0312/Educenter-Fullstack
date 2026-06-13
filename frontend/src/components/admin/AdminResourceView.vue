<template>
  <div class="space-y-4">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
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
            :value="searchText"
            @input="$emit('update:searchText', $event.target.value)"
            @press-enter="$emit('search', searchText)"
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
        v-if="selectable && selectedRowKeys.length"
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
          :row-selection="selectable ? rowSelection : null"
          :row-key="rowKey"
          size="small"
          class="admin-table w-full"
          @change="onTableChange"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === '__status'">
              <StatusBadge :value="record[statusField]" :options="statusOptions" />
            </template>

            <template v-else-if="column.key === '__money'">
              <span class="font-medium text-base-primary whitespace-nowrap">
                {{ formatVnd(record[column.dataIndex] || 0) }}
              </span>
            </template>

            <template v-else-if="column.key === '__date'">
              <span class="text-base-secondary whitespace-nowrap">
                {{ formatDate(record[column.dataIndex]) }}
              </span>
            </template>

            <template v-else-if="column.key === '__actions'">
              <ActionDropdown
                :record="record"
                :show-edit="!!api?.update && canEditRecord(record)"
                :show-delete="!!api?.delete && canDeleteRecord(record)"
                :show-more="canShowMoreActions(record)"
                :more-icon="moreActionIcon"
                :more-title="moreActionTitle"
                @edit="openEdit"
                @delete="handleDelete"
              >
                <template v-if="canShowMoreActions(record)" #extra>
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
                :rules="field.required ? [{ required: true, message: `Vui lòng nhập ${field.label.toLowerCase()}` }] : []"
                :class="field.fullWidth ? 'md:col-span-2' : ''"
                class="mb-0"
              >
                <a-textarea
                  v-if="field.type === 'textarea'"
                  v-model:value="formState[field.name]"
                  :rows="field.rows || 3"
                  :placeholder="field.placeholder || field.label"
                  class="text-xs"
                />
                <a-input-number
                  v-else-if="field.type === 'number'"
                  v-model:value="formState[field.name]"
                  class="w-full text-xs"
                  :min="field.min ?? 0"
                />
                <a-date-picker
                  v-else-if="field.type === 'date'"
                  v-model:value="formState[field.name]"
                  class="w-full text-xs"
                  value-format="YYYY-MM-DD"
                />
                <a-select
                  v-else-if="field.type === 'select'"
                  v-model:value="formState[field.name]"
                  :placeholder="field.placeholder || field.label"
                  class="text-xs w-full"
                  allow-clear
                >
                  <a-select-option v-for="option in field.options || []" :key="option.value" :value="option.value">
                    {{ option.label || option.text }}
                  </a-select-option>
                </a-select>
                <a-switch v-else-if="field.type === 'switch'" v-model:checked="formState[field.name]" />
                <a-input
                  v-else
                  v-model:value="formState[field.name]"
                  :type="field.type || 'text'"
                  :placeholder="field.placeholder || field.label"
                  class="text-xs"
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
  selectable: { type: Boolean, default: true },
  canEditRecord: { type: Function, default: () => true },
  canDeleteRecord: { type: Function, default: () => true },
  canSelectRecord: { type: Function, default: () => true },
  canShowMoreActions: { type: Function, default: () => true },
  moreActionIcon: { type: Object, default: null },
  moreActionTitle: { type: String, default: 'Thao tác khác' },
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

// Group form fields for modular rendering
const computedGroups = computed(() => {
  if (props.formGroups && props.formGroups.length > 0) {
    return props.formGroups.map((group) => ({
      title: group.title,
      fields: props.fields.filter((f) => group.fields.includes(f.name))
    }))
  }
  return [{ title: '', fields: props.fields }]
})

// Table columns — NO fixed right, let them scroll naturally
const tableColumns = computed(() => {
  const mapped = props.columns.map((column) => {
    if (column.type === 'status') {
      return { ...column, key: '__status', filters: props.statusOptions, onFilter: (value, record) => record[props.statusField] === value }
    }
    if (column.type === 'money') return { ...column, key: '__money', sorter: (a, b) => (a[column.dataIndex] || 0) - (b[column.dataIndex] || 0) }
    if (column.type === 'date') return { ...column, key: '__date' }
    return column
  })

  return [
    ...mapped,
    // Action column — NOT fixed, let it scroll naturally with table
    { title: 'Thao tác', key: '__actions', width: 130, align: 'center', fixed: undefined },
  ]
})

const rowSelection = computed(() => ({
  selectedRowKeys: selectedRowKeys.value,
  onChange: (keys) => {
    selectedRowKeys.value = keys
  },
  getCheckboxProps: (record) => ({
    disabled: !props.canSelectRecord(record),
  }),
}))

const filteredItems = computed(() => {
  const keyword = searchText.value.trim().toLowerCase()
  return items.value.filter((item) => {
    const matchesKeyword = !keyword || props.searchableFields.some((field) => String(item[field] ?? '').toLowerCase().includes(keyword))
    const matchesStatus = statusFilter.value === undefined || item[props.statusField] === statusFilter.value

    let matchesCustom = true
    if (props.filterFn) {
      matchesCustom = props.filterFn(item)
    }

    return matchesKeyword && matchesStatus && matchesCustom
  })
})

// Client-side pagination — reactive so Ant Design Vue 4 updates correctly
const totalItems = computed(() => filteredItems.value.length)

const paginatedItems = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return filteredItems.value.slice(start, start + pageSize.value)
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
  currentPage.value = 1
  emit('reset')
}

function onTableChange(paginationInfo) {
  if (paginationInfo.current) currentPage.value = paginationInfo.current
  if (paginationInfo.pageSize) pageSize.value = paginationInfo.pageSize
}

// Reset to page 1 when filters/search change
watch([searchText, statusFilter], () => {
  currentPage.value = 1
})

function resetForm(record = null) {
  Object.keys(formState).forEach((key) => delete formState[key])
  props.fields.forEach((field) => {
    formState[field.name] = record?.[field.name] ?? field.default ?? null
  })
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
    const allowedIds = items.value
      .filter((item) => selectedRowKeys.value.includes(item[props.rowKey]) && props.canDeleteRecord(item))
      .map((item) => item[props.rowKey])
    if (!allowedIds.length) {
      message.warning('Không có bản ghi hợp lệ để xóa')
      return
    }
    await props.api.bulkDelete(allowedIds)
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
  max-width: 0;
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
