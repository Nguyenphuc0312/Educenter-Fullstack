<template>
  <div class="space-y-6">
    <PageHeader 
      title="Hồ sơ cá nhân" 
      subtitle="Quản lý thông tin tài khoản và dữ liệu cá nhân liên kết trên hệ thống." 
    />

    <LoadingSpinner v-if="loading" size="lg" class="py-20" />
    
    <div v-else class="grid grid-cols-1 xl:grid-cols-3 gap-6">
      
      <section class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden flex flex-col h-max">
        <div class="h-32 bg-gradient-to-r from-blue-600 to-indigo-600 relative">
          <div class="absolute inset-0 opacity-20 bg-[url('https://www.transparenttextures.com/patterns/cubes.png')]"></div>
        </div>
        
        <div class="px-6 pb-6 flex flex-col items-center -mt-16 relative z-10 text-center">
          
          <div class="relative group cursor-pointer" @click="triggerAvatarUpload">
            <div class="w-32 h-32 rounded-full border-4 border-white bg-white text-blue-600 flex items-center justify-center text-4xl font-black shadow-md mb-4 overflow-hidden relative">
              <img v-if="avatarPreview || student?.avatar" :src="avatarPreview || student?.avatar" class="w-full h-full object-cover" alt="Avatar" />
              <div v-else class="w-full h-full bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center">
                {{ initials }}
              </div>
              
              <div class="absolute inset-0 bg-black/50 flex flex-col items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-300">
                <svg class="w-8 h-8 text-white mb-1" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z" /><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 13a3 3 0 11-6 0 3 3 0 016 0z" /></svg>
                <span class="text-[10px] font-bold text-white uppercase tracking-wider">Đổi ảnh</span>
              </div>
            </div>
            <input type="file" ref="fileInput" class="hidden" accept="image/png, image/jpeg, image/jpg" @change="onAvatarSelected" />
          </div>
          
          <h2 class="text-2xl font-black text-slate-800 leading-tight">
            {{ student?.fullName || auth.user?.fullName || 'Học viên' }}
          </h2>
          <p class="text-slate-500 font-medium mt-1">@{{ auth.user?.username || student?.studentCode }}</p>
          
          <span :class="['mt-3 px-3 py-1 text-xs font-bold rounded-full border', statusClass(student?.status)]">
            {{ statusText(student?.status || 'Active') }}
          </span>

          <div class="w-full grid grid-cols-2 gap-3 mt-6 pt-6 border-t border-slate-100">
            <button class="px-4 py-2 bg-slate-50 hover:bg-slate-100 text-slate-700 font-semibold rounded-lg text-sm transition-colors border border-slate-200 shadow-sm active:scale-95">
              Đổi mật khẩu
            </button>
            <button @click="openEditModal" class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg text-sm transition-colors border border-blue-600 shadow-sm active:scale-95">
              Cập nhật hồ sơ
            </button>
          </div>
        </div>
      </section>

      <section class="xl:col-span-2 bg-white rounded-2xl border border-slate-100 shadow-sm p-6 lg:p-8">
        <div class="mb-6 pb-4 border-b border-slate-100 flex justify-between items-center">
          <div>
            <h2 class="text-xl font-bold text-slate-800">Thông tin chi tiết</h2>
            <p class="text-sm text-slate-500 mt-1">Dữ liệu hồ sơ cá nhân đồng bộ từ hệ thống quản lý học tập.</p>
          </div>
        </div>

        <div v-if="error" class="text-center text-red-500 py-4">{{ error }}</div>
        
        <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4 lg:gap-6">
          <div class="bg-slate-50 rounded-xl p-4 border border-slate-100 hover:border-blue-200 transition-colors">
            <span class="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-slate-400 mb-1">
              <svg class="w-4 h-4 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 6H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V8a2 2 0 00-2-2h-5m-4 0V5a2 2 0 114 0v1m-4 0a2 2 0 104 0m-5 8a2 2 0 100-4 2 2 0 000 4zm0 0c1.306 0 2.417.835 2.83 2M9 14a3.001 3.001 0 00-2.83 2M15 11h3m-3 4h2" /></svg>
              Mã học viên
            </span>
            <strong class="text-base text-slate-800 break-words">{{ student?.studentCode || '-' }}</strong>
          </div>

          <div class="bg-slate-50 rounded-xl p-4 border border-slate-100 hover:border-blue-200 transition-colors">
            <span class="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-slate-400 mb-1">
              <svg class="w-4 h-4 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" /></svg>
              Email liên hệ
            </span>
            <strong class="text-base text-slate-800 break-words">{{ student?.email || auth.user?.email || '-' }}</strong>
          </div>

          <div class="bg-slate-50 rounded-xl p-4 border border-slate-100 hover:border-blue-200 transition-colors">
            <span class="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-slate-400 mb-1">
              <svg class="w-4 h-4 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" /></svg>
              Số điện thoại
            </span>
            <strong class="text-base text-slate-800 break-words">{{ student?.phone || '-' }}</strong>
          </div>

          <div class="bg-slate-50 rounded-xl p-4 border border-slate-100 hover:border-blue-200 transition-colors">
            <span class="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-slate-400 mb-1">
              <svg class="w-4 h-4 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>
              Ngày sinh
            </span>
            <strong class="text-base text-slate-800 break-words">{{ formatDate(student?.dateOfBirth) || '-' }}</strong>
          </div>

          <div class="bg-slate-50 rounded-xl p-4 border border-slate-100 hover:border-blue-200 transition-colors">
            <span class="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-slate-400 mb-1">
              <svg class="w-4 h-4 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" /></svg>
              Giới tính
            </span>
            <strong class="text-base text-slate-800 break-words">{{ genderText(student?.gender) }}</strong>
          </div>

          <div class="bg-slate-50 rounded-xl p-4 border border-slate-100 hover:border-blue-200 transition-colors">
            <span class="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-slate-400 mb-1">
              <svg class="w-4 h-4 text-slate-400" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1" /></svg>
              Reference ID <span class="text-[10px] font-normal lowercase text-slate-400">(Chỉ đọc)</span>
            </span>
            <strong class="text-sm font-mono text-slate-500 break-words">{{ auth.user?.referenceId || '-' }}</strong>
          </div>

          <div class="md:col-span-2 bg-slate-50 rounded-xl p-4 border border-slate-100 hover:border-blue-200 transition-colors">
            <span class="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-slate-400 mb-1">
              <svg class="w-4 h-4 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.242-4.243a8 8 0 1111.314 0z" /><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" /></svg>
              Địa chỉ liên hệ
            </span>
            <strong class="text-base text-slate-800 break-words">{{ student?.address || 'Chưa cập nhật địa chỉ' }}</strong>
          </div>
        </div>
      </section>
    </div>

    <a-modal 
      v-model:open="isEditModalVisible" 
      title="Cập nhật thông tin cá nhân" 
      :footer="null" 
      destroyOnClose 
      centered 
      width="650px"
    >
      <form @submit.prevent="saveProfile" class="pt-4 space-y-5">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-5">
          
          <div class="space-y-1.5">
            <label class="text-sm font-semibold text-slate-700">Họ và tên <span class="text-red-500">*</span></label>
            <input 
              v-model="editForm.fullName" 
              type="text" 
              required
              placeholder="Nhập họ và tên..."
              class="w-full px-3.5 py-2.5 bg-white border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-100 focus:border-blue-500 outline-none transition-all"
            />
          </div>

          <div class="space-y-1.5">
            <label class="text-sm font-semibold text-slate-700">Email liên hệ</label>
            <input 
              :value="editForm.email" 
              type="email" 
              disabled
              class="w-full px-3.5 py-2.5 bg-slate-100 border border-slate-200 rounded-lg text-sm text-slate-500 cursor-not-allowed outline-none"
            />
            <p class="text-[11px] text-slate-400">Vui lòng liên hệ Admin nếu muốn đổi Email.</p>
          </div>

          <div class="space-y-1.5">
            <label class="text-sm font-semibold text-slate-700">Số điện thoại</label>
            <input 
              v-model="editForm.phone" 
              type="tel" 
              placeholder="09xx xxx xxx"
              class="w-full px-3.5 py-2.5 bg-white border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-100 focus:border-blue-500 outline-none transition-all"
            />
          </div>

          <div class="space-y-1.5">
            <label class="text-sm font-semibold text-slate-700">Ngày sinh</label>
            <input 
              v-model="editForm.dateOfBirth" 
              type="date" 
              class="w-full px-3.5 py-2.5 bg-white border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-100 focus:border-blue-500 outline-none transition-all"
            />
          </div>

          <div class="space-y-1.5">
            <label class="text-sm font-semibold text-slate-700">Giới tính</label>
            <select 
              v-model="editForm.gender" 
              class="w-full px-3.5 py-2.5 bg-white border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-100 focus:border-blue-500 outline-none transition-all appearance-none"
            >
              <option value="Male">Nam</option>
              <option value="Female">Nữ</option>
              <option value="Other">Khác</option>
            </select>
          </div>

          <div class="sm:col-span-2 space-y-1.5">
            <label class="text-sm font-semibold text-slate-700">Địa chỉ liên hệ</label>
            <textarea 
              v-model="editForm.address" 
              rows="3"
              placeholder="Nhập địa chỉ thường trú..."
              class="w-full px-3.5 py-2.5 bg-white border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-100 focus:border-blue-500 outline-none transition-all resize-none"
            ></textarea>
          </div>
        </div>

        <div class="flex justify-end gap-3 pt-5 border-t border-slate-100 mt-6">
          <button 
            type="button" 
            @click="isEditModalVisible = false"
            class="px-5 py-2.5 bg-white border border-slate-300 text-slate-700 font-semibold rounded-lg text-sm hover:bg-slate-50 transition-colors"
          >
            Hủy bỏ
          </button>
          <button 
            type="submit" 
            :disabled="isSaving"
            class="px-5 py-2.5 bg-blue-600 border border-blue-600 text-white font-semibold rounded-lg text-sm hover:bg-blue-700 transition-colors flex items-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed"
          >
            <LoadingSpinner v-if="isSaving" size="sm" class="!text-white" />
            <span v-else>Lưu thay đổi</span>
          </button>
        </div>
      </form>
    </a-modal>

  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import dayjs from 'dayjs'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { useAuthStore } from '@/stores/auth'
