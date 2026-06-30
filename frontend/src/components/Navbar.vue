<template>
  <div>
    <nav
      class="fixed top-0 left-0 right-0 z-50 transition-all duration-300"
      :class="scrolled ? 'bg-page/95 backdrop-blur-md border-b border-base shadow-sm' : 'bg-transparent'"
    >
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <!-- Logo -->
          <router-link to="/" @click="e => handleNavClick(e, '#home')" class="flex items-center gap-2.5 group">
            <div class="w-8 h-8 rounded-lg gradient-primary flex items-center justify-center">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-white">
                <path d="M21.42 10.922a1 1 0 0 0-.019-1.838L12.83 5.18a2 2 0 0 0-1.66 0L2.6 9.08a1 1 0 0 0 0 1.832l8.57 3.908a2 2 0 0 0 1.66 0z"/>
                <path d="M6 12v5c0 2 2 3 6 3s6-1 6-3v-5"/>
              </svg>
            </div>
            <span class="text-lg font-bold text-base-primary">
              Edu<span class="text-blue-600 dark:text-blue-400">Center</span>
            </span>
          </router-link>

          <!-- Desktop Nav -->
          <div class="hidden lg:flex items-center gap-1">
            <a
              v-for="link in navLinks"
              :key="link.href"
              :href="link.href"
              @click="e => handleNavClick(e, link.href)"
              class="px-3 py-2 text-sm font-medium text-base-secondary hover:text-blue-600 dark:hover:text-blue-400 rounded-lg hover:bg-blue-50 dark:hover:bg-blue-950/40 transition-all duration-200"
            >
              {{ link.label }}
            </a>
          </div>

          <!-- Right -->
          <div class="hidden lg:flex items-center gap-3">
            <ThemeToggle />
            <template v-if="isAuthenticated">
              <div class="flex items-center gap-2">
                <router-link
                  :to="getDashboardLink()"
                  class="flex items-center gap-1.5 px-4 py-2 text-sm font-medium text-white text-on-primary gradient-primary rounded-lg hover:opacity-90 transition-opacity"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <rect width="7" height="9" x="3" y="3" rx="1"/>
                    <rect width="7" height="5" x="14" y="3" rx="1"/>
                    <rect width="7" height="9" x="14" y="12" rx="1"/>
                    <rect width="7" height="5" x="3" y="16" rx="1"/>
                  </svg>
                  Portal
                </router-link>
                <button
                  @click="handleLogout"
                  class="flex items-center gap-1.5 px-4 py-2 text-sm font-medium text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-950/40 rounded-lg border border-red-200 dark:border-red-900/50 transition-all duration-200 cursor-pointer"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
                    <polyline points="16 17 21 12 16 7"/>
                    <line x1="21" x2="9" y1="12" y2="12"/>
                  </svg>
                  Đăng xuất
                </button>
              </div>
            </template>
            <template v-else>
              <button
                id="btn-login"
                @click="router.push('/login')"
                class="px-4 py-2 text-sm font-medium text-base-secondary hover:text-blue-600 dark:hover:text-blue-400 rounded-lg border border-base hover:border-blue-300 dark:hover:border-blue-700 transition-all duration-200 cursor-pointer"
              >
                Đăng nhập
              </button>
              <button
                id="btn-register-nav"
                @click="router.push('/register')"
                class="px-4 py-2 text-sm font-semibold text-white text-on-primary gradient-primary rounded-lg transition-opacity hover:opacity-90 cursor-pointer"
              >
                Đăng ký học
              </button>
            </template>
          </div>

          <!-- Mobile right -->
          <div class="lg:hidden flex items-center gap-2">
            <ThemeToggle />
            <button
              id="btn-mobile-menu"
              @click="mobileOpen = !mobileOpen"
              aria-label="Toggle menu"
              class="w-9 h-9 flex items-center justify-center rounded-lg border border-base bg-card-base text-base-secondary hover:text-blue-600 transition-colors cursor-pointer"
            >
              <svg v-if="mobileOpen" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <line x1="18" x2="6" y1="6" y2="18"/>
                <line x1="6" x2="18" y1="6" y2="18"/>
              </svg>
              <svg v-else xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <line x1="4" x2="20" y1="12" y2="12"/>
                <line x1="4" x2="20" y1="6" y2="6"/>
                <line x1="4" x2="20" y1="18" y2="18"/>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </nav>

    <!-- Mobile Menu -->
    <transition name="mobile-menu">
      <div
        v-if="mobileOpen"
        class="fixed top-16 left-0 right-0 z-40 bg-page border-b border-base shadow-lg lg:hidden"
      >
        <div class="max-w-7xl mx-auto px-4 py-4 flex flex-col gap-1">
          <a
            v-for="link in navLinks"
            :key="link.href"
            :href="link.href"
            @click="e => handleNavClick(e, link.href)"
            class="flex items-center justify-between px-4 py-3 rounded-xl text-sm font-medium text-base-secondary hover:text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-950/40 transition-all"
          >
            {{ link.label }}
            <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="opacity-40">
              <path d="m9 18 6-6-6-6"/>
            </svg>
          </a>
          <div class="flex gap-3 mt-3 pt-3 border-t border-base">
            <template v-if="isAuthenticated">
              <router-link
                :to="getDashboardLink()"
                @click="mobileOpen = false"
                class="flex-1 py-2.5 text-center text-sm font-semibold text-white text-on-primary gradient-primary rounded-xl hover:opacity-90 transition-opacity"
              >
                Portal
              </router-link>
              <button
                @click="handleLogoutMobile"
                class="flex-1 py-2.5 text-sm font-medium border border-red-200 dark:border-red-900/50 rounded-xl text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-950/40 transition-all cursor-pointer"
              >
                Đăng xuất
              </button>
            </template>
            <template v-else>
              <button
                @click="goToLoginMobile"
                class="flex-1 py-2.5 text-sm font-medium border border-base rounded-xl text-base-secondary hover:text-blue-600 hover:border-blue-300 transition-all cursor-pointer"
              >
                Đăng nhập
              </button>
              <button
                @click="goToRegisterMobile"
                class="flex-1 py-2.5 text-sm font-semibold text-white text-on-primary gradient-primary rounded-xl hover:opacity-90 transition-opacity cursor-pointer"
              >
                Đăng ký học
              </button>
            </template>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { getRoleHomePath } from '../lib/constants';
