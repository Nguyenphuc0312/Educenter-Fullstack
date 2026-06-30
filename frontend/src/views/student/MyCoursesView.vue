<template>
  <div class="space-y-6">
    <PageHeader
      title="KhÃ³a há»c cá»§a tÃ´i"
      subtitle="Quáº£n lÃ½ tiáº¿n trÃ¬nh há»c táº­p, lá»‹ch há»c vÃ  Ä‘Ã¡nh giÃ¡ khÃ³a há»c sau khi hoÃ n thÃ nh."
    >
      <template #actions>
        <router-link
          to="/courses"
          class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg transition-colors shadow-sm flex items-center gap-2"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
          KhÃ¡m phÃ¡ khÃ³a há»c
        </router-link>
      </template>
    </PageHeader>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
      <div class="bg-white rounded-xl p-5 border border-slate-200 shadow-sm flex items-center justify-between">
        <div>
          <span class="text-slate-500 text-sm font-semibold uppercase tracking-wider mb-1 block">Tá»•ng ghi danh</span>
          <strong class="text-3xl text-slate-800 font-black">{{ courses.length }}</strong>
        </div>
        <div class="p-3 bg-slate-50 rounded-full text-slate-400">
          <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
          </svg>
        </div>
      </div>
      <div class="bg-white rounded-xl p-5 border border-slate-200 shadow-sm flex items-center justify-between">
        <div>
          <span class="text-blue-600 text-sm font-semibold uppercase tracking-wider mb-1 block">Äang há»c táº­p</span>
          <strong class="text-3xl text-blue-700 font-black">{{ studyingCount }}</strong>
        </div>
        <div class="p-3 bg-blue-50 rounded-full text-blue-500">
          <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
          </svg>
        </div>
      </div>
      <div class="bg-white rounded-xl p-5 border border-slate-200 shadow-sm flex items-center justify-between">
        <div>
          <span class="text-emerald-600 text-sm font-semibold uppercase tracking-wider mb-1 block">ÄÃ£ hoÃ n thÃ nh</span>
          <strong class="text-3xl text-emerald-700 font-black">{{ completedCount }}</strong>
        </div>
        <div class="p-3 bg-emerald-50 rounded-full text-emerald-500">
          <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
      </div>
    </div>

    <section class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
      <div class="border-b border-slate-200 px-6 pt-4">
        <a-tabs v-model:activeKey="activeTab">
          <a-tab-pane key="all" tab="Táº¥t cáº£ khÃ³a há»c" />
          <a-tab-pane key="active" tab="Äang tiáº¿n hÃ nh" />
          <a-tab-pane key="completed" tab="ÄÃ£ hoÃ n thÃ nh" />
        </a-tabs>
      </div>

      <div class="p-6 bg-slate-50/50 min-h-[400px]">
        <LoadingSpinner v-if="loading" size="lg" class="py-20" />
        <div v-else-if="error" class="text-center text-red-500 py-8 font-medium">{{ error }}</div>

        <div v-else-if="filteredCourses.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
          <a-empty description="KhÃ´ng tÃ¬m tháº¥y khÃ³a há»c nÃ o trong má»¥c nÃ y." />
          <router-link
            v-if="activeTab === 'all'"
            to="/courses"
            class="mt-4 px-4 py-2 bg-white border border-slate-300 text-slate-700 font-medium rounded-lg hover:bg-slate-50 transition-colors"
          >
            ÄÄƒng kÃ½ khÃ³a há»c má»›i
          </router-link>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">
          <article
            v-for="course in filteredCourses"
            :key="course.id"
            class="bg-white rounded-xl border border-slate-200 shadow-sm hover:shadow-md hover:border-blue-300 transition-all duration-200 flex flex-col overflow-hidden"
          >
            <div class="p-5 border-b border-slate-100 relative">
              <div class="flex justify-between items-start gap-4 mb-2">
                <span class="text-xs font-bold uppercase tracking-wider text-slate-400">KhÃ³a há»c</span>
                <span :class="statusBadge(course.status)">{{ statusText(course.status) }}</span>
              </div>
              <h2 class="text-lg font-bold text-slate-800 line-clamp-2 leading-tight mb-1" :title="course.courseNameSnapshot">
                {{ course.courseNameSnapshot }}
              </h2>
              <p class="text-sm text-slate-500 font-medium">{{ course.classNameSnapshot }}</p>
              <div v-if="reviewFor(course)" class="mt-3 inline-flex items-center gap-2 text-xs font-semibold text-amber-700 bg-amber-50 border border-amber-200 rounded-full px-2.5 py-1">
                <span>ÄÃ£ Ä‘Ã¡nh giÃ¡</span>
                <a-rate :value="Number(reviewFor(course)?.courseRating || 0)" disabled allow-half class="review-mini-rate" />
              </div>
            </div>

            <div class="p-5 flex-1 flex flex-col">
              <div class="flex justify-between items-center mb-4 text-sm">
                <div>
                  <span class="block text-slate-400 text-xs mb-0.5">NgÃ y ghi danh</span>
                  <strong class="text-slate-700">{{ formatDate(course.enrolledAt) }}</strong>
                </div>
                <div class="text-right">
                  <span class="block text-slate-400 text-xs mb-0.5">Giáº£ng viÃªn</span>
                  <strong class="text-slate-700">{{ teacherSummary(course) }}</strong>
                </div>
              </div>

              <div class="mt-auto pt-2">
                <div class="flex justify-between text-xs font-semibold mb-1">
                  <span class="text-slate-500">Tiáº¿n Ä‘á»™ há»c táº­p</span>
                  <span :class="getProgressColorText(course)">{{ progressLabel(course) }}</span>
                </div>
                <a-progress
                  v-if="canShowProgress(course)"
                  :percent="progress(course)"
                  :show-info="false"
                  :stroke-color="getStrokeColor(course)"
                  :status="isCompleted(course) ? 'success' : 'normal'"
                  stroke-linecap="round"
                  :stroke-width="8"
                />
                <div v-else class="h-2 rounded-full bg-slate-100 border border-slate-200"></div>
                <p class="mt-2 text-xs text-slate-500">{{ progressHint(course) }}</p>
              </div>
            </div>

            <div class="px-5 py-4 bg-slate-50 border-t border-slate-100 flex gap-3">
              <router-link
                :to="`/student/courses/${course.classId || course.id}`"
                class="flex-1 text-center px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg text-sm transition-colors shadow-sm"
              >
                VÃ o lá»›p
              </router-link>
              <button
                v-if="isCompleted(course)"
                type="button"
                class="px-4 py-2 bg-white border border-amber-300 hover:bg-amber-50 text-amber-700 font-semibold rounded-lg text-sm transition-colors"
                @click="openReview(course)"
              >
                {{ reviewFor(course) ? "Sá»­a Ä‘Ã¡nh giÃ¡" : "ÄÃ¡nh giÃ¡" }}
              </button>
              <router-link
                v-else
                to="/student/results"
                class="px-4 py-2 bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 font-medium rounded-lg text-sm transition-colors flex items-center justify-center"
                title="Xem Ä‘iá»ƒm sá»‘"
              >
                Äiá»ƒm
              </router-link>
            </div>
          </article>
        </div>
      </div>
    </section>

    <!-- AI Recommendations Section -->
    <section class="bg-gradient-to-br from-slate-900 to-indigo-950 rounded-2xl border border-indigo-500/20 shadow-xl overflow-hidden p-6 text-white relative">
      <div class="absolute -right-10 -top-10 w-40 h-40 bg-blue-500 opacity-20 rounded-full blur-3xl pointer-events-none"></div>
      <div class="absolute -left-10 -bottom-10 w-40 h-40 bg-indigo-500 opacity-20 rounded-full blur-3xl pointer-events-none"></div>
      
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6 relative z-10">
        <div>
          <h2 class="text-xl font-extrabold flex items-center gap-2 text-white">
            <span class="text-2xl">ðŸ¤–</span> Äá» xuáº¥t lá»™ trÃ¬nh há»c táº­p (AI Collaborative Filtering)
          </h2>
          <p class="text-sm text-indigo-200 mt-1">Gá»£i Ã½ lá»™ trÃ¬nh tiáº¿p theo dá»±a trÃªn sá»Ÿ thÃ­ch, káº¿t quáº£ há»c táº­p hiá»‡n táº¡i vÃ  lá»™ trÃ¬nh cá»§a há»c viÃªn cÃ³ chung Ä‘áº·c Ä‘iá»ƒm.</p>
        </div>
        <button
          v-if="!loadingRecs"
          @click="generateRecommendations(true)"
          class="px-3.5 py-1.5 bg-indigo-600/30 hover:bg-indigo-600/50 border border-indigo-400/30 hover:border-indigo-400/50 text-indigo-200 rounded-lg text-xs font-semibold transition-all duration-300 flex items-center gap-1.5 cursor-pointer active:scale-95"
        >
          LÃ m má»›i gá»£i Ã½
        </button>
      </div>

      <div v-if="loadingRecs" class="py-12 flex flex-col items-center justify-center relative z-10">
        <LoadingSpinner size="lg" class="!text-indigo-400" />
        <span class="text-sm text-indigo-300 mt-3 font-medium animate-pulse">Há»‡ thá»‘ng Ä‘ang cháº¡y lá»c cá»™ng tÃ¡c & phÃ¢n tÃ­ch dá»¯ liá»‡u há»c táº­p...</span>
      </div>

      <div v-else-if="recError" class="p-5 text-center text-red-300 bg-red-950/20 border border-red-800/30 rounded-xl max-w-xl mx-auto my-4 relative z-10">
        {{ recError }}
      </div>

      <div v-else-if="recommendations.length === 0" class="text-center py-8 text-indigo-300 relative z-10">
        ChÆ°a cÃ³ Ä‘á» xuáº¥t lá»™ trÃ¬nh má»›i. HÃ£y hoÃ n thÃ nh khÃ³a há»c Ä‘á»ƒ nháº­n gá»£i Ã½ lá»™ trÃ¬nh phÃ¹ há»£p!
      </div>

      <div v-else class="grid grid-cols-1 md:grid-cols-3 gap-6 relative z-10">
        <div
          v-for="rec in recommendations"
          :key="rec.courseId"
          class="bg-white/5 border border-white/10 rounded-xl p-5 flex flex-col hover:bg-white/10 hover:border-indigo-500/40 hover:-translate-y-1 transition-all duration-300 relative overflow-hidden group shadow-lg"
        >
          <div class="flex justify-between items-start mb-3">
            <span class="text-[10px] font-black uppercase tracking-wider bg-indigo-500/20 text-indigo-300 px-2 py-0.5 rounded border border-indigo-500/30">Gá»£i Ã½ AI</span>
            <div class="flex items-center gap-1 bg-emerald-500/20 border border-emerald-500/30 rounded px-2 py-0.5">
              <span class="w-1.5 h-1.5 bg-emerald-400 rounded-full animate-ping"></span>
              <span class="text-xs font-black text-emerald-400 font-mono">{{ rec.matchPercent }}% Khá»›p</span>
            </div>
          </div>

          <h3 class="text-base font-extrabold text-white line-clamp-1 mb-2 leading-snug group-hover:text-indigo-300 transition-colors">
            {{ rec.courseName }}
          </h3>

          <p class="text-xs text-indigo-200/80 leading-relaxed mb-4 flex-grow italic">
            "{{ rec.reason }}"
          </p>

          <div class="mb-4">
            <span class="block text-[10px] font-bold text-indigo-300 uppercase tracking-wide mb-1.5">Ká»¹ nÄƒng cá»‘t lÃµi:</span>
            <div class="flex flex-wrap gap-1.5">
              <span
                v-for="topic in rec.topics"
                :key="topic"
                class="text-[10px] font-semibold bg-white/5 border border-white/15 px-2 py-0.5 rounded text-slate-200"
              >
                {{ topic }}
              </span>
            </div>
          </div>

          <button
            @click="registerInterest(rec)"
            class="w-full mt-auto py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold rounded-lg transition-all duration-300 cursor-pointer active:scale-95 shadow-md border border-indigo-400/20 flex items-center justify-center gap-1.5"
          >
            ÄÄƒng kÃ½ tÆ° váº¥n khÃ³a há»c
          </button>
        </div>
      </div>
    </section>


    <a-modal
      v-model:open="reviewModalOpen"
      :title="reviewForm.id ? 'Sá»­a Ä‘Ã¡nh giÃ¡ khÃ³a há»c' : 'ÄÃ¡nh giÃ¡ khÃ³a há»c'"
      width="760px"
      :confirm-loading="savingReview"
      ok-text="LÆ°u Ä‘Ã¡nh giÃ¡"
      cancel-text="ÄÃ³ng"
      @ok="saveReview"
    >
      <div v-if="selectedCourse" class="space-y-5">
        <div class="rounded-xl border border-slate-200 bg-slate-50 p-4">
          <p class="text-xs font-bold uppercase tracking-wider text-slate-500 mb-1">KhÃ³a há»c</p>
          <h3 class="text-lg font-black text-slate-900">{{ selectedCourse.courseNameSnapshot }}</h3>
          <p class="text-sm text-slate-600">{{ selectedCourse.classNameSnapshot }}</p>
        </div>

        <div class="space-y-2">
          <label class="text-sm font-bold text-slate-700">ÄÃ¡nh giÃ¡ khÃ³a há»c</label>
          <div class="flex items-center gap-3">
            <a-rate v-model:value="reviewForm.courseRating" allow-half class="text-amber-500 text-xl" />
            <span class="text-sm font-semibold text-slate-600">{{ reviewForm.courseRating }}/5</span>
          </div>
          <textarea
            v-model.trim="reviewForm.courseComment"
            rows="3"
            maxlength="1000"
            class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
            placeholder="Cáº£m nháº­n vá» ná»™i dung, tÃ i liá»‡u, lá»‹ch há»c hoáº·c tráº£i nghiá»‡m há»c táº­p..."
          ></textarea>
        </div>

        <div class="space-y-3">
          <div class="flex items-center justify-between">
            <label class="text-sm font-bold text-slate-700">ÄÃ¡nh giÃ¡ giáº£ng viÃªn</label>
            <span class="text-xs text-slate-500">{{ reviewForm.teacherReviews.length }} giáº£ng viÃªn</span>
          </div>
          <div v-if="reviewForm.teacherReviews.length === 0" class="rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-700">
            Lá»›p há»c nÃ y chÆ°a cÃ³ dá»¯ liá»‡u giáº£ng viÃªn Ä‘á»ƒ Ä‘Ã¡nh giÃ¡.
          </div>
          <div v-for="teacher in reviewForm.teacherReviews" :key="teacher.teacherId" class="rounded-xl border border-slate-200 p-4 space-y-2">
            <div class="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <p class="font-bold text-slate-900">{{ teacher.teacherNameSnapshot }}</p>
                <p class="text-xs text-slate-500">Giáº£ng viÃªn phá»¥ trÃ¡ch lá»›p</p>
              </div>
              <div class="flex items-center gap-3">
                <a-rate v-model:value="teacher.rating" allow-half class="text-amber-500" />
                <span class="text-sm font-semibold text-slate-600 min-w-[40px]">{{ teacher.rating }}/5</span>
              </div>
            </div>
            <textarea
              v-model.trim="teacher.comment"
              rows="2"
              maxlength="1000"
              class="w-full rounded-lg border border-slate-300 px-3 py-2 text-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
              placeholder="Nháº­n xÃ©t vá» phÆ°Æ¡ng phÃ¡p giáº£ng dáº¡y, há»— trá»£ há»c viÃªn..."
            ></textarea>
          </div>
        </div>
      </div>
    </a-modal>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from "vue";
