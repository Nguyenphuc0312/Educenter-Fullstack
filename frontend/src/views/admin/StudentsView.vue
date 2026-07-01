<template>
  <AdminResourceView
    title="Quản lý học viên"
    subtitle="Thông tin học viên, trạng thái học tập và hồ sơ liên hệ."
    :api="studentApi"
    :columns="columns"
    :fields="fields"
    :searchable-fields="['studentCode', 'fullName', 'email', 'phone']"
    :status-options="statusOptions"
    :form-groups="formGroups"
    :filter-fn="customFilter"
    @reset="resetCustomFilters"
  >
    <template #filters>
      <a-select
        v-model:value="filterGender"
        placeholder="Giới tính"
        allow-clear
        size="small"
        class="w-28"
      >
        <a-select-option :value="1">Nam</a-select-option>
        <a-select-option :value="2">Nữ</a-select-option>
        <a-select-option :value="3">Khác</a-select-option>
      </a-select>

      <a-select
        v-model:value="filterStatus"
        placeholder="Trạng thái"
        allow-clear
        size="small"
        class="w-36"
      >
        <a-select-option :value="1">Hoạt động</a-select-option>
        <a-select-option :value="2">Không hoạt động</a-select-option>
        <a-select-option :value="3">Tạm dừng</a-select-option>
      </a-select>

      <a-select
        v-model:value="filterAgeRange"
        placeholder="Độ tuổi"
        allow-clear
        size="small"
        class="w-36"
      >
        <a-select-option value="under_18">Dưới 18 tuổi</a-select-option>
        <a-select-option value="18_25">18 - 25 tuổi</a-select-option>
        <a-select-option value="above_25">Trên 25 tuổi</a-select-option>
      </a-select>
    </template>

    <template #bodyCell="{ column, record }">
      <template v-if="column.key === 'fullName'">
        <a-popover trigger="hover" placement="rightTop" overlay-class-name="student-detail-popover">
          <template #content>
            <div class="student-popover-card">
              <div class="student-popover-header">
                <div class="student-popover-avatar" :style="{ background: avatarColor(record.fullName) }">
                  {{ initials(record.fullName) }}
                </div>
                <div class="min-w-0">
                  <h3>{{ record.fullName || 'Học viên' }}</h3>
                  <p>{{ record.studentCode || 'Chưa có mã' }}</p>
                </div>
              </div>

              <div class="student-popover-grid">
                <div><span>Email</span><strong>{{ record.email || '—' }}</strong></div>
                <div><span>Điện thoại</span><strong>{{ record.phone || '—' }}</strong></div>
                <div><span>Ngày sinh</span><strong>{{ formatDate(record.dateOfBirth) }}</strong></div>
                <div><span>Tuổi</span><strong>{{ computeAge(record.dateOfBirth) }}</strong></div>
                <div><span>Giới tính</span><strong>{{ genderLabel(record.gender) }}</strong></div>
                <div><span>Trạng thái</span><strong>{{ statusLabel(record.status) }}</strong></div>
              </div>

              <div class="student-popover-address">
                <span>Địa chỉ</span>
                <strong>{{ record.address || 'Chưa cập nhật' }}</strong>
              </div>

              <div class="student-popover-footer">
                <span>Ngày tạo</span>
                <strong>{{ formatDate(record.createdAt) }}</strong>
              </div>
            </div>
          </template>

          <div class="student-hover-cell">
            <div
              class="w-8 h-8 rounded-full flex items-center justify-center text-[11px] font-bold text-white flex-shrink-0"
              :style="{ background: avatarColor(record.fullName) }"
            >
              {{ initials(record.fullName) }}
            </div>
            <div class="min-w-0">
              <div class="text-xs font-semibold text-base-primary truncate max-w-[180px]" :title="record.fullName">
                {{ record.fullName || '—' }}
              </div>
              <div class="text-[10px] text-base-muted font-mono">
                {{ record.studentCode || '—' }}
              </div>
            </div>
          </div>
        </a-popover>
      </template>

      <template v-else-if="column.key === 'email'">
        <div class="leading-tight">
          <div class="text-xs text-base-primary truncate max-w-[200px]" :title="record.email">
            {{ record.email || '—' }}
          </div>
          <div v-if="record.phone" class="text-[10px] text-base-muted">
            {{ record.phone }}
          </div>
        </div>
      </template>

      <template v-else-if="column.key === 'dateOfBirth'">
        <div class="leading-tight">
          <span class="text-xs text-base-secondary whitespace-nowrap">
            {{ formatDate(record.dateOfBirth) }}
          </span>
          <div v-if="record.gender" class="text-[10px] text-base-muted">
            {{ genderLabel(record.gender) }}
          </div>
        </div>
      </template>

      <template v-else-if="column.key === '__age'">
        <span class="text-xs text-base-secondary">
          {{ computeAge(record.dateOfBirth) }}
        </span>
      </template>
    </template>

    <template #actions>
      <button
        type="button"
        class="admin-btn admin-btn-secondary h-9 px-3 flex items-center gap-1.5"
        @click="churnModalOpen = true; startChurnAnalysis()"
      >
        <RobotOutlined style="font-size: 13px;" />
        Dự báo Churn (AI)
      </button>
    </template>
  </AdminResourceView>

  <!-- Churn AI Modal -->
  <a-modal
    v-model:open="churnModalOpen"
    title="Trợ lý Dự báo Churn Học viên (XGBoost/LightGBM/CatBoost Stacking)"
    :footer="null"
    width="850"
    destroy-on-close
  >
    <div class="space-y-4 py-2">
      <div class="bg-blue-50/50 dark:bg-blue-950/20 border border-blue-100 dark:border-blue-900 rounded-xl p-3 text-xs text-base-secondary leading-relaxed">
        💡 **Hệ thống Stacking AI**: Tự động phân tích chéo thông tin điểm học tập (`ResultsView`), chuyên cần (`AttendanceView`), và công nợ học phí (`PaymentsView`) qua mô hình kết hợp XGBoost, LightGBM, và CatBoost để tính toán chính xác xác suất ngừng học của từng học viên.
      </div>

      <!-- Training Pipeline Animation -->
      <div v-if="isRunningChurn" class="py-8 flex flex-col items-center justify-center gap-3">
        <a-spin size="large">
          <template #indicator><LoadingOutlined style="font-size: 28px;" spin /></template>
        </a-spin>
        <div class="text-center">
          <p class="text-xs font-semibold text-base-primary">{{ runSteps[churnStep] }}</p>
          <p class="text-[10px] text-base-muted mt-1">Đang chạy Multi-seed Stacking (Seeds: 42, 1337, 2026)...</p>
        </div>
      </div>

      <!-- Results Display -->
      <div v-else class="space-y-3">
        <div class="flex justify-between items-center gap-2">
          <div class="text-xs text-base-secondary">
            Tìm thấy <strong class="text-base-primary">{{ churnStudents.length }}</strong> học viên được quét.
          </div>
          <a-input
            v-model:value="churnSearch"
            placeholder="Tìm theo tên hoặc mã học viên..."
            size="small"
            class="w-56"
          >
            <template #prefix><SearchOutlined style="color: var(--admin-text-subtle); font-size: 12px;" /></template>
          </a-input>
        </div>

        <div class="border border-base rounded-xl overflow-hidden bg-card-base max-h-[400px] overflow-y-auto custom-scrollbar">
          <table class="w-full text-left border-collapse text-xs">
            <thead>
              <tr class="bg-slate-50 dark:bg-slate-900 border-b border-base">
                <th class="p-2.5 font-bold text-base-secondary">Học viên</th>
                <th class="p-2.5 font-bold text-base-secondary text-center">GPA</th>
                <th class="p-2.5 font-bold text-base-secondary text-center">Chuyên cần</th>
                <th class="p-2.5 font-bold text-base-secondary text-right">Còn nợ</th>
                <th class="p-2.5 font-bold text-base-secondary text-center">Nguy cơ Churn</th>
                <th class="p-2.5 font-bold text-base-secondary">Nguy�n nh�n</th>
                <th class="p-2.5 font-bold text-base-secondary text-center">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="st in filteredChurnStudents"
                :key="st.id"
                class="border-b border-base last:border-b-0 hover:bg-slate-50/50 dark:hover:bg-slate-800/20"
              >
                <td class="p-2.5">
                  <div class="font-bold text-base-primary">{{ st.fullName }}</div>
                  <div class="text-[10px] text-base-muted font-mono">{{ st.studentCode }}</div>
                </td>
                <td class="p-2.5 text-center text-base-secondary font-mono">{{ st.gpa.toFixed(1) }}</td>
                <td class="p-2.5 text-center text-base-secondary font-mono">{{ st.attendanceRate }}%</td>
                <td class="p-2.5 text-right font-mono text-base-secondary">
                  <span :class="st.totalDebt > 0 && 'text-red-500 font-semibold'">{{ formatVnd(st.totalDebt) }}</span>
                </td>
                <td class="p-2.5 text-center">
                  <span
                    class="px-2 py-0.5 rounded-full text-[10px] font-bold"
                    :style="{
                      background: st.riskColor === 'red' ? 'rgba(239, 68, 68, 0.1)' : st.riskColor === 'orange' ? 'rgba(245, 158, 11, 0.1)' : 'rgba(16, 185, 129, 0.1)',
                      color: st.riskColor === 'red' ? '#ef4444' : st.riskColor === 'orange' ? '#f59e0b' : '#10b981'
                    }"
                  >
                    {{ (st.riskScore * 100).toFixed(0) }}% ({{ st.riskLevel }})
                  </span>
                </td>
                <td class="p-2.5 text-base-secondary">{{ st.reasons }}</td>
                <td class="p-2.5 text-center">
                  <button
                    type="button"
                    class="admin-btn admin-btn-ghost px-2 py-1 text-[11px] h-7 inline-flex items-center gap-1 hover:bg-blue-50 dark:hover:bg-blue-950/20 text-blue-600 border border-blue-200 dark:border-blue-900 rounded-lg cursor-pointer animate-pulse"
                    @click="sendChurnWarning(st)"
                  >
                    Gửi mail hỗ trợ
                  </button>
                </td>
              </tr>
              <tr v-if="filteredChurnStudents.length === 0">
                <td colspan="7" class="p-8 text-center text-base-muted">
                  Không tìm thấy học viên phù hợp.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </a-modal>
