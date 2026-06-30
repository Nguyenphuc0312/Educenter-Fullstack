<template>
  <div class="text-center py-4">
    <div
      class="mx-auto w-12 h-12 rounded-full flex items-center justify-center mb-3"
      style="background: var(--admin-surface-2); color: var(--admin-text-subtle);"
    >
      <svg
        width="22"
        height="22"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        stroke-width="1.5"
        stroke-linecap="round"
        stroke-linejoin="round"
      >
        <line x1="12" y1="20" x2="12" y2="10" />
        <line x1="18" y1="20" x2="18" y2="4" />
        <line x1="6" y1="20" x2="6" y2="16" />
      </svg>
    </div>
    <p class="text-sm font-semibold" style="color: var(--admin-text);">
      {{ title }}
    </p>
    <p v-if="description" class="text-xs mt-1" style="color: var(--admin-text-muted);">
      {{ description }}
    </p>
  </div>
</template>

<script setup>
defineProps({
  title: {
    type: String,
    default: 'Chưa có dữ liệu'
  },
  description: {
    type: String,
    default: ''
  },
  showActionButton: {
    type: Boolean,
    default: false
  }
})
</script>
