<template>
  <div class="min-h-screen flex flex-col md:flex-row admin-shell" data-role="admin">
    <SidebarNav :menuItems="menuItems" roleTitle="Admin" />
    <main class="flex-grow p-4 md:p-8 md:pl-72 pt-20 md:pt-8 min-w-0 admin-content">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import SidebarNav from '../components/layout/SidebarNav.vue';

// Menu items kept for compatibility (SidebarNav renders its own grouped list for Admin).
const menuItems = [
  { to: 'dashboard', label: 'Tổng quan', icon: 'LayoutDashboard' },
  { to: 'courses', label: 'Khóa học', icon: 'BookOpen' },
  { to: 'classes', label: 'Lớp học', icon: 'Users' },
  { to: 'schedules', label: 'Lịch học', icon: 'Calendar' },
  { to: 'teachers', label: 'Giảng viên', icon: 'Award' },
  { to: 'students', label: 'Học viên', icon: 'GraduationCap' },
  { to: 'enrollments', label: 'Ghi danh', icon: 'FileText' },
  { to: 'attendance', label: 'Điểm danh', icon: 'UserCheck' },
  { to: 'results', label: 'Kết quả học tập', icon: 'BarChart3' },
  { to: 'accounts', label: 'Tài khoản', icon: 'UserCheck' },
  { to: 'tuition', label: 'Học phí', icon: 'CreditCard' },
  { to: 'payments', label: 'Thanh toán', icon: 'DollarSign' },
  { to: 'reports', label: 'Báo cáo & Thống kê', icon: 'BarChart3' }
];
</script>
