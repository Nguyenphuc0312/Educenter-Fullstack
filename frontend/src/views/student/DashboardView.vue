<template>
  <div class="space-y-6">
    <LoadingSpinner v-if="isLoading" size="lg" class="py-20" />
    <ErrorState
      v-else-if="error"
      title="Lỗi tải dữ liệu"
      :description="error"
      :onRetry="fetchDashboardData"
    />

    <template v-else>
      <PageHeader
        :title="`Xin chào, ${student?.fullName || user?.fullName || 'học viên'} 👋`"
        :subtitle="`Mã học viên: ${student?.studentCode || 'N/A'} · Chào mừng bạn quay trở lại hệ thống học tập.`"
      >

        <template #actions>
          <router-link
            to="/courses"
            class="px-4 py-2 border border-blue-200 text-blue-600 hover:bg-blue-50 font-medium rounded-lg transition-colors"
          >
            Tìm khóa học
          </router-link>
          <router-link
            to="/student/schedule"
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg transition-colors shadow-sm"
          >
            Xem lịch tuần
          </router-link>
        </template>
      </PageHeader>


      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
        <div
          class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm flex flex-col justify-between relative overflow-hidden"
        >
          <div
            class="absolute -right-4 -top-4 w-16 h-16 bg-blue-50 rounded-full blur-2xl"
          ></div>
          <div>
            <p class="text-sm font-medium text-slate-500 mb-1">Khóa đang học</p>
            <strong class="text-3xl font-black text-slate-800">{{
              activeCourses.length
            }}</strong>
          </div>
          <p
            class="text-xs text-blue-600 mt-3 font-medium bg-blue-50 w-max px-2 py-1 rounded"
          >
            Đã xác nhận
          </p>
        </div>

        <div
          class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm flex items-center justify-between"
        >
          <div>
            <p class="text-sm font-medium text-slate-500 mb-1">Chuyên cần</p>
            <strong class="text-2xl font-black text-slate-800">{{
              formatPercent(attendancePercent, 0)
            }}</strong>
            <p class="text-xs text-emerald-600 mt-2 font-medium">
              Theo bản ghi
            </p>
          </div>
          <a-progress
            type="circle"
            :percent="Math.round(attendancePercent)"
            :width="60"
            stroke-color="#10b981"
          />
        </div>

        <div
          class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm flex flex-col justify-between relative overflow-hidden"
        >
          <div
            class="absolute -right-4 -top-4 w-16 h-16 bg-purple-50 rounded-full blur-2xl"
          ></div>
          <div>
            <p class="text-sm font-medium text-slate-500 mb-1">
              Điểm trung bình
            </p>
            <strong class="text-3xl font-black text-slate-800">{{
              averageScoreText
            }}</strong>
          </div>
          <p
            class="text-xs text-purple-600 mt-3 font-medium bg-purple-50 w-max px-2 py-1 rounded"
          >
            Từ kết quả đã nhập
          </p>
        </div>

        <div
          class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm flex flex-col justify-between"
        >
          <div>
            <p class="text-sm font-medium text-slate-500 mb-1">
              Công nợ học phí
            </p>
            <strong
              :class="[
                'text-2xl font-black',
                tuitionSummary.debt > 0 ? 'text-red-500' : 'text-slate-800',
              ]"
            >
              {{ formatVnd(tuitionSummary.debt) }}
            </strong>
          </div>
          <p
            :class="[
              'text-xs font-medium w-max px-2 py-1 rounded mt-3',
              tuitionSummary.debt > 0
                ? 'text-red-600 bg-red-50'
                : 'text-slate-600 bg-slate-50',
            ]"
          >
            {{ tuitionSummary.debt > 0 ? "Cần thanh toán" : "Đã hoàn tất" }}
          </p>
        </div>
      </div>

      <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">
        <div class="xl:col-span-2 space-y-6">
          <section
            class="bg-white rounded-2xl border border-slate-100 shadow-sm p-6"
          >
            <div class="flex justify-between items-center mb-5">
              <div>
                <h2 class="text-lg font-bold text-slate-800">
                  Khóa học của tôi
                </h2>
                <p class="text-sm text-slate-500">Tiến độ học tập hiện tại.</p>
              </div>
              <router-link
                to="/student/courses"
                class="text-sm font-medium text-blue-600 hover:text-blue-700"
                >Xem tất cả &rarr;</router-link
              >
            </div>

            <a-empty
              v-if="courses.length === 0"
              description="Bạn chưa có khóa học nào."
              class="py-8"
            />

            <div v-else class="space-y-4">
              <article
                v-for="course in courses.slice(0, 4)"
                :key="course.id"
                class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 p-4 rounded-xl border border-slate-100 hover:border-blue-200 transition-colors bg-slate-50/50"
              >
                <div class="min-w-0 flex-1">
                  <div class="flex items-center gap-2 mb-1">
                    <h3 class="font-bold text-slate-800 truncate">
                      {{ course.courseNameSnapshot || "Khóa học" }}
                    </h3>
                    <span :class="statusClass(course.status)">{{
                      statusText(course.status)
                    }}</span>
                  </div>
                  <p class="text-sm text-slate-500">
                    {{ course.classNameSnapshot || "Chưa có lớp" }} · Ghi danh
                    {{ formatDate(course.enrolledAt) }}
                  </p>
                </div>
                <div class="w-full sm:w-48 shrink-0">
                  <div
                    class="flex justify-between text-xs font-semibold text-slate-500 mb-1"
                  >
                    <span>Tiến độ</span>
                    <span>{{ progressLabel(course) }}</span>
                  </div>
                  <a-progress
                    v-if="canShowProgress(course)"
                    :percent="progressForCourse(course)"
                    :show-info="false"
                    stroke-color="#3b82f6"
                  />
                  <div
                    v-else
                    class="h-2 rounded-full bg-slate-100 border border-slate-200"
                  ></div>
                  <p class="mt-1 text-xs text-slate-500">
                    {{ progressHint(course) }}
                  </p>
                </div>
              </article>
            </div>
          </section>

          <section
            class="bg-white rounded-2xl border border-slate-100 shadow-sm p-6"
          >
            <div class="flex justify-between items-center mb-5">
              <div>
                <h2 class="text-lg font-bold text-slate-800">
                  Kết quả học tập
                </h2>
                <p class="text-sm text-slate-500">Điểm đánh giá mới nhất.</p>
              </div>
              <router-link
                to="/student/results"
                class="text-sm font-medium text-blue-600 hover:text-blue-700"
                >Chi tiết &rarr;</router-link
              >
            </div>

            <a-empty
              v-if="results.length === 0"
              description="Chưa có kết quả học tập."
              class="py-6"
            />

            <div v-else class="space-y-3">
              <article
                v-for="result in results.slice(0, 3)"
                :key="result.id"
                class="flex justify-between items-center p-4 rounded-xl border border-slate-100 bg-slate-50/50"
              >
                <div class="min-w-0 pr-4">
                  <h3 class="font-bold text-slate-800 text-sm truncate">
                    {{ result.courseNameSnapshot }}
                  </h3>
                  <p class="text-xs text-slate-500 mt-1">
                    {{ result.classNameSnapshot }}
                  </p>
                </div>
                <div class="text-right shrink-0 flex flex-col items-end">
                  <strong class="text-lg text-slate-800">{{
                    formatScore(result.averageScore)
                  }}</strong>
                  <span :class="['mt-1', statusClass(result.resultStatus)]">{{
                    statusText(result.resultStatus)
                  }}</span>
                </div>
              </article>
            </div>
          </section>
        </div>

        <div class="space-y-6">
          <section
            class="bg-white rounded-2xl border border-slate-100 shadow-sm p-6"
          >
            <h2 class="text-lg font-bold text-slate-800 mb-1">
              Lịch học sắp tới
            </h2>
            <p class="text-sm text-slate-500 mb-5">
              Chuẩn bị cho các buổi học tiếp theo.
            </p>

            <a-empty
              v-if="upcomingSchedules.length === 0"
              description="Chưa có lịch học sắp tới."
              class="py-6"
            />

            <div
              v-else
              class="relative border-l-2 border-slate-100 ml-3 space-y-6"
            >
              <article
                v-for="(schedule, index) in upcomingSchedules.slice(0, 4)"
                :key="schedule.id"
                class="relative pl-6"
              >
                <div
                  class="absolute -left-[9px] top-1 w-4 h-4 rounded-full bg-white border-2 border-blue-500"
                ></div>

                <div class="bg-slate-50 rounded-xl p-3 border border-slate-100">
                  <div class="flex justify-between items-start mb-2">
                    <span
                      class="text-xs font-bold text-blue-600 bg-blue-100 px-2 py-0.5 rounded"
                      >{{ shiftText(schedule.studyShift) }}</span
                    >
                    <span class="text-xs font-medium text-slate-500">{{
                      dayText(schedule.dayOfWeek)
                    }}</span>
                  </div>
                  <h3
                    class="font-bold text-slate-800 text-sm mb-1 leading-tight"
                  >
                    {{
                      schedule.classNameSnapshot ||
                      schedule.className ||
                      "Lớp học"
                    }}
                  </h3>
                  <p class="text-xs text-slate-500 flex items-center gap-1">
                    🕒 {{ formatTime(schedule.startTime) }} -
                    {{ formatTime(schedule.endTime) }}
                  </p>
                  <p
                    class="text-xs text-slate-500 flex items-center gap-1 mt-1"
                  >
                    📍 {{ schedule.room || "Phòng chưa cập nhật" }}
                  </p>
                </div>
              </article>
            </div>
          </section>

          <section
            class="bg-gradient-to-br from-slate-800 to-slate-900 rounded-2xl shadow-sm p-6 text-white relative overflow-hidden"
          >
            <div
              class="absolute -right-6 -bottom-6 w-32 h-32 bg-white opacity-5 rounded-full blur-2xl"
            ></div>
            <div class="flex justify-between items-center mb-6 relative z-10">
              <h2 class="text-lg font-bold">Học phí</h2>
              <router-link
                to="/student/tuition"
                class="text-sm font-medium text-slate-300 hover:text-white"
                >Chi tiết &rarr;</router-link
              >
            </div>

            <div class="space-y-4 relative z-10">
              <div
                class="flex justify-between items-end border-b border-slate-700 pb-3"
              >
                <span class="text-sm text-slate-400">Đã thanh toán</span>
                <strong class="text-lg text-emerald-400">{{
                  formatVnd(tuitionSummary.paid)
                }}</strong>
              </div>
              <div class="flex justify-between items-end">
                <span class="text-sm text-slate-400">Còn nợ</span>
                <strong class="text-xl text-red-400">{{
                  formatVnd(tuitionSummary.debt)
                }}</strong>
              </div>
            </div>

            <router-link
              v-if="tuitionSummary.debt > 0"
              to="/student/tuition"
              class="mt-5 block w-full py-2 bg-red-500 hover:bg-red-600 text-center text-white text-sm font-bold rounded-lg transition-colors relative z-10"
            >
              Thanh toán ngay
            </router-link>
          </section>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import { useAuthStore } from "@/stores/auth";
