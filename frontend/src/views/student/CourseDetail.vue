<template>
  <div class="space-y-6">
    <div class="flex items-center gap-4 mb-2">
      <router-link
        to="/student/courses"
        class="p-2 bg-white border border-slate-200 rounded-lg hover:bg-slate-50 text-slate-500 transition-colors shadow-sm"
      >
        <svg
          class="w-5 h-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M10 19l-7-7m0 0l7-7m-7 7h18"
          />
        </svg>
      </router-link>
      <div>
        <h1 class="text-2xl font-black text-slate-800">
          {{ courseInfo.className || "Chi tiết lớp học" }}
        </h1>
        <p class="text-sm text-slate-500 font-medium mt-1">
          {{ courseInfo.courseName || "Đang tải dữ liệu..." }}
        </p>
      </div>
    </div>

    <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
      <div
        class="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex items-center gap-3 transition-hover hover:border-blue-300"
      >
        <div class="p-2 bg-blue-50 text-blue-600 rounded-lg">
          <svg
            class="w-5 h-5"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
            />
          </svg>
        </div>
        <div>
          <p class="text-xs text-slate-500 font-semibold uppercase">
            Khai giảng
          </p>
          <strong class="text-slate-800">{{
            courseInfo.startDate || "N/A"
          }}</strong>
        </div>
      </div>

      <div
        class="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex items-center gap-3 transition-hover hover:border-emerald-300"
      >
        <div class="p-2 bg-emerald-50 text-emerald-600 rounded-lg">
          <svg
            class="w-5 h-5"
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
        <div>
          <p class="text-xs text-slate-500 font-semibold uppercase">
            Chuyên cần
          </p>
          <strong class="text-slate-800"
            >{{ courseInfo.attendancePercent || "0" }}%</strong
          >
        </div>
      </div>

      <div
        class="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex items-center gap-3 transition-hover hover:border-amber-300"
      >
        <div class="p-2 bg-amber-50 text-amber-600 rounded-lg">
          <svg
            class="w-5 h-5"
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
        <div>
          <p class="text-xs text-slate-500 font-semibold uppercase">Bài tập</p>
          <strong class="text-slate-800">3/5 Đã nộp</strong>
        </div>
      </div>

      <div
        class="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex items-center gap-3 transition-hover hover:border-purple-300"
      >
        <div class="p-2 bg-purple-50 text-purple-600 rounded-lg">
          <svg
            class="w-5 h-5"
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
        </div>
        <div>
          <p class="text-xs text-slate-500 font-semibold uppercase">
            Trung bình
          </p>
          <strong class="text-slate-800">{{
            courseInfo.averageScore || "Chưa có"
          }}</strong>
        </div>
      </div>
    </div>

    <div
      class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden"
    >
      <div class="px-6 pt-4 border-b border-slate-200">
        <a-tabs v-model:activeKey="activeTab">
          <a-tab-pane key="sessions" tab="Lộ trình & Buổi học" />
          <a-tab-pane key="documents" tab="Tài liệu" />
          <a-tab-pane key="assignments" tab="Bài tập" />
          <a-tab-pane key="attendance" tab="Điểm danh" />
          <a-tab-pane key="grades" tab="Bảng điểm" />
        </a-tabs>
      </div>

      <div class="p-6 bg-slate-50 min-h-[500px]">
        <LoadingSpinner v-if="loading" size="lg" class="py-20" />

        <div v-else>
          <div
            v-if="activeTab === 'sessions'"
            class="space-y-4 max-w-4xl mx-auto"
          >
            <a-empty
              v-if="mockSessions.length === 0"
              description="Chưa có lộ trình học tập."
              class="py-10"
            />
            <a-timeline v-else>
              <a-timeline-item
                v-for="(session, idx) in mockSessions"
                :key="idx"
                :color="
                  session.isPast ? 'green' : session.isCurrent ? 'blue' : 'gray'
                "
              >
                <div
                  class="bg-white p-5 rounded-xl border border-slate-200 shadow-sm ml-2 mb-4 hover:shadow-md transition-shadow"
                >
                  <div class="flex justify-between items-start mb-2">
                    <h3 class="font-bold text-slate-800 text-base">
                      Buổi {{ session.number }}: {{ session.title }}
                    </h3>
                    <span
                      class="text-xs font-semibold px-2.5 py-1 rounded-md bg-slate-100 text-slate-600 border border-slate-200"
                    >
                      {{ session.date }}
                    </span>
                  </div>
                  <p class="text-sm text-slate-600 mb-4">
                    {{ session.description }}
                  </p>
                  <div class="flex gap-2 text-xs font-medium">
                    <span
                      v-if="session.hasHomework"
                      class="px-2.5 py-1 bg-amber-50 text-amber-700 rounded-md border border-amber-200 flex items-center gap-1"
                    >
                      <svg
                        class="w-3.5 h-3.5"
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
                      Có bài tập
                    </span>
                    <span
                      v-if="session.hasDocument"
                      class="px-2.5 py-1 bg-blue-50 text-blue-700 rounded-md border border-blue-200 flex items-center gap-1"
                    >
                      <svg
                        class="w-3.5 h-3.5"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M7 21h10a2 2 0 002-2V9.414a1 1 0 00-.293-.707l-5.414-5.414A1 1 0 0012.586 3H7a2 2 0 00-2 2v14a2 2 0 002 2z"
                        />
                      </svg>
                      Có tài liệu
                    </span>
                  </div>
                </div>
              </a-timeline-item>
            </a-timeline>
          </div>

          <div v-else-if="activeTab === 'documents'">
            <a-empty
              v-if="mockDocuments.length === 0"
              description="Chưa có tài liệu nào được tải lên."
              class="py-10"
            />
            <div
              v-else
              class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4"
            >
              <div
                v-for="(doc, idx) in mockDocuments"
                :key="idx"
                class="bg-white p-4 rounded-xl border border-slate-200 hover:border-blue-400 hover:shadow-md transition-all flex items-start justify-between gap-4 group cursor-pointer"
              >
                <div class="flex items-start gap-3">
                  <div class="p-3 bg-red-50 text-red-500 rounded-lg shrink-0">
                    <svg
                      class="w-6 h-6"
                      fill="currentColor"
                      viewBox="0 0 20 20"
                    >
                      <path
                        fill-rule="evenodd"
                        d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4zm2 6a1 1 0 011-1h6a1 1 0 110 2H7a1 1 0 01-1-1zm1 3a1 1 0 100 2h6a1 1 0 100-2H7z"
                        clip-rule="evenodd"
                      />
                    </svg>
                  </div>
                  <div>
                    <h4
                      class="font-bold text-slate-800 text-sm line-clamp-2 mb-1 group-hover:text-blue-600 transition-colors"
                    >
                      {{ doc.name }}
                    </h4>
                    <p class="text-xs text-slate-500">
                      {{ doc.size }} • Cập nhật: {{ doc.date }}
                    </p>
                  </div>
                </div>
                <button
                  class="text-slate-400 hover:text-blue-600 transition-colors p-1"
                >
                  <svg
                    class="w-5 h-5"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"
                    />
                  </svg>
                </button>
              </div>
            </div>
          </div>

          <div
            v-else-if="activeTab === 'assignments'"
            class="space-y-4 max-w-4xl mx-auto"
          >
            <a-empty
              v-if="mockAssignments.length === 0"
              description="Giảng viên chưa giao bài tập nào."
              class="py-10"
            />
            <div
              v-else
              v-for="(task, idx) in mockAssignments"
              :key="idx"
              class="bg-white p-5 rounded-xl border border-slate-200 shadow-sm flex flex-col sm:flex-row justify-between gap-4 hover:border-blue-300 transition-colors"
            >
              <div>
                <div class="flex items-center gap-3 mb-2">
                  <h4 class="font-bold text-slate-800 text-base">
                    {{ task.title }}
                  </h4>
                  <span
                    :class="[
                      'text-[10px] font-bold px-2.5 py-0.5 rounded-full uppercase border',
                      task.status === 'Submitted'
                        ? 'bg-emerald-50 text-emerald-700 border-emerald-200'
                        : 'bg-red-50 text-red-700 border-red-200',
                    ]"
                  >
                    {{ task.status === "Submitted" ? "Đã nộp" : "Chưa nộp" }}
                  </span>
                </div>
                <p class="text-sm text-slate-500 flex items-center gap-1.5">
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
                      d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                    />
                  </svg>
                  Hạn chót:
                  <strong class="text-slate-700">{{ task.deadline }}</strong>
                </p>
              </div>
              <div class="flex items-center">
                <button
                  class="px-5 py-2.5 bg-blue-50 text-blue-600 hover:bg-blue-600 hover:text-white font-semibold rounded-lg text-sm transition-colors border border-blue-200 hover:border-blue-600 whitespace-nowrap"
                >
                  Xem chi tiết
                </button>
              </div>
            </div>
          </div>

          <div v-else-if="activeTab === 'attendance'">
            <div
              class="bg-white rounded-xl border border-slate-200 overflow-hidden max-w-4xl mx-auto"
            >
              <a-table
                :dataSource="mockAttendance"
                :columns="attendanceColumns"
                :pagination="false"
                class="enterprise-table"
              >
                <template #bodyCell="{ column, record }">
                  <template v-if="column.key === 'status'">
                    <span
                      :class="[
                        'px-2.5 py-1 text-xs font-bold rounded-md border',
                        record.status === 'Present'
                          ? 'bg-emerald-50 text-emerald-600 border-emerald-200'
                          : 'bg-red-50 text-red-600 border-red-200',
                      ]"
                    >
                      {{ record.status === "Present" ? "Có mặt" : "Vắng mặt" }}
                    </span>
                  </template>
                </template>
                <template #emptyText>
                  <a-empty description="Chưa có dữ liệu điểm danh." />
                </template>
              </a-table>
            </div>
          </div>

          <div v-else-if="activeTab === 'grades'">
            <div
              class="bg-white rounded-xl border border-slate-200 overflow-hidden max-w-4xl mx-auto"
            >
              <a-table
                :dataSource="mockGrades"
                :columns="gradeColumns"
                :pagination="false"
                class="enterprise-table"
              >
                <template #bodyCell="{ column, record }">
                  <template v-if="column.key === 'score'">
                    <strong
                      :class="
                        record.score >= 5
                          ? 'text-emerald-600 text-lg'
                          : 'text-red-600 text-lg'
                      "
                    >
                      {{ record.score }}
                    </strong>
                  </template>
                </template>
                <template #emptyText>
                  <a-empty description="Chưa có điểm số nào được ghi nhận." />
                </template>
              </a-table>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRoute } from "vue-router";
