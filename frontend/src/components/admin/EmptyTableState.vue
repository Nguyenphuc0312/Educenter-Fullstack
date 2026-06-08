<template>
  <div :class="['admin-empty-state', variantClass]">
    <!-- Icon Container -->
    <div :class="['mx-auto flex items-center justify-center rounded-full mb-4', iconSizeClass, iconBgClass]">
      <component :is="iconComponent" :class="['text-2xl', iconTextClass]" />
    </div>

    <!-- Content -->
    <h3 class="text-sm font-semibold mb-1" :class="titleClass">
      {{ resolvedTitle }}
    </h3>
    <p class="text-xs max-w-sm mx-auto leading-relaxed" :class="descClass">
      {{ description }}
    </p>

    <div v-if="showActionButton" class="flex gap-2 justify-center mt-4">
      <a-button size="small" type="primary" class="admin-btn admin-btn-primary h-9 px-4 text-xs" @click="$emit('action')">
        {{ actionButtonText }}
      </a-button>
    </div>
  </div>
</template>

<script setup>
import { computed, h } from 'vue'

const props = defineProps({
  variant: {
    type: String,
    default: 'empty' // 'empty' | 'error' | 'no-result'
  },
  title: {
    type: String,
    default: ''
  },
  description: {
    type: String,
    default: 'Hiện tại chưa có bản ghi nào. Hãy thêm mới hoặc kiểm tra lại các bộ lọc tìm kiếm.'
  },
  showActionButton: {
    type: Boolean,
    default: false
  },
  actionButtonText: {
    type: String,
    default: 'Thêm mới'
  }
})

defineEmits(['action'])

const defaultTitles = {
  empty: 'Chưa có dữ liệu',
  'no-result': 'Không tìm thấy kết quả',
  error: 'Đã xảy ra lỗi'
}

const resolvedTitle = computed(() => props.title || defaultTitles[props.variant] || defaultTitles.empty)

const variantClass = computed(() => {
  if (props.variant === 'error') return 'admin-empty-state--error'
  if (props.variant === 'no-result') return 'admin-empty-state--no-result'
  return 'admin-empty-state--empty'
})

const titleClass = computed(() => {
  if (props.variant === 'error') return 'text-rose-700 dark:text-rose-400'
  return 'text-slate-800 dark:text-slate-200'
})

const descClass = computed(() => {
  if (props.variant === 'error') return 'text-rose-600 dark:text-rose-400/80'
  return 'text-slate-500 dark:text-slate-400'
})

const iconSizeClass = computed(() => {
  return 'w-14 h-14'
})

const iconBgClass = computed(() => {
  if (props.variant === 'error') return 'bg-rose-100 text-rose-500 dark:bg-rose-950/30 dark:text-rose-400'
  if (props.variant === 'no-result') return 'bg-amber-100 text-amber-500 dark:bg-amber-950/30 dark:text-amber-400'
  return 'bg-slate-100 text-slate-400 dark:bg-slate-800 dark:text-slate-500'
})

const iconTextClass = computed(() => '')

// Inline SVG icons
const InboxIcon = {
  render: () => h('svg', { width: '28', height: '28', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '1.5', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('polyline', { points: '21 8 21 21 3 21 3 8' }),
    h('rect', { x: '1', y: '3', width: '22', height: '5', rx: '1' }),
    h('line', { x1: '10', y1: '12', x2: '14', y2: '12' })
  ])
}

const SearchIcon = {
  render: () => h('svg', { width: '28', height: '28', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '1.5', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('circle', { cx: '11', cy: '11', r: '8' }),
    h('line', { x1: '21', y1: '21', x2: '16.65', y2: '16.65' })
  ])
}

const ErrorIcon = {
  render: () => h('svg', { width: '28', height: '28', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '1.5', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('circle', { cx: '12', cy: '12', r: '10' }),
    h('line', { x1: '12', y1: '8', x2: '12', y2: '12' }),
    h('line', { x1: '12', y1: '16', x2: '12.01', y2: '16' })
  ])
}

const iconComponent = computed(() => {
  if (props.variant === 'error') return ErrorIcon
  if (props.variant === 'no-result') return SearchIcon
  return InboxIcon
})
</script>