import { studentApi } from "@/api/studentApi";
import { tuitionApi } from "@/api/tuitionApi";
import { scheduleApi } from "@/api/scheduleApi";
import PageHeader from "@/components/ui/PageHeader.vue";
import LoadingSpinner from "@/components/ui/LoadingSpinner.vue";
import ErrorState from "@/components/ui/ErrorState.vue";
import {
  formatDate,
  formatPercent,
  formatScore,
  formatTime,
  formatVnd,
} from "@/lib/formatters";
import { useStatusLabel } from "@/composables/useStatusLabel";

const { statusText, statusClass, shiftText, dayText } = useStatusLabel();

const authStore = useAuthStore();
const user = computed(() => authStore.user);

const profile = ref(null);
const invoices = ref([]);
const schedules = ref([]);
const isLoading = ref(true);
const error = ref(null);

const student = computed(() => profile.value?.student || null);
const courses = computed(() => profile.value?.courses || []);
const results = computed(() => profile.value?.results || []);
const activeCourses = computed(() =>
  courses.value.filter((x) =>
    ["Confirmed", "Studying"].includes(String(x.status)),
  ),
);
const attendancePercent = computed(
  () =>
    profile.value?.attendanceSummary?.attendancePercent ??
    averageAttendanceFromResults.value,
);
const averageAttendanceFromResults = computed(() => {
  if (!results.value.length) return 0;
  return (
    results.value.reduce(
      (sum, item) => sum + Number(item.attendancePercent || 0),
      0,
    ) / results.value.length
  );
});
const averageScoreText = computed(() => {
  if (!results.value.length) return "-";
  const avg =
    results.value.reduce(
      (sum, item) => sum + Number(item.averageScore || 0),
      0,
    ) / results.value.length;
  return formatScore(avg);
});
const tuitionSummary = computed(() =>
  invoices.value.reduce(
    (sum, invoice) => ({
      paid: sum.paid + Number(invoice.paidAmount || 0),
      debt: sum.debt + Number(invoice.debtAmount || 0),
    }),
    { paid: 0, debt: 0 },
  ),
);