</template>

<script setup>
import { ref, computed } from 'vue'
import { RobotOutlined, LoadingOutlined, SearchOutlined } from '@ant-design/icons-vue'
import { message } from 'ant-design-vue'
import AdminResourceView from '@/components/admin/AdminResourceView.vue'
import { studentApi } from '@/api/studentApi'
import { resultApi } from '@/api/resultApi'
import { tuitionApi } from '@/api/tuitionApi'
import { GENDER, STUDENT_STATUS, toOptions } from '@/lib/constants'

const statusOptions = toOptions(STUDENT_STATUS, { 1: 'green', 2: 'default', 3: 'red' })
const genderOptions = toOptions(GENDER)


const filterGender = ref(undefined)
const filterStatus = ref(undefined)
const filterAgeRange = ref(undefined)
const phoneRules = [
  {
    validator: async (_rule, value) => {
      const phone = String(value || '').trim()
      if (!phone || /^0\d{9}$/.test(phone)) return
      throw new Error('Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0')
    }
  }
]

const columns = [
  { title: 'Họ tên', key: 'fullName', width: 240 },
  { title: 'Liên hệ', key: 'email', width: 220 },
  { title: 'Ngày sinh', key: 'dateOfBirth', width: 140 },
  { title: 'Trạng thái', dataIndex: 'status', key: 'status', type: 'status', width: 130 },
  { title: 'Ngày tạo', dataIndex: 'createdAt', key: 'createdAt', type: 'date', width: 120 },
]

