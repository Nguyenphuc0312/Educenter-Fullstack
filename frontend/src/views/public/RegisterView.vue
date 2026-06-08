<template>
  <div class="flex items-center justify-center min-h-[90vh] px-4 py-12">
    <div class="w-full max-w-md bg-card-base border border-base rounded-3xl p-8 shadow-xl animate-fade-in">
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
          <input id="password" v-model="formData.password" type="password" required placeholder="••••••••" :disabled="isLoading"
            class="w-full px-4 py-3 text-sm rounded-xl border border-base bg-card-base text-base-primary placeholder:text-base-muted focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 transition-all disabled:opacity-55" />
        </div>

        <div class="flex flex-col gap-1.5">
          <label for="confirmPassword" class="text-xs font-semibold text-base-primary">Nhập lại mật khẩu <span class="text-red-500">*</span></label>
          <input id="confirmPassword" v-model="formData.confirmPassword" type="password" required placeholder="••••••••" :disabled="isLoading"
            class="w-full px-4 py-3 text-sm rounded-xl border border-base bg-card-base text-base-primary placeholder:text-base-muted focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 transition-all disabled:opacity-55" />
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
import { authApi } from '../../api/authApi'
import { message } from 'ant-design-vue'

const router = useRouter()
const isLoading = ref(false)
const errorMsg = ref('')
const formData = reactive({
  username: '',
  password: '',
  confirmPassword: '',
  fullName: '',
  email: '',
  phone: '',
})

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

  errorMsg.value = ''
  isLoading.value = true

  try {
    await authApi.register({ username, password, fullName, email, phone, role: 'Student' })
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