// BẢNG MAP THỨ TỰ NGÀY TRONG TUẦN
const dayOrder = {
  Monday: 1,
  Tuesday: 2,
  Wednesday: 3,
  Thursday: 4,
  Friday: 5,
  Saturday: 6,
  Sunday: 7,
  "Thứ 2": 1,
  "Thứ 3": 2,
  "Thứ 4": 3,
  "Thứ 5": 4,
  "Thứ 6": 5,
  "Thứ 7": 6,
  "Chủ nhật": 7,
};

// ĐÃ SỬA LẠI LOGIC: Sắp xếp theo ngày trong tuần -> giờ học
const upcomingSchedules = computed(() => {
  return schedules.value.slice().sort((a, b) => {
    // 1. So sánh ngày
    const dayA = dayOrder[a.dayOfWeek] || 99;
    const dayB = dayOrder[b.dayOfWeek] || 99;
    if (dayA !== dayB) return dayA - dayB;

    // 2. Nếu trùng ngày, so sánh giờ học
    return String(a.startTime || "").localeCompare(String(b.startTime || ""));
  });
});

onMounted(fetchDashboardData);

async function fetchDashboardData() {
  if (!user.value?.referenceId) {
    isLoading.value = false;
    return;
  }
  isLoading.value = true;
  error.value = null;
  try {
    const studentId = user.value.referenceId;
    const [profileData, invoiceData] = await Promise.all([
      studentApi.getLearningProfile(studentId),
      tuitionApi.getByStudent(studentId).catch(() => []),
    ]);
    profile.value = profileData;
    invoices.value = Array.isArray(invoiceData) ? invoiceData : [];

    const classIds = [
      ...new Set(
        (profileData?.courses || []).map((x) => x.classId).filter(Boolean),
      ),
    ];
    const scheduleGroups = await Promise.all(
      classIds
        .slice(0, 4)
        .map((id) => scheduleApi.getByClass(id).catch(() => [])),
    );
    schedules.value = scheduleGroups.flat();

  } catch (err) {
    console.error(err);
    error.value =
      "Không thể tải dữ liệu tổng quan học viên. Vui lòng kiểm tra API Gateway và đăng nhập lại.";
  } finally {
    isLoading.value = false;
  }
}

