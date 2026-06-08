<template>
  <div>
    <!-- Desktop Sidebar (visible on md+) -->
    <aside class="hidden md:block fixed top-0 bottom-0 left-0 w-64 z-30">
      <div class="flex flex-col h-full bg-card-base border-r border-base">
        <!-- Brand Header -->
        <div class="flex items-center gap-2.5 px-6 h-16 border-b border-base shrink-0">
          <router-link to="/" class="flex items-center gap-2 group">
            <div class="w-8 h-8 rounded-lg gradient-primary flex items-center justify-center">
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M21.42 10.922a1 1 0 0 0-.019-1.838L12.83 5.18a2 2 0 0 0-1.66 0L2.6 9.08a1 1 0 0 0 0 1.832l8.57 3.908a2 2 0 0 0 1.66 0z"/>
                <path d="M6 12v5c0 2 2 3 6 3s6-1 6-3v-5"/>
              </svg>
            </div>
            <div>
              <span class="text-sm font-bold text-base-primary leading-none block">
                Edu<span class="text-blue-600 dark:text-blue-400">Center</span>
              </span>
              <span class="text-[10px] font-semibold text-blue-600 dark:text-blue-400 uppercase tracking-wider block mt-0.5">
                {{ roleTitle }}
              </span>
            </div>
          </router-link>
        </div>

        <!-- Navigation Links -->
        <nav class="flex-1 px-4 py-6 overflow-y-auto space-y-1 scrollbar-hide">
          <template v-if="roleTitle === 'Admin'">
            <!-- Group 1: General -->
            <div class="mb-2">
              <router-link
                v-for="item in generalItems"
                :key="item.to"
                :to="`/admin/${item.to}`"
                class="admin-sidebar-link flex items-center gap-2.5 px-3 py-2 rounded-lg text-xs font-semibold transition-all duration-200"
                :class="isRouteActive(item.to) ? 'is-active' : 'text-base-secondary'"
              >
                <component :is="item.icon" class="w-3.5 h-3.5 shrink-0" />
                <span>{{ item.label }}</span>
              </router-link>
            </div>

            <!-- Group 2: Course & Schedule -->
            <div class="mb-2">
              <div class="admin-sidebar-group-label">Course & Schedule</div>
              <router-link
                v-for="item in courseScheduleItems"
                :key="item.to"
                :to="`/admin/${item.to}`"
                class="admin-sidebar-link flex items-center gap-2.5 px-3 py-1.5 rounded-lg text-xs font-semibold transition-all duration-200"
                :class="isRouteActive(item.to) ? 'is-active' : 'text-base-secondary'"
              >
                <component :is="item.icon" class="w-3.5 h-3.5 shrink-0" />
                <span>{{ item.label }}</span>
              </router-link>
            </div>

            <!-- Group 3: Student & Attendance -->
            <div class="mb-2">
              <div class="admin-sidebar-group-label">Student & Attendance</div>
              <router-link
                v-for="item in studentAttendanceItems"
                :key="item.to"
                :to="`/admin/${item.to}`"
                class="admin-sidebar-link flex items-center gap-2.5 px-3 py-1.5 rounded-lg text-xs font-semibold transition-all duration-200"
                :class="isRouteActive(item.to) ? 'is-active' : 'text-base-secondary'"
              >
                <component :is="item.icon" class="w-3.5 h-3.5 shrink-0" />
                <span>{{ item.label }}</span>
              </router-link>
            </div>

            <!-- Group 4: Payment & Report -->
            <div class="mb-2">
              <div class="admin-sidebar-group-label">Payment & Report</div>
              <router-link
                v-for="item in paymentReportItems"
                :key="item.to"
                :to="`/admin/${item.to}`"
                class="admin-sidebar-link flex items-center gap-2.5 px-3 py-1.5 rounded-lg text-xs font-semibold transition-all duration-200"
                :class="isRouteActive(item.to) ? 'is-active' : 'text-base-secondary'"
              >
                <component :is="item.icon" class="w-3.5 h-3.5 shrink-0" />
                <span>{{ item.label }}</span>
              </router-link>
            </div>
          </template>

          <template v-else>
            <router-link
              v-for="item in normalizedMenuItems"
              :key="item.to"
              :to="item.to"
              @click="mobileOpen = false"
              class="flex items-center gap-3 px-4 py-3 rounded-xl text-xs font-semibold transition-all duration-200"
              :class="isPathActive(item.to) ? 'gradient-primary text-white shadow-md shadow-blue-500/10' : 'text-base-secondary hover:text-blue-600 dark:hover:text-blue-400 hover:bg-slate-100 dark:hover:bg-slate-800/50'"
            >
              <component :is="item.iconComponent" class="w-4 h-4 shrink-0" />
              <span>{{ item.label }}</span>
            </router-link>
          </template>
        </nav>

        <!-- Bottom Footer Actions -->
        <div class="p-4 border-t flex items-center justify-between gap-2 shrink-0" style="border-color: var(--admin-border); background: var(--admin-surface-2);">
          <UserMenu />
          <ThemeToggle />
        </div>
      </div>
    </aside>

    <!-- Mobile Header (visible on <md) -->
    <header class="md:hidden fixed top-0 left-0 right-0 h-16 bg-card-base border-b border-base flex items-center justify-between px-4 z-40">
      <router-link to="/" class="flex items-center gap-2 group">
        <div class="w-8 h-8 rounded-lg gradient-primary flex items-center justify-center">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M21.42 10.922a1 1 0 0 0-.019-1.838L12.83 5.18a2 2 0 0 0-1.66 0L2.6 9.08a1 1 0 0 0 0 1.832l8.57 3.908a2 2 0 0 0 1.66 0z"/>
            <path d="M6 12v5c0 2 2 3 6 3s6-1 6-3v-5"/>
          </svg>
        </div>
        <div>
          <span class="text-sm font-bold text-base-primary leading-none block">
            Edu<span class="text-blue-600 dark:text-blue-400">Center</span>
          </span>
          <span class="text-[10px] font-semibold text-blue-600 dark:text-blue-400 uppercase tracking-wider block mt-0.5">
            {{ roleTitle }}
          </span>
        </div>
      </router-link>

      <button
        @click="mobileOpen = !mobileOpen"
        class="w-9 h-9 flex items-center justify-center rounded-lg border border-base bg-card-base text-base-secondary hover:text-blue-600 cursor-pointer"
      >
        <svg v-if="mobileOpen" xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <line x1="18" x2="6" y1="6" y2="18"/>
          <line x1="6" x2="18" y1="6" y2="18"/>
        </svg>
        <svg v-else xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <line x1="4" x2="20" y1="12" y2="12"/>
          <line x1="4" x2="20" y1="6" y2="6"/>
          <line x1="4" x2="20" y1="18" y2="18"/>
        </svg>
      </button>
    </header>

    <!-- Mobile Drawer (visible on <md) -->
    <transition name="fade">
      <div v-if="mobileOpen && isMobileViewport" class="mobile-drawer-layer md:hidden fixed inset-0 z-40 flex">
        <!-- Backdrop -->
        <div @click="mobileOpen = false" class="fixed inset-0 bg-slate-900/60 backdrop-blur-sm"></div>
        <!-- Drawer Content -->
        <transition name="slide">
          <div class="relative flex flex-col w-64 max-w-xs h-full z-10">
            <div class="flex flex-col h-full bg-card-base border-r border-base">
              <!-- Brand Header -->
              <div class="flex items-center gap-2.5 px-6 h-16 border-b border-base shrink-0">
                <router-link to="/" class="flex items-center gap-2 group" @click="mobileOpen = false">
                  <div class="w-8 h-8 rounded-lg gradient-primary flex items-center justify-center">
                    <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                      <path d="M21.42 10.922a1 1 0 0 0-.019-1.838L12.83 5.18a2 2 0 0 0-1.66 0L2.6 9.08a1 1 0 0 0 0 1.832l8.57 3.908a2 2 0 0 0 1.66 0z"/>
                      <path d="M6 12v5c0 2 2 3 6 3s6-1 6-3v-5"/>
                    </svg>
                  </div>
                  <div>
                    <span class="text-sm font-bold text-base-primary leading-none block">
                      Edu<span class="text-blue-600 dark:text-blue-400">Center</span>
                    </span>
                    <span class="text-[10px] font-semibold text-blue-600 dark:text-blue-400 uppercase tracking-wider block mt-0.5">
                      {{ roleTitle }}
                    </span>
                  </div>
                </router-link>
              </div>

              <!-- Navigation Links -->
              <nav class="flex-1 px-4 py-6 overflow-y-auto space-y-1 scrollbar-hide">
                <template v-if="roleTitle === 'Admin'">
                  <!-- Group 1: General -->
                  <div class="mb-4">
                    <router-link
                      v-for="item in generalItems"
                      :key="item.to"
                      :to="`/admin/${item.to}`"
                      @click="mobileOpen = false"
                      class="admin-sidebar-link flex items-center gap-3 px-4 py-3 rounded-xl text-xs font-semibold transition-all duration-200"
                      :class="isRouteActive(item.to) ? 'is-active' : 'text-base-secondary'"
                    >
                      <component :is="item.icon" class="w-4 h-4 shrink-0" />
                      <span>{{ item.label }}</span>
                    </router-link>
                  </div>

                  <!-- Group 2: Course & Schedule -->
                  <div class="mb-4">
                    <div class="admin-sidebar-group-label" style="padding-left: 16px;">Course & Schedule</div>
                    <router-link
                      v-for="item in courseScheduleItems"
                      :key="item.to"
                      :to="`/admin/${item.to}`"
                      @click="mobileOpen = false"
                      class="admin-sidebar-link flex items-center gap-3 px-4 py-2.5 rounded-xl text-xs font-semibold transition-all duration-200"
                      :class="isRouteActive(item.to) ? 'is-active' : 'text-base-secondary'"
                    >
                      <component :is="item.icon" class="w-4 h-4 shrink-0" />
                      <span>{{ item.label }}</span>
                    </router-link>
                  </div>

                  <!-- Group 3: Student & Attendance -->
                  <div class="mb-4">
                    <div class="admin-sidebar-group-label" style="padding-left: 16px;">Student & Attendance</div>
                    <router-link
                      v-for="item in studentAttendanceItems"
                      :key="item.to"
                      :to="`/admin/${item.to}`"
                      @click="mobileOpen = false"
                      class="admin-sidebar-link flex items-center gap-3 px-4 py-2.5 rounded-xl text-xs font-semibold transition-all duration-200"
                      :class="isRouteActive(item.to) ? 'is-active' : 'text-base-secondary'"
                    >
                      <component :is="item.icon" class="w-4 h-4 shrink-0" />
                      <span>{{ item.label }}</span>
                    </router-link>
                  </div>

                  <!-- Group 4: Payment & Report -->
                  <div class="mb-4">
                    <div class="admin-sidebar-group-label" style="padding-left: 16px;">Payment & Report</div>
                    <router-link
                      v-for="item in paymentReportItems"
                      :key="item.to"
                      :to="`/admin/${item.to}`"
                      @click="mobileOpen = false"
                      class="admin-sidebar-link flex items-center gap-3 px-4 py-2.5 rounded-xl text-xs font-semibold transition-all duration-200"
                      :class="isRouteActive(item.to) ? 'is-active' : 'text-base-secondary'"
                    >
                      <component :is="item.icon" class="w-4 h-4 shrink-0" />
                      <span>{{ item.label }}</span>
                    </router-link>
                  </div>
                </template>

                <template v-else>
                  <router-link
                    v-for="item in normalizedMenuItems"
                    :key="item.to"
                    :to="item.to"
                    @click="mobileOpen = false"
                    class="flex items-center gap-3 px-4 py-3 rounded-xl text-xs font-semibold transition-all duration-200"
                    :class="isPathActive(item.to) ? 'gradient-primary text-white shadow-md shadow-blue-500/10' : 'text-base-secondary hover:text-blue-600 dark:hover:text-blue-400 hover:bg-slate-100 dark:hover:bg-slate-800/50'"
                  >
                    <component :is="item.iconComponent" class="w-4 h-4 shrink-0" />
                    <span>{{ item.label }}</span>
                  </router-link>
                </template>
              </nav>

              <!-- Bottom Footer Actions -->
              <div class="p-4 border-t border-base flex items-center justify-between gap-2 shrink-0 bg-slate-50/50 dark:bg-slate-900/20">
                <UserMenu />
                <ThemeToggle />
              </div>
            </div>
          </div>
        </transition>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, computed, h, onMounted, onUnmounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import ThemeToggle from '../ThemeToggle.vue';
