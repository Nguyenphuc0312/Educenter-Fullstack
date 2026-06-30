<template>
  <div class="relative flex items-center justify-center min-h-[90vh] px-4 py-12 overflow-hidden">
    <div class="absolute top-1/4 left-1/4 w-80 h-80 rounded-full bg-blue-500/10 dark:bg-blue-500/5 blur-3xl pointer-events-none animate-float-slow"></div>
    <div class="absolute bottom-1/4 right-1/4 w-80 h-80 rounded-full bg-purple-500/10 dark:bg-purple-500/5 blur-3xl pointer-events-none animate-float-slower"></div>
    <div class="w-full max-w-md bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl border border-slate-200/50 dark:border-slate-800/50 rounded-3xl p-8 shadow-2xl animate-fade-in relative z-10">
      <div class="flex flex-col items-center mb-8">
        <div class="w-12 h-12 rounded-2xl gradient-primary flex items-center justify-center text-white mb-4 shadow-lg shadow-blue-500/10">
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21.42 10.922a1 1 0 0 0-.019-1.838L12.83 5.18a2 2 0 0 0-1.66 0L2.6 9.08a1 1 0 0 0 0 1.832l8.57 3.908a2 2 0 0 0 1.66 0z"/>
            <path d="M6 12v5c0 2 2 3 6 3s6-1 6-3v-5"/>
          </svg>
        </div>
        <h2 class="text-xl font-bold text-base-primary">Đăng ký học viên</h2>
        <p class="text-xs text-base-secondary mt-1">Tự tạo tài khoản học tập tại EduCenter</p>
      </div>

      <div
        v-if="errorMsg"
        class="mb-6 p-4 rounded-xl border border-red-200 dark:border-red-950/30 bg-red-50/50 dark:bg-red-950/10 text-red-600 dark:text-red-400 text-xs font-semibold flex items-start gap-2"
      >
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="shrink-0 mt-0.5">
          <circle cx="12" cy="12" r="10"/>
          <line x1="12" x2="12" y1="8" y2="12"/>
          <line x1="12" x2="12.01" y1="16" y2="16"/>
        </svg>
        <span>{{ errorMsg }}</span>
      </div>

      <form @submit.prevent="handleSubmit" class="space-y-4">
        <div class="flex flex-col gap-1.5">
          <label for="fullName" class="text-xs font-semibold text-base-primary">Họ và tên <span class="text-red-500">*</span></label>
          <input id="fullName" v-model="formData.fullName" type="text" required placeholder="Nguyễn Văn A" :disabled="isLoading"
            class="w-full px-4 py-3 text-sm rounded-xl border border-base bg-card-base text-base-primary placeholder:text-base-muted focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 transition-all disabled:opacity-55" />
        </div>

        <div class="flex flex-col gap-1.5">
          <label for="email" class="text-xs font-semibold text-base-primary">Email <span class="text-red-500">*</span></label>
          <input id="email" v-model="formData.email" type="email" required placeholder="email@example.com" :disabled="isLoading"
            class="w-full px-4 py-3 text-sm rounded-xl border border-base bg-card-base text-base-primary placeholder:text-base-muted focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 transition-all disabled:opacity-55" />
        </div>

        <div class="flex flex-col gap-1.5">
          <label for="phone" class="text-xs font-semibold text-base-primary">Số điện thoại</label>
          <input id="phone" v-model="formData.phone" type="tel" placeholder="0987654321" :disabled="isLoading"
            class="w-full px-4 py-3 text-sm rounded-xl border border-base bg-card-base text-base-primary placeholder:text-base-muted focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 transition-all disabled:opacity-55" />
        </div>

        <div class="flex flex-col gap-1.5">
          <label for="username" class="text-xs font-semibold text-base-primary">Tên đăng nhập <span class="text-red-500">*</span></label>
          <input id="username" v-model="formData.username" type="text" required placeholder="Nhập tên đăng nhập" :disabled="isLoading"
            class="w-full px-4 py-3 text-sm rounded-xl border border-base bg-card-base text-base-primary placeholder:text-base-muted focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 transition-all disabled:opacity-55" />
        </div>

        <div class="flex flex-col gap-1.5">
          <label for="password" class="text-xs font-semibold text-base-primary">Mật khẩu <span class="text-red-500">*</span></label>
          <div class="relative">
            <input id="password" v-model="formData.password" :type="showPassword ? 'text' : 'password'" required placeholder="********" autocomplete="new-password" :disabled="isLoading"
              class="w-full px-4 py-3 pr-11 text-sm rounded-xl border border-base bg-card-base text-base-primary placeholder:text-base-muted focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 transition-all disabled:opacity-55" />
            <button
              type="button"
              class="absolute inset-y-0 right-3 flex items-center text-base-muted hover:text-base-primary disabled:opacity-50"
              :disabled="isLoading"
              :aria-label="showPassword ? 'Hide password' : 'Show password'"
              @click="showPassword = !showPassword"
            >
              <svg v-if="showPassword" xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M3 3l18 18M10.58 10.58A2 2 0 0012 14a2 2 0 001.42-.58M9.88 4.24A10.46 10.46 0 0112 4c5 0 9 4.5 10 8a11.72 11.72 0 01-3.1 4.74M6.11 6.11C4.11 7.45 2.75 9.7 2 12c1 3.5 5 8 10 8 1.36 0 2.65-.33 3.8-.9" /></svg>
              <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z" /><circle cx="12" cy="12" r="3" /></svg>
            </button>
          </div>
        </div>

        <div class="flex flex-col gap-1.5">
          <label for="confirmPassword" class="text-xs font-semibold text-base-primary">Nhập lại mật khẩu <span class="text-red-500">*</span></label>
          <div class="relative">
            <input id="confirmPassword" v-model="formData.confirmPassword" :type="showConfirmPassword ? 'text' : 'password'" required placeholder="********" autocomplete="new-password" :disabled="isLoading"
              class="w-full px-4 py-3 pr-11 text-sm rounded-xl border border-base bg-card-base text-base-primary placeholder:text-base-muted focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 transition-all disabled:opacity-55" />
            <button
              type="button"
              class="absolute inset-y-0 right-3 flex items-center text-base-muted hover:text-base-primary disabled:opacity-50"
              :disabled="isLoading"
              :aria-label="showConfirmPassword ? 'Hide password' : 'Show password'"
              @click="showConfirmPassword = !showConfirmPassword"
            >
              <svg v-if="showConfirmPassword" xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M3 3l18 18M10.58 10.58A2 2 0 0012 14a2 2 0 001.42-.58M9.88 4.24A10.46 10.46 0 0112 4c5 0 9 4.5 10 8a11.72 11.72 0 01-3.1 4.74M6.11 6.11C4.11 7.45 2.75 9.7 2 12c1 3.5 5 8 10 8 1.36 0 2.65-.33 3.8-.9" /></svg>
              <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z" /><circle cx="12" cy="12" r="3" /></svg>
            </button>
          </div>
        </div>

        <button
          type="submit"
          :disabled="isLoading"
          class="w-full py-3 px-4 font-semibold text-white gradient-primary rounded-xl hover:opacity-95 transition-all flex items-center justify-center gap-2 disabled:opacity-55 hover:scale-[1.01] active:scale-[0.99] cursor-pointer"
        >
          <div v-if="isLoading" class="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
          <template v-else>
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/>
              <circle cx="9" cy="7" r="4"/>
              <line x1="19" x2="19" y1="8" y2="14"/>
              <line x1="22" x2="16" y1="11" y2="11"/>
            </svg>
            Đăng ký tài khoản
          </template>
        </button>
      </form>

      <div class="text-center mt-6 pt-6 border-t border-base">
        <p class="text-xs text-base-secondary">
          Bạn đã có tài khoản?
          <router-link to="/login" class="text-blue-600 dark:text-blue-400 font-semibold hover:underline ml-1">Đăng nhập ngay</router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { authApi } from "@/api/authApi"
