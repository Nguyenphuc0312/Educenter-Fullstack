<template>
  <div class="space-y-6">
    <PageHeader 
      title="Hồ sơ giảng viên" 
      subtitle="Quản lý thông tin tài khoản, hồ sơ chuyên môn và thông tin liên hệ của bạn." 
    />

    <LoadingSpinner v-if="loading" size="lg" class="py-24" />
    
    <div v-else class="grid grid-cols-1 xl:grid-cols-3 gap-6">
      
      <section class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden flex flex-col h-max transition-shadow hover:shadow-md">
        <div class="h-32 bg-gradient-to-r from-blue-600 to-indigo-700 relative">
          <div class="absolute inset-0 opacity-20 bg-[url('https://www.transparenttextures.com/patterns/cubes.png')]"></div>
        </div>
        
        <div class="px-6 pb-6 flex flex-col items-center -mt-16 relative z-10 text-center">
          <div class="relative group cursor-pointer" @click="triggerAvatarUpload">
            <div class="w-32 h-32 rounded-full border-4 border-white shadow-md mb-4 overflow-hidden relative bg-white flex items-center justify-center">
              <img v-if="avatarPreview || teacher?.avatarUrl" :src="avatarPreview || teacher?.avatarUrl" class="w-full h-full object-cover" alt="Avatar" />
              <div v-else class="w-full h-full bg-gradient-to-br from-indigo-50 to-blue-100 flex items-center justify-center text-4xl font-black text-blue-600">
                {{ initials }}
              </div>
              
              <div class="absolute inset-0 bg-black/50 flex flex-col items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity duration-300">
                <svg class="w-8 h-8 text-white mb-1" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z" /><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 13a3 3 0 11-6 0 3 3 0 016 0z" /></svg>
                <span class="text-[10px] font-bold text-white uppercase tracking-wider">Đổi ảnh</span>
              </div>
            </div>
            <input type="file" ref="fileInput" class="hidden" accept="image/png, image/jpeg, image/jpg" @change="onAvatarSelected" />
          </div>
          
          <h2 class="text-xl font-black text-slate-800 leading-tight">
            {{ teacher?.fullName || auth.user?.fullName || auth.user?.username || 'Giảng viên' }}
          </h2>
          <p class="text-sm font-semibold text-blue-600 mt-1">{{ teacher?.specialization || 'Chưa cập nhật chuyên môn' }}</p>
          
          <div class="flex items-center gap-2 mt-3">
            <span :class="['px-3 py-1 text-xs font-bold rounded-full border', statusClass(teacher?.status)]">
              {{ statusLabel(teacher?.status) }}
            </span>
          </div>

          <div class="mt-4 flex flex-col items-center">
            <span class="text-[10px] font-bold text-slate-400 uppercase tracking-wider mb-1">Đánh giá học viên</span>
            <div class="flex items-center gap-2">
              <a-rate :value="Number(teacher?.rating || 0)" disabled allow-half class="text-amber-500 text-lg" />
              <span class="font-bold text-slate-700">{{ Number(teacher?.rating || 0).toFixed(1) }}</span>
            </div>
          </div>

          <div class="w-full grid grid-cols-2 gap-3 mt-6 pt-6 border-t border-slate-100">
            <button class="px-4 py-2.5 bg-slate-50 hover:bg-slate-100 text-slate-700 font-bold rounded-xl text-sm transition-colors border border-slate-200 shadow-sm active:scale-95">
              Đổi mật khẩu
            </button>
            <button @click="openEditModal" class="px-4 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-xl text-sm transition-colors border border-blue-600 shadow-sm active:scale-95 flex items-center justify-center gap-2">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" /></svg>
              Cập nhật
            </button>
          </div>
        </div>
      </section>

      <section class="xl:col-span-2 bg-white rounded-2xl border border-slate-200 shadow-sm p-6 lg:p-8 flex flex-col transition-shadow hover:shadow-md">
        <div class="mb-6 pb-4 border-b border-slate-100 flex justify-between items-center">
          <div>
            <h2 class="text-lg font-bold text-slate-800">Thông tin chi tiết</h2>
            <p class="text-xs text-slate-500 mt-1">Dữ liệu hồ sơ đồng bộ từ hệ thống quản lý đào tạo.</p>
          </div>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 lg:gap-6 flex-1">
          <div class="bg-slate-50 rounded-xl p-4 border border-slate-100 hover:border-blue-200 transition-colors">
            <span class="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-slate-400 mb-1.5">
              <svg class="w-4 h-4 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" /></svg>
              Email liên hệ
            </span>
            <strong class="text-sm text-slate-800 break-words">{{ teacher?.email || auth.user?.email || '-' }}</strong>
          </div>

          <div class="bg-slate-50 rounded-xl p-4 border border-slate-100 hover:border-emerald-200 transition-colors">
            <span class="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-slate-400 mb-1.5">
              <svg class="w-4 h-4 text-emerald-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" /></svg>
              Số điện thoại
            </span>
            <strong class="text-sm text-slate-800 break-words">{{ teacher?.phone || auth.user?.phone || 'Chưa cập nhật' }}</strong>
          </div>

          <div class="bg-slate-50 rounded-xl p-4 border border-slate-100 hover:border-amber-200 transition-colors">
            <span class="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-slate-400 mb-1.5">
              <svg class="w-4 h-4 text-amber-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2 2v2m4 6h.01M5 20h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" /></svg>
              Kinh nghiệm giảng dạy
            </span>
            <strong class="text-sm text-slate-800 break-words">{{ teacher?.experienceYears != null ? `${teacher.experienceYears} năm` : 'Chưa cập nhật' }}</strong>
          </div>

          <div class="bg-slate-50 rounded-xl p-4 border border-slate-100 hover:border-purple-200 transition-colors">
            <span class="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-slate-400 mb-1.5">
              <svg class="w-4 h-4 text-purple-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4" /></svg>
              Mã liên kết (Ref ID)
            </span>
            <strong class="text-sm font-mono text-slate-500 break-words">{{ auth.user?.referenceId || '-' }}</strong>
          </div>

          <div class="sm:col-span-2 bg-slate-50 rounded-xl p-4 border border-slate-100 hover:border-indigo-200 transition-colors h-full">
            <span class="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-slate-400 mb-2">
              <svg class="w-4 h-4 text-indigo-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
              Giới thiệu bản thân
            </span>
            <p class="text-sm text-slate-700 leading-relaxed whitespace-pre-wrap">
              {{ teacher?.bio || 'Bạn chưa cập nhật thông tin giới thiệu bản thân. Hãy nhấn nút "Cập nhật" để bổ sung mô tả chuyên môn.' }}
            </p>
          </div>
        </div>
      </section>
    </div>

    <a-modal 
      v-model:open="isEditModalVisible" 
      title="Cập nhật hồ sơ giảng viên" 
      :footer="null" 
      destroyOnClose 
      centered 
      width="600px"
    >
      <form @submit.prevent="saveProfile" class="pt-4 space-y-5">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-5">
          
          <div class="space-y-1.5 sm:col-span-2">
            <label class="text-sm font-semibold text-slate-700">Họ và tên</label>
            <input 
              :value="teacher?.fullName || auth.user?.fullName" 
              type="text" 
              disabled
              class="w-full px-3.5 py-2.5 bg-slate-100 border border-slate-200 rounded-lg text-sm text-slate-500 cursor-not-allowed outline-none"
            />
          </div>

          <div class="space-y-1.5">
            <label class="text-sm font-semibold text-slate-700">Số điện thoại liên hệ</label>
            <input 
              v-model="editForm.phone" 
              type="tel" 
              placeholder="Nhập số điện thoại..."
              class="w-full px-3.5 py-2.5 bg-white border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-100 focus:border-blue-500 outline-none transition-all"
            />
          </div>

          <div class="space-y-1.5">
            <label class="text-sm font-semibold text-slate-700">Chuyên môn</label>
            <input 
              :value="teacher?.specialization" 
              type="text" 
              disabled
              class="w-full px-3.5 py-2.5 bg-slate-100 border border-slate-200 rounded-lg text-sm text-slate-500 cursor-not-allowed outline-none"
            />
            <p class="text-[10px] text-slate-400 mt-1">Liên hệ Quản trị viên để thay đổi.</p>
          </div>

          <div class="sm:col-span-2 space-y-1.5">
            <label class="text-sm font-semibold text-slate-700">Giới thiệu bản thân (Bio)</label>
            <textarea 
              v-model="editForm.bio" 
              rows="4"
              placeholder="Nhập giới thiệu về kinh nghiệm, kỹ năng giảng dạy..."
              class="w-full px-3.5 py-2.5 bg-white border border-slate-300 rounded-lg text-sm focus:ring-2 focus:ring-blue-100 focus:border-blue-500 outline-none transition-all resize-none custom-scrollbar"
            ></textarea>
          </div>
        </div>

        <div class="flex justify-end gap-3 pt-5 border-t border-slate-100 mt-6">
          <button 
            type="button" 
            @click="isEditModalVisible = false"
            class="px-5 py-2.5 bg-white border border-slate-300 text-slate-700 font-bold rounded-xl text-sm hover:bg-slate-50 transition-colors active:scale-95"
          >
            Hủy bỏ
          </button>
          <button 
            type="submit" 
            :disabled="isSaving"
            class="px-5 py-2.5 bg-blue-600 border border-blue-600 text-white font-bold rounded-xl text-sm hover:bg-blue-700 transition-colors flex items-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed active:scale-95"
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
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { teacherApi } from '@/api/teacherApi'
import { useAuthStore } from '@/stores/auth'
import { getInitials } from '@/lib/formatters'

