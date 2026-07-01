<template>
  <AdminResourceView
    title="Quản lý khóa học"
    subtitle="Quản lý danh mục khóa học, học phí, trạng thái mở lớp và dữ liệu public."
    :api="courseApi"
    :columns="columns"
    :fields="fields"
    :searchable-fields="['code', 'name', 'category', 'level']"
    :status-options="courseStatusOptions"
    :normalize-out="normalizeCourse"
    :form-groups="formGroups"
    :filter-fn="customFilter"
    @reset="resetCustomFilters"
  >
    <!-- Custom filters slot -->
    <template #filters>
      <!-- Category Filter -->
      <a-select
        v-model:value="filterCategory"
        placeholder="Danh mục"
        allow-clear
        size="small"
        class="w-36"
      >
        <a-select-option value="Frontend">Frontend</a-select-option>
        <a-select-option value="Backend">Backend</a-select-option>
        <a-select-option value="Mobile">Mobile</a-select-option>
        <a-select-option value="DevOps">DevOps</a-select-option>
      </a-select>

      <!-- Level Filter -->
      <a-select
        v-model:value="filterLevel"
        placeholder="Cấp độ"
        allow-clear
        size="small"
        class="w-32"
      >
        <a-select-option value="Cơ bản">Cơ bản</a-select-option>
        <a-select-option value="Trung cấp">Trung cấp</a-select-option>
        <a-select-option value="N�ng cao">N�ng cao</a-select-option>
      </a-select>

      <!-- Tuition Range Filter -->
      <a-select
        v-model:value="filterTuitionRange"
        placeholder="Mức học phí"
        allow-clear
        size="small"
        class="w-36"
      >
        <a-select-option value="under_5m">Dưới 5 triệu</a-select-option>
        <a-select-option value="5m_10m">5 - 10 triệu</a-select-option>
        <a-select-option value="above_10m">Trên 10 triệu</a-select-option>
      </a-select>

      <!-- Best Seller Filter -->
      <a-select
        v-model:value="filterBestSeller"
        placeholder="Bán chạy"
        allow-clear
        size="small"
        class="w-28"
      >
        <a-select-option :value="true">Bán chạy</a-select-option>
        <a-select-option :value="false">Thường</a-select-option>
      </a-select>
    </template>

    <!-- Custom cells -->
    <template #bodyCell="{ column, record }">
      <!-- Course cell: thumbnail/icon + name + code -->
      <template v-if="column.key === 'name'">
        <div class="flex items-center gap-2.5 min-w-0">
          <!-- Thumbnail or category icon -->
          <div
            v-if="record.thumbnailUrl"
            class="w-10 h-10 rounded-lg overflow-hidden flex-shrink-0 bg-slate-100 dark:bg-slate-800"
          >
            <img
              :src="record.thumbnailUrl"
              :alt="record.name"
              class="w-full h-full object-cover"
              loading="lazy"
            />
          </div>
          <div
            v-else
            class="w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0 text-base leading-none"
            :style="{ background: categoryColor(record.category) }"
          >
            {{ categoryEmoji(record.category) }}
          </div>

          <div class="min-w-0">
            <div class="text-xs font-semibold text-base-primary truncate max-w-[180px]" :title="record.name">
              {{ record.name || '—' }}
            </div>
            <div class="text-[10px] text-base-muted font-mono">
              {{ record.code || '—' }}
            </div>
          </div>
        </div>
      </template>

      <!-- Tuition fee with currency format -->
      <template v-else-if="column.key === 'tuitionFee'">
        <span class="font-bold text-base-primary whitespace-nowrap">
          {{ formatVnd(record.tuitionFee || 0) }}
        </span>
      </template>

      <!-- Sessions count -->
      <template v-else-if="column.key === 'totalSessions'">
        <span class="text-xs text-base-secondary whitespace-nowrap">
          {{ record.totalSessions || 0 }} buổi
        </span>
      </template>

      <!-- Category + Level -->
      <template v-else-if="column.key === 'category'">
        <div class="flex flex-col gap-0.5">
          <span class="text-xs font-medium text-base-primary">{{ record.category || '—' }}</span>
          <span class="text-[10px] text-base-muted">{{ record.level || '—' }}</span>
        </div>
      </template>
    </template>
  </AdminResourceView>
</template>

<script setup>
import { ref } from 'vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import { courseApi } from '@/api/courseApi'
import { COURSE_STATUS, toOptions } from '@/lib/constants'
import { formatVnd } from '@/lib/formatters'

const courseStatusOptions = toOptions(COURSE_STATUS, { 0: 'default', 1: 'blue', 2: 'slate', 3: 'orange' })