// import { studentApi } from '@/api/studentApi'
import LoadingSpinner from "@/components/ui/LoadingSpinner.vue";

const route = useRoute();
const activeTab = ref("sessions");
const loading = ref(true);

const courseInfo = ref({
  className: "",
  courseName: "",
  startDate: "",
  attendancePercent: 0,
  averageScore: 0,
});

onMounted(() => {
  const classId = route.params.id;
  fetchClassDetails(classId);
});

async function fetchClassDetails(id) {
  loading.value = true;
  try {
    await new Promise((resolve) => setTimeout(resolve, 600));

    courseInfo.value = {
      className: "Lớp Lập trình Web Fullstack 01",
      courseName: "Khóa học Fullstack Web Developer",
      startDate: "15/06/2026",
      attendancePercent: 85,
      averageScore: 8.5,
    };
  } catch (error) {
    console.error("Lỗi tải dữ liệu lớp học", error);
  } finally {
    loading.value = false;
  }
}

const mockSessions = ref([
  {
    number: 1,
    title: "Tổng quan HTML & CSS",
    date: "15/06/2026",
    description: "Giới thiệu về cấu trúc web và CSS cơ bản.",
    isPast: true,
    isCurrent: false,
    hasHomework: true,
    hasDocument: true,
  },
  {
    number: 2,
    title: "Tailwind CSS Thực chiến",
    date: "18/06/2026",
    description: "Ứng dụng Tailwind CSS để dựng UI.",
    isPast: false,
    isCurrent: true,
    hasHomework: false,
    hasDocument: true,
  },
  {
    number: 3,
    title: "Javascript cơ bản",
    date: "22/06/2026",
    description: "Biến, vòng lặp, mảng và object.",
    isPast: false,
    isCurrent: false,
    hasHomework: true,
    hasDocument: false,
  },
]);