import { studentApi } from '@/api/studentApi'
import { formatDate, getInitials } from '@/lib/formatters'

const auth = useAuthStore()
const student = ref(null)
const loading = ref(true)
const error = ref('')

const initials = computed(() => getInitials(student.value?.fullName || auth.user?.fullName || auth.user?.username || 'HV'))

// --- XỬ LÝ AVATAR ---
const fileInput = ref(null)
const avatarPreview = ref(null)
const avatarFileToUpload = ref(null)

function triggerAvatarUpload() {
  fileInput.value.click()
}

async function onAvatarSelected(event) {
  const file = event.target.files[0]
  if (!file) return

  // Validate kích thước (vd: tối đa 2MB)
  if (file.size > 2 * 1024 * 1024) {
    message.error('Kích thước ảnh không được vượt quá 2MB.')
    return
  }

  // Preview ảnh Local
  avatarPreview.value = URL.createObjectURL(file)
  avatarFileToUpload.value = file

  // Gọi API upload ngay lập tức (hoặc lưu cùng form cập nhật tùy logic backend)
  message.loading({ content: 'Đang tải ảnh lên...', key: 'avatarUpload' })
  try {
    // Giả lập API Upload (Thay bằng: await studentApi.uploadAvatar(student.value.id, file))
    await new Promise(resolve => setTimeout(resolve, 1000)) 
    
    message.success({ content: 'Cập nhật ảnh đại diện thành công!', key: 'avatarUpload' })
    // Cập nhật state nội bộ
    if (student.value) student.value.avatar = avatarPreview.value
  } catch (error) {
    message.error({ content: 'Lỗi tải ảnh lên. Vui lòng thử lại.', key: 'avatarUpload' })
    avatarPreview.value = null // Revert
  }
}