const auth = useAuthStore()
const teacher = ref(null)
const loading = ref(true)

const initials = computed(() => getInitials(teacher.value?.fullName || auth.user?.fullName || auth.user?.username || 'GV'))

// --- AVATAR UPLOAD LOGIC ---
const fileInput = ref(null)
const avatarPreview = ref(null)

function triggerAvatarUpload() {
  fileInput.value.click()
}

async function onAvatarSelected(event) {
  const file = event.target.files[0]
  if (!file) return

  if (file.size > 2 * 1024 * 1024) {
    message.error('Kích thước ảnh không được vượt quá 2MB.')
    return
  }

  avatarPreview.value = URL.createObjectURL(file)
  message.loading({ content: 'Đang tải ảnh lên...', key: 'avatar' })
  
  try {
    // Giả lập delay Upload API
    await new Promise(resolve => setTimeout(resolve, 1000))
    message.success({ content: 'Cập nhật ảnh đại diện thành công!', key: 'avatar' })
  } catch (error) {
    message.error({ content: 'Lỗi tải ảnh lên.', key: 'avatar' })
    avatarPreview.value = null
  }
}

// --- UPDATE PROFILE LOGIC ---
const isEditModalVisible = ref(false)
const isSaving = ref(false)
const phonePattern = /^0\d{9}$/
const editForm = ref({
  phone: '',
  bio: ''
})

