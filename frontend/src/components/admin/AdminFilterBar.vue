<template>
  <div class="bg-card-base border border-base rounded-xl p-3 shadow-sm transition-all">
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-3">
      <!-- Search & Custom Filters Slot -->
      <div class="flex items-center gap-2.5 flex-wrap flex-1">
        <a-input-search
          v-if="showSearch"
          :value="searchText"
          @input="$emit('update:searchText', $event.target.value)"
          @search="$emit('search', $event)"
          :placeholder="searchPlaceholder"
          allow-clear
          size="small"
          class="max-w-xs w-full md:w-64"
        />
        
        <!-- Custom Filter Inputs slot -->
        <slot />

        <a-button size="small" class="flex items-center gap-1" @click="$emit('reset')">
          <template #icon><ReloadOutlined /></template>
          Đặt lại
        </a-button>
      </div>

      <!-- Actions slot (optional) -->
      <div v-if="$slots.actions" class="flex items-center gap-2 flex-wrap justify-end">
        <slot name="actions" />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ReloadOutlined } from '@ant-design/icons-vue'

defineProps({
  searchText: {
    type: String,
    default: ''
  },
  searchPlaceholder: {
    type: String,
    default: 'Tìm kiếm...'
  },
  showSearch: {
    type: Boolean,
    default: true
  }
})

defineEmits(['update:searchText', 'search', 'reset'])
</script>