import { message } from 'ant-design-vue'

const router = useRouter()
const isLoading = ref(false)
const errorMsg = ref('')
const showPassword = ref(false)
const showConfirmPassword = ref(false)
const formData = reactive({
  username: '',
  password: '',
  confirmPassword: '',
  fullName: '',
  email: '',
  phone: '',
})
const PHONE_PATTERN = /^0\d{9}$/

async function handleSubmit() {
  const { username, password, confirmPassword, fullName, email, phone } = formData

  if (!username || !password || !fullName || !email) {
    errorMsg.value = 'Vui lòng điền đầy đủ các thông tin bắt buộc.'
    return
  }

  if (password !== confirmPassword) {
    errorMsg.value = 'Mật khẩu nhập lại không khớp.'
    return
  }

  if (phone && !PHONE_PATTERN.test(String(phone).trim())) {
    errorMsg.value = 'Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.'
    return
  }

  errorMsg.value = ''
  isLoading.value = true

  try {
    await authApi.register({ username, password, fullName, email, phone: String(phone || '').trim(), role: 'Student' })
    message.success('Đăng ký tài khoản học viên thành công! Vui lòng đăng nhập.')
    router.push('/login')
  } catch (err) {
    console.error(err)
    const msg = err.message || 'Có lỗi xảy ra trong quá trình đăng ký.'
    errorMsg.value = msg
    message.error(msg)
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
.animate-fade-in {
  animation: fadeInUp 0.4s ease both;
}
@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
