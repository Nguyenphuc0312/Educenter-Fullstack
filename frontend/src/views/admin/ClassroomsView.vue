<template>
  <AdminResourceView
    title="Quản lý phòng học"
    subtitle="Cài đặt phòng học lý thuyết, thực hành, phòng seminar hoặc phòng học trực tuyến."
    :api="classroomApi"
    :columns="columns"
    :fields="fields"
    :searchable-fields="['code', 'name', 'building', 'floor', 'description']"
    :status-options="statusOptions"
    :form-groups="formGroups"
    :filter-fn="customFilter"
    @reset="resetCustomFilters"
  >
    <!-- Custom filters slot -->
    <template #filters>
      <a-select
        v-model:value="filterType"
        placeholder="Loại phòng"
        allow-clear
        size="small"
        class="w-40"
      >
        <a-select-option v-for="(label, val) in CLASSROOM_TYPE" :key="val" :value="Number(val)">
          {{ label }}
        </a-select-option>
      </a-select>

      <a-select
        v-model:value="filterStatus"
        placeholder="Trạng thái"
        allow-clear
        size="small"
        class="w-36"
      >
        <a-select-option v-for="(label, val) in CLASSROOM_STATUS" :key="val" :value="Number(val)">
          {{ label }}
        </a-select-option>
      </a-select>

      <div class="flex items-center gap-1.5 ml-2">
        <a-checkbox v-model:checked="filterAirConditioner" class="text-xs">Điều hòa</a-checkbox>
        <a-checkbox v-model:checked="filterProjector" class="text-xs">Máy chiếu</a-checkbox>
      </div>
    </template>

    <!-- Custom body cells -->
    <template #bodyCell="{ column, record }">
      <!-- Cột phòng học -->
      <template v-if="column.key === 'classroomInfo'">
        <div class="flex flex-col min-w-0">
          <div class="flex items-center gap-2">
            <span class="text-[13px] font-semibold text-base-primary">
              {{ record.name || '—' }}
            </span>
            <span
              v-if="record.isOnline"
              class="inline-flex items-center px-1.5 py-0.5 rounded text-[9px] font-bold uppercase tracking-wider bg-blue-50 text-blue-600 dark:bg-blue-950/30 dark:text-blue-400 border border-blue-100 dark:border-blue-900/50"
            >
              Online
            </span>
          </div>
          <span class="text-[11px] font-mono text-base-secondary mt-0.5">
            {{ record.code || '—' }}
          </span>
        </div>
      </template>

      <!-- Cột địa điểm -->
      <template v-else-if="column.key === 'location'">
        <span v-if="record.isOnline" class="text-[12px] text-base-secondary italic">
          Kênh trực tuyến
        </span>
        <span v-else-if="record.building || record.floor" class="text-[12px] text-base-primary">
          {{ [record.floor, record.building].filter(Boolean).join(', ') }}
        </span>
        <span v-else class="text-[12px] text-base-muted">—</span>
      </template>

      <!-- Cột phân loại -->
      <template v-else-if="column.key === 'classroomType'">
        <span class="text-[12px] font-medium text-base-primary">
          {{ classroomTypeLabel(record.type) }}
        </span>
      </template>

      <!-- Cột tiện ích -->
      <template v-else-if="column.key === 'amenities'">
        <div class="flex items-center gap-1.5">
          <span
            v-if="record.hasAirConditioner"
            class="inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-semibold bg-emerald-50 text-emerald-600 dark:bg-emerald-950/20 dark:text-emerald-400"
            title="Có điều hòa"
          >
            ❄️ Điều hòa
          </span>
          <span
            v-if="record.hasProjector"
            class="inline-flex items-center px-1.5 py-0.5 rounded text-[10px] font-semibold bg-indigo-50 text-indigo-600 dark:bg-indigo-950/20 dark:text-indigo-400"
            title="Có máy chiếu"
          >
            📹 Máy chiếu
          </span>
          <span
            v-if="!record.hasAirConditioner && !record.hasProjector && !record.isOnline"
            class="text-[11px] text-base-muted"
          >
            Không có
          </span>
          <a
            v-if="record.isOnline && record.onlineMeetingUrl"
            :href="record.onlineMeetingUrl"
            target="_blank"
            class="text-xs text-blue-600 dark:text-blue-400 hover:underline inline-flex items-center gap-1"
          >
            🔗 Link lớp học
          </a>
        </div>
      </template>
    </template>
  </AdminResourceView>
