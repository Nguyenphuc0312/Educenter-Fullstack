<template>
  <div :class="['flex flex-col items-center justify-center text-center p-8 border border-red-100 dark:border-red-950/20 rounded-2xl bg-red-50/50 dark:bg-red-950/10', className]">
    <div class="w-12 h-12 rounded-xl bg-red-100 dark:bg-red-900/20 flex items-center justify-center text-red-600 dark:text-red-400 mb-3">
      <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="12" cy="12" r="10"/>
        <line x1="12" y1="8" x2="12" y2="12"/>
        <line x1="12" y1="16" x2="12.01" y2="16"/>
      </svg>
    </div>
    <h3 class="text-sm font-semibold text-red-800 dark:text-red-400 mb-1">{{ title }}</h3>
    <p class="text-xs text-red-600 dark:text-red-500/80 max-w-xs mb-4">{{ description }}</p>
    <button
      v-if="onRetry"
      @click="onRetry"
      class="px-4 py-2 text-xs font-semibold text-white bg-red-600 dark:bg-red-500 rounded-lg hover:opacity-90 transition-opacity cursor-pointer"
    >
      Thử lại
    </button>
  </div>
</template>

<script setup>
defineProps({
  title: {
    type: String,
    default: 'Đã xảy ra lỗi'
  },
  description: {
    type: String,
    default: 'Không thể tải dữ liệu. Vui lòng thử lại sau.'
  },
  onRetry: {
    type: Function,
    default: null
  },
  className: {
    type: String,
    default: ''
  }
})
</script>