// Custom filter states
const filterCategory = ref(undefined)
const filterLevel = ref(undefined)
const filterTuitionRange = ref(undefined)
const filterBestSeller = ref(undefined)

const columns = [
  { title: 'Khóa học', key: 'name', width: 280 },
  { title: 'Danh mục / Cấp độ', key: 'category', width: 160 },
  { title: 'Học phí', key: 'tuitionFee', type: 'money', width: 140, sorter: (a, b) => (a.tuitionFee || 0) - (b.tuitionFee || 0) },
  { title: 'Số buổi', key: 'totalSessions', width: 100 },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 140 },
]

const fields = [
  { name: 'code', label: 'Mã khóa học', required: true, editOnly: true, default: '' },
  { name: 'name', label: 'Tên khóa học', required: true, default: '' },
  { name: 'slug', label: 'Slug', required: true, editOnly: true, default: '' },
  { name: 'category', label: 'Danh mục', required: true, default: 'Frontend' },
  { name: 'level', label: 'Cấp độ', required: true, default: 'Cơ bản' },
  { name: 'tuitionFee', label: 'Học phí', type: 'number', required: true, default: 0 },
  { name: 'totalSessions', label: 'Tổng số buổi', type: 'number', required: true, default: 12 },
  { name: 'durationText', label: 'Thời lượng', default: '8 tuần' },
  { name: 'status', label: 'Trạng thái', type: 'select', options: courseStatusOptions, default: 1 },
  { name: 'shortDescription', label: 'Mô tả ngắn', type: 'textarea', fullWidth: true, default: '' },
  { name: 'description', label: 'Mô tả chi tiết', type: 'textarea', fullWidth: true, default: '' },
  { name: 'thumbnailUrl', label: 'Ảnh đại diện URL', fullWidth: true, default: '' },
  { name: 'isBestSeller', label: 'Bán chạy', type: 'switch', default: false },
  { name: 'isPopularThisWeek', label: 'Phổ biến tuần này', type: 'switch', default: false },
]

// Sectioned form group configuration
const formGroups = [
  {
    title: 'Thông tin cơ bản',
    fields: ['code', 'name', 'slug', 'category', 'level']
  },
  {
    title: 'Học phí & Thời lượng',
    fields: ['tuitionFee', 'totalSessions', 'durationText']
  },
  {
    title: 'Mô tả khóa học',
    fields: ['shortDescription', 'description', 'thumbnailUrl']
  },
  {
    title: 'Trạng thái & Phân loại',
    fields: ['status', 'isBestSeller', 'isPopularThisWeek']
  }
]

// Custom filtration handler
function customFilter(item) {
  const matchCategory = !filterCategory.value || item.category === filterCategory.value
  const matchLevel = !filterLevel.value || item.level === filterLevel.value

  let matchTuition = true
  if (filterTuitionRange.value === 'under_5m') {
    matchTuition = item.tuitionFee < 5000000
  } else if (filterTuitionRange.value === '5m_10m') {
    matchTuition = item.tuitionFee >= 5000000 && item.tuitionFee <= 10000000
  } else if (filterTuitionRange.value === 'above_10m') {
    matchTuition = item.tuitionFee > 10000000
  }

  const matchBestSeller = filterBestSeller.value === undefined || item.isBestSeller === filterBestSeller.value

  return matchCategory && matchLevel && matchTuition && matchBestSeller
}

function resetCustomFilters() {
  filterCategory.value = undefined
  filterLevel.value = undefined
  filterTuitionRange.value = undefined
  filterBestSeller.value = undefined
}

function normalizeCourse(data) {
  const slug = data.slug || data.name?.toLowerCase().normalize('NFD').replace(/[̀-ͯ]/g, '').replace(/[^a-z0-9]+/g, '-').replace(/(^-|-$)/g, '')
  return { ...data, slug }
}

// Category visual helpers
const CATEGORY_COLORS = {
  Frontend: '#4f46e5',
  Backend: '#059669',
  Mobile: '#0891b2',
  DevOps: '#d97706',
  Fullstack: '#7c3aed',
  Database: '#dc2626',
  Security: '#65a30d',
}

const CATEGORY_EMOJI = {
  Frontend: '⚛️',
  Backend: '⚙️',
  Mobile: '📱',
  DevOps: '🚀',
  Fullstack: '🧩',
  Database: '💾',
  Security: '🔒',
}

function categoryColor(category) {
  return CATEGORY_COLORS[category] || '#6366f1'
}

function categoryEmoji(category) {
  return CATEGORY_EMOJI[category] || '📚'
}
</script>
