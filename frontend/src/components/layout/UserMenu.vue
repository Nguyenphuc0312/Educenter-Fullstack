<template>
  <div class="relative" ref="dropdownRef">
    <button
      type="button"
      @click="isOpen = !isOpen"
      class="flex items-center gap-2 p-1.5 rounded-xl hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors text-left cursor-pointer"
    >
      <div class="w-9 h-9 rounded-lg gradient-primary flex items-center justify-center text-white font-bold text-sm shrink-0">
        {{ user?.fullName?.charAt(0).toUpperCase() || 'U' }}
      </div>
      <div class="hidden md:block pr-1">
        <p class="text-xs font-semibold text-base-primary leading-tight">{{ user?.fullName }}</p>
        <p class="text-[10px] text-base-tertiary leading-none mt-0.5">{{ getRoleLabel(user?.role) }}</p>
      </div>
      <!-- Chevron Down Icon -->
      <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-chevron-down text-base-tertiary hidden md:block shrink-0">
        <path d="m6 9 6 6 6-6"/>
      </svg>
    </button>

    <transition name="dropdown">
      <div
        v-if="isOpen"
        class="absolute right-0 bottom-full mb-2 w-48 bg-card-base border border-base rounded-xl p-1 shadow-lg z-50"
      >
        <div class="px-3 py-2 border-b border-base md:hidden">
          <p class="text-xs font-semibold text-base-primary">{{ user?.fullName }}</p>
          <p class="text-[10px] text-base-tertiary">{{ getRoleLabel(user?.role) }}</p>
        </div>
        <button
          type="button"
          @click="goToProfile"
          class="w-full flex items-center gap-2 px-3 py-2 text-xs text-base-secondary hover:text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-950/40 rounded-lg transition-colors cursor-pointer text-left"
        >
          <!-- User Icon -->
          <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-user">
            <path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2"/>
            <circle cx="12" cy="7" r="4"/>
          </svg>
          Hồ sơ của tôi
        </button>
        <button
          type="button"
          @click.stop="handleLogout"
          class="w-full flex items-center gap-2 px-3 py-2 text-xs text-red-600 hover:bg-red-50 dark:hover:bg-red-950/20 rounded-lg transition-colors cursor-pointer text-left"
        >
          <!-- LogOut Icon -->
          <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-log-out">
            <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
            <polyline points="16 17 21 12 16 7"/>
            <line x1="21" x2="9" y1="12" y2="12"/>
          </svg>
          Đăng xuất
        </button>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../../stores/auth';
import { getRoleLabel, getRoleProfilePath } from '../../lib/constants';

const authStore = useAuthStore();
const router = useRouter();
const isOpen = ref(false);
const dropdownRef = ref(null);

const user = computed(() => authStore.user);

function handleClickOutside(event) {
  if (dropdownRef.value && !dropdownRef.value.contains(event.target)) {
    isOpen.value = false;
  }
}

onMounted(() => {
  document.addEventListener('mousedown', handleClickOutside);
});

onUnmounted(() => {
  document.removeEventListener('mousedown', handleClickOutside);
});

function goToProfile() {
  isOpen.value = false;
  router.push(getRoleProfilePath(user.value?.role));
}

function handleLogout() {
  isOpen.value = false;
  authStore.logout();
  router.replace('/login');
}
</script>

<style scoped>
.dropdown-enter-active,
.dropdown-leave-active {
  transition: opacity 0.15s ease, transform 0.15s ease;
}

.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: translateY(8px) scale(0.95);
}
</style>
