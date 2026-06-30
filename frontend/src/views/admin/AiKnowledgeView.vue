<template>
  <section class="ai-page">
    <div class="ai-hero">
      <div>
        <p class="eyebrow">EduCenter AI</p>
        <h1>Quản trị tài liệu & tự động hóa AI</h1>
        <p class="subtitle">
          Nạp tài liệu RAG theo quyền truy cập, tạo báo cáo nhanh, soạn email nháp bằng AI và chỉ gửi sau khi Admin xác nhận.
        </p>
      </div>
      <a-button :loading="loadingDocs" @click="loadAll">Làm mới</a-button>
    </div>

    <div class="ai-grid">
      <a-card title="Nạp tài liệu cho chatbot">
        <div class="form-stack">
          <a-select v-model:value="scope" :options="scopeOptions" />
          <a-select v-model:value="audience" :options="roleOptions" />
          <a-input
            v-if="scope === 'Account'"
            v-model:value="ownerReferenceId"
            placeholder="Reference ID tài khoản cụ thể"
          />
          <a-input
            v-if="scope === 'Class'"
            v-model:value="classId"
            placeholder="Class ID"
          />
          <a-upload :before-upload="upload" :show-upload-list="false" accept=".pdf,.docx,.txt,.md">
            <a-button type="primary" :loading="uploading">Chọn PDF/DOCX/TXT/MD</a-button>
          </a-upload>
          <p class="hint">Scope quyết định ai được tra cứu: toàn hệ thống, theo role, theo tài khoản hoặc theo lớp.</p>
        </div>
      </a-card>

      <a-card title="Tạo báo cáo AI">
        <div class="form-stack">
          <a-textarea
            v-model:value="reportInstruction"
            :rows="5"
            placeholder="Ví dụ: Tổng hợp tình hình học viên, điểm danh và kết quả học tập tháng này"
          />
          <div class="actions">
            <a-button type="primary" :loading="reporting" @click="createReport">Tạo báo cáo</a-button>
            <a-button :disabled="!report" @click="downloadReport('md')">Tải Markdown</a-button>
            <a-button :disabled="!report" @click="downloadReport('csv')">Tải CSV</a-button>
          </div>
        </div>
      </a-card>
    </div>

    <a-card v-if="report" class="report-card" title="Bản báo cáo AI">
      <pre>{{ report.markdown }}</pre>
    </a-card>

    <div class="ai-grid">
      <a-card title="Soạn email nháp">
        <div class="form-stack">
          <a-input v-model:value="recipients" placeholder="email1@...; email2@..." />
          <a-textarea
            v-model:value="instruction"
            :rows="5"
            placeholder="Ví dụ: Soạn thông báo nghỉ học do mưa lớn, giọng văn lịch sự"
          />
          <a-button type="primary" :loading="drafting" @click="createDraft">Soạn nháp bằng AI</a-button>
          <p class="hint">AI chỉ tạo nháp. Email chưa được gửi cho đến khi Admin bấm xác nhận.</p>
        </div>
      </a-card>

      <a-card title="Tài liệu đã cấp cho AI">
        <a-table :data-source="docs" :pagination="{ pageSize: 5 }" row-key="id" size="small">
          <a-table-column title="Tên tài liệu" data-index="title" />
          <a-table-column title="Scope" data-index="scope" />
          <a-table-column title="Role" data-index="audienceRole" />
        </a-table>
      </a-card>
    </div>

    <a-card title="Email nháp chờ xác nhận">
      <a-table :data-source="drafts" :pagination="{ pageSize: 5 }" row-key="id">
        <a-table-column title="Người nhận" data-index="recipients" />
        <a-table-column title="Tiêu đề" data-index="subject" />
        <a-table-column title="Trạng thái" data-index="status" />
        <a-table-column title="Thao tác">
          <template #default="{ record }">
            <a-popconfirm
              v-if="record.status === 'Draft'"
              title="Xác nhận gửi email này?"
              ok-text="Gửi"
              cancel-text="Hủy"
              @confirm="confirmDraft(record.id)"
            >
              <a-button type="primary">Xác nhận gửi</a-button>
            </a-popconfirm>
            <span v-else class="sent-text">Đã xử lý</span>
          </template>
        </a-table-column>
      </a-table>
    </a-card>
  </section>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import { callApi, http } from '@/api/httpClient'

const docs = ref([])
const drafts = ref([])
const report = ref(null)
const scope = ref('Role')
const audience = ref('All')
const ownerReferenceId = ref('')
const classId = ref('')
const recipients = ref('')
const instruction = ref('')
const reportInstruction = ref('Tổng hợp nhanh tình hình học viên, ghi danh, điểm danh và kết quả học tập hiện tại.')
const loadingDocs = ref(false)
const uploading = ref(false)
const drafting = ref(false)
const reporting = ref(false)

