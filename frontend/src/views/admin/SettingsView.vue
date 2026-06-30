<template>
  <div class="settings-page">
    <header class="mb-6">
      <h1 class="text-2xl font-bold text-base-primary">Cấu hình</h1>
      <p class="mt-1 text-sm text-base-secondary">
        Quản lý thông tin kết nối và kênh thông báo của hệ thống.
      </p>
    </header>

    <a-spin :spinning="loading">
      <section class="settings-panel">
        <div class="settings-panel__header">
          <div class="settings-icon"><MailOutlined /></div>
          <div class="min-w-0">
            <div class="flex flex-wrap items-center gap-2">
              <h2 class="text-base font-bold text-base-primary">Thông báo Gmail</h2>
              <span
                class="status-badge"
                :class="form.hasAppPassword ? 'status-badge--ready' : 'status-badge--warning'"
              >
                <CheckCircleOutlined v-if="form.hasAppPassword" />
                <ExclamationCircleOutlined v-else />
                {{ form.hasAppPassword ? 'Đã cấu hình' : 'Chưa có app password' }}
              </span>
            </div>
            <p class="mt-1 text-xs text-base-secondary">
              Gmail này được dùng làm địa chỉ gửi thông báo chính của EduCenter.
            </p>
          </div>
        </div>

        <a-form layout="vertical" class="settings-form" @finish="saveSettings">
          <div class="settings-grid">
            <a-form-item
              label="Gmail gửi thông báo"
              name="fromEmail"
              :rules="[
                { required: true, message: 'Vui lòng nhập Gmail' },
                { type: 'email', message: 'Địa chỉ email không hợp lệ' },
              ]"
            >
              <a-input v-model:value="form.fromEmail" size="large" placeholder="example@gmail.com">
                <template #prefix><MailOutlined class="field-icon" /></template>
              </a-input>
            </a-form-item>

            <a-form-item
              label="Tên người gửi"
              name="fromName"
              :rules="[{ required: true, message: 'Vui lòng nhập tên người gửi' }]"
            >
              <a-input v-model:value="form.fromName" size="large" placeholder="EduCenter">
                <template #prefix><UserOutlined class="field-icon" /></template>
              </a-input>
            </a-form-item>

            <a-form-item label="Gmail app password" name="appPassword">
              <a-input-password
                v-model:value="form.appPassword"
                size="large"
                :placeholder="form.hasAppPassword ? 'Để trống nếu không thay đổi' : 'Nhập app password'"
                autocomplete="new-password"
              >
                <template #prefix><KeyOutlined class="field-icon" /></template>
              </a-input-password>
              <div class="mt-1.5 text-[11px] text-base-muted">
                {{ form.hasAppPassword ? 'Khóa hiện tại đã được mã hóa và lưu an toàn.' : 'Cần app password để gửi email.' }}
              </div>
            </a-form-item>

            <a-form-item
              label="Máy chủ SMTP"
              name="smtpHost"
              :rules="[{ required: true, message: 'Vui lòng nhập máy chủ SMTP' }]"
            >
              <a-input v-model:value="form.smtpHost" size="large" placeholder="smtp.gmail.com">
                <template #prefix><CloudServerOutlined class="field-icon" /></template>
              </a-input>
            </a-form-item>

            <a-form-item
              label="Cổng SMTP"
              name="smtpPort"
              :rules="[{ required: true, message: 'Vui lòng nhập cổng SMTP' }]"
            >
              <a-input-number
                v-model:value="form.smtpPort"
                size="large"
                :min="1"
                :max="65535"
                class="w-full"
              />
            </a-form-item>

            <a-form-item label="Bảo mật kết nối">
              <div class="ssl-control">
                <div>
                  <div class="text-sm font-semibold text-base-primary">Sử dụng SSL/TLS</div>
                  <div class="text-xs text-base-muted">Khuyến nghị bật khi sử dụng Gmail SMTP.</div>
                </div>
                <a-switch v-model:checked="form.enableSsl" />
              </div>
            </a-form-item>
          </div>

          <div class="settings-actions">
            <div class="text-xs text-base-muted">
              Cập nhật gần nhất: {{ formatDateTime(form.updatedAt) }}
            </div>
            <a-button type="primary" html-type="submit" size="large" :loading="saving">
              <template #icon><SaveOutlined /></template>
              Lưu cấu hình
            </a-button>
          </div>
        </a-form>
      </section>

      <section class="settings-panel mt-5">
        <div class="settings-panel__header settings-panel__header--compact">
          <div class="settings-icon settings-icon--green"><SendOutlined /></div>
          <div>
            <h2 class="text-base font-bold text-base-primary">Kiểm tra gửi email</h2>
            <p class="mt-1 text-xs text-base-secondary">
              Gửi một email mẫu để xác nhận cấu hình SMTP đang hoạt động.
            </p>
          </div>
        </div>

        <div class="test-email-row">
          <a-input
            v-model:value="testEmail"
            size="large"
            placeholder="Email nhận thử"
            @press-enter="sendTestEmail"
          >
            <template #prefix><MailOutlined class="field-icon" /></template>
          </a-input>
          <a-button
            type="primary"
            ghost
            size="large"
            :loading="testing"
            :disabled="!form.hasAppPassword"
            @click="sendTestEmail"
          >
            <template #icon><SendOutlined /></template>
            Gửi thử
          </a-button>
        </div>
      </section>
    </a-spin>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue'