// --- XỬ LÝ FORM CẬP NHẬT HỒ SƠ ---
const isEditModalVisible = ref(false)
const isSaving = ref(false)
const phonePattern = /^0\d{9}$/
const editForm = ref({
  fullName: '',
  email: '',
  phone: '',
  dateOfBirth: '',
  gender: 'Male',
  address: ''
})

function openEditModal() {
  if (!student.value) return
  // Clone dữ liệu hiện tại vào Form
  editForm.value = {
    fullName: student.value.fullName || '',
    email: student.value.email || auth.user?.email || '',
    phone: student.value.phone || '',
    // Format ngày sinh chuẩn yyyy-mm-dd cho thẻ <input type="date">
    dateOfBirth: student.value.dateOfBirth ? dayjs(student.value.dateOfBirth).format('YYYY-MM-DD') : '',
    gender: student.value.gender || 'Male',
    address: student.value.address || ''
  }
  isEditModalVisible.value = true
}

async function saveProfile() {
  if (!phonePattern.test(String(editForm.value.phone || '').trim())) {
    message.error('Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.')
    return
  }

  isSaving.value = true
  try {
    // Chuẩn bị Payload
    const payload = {
      ...student.value, // Giữ lại ID và các trường ẩn
      fullName: editForm.value.fullName,
      phone: String(editForm.value.phone).trim(),
      dateOfBirth: editForm.value.dateOfBirth ? new Date(editForm.value.dateOfBirth).toISOString() : null,
      gender: editForm.value.gender,
      address: editForm.value.address
    }

    // Giả lập gọi API (Thay bằng: await studentApi.update(student.value.id, payload))
    await new Promise(resolve => setTimeout(resolve, 800))
    
    // Cập nhật lại giao diện
    student.value = payload
    
    message.success('Cập nhật hồ sơ thành công!')
    isEditModalVisible.value = false
  } catch (error) {
    message.error('Lỗi khi lưu dữ liệu. Vui lòng kiểm tra lại.')
  } finally {
    isSaving.value = false
  }
}

