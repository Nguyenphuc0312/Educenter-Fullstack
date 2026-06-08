<template>
  <button
    @click="themeStore.toggle"
    aria-label="Toggle theme"
    class="relative flex items-center justify-center w-9 h-9 rounded-full border border-base bg-card-base text-base-secondary hover:text-blue-600 dark:hover:text-blue-400 transition-all duration-200 hover:scale-105 active:scale-95 cursor-pointer"
  >
    <transition name="fade-rotate" mode="out-in">
      <div :key="themeStore.isDark ? 'moon' : 'sun'">
        <!-- Sun Icon (shown in dark mode to switch to light) -->
        <svg v-if="themeStore.isDark" xmlns="http://www.w3.org/2000/svg" width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-sun">
          <circle cx="12" cy="12" r="4"/>
          <path d="M12 2v2"/>
          <path d="M12 20v2"/>
          <path d="m4.93 4.93 1.41 1.41"/>
          <path d="m17.66 17.66 1.41 1.41"/>
          <path d="M2 12h2"/>
          <path d="M20 12h2"/>
          <path d="m6.34 17.66-1.41 1.41"/>
          <path d="m19.07 4.93-1.41 1.41"/>
        </svg>
        <!-- Moon Icon (shown in light mode to switch to dark) -->
        <svg v-else xmlns="http://www.w3.org/2000/svg" width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-moon">
          <path d="M12 3a6 6 0 0 0 9 9 9 9 0 1 1-9-9Z"/>
        </svg>
      </div>
    </transition>
  </button>
</template>

<script setup>
import { useThemeStore } from '../stores/theme';

const themeStore = useThemeStore();
</script>

<style scoped>
.fade-rotate-enter-active,
.fade-rotate-leave-active {
  transition: all 0.2s ease-out;
}

.fade-rotate-enter-from {
  opacity: 0;
  transform: rotate(-90deg) scale(0.8);
}

.fade-rotate-leave-to {
  opacity: 0;
  transform: rotate(90deg) scale(0.8);
}
</style>
