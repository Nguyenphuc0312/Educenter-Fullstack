<template>
  <div class="flex items-center gap-1.5 justify-end">
    <!-- View Button (Optional, usually for details) -->
    <a-button
      v-if="showView"
      size="small"
      type="text"
      class="h-7 w-7 flex items-center justify-center hover:bg-slate-100 dark:hover:bg-slate-800 rounded-md"
      @click="$emit('view', record)"
      title="Chi tiết"
    >
      <template #icon><EyeOutlined class="text-slate-500 dark:text-slate-400 text-[13px]" /></template>
    </a-button>

    <!-- Edit Button (Optional) -->
    <a-button
      v-if="showEdit"
      size="small"
      type="text"
      class="h-7 w-7 flex items-center justify-center hover:bg-slate-100 dark:hover:bg-slate-800 rounded-md"
      @click="$emit('edit', record)"
      title="Chỉnh sửa"
    >
      <template #icon><EditOutlined class="text-blue-500 text-[13px]" /></template>
    </a-button>

    <!-- More Actions Dropdown -->
    <a-dropdown v-if="hasMoreActions" trigger="click" placement="bottomRight">
      <a-button
        size="small"
        type="text"
        class="h-7 w-7 flex items-center justify-center hover:bg-slate-100 dark:hover:bg-slate-800 rounded-md"
        title="Thao tác khác"
      >
        <template #icon><MoreOutlined class="text-slate-600 dark:text-slate-300 text-[14px]" /></template>
      </a-button>
      <template #overlay>
        <a-menu class="min-w-[140px] shadow-md rounded-lg p-1 dark:bg-slate-900 border border-slate-100 dark:border-slate-800">
          <!-- Custom Extra Actions Slot -->
          <slot name="extra" :record="record" />

          <!-- Render from actions array prop -->
          <template v-for="act in computedActions" :key="act.key">
            <a-menu-divider v-if="act.divider" />
            <a-menu-item
              v-else
              :disabled="act.disabled"
              :danger="act.danger"
              class="rounded-md px-3 py-1.5 text-xs"
              @click="handleActionClick(act)"
            >
              <div class="flex items-center gap-2">
                <component :is="act.icon" v-if="act.icon" class="text-[12px]" />
                <span>{{ act.label }}</span>
              </div>
            </a-menu-item>
          </template>

          <!-- Standard Delete Action inside dropdown if showDelete is true -->
          <template v-if="showDelete">
            <a-menu-divider />
            <a-menu-item danger class="rounded-md px-3 py-1.5 text-xs p-0">
              <a-popconfirm
                :title="deleteConfirmText"
                ok-text="Xóa"
                cancel-text="Hủy"
                ok-type="danger"
                placement="left"
                @confirm="$emit('delete', record)"
              >
                <div class="flex items-center gap-2 w-full h-full px-3 py-1.5">
                  <DeleteOutlined class="text-[12px]" />
                  <span>Xóa</span>
                </div>
              </a-popconfirm>
            </a-menu-item>
          </template>
        </a-menu>
      </template>
    </a-dropdown>
  </div>
</template>

<script setup>
import { computed, useSlots } from 'vue'
import {
  EditOutlined,
  DeleteOutlined,
  MoreOutlined,
  EyeOutlined,
} from '@ant-design/icons-vue'

const props = defineProps({
  record: {
    type: Object,
    required: true
  },
  showView: {
    type: Boolean,
    default: false
  },
  showEdit: {
    type: Boolean,
    default: false
  },
  showDelete: {
    type: Boolean,
    default: false
  },
  deleteConfirmText: {
    type: String,
    default: 'Xóa bản ghi này?'
  },
  actions: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['view', 'edit', 'delete', 'action'])
const slots = useSlots()

const computedActions = computed(() => {
  return props.actions
})

const hasMoreActions = computed(() => {
  return props.showDelete || props.actions.length > 0 || !!slots.extra
})

function handleActionClick(act) {
  if (act.onClick) {
    act.onClick(props.record)
  } else {
    emit('action', { key: act.key, record: props.record })
  }
}
</script>

<style scoped>
:deep(.ant-dropdown-menu) {
  padding: 4px !important;
}
:deep(.ant-dropdown-menu-item) {
  padding: 6px 12px !important;
}
:deep(.ant-dropdown-menu-item button),
:deep(.ant-dropdown-menu-item .ant-btn) {
  width: 100% !important;
  text-align: left !important;
  display: flex !important;
  align-items: center !important;
  justify-content: flex-start !important;
  height: auto !important;
  padding: 0 !important;
  border: none !important;
  background: transparent !important;
  box-shadow: none !important;
  color: inherit !important;
  font-size: 12px !important;
}
</style>