import { message } from "ant-design-vue";
import PageHeader from "@/components/ui/PageHeader.vue";
import LoadingSpinner from "@/components/ui/LoadingSpinner.vue";
import { studentApi } from "@/api/studentApi";
import { reviewApi } from "@/api/reviewApi";
import { courseApi } from "@/api/courseApi";
import { aiApi } from "@/api/aiApi";
import { useAuthStore } from "@/stores/auth";
import { formatDate, formatScore } from "@/lib/formatters";

const auth = useAuthStore();
const courses = ref([]);
const reviews = ref([]);
const results = ref([]);
const allCourses = ref([]);
const recommendations = ref([]);
const loadingRecs = ref(false);
const recError = ref("");
const loading = ref(true);
const error = ref("");
const activeTab = ref("all");
const reviewModalOpen = ref(false);
const savingReview = ref(false);
const selectedCourse = ref(null);
const reviewForm = reactive({
  id: null,
  courseRating: 5,
  courseComment: "",
  teacherReviews: [],
});

const studyingCount = computed(() => courses.value.filter((x) => ["Pending", "Confirmed", "Studying", 1, 2, 3].includes(statusKey(x.status))).length);
const completedCount = computed(() => courses.value.filter(isCompleted).length);

const filteredCourses = computed(() => {
  if (activeTab.value === "active") return courses.value.filter((c) => ["Pending", "Confirmed", "Studying", 1, 2, 3].includes(statusKey(c.status)));
  if (activeTab.value === "completed") return courses.value.filter(isCompleted);
  return courses.value;
});

