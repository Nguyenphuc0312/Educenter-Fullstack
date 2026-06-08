<template>
  <div class="flex items-center gap-1">
    <svg
      v-for="i in 5"
      :key="i"
      xmlns="http://www.w3.org/2000/svg"
      :width="size"
      :height="size"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
      :class="i <= full ? 'text-amber-400 fill-amber-400' : 'text-slate-300 dark:text-slate-600'"
    >
      <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/>
    </svg>
    <span v-if="showValue" class="text-sm font-semibold text-amber-500 ml-1">
      {{ rating.toFixed(1) }}
    </span>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  rating: {
    type: Number,
    required: true
  },
  showValue: {
    type: Boolean,
    default: true
  },
  size: {
    type: Number,
    default: 14
  }
})

const full = computed(() => Math.floor(props.rating))
</script>