// --- LẤY DỮ LIỆU BAN ĐẦU ---
onMounted(loadData)

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    student.value = auth.user?.referenceId ? await studentApi.getById(auth.user.referenceId) : null
  } catch (err) {
    error.value = err.message || 'Không tải được hồ sơ học viên. Vui lòng thử lại sau.'
  } finally {
    loading.value = false
  }
}

// --- HELPERS GIAO DIỆN ---
function statusText(status) {
  return ({ Active: 'Đang hoạt động', Inactive: 'Ngừng hoạt động', Suspended: 'Tạm khóa' })[status] || status
}

function statusClass(status) {
  if (status === 'Active') return 'bg-emerald-50 text-emerald-600 border-emerald-200'
  if (status === 'Suspended') return 'bg-red-50 text-red-600 border-red-200'
  return 'bg-slate-50 text-slate-600 border-slate-200' // Inactive
}

function genderText(gender) {
  return ({ Male: 'Nam', Female: 'Nữ', Other: 'Khác' })[gender] || gender || '-'
}
</script>

<style scoped>
/* Tuỳ chỉnh thanh cuộn cho Textarea */
textarea::-webkit-scrollbar {
  width: 6px;
}
textarea::-webkit-scrollbar-track {
  background: #f1f5f9; 
  border-radius: 4px;
}
textarea::-webkit-scrollbar-thumb {
  background: #cbd5e1; 
  border-radius: 4px;
}
</style>