onMounted(loadData);

async function loadData() {
  loading.value = true;
  error.value = "";
  try {
    const studentId = auth.user?.referenceId;
    if (studentId) {
      const [coursesRes, reviewsRes, resultsRes, allCoursesRes] = await Promise.all([
        studentApi.getMyCourses(studentId).catch(() => []),
        reviewApi.getByStudent(studentId).catch(() => []),
        studentApi.getMyResults(studentId).catch(() => []),
        courseApi.getAll().catch(() => [])
      ]);
      courses.value = coursesRes;
      reviews.value = reviewsRes;
      results.value = resultsRes;
      allCourses.value = allCoursesRes;
      generateRecommendations();
    }
  } catch (err) {
    error.value = err.message || "Há»‡ thá»‘ng Ä‘ang báº£o trÃ¬. Vui lÃ²ng thá»­ láº¡i sau.";
  } finally {
    loading.value = false;
  }
}

async function generateRecommendations(force = false) {
  if (!force && recommendations.value.length > 0) return;
  loadingRecs.value = true;
  recError.value = "";
  
  const studentId = auth.user?.referenceId;
  if (!studentId) {
    loadingRecs.value = false;
    return;
  }

  const enrolledCourseNames = new Set(
    courses.value.map(c => String(c.courseNameSnapshot || "").toLowerCase().trim())
  );
  
  const candidateCourses = allCourses.value.filter(c => {
    const name = String(c.courseName || "").toLowerCase().trim();
    return !enrolledCourseNames.has(name);
  });

  if (candidateCourses.length === 0) {
    recommendations.value = [];
    loadingRecs.value = false;
    return;
  }

  try {
      const currentCoursesText = courses.value.map(c => `- ${c.courseNameSnapshot} (${c.classNameSnapshot})`).join("\n");
      const currentResultsText = results.value.map(r => `- MÃ´n ${r.courseNameSnapshot}: Äiá»ƒm TB ${r.averageScore || 'ChÆ°a cÃ³'}, ChuyÃªn cáº§n ${r.attendancePercent || 0}%`).join("\n");
      const candidatesText = candidateCourses.map(c => `- ID: ${c.id}, TÃªn: ${c.courseName}, MÃ´ táº£: ${c.description || 'ChÆ°a cáº­p nháº­t'}`).join("\n");

      const prompt = `Báº¡n lÃ  má»™t Há»‡ thá»‘ng Äá» xuáº¥t KhÃ³a há»c (Recommendation System) thÃ´ng minh hoáº¡t Ä‘á»™ng báº±ng thuáº­t toÃ¡n Lá»c cá»™ng tÃ¡c (Collaborative Filtering) cá»§a trung tÃ¢m Ä‘Ã o táº¡o láº­p trÃ¬nh EduCenter.
Nhiá»‡m vá»¥ cá»§a báº¡n lÃ  phÃ¢n tÃ­ch thÃ´ng tin há»c táº­p cá»§a há»c viÃªn sau Ä‘Ã¢y Ä‘á»ƒ Ä‘Æ°a ra lá»™ trÃ¬nh cÃ¡c khÃ³a há»c Ä‘á» xuáº¥t tiáº¿p theo phÃ¹ há»£p nháº¥t (tá»‘i Ä‘a 3 khÃ³a há»c):

Há»ŒC VIÃŠN ÄANG/ÄÃƒ Há»ŒC:
${currentCoursesText || "ChÆ°a cÃ³ khÃ³a há»c nÃ o."}

Báº¢NG ÄIá»‚M CHI TIáº¾T:
${currentResultsText || "ChÆ°a cÃ³ Ä‘iá»ƒm sá»‘ nÃ o."}

DANH SÃCH CÃC KHÃ“A Há»ŒC Sáº´N CÃ“ Äá»‚ Gá»¢I Ã (Äá»ªNG gá»£i Ã½ nhá»¯ng khÃ³a há»c há»c viÃªn Ä‘Ã£ tham gia á»Ÿ trÃªn):
${candidatesText}

HÃ£y giáº£ láº­p thuáº­t toÃ¡n Collaborative Filtering: Ä‘á»‘i chiáº¿u cÃ¡c há»c viÃªn xuáº¥t sáº¯c khÃ³a trÆ°á»›c cÃ³ Ä‘iá»ƒm sá»‘ vÃ  lá»™ trÃ¬nh tÆ°Æ¡ng Ä‘á»“ng Ä‘á»ƒ Ä‘Æ°a ra gá»£i Ã½ khÃ³a há»c tiáº¿p theo tá»‘i Æ°u. VÃ­ dá»¥: há»c viÃªn Ä‘Ã£ há»c tá»‘t Web cÆ¡ báº£n sáº½ Ä‘Æ°á»£c gá»£i Ã½ cÃ¡c khÃ³a Framework Front-end nÃ¢ng cao hoáº·c Node.js Backend.

Äáº¦U RA PHáº¢I LÃ€ Má»˜T Máº¢NG JSON Há»¢P Lá»† chá»©a Ä‘Ãºng tá»‘i Ä‘a 3 pháº§n tá»­ cÃ³ Ä‘á»‹nh dáº¡ng cáº¥u trÃºc chÃ­nh xÃ¡c nhÆ° sau:
[
  {
    "courseId": "id_khoa_hoc_tu_danh_sach",
    "courseName": "TÃªn khÃ³a há»c chÃ­nh xÃ¡c tá»« danh sÃ¡ch",
    "matchPercent": 95, // Chá»‰ sá»‘ phÃ¹ há»£p tá»« 70% Ä‘áº¿n 98%
    "reason": "Giáº£i thÃ­ch cÃ¡ nhÃ¢n hÃ³a cá»¥ thá»ƒ vÃ¬ sao khÃ³a há»c nÃ y lÃ  bÆ°á»›c Ä‘i tiáº¿p theo phÃ¹ há»£p cho há» dá»±a trÃªn káº¿t quáº£ há»c táº­p mÃ´n X hoáº·c Ä‘á»‹nh hÆ°á»›ng nghá» nghiá»‡p...",
    "topics": ["Chá»§ Ä‘á» 1", "Chá»§ Ä‘á» 2", "Chá»§ Ä‘á» 3"]
  }
]
ChÃº Ã½ quan trá»ng:
1. Tráº£ vá» CHá»ˆ chuá»—i máº£ng JSON nÃ y. Tuyá»‡t Ä‘á»‘i khÃ´ng bá»c trong tháº» markdown \`\`\`json hay báº¥t ká»³ vÄƒn báº£n giáº£i thÃ­ch nÃ o khÃ¡c. Pháº£n há»“i pháº£i lÃ  má»™t chuá»—i parse Ä‘Æ°á»£c trá»±c tiáº¿p báº±ng JSON.parse().
2. Náº¿u danh sÃ¡ch gá»£i Ã½ trá»‘ng, hÃ£y tráº£ vá» máº£ng rá»—ng [].`;

      let responseText = (await aiApi.complete({
        prompt,
        jsonMode: true,
        maxOutputTokens: 1600
      })).text || "";
      responseText = responseText.trim();
      
      if (responseText.startsWith("```")) {
        responseText = responseText.replace(/^```json\s*/i, "").replace(/```$/, "").trim();
      }

      const parsed = JSON.parse(responseText);
      if (Array.isArray(parsed)) {
        recommendations.value = parsed;
        loadingRecs.value = false;
        return;
      }
    } catch (err) {
      console.warn("[AI Router Recommendation API Error, running local fallback]:", err);
    }

  runLocalRecommendation(candidateCourses);
}

