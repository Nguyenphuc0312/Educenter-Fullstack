<template>
  <div class="flex flex-col items-center justify-center min-h-[70vh] px-4 text-center">
    <div class="w-16 h-16 rounded-2xl bg-slate-100 dark:bg-slate-800/50 flex items-center justify-center text-slate-500 dark:text-slate-400 mb-6 animate-scale-in">
      <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M9.5 2A2.5 2.5 0 0 1 12 4.5v15a2.5 2.5 0 0 1-4.96.44 2.5 2.5 0 0 1-2.96-3.08 3 3 0 0 1-.34-5.58 2.5 2.5 0 0 1 1.32-4.24 2.5 2.5 0 0 1 1.98-3A2.5 2.5 0 0 1 9.5 2Z"/>
        <path d="M14.5 2A2.5 2.5 0 0 0 12 4.5v15a2.5 2.5 0 0 0 4.96.44 2.5 2.5 0 0 0 2.96-3.08 3 3 0 0 0 .34-5.58 2.5 2.5 0 0 0-1.32-4.24 2.5 2.5 0 0 0-1.98-3A2.5 2.5 0 0 0 14.5 2Z"/>
      </svg>
    </div>
    <h1 class="text-4xl font-extrabold text-base-primary tracking-tight mb-2">404</h1>
    <p class="text-lg font-semibold text-base-primary mb-1">Trang không tìm thấy</p>
    <p class="text-sm text-base-secondary max-w-md mb-8">
      Trang bạn đang tìm kiếm không tồn tại hoặc đã bị xóa. Hãy kiểm tra lại địa chỉ URL.
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