import UserMenu from './UserMenu.vue';

const props = defineProps({
  menuItems: {
    type: Array,
    default: () => []
  },
  roleTitle: {
    type: String,
    default: 'Portal'
  }
});

const route = useRoute();
const mobileOpen = ref(false);
const isMobileViewport = ref(false);

function syncViewportState() {
  if (typeof window === 'undefined') return;
  isMobileViewport.value = window.matchMedia('(max-width: 767px)').matches;
  if (!isMobileViewport.value) {
    mobileOpen.value = false;
  }
}

watch(() => route.fullPath, () => {
  mobileOpen.value = false;
});

onMounted(() => {
  syncViewportState();
  window.addEventListener('resize', syncViewportState);
});

onUnmounted(() => {
  window.removeEventListener('resize', syncViewportState);
});

// Helper to check route active status for admin subroutes
function isRouteActive(to) {
  const adminPrefix = '/admin/';
  const pathToCheck = adminPrefix + to;
  return route.path === pathToCheck || route.path.startsWith(pathToCheck + '/');
}

function isPathActive(to) {
  // Checks for direct matching or exact prefix match
  if (to.startsWith('/')) {
    return route.path === to || route.path.startsWith(to + '/');
  }
  // If it's a relative path in children routes
  return route.path.endsWith('/' + to) || route.path.includes('/' + to + '/');
}