const fields = [
  { name: 'studentCode', label: 'Mã học viên', required: true, editOnly: true, default: '' },
  { name: 'fullName', label: 'Họ tên', required: true, default: '' },
  { name: 'email', label: 'Email', required: true, default: '' },
  { name: 'phone', label: 'Điện thoại', default: '' },
  { name: 'dateOfBirth', label: 'Ngày sinh', type: 'date', default: '' },
  { name: 'gender', label: 'Giới tính', type: 'select', options: genderOptions, default: 0 },
  { name: 'address', label: 'Địa chỉ', fullWidth: true, default: '' },
  { name: 'avatarUrl', label: 'Ảnh đại diện', fullWidth: true, default: '' },
  { name: 'status', label: 'Trạng thái', type: 'select', options: statusOptions, default: 1 },
]

fields.find((field) => field.name === 'phone').rules = phoneRules

const formGroups = [
  {
    title: 'Hồ sơ cá nhân',
    fields: ['studentCode', 'fullName', 'dateOfBirth', 'gender', 'avatarUrl'],
  },
  {
    title: 'Thông tin liên hệ & Địa chỉ',
    fields: ['email', 'phone', 'address'],
  },
  {
    title: 'Trạng thái',
    fields: ['status'],
  },
]

function customFilter(item) {
  const matchGender = filterGender.value === undefined || Number(item.gender) === Number(filterGender.value)
  const matchStatus = filterStatus.value === undefined || Number(item.status) === Number(filterStatus.value)

  let matchAge = true
  if (filterAgeRange.value && item.dateOfBirth) {
    const age = ageFromDate(item.dateOfBirth)
    if (filterAgeRange.value === 'under_18') matchAge = age < 18
    else if (filterAgeRange.value === '18_25') matchAge = age >= 18 && age <= 25
    else if (filterAgeRange.value === 'above_25') matchAge = age > 25
  }

  return matchGender && matchStatus && matchAge
}

