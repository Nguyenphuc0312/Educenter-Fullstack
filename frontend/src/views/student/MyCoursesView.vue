<template>
  <div class="space-y-6">
    <PageHeader
      title="Khóa học của tôi"
      subtitle="Quản lý tiến trình học tập, tra cứu tài liệu và lịch học các lớp đã đăng ký."
    >
      <template #actions>
        <router-link
          to="/courses"
          class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg transition-colors shadow-sm flex items-center gap-2"
        >
          <svg
            class="w-4 h-4"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
            />
          </svg>
          Khám phá khóa học
        </router-link>
      </template>
    </PageHeader>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
      <div
        class="bg-white rounded-xl p-5 border border-slate-200 shadow-sm flex items-center justify-between"
      >
        <div>
          <span
            class="text-slate-500 text-sm font-semibold uppercase tracking-wider mb-1 block"
            >Tổng ghi danh</span
          >
          <strong class="text-3xl text-slate-800 font-black">{{
            courses.length
          }}</strong>
        </div>
        <div class="p-3 bg-slate-50 rounded-full text-slate-400">
          <svg
            class="w-8 h-8"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"
            />
          </svg>
        </div>
      </div>
      <div
        class="bg-white rounded-xl p-5 border border-slate-200 shadow-sm flex items-center justify-between"
      >
        <div>
          <span
            class="text-blue-600 text-sm font-semibold uppercase tracking-wider mb-1 block"
            >Đang học tập</span
          >
          <strong class="text-3xl text-blue-700 font-black">{{
            studyingCount
          }}</strong>
        </div>
        <div class="p-3 bg-blue-50 rounded-full text-blue-500">
          <svg
            class="w-8 h-8"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"
            />
          </svg>
        </div>
      </div>
      <div
        class="bg-white rounded-xl p-5 border border-slate-200 shadow-sm flex items-center justify-between"
      >
        <div>
          <span
            class="text-emerald-600 text-sm font-semibold uppercase tracking-wider mb-1 block"
            >Đã hoàn thành</span
          >
          <strong class="text-3xl text-emerald-700 font-black">{{
            completedCount
          }}</strong>
        </div>
        <div class="p-3 bg-emerald-50 rounded-full text-emerald-500">
          <svg
            class="w-8 h-8"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>
        </div>
      </div>
    </div>

    <section
      class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden"
    >
      <div class="border-b border-slate-200 px-6 pt-4">
        <a-tabs v-model:activeKey="activeTab">
          <a-tab-pane key="all" tab="Tất cả khóa học" />
          <a-tab-pane key="active" tab="Đang tiến hành" />
          <a-tab-pane key="completed" tab="Đã hoàn thành" />
        </a-tabs>
      </div>

      <div class="p-6 bg-slate-50/50 min-h-[400px]">
        <LoadingSpinner v-if="loading" size="lg" class="py-20" />
        <div
          v-else-if="error"
          class="text-center text-red-500 py-8 font-medium"
        >
          {{ error }}
        </div>

        <div
          v-else-if="filteredCourses.length === 0"
          class="flex flex-col items-center justify-center py-16 text-center"
        >
          <a-empty description="Không tìm thấy khóa học nào trong mục này." />
          <router-link
            v-if="activeTab === 'all'"
            to="/courses"
            class="mt-4 px-4 py-2 bg-white border border-slate-300 text-slate-700 font-medium rounded-lg hover:bg-slate-50 transition-colors"
          >
            Đăng ký khóa học mới
          </router-link>
        </div>

        <div
          v-else
          class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6"
        >
          <article
            v-for="course in filteredCourses"
            :key="course.id"
            class="bg-white rounded-xl border border-slate-200 shadow-sm hover:shadow-md hover:border-blue-300 transition-all duration-200 flex flex-col overflow-hidden"
          >
            <div class="p-5 border-b border-slate-100 relative">
              <div class="flex justify-between items-start gap-4 mb-2">
                <span
                  class="text-xs font-bold uppercase tracking-wider text-slate-400"
                  >Khóa học</span
                >
                <span :class="statusBadge(course.status)">{{
                  statusText(course.status)
                }}</span>
              </div>
              <h2
                class="text-lg font-bold text-slate-800 line-clamp-2 leading-tight mb-1"
                :title="course.courseNameSnapshot"
              >
                {{ course.courseNameSnapshot }}
              </h2>
              <p class="text-sm text-slate-500 font-medium">
                {{ course.classNameSnapshot }}
              </p>
            </div>

            <div class="p-5 flex-1 flex flex-col">
              <div class="flex justify-between items-center mb-4 text-sm">
                <div>
                  <span class="block text-slate-400 text-xs mb-0.5"
                    >Ngày ghi danh</span
                  >
                  <strong class="text-slate-700">{{
                    formatDate(course.enrolledAt)
                  }}</strong>
                </div>
                <div class="text-right">
                  <span class="block text-slate-400 text-xs mb-0.5"
                    >Lộ trình</span
                  >
                  <strong class="text-slate-700">Tiêu chuẩn</strong>
                </div>
              </div>

              <div class="mt-auto pt-2">
                <div class="flex justify-between text-xs font-semibold mb-1">
                  <span class="text-slate-500">Tiến độ học tập</span>
                  <span :class="getProgressColorText(course)"
                    >{{ progressLabel(course) }}</span
                  >
                </div>
                <a-progress
                  v-if="canShowProgress(course)"
                  :percent="progress(course)"
                  :show-info="false"
                  :stroke-color="getStrokeColor(course)"
                  :status="course.status === 'Completed' ? 'success' : 'normal'"
                  stroke-linecap="round"
                  :stroke-width="8"
                />
                <div
                  v-else
                  class="h-2 rounded-full bg-slate-100 border border-slate-200"
                ></div>
                <p class="mt-2 text-xs text-slate-500">
                  {{ progressHint(course) }}
                </p>
              </div>
            </div>

            <div
              class="px-5 py-4 bg-slate-50 border-t border-slate-100 flex gap-3"
            >
              <router-link
                :to="`/student/courses/${course.classId || course.id}`"
                class="flex-1 text-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg text-sm transition-colors shadow-sm"
              >
                Vào không gian lớp
              </router-link>
              <router-link
                to="/student/results"
                class="px-4 py-2 bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 font-medium rounded-lg text-sm transition-colors flex items-center justify-center"
                title="Xem điểm số"
              >
                <svg
                  class="w-4 h-4"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
                  />
                </svg>
              </router-link>
            </div>
          </article>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import PageHeader from "@/components/ui/PageHeader.vue";
