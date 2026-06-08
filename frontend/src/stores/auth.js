import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { authApi } from '../api/authApi';
import { normalizeRole } from '../lib/constants';

const normalizeUser = (user) => user ? { ...user, role: normalizeRole(user.role) } : user;

export const useAuthStore = defineStore('auth', () => {
  const user = ref(null);
  const token = ref(null);
  const isAuthenticated = ref(false);
  const isLoading = ref(true);

  const role = computed(() => user.value ? user.value.role : null);

  function restoreSession() {
    try {
      const storedToken = localStorage.getItem('edu_token');
      const userJson = localStorage.getItem('edu_user');
      if (storedToken && userJson) {
        token.value = storedToken;
        user.value = normalizeUser(JSON.parse(userJson));
        isAuthenticated.value = true;
      } else {
        token.value = null;
        user.value = null;
        isAuthenticated.value = false;
      }
    } catch (e) {
      token.value = null;
      user.value = null;
      isAuthenticated.value = false;
    } finally {
      isLoading.value = false;
    }
  }

  async function login(credentials) {
    isLoading.value = true;
    try {
      const data = await authApi.login(credentials);
      const resolvedToken = data.accessToken || data.token;
      const resolvedUser = normalizeUser(data.user);

      token.value = resolvedToken;
      user.value = resolvedUser;
      isAuthenticated.value = true;

      localStorage.setItem('edu_token', resolvedToken);
      localStorage.setItem('edu_user', JSON.stringify(resolvedUser));

      return resolvedUser;
    } catch (err) {
      throw err;
    } finally {
      isLoading.value = false;
    }
  }

  async function logout() {
    token.value = null;
    user.value = null;
    isAuthenticated.value = false;
    isLoading.value = false;
    localStorage.removeItem('edu_token');
    localStorage.removeItem('edu_user');
    authApi.logout().catch(() => {});
  }

  async function fetchMe() {
    try {
      const resolvedUser = normalizeUser(await authApi.me());
      user.value = resolvedUser;
      localStorage.setItem('edu_user', JSON.stringify(resolvedUser));
      return resolvedUser;
    } catch (err) {
      if (err.status === 401) {
        logout();
      }
      throw err;
    }
  }

  // Restore immediately
  restoreSession();

  return {
    user,
    token,
    isAuthenticated,
    isLoading,
    role,
    restoreSession,
    login,
    logout,
    fetchMe
  };
});