const mockDocuments = ref([
  { name: "[Slide] Bài 1 - HTML CSS.pdf", size: "2.4 MB", date: "14/06/2026" },
  {
    name: "[Bài đọc] CheatSheet Tailwind.pdf",
    size: "1.1 MB",
    date: "16/06/2026",
  },
  { name: "SourceCode_Buoi_1.zip", size: "15 MB", date: "15/06/2026" },
]);

const mockAssignments = ref([
  {
    title: "Bài tập 1: Clone giao diện Landing Page",
    deadline: "20/06/2026 23:59",
    status: "Submitted",
  },
  {
    title: "Bài tập 2: Dựng Form Đăng ký bằng Tailwind",
    deadline: "25/06/2026 23:59",
    status: "Pending",
  },
]);

const attendanceColumns = [
  { title: "Buổi học", dataIndex: "session", key: "session", width: "25%" },
  { title: "Ngày học", dataIndex: "date", key: "date", width: "25%" },
  { title: "Trạng thái", key: "status", align: "center", width: "20%" },
  { title: "Ghi chú", dataIndex: "note", key: "note" },
];

const mockAttendance = ref([
  {
    key: "1",
    session: "Buổi 1",
    date: "15/06/2026",
    status: "Present",
    note: "",
  },
  {
    key: "2",
    session: "Buổi 2",
    date: "18/06/2026",
    status: "Absent",
    note: "Có phép",
  },
]);

