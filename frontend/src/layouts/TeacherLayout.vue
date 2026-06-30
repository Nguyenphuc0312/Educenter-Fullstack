<template>
  <div class="min-h-screen flex flex-col md:flex-row admin-shell" data-role="teacher">
    <SidebarNav :menuItems="menuItems" roleTitle="Giảng viên" />
    <main class="flex-grow p-4 md:p-8 md:pl-72 pt-20 md:pt-8 min-w-0 admin-content">
      <router-view />
    </main>
    <AIChatbot role="teacher" />
  </div>
</template>

<script setup>
import SidebarNav from '../components/layout/SidebarNav.vue'
import AIChatbot from '../components/AIChatbot.vue'

const menuItems = [
  { to: '/teacher/dashboard', label: 'Tổng quan', icon: 'LayoutDashboard' },
  { to: '/teacher/classes', label: 'Lớp phụ trách', icon: 'Users' },
  { to: '/teacher/schedule', label: 'Lịch tuần', icon: 'Calendar' },
  { to: '/teacher/attendance', label: 'Điểm danh', icon: 'UserCheck' },
  { to: '/teacher/results', label: 'Nhập kết quả', icon: 'BarChart3' },
  { to: '/teacher/materials', label: 'Tạo tài liệu (AI) 📄', icon: 'FileText' },
  { to: '/teacher/profile', label: 'Hồ sơ cá nhân', icon: 'User' }
]
</script>
