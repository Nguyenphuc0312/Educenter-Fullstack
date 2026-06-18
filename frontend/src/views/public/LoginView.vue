<template>
  <div class="flex items-center justify-center min-h-[80vh] px-4 py-12">
    <div
      class="w-full max-w-md bg-card-base border border-base rounded-3xl p-8 shadow-xl animate-fade-in"
    >
      <div class="flex flex-col items-center mb-8">
        <div class="w-12 h-12 rounded-2xl gradient-primary flex items-center justify-center text-white mb-4 shadow-lg shadow-blue-500/10">
          <!-- GraduationCap SVG -->
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21.42 10.922a1 1 0 0 0-.019-1.838L12.83 5.18a2 2 0 0 0-1.66 0L2.6 9.08a1 1 0 0 0 0 1.832l8.57 3.908a2 2 0 0 0 1.66 0z"/>
            <path d="M6 12v5c0 2 2 3 6 3s6-1 6-3v-5"/>
          </svg>
        </div>
        <h2 class="text-xl font-bold text-base-primary">Chào mừng trở lại</h2>
        <p class="text-xs text-base-secondary mt-1">Đăng nhập vào cổng thông tin EduCenter</p>
      </div>

      <div
        v-if="errorMsg"
        class="mb-6 p-4 rounded-xl border border-red-200 dark:border-red-950/30 bg-red-50/50 dark:bg-red-950/10 text-red-600 dark:text-red-400 text-xs font-semibold flex items-start gap-2"
      >
        <!-- AlertCircle SVG -->
        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="shrink-0 mt-0.5">
          <circle cx="12" cy="12" r="10"/>
          <line x1="12" x2="12" y1="8" y2="12"/>
          <line x1="12" x2="12.01" y1="16" y2="16"/>
        </svg>
        <span>{{ errorMsg }}</span>
      </div>

      <form @submit.prevent="handleSubmit" class="space-y-4">
        <!-- Username Field -->
        <div class="flex flex-col gap-1.5">
          <label for="username" class="text-xs font-semibold text-base-primary">
            Tên đăng nhập <span class="text-red-500">*</span>
          </label>
          <input
            id="username"
            v-model="username"
            type="text"
            required
            placeholder="Nhập tên đăng nhập"
            autocomplete="username"
            :disabled="isLoading"
            class="w-full px-4 py-3 text-sm rounded-xl border border-base bg-card-base text-base-primary placeholder:text-base-muted focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 dark:focus:border-blue-500 transition-all disabled:opacity-55"
          />
        </div>

        <!-- Password Field -->
        <div class="flex flex-col gap-1.5">
          <label for="password" class="text-xs font-semibold text-base-primary">
            Mật khẩu <span class="text-red-500">*</span>
          </label>
          <div class="relative">
            <input
              id="password"
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              required
              placeholder="********"
              autocomplete="current-password"
              :disabled="isLoading"
              class="w-full px-4 py-3 pr-11 text-sm rounded-xl border border-base bg-card-base text-base-primary placeholder:text-base-muted focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 dark:focus:border-blue-500 transition-all disabled:opacity-55"
            />
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

        <div class="text-right">
          <a href="#contact" class="text-xs text-blue-600 dark:text-blue-400 hover:underline">
            Quên mật khẩu?
          </a>
        </div>

        <button
          type="submit"
          :disabled="isLoading"
          class="w-full py-3 px-4 font-semibold text-white gradient-primary rounded-xl hover:opacity-95 transition-all flex items-center justify-center gap-2 disabled:opacity-55 hover:scale-[1.01] active:scale-[0.99] cursor-pointer"
        >
          <div v-if="isLoading" class="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
          <template v-else>
            <!-- LogIn SVG -->
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4"/>
              <polyline points="10 17 15 12 10 7"/>
              <line x1="15" x2="3" y1="12" y2="12"/>
            </svg>
            Đăng nhập
          </template>
        </button>
      </form>

      <div class="text-center mt-6 pt-6 border-t border-base">
        <p class="text-xs text-base-secondary">
          Bạn chưa có tài khoản?
          <router-link to="/register" class="text-blue-600 dark:text-blue-400 font-semibold hover:underline ml-1">
            Đăng ký học ngay
          </router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { getRoleHomePath } from '../../lib/constants'
import { message } from 'ant-design-vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const username = ref('')
const password = ref('')
const showPassword = ref(false)
const errorMsg = ref('')

const isLoading = computed(() => authStore.isLoading)

const from = route.query?.redirect || ''

async function handleSubmit() {
  if (!username.value || !password.value) {
    errorMsg.value = 'Vui lòng điền đầy đủ tên đăng nhập và mật khẩu.'
    return
  }

  errorMsg.value = ''
  try {
    const user = await authStore.login({ username: username.value, password: password.value })
    message.success(`Đăng nhập thành công! Chào mừng ${user.fullName}`)

    let targetPath = from || getRoleHomePath(user.role)
    router.replace(targetPath)
  } catch (err) {
    console.error(err)
    const msg = err.message || 'Tên đăng nhập hoặc mật khẩu không đúng.'
    errorMsg.value = msg
    message.error(msg)
  }
}
</script>

<style scoped>
.animate-fade-in {
  animation: fadeInUp 0.4s ease both;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
