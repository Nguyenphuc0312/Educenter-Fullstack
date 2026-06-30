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
          {{ courseInfo.className || "Chi tiáº¿t lá»›p há»c" }}
        </h1>
        <p class="text-sm text-slate-500 font-medium mt-1">
          {{ courseInfo.courseName || "Äang táº£i dá»¯ liá»‡u..." }}
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
            Khai giáº£ng
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
            ChuyÃªn cáº§n
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
          <p class="text-xs text-slate-500 font-semibold uppercase">BÃ i táº­p</p>
          <strong class="text-slate-800">3/5 ÄÃ£ ná»™p</strong>
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
            Trung bÃ¬nh
          </p>
          <strong class="text-slate-800">{{
            courseInfo.averageScore || "ChÆ°a cÃ³"
          }}</strong>
        </div>
      </div>
    </div>

    <div
      class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden"
    >
      <div class="px-6 pt-4 border-b border-slate-200">
        <a-tabs v-model:activeKey="activeTab">
          <a-tab-pane key="sessions" tab="Lá»™ trÃ¬nh & Buá»•i há»c" />
          <a-tab-pane key="documents" tab="TÃ i liá»‡u" />
          <a-tab-pane key="assignments" tab="BÃ i táº­p" />
          <a-tab-pane key="attendance" tab="Äiá»ƒm danh" />
          <a-tab-pane key="grades" tab="Báº£ng Ä‘iá»ƒm" />
          <a-tab-pane key="ai-tutor" tab="Gia sÆ° áº£o 24/7 ðŸ’¬" />
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
              description="ChÆ°a cÃ³ lá»™ trÃ¬nh há»c táº­p."
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
                      Buá»•i {{ session.number }}: {{ session.title }}
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
                      CÃ³ bÃ i táº­p
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
                      CÃ³ tÃ i liá»‡u
                    </span>
                  </div>
                </div>
              </a-timeline-item>
            </a-timeline>
          </div>

          <div v-else-if="activeTab === 'documents'">
            <a-empty
              v-if="mockDocuments.length === 0"
              description="ChÆ°a cÃ³ tÃ i liá»‡u nÃ o Ä‘Æ°á»£c táº£i lÃªn."
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
                      {{ doc.size }} â€¢ Cáº­p nháº­t: {{ doc.date }}
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
              description="Giáº£ng viÃªn chÆ°a giao bÃ i táº­p nÃ o."
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
                    {{ task.status === "Submitted" ? "ÄÃ£ ná»™p" : "ChÆ°a ná»™p" }}
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
                  Háº¡n chÃ³t:
                  <strong class="text-slate-700">{{ task.deadline }}</strong>
                </p>
              </div>
              <div class="flex items-center">
                <button
                  class="px-5 py-2.5 bg-blue-50 text-blue-600 hover:bg-blue-600 hover:text-white font-semibold rounded-lg text-sm transition-colors border border-blue-200 hover:border-blue-600 whitespace-nowrap"
                >
                  Xem chi tiáº¿t
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
                      {{ record.status === "Present" ? "CÃ³ máº·t" : "Váº¯ng máº·t" }}
                    </span>
                  </template>
                </template>
                <template #emptyText>
                  <a-empty description="ChÆ°a cÃ³ dá»¯ liá»‡u Ä‘iá»ƒm danh." />
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
                  <a-empty description="ChÆ°a cÃ³ Ä‘iá»ƒm sá»‘ nÃ o Ä‘Æ°á»£c ghi nháº­n." />
                </template>
              </a-table>
            </div>
          </div>

          <div v-else-if="activeTab === 'ai-tutor'">
            <div class="grid grid-cols-1 lg:grid-cols-4 gap-6 max-w-6xl mx-auto">
              <!-- Sidebar: Quick Prompts -->
              <div class="lg:col-span-1 bg-white p-4 rounded-2xl border border-slate-200 shadow-sm flex flex-col gap-3">
                <div class="flex items-center gap-2 border-b border-slate-100 pb-3 mb-1">
                  <span class="text-lg">ðŸ’¡</span>
                  <h3 class="font-bold text-slate-800 text-sm m-0">Gá»£i Ã½ nhanh</h3>
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
                  <span>Tráº¡ng thÃ¡i: Sáºµn sÃ ng</span>
                  <button 
                    @click="clearTutorHistory" 
                    class="text-red-500 hover:text-red-700 font-bold transition-colors cursor-pointer flex items-center gap-1"
                    title="XÃ³a cuá»™c trÃ² chuyá»‡n hiá»‡n táº¡i"
                  >
                    ðŸ—‘ï¸ XÃ³a lá»‹ch sá»­
                  </button>
                </div>
              </div>

              <!-- Main Chat Box -->
              <div class="lg:col-span-3 bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden flex flex-col h-[520px]">
                <!-- Chat Header -->
                <header class="bg-gradient-to-r from-blue-600 to-indigo-600 px-5 py-3 flex items-center justify-between text-white shrink-0">
                  <div class="flex items-center gap-3">
                    <div class="w-9 h-9 rounded-full bg-white/20 flex items-center justify-center font-bold text-base">ðŸ¤–</div>
                    <div>
                      <h3 class="text-sm font-bold m-0 leading-none">Gia sÆ° áº£o 24/7 (AI Tutor)</h3>
                      <span class="text-[10px] text-blue-100 mt-1 block">Tá»± Ä‘á»™ng huáº¥n luyá»‡n trÃªn tÃ i liá»‡u lá»›p há»c</span>
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
                      ðŸ¤–
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
                      ðŸ¤–
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
                      placeholder="Äáº·t cÃ¢u há»i vá» buá»•i há»c, bÃ i táº­p, slide tÃ i liá»‡u..."
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
      className: "Lá»›p Láº­p trÃ¬nh Web Fullstack 01",
      courseName: "KhÃ³a há»c Fullstack Web Developer",
      startDate: "15/06/2026",
      attendancePercent: 85,
      averageScore: 8.5,
    };
  } catch (error) {
    console.error("Lá»—i táº£i dá»¯ liá»‡u lá»›p há»c", error);
  } finally {
    loading.value = false;
  }
}

