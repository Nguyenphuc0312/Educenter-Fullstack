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
          <a-tab-pane key="ai-tutor" tab="Gia sư ảo 24/7 💬" />
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

          <div v-else-if="activeTab === 'ai-tutor'">
            <div class="grid grid-cols-1 lg:grid-cols-4 gap-6 max-w-6xl mx-auto">
              <!-- Sidebar: Quick Prompts -->
              <div class="lg:col-span-1 bg-white p-4 rounded-2xl border border-slate-200 shadow-sm flex flex-col gap-3">
                <div class="flex items-center gap-2 border-b border-slate-100 pb-3 mb-1">
                  <span class="text-lg">💡</span>
                  <h3 class="font-bold text-slate-800 text-sm m-0">Gợi ý nhanh</h3>
                </div>
                <div class="flex flex-col gap-2">
                  <button
                    v-for="prompt in tutorQuickPrompts"
                    :key="prompt.label"
                    @click="askQuickQuestion(prompt.text)"
                    class="w-full text-left px-3 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-xs font-semibold text-slate-700 hover:bg-blue-50 hover:border-blue-300 hover:text-blue-600 transition-all duration-200 cursor-pointer active:scale-[0.98]"
                  >
                    {{ prompt.label }}
                  </button>
                </div>
                
                <div class="mt-auto pt-4 border-t border-slate-100 flex items-center justify-between text-xs text-slate-400">
                  <span>Trạng thái: Sẵn sàng</span>
                  <button 
                    @click="clearTutorHistory" 
                    class="text-red-500 hover:text-red-700 font-bold transition-colors cursor-pointer flex items-center gap-1"
                    title="Xóa cuộc trò chuyện hiện tại"
                  >
                    🗑️ Xóa lịch sử
                  </button>
                </div>
              </div>

              <!-- Main Chat Box -->
              <div class="lg:col-span-3 bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden flex flex-col h-[520px]">
                <!-- Chat Header -->
                <header class="bg-gradient-to-r from-blue-600 to-indigo-600 px-5 py-3 flex items-center justify-between text-white shrink-0">
                  <div class="flex items-center gap-3">
                    <div class="w-9 h-9 rounded-full bg-white/20 flex items-center justify-center font-bold text-base">🤖</div>
                    <div>
                      <h3 class="text-sm font-bold m-0 leading-none">Gia sư ảo 24/7 (AI Tutor)</h3>
                      <span class="text-[10px] text-blue-100 mt-1 block">Tự động huấn luyện trên tài liệu lớp học</span>
                    </div>
                  </div>
                  <span class="text-xs px-2.5 py-1 bg-white/20 rounded-full font-semibold border border-white/10">AI Router 1.5 Flash</span>
                </header>

                <!-- Chat Body (Messages List) -->
                <div ref="tutorMessageContainer" class="flex-1 p-5 overflow-y-auto space-y-4 bg-slate-50/50 custom-scrollbar">
                  <div
                    v-for="(msg, index) in tutorMessages"
                    :key="index"
                    class="flex items-end gap-3"
                    :class="msg.role === 'user' ? 'justify-end' : 'justify-start'"
                  >
                    <!-- AI Avatar -->
                    <div v-if="msg.role !== 'user'" class="w-8 h-8 rounded-full bg-indigo-600 flex items-center justify-center shrink-0 text-white text-xs shadow-sm">
                      🤖
                    </div>

                    <!-- Message Bubble -->
                    <div
                      class="max-w-[75%] rounded-2xl px-4 py-3 text-sm leading-relaxed shadow-sm border"
                      :class="msg.role === 'user' ? 'bg-blue-600 text-white border-blue-500 rounded-br-none' : 'bg-white text-slate-800 border-slate-200 rounded-bl-none'"
                    >
                      <div class="prose prose-sm dark:prose-invert max-w-none text-xs select-text text-inherit" v-html="parseMarkdown(msg.text)"></div>
                      <span class="text-[9px] opacity-65 block text-right mt-1.5 font-medium">
                        {{ formatMsgTime(msg.timestamp) }}
                      </span>
                    </div>

                    <!-- User Initials Avatar -->
                    <div v-if="msg.role === 'user'" class="w-8 h-8 rounded-full bg-slate-200 border border-slate-300 flex items-center justify-center shrink-0 text-[10px] font-black text-slate-700 shadow-inner">
                      {{ studentInitials }}
                    </div>
                  </div>

                  <!-- Typing indicator -->
                  <div v-if="isTutorTyping" class="flex items-end gap-3 justify-start">
                    <div class="w-8 h-8 rounded-full bg-indigo-600 flex items-center justify-center shrink-0 text-white text-xs">
                      🤖
                    </div>
                    <div class="bg-white border border-slate-200 rounded-2xl rounded-bl-none px-4 py-3 flex items-center gap-1 shadow-sm">
                      <span class="w-1.5 h-1.5 rounded-full bg-slate-400 animate-bounce"></span>
                      <span class="w-1.5 h-1.5 rounded-full bg-slate-400 animate-bounce delay-100"></span>
                      <span class="w-1.5 h-1.5 rounded-full bg-slate-400 animate-bounce delay-200"></span>
                    </div>
                  </div>
                </div>

                <!-- Input Footer -->
                <footer class="p-4 border-t border-slate-200 shrink-0 bg-white">
                  <form @submit.prevent="sendTutorMessage" class="flex items-center gap-3">
                    <input
                      v-model="tutorInput"
                      type="text"
                      placeholder="Đặt câu hỏi về buổi học, bài tập, slide tài liệu..."
                      class="flex-grow px-4 py-2.5 text-xs rounded-xl border border-slate-200 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100 transition-all placeholder:text-slate-400 text-slate-800"
                      :disabled="isTutorTyping"
                      ref="tutorInputRef"
                    />
                    <button
                      type="submit"
                      class="w-10 h-10 rounded-xl bg-blue-600 hover:bg-blue-700 active:scale-95 text-white flex items-center justify-center cursor-pointer transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-md shadow-blue-200"
                      :disabled="!tutorInput.trim() || isTutorTyping"
                    >
                      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2.5" stroke="currentColor" class="w-4 h-4">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M6 12L3.269 3.126A59.768 59.768 0 0121.485 12 59.77 59.77 0 013.27 20.876L5.999 12zm0 0h7.5" />
                      </svg>
                    </button>
                  </form>
                </footer>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch, nextTick } from "vue";
