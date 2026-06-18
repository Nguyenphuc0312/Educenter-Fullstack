<template>
  <div class="flex items-center gap-1.5 justify-end">
    <!-- View Button (Optional, usually for details) -->
    <a-button
      v-if="showView"
      size="small"
      type="text"
      class="action-icon-btn"
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
      class="action-icon-btn action-icon-btn-edit"
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
        class="action-icon-btn"
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
  showExtra: {
    type: Boolean,
    default: true
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
  return props.showDelete || props.actions.length > 0 || (props.showExtra && !!slots.extra)
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
.action-icon-btn {
  width: 30px !important;
  height: 30px !important;
  display: inline-flex !important;
  align-items: center !important;
  justify-content: center !important;
  padding: 0 !important;
  border: 1px solid var(--admin-border, #e2e8f0) !important;
  border-radius: 8px !important;
  background: var(--admin-surface-2, #f8fafc) !important;
  color: var(--admin-text-muted, #64748b) !important;
  box-shadow: 0 1px 2px rgba(15, 23, 42, 0.04) !important;
}
.action-icon-btn:hover {
  border-color: var(--admin-accent, #4f46e5) !important;
  background: var(--admin-accent-soft, #eef2ff) !important;
  color: var(--admin-accent, #4f46e5) !important;
}
.action-icon-btn :deep(.ant-btn-icon) {
  display: inline-flex !important;
  align-items: center !important;
  justify-content: center !important;
  margin: 0 !important;
}
.action-icon-btn :deep(svg) {
  width: 14px;
  height: 14px;
}
.dark .action-icon-btn {
  background: #0b1728 !important;
  border-color: #263653 !important;
  color: #cbd5e1 !important;
  box-shadow: none !important;
}
.dark .action-icon-btn:hover {
  background: rgba(59, 130, 246, 0.16) !important;
  border-color: #60a5fa !important;
  color: #bfdbfe !important;
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
