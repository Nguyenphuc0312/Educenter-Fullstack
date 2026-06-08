<template>
  <div class="space-y-6">
    <PageHeader :title="title" :subtitle="subtitle" />
    <section class="bg-card-base border border-base rounded-2xl shadow-sm overflow-hidden">
      <a-table
        :data-source="items"
        :columns="columns"
        :loading="loading"
        row-key="id"
        size="small"
        :pagination="{ pageSize: 8, showSizeChanger: false }"
        :scroll="{ x: 800 }"
      >
        <template #emptyText>
          <div class="py-10 text-center">
            <p class="text-sm font-semibold text-base-primary">Chưa có dữ liệu</p>
            <p class="text-xs text-base-secondary mt-1">Dữ liệu sẽ hiển thị khi hệ thống đồng bộ từ API.</p>
          </div>
        </template>
      </a-table>
    </section>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import PageHeader from '@/components/ui/PageHeader.vue'

const props = defineProps({
  title: { type: String, required: true },
  subtitle: { type: String, default: '' },
  columns: { type: Array, default: () => [] },
  loader: { type: Function, required: true },
})

const items = ref([])
const loading = ref(false)

onMounted(async () => {
  loading.value = true
  try {
    const data = await props.loader()
    items.value = Array.isArray(data) ? data : data?.items || []
  } catch (error) {
    message.error(error.message || 'Không tải được dữ liệu')
  } finally {
    loading.value = false
  }
})
</script>