function progressForCourse(course) {
  if (course.status === "Completed") return 100;
  const value = Number(course.progressPercent ?? 0);
  return Math.max(0, Math.min(100, Math.round(value)));
}

function canShowProgress(course) {
  return Boolean(course.canShowProgress);
}

function progressLabel(course) {
  if (isInvalidEnrollmentPeriod(course)) return "Không áp dụng";
  if (!canShowProgress(course)) return "Chua tinh";
  return `${progressForCourse(course)}%`;
}

function progressHint(course) {
  if (isInvalidEnrollmentPeriod(course)) return "Ngày ghi danh sau khi lớp đã kết thúc. Vui lòng liên hệ Admin để chuyển lớp.";
  if (course.status === "Pending") return "Cho Admin duyet.";
  if (course.status === "Confirmed") return "Lop chua bat dau.";
  if (course.status === "Cancelled") return "Dang ky da huy.";
  if (course.status === "Completed") return "Da hoan thanh.";
  return `${course.completedSessions ?? 0}/${course.totalSessions ?? 0} buoi.`;
}

function isInvalidEnrollmentPeriod(course) {
  if (!course?.classEndDate || !course?.enrolledAt) return false;
  return new Date(course.enrolledAt).getTime() > new Date(course.classEndDate).getTime();
}


</script>
