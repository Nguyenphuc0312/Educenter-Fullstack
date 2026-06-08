<template>
  <div :class="['flex items-center justify-center p-4', className]">
    <div :class="['animate-spin rounded-full border-t-blue-600 dark:border-t-blue-400 border-slate-200 dark:border-slate-800', sizeClasses[size] || sizeClasses.md]"></div>
  </div>
</template>

<script setup>
defineProps({
  size: {
    type: String,
    default: 'md'
  },
  className: {
    type: String,
    default: ''
  }
})

const sizeClasses = {
  sm: 'w-5 h-5 border-2',
  md: 'w-8 h-8 border-3',
  lg: 'w-12 h-12 border-4'
}
</script>
