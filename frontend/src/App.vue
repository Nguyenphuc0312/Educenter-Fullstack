<template>
  <a-config-provider :theme="themeConfig">
    <router-view />
  </a-config-provider>
</template>

<script setup>
import { onMounted, onUnmounted, computed } from 'vue';
import { theme } from 'ant-design-vue';
import { useThemeStore } from './stores/theme';
import { useAuthStore } from './stores/auth';

const themeStore = useThemeStore();
const authStore = useAuthStore();
let sessionWatcherId = null;

const themeConfig = computed(() => {
  return {
    algorithm: themeStore.isDark ? theme.darkAlgorithm : theme.defaultAlgorithm,
    token: {
      colorPrimary: '#2563eb', // primary blue
      borderRadius: 12,
      fontFamily: "'Inter', 'Be Vietnam Pro', system-ui, sans-serif",
    }
  };
});

onMounted(() => {
  themeStore.updateTheme();
  authStore.restoreSession();
  sessionWatcherId = window.setInterval(() => {
    if (authStore.isAuthenticated) {
      authStore.fetchMe().catch(() => {});
    }
  }, 5 * 60 * 1000); // Kiểm tra session mỗi 5 phút
});

onUnmounted(() => {
  if (sessionWatcherId) {
    window.clearInterval(sessionWatcherId);
  }
});
</script>

<style>
/* Global style adjustments if any */
</style>