function openEditModal() {
  editForm.value = {
    phone: teacher.value?.phone || '',
    bio: teacher.value?.bio || ''
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
    const payload = {
      ...teacher.value,
      phone: String(editForm.value.phone).trim(),
      bio: editForm.value.bio
    }
    
    // Giả lập gọi API update
    await new Promise(resolve => setTimeout(resolve, 800))
    
    teacher.value = payload
    message.success('Cập nhật hồ sơ thành công!')
    isEditModalVisible.value = false
  } catch (error) {
    message.error('Lỗi khi lưu dữ liệu. Vui lòng thử lại.')
  } finally {
    isSaving.value = false
  }
}

// --- HELPERS ---
function statusLabel(value) {
  return ({ Active: 'Đang hoạt động', Inactive: 'Ngừng hoạt động', 0: 'Đang hoạt động', 1: 'Ngừng hoạt động' }[value] || 'Không xác định')
}

function statusClass(value) {
  if (value === 'Active' || value === 0) return 'bg-emerald-50 text-emerald-600 border-emerald-200'
  return 'bg-slate-50 text-slate-500 border-slate-200'
}

onMounted(async () => {
  loading.value = true
  try {
    if (auth.user?.referenceId) {
      teacher.value = await teacherApi.getById(auth.user.referenceId).catch(() => null)
    }
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
/* Tuỳ chỉnh thanh cuộn cho Textarea */
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: #f1f5f9; 
  border-radius: 4px;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #cbd5e1; 
  border-radius: 4px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #94a3b8; 
}
</style>
