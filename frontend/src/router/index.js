import { createRouter, createWebHistory } from "vue-router";
import { useAuthStore } from "../stores/auth";
import { normalizeRole } from "../lib/constants";

// Layouts
import PublicLayout from "../layouts/PublicLayout.vue";
import StudentLayout from "../layouts/StudentLayout.vue";
import TeacherLayout from "../layouts/TeacherLayout.vue";
import AdminLayout from "../layouts/AdminLayout.vue";

// Public Pages
const HomePage = () => import("../views/public/HomeView.vue");
const LoginPage = () => import("../views/public/LoginView.vue");
const RegisterPage = () => import("../views/public/RegisterView.vue");
const CoursesPage = () => import("../views/public/CoursesView.vue");
const CourseDetailPage = () => import("../views/public/CourseDetailView.vue");
const ForbiddenPage = () => import("../views/public/ForbiddenView.vue");
const NotFoundPage = () => import("../views/public/NotFoundView.vue");

// Student Pages
const StudentDashboard = () => import("../views/student/DashboardView.vue");
const StudentCourses = () => import("../views/student/MyCoursesView.vue");
const StudentSchedule = () => import("../views/student/MyScheduleView.vue");
const StudentAttendance = () => import("../views/student/MyAttendanceView.vue");
const StudentResults = () => import("../views/student/MyResultsView.vue");
const StudentTuition = () => import("../views/student/MyTuitionView.vue");
const StudentProfile = () => import("../views/student/ProfileView.vue");

// Teacher Pages
const TeacherDashboard = () => import("../views/teacher/DashboardView.vue");
const TeacherClasses = () => import("../views/teacher/MyClassesView.vue");
const TeacherClassDetail = () => import("../views/teacher/ClassDetailView.vue");
const TeacherSchedule = () => import("../views/teacher/ScheduleView.vue");
const TeacherAttendance = () => import("../views/teacher/AttendanceView.vue");
const TeacherResultInput = () => import("../views/teacher/ResultInputView.vue");
const TeacherProfile = () => import("../views/teacher/ProfileView.vue");

// Admin Pages
const AdminDashboard = () => import("../views/admin/DashboardView.vue");
const AdminCourses = () => import("../views/admin/CoursesView.vue");
const AdminClasses = () => import("../views/admin/ClassesView.vue");
const AdminSchedules = () => import("../views/admin/SchedulesView.vue");
const AdminTeachers = () => import("../views/admin/TeachersView.vue");
const AdminStudents = () => import("../views/admin/StudentsView.vue");
const AdminEnrollments = () => import("../views/admin/EnrollmentsView.vue");
const AdminAttendance = () => import("../views/admin/AttendanceView.vue");
const AdminResults = () => import("../views/admin/ResultsView.vue");
const AdminAccounts = () => import("../views/admin/AccountsView.vue");
const AdminTuition = () => import("../views/admin/TuitionView.vue");
const AdminPayments = () => import("../views/admin/PaymentsView.vue");
const AdminReports = () => import("../views/admin/ReportsView.vue");