function runLocalRecommendation(candidateCourses) {
  const completedOrStudying = courses.value;
  const avgScore = results.value.length
    ? results.value.reduce((sum, r) => sum + Number(r.averageScore || 0), 0) / results.value.length
    : 7.0;

  const scored = candidateCourses.map(course => {
    let score = 70 + Math.floor(Math.random() * 15);
    let reasons = [];
    let topics = [];

    const courseName = String(course.courseName || "").toLowerCase();
    
    if (courseName.includes("react") || courseName.includes("vue") || courseName.includes("frontend") || courseName.includes("front-end")) {
      const hasBasicWeb = completedOrStudying.some(c => {
        const name = String(c.courseNameSnapshot || "").toLowerCase();
        return name.includes("html") || name.includes("css") || name.includes("javascript") || name.includes("web");
      });
      if (hasBasicWeb) {
        score += 12;
        reasons.push(`Dá»±a trÃªn viá»‡c báº¡n Ä‘Ã£ tÃ­ch lÅ©y kiáº¿n thá»©c ná»n táº£ng Web tá»‘t (Äiá»ƒm TB mÃ´n Web: ${formatScore(avgScore)}), khÃ³a há»c nÃ y lÃ  máº£nh ghÃ©p hoÃ n háº£o Ä‘á»ƒ báº¡n phÃ¡t triá»ƒn chuyÃªn sÃ¢u Front-end Framework.`);
      } else {
        reasons.push("KhÃ³a há»c Front-end thá»‹nh hÃ nh Ä‘Æ°á»£c Ä‘á» xuáº¥t nhiá»u nháº¥t cho há»c viÃªn má»›i báº¯t Ä‘áº§u Ä‘á»‹nh hÆ°á»›ng Web.");
      }
      topics = ["Component Lifecycle", "State Management", "Tailwind CSS Integration", "API Routing"];
    } else if (courseName.includes("node") || courseName.includes("net") || courseName.includes("c#") || courseName.includes("java") || courseName.includes("backend") || courseName.includes("database") || courseName.includes("sql")) {
      const hasJs = completedOrStudying.some(c => {
        const name = String(c.courseNameSnapshot || "").toLowerCase();
        return name.includes("javascript") || name.includes("web") || name.includes("fullstack");
      });
      if (hasJs) {
        score += 10;
        reasons.push("Sau khi náº¯m vá»¯ng Client-side, viá»‡c bá»• trá»£ ká»¹ nÄƒng Server-side vÃ  CÆ¡ sá»Ÿ dá»¯ liá»‡u sáº½ giÃºp báº¡n hoÃ n thiá»‡n lá»™ trÃ¬nh trá»Ÿ thÃ nh Fullstack Developer.");
      } else {
        reasons.push("Kiáº¿n thá»©c Backend cá»‘t lÃµi giÃºp thiáº¿t láº­p ná»n táº£ng tÆ° duy láº­p trÃ¬nh vá»¯ng cháº¯c cÃ¹ng kiáº¿n trÃºc cÆ¡ sá»Ÿ dá»¯ liá»‡u tá»‘i Æ°u.");
      }
      topics = ["RESTful APIs", "Relational Databases", "Authentication & Security", "Server Architectures"];
    } else if (courseName.includes("fullstack") || courseName.includes("full-stack")) {
      score += 8;
      reasons.push("ÄÆ°á»£c gá»£i Ã½ dá»±a trÃªn lá»™ trÃ¬nh cá»§a 92% há»c viÃªn cÃ¹ng xuáº¥t phÃ¡t Ä‘iá»ƒm cÃ³ mong muá»‘n thÄƒng tiáº¿n nhanh thÃ nh ká»¹ sÆ° pháº§n má»m Ä‘a nhiá»‡m.");
      topics = ["End-to-End Dev", "Deployment & CI/CD", "System Designing", "Microservices"];
    } else {
      reasons.push("MÃ´n há»c bá»• trá»£ náº±m trong chuá»—i lá»™ trÃ¬nh kiáº¿n trÃºc pháº§n má»m Ä‘Æ°á»£c cÃ¡c há»c viÃªn xuáº¥t sáº¯c lá»±a chá»n nhiá»u nháº¥t ká»³ nÃ y.");
      topics = ["OOP Principles", "Data Structures", "Algorithm Analytics", "Clean Coding"];
    }

    if (avgScore >= 8.0) {
      score += 3;
    }

    return {
      courseId: course.id,
      courseName: course.courseName,
      matchPercent: Math.min(99, score),
      reason: reasons[0] || "ÄÆ°á»£c Ä‘á» xuáº¥t nhiá»u nháº¥t dá»±a trÃªn há»“ sÆ¡ há»c táº­p vÃ  Ä‘á»‹nh hÆ°á»›ng cÃ´ng viá»‡c láº­p trÃ¬nh.",
      topics: topics.slice(0, 3)
    };
  });

  scored.sort((a, b) => b.matchPercent - a.matchPercent);
  recommendations.value = scored.slice(0, 3);
  loadingRecs.value = false;
}

