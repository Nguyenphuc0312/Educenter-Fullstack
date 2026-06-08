<template>
  <div :class="['w-full bg-slate-100 dark:bg-slate-700 rounded-full h-2 overflow-hidden', className]">
    <div
      :class="['h-full rounded-full transition-all duration-700', colorClass]"
      :style="{ width: `${clamped}%` }"
    />
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  value: {
    type: Number,
    required: true
  },
  className: {
    type: String,
    default: ''
  },
  colorClass: {
    type: String,
    default: 'bg-blue-500'
  }
})

const clamped = computed(() => Math.min(100, Math.max(0, props.value)))
</script>
