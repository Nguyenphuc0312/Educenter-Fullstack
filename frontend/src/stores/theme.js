import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useThemeStore = defineStore('theme', () => {
  const isDark = ref(localStorage.getItem('educentertheme') === 'dark' || 
    (!localStorage.getItem('educentertheme') && window.matchMedia('(prefers-color-scheme: dark)').matches));

  function toggle() {
    isDark.value = !isDark.value;
    updateTheme();
  }

  function updateTheme() {
    const root = document.documentElement;
    if (isDark.value) {
      root.classList.add('dark');
      localStorage.setItem('educentertheme', 'dark');
    } else {
      root.classList.remove('dark');
      localStorage.setItem('educentertheme', 'light');
    }
  }

  // Initialize
  updateTheme();

  return { isDark, toggle, updateTheme };
});