// Lucide Icon SVG Definitions
const LayoutDashboard = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', width: '16', height: '16', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('rect', { width: '7', height: '9', x: '3', y: '3', rx: '1' }),
    h('rect', { width: '7', height: '5', x: '14', y: '3', rx: '1' }),
    h('rect', { width: '7', height: '9', x: '14', y: '12', rx: '1' }),
    h('rect', { width: '7', height: '5', x: '3', y: '16', rx: '1' })
  ])
};

const BookOpen = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', width: '16', height: '16', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z' }),
    h('path', { d: 'M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z' })
  ])
};

const Users = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', width: '16', height: '16', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2' }),
    h('circle', { cx: '9', cy: '7', r: '4' }),
    h('path', { d: 'M22 21v-2a4 4 0 0 0-3-3.87' }),
    h('path', { d: 'M16 3.13a4 4 0 0 1 0 7.75' })
  ])
};

const Calendar = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', width: '16', height: '16', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M8 2v4' }),
    h('path', { d: 'M16 2v4' }),
    h('rect', { width: '18', height: '18', x: '3', y: '4', rx: '2' }),
    h('path', { d: 'M3 10h18' })
  ])
};

const Award = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', width: '16', height: '16', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('circle', { cx: '12', cy: '8', r: '7' }),
    h('polyline', { points: '8.21 13.89 7 23 12 20 17 23 15.79 13.88' })
  ])
};