import LoadingSpinner from "@/components/ui/LoadingSpinner.vue";
import { studentApi } from "@/api/studentApi";
import { useAuthStore } from "@/stores/auth";
import { formatDate } from "@/lib/formatters";

const auth = useAuthStore();
const courses = ref([]);
const loading = ref(true);
const error = ref("");
const activeTab = ref("all"); // Quản lý trạng thái lọc

const studyingCount = computed(
  () =>
    courses.value.filter((x) =>
      ["Confirmed", "Studying"].includes(String(x.status)),
    ).length,
);
const completedCount = computed(
  () => courses.value.filter((x) => x.status === "Completed").length,
);

// Computed property để lọc danh sách khóa học dựa trên Tab đang chọn
const filteredCourses = computed(() => {
  if (activeTab.value === "active") {
    return courses.value.filter((c) =>
      ["Confirmed", "Studying", "Pending"].includes(String(c.status)),
    );
  }
  if (activeTab.value === "completed") {
    return courses.value.filter((c) => c.status === "Completed");
  }
  return courses.value; // 'all'
});

onMounted(loadData);

async function loadData() {
  loading.value = true;
  error.value = "";
  try {
    courses.value = auth.user?.referenceId
      ? await studentApi.getMyCourses(auth.user.referenceId)
      : [];
  } catch (err) {
    error.value = err.message || "Hệ thống đang bảo trì. Vui lòng thử lại sau.";
  } finally {
    loading.value = false;
  }
}

// Helpers giao diện chuẩn Tailwind
function statusText(status) {
  return (
    {
      Pending: "Chờ duyệt",
      Confirmed: "Đã xác nhận",
      Studying: "Đang học",
      Completed: "Hoàn thành",
      Cancelled: "Đã hủy",
    }[status] || status
  );
}

function statusBadge(status) {
  const base =
    "inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold border ";
  if (["Confirmed", "Studying"].includes(String(status)))
    return base + "bg-blue-50 text-blue-700 border-blue-200";
  if (status === "Completed")
    return base + "bg-emerald-50 text-emerald-700 border-emerald-200";
  if (status === "Pending")
    return base + "bg-amber-50 text-amber-700 border-amber-200";
  return base + "bg-slate-50 text-slate-600 border-slate-200"; // Cancelled
}

// Giả lập tiến độ
function canShowProgress(course) {
  return Boolean(course.canShowProgress);
}

function progress(course) {
  if (course.status === "Completed") return 100;
  const value = Number(course.progressPercent ?? 0);
  return Math.max(0, Math.min(100, Math.round(value)));
}

function progressLabel(course) {
  if (!canShowProgress(course)) return "Chua tinh";
  return `${progress(course)}%`;
}

function progressHint(course) {
  if (course.status === "Pending") return "Cho Admin duyet, chua co tien do hoc.";
  if (course.status === "Confirmed") return "Da xac nhan, lop chua bat dau hoc.";
  if (course.status === "Cancelled") return "Dang ky da huy, khong tinh tien do.";
  if (course.status === "Completed") return "Khoa hoc da hoan thanh.";
  return `${course.completedSessions ?? 0}/${course.totalSessions ?? 0} buoi da ghi nhan.`;
}

// Tùy chỉnh màu sắc Ant Design Progress Bar
function getStrokeColor(course) {
  if (course.status === "Completed") return "#10b981"; // emerald-500
  return "#2563eb"; // blue-600
}

function getProgressColorText(course) {
  if (course.status === "Completed") return "text-emerald-600";
  if (!canShowProgress(course)) return "text-slate-500";
  return "text-blue-600";
}
</script>

<style scoped>
:deep(.ant-tabs-nav) {
  margin-bottom: 0 !important;
}
:deep(.ant-tabs-tab) {
  padding: 12px 0;
  font-weight: 500;
  color: #64748b; /* text-slate-500 */
}
:deep(.ant-tabs-tab-active) {
  font-weight: 600;
}
</style>