import { message } from 'ant-design-vue'
import {
  CheckCircleOutlined,
  CloudServerOutlined,
  ExclamationCircleOutlined,
  KeyOutlined,
  MailOutlined,
  SaveOutlined,
  SendOutlined,
  UserOutlined,
} from '@ant-design/icons-vue'
import { settingsApi } from '@/api/settingsApi'

const loading = ref(false)
const saving = ref(false)
const testing = ref(false)
const testEmail = ref('')

const form = reactive({
  fromEmail: '',
  fromName: 'EduCenter',
  smtpHost: 'smtp.gmail.com',
  smtpPort: 587,
  enableSsl: true,
  appPassword: '',
  hasAppPassword: false,
  updatedAt: null,
})

function applySettings(data) {
  form.fromEmail = data?.fromEmail || ''
  form.fromName = data?.fromName || 'EduCenter'
  form.smtpHost = data?.smtpHost || 'smtp.gmail.com'
  form.smtpPort = Number(data?.smtpPort || 587)
  form.enableSsl = data?.enableSsl !== false
  form.hasAppPassword = Boolean(data?.hasAppPassword)
  form.updatedAt = data?.updatedAt || null
  form.appPassword = ''
  if (!testEmail.value) testEmail.value = form.fromEmail
}

async function loadSettings() {
  loading.value = true
  try {
    applySettings(await settingsApi.getNotificationEmail())
  } catch (error) {
    message.error(error.message || 'Không thể tải cấu hình Gmail')
  } finally {
    loading.value = false
  }
}

async function saveSettings() {
  saving.value = true
  try {
    const data = await settingsApi.updateNotificationEmail({
      fromEmail: form.fromEmail.trim(),
      fromName: form.fromName.trim(),
      smtpHost: form.smtpHost.trim(),
      smtpPort: Number(form.smtpPort),
      enableSsl: form.enableSsl,
      appPassword: form.appPassword || null,
    })
    applySettings(data)
    message.success('Đã lưu cấu hình Gmail')
  } catch (error) {
    message.error(error.message || 'Không thể lưu cấu hình Gmail')
  } finally {
    saving.value = false
  }
}

async function sendTestEmail() {
  const recipient = testEmail.value.trim()
  if (!recipient || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(recipient)) {
    message.warning('Vui lòng nhập email nhận hợp lệ')
    return
  }

  testing.value = true
  try {
    await settingsApi.sendTestEmail({ toEmail: recipient })
    message.success(`Đã gửi email kiểm tra tới ${recipient}`)
  } catch (error) {
    message.error(error.message || 'Không thể gửi email kiểm tra')
  } finally {
    testing.value = false
  }
}

function formatDateTime(value) {
  if (!value) return 'Chưa cập nhật'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return 'Chưa cập nhật'
  return new Intl.DateTimeFormat('vi-VN', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(date)
}

onMounted(loadSettings)
</script>

<style scoped>
.settings-page {
  max-width: 1120px;
  margin: 0 auto;
}

.settings-panel {
  overflow: hidden;
  border: 1px solid var(--admin-border);
  border-radius: 8px;
  background: var(--admin-surface);
  box-shadow: 0 8px 24px rgba(15, 23, 42, 0.05);
}

.settings-panel__header {
  display: flex;
  align-items: flex-start;
  gap: 14px;
  padding: 22px 24px;
  border-bottom: 1px solid var(--admin-border);
}

.settings-panel__header--compact {
  padding-bottom: 18px;
}

.settings-icon {
  display: grid;
  width: 42px;
  height: 42px;
  flex: 0 0 42px;
  place-items: center;
  border-radius: 8px;
  background: #eaf2ff;
  color: #155eef;
  font-size: 19px;
}

.settings-icon--green {
  background: #e9f8f1;
  color: #039855;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  min-height: 24px;
  padding: 2px 9px;
  border: 1px solid;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 700;
}

.status-badge--ready {
  border-color: #a6f4c5;
  background: #ecfdf3;
  color: #027a48;
}

.status-badge--warning {
  border-color: #fedf89;
  background: #fffaeb;
  color: #b54708;
}

.settings-form {
  padding: 24px;
}

.settings-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 4px 20px;
}

.field-icon {
  color: #98a2b3;
}

.ssl-control {
  display: flex;
  min-height: 40px;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 8px 12px;
  border: 1px solid var(--admin-border);
  border-radius: 6px;
}

.settings-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-top: 6px;
  padding-top: 20px;
  border-top: 1px solid var(--admin-border);
}

.test-email-row {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 12px;
  padding: 20px 24px 24px;
}

@media (max-width: 720px) {
  .settings-grid {
    grid-template-columns: 1fr;
  }

  .settings-panel__header,
  .settings-form,
  .test-email-row {
    padding-left: 16px;
    padding-right: 16px;
  }

  .settings-actions {
    align-items: stretch;
    flex-direction: column;
  }

  .test-email-row {
    grid-template-columns: 1fr;
  }
}
</style>
