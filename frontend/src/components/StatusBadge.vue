<template>
  <span :class="[className, 'text-xs font-semibold px-2.5 py-1 rounded-full']">
    {{ label }}
  </span>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  status: {
    type: String,
    required: true
  }
})

const config = {
  selling: { label: 'Đang mở bán', className: 'badge-selling' },
  upcoming: { label: 'Sắp khai giảng', className: 'badge-upcoming' },
  few_seats: { label: 'Còn ít chỗ', className: 'badge-few' }
}

const label = computed(() => (config[props.status] || config.selling).label)
const className = computed(() => (config[props.status] || config.selling).className)
</script>