import { useRoute } from "vue-router";
import { useAuthStore } from "@/stores/auth";
import dayjs from "dayjs";
// import { studentApi } from '@/api/studentApi'
import { aiApi } from "@/api/aiApi";
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

onMounted(async () => {
  const classId = route.params.id;
  await fetchClassDetails(classId);
  loadTutorMessages();
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

// === AI VIRTUAL TUTOR SYSTEM ===
const authStore = useAuthStore();
const tutorMessages = ref([]);
const tutorInput = ref("");
const isTutorTyping = ref(false);
const tutorMessageContainer = ref(null);
const tutorInputRef = ref(null);

const studentInitials = computed(() => {
  const name = authStore.user?.fullName || authStore.user?.username || 'Học viên';
  const parts = name.trim().split(/\s+/);
  return parts.slice(-2).map(x => x[0]).join('').toUpperCase();
});

const tutorHistoryKey = computed(() => `educenter_ai_tutor_history_${route.params.id || 'default'}`);

const tutorQuickPrompts = ref([
  { label: "📖 Tóm tắt nội dung khóa học", text: "Hãy tóm tắt toàn bộ lộ trình và giáo trình của khóa học này." },
  { label: "❓ Giải thích nội dung Buổi 2", text: "Giải thích chi tiết cho tôi về kiến thức buổi học số 2: Tailwind CSS Thực chiến." },
  { label: "📝 Hướng dẫn làm Bài tập 2", text: "Hãy gợi ý giải thuật và hướng làm bài tập số 2: Dựng Form Đăng ký bằng Tailwind. Đừng cho code giải hoàn chỉnh ngay nhé." },
  { label: "📚 Tóm tắt slide tài liệu", text: "Tóm tắt các slide tài liệu chính hiện có của lớp học này." },
  { label: "📅 Lập lịch ôn tập chi tiết", text: "Dựa vào giáo trình và kết quả học tập hiện tại của tôi trong lớp này, hãy gợi ý cho tôi 1 lộ trình ôn tập kiến thức tối ưu." }
]);

function loadTutorMessages() {
  const saved = localStorage.getItem(tutorHistoryKey.value);
  if (saved) {
    try {
      tutorMessages.value = JSON.parse(saved);
    } catch {
      tutorMessages.value = [];
    }
  }
  if (tutorMessages.value.length === 0) {
    tutorMessages.value.push({
      role: "model",
      text: `Xin chào! Tôi là Gia sư ảo 24/7 của lớp học **${courseInfo.value.className || 'Lớp học'}**. Tôi đã tìm hiểu kỹ giáo trình, tài liệu và các bài tập của môn học này. Bạn có câu hỏi nào cần tôi hỗ trợ giải thích, hướng dẫn bài tập hay tóm tắt slide học tập không?`,
      timestamp: new Date().toISOString()
    });
  }
}

function saveTutorMessages() {
  localStorage.setItem(tutorHistoryKey.value, JSON.stringify(tutorMessages.value));
}

function clearTutorHistory() {
  tutorMessages.value = [
    {
      role: "model",
      text: `Đã xóa lịch sử chat. Tôi sẵn sàng hỗ trợ bạn học tập khóa học **${courseInfo.value.className || 'Lớp học'}** từ đầu!`,
      timestamp: new Date().toISOString()
    }
  ];
  saveTutorMessages();
}

function scrollTutorChatToBottom() {
  nextTick(() => {
    if (tutorMessageContainer.value) {
      tutorMessageContainer.value.scrollTop = tutorMessageContainer.value.scrollHeight;
    }
  });
}

function formatMsgTime(isoString) {
  return dayjs(isoString).format("HH:mm");
}

function parseMarkdown(text) {
  if (!text) return "";
  let html = text
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;");
  
  html = html.replace(/```([\s\S]*?)```/g, '<pre class="bg-slate-100 p-3 rounded-lg text-xs font-mono my-2 overflow-x-auto select-all text-slate-800">$1</pre>');
  html = html.replace(/`([^`\n]+)`/g, '<code class="bg-slate-100 px-1 py-0.5 rounded text-[11px] font-mono mx-0.5 text-slate-900">$1</code>');
  html = html.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
  html = html.replace(/\*([^*]+)\*/g, '<em>$1</em>');
  html = html.replace(/^\s*[-*]\s+(.*)$/gm, '<li class="ml-4 list-disc text-xs my-0.5">$1</li>');
  html = html.replace(/\n/g, "<br />");
  return html;
}

const systemInstructionText = computed(() => {
  const sessionsText = mockSessions.value.map(s => `- Buổi ${s.number}: ${s.title} (${s.date}) - ${s.description}`).join("\n");
  const docsText = mockDocuments.value.map(d => `- ${d.name} (${d.size})`).join("\n");
  const tasksText = mockAssignments.value.map(t => `- ${t.title} (Hạn: ${t.deadline}) - Trạng thái: ${t.status === 'Submitted' ? 'Đã nộp' : 'Chưa nộp'}`).join("\n");
  const gradesText = mockGrades.value.map(g => `- ${g.type}: ${g.score}/10 (${g.comment})`).join("\n");

  return `Bạn là một Gia sư ảo 24/7 (AI Tutor) thông thái, tận tâm và kiên nhẫn của trung tâm EduCenter.
Nhiệm vụ của bạn là hỗ trợ học viên giải đáp mọi thắc mắc học tập liên quan đến lớp học này.
Lưu ý quan trọng về hướng dẫn làm bài tập: Tuyệt đối không viết sẵn code giải hoàn chỉnh cho học viên. Hãy giải thích tư duy logic, các bước thuật toán hoặc cung cấp đoạn code gợi ý nhỏ (pseudocode/snippets) để học viên tự suy nghĩ và hoàn thành bài viết của mình.

THÔNG TIN LỚP HỌC (CONTEXT):
- Lớp học: ${courseInfo.value.className || 'Chưa cập nhật'}
- Khóa học: ${courseInfo.value.courseName || 'Chưa cập nhật'}
- Ngày khai giảng: ${courseInfo.value.startDate || 'Chưa cập nhật'}

GIÁO TRÌNH LỘ TRÌNH CHI TIẾT:
${sessionsText}

DANH SÁCH TÀI LIỆU LỚP HỌC:
${docsText}

DANH SÁCH BÀI TẬP VỀ NHÀ:
${tasksText}

KẾT QUẢ HỌC TẬP HIỆN TẠI CỦA HỌC VIÊN NÀY:
${gradesText}

Hãy trả lời bằng tiếng Việt, có thái độ thân thiện, khích lệ và sử dụng định dạng Markdown sạch sẽ để học viên dễ đọc bài viết của bạn.`;
});

async function askQuickQuestion(text) {
  if (isTutorTyping.value) return;
  tutorInput.value = text;
  await sendTutorMessage();
}

async function sendTutorMessage() {
  const text = tutorInput.value.trim();
  if (!text || isTutorTyping.value) return;

  tutorMessages.value.push({
    role: "user",
    text,
    timestamp: new Date().toISOString()
  });

  tutorInput.value = "";
  isTutorTyping.value = true;
  scrollTutorChatToBottom();

  const historyForAi = tutorMessages.value.slice(0, -1).map(m => ({
    role: m.role,
    content: m.text
  }));

  try {
    const data = await aiApi.complete({
      system: systemInstructionText.value,
      prompt: text,
      history: historyForAi,
      maxOutputTokens: 1400
    });
    const replyText = data.text || "T?i kh?ng nh?n ???c ph?n h?i ph? h?p t? AI. Vui l?ng h?i l?i."

    tutorMessages.value.push({
      role: "model",
      text: replyText,
      timestamp: new Date().toISOString()
    });
    
    saveTutorMessages();
  } catch (err) {
    console.error("[AI Tutor Error]", err);
    tutorMessages.value.push({
      role: "model",
      text: `❌ **Không thể kết nối với Gia sư ảo**: ${err.message || 'Lỗi mạng hoặc API key không hợp lệ'}.\n\nVui lòng thử lại sau hoặc báo với quản trị viên hệ thống để kiểm tra key AI Router.`,
      timestamp: new Date().toISOString()
    });
  } finally {
    isTutorTyping.value = false;
    scrollTutorChatToBottom();
  }
}

watch(activeTab, (newTab) => {
  if (newTab === "ai-tutor") {
    scrollTutorChatToBottom();
    nextTick(() => {
      tutorInputRef.value?.focus();
    });
  }
});
watch(route, () => {
  loadTutorMessages();
});
watch(courseInfo, () => {
  if (tutorMessages.value.length === 1 && tutorMessages.value[0].role === "model") {
    tutorMessages.value[0].text = `Xin chào! Tôi là Gia sư ảo 24/7 của lớp học **${courseInfo.value.className || 'Lớp học'}**. Tôi đã tìm hiểu kỹ giáo trình, tài liệu và các bài tập của môn học này. Bạn có câu hỏi nào cần tôi hỗ trợ giải thích, hướng dẫn bài tập hay tóm tắt slide học tập không?`;
  }
}, { deep: true });
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