const gradeColumns = [
  { title: "Đầu điểm", dataIndex: "type", key: "type", width: "30%" },
  { title: "Hệ số", dataIndex: "weight", key: "weight", width: "15%" },
  { title: "Điểm số", key: "score", align: "right", width: "15%" },
  { title: "Nhận xét của giảng viên", dataIndex: "comment", key: "comment" },
];

const mockGrades = ref([
  {
    key: "1",
    type: "Bài tập về nhà 1",
    weight: "10%",
    score: 9.0,
    comment: "Làm tốt, giao diện chuẩn pixel.",
  },
  {
    key: "2",
    type: "Kiểm tra giữa kỳ",
    weight: "30%",
    score: 4.5,
    comment: "Cần chú ý lại phần logic Javascript.",
  },
]);
</script>

<style scoped>
:deep(.ant-tabs-nav) {
  margin-bottom: 0 !important;
}
:deep(.ant-tabs-tab) {
  padding: 16px 0;
  font-weight: 600;
  color: #64748b;
  font-size: 15px;
}
:deep(.ant-tabs-tab-active) {
  color: #2563eb !important;
}
/* Tuỳ chỉnh Ant Design Table chuẩn Enterprise */
:deep(.enterprise-table .ant-table-thead > tr > th) {
  background: #f8fafc;
  color: #475569;
  font-weight: 600;
  text-transform: uppercase;
  font-size: 0.75rem;
  padding: 12px 16px;
  border-bottom: 2px solid #e2e8f0;
}
:deep(.enterprise-table .ant-table-tbody > tr > td) {
  padding: 16px;
  border-bottom: 1px solid #f1f5f9;
}
:deep(.enterprise-table .ant-table-tbody > tr:hover > td) {
  background-color: #f8fafc !important;
}
</style>