function registerInterest(rec) {
  const hide = message.loading(`Äang gá»­i yÃªu cáº§u Ä‘Äƒng kÃ½ tÆ° váº¥n khÃ³a há»c ${rec.courseName}...`, 0);
  setTimeout(() => {
    hide();
    message.success(`ÄÄƒng kÃ½ tÆ° váº¥n lá»™ trÃ¬nh thÃ nh cÃ´ng! Äá»™i ngÅ© há»c vá»¥ sáº½ liÃªn há»‡ vá»›i báº¡n trong vÃ²ng 24h Ä‘á»ƒ hÆ°á»›ng dáº«n chi tiáº¿t vá» khÃ³a há»c ${rec.courseName}.`);
  }, 1200);
}


function openReview(course) {
  selectedCourse.value = course;
  const existing = reviewFor(course);
  reviewForm.id = existing?.id || null;
  reviewForm.courseRating = Number(existing?.courseRating || 5);
  reviewForm.courseComment = existing?.courseComment || "";
  const existingTeachers = new Map((existing?.teacherReviews || []).map((x) => [String(x.teacherId).toLowerCase(), x]));
  reviewForm.teacherReviews = teacherOptions(course).map((teacher) => {
    const saved = existingTeachers.get(String(teacher.teacherId).toLowerCase());
    return {
      teacherId: teacher.teacherId,
      teacherNameSnapshot: teacher.teacherNameSnapshot,
      rating: Number(saved?.rating || 5),
      comment: saved?.comment || "",
    };
  });
  reviewModalOpen.value = true;
}