const routes = [
  // Public
  {
    path: "/",
    component: PublicLayout,
    children: [
      { path: "", name: "home", component: HomePage },
      { path: "login", name: "login", component: LoginPage },
      { path: "register", name: "register", component: RegisterPage },
      { path: "courses", name: "courses", component: CoursesPage },
      {
        path: "courses/:slug",
        name: "course-detail",
        component: CourseDetailPage,
        props: true,
      },
      { path: "forbidden", name: "forbidden", component: ForbiddenPage },
      { path: ":pathMatch(.*)*", name: "not-found", component: NotFoundPage },
    ],
  },
  // Student
  {
    path: "/student",
    component: StudentLayout,
    meta: { requiresAuth: true, roles: ["Student"] },
    children: [
      { path: "", redirect: { name: "student-dashboard" } },
      {
        path: "dashboard",
        name: "student-dashboard",
        component: StudentDashboard,
      },
      { path: "courses", name: "student-courses", component: StudentCourses },
      {
        path: "schedule",
        name: "student-schedule",
        component: StudentSchedule,
      },
      {
        path: "attendance",
        name: "student-attendance",
        component: StudentAttendance,
      },
      { path: "results", name: "student-results", component: StudentResults },
      { path: "tuition", name: "student-tuition", component: StudentTuition },
      { path: "profile", name: "student-profile", component: StudentProfile },
      {
        path: "/student/courses/:id",
        name: "StudentCourseDetail",
        component: () => import("../views/student/CourseDetail.vue"), // Trỏ đến file mới sẽ tạo ở Bước 3
        meta: { requiresAuth: true, role: "student" },
      },
    ],
  },
  // Teacher
  {
    path: "/teacher",
    component: TeacherLayout,
    meta: { requiresAuth: true, roles: ["Teacher"] },
    children: [
      { path: "", redirect: { name: "teacher-dashboard" } },
      {
        path: "dashboard",
        name: "teacher-dashboard",
        component: TeacherDashboard,
      },
      { path: "classes", name: "teacher-classes", component: TeacherClasses },
      {
        path: "classes/:classId",
        name: "teacher-class-detail",
        component: TeacherClassDetail,
        props: true,
      },
      {
        path: "schedule",
        name: "teacher-schedule",
        component: TeacherSchedule,
      },
      {
        path: "attendance",
        name: "teacher-attendance",
        component: TeacherAttendance,
      },
      {
        path: "results",
        name: "teacher-results",
        component: TeacherResultInput,
      },
      {
        path: "classes/:classId/attendance",
        name: "teacher-class-attendance",
        component: TeacherAttendance,
        props: true,
      },
      {
        path: "classes/:classId/results",
        name: "teacher-class-results",
        component: TeacherResultInput,
        props: true,
      },
      { path: "profile", name: "teacher-profile", component: TeacherProfile },
    ],
  },
  // Admin
  {
    path: "/admin",
    component: AdminLayout,
    meta: { requiresAuth: true, roles: ["Admin"] },
    children: [
      { path: "", redirect: { name: "admin-dashboard" } },
      { path: "dashboard", name: "admin-dashboard", component: AdminDashboard },
      { path: "courses", name: "admin-courses", component: AdminCourses },
      { path: "classes", name: "admin-classes", component: AdminClasses },
      { path: "schedules", name: "admin-schedules", component: AdminSchedules },
      { path: "teachers", name: "admin-teachers", component: AdminTeachers },
      { path: "students", name: "admin-students", component: AdminStudents },
      {
        path: "enrollments",
        name: "admin-enrollments",
        component: AdminEnrollments,
      },
      {
        path: "attendance",
        name: "admin-attendance",
        component: AdminAttendance,
      },
      { path: "results", name: "admin-results", component: AdminResults },
      { path: "accounts", name: "admin-accounts", component: AdminAccounts },
      { path: "tuition", name: "admin-tuition", component: AdminTuition },
      { path: "payments", name: "admin-payments", component: AdminPayments },
      { path: "reports", name: "admin-reports", component: AdminReports },
    ],
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition;
    }
    return { top: 0 };
  },
});

// Guard
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();

  // Restore session if not yet loaded and token exists
  if (!authStore.user) {
    authStore.restoreSession();
  }

  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth);
  const allowedRoles = to.matched.find((record) => record.meta.roles)?.meta
    .roles;

  if (requiresAuth && !authStore.isAuthenticated) {
    return next({ name: "login", query: { redirect: to.fullPath } });
  }

  if (requiresAuth && allowedRoles) {
    const userRole = normalizeRole(authStore.role);
    const normalizedAllowedRoles = allowedRoles.map(normalizeRole);

    if (!normalizedAllowedRoles.includes(userRole)) {
      return next({ name: "forbidden" });
    }
  }

  next();
});

export default router;