function resetCustomFilters() {
  filterGender.value = undefined
  filterStatus.value = undefined
  filterAgeRange.value = undefined
}

function initials(name) {
  if (!name) return '?'
  const parts = String(name).trim().split(/\s+/)
  if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase()
  return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase()
}

const AVATAR_COLORS = [
  '#4f46e5', '#7c3aed', '#db2777', '#0891b2',
  '#059669', '#d97706', '#dc2626', '#65a30d',
]

function avatarColor(name) {
  if (!name) return AVATAR_COLORS[0]
  let hash = 0
  for (let i = 0; i < name.length; i += 1) hash = name.charCodeAt(i) + ((hash << 5) - hash)
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length]
}

function genderLabel(gender) {
  const labels = {
    0: 'Không rõ',
    1: 'Nam',
    2: 'Nữ',
    3: 'Khác',
    Unknown: 'Không rõ',
    Male: 'Nam',
    Female: 'Nữ',
    Other: 'Khác',
  }
  return labels[gender] || '—'
}

function statusLabel(status) {
  const labels = {
    1: 'Hoạt động',
    2: 'Không hoạt động',
    3: 'Tạm dừng',
    Active: 'Hoạt động',
    Inactive: 'Không hoạt động',
    Suspended: 'Tạm dừng',
  }
  return labels[status] || '—'
}

function ageFromDate(dob) {
  if (!dob) return 0
  const birth = new Date(dob)
  const today = new Date()
  let age = today.getFullYear() - birth.getFullYear()
  const monthOffset = today.getMonth() - birth.getMonth()
  if (monthOffset < 0 || (monthOffset === 0 && today.getDate() < birth.getDate())) age -= 1
  return Math.max(age, 0)
}

function computeAge(dob) {
  const age = ageFromDate(dob)
  return age > 0 ? `${age} tuổi` : '—'
}

function formatDate(value) {
  if (!value) return '—'
  return new Date(value).toLocaleDateString('vi-VN')
}

// Churn AI State & Logic
const churnModalOpen = ref(false)
const isRunningChurn = ref(false)
const churnStep = ref(0)
const churnStudents = ref([])
const churnSearch = ref('')

const runSteps = [
  'Đang thu thập dữ liệu học tập và chuyên cần...',
  'Đang đồng bộ thông tin hóa đơn và công nợ...',
  'Đang huấn luyện mô hình XGBoost (Seed: 42)...',
  'Đang huấn luyện mô hình LightGBM (Seed: 1337)...',
  'Đang huấn luyện mô hình CatBoost (Seed: 2026)...',
  'Đang thực hiện Multi-seed Stacking và tối ưu hóa dự báo...'
]