const mockSessions = ref([
  {
    number: 1,
    title: "Tá»•ng quan HTML & CSS",
    date: "15/06/2026",
    description: "Giá»›i thiá»‡u vá» cáº¥u trÃºc web vÃ  CSS cÆ¡ báº£n.",
    isPast: true,
    isCurrent: false,
    hasHomework: true,
    hasDocument: true,
  },
  {
    number: 2,
    title: "Tailwind CSS Thá»±c chiáº¿n",
    date: "18/06/2026",
    description: "á»¨ng dá»¥ng Tailwind CSS Ä‘á»ƒ dá»±ng UI.",
    isPast: false,
    isCurrent: true,
    hasHomework: false,
    hasDocument: true,
  },
  {
    number: 3,
    title: "Javascript cÆ¡ báº£n",
    date: "22/06/2026",
    description: "Biáº¿n, vÃ²ng láº·p, máº£ng vÃ  object.",
    isPast: false,
    isCurrent: false,
    hasHomework: true,
    hasDocument: false,
  },
]);

const mockDocuments = ref([
  { name: "[Slide] BÃ i 1 - HTML CSS.pdf", size: "2.4 MB", date: "14/06/2026" },
  {
    name: "[BÃ i Ä‘á»c] CheatSheet Tailwind.pdf",
    size: "1.1 MB",
    date: "16/06/2026",
  },
  { name: "SourceCode_Buoi_1.zip", size: "15 MB", date: "15/06/2026" },
]);

const mockAssignments = ref([
  {
    title: "BÃ i táº­p 1: Clone giao diá»‡n Landing Page",
    deadline: "20/06/2026 23:59",
    status: "Submitted",
  },
  {
    title: "BÃ i táº­p 2: Dá»±ng Form ÄÄƒng kÃ½ báº±ng Tailwind",
    deadline: "25/06/2026 23:59",
    status: "Pending",
  },
]);

const attendanceColumns = [
  { title: "Buá»•i há»c", dataIndex: "session", key: "session", width: "25%" },
  { title: "NgÃ y há»c", dataIndex: "date", key: "date", width: "25%" },
  { title: "Tráº¡ng thÃ¡i", key: "status", align: "center", width: "20%" },
  { title: "Ghi chÃº", dataIndex: "note", key: "note" },
];

const mockAttendance = ref([
  {
    key: "1",
    session: "Buá»•i 1",
    date: "15/06/2026",
    status: "Present",
    note: "",
  },
  {
    key: "2",
    session: "Buá»•i 2",
    date: "18/06/2026",
    status: "Absent",
    note: "CÃ³ phÃ©p",
  },
]);