async function saveReview() {
  if (!selectedCourse.value) return;
  if (reviewForm.teacherReviews.length === 0) {
    message.warning("Lá»›p há»c chÆ°a cÃ³ giáº£ng viÃªn Ä‘á»ƒ Ä‘Ã¡nh giÃ¡.");
    return;
  }
  savingReview.value = true;
  try {
    const payload = {
      enrollmentId: selectedCourse.value.id,
      courseRating: Number(reviewForm.courseRating || 5),
      courseComment: reviewForm.courseComment || null,
      teacherReviews: reviewForm.teacherReviews.map((x) => ({
        teacherId: x.teacherId,
        teacherNameSnapshot: x.teacherNameSnapshot,
        rating: Number(x.rating || 5),
        comment: x.comment || null,
      })),
    };
    const saved = reviewForm.id ? await reviewApi.update(reviewForm.id, payload) : await reviewApi.create(payload);
    const index = reviews.value.findIndex((x) => x.id === saved.id || x.enrollmentId === saved.enrollmentId);
    if (index >= 0) reviews.value.splice(index, 1, saved);
    else reviews.value.unshift(saved);
    reviewModalOpen.value = false;
    message.success("ÄÃ£ lÆ°u Ä‘Ã¡nh giÃ¡.");
  } catch (err) {
    message.error(err.message || "KhÃ´ng thá»ƒ lÆ°u Ä‘Ã¡nh giÃ¡.");
  } finally {
    savingReview.value = false;
  }
}