import ThemeToggle from './ThemeToggle.vue';

const navLinks = [
  { href: '#home', label: 'Trang chủ' },
  { href: '#courses', label: 'Khóa học' },
  { href: '#best-selling', label: 'Bán chạy' },
  { href: '#teachers', label: 'Giảng viên' },
  { href: '#certificates', label: 'Chứng chỉ' },
  { href: '#testimonials', label: 'Đánh giá' },
  { href: '#contact', label: 'Liên hệ' },
];

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();

const scrolled = ref(false);
const mobileOpen = ref(false);

const isAuthenticated = computed(() => authStore.isAuthenticated);
const user = computed(() => authStore.user);

function handleScroll() {
  scrolled.value = window.scrollY > 20;
}

onMounted(() => {
  window.addEventListener('scroll', handleScroll, { passive: true });
});

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll);
});

function handleNavClick(e, href) {
  e.preventDefault();
  mobileOpen.value = false;
  if (route.path !== '/') {
    router.push('/').then(() => {
      setTimeout(() => {
        const target = document.querySelector(href);
        if (target) target.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }, 200);
    });
  } else {
    const target = document.querySelector(href);
    if (target) target.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
}

function getDashboardLink() {
  return getRoleHomePath(user.value?.role);
}

function handleLogout() {
  authStore.logout().then(() => {
    router.push('/');
  });
}

function handleLogoutMobile() {
  mobileOpen.value = false;
  handleLogout();
}

function goToLoginMobile() {
  mobileOpen.value = false;
  router.push('/login');
}

function goToRegisterMobile() {
  mobileOpen.value = false;
  router.push('/register');
}
</script>

<style scoped>
.mobile-menu-enter-active,
.mobile-menu-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.mobile-menu-enter-from,
.mobile-menu-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>