const gradeColumns = [
  { title: "Äáº§u Ä‘iá»ƒm", dataIndex: "type", key: "type", width: "30%" },
  { title: "Há»‡ sá»‘", dataIndex: "weight", key: "weight", width: "15%" },
  { title: "Äiá»ƒm sá»‘", key: "score", align: "right", width: "15%" },
  { title: "Nháº­n xÃ©t cá»§a giáº£ng viÃªn", dataIndex: "comment", key: "comment" },
];

const mockGrades = ref([
  {
    key: "1",
    type: "BÃ i táº­p vá» nhÃ  1",
    weight: "10%",
    score: 9.0,
    comment: "LÃ m tá»‘t, giao diá»‡n chuáº©n pixel.",
  },
  {
    key: "2",
    type: "Kiá»ƒm tra giá»¯a ká»³",
    weight: "30%",
    score: 4.5,
    comment: "Cáº§n chÃº Ã½ láº¡i pháº§n logic Javascript.",
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
  const name = authStore.user?.fullName || authStore.user?.username || 'Há»c viÃªn';
  const parts = name.trim().split(/\s+/);
  return parts.slice(-2).map(x => x[0]).join('').toUpperCase();
});

const tutorHistoryKey = computed(() => `educenter_ai_tutor_history_${route.params.id || 'default'}`);

const tutorQuickPrompts = ref([
  { label: "ðŸ“– TÃ³m táº¯t ná»™i dung khÃ³a há»c", text: "HÃ£y tÃ³m táº¯t toÃ n bá»™ lá»™ trÃ¬nh vÃ  giÃ¡o trÃ¬nh cá»§a khÃ³a há»c nÃ y." },
  { label: "â“ Giáº£i thÃ­ch ná»™i dung Buá»•i 2", text: "Giáº£i thÃ­ch chi tiáº¿t cho tÃ´i vá» kiáº¿n thá»©c buá»•i há»c sá»‘ 2: Tailwind CSS Thá»±c chiáº¿n." },
  { label: "ðŸ“ HÆ°á»›ng dáº«n lÃ m BÃ i táº­p 2", text: "HÃ£y gá»£i Ã½ giáº£i thuáº­t vÃ  hÆ°á»›ng lÃ m bÃ i táº­p sá»‘ 2: Dá»±ng Form ÄÄƒng kÃ½ báº±ng Tailwind. Äá»«ng cho code giáº£i hoÃ n chá»‰nh ngay nhÃ©." },
  { label: "ðŸ“š TÃ³m táº¯t slide tÃ i liá»‡u", text: "TÃ³m táº¯t cÃ¡c slide tÃ i liá»‡u chÃ­nh hiá»‡n cÃ³ cá»§a lá»›p há»c nÃ y." },
  { label: "ðŸ“… Láº­p lá»‹ch Ã´n táº­p chi tiáº¿t", text: "Dá»±a vÃ o giÃ¡o trÃ¬nh vÃ  káº¿t quáº£ há»c táº­p hiá»‡n táº¡i cá»§a tÃ´i trong lá»›p nÃ y, hÃ£y gá»£i Ã½ cho tÃ´i 1 lá»™ trÃ¬nh Ã´n táº­p kiáº¿n thá»©c tá»‘i Æ°u." }
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
      text: `Xin chÃ o! TÃ´i lÃ  Gia sÆ° áº£o 24/7 cá»§a lá»›p há»c **${courseInfo.value.className || 'Lá»›p há»c'}**. TÃ´i Ä‘Ã£ tÃ¬m hiá»ƒu ká»¹ giÃ¡o trÃ¬nh, tÃ i liá»‡u vÃ  cÃ¡c bÃ i táº­p cá»§a mÃ´n há»c nÃ y. Báº¡n cÃ³ cÃ¢u há»i nÃ o cáº§n tÃ´i há»— trá»£ giáº£i thÃ­ch, hÆ°á»›ng dáº«n bÃ i táº­p hay tÃ³m táº¯t slide há»c táº­p khÃ´ng?`,
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
      text: `ÄÃ£ xÃ³a lá»‹ch sá»­ chat. TÃ´i sáºµn sÃ ng há»— trá»£ báº¡n há»c táº­p khÃ³a há»c **${courseInfo.value.className || 'Lá»›p há»c'}** tá»« Ä‘áº§u!`,
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
  const sessionsText = mockSessions.value.map(s => `- Buá»•i ${s.number}: ${s.title} (${s.date}) - ${s.description}`).join("\n");
  const docsText = mockDocuments.value.map(d => `- ${d.name} (${d.size})`).join("\n");
  const tasksText = mockAssignments.value.map(t => `- ${t.title} (Háº¡n: ${t.deadline}) - Tráº¡ng thÃ¡i: ${t.status === 'Submitted' ? 'ÄÃ£ ná»™p' : 'ChÆ°a ná»™p'}`).join("\n");
  const gradesText = mockGrades.value.map(g => `- ${g.type}: ${g.score}/10 (${g.comment})`).join("\n");

  return `Báº¡n lÃ  má»™t Gia sÆ° áº£o 24/7 (AI Tutor) thÃ´ng thÃ¡i, táº­n tÃ¢m vÃ  kiÃªn nháº«n cá»§a trung tÃ¢m EduCenter.
Nhiá»‡m vá»¥ cá»§a báº¡n lÃ  há»— trá»£ há»c viÃªn giáº£i Ä‘Ã¡p má»i tháº¯c máº¯c há»c táº­p liÃªn quan Ä‘áº¿n lá»›p há»c nÃ y.
LÆ°u Ã½ quan trá»ng vá» hÆ°á»›ng dáº«n lÃ m bÃ i táº­p: Tuyá»‡t Ä‘á»‘i khÃ´ng viáº¿t sáºµn code giáº£i hoÃ n chá»‰nh cho há»c viÃªn. HÃ£y giáº£i thÃ­ch tÆ° duy logic, cÃ¡c bÆ°á»›c thuáº­t toÃ¡n hoáº·c cung cáº¥p Ä‘oáº¡n code gá»£i Ã½ nhá» (pseudocode/snippets) Ä‘á»ƒ há»c viÃªn tá»± suy nghÄ© vÃ  hoÃ n thÃ nh bÃ i viáº¿t cá»§a mÃ¬nh.

THÃ”NG TIN Lá»šP Há»ŒC (CONTEXT):
- Lá»›p há»c: ${courseInfo.value.className || 'ChÆ°a cáº­p nháº­t'}
- KhÃ³a há»c: ${courseInfo.value.courseName || 'ChÆ°a cáº­p nháº­t'}
- NgÃ y khai giáº£ng: ${courseInfo.value.startDate || 'ChÆ°a cáº­p nháº­t'}

GIÃO TRÃŒNH Lá»˜ TRÃŒNH CHI TIáº¾T:
${sessionsText}

DANH SÃCH TÃ€I LIá»†U Lá»šP Há»ŒC:
${docsText}

DANH SÃCH BÃ€I Táº¬P Vá»€ NHÃ€:
${tasksText}

Káº¾T QUáº¢ Há»ŒC Táº¬P HIá»†N Táº I Cá»¦A Há»ŒC VIÃŠN NÃ€Y:
${gradesText}

HÃ£y tráº£ lá»i báº±ng tiáº¿ng Viá»‡t, cÃ³ thÃ¡i Ä‘á»™ thÃ¢n thiá»‡n, khÃ­ch lá»‡ vÃ  sá»­ dá»¥ng Ä‘á»‹nh dáº¡ng Markdown sáº¡ch sáº½ Ä‘á»ƒ há»c viÃªn dá»… Ä‘á»c bÃ i viáº¿t cá»§a báº¡n.`;
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
      text: `âŒ **KhÃ´ng thá»ƒ káº¿t ná»‘i vá»›i Gia sÆ° áº£o**: ${err.message || 'Lá»—i máº¡ng hoáº·c API key khÃ´ng há»£p lá»‡'}.\n\nVui lÃ²ng thá»­ láº¡i sau hoáº·c bÃ¡o vá»›i quáº£n trá»‹ viÃªn há»‡ thá»‘ng Ä‘á»ƒ kiá»ƒm tra key AI Router.`,
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
    tutorMessages.value[0].text = `Xin chÃ o! TÃ´i lÃ  Gia sÆ° áº£o 24/7 cá»§a lá»›p há»c **${courseInfo.value.className || 'Lá»›p há»c'}**. TÃ´i Ä‘Ã£ tÃ¬m hiá»ƒu ká»¹ giÃ¡o trÃ¬nh, tÃ i liá»‡u vÃ  cÃ¡c bÃ i táº­p cá»§a mÃ´n há»c nÃ y. Báº¡n cÃ³ cÃ¢u há»i nÃ o cáº§n tÃ´i há»— trá»£ giáº£i thÃ­ch, hÆ°á»›ng dáº«n bÃ i táº­p hay tÃ³m táº¯t slide há»c táº­p khÃ´ng?`;
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
/* Tuá»³ chá»‰nh Ant Design Table chuáº©n Enterprise */
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