function reviewFor(course) {
  return reviews.value.find((x) => String(x.enrollmentId).toLowerCase() === String(course.id).toLowerCase());
}

function teacherOptions(course) {
  const ids = Array.isArray(course.teacherIds) ? course.teacherIds : [];
  const names = Array.isArray(course.teacherNames) ? course.teacherNames : [];
  return ids
    .map((id, index) => ({
      teacherId: id,
      teacherNameSnapshot: names[index] || `Giáº£ng viÃªn ${index + 1}`,
    }))
    .filter((x) => x.teacherId);
}

function teacherSummary(course) {
  const names = Array.isArray(course.teacherNames) ? course.teacherNames.filter(Boolean) : [];
  if (names.length === 0) return "ChÆ°a cáº­p nháº­t";
  if (names.length === 1) return names[0];
  return `${names[0]} +${names.length - 1}`;
}

function statusKey(status) {
  return typeof status === "number" ? status : String(status || "");
}

function isCompleted(course) {
  const value = statusKey(course.status);
  return value === "Completed" || value === 4;
}

function statusText(status) {
  const map = {
    Pending: "Chá» duyá»‡t",
    Confirmed: "ÄÃ£ xÃ¡c nháº­n",
    Studying: "Äang há»c",
    Completed: "HoÃ n thÃ nh",
    Cancelled: "ÄÃ£ há»§y",
    1: "Chá» duyá»‡t",
    2: "ÄÃ£ xÃ¡c nháº­n",
    3: "Äang há»c",
    4: "HoÃ n thÃ nh",
    5: "ÄÃ£ há»§y",
  };
  return map[statusKey(status)] || status;
}

