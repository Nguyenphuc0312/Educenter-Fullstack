<template>
  <a-modal
    v-model:open="isOpen"
    :title="null"
    :footer="null"
    :width="440"
    :centered="true"
    :destroy-on-close="true"
  >
    <div class="p-2 text-center">
      <!-- Icon Container -->
      <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full mb-4" :class="colorClasses.bg">
        <component :is="iconComponent" class="text-xl" :class="colorClasses.text" />
      </div>
      
      <!-- Content -->
      <h3 class="text-base font-semibold text-slate-900 dark:text-slate-100 mb-2">
        {{ title }}
      </h3>
      <p class="text-xs text-slate-500 dark:text-slate-400 mb-6 leading-relaxed">
        {{ message }}
      </p>
      
      <!-- Actions -->
      <div class="flex items-center justify-center gap-3">
        <a-button size="small" class="px-4 text-xs h-8" @click="isOpen = false">
          {{ cancelText }}
        </a-button>
        <a-button
          size="small"
          :type="type === 'danger' ? 'primary' : 'primary'"
          :danger="type === 'danger'"
          :loading="loading"
          class="px-4 text-xs h-8"
          :class="type === 'success' ? '!bg-emerald-600 !border-emerald-600 hover:!bg-emerald-700' : ''"
          @click="handleConfirm"
        >
          {{ okText }}
        </a-button>
      </div>
    </div>
  </a-modal>
</template>

<script setup>
import { computed } from 'vue'
import {
  ExclamationCircleOutlined,
  WarningOutlined,
  InfoCircleOutlined,
  CheckCircleOutlined
} from '@ant-design/icons-vue'

const props = defineProps({
  open: {
    type: Boolean,
    required: true
  },
  title: {
    type: String,
    required: true
  },
  message: {
    type: String,
    required: true
  },
  type: {
    type: String,
    default: 'warning' // 'danger' | 'warning' | 'info' | 'success'
  },
  okText: {
    type: String,
    default: 'Xác nhận'
  },
  cancelText: {
    type: String,
    default: 'Hủy'
  },
  loading: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:open', 'confirm'])

const isOpen = computed({
  get: () => props.open,
  set: (val) => emit('update:open', val)
})

const iconComponent = computed(() => {
  switch (props.type) {
    case 'danger':
      return ExclamationCircleOutlined
    case 'success':
      return CheckCircleOutlined
    case 'info':
      return InfoCircleOutlined
    case 'warning':
    default:
      return WarningOutlined
  }
})

const colorClasses = computed(() => {
  switch (props.type) {
    case 'danger':
      return {
        bg: 'bg-red-50 dark:bg-red-950/20',
        text: 'text-red-600 dark:text-red-400'
      }
    case 'success':
      return {
        bg: 'bg-emerald-50 dark:bg-emerald-950/20',
        text: 'text-emerald-600 dark:text-emerald-400'
      }
    case 'info':
      return {
        bg: 'bg-blue-50 dark:bg-blue-950/20',
        text: 'text-blue-600 dark:text-blue-400'
      }
    case 'warning':
    default:
      return {
        bg: 'bg-amber-50 dark:bg-amber-950/20',
        text: 'text-amber-600 dark:text-amber-400'
      }
  }
})

function handleConfirm() {
  emit('confirm')
}
</script>