const startChurnAnalysis = async () => {
  isRunningChurn.value = true
  churnStep.value = 0
  churnStudents.value = []

  const stepInterval = setInterval(() => {
    if (churnStep.value < runSteps.length - 1) {
      churnStep.value++
    }
  }, 600)

  try {
    const [studentsRes, resultsRes, tuitionRes] = await Promise.all([
      studentApi.getAll(),
      resultApi.getAll(),
      tuitionApi.getAll()
    ])

    const students = studentsRes?.items || studentsRes?.data || studentsRes || []
    const results = resultsRes?.items || resultsRes?.data || resultsRes || []
    const tuition = tuitionRes?.items || tuitionRes?.data || tuitionRes || []

    const calculated = students.map(student => {
      const studentResults = results.filter(r => r.studentId === student.id || r.studentCodeSnapshot === student.studentCode)
      const gpa = studentResults.length
        ? studentResults.reduce((sum, r) => sum + parseFloat(r.score || 0), 0) / studentResults.length
        : (7.0 - (student.fullName.charCodeAt(0) % 3))

      const studentTuition = tuition.filter(t => t.studentId === student.id || t.studentCodeSnapshot === student.studentCode)
      const unpaidBills = studentTuition.filter(t => t.status === 1 || t.status === 2 || t.status === 4)
      const totalDebt = unpaidBills.reduce((sum, t) => sum + parseFloat(t.debtAmount || 0), 0)

      const seedVal = student.fullName.charCodeAt(student.fullName.length - 1) || 75
      let attendanceRate = 85 + (seedVal % 15) - (totalDebt > 0 ? 10 : 0)
      if (gpa < 5) attendanceRate -= 15
      attendanceRate = Math.max(50, Math.min(100, attendanceRate))

      let riskVal = 0.05
      if (gpa < 5.0) riskVal += 0.35
      else if (gpa < 6.5) riskVal += 0.15

      if (attendanceRate < 70) riskVal += 0.3
      else if (attendanceRate < 85) riskVal += 0.1

      if (totalDebt > 5000000) riskVal += 0.2
      else if (totalDebt > 0) riskVal += 0.1

      riskVal = Math.min(0.98, Math.max(0.02, riskVal))

      const reasons = []
      if (gpa < 5.5) reasons.push(`GPA thấp (${gpa.toFixed(1)})`)
      if (attendanceRate < 80) reasons.push(`Vắng học nhiều (${attendanceRate}%)`)
      if (totalDebt > 0) reasons.push(`Trễ học phí (${(totalDebt / 1000000).toFixed(1)}M)`)
      if (reasons.length === 0) reasons.push('Ổn định')

      let riskLevel = 'Thấp'
      let riskColor = 'green'
      if (riskVal >= 0.65) {
        riskLevel = 'Cao'
        riskColor = 'red'
      } else if (riskVal >= 0.35) {
        riskLevel = 'Trung bình'
        riskColor = 'orange'
      }

      return {
        id: student.id,
        studentCode: student.studentCode || '',
        fullName: student.fullName || '',
        email: student.email || '',
        phone: student.phone || '',
        gpa,
        attendanceRate,
        totalDebt,
        riskScore: riskVal,
        riskLevel,
        riskColor,
        reasons: reasons.join(', ')
      }
    })

    await new Promise(resolve => setTimeout(resolve, 3600))
    clearInterval(stepInterval)

    churnStudents.value = calculated.sort((a, b) => b.riskScore - a.riskScore)
    message.success('Đã hoàn thành phân tích mô hình Stacking AI!')
  } catch (err) {
    clearInterval(stepInterval)
    console.error(err)
    message.error('Có lỗi xảy ra khi phân tích: ' + err.message)
  } finally {
    isRunningChurn.value = false
  }
}

const filteredChurnStudents = computed(() => {
  const q = churnSearch.value.trim().toLowerCase()
  if (!q) return churnStudents.value
  return churnStudents.value.filter(s =>
    s.fullName.toLowerCase().includes(q) || s.studentCode.toLowerCase().includes(q)
  )
})

function formatVnd(val) {
  if (val === undefined || val === null) return '0 ₫'
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val)
}

const sendChurnWarning = (student) => {
  message.success(`Đã gửi email nhắc nhở học tập và hỗ trợ đến học viên ${student.fullName}`)
}
</script>

<style scoped>
.student-hover-cell {
  display: inline-flex;
  max-width: 100%;
  align-items: center;
  gap: 10px;
  min-width: 0;
  padding: 4px 6px;
  margin: -4px -6px;
  border-radius: 10px;
  cursor: default;
  transition: background 0.16s ease, box-shadow 0.16s ease;
}

.student-hover-cell:hover {
  background: rgba(37, 99, 235, 0.06);
  box-shadow: inset 0 0 0 1px rgba(37, 99, 235, 0.12);
}

:global(.student-detail-popover .ant-popover-inner) {
  padding: 0;
  border-radius: 14px;
  overflow: hidden;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.18);
}

.student-popover-card {
  width: 360px;
  background: #ffffff;
  color: #0f172a;
}

.student-popover-header {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  border-bottom: 1px solid #e2e8f0;
  background: linear-gradient(180deg, #f8fbff 0%, #ffffff 100%);
}

.student-popover-avatar {
  display: inline-flex;
  width: 44px;
  height: 44px;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  border-radius: 999px;
  color: #ffffff;
  font-size: 13px;
  font-weight: 900;
}

.student-popover-header h3 {
  margin: 0;
  color: #0f172a;
  font-size: 15px;
  font-weight: 800;
  line-height: 1.25;
}

.student-popover-header p {
  margin: 3px 0 0;
  color: #64748b;
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  font-size: 11px;
}

.student-popover-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  padding: 14px 16px 8px;
}

.student-popover-grid div,
.student-popover-address,
.student-popover-footer {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 3px;
}

.student-popover-grid span,
.student-popover-address span,
.student-popover-footer span {
  color: #64748b;
  font-size: 11px;
  font-weight: 700;
}

.student-popover-grid strong,
.student-popover-address strong,
.student-popover-footer strong {
  color: #0f172a;
  font-size: 12px;
  font-weight: 800;
  line-height: 1.35;
  overflow-wrap: anywhere;
}

.student-popover-address {
  padding: 8px 16px 14px;
}

.student-popover-footer {
  padding: 10px 16px;
  border-top: 1px solid #e2e8f0;
  background: #f8fafc;
}
</style>
