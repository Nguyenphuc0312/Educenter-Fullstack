<template>
  <router-view />
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue';
import { useThemeStore } from './stores/theme';
import { useAuthStore } from './stores/auth';

const themeStore = useThemeStore();
const authStore = useAuthStore();
let sessionWatcherId = null;

onMounted(() => {
  themeStore.updateTheme();
  authStore.restoreSession();
  sessionWatcherId = window.setInterval(() => {
    if (authStore.isAuthenticated) {
      authStore.fetchMe().catch(() => {});
    }
  }, 5000);
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