</template>

<script setup>
import { ref } from 'vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import { classroomApi } from '@/api/classroomApi'
import { CLASSROOM_TYPE, CLASSROOM_STATUS, toOptions } from '@/lib/constants'

const statusOptions = toOptions(CLASSROOM_STATUS, { 0: 'green', 1: 'blue', 2: 'orange', 3: 'red' })
const typeOptions = toOptions(CLASSROOM_TYPE)

const filterType = ref(undefined)
const filterStatus = ref(undefined)
const filterAirConditioner = ref(false)
const filterProjector = ref(false)

const columns = [
  { title: 'Phòng học', dataIndex: 'name', key: 'classroomInfo', minWidth: 200 },
  { title: 'Địa điểm', dataIndex: 'building', key: 'location', width: 180 },
  { title: 'Sức chứa', dataIndex: 'capacity', key: 'capacity', width: 110, sortable: true },
  { title: 'Phân loại', dataIndex: 'type', key: 'classroomType', width: 150 },
  { title: 'Tiện ích / Kênh', key: 'amenities', width: 220 },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 140 }
]

const fields = [
  { name: 'code', label: 'Mã phòng', required: true, default: '' },
  { name: 'name', label: 'Tên phòng', required: true, default: '' },
  { name: 'capacity', label: 'Sức chứa (Học viên)', type: 'number', required: true, default: 30, min: 1 },
  { name: 'type', label: 'Loại phòng', type: 'select', options: typeOptions, required: true, default: 0 },
  { name: 'building', label: 'Tòa nhà (VD: Tòa A)', default: '' },
  { name: 'floor', label: 'Tầng (VD: Tầng 2)', default: '' },
  { name: 'hasProjector', label: 'Trang bị máy chiếu', type: 'switch', default: false },
  { name: 'hasAirConditioner', label: 'Trang bị điều hòa', type: 'switch', default: false },
  { name: 'isOnline', label: 'Lớp học trực tuyến (Online)', type: 'switch', default: false },
  { 
    name: 'onlineMeetingUrl', 
    label: 'Đường dẫn phòng học (Zoom/Meet)', 
    default: '',
    disabled: (formState) => !formState.isOnline
  },
  { name: 'status', label: 'Trạng thái', type: 'select', options: statusOptions, required: true, default: 0 },
  { name: 'description', label: 'Mô tả chi tiết', type: 'textarea', default: '', fullWidth: true }
]

const formGroups = [
  {
    title: 'Thông tin cơ bản',
    fields: ['code', 'name', 'capacity', 'type']
  },
  {
    title: 'Vị trí vật lý (Chỉ dùng cho lớp trực tiếp)',
    fields: ['building', 'floor']
  },
  {
    title: 'Tiện nghi & Lớp trực tuyến',
    fields: ['hasProjector', 'hasAirConditioner', 'isOnline', 'onlineMeetingUrl']
  },
  {
    title: 'Khác',
    fields: ['status', 'description']
  }
]

function customFilter(item) {
  const matchType = filterType.value === undefined || Number(item.type) === Number(filterType.value)
  const matchStatus = filterStatus.value === undefined || Number(item.status) === Number(filterStatus.value)
  const matchAC = !filterAirConditioner.value || item.hasAirConditioner
  const matchProjector = !filterProjector.value || item.hasProjector
  return matchType && matchStatus && matchAC && matchProjector
}

function resetCustomFilters() {
  filterType.value = undefined
  filterStatus.value = undefined
  filterAirConditioner.value = false
  filterProjector.value = false
}

function classroomTypeLabel(type) {
  if (type === null || type === undefined) return '—'
  return CLASSROOM_TYPE[Number(type)] || '—'
}
</script>
