<template>
  <div class="flex flex-col items-center justify-center min-h-[70vh] px-4 text-center">
    <div class="w-16 h-16 rounded-2xl bg-red-50 dark:bg-red-950/30 flex items-center justify-center text-red-600 dark:text-red-400 mb-6 animate-scale-in">
      <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
        <line x1="12" x2="12" y1="8" y2="12"/>
        <line x1="12" x2="12.01" y1="16" y2="16"/>
      </svg>
    </div>
    <h1 class="text-4xl font-extrabold text-base-primary tracking-tight mb-2">403</h1>
    <p class="text-lg font-semibold text-base-primary mb-1">Không có quyền truy cập</p>
    <p class="text-sm text-base-secondary max-w-md mb-8">
      Tài khoản của bạn không được phân quyền để truy cập vào phân hệ này. Vui lòng liên hệ ban quản trị nếu đây là sự nhầm lẫn.
    </p>
    <router-link
      to="/"
      class="flex items-center gap-2 px-5 py-3 text-sm font-semibold text-white gradient-primary rounded-xl hover:opacity-90 transition-opacity shadow-lg shadow-blue-500/15"
    >
      <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>
        <polyline points="9 22 9 12 15 12 15 22"/>
      </svg>
      Về trang chủ
    </router-link>
  </div>
</template>

<script setup>
</script>

<style scoped>
.animate-scale-in {
  animation: scaleIn 0.3s ease both;
}
@keyframes scaleIn {
  from { transform: scale(0.8); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}
</style>
