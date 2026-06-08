<template>
  <div class="min-h-screen bg-page flex flex-col md:flex-row">
    <SidebarNav :menuItems="menuItems" roleTitle="Học viên" />
    <main class="flex-grow p-4 md:p-8 md:pl-72 pt-20 md:pt-8 min-w-0">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import SidebarNav from '../components/layout/SidebarNav.vue';
import '../assets/student.css';

const menuItems = [
  { to: '/student/dashboard', label: 'Tổng quan', icon: 'LayoutDashboard' },
  { to: '/student/courses', label: 'Khóa học của tôi', icon: 'BookOpen' },
  { to: '/student/schedule', label: 'Lịch học', icon: 'Calendar' },
  { to: '/student/attendance', label: 'Chuyên cần', icon: 'UserCheck' },
  { to: '/student/results', label: 'Bảng điểm', icon: 'Award' },
  { to: '/student/tuition', label: 'Học phí & Hóa đơn', icon: 'CreditCard' },
  { to: '/student/profile', label: 'Hồ sơ cá nhân', icon: 'User' }
];
</script>

