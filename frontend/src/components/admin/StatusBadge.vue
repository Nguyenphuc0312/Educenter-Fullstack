<template>
  <span
    :class="[
      'inline-flex items-center px-2.5 py-1 rounded-full text-[11px] font-semibold border transition-colors gap-1.5 whitespace-nowrap',
      badgeClass
    ]"
  >
    <span v-if="dot" class="w-1.5 h-1.5 rounded-full" :class="dotClass"></span>
    <slot>{{ computedLabel }}</slot>
  </span>
</template>

<script setup>
import { computed } from 'vue'
import { STATUS_LOCALES } from '@/lib/constants'

const props = defineProps({
  value: {
    type: [String, Number, Boolean],
    default: null
  },
  label: {
    type: String,
    default: ''
  },
  color: {
    type: String,
    default: ''
  },
  options: {
    type: Array,
    default: () => []
  },
  dot: {
    type: Boolean,
    default: false
  }
})

const matchedOption = computed(() => {
  if (props.value === null || props.value === undefined) return null
  return props.options.find(
    (opt) => String(opt.value) === String(props.value)
  )
})

const computedLabel = computed(() => {
  if (props.label) return props.label
  if (matchedOption.value) {
    return matchedOption.value.text || matchedOption.value.label
  }
  // Fallback: nếu value là string tiếng Anh, tra STATUS_LOCALES để Việt hóa
  if (props.value !== null && props.value !== undefined) {
    const s = String(props.value)
    if (s && STATUS_LOCALES[s]) {
      return STATUS_LOCALES[s].label
    }
    return s
  }
  return ''
})

const resolvedColor = computed(() => {
  if (props.color) return props.color
  if (matchedOption.value && matchedOption.value.color) {
    return matchedOption.value.color
  }
  // Fallback color: nếu value là string tiếng Anh, lấy color từ STATUS_LOCALES
  if (props.value !== null && props.value !== undefined) {
    const s = String(props.value)
    if (s && STATUS_LOCALES[s]) {
      return STATUS_LOCALES[s].color
    }
  }
  return 'blue'
})

const badgeClass = computed(() => {
  const c = resolvedColor.value.toLowerCase()
  switch (c) {
    case 'green':
    case 'success':
    case 'completed':
    case 'paid':
    case 'present':
      return 'bg-emerald-50 text-emerald-700 border-emerald-200/60 dark:bg-emerald-950/20 dark:text-emerald-400 dark:border-emerald-900/40'
    case 'red':
    case 'error':
    case 'absent':
    case 'failed':
    case 'overdue':
      return 'bg-rose-50 text-rose-700 border-rose-200/60 dark:bg-rose-950/20 dark:text-rose-400 dark:border-rose-900/40'
    case 'orange':
    case 'warning':
    case 'unpaid':
      return 'bg-amber-50 text-amber-700 border-amber-200/60 dark:bg-amber-950/20 dark:text-amber-400 dark:border-amber-900/40'
    case 'blue':
    case 'processing':
    case 'opening':
    case 'scheduled':
      return 'bg-blue-50 text-blue-700 border-blue-200/60 dark:bg-blue-950/20 dark:text-blue-400 dark:border-blue-900/40'
    case 'purple':
      return 'bg-purple-50 text-purple-700 border-purple-200/60 dark:bg-purple-950/20 dark:text-purple-400 dark:border-purple-900/40'
    case 'indigo':
    case 'inprogress':
      return 'bg-indigo-50 text-indigo-700 border-indigo-200/60 dark:bg-indigo-950/20 dark:text-indigo-400 dark:border-indigo-900/40'
    case 'amber':
    case 'late':
    case 'partial':
      return 'bg-amber-100 text-amber-800 border-amber-200/60 dark:bg-amber-950/30 dark:text-amber-300 dark:border-amber-900/40'
    case 'rose':
    case 'locked':
      return 'bg-rose-100 text-rose-800 border-rose-200/60 dark:bg-rose-950/30 dark:text-rose-300 dark:border-rose-900/40'
    case 'gray':
    case 'slate':
    case 'cancelled':
    case 'inactive':
    default:
      return 'bg-slate-100 text-slate-700 border-slate-200/60 dark:bg-slate-800/40 dark:text-slate-400 dark:border-slate-700/40'
  }
})

const dotClass = computed(() => {
  const c = resolvedColor.value.toLowerCase()
  switch (c) {
    case 'green':
    case 'success':
    case 'completed':
    case 'paid':
    case 'present':
      return 'bg-emerald-500'
    case 'red':
    case 'error':
    case 'absent':
    case 'failed':
    case 'overdue':
      return 'bg-rose-500'
    case 'orange':
    case 'warning':
    case 'unpaid':
      return 'bg-amber-500'
    case 'blue':
    case 'processing':
    case 'opening':
    case 'scheduled':
      return 'bg-blue-500'
    case 'purple':
      return 'bg-purple-500'
    case 'indigo':
    case 'inprogress':
      return 'bg-indigo-500'
    case 'amber':
    case 'late':
    case 'partial':
      return 'bg-amber-400'
    case 'rose':
    case 'locked':
      return 'bg-rose-500'
    case 'gray':
    case 'slate':
    case 'cancelled':
    case 'inactive':
    default:
      return 'bg-slate-400'
  }
})
</script>