const scopeOptions = [
  { value: 'Global', label: 'Toàn hệ thống' },
  { value: 'Role', label: 'Theo role' },
  { value: 'Account', label: 'Theo tài khoản' },
  { value: 'Class', label: 'Theo lớp' }
]

const roleOptions = [
  { value: 'All', label: 'Mọi role' },
  { value: 'Admin', label: 'Admin' },
  { value: 'Teacher', label: 'Giảng viên' },
  { value: 'Student', label: 'Học viên' }
]

async function loadDocs() {
  docs.value = await callApi(http.get('/gateway/ai/knowledge'))
}

async function loadDrafts() {
  drafts.value = await callApi(http.get('/gateway/ai/knowledge/email-drafts'))
}

async function loadAll() {
  loadingDocs.value = true
  try {
    await Promise.all([loadDocs(), loadDrafts()])
  } finally {
    loadingDocs.value = false
  }
}

async function upload(file) {
  uploading.value = true
  try {
    const data = new FormData()
    data.append('file', file)
    data.append('scope', scope.value)
    data.append('audienceRole', audience.value)
    if (scope.value === 'Account' && ownerReferenceId.value) data.append('ownerReferenceId', ownerReferenceId.value)
    if (scope.value === 'Class' && classId.value) data.append('classId', classId.value)
    await callApi(http.post('/gateway/ai/knowledge/upload', data, { headers: { 'Content-Type': 'multipart/form-data' } }))
    message.success('Đã nạp tài liệu vào RAG')
    await loadDocs()
  } finally {
    uploading.value = false
  }
  return false
}

async function createDraft() {
  if (!recipients.value || !instruction.value) {
    message.warning('Nhập người nhận và yêu cầu soạn email')
    return
  }
  drafting.value = true
  try {
    await callApi(http.post('/gateway/ai/knowledge/email-drafts', {
      recipients: recipients.value,
      instruction: instruction.value
    }))
    message.success('Đã tạo email nháp')
    await loadDrafts()
  } finally {
    drafting.value = false
  }
}

async function confirmDraft(id) {
  await callApi(http.post(`/gateway/ai/knowledge/email-drafts/${id}/confirm`))
  message.success('Đã gửi email')
  await loadDrafts()
}

async function createReport() {
  if (!reportInstruction.value) {
    message.warning('Nhập yêu cầu báo cáo')
    return
  }
  reporting.value = true
  try {
    report.value = await callApi(http.post('/gateway/ai/knowledge/reports', {
      instruction: reportInstruction.value
    }))
    message.success('Đã tạo báo cáo AI')
  } finally {
    reporting.value = false
  }
}

function downloadReport(type) {
  if (!report.value) return
  const content = type === 'csv' ? report.value.csv : report.value.markdown
  const mime = type === 'csv' ? 'text/csv;charset=utf-8' : 'text/markdown;charset=utf-8'
  const blob = new Blob([content], { type: mime })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = type === 'csv' ? 'educenter-ai-report.csv' : 'educenter-ai-report.md'
  link.click()
  URL.revokeObjectURL(url)
}

onMounted(loadAll)
</script>

<style scoped>
.ai-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.ai-hero {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  align-items: flex-start;
  padding: 24px;
  border: 1px solid rgba(148, 163, 184, 0.25);
  border-radius: 24px;
  background: linear-gradient(135deg, rgba(79, 70, 229, 0.10), rgba(14, 165, 233, 0.08));
}

.eyebrow {
  margin: 0 0 6px;
  color: #4f46e5;
  font-size: 12px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

h1 {
  margin: 0;
  color: #0f172a;
  font-size: 28px;
  font-weight: 900;
}

.subtitle {
  max-width: 780px;
  margin: 8px 0 0;
  color: #64748b;
  line-height: 1.6;
}

.ai-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 20px;
}

.form-stack {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.hint {
  margin: 0;
  color: #64748b;
  font-size: 12px;
}

.report-card pre {
  max-height: 360px;
  margin: 0;
  padding: 16px;
  overflow: auto;
  white-space: pre-wrap;
  border-radius: 14px;
  background: #0f172a;
  color: #e2e8f0;
}

.sent-text {
  color: #64748b;
  font-weight: 600;
}

@media (max-width: 1024px) {
  .ai-grid {
    grid-template-columns: 1fr;
  }

  .ai-hero {
    flex-direction: column;
  }
}
</style>