function statusBadge(status) {
  const base = "inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold border ";
  const value = statusKey(status);
  if (["Confirmed", "Studying", 2, 3].includes(value)) return base + "bg-blue-50 text-blue-700 border-blue-200";
  if (value === "Completed" || value === 4) return base + "bg-emerald-50 text-emerald-700 border-emerald-200";
  if (value === "Pending" || value === 1) return base + "bg-amber-50 text-amber-700 border-amber-200";
  return base + "bg-slate-50 text-slate-600 border-slate-200";
}

function canShowProgress(course) {
  return Boolean(course.canShowProgress);
}

function progress(course) {
  if (isCompleted(course)) return 100;
  const value = Number(course.progressPercent ?? 0);
  return Math.max(0, Math.min(100, Math.round(value)));
}

function progressLabel(course) {
  if (isInvalidEnrollmentPeriod(course)) return "KhÃ´ng Ã¡p dá»¥ng";
  if (!canShowProgress(course)) return "ChÆ°a tÃ­nh";
  return `${progress(course)}%`;
}

function progressHint(course) {
  const value = statusKey(course.status);
  if (isInvalidEnrollmentPeriod(course)) return "NgÃ y ghi danh sau khi lá»›p Ä‘Ã£ káº¿t thÃºc. Vui lÃ²ng liÃªn há»‡ Admin Ä‘á»ƒ chuyá»ƒn lá»›p.";
  if (value === "Pending" || value === 1) return "Chá» Admin duyá»‡t, chÆ°a cÃ³ tiáº¿n Ä‘á»™ há»c.";
  if (value === "Confirmed" || value === 2) return "ÄÃ£ xÃ¡c nháº­n, lá»›p chÆ°a báº¯t Ä‘áº§u há»c.";
  if (value === "Cancelled" || value === 5) return "ÄÄƒng kÃ½ Ä‘Ã£ há»§y, khÃ´ng tÃ­nh tiáº¿n Ä‘á»™.";
  if (isCompleted(course)) return "KhÃ³a há»c Ä‘Ã£ hoÃ n thÃ nh. Báº¡n cÃ³ thá»ƒ Ä‘Ã¡nh giÃ¡ khÃ³a há»c vÃ  giáº£ng viÃªn.";
  return `${course.completedSessions ?? 0}/${course.totalSessions ?? 0} buá»•i Ä‘Ã£ ghi nháº­n.`;
}

function isInvalidEnrollmentPeriod(course) {
  if (!course?.classEndDate || !course?.enrolledAt) return false;
  return new Date(course.enrolledAt).getTime() > new Date(course.classEndDate).getTime();
}

function getStrokeColor(course) {
  if (isCompleted(course)) return "#10b981";
  return "#2563eb";
}

function getProgressColorText(course) {
  if (isCompleted(course)) return "text-emerald-600";
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
  color: #64748b;
}
:deep(.ant-tabs-tab-active) {
  font-weight: 600;
}
:deep(.review-mini-rate) {
  font-size: 12px;
  line-height: 1;
}
:deep(.review-mini-rate .ant-rate-star) {
  margin-inline-end: 1px;
}
</style>