const GraduationCap = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', width: '16', height: '16', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M21.42 10.922a1 1 0 0 0-.019-1.838L12.83 5.18a2 2 0 0 0-1.66 0L2.6 9.08a1 1 0 0 0 0 1.832l8.57 3.908a2 2 0 0 0 1.66 0z' }),
    h('path', { d: 'M6 12v5c0 2 2 3 6 3s6-1 6-3v-5' })
  ])
};

const FileText = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', width: '16', height: '16', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M15 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7Z' }),
    h('path', { d: 'M14 2v4a2 2 0 0 0 2 2h4' }),
    h('path', { d: 'M10 9H8' }),
    h('path', { d: 'M16 13H8' }),
    h('path', { d: 'M16 17H8' })
  ])
};

const UserCheck = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', width: '16', height: '16', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2' }),
    h('circle', { cx: '9', cy: '7', r: '4' }),
    h('polyline', { points: '16 11 18 13 22 9' })
  ])
};

const CreditCard = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', width: '16', height: '16', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('rect', { width: '20', height: '14', x: '2', y: '5', rx: '2' }),
    h('line', { x1: '2', x2: '22', y1: '10', y2: '10' })
  ])
};

const DollarSign = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', width: '16', height: '16', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('line', { x1: '12', x2: '12', y1: '2', y2: '22' }),
    h('path', { d: 'M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6' })
  ])
};

const BarChart3 = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', width: '16', height: '16', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('line', { x1: '18', x2: '18', y1: '20', y2: '10' }),
    h('line', { x1: '12', x2: '12', y1: '20', y2: '4' }),
    h('line', { x1: '6', x2: '6', y1: '20', y2: '14' })
  ])
};

const UserIcon = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', width: '16', height: '16', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2' }),
    h('circle', { cx: '12', cy: '7', r: '4' })
  ])
};

const mapIcon = (iconName) => {
  switch (iconName) {
    case 'LayoutDashboard': return LayoutDashboard;
    case 'BookOpen': return BookOpen;
    case 'Users': return Users;
    case 'Calendar': return Calendar;
    case 'Award': return Award;
    case 'GraduationCap': return GraduationCap;
    case 'FileText': return FileText;
    case 'UserCheck': return UserCheck;
    case 'CreditCard': return CreditCard;
    case 'DollarSign': return DollarSign;
    case 'BarChart3': return BarChart3;
    case 'User': return UserIcon;
    default: return BookOpen;
  }
};

// Admin specific items grouped
const generalItems = [
  { to: 'dashboard', label: 'Tổng quan', icon: LayoutDashboard }
];

const courseScheduleItems = [
  { to: 'courses', label: 'Khóa học', icon: BookOpen },
  { to: 'classes', label: 'Lớp học', icon: Users },
  { to: 'schedules', label: 'Lịch học', icon: Calendar },
  { to: 'teachers', label: 'Giảng viên', icon: Award }
];

const studentAttendanceItems = [
  { to: 'students', label: 'Học viên', icon: GraduationCap },
  { to: 'enrollments', label: 'Ghi danh', icon: FileText },
  { to: 'attendance', label: 'Điểm danh', icon: UserCheck },
  { to: 'results', label: 'Kết quả học tập', icon: BarChart3 }
];

const paymentReportItems = [
  { to: 'accounts', label: 'Tài khoản', icon: UserCheck },
  { to: 'tuition', label: 'Học phí', icon: CreditCard },
  { to: 'payments', label: 'Thanh toán', icon: DollarSign },
  { to: 'reports', label: 'Báo cáo & Thống kê', icon: BarChart3 }
];

// Normalize menuItems for other roles
const normalizedMenuItems = computed(() => {
  return props.menuItems.map(item => {
    // Resolve icon component dynamically
    let resolvedIcon = item.icon;
    if (typeof item.icon === 'string') {
      resolvedIcon = mapIcon(item.icon);
    } else if (item.icon && item.icon.name) {
      resolvedIcon = mapIcon(item.icon.name);
    } else {
      // Direct lookup from standard list
      const iconMap = {
        LayoutDashboard, BookOpen, Calendar, UserCheck, Award, CreditCard, User: UserIcon, Users
      };
      // fallback
      resolvedIcon = iconMap[item.icon?.displayName || item.icon?.name] || BookOpen;
    }
    return {
      ...item,
      iconComponent: resolvedIcon
    };
  });
});
</script>

<style scoped>
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.25s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}

.slide-enter-active, .slide-leave-active {
  transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}
.slide-enter-from, .slide-leave-to {
  transform: translateX(-100%);
}

.scrollbar-hide::-webkit-scrollbar {
  display: none;
}
.scrollbar-hide {
  -ms-overflow-style: none;
  scrollbar-width: none;
}

@media (min-width: 768px) {
  .mobile-drawer-layer {
    display: none !important;
    pointer-events: none !important;
  }
}
</style>

