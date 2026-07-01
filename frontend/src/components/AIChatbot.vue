<template>
  <div class="ai-chatbot-wrapper select-none">
    <!-- Floating Toggle Button -->
    <button
      @click="toggleChat"
      class="floating-chatbot-btn cursor-pointer"
      :class="roleTheme.btnClass"
      aria-label="Toggle AI Assistant"
    >
      <span class="pulse-glow" :class="roleTheme.pulseClass"></span>
      <svg
        v-if="!isOpen"
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
        stroke-width="2"
        stroke="currentColor"
        class="w-6 h-6 animate-bounce-slow"
      >
        <path stroke-linecap="round" stroke-linejoin="round" d="M9.813 15.904L9 21l3.75-1.5 3.75 1.5-.813-5.096A7.5 7.5 0 109.813 15.904z" />
        <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 10.5c0 1.25-.37 2.413-1.007 3.388M4.5 10.5c0 1.25.37 2.413 1.007 3.388" />
      </svg>
      <svg
        v-else
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
        stroke-width="2"
        stroke="currentColor"
        class="w-5 h-5"
      >
        <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
      </svg>
    </button>

    <!-- Chat Dialog Window -->
    <transition name="chat-fade">
      <div v-show="isOpen" class="chat-dialog-window bg-card-base border border-base shadow-2xl flex flex-col backdrop-blur-xl">
        <!-- Header -->
        <header class="chat-dialog-header text-white flex items-center justify-between px-4 py-3 shrink-0" :class="roleTheme.headerClass">
          <div class="flex items-center gap-2">
            <div class="w-8 h-8 rounded-full bg-white/20 flex items-center justify-center font-bold text-xs">AI</div>
            <div>
              <h3 class="text-sm font-bold m-0 leading-none">{{ roleTheme.title }}</h3>
              <span class="text-[10px] opacity-75 font-medium block mt-0.5 flex items-center gap-1">
                <span class="inline-block w-1.5 h-1.5 rounded-full bg-green-300 animate-pulse"></span>
                GPT qua AI Router - Trực tuyến
              </span>
            </div>
          </div>
          <div class="flex items-center gap-1.5">
            <button @click="clearHistory" class="p-1 hover:bg-white/10 rounded transition-colors text-white shrink-0 cursor-pointer" title="Xóa lịch sử chat">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" class="w-4 h-4">
                <path stroke-linecap="round" stroke-linejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
              </svg>
            </button>
            <button @click="toggleChat" class="p-1 hover:bg-white/10 rounded transition-colors text-white shrink-0 cursor-pointer" title="Đóng cửa sổ">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" class="w-4 h-4">
                <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 8.25l-7.5 7.5-7.5-7.5" />
              </svg>
            </button>
          </div>
        </header>

        <!-- Message Body List -->
        <div ref="messageContainer" class="flex-grow p-4 overflow-y-auto space-y-3.5 custom-scrollbar">
          <div
            v-for="(msg, index) in messages"
            :key="index"
            class="flex items-end gap-2"
            :class="msg.role === 'user' ? 'justify-end' : 'justify-start'"
          >
            <!-- System/AI Avatar -->
            <div v-if="msg.role !== 'user'" class="w-7 h-7 rounded-full flex items-center justify-center shrink-0 text-xs text-white" :class="roleTheme.avatarBg">
              AI
            </div>

            <!-- Message Bubble -->
            <div
              class="message-bubble rounded-2xl px-3 py-2 text-xs leading-relaxed max-w-[80%] border"
              :class="msg.role === 'user' ? roleTheme.userBubbleClass : 'bg-slate-50 dark:bg-slate-800/40 border-slate-200/50 dark:border-slate-800/50 text-base-primary'"
            >
              <div class="select-text prose dark:prose-invert max-w-none text-xs" v-html="parseMarkdown(msg.text)"></div>
              <span class="text-[9px] opacity-50 block text-right mt-1 font-medium">
                {{ formatTime(msg.timestamp) }}
              </span>
            </div>

            <!-- User Initial Avatar -->
            <div v-if="msg.role === 'user'" class="w-7 h-7 rounded-full bg-slate-200 dark:bg-slate-700 flex items-center justify-center shrink-0 text-[10px] font-black text-slate-600 dark:text-slate-300">
              {{ userInitials }}
            </div>
          </div>

          <!-- Streaming partial response -->
          <div v-if="streamingText" class="flex items-end gap-2 justify-start">
            <div class="w-7 h-7 rounded-full flex items-center justify-center shrink-0 text-xs text-white" :class="roleTheme.avatarBg">
              AI
            </div>
            <div class="message-bubble rounded-2xl px-3 py-2 text-xs leading-relaxed max-w-[80%] border bg-slate-50 dark:bg-slate-800/40 border-slate-200/50 dark:border-slate-800/50 text-base-primary">
              <div class="select-text prose dark:prose-invert max-w-none text-xs" v-html="parseMarkdown(streamingText)"></div>
              <span class="streaming-cursor">|</span>
            </div>
          </div>

          <!-- Typing Bouncing Bobs (while waiting for first chunk) -->
          <div v-else-if="isTyping" class="flex items-end gap-2 justify-start">
            <div class="w-7 h-7 rounded-full flex items-center justify-center shrink-0 text-xs text-white" :class="roleTheme.avatarBg">
              AI
            </div>
            <div class="bg-slate-50 dark:bg-slate-800/40 border border-slate-200/50 dark:border-slate-800/50 rounded-2xl px-4 py-3 flex items-center gap-1">
              <span class="w-1.5 h-1.5 rounded-full bg-slate-400 dark:bg-slate-500 animate-bounce"></span>
              <span class="w-1.5 h-1.5 rounded-full bg-slate-400 dark:bg-slate-500 animate-bounce" style="animation-delay:0.15s"></span>
              <span class="w-1.5 h-1.5 rounded-full bg-slate-400 dark:bg-slate-500 animate-bounce" style="animation-delay:0.3s"></span>
            </div>
          </div>
        </div>

        <!-- Quick Today Schedule Query Helper -->
        <div v-if="todayScheduleContext" class="px-4 py-1.5 border-t border-slate-200/40 dark:border-slate-800/20 shrink-0">
          <button
            @click="queryTodaySchedule"
            class="w-full text-left inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl border border-dashed text-xs font-semibold hover:opacity-90 active:scale-95 transition-all duration-200 cursor-pointer"
            :class="roleTheme.quickQueryClass"
          >
            Tra lịch học/dạy hôm nay của tôi
          </button>
        </div>

        <!-- Message Input Footer -->
        <footer class="p-3 border-t border-base shrink-0 bg-slate-50/50 dark:bg-slate-900/10">
          <form @submit.prevent="sendMessage" class="flex items-center gap-2">
            <input
              v-model="inputValue"
              type="text"
              placeholder="Nhập tin nhắn..."
              class="flex-grow px-3 py-2 text-xs rounded-xl border border-base bg-card-base text-base-primary placeholder:text-base-muted focus:outline-none focus:ring-1 focus:ring-blue-500 transition-all"
              :disabled="isTyping || !!streamingText"
              ref="chatInput"
            />
            <button
              type="submit"
              class="w-8 h-8 rounded-xl flex items-center justify-center text-white cursor-pointer active:scale-90 transition-transform shrink-0 disabled:opacity-50"
              :class="roleTheme.btnClass"
              :disabled="!inputValue.trim() || isTyping || !!streamingText"
            >
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2.5" stroke="currentColor" class="w-4 h-4">
                <path stroke-linecap="round" stroke-linejoin="round" d="M6 12L3.269 3.126A59.768 59.768 0 0121.485 12 59.77 59.77 0 013.27 20.876L5.999 12zm0 0h7.5" />
              </svg>
            </button>
          </form>
        </footer>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { computed, nextTick, onMounted, ref } from 'vue'
import dayjs from 'dayjs'
import { useAuthStore } from '@/stores/auth'
import { classApi } from '@/api/classApi'
import { scheduleApi } from '@/api/scheduleApi'
import { studentApi } from '@/api/studentApi'
import { DAY_OF_WEEK_VN } from '@/lib/constants'
import { formatTime as formatterTime } from '@/lib/formatters'
import { callApi, http } from '@/api/httpClient'

const props = defineProps({
  role: {
    type: String,
    required: true,
    validator: (v) => ['admin', 'teacher', 'student'].includes(v)
  }
})

const authStore = useAuthStore()
const isOpen = ref(false)
const isTyping = ref(false)
const streamingText = ref('')   // holds partial streaming response
const inputValue = ref('')
const messages = ref([])
const todayScheduleContext = ref('')
const weekScheduleItems = ref([])
const managedClassItems = ref([])

const messageContainer = ref(null)
const chatInput = ref(null)

const localKey = computed(() => `educenter_ai_messages_${props.role}`)

// Dynamic theme configuration based on role
const themeConfigs = {
  admin: {
    title: 'Trợ lý AI Quản trị',
    btnClass: 'bg-blue-600 hover:bg-blue-700 text-white',
    headerClass: 'bg-gradient-to-r from-blue-600 to-blue-700',
    pulseClass: 'bg-blue-500',
    avatarBg: 'bg-blue-600',
    userBubbleClass: 'bg-blue-600 text-white border-blue-500',
    quickQueryClass: 'border-blue-300 dark:border-blue-800 text-blue-600 dark:text-blue-400 bg-blue-50/50 dark:bg-blue-950/10',
    welcome: 'Xin chào Quản trị viên! Tôi là Trợ lý AI đặc biệt của EduCenter, sử dụng **GPT qua AI Router**.\n\nTôi có thể giúp bạn:\n- Phân tích thống kê, doanh thu\n- Tra lịch học hôm nay\n- Tư vấn duyệt đề xuất dạy thay\n- Soạn thông báo cho học viên\n\nHãy hỏi tôi bất cứ điều gì!'
  },
  teacher: {
    title: 'Trợ lý AI Giảng viên',
    btnClass: 'bg-purple-600 hover:bg-purple-700 text-white',
    headerClass: 'bg-gradient-to-r from-purple-600 to-purple-700',
    pulseClass: 'bg-purple-500',
    avatarBg: 'bg-purple-600',
    userBubbleClass: 'bg-purple-600 text-white border-purple-500',
    quickQueryClass: 'border-purple-300 dark:border-purple-800 text-purple-600 dark:text-purple-400 bg-purple-50/50 dark:bg-purple-950/10',
    welcome: 'Chào Thầy/Cô! Tôi là Trợ lý AI đồng hành giảng dạy, sử dụng **GPT qua AI Router**.\n\nTôi có thể hỗ trợ:\n- Soạn giáo án, ngân hàng câu hỏi\n- Viết nhận xét học viên\n- Gợi ý cách giảng dạy hiệu quả\n- Giải thích kỹ thuật lập trình\n\nHãy hỏi tôi bất cứ điều gì!'
  },
  student: {
    title: 'Trợ lý AI Học tập',
    btnClass: 'bg-emerald-600 hover:bg-emerald-700 text-white',
    headerClass: 'bg-gradient-to-r from-emerald-600 to-teal-600',
    pulseClass: 'bg-emerald-500',
    avatarBg: 'bg-emerald-600',
    userBubbleClass: 'bg-emerald-600 text-white border-emerald-500',
    quickQueryClass: 'border-emerald-300 dark:border-emerald-800 text-emerald-600 dark:text-emerald-400 bg-emerald-50/50 dark:bg-emerald-950/10',
    welcome: 'Chào bạn! Tôi là Trợ lý học tập thông minh EduCenter, sử dụng **GPT qua AI Router**.\n\nTôi có thể:\n- Debug code, giải thích lỗi\n- Giải thích khái niệm (React, .NET, SQL...)\n- Tư vấn lộ trình học tập\n- Xem lịch học hôm nay\n\nHãy hỏi tôi bất cứ điều gì!'
  }
}

const roleTheme = computed(() => themeConfigs[props.role] || themeConfigs.student)

const userInitials = computed(() => {
  const name = authStore.user?.fullName || authStore.user?.username || 'User'
  const parts = name.trim().split(/\s+/)
  return parts.slice(-2).map(x => x[0]).join('').toUpperCase()
})

// System instruction prompt context for AI Router API
const systemInstructions = {
  admin: () => `Bạn là trợ lý AI dành cho Quản trị viên (Admin) của trung tâm EduCenter. Hãy giúp Admin trong các tác vụ quản lý khóa học, lớp học, lịch học, giảng viên, học viên, học phí, doanh thu, phân tích dữ liệu lớp học, soạn thảo các thông báo/email chung cho học viên, nhắc nợ, cảnh báo học tập hoặc gợi ý giảng viên dạy thay.
${todayScheduleContext.value ? `\n[Dữ liệu quan trọng] Dưới đây là thông tin các lớp học diễn ra tại trung tâm hôm nay để bạn hỗ trợ tra cứu: ${todayScheduleContext.value}` : ''}
Hãy hội thoại tự nhiên như một trợ lý AI hiện đại. Trả lời được lời chào, câu hỏi chung, giải thích khái niệm, gợi ý cách làm và soạn thảo nội dung. Với dữ liệu nội bộ EduCenter mà bạn không được cung cấp trong prompt, không tự bịa số liệu; hãy nói rõ cần mở đúng màn hình hoặc cần hệ thống tra cứu thêm. Trả lời bằng tiếng Việt, ngắn gọn, có Markdown khi hữu ích.`,

  teacher: () => `Bạn là trợ lý AI dành cho Giảng viên (Teacher) của trung tâm EduCenter. Hãy giúp Giảng viên soạn giáo án, đề xuất câu hỏi ôn tập, viết nhận xét học tập cho học viên, tư vấn cách truyền đạt kiến thức các công nghệ (ReactJS, VueJS, SQL, ASP.NET,...) hoặc gợi ý cách xử lý các buổi học khó.
Lưu ý bảo mật: Bạn tuyệt đối không được tiết lộ thông tin lương thưởng hay tài chính của trung tâm cho giảng viên.
${todayScheduleContext.value ? `\n[Dữ liệu quan trọng] Dưới đây là lịch dạy học của Giảng viên này hôm nay để bạn hỗ trợ trả lời: ${todayScheduleContext.value}` : ''}
Hãy hội thoại tự nhiên như một trợ lý AI hiện đại. Trả lời được lời chào, câu hỏi chung, giải thích kỹ thuật, gợi ý bài giảng, soạn giáo án, viết nhận xét và hỗ trợ lập trình. Với dữ liệu nội bộ EduCenter mà bạn không được cung cấp trong prompt, không tự bịa; hãy nói rõ giới hạn. Trả lời bằng tiếng Việt, ngắn gọn, có Markdown khi hữu ích.`,

  student: () => `Bạn là trợ lý AI đồng hành học tập của Học viên (Student) tại trung tâm EduCenter. Hãy giúp học viên giải thích các khái niệm lập trình (React, Vue, C#, SQL,...), hướng dẫn sửa lỗi code (debug), lên kế hoạch học tập cá nhân, gợi ý tài liệu học thêm, giải đáp bài tập hoặc tư vấn lộ trình học tập để trở thành lập trình viên Full-Stack.
Lưu ý bảo mật: Bạn không có quyền truy cập vào bảng điểm nội bộ hay thông tin quản trị trung tâm, không tiết lộ tài chính hay hệ thống dữ liệu khác.
${todayScheduleContext.value ? `\n[Dữ liệu quan trọng] Dưới đây là lịch học hôm nay của học viên này để bạn hỗ trợ trả lời khi học viên hỏi: ${todayScheduleContext.value}` : ''}
Hãy hội thoại tự nhiên như một trợ lý AI hiện đại. Trả lời được lời chào, câu hỏi chung, giải thích kỹ thuật, debug code, gợi ý lộ trình học và tài liệu. Với dữ liệu nội bộ EduCenter mà bạn không được cung cấp trong prompt, không tự bịa; hãy nói rõ giới hạn. Trả lời bằng tiếng Việt, thân thiện, dễ hiểu, có Markdown khi hữu ích.`
}

const activeSystemInstruction = computed(() => {
  const handler = systemInstructions[props.role]
  return handler ? handler() : systemInstructions.student()
})

// Load messages and initialize schedules
onMounted(() => {
  loadMessages()
  loadTodaySchedules()
})

function loadMessages() {
  const saved = localStorage.getItem(localKey.value)
  if (saved) {
    try { messages.value = JSON.parse(saved) } catch { messages.value = [] }
  }
  if (messages.value.length === 0) {
    messages.value.push({
      role: 'model',
      text: roleTheme.value.welcome,
      timestamp: new Date().toISOString()
    })
  }
}

async function loadTodaySchedules() {
  const referenceId = authStore.user?.referenceId
  if (!referenceId) return

  const todayNum = dayjs().day()
  const todayMap = { 0: 'Sunday', 1: 'Monday', 2: 'Tuesday', 3: 'Wednesday', 4: 'Thursday', 5: 'Friday', 6: 'Saturday' }
  const todayEng = todayMap[todayNum]

  try {
    if (props.role === 'student') {
      const myCourses = await studentApi.getMyCourses(referenceId)
      managedClassItems.value = myCourses || []
      const classIds = [...new Set(myCourses.map((x) => x.classId).filter(Boolean))]
      const groups = await Promise.all(classIds.map((id) => scheduleApi.getByClass(id).catch(() => [])))
      weekScheduleItems.value = groups.flat()
      const todayClasses = weekScheduleItems.value.filter(s => s.dayOfWeek === todayEng)
      if (todayClasses.length > 0) {
        todayScheduleContext.value = todayClasses.map(c =>
          `- Lớp: ${c.classNameSnapshot || 'Lớp học'}, Phòng: ${c.room || 'Chưa xếp'}, Thời gian: ${formatterTime(c.startTime)} - ${formatterTime(c.endTime)}, Giảng viên: ${c.teacherName || 'Chưa xếp'}`
        ).join('\n')
      }
    } else if (props.role === 'teacher') {
      const [classesRes, schedulesRes] = await Promise.all([
        classApi.getByTeacher(referenceId).catch(() => []),
        scheduleApi.getByTeacher(referenceId)
      ])
      managedClassItems.value = classesRes || []
      weekScheduleItems.value = schedulesRes || []
      const todayClasses = weekScheduleItems.value.filter(s => s.dayOfWeek === todayEng)
      if (todayClasses.length > 0) {
        todayScheduleContext.value = todayClasses.map(c =>
          `- Dạy lớp: ${c.classNameSnapshot || 'Lớp học'}, Phòng: ${c.room || 'Chưa xếp'}, Thời gian: ${formatterTime(c.startTime)} - ${formatterTime(c.endTime)}`
        ).join('\n')
      }
    } else if (props.role === 'admin') {
      const schedulesRes = await scheduleApi.getAll()
      weekScheduleItems.value = schedulesRes || []
      const todayClasses = weekScheduleItems.value.filter(s => s.dayOfWeek === todayEng)
      if (todayClasses.length > 0) {
        todayScheduleContext.value = `Hôm nay trung tâm có ${todayClasses.length} lớp học diễn ra:\n` + todayClasses.map(c =>
          `- Lớp: ${c.classNameSnapshot || 'Lớp học'}, Phòng: ${c.room || 'Chưa xếp'}, Giờ: ${formatterTime(c.startTime)} - ${formatterTime(c.endTime)}, GV: ${c.teacherName || 'Chưa xếp'}`
        ).join('\n')
      }
    }
  } catch (e) {
    console.warn('[AIChatbot] Failed to load schedule context:', e)
  }
}

function isScheduleQuestion(text) {
  const normalized = text.toLowerCase()
  return /(lịch|lich|schedule|thời khóa biểu|thoi khoa bieu)/i.test(normalized)
}

function isManagedClassesQuestion(text) {
  const normalized = text.toLowerCase()
  return /(lớp|lop|class).*(quản lý|quan ly|phụ trách|phu trach|đang dạy|dang day|dạy|day)|quản lý.*(lớp|lop)|phụ trách.*(lớp|lop)/i.test(normalized)
}

function fallbackScheduleIntent(text) {
  const normalized = text.toLowerCase()
  if (isManagedClassesQuestion(text)) return { intent: 'managed_classes_lookup', confidence: 0.65 }
  if (!isScheduleQuestion(text)) return { intent: 'other', confidence: 0 }
  if (/(tuần sau|tuan sau|tuần tới|tuan toi|next week)/i.test(normalized)) return { intent: 'schedule_lookup', rangeType: 'week', weekOffset: 1, confidence: 0.7 }
  if (/(tuần này|tuan nay|trong tuần|trong tuan|weekly|this week)/i.test(normalized)) return { intent: 'schedule_lookup', rangeType: 'week', weekOffset: 0, confidence: 0.7 }
  if (/(ngày mai|ngay mai|mai|tomorrow)/i.test(normalized)) return { intent: 'schedule_lookup', rangeType: 'day', dayOffset: 1, confidence: 0.7 }
  if (/(hôm nay|hom nay|today)/i.test(normalized)) return { intent: 'schedule_lookup', rangeType: 'day', dayOffset: 0, confidence: 0.7 }
  return { intent: 'schedule_lookup', rangeType: 'day', dayOffset: 0, confidence: 0.55 }
}

function parseJsonObject(text) {
  if (!text) return null
  try {
    return JSON.parse(text)
  } catch {
    const match = String(text).match(/\{[\s\S]*\}/)
    if (!match) return null
    try {
      return JSON.parse(match[0])
    } catch {
      return null
    }
  }
}

async function classifyEduIntent(text) {
  try {
    const today = dayjs().format('YYYY-MM-DD')
    const result = await callApi(http.post('/gateway/ai/knowledge/complete', {
      jsonMode: true,
      maxOutputTokens: 300,
      system: `Bạn là bộ phân loại ý định cho chatbot EduCenter. Chỉ trả về JSON hợp lệ, không markdown.
Ngày hiện tại: ${today}.
Schema:
{
  "intent": "schedule_lookup" | "managed_classes_lookup" | "other",
  "rangeType": "day" | "week" | null,
  "weekOffset": number | null,
  "dayOffset": number | null,
  "dayOfWeek": number | null,
  "date": "YYYY-MM-DD" | null,
  "confidence": number
}
Quy ước dayOfWeek: 0=Chủ nhật, 1=Thứ hai, ..., 6=Thứ bảy.
Ví dụ:
"tuần sau có lịch không" -> {"intent":"schedule_lookup","rangeType":"week","weekOffset":1,"dayOffset":null,"dayOfWeek":null,"date":null,"confidence":0.95}
"thứ 5 tuần sau tôi có dạy không" -> {"intent":"schedule_lookup","rangeType":"day","weekOffset":1,"dayOffset":null,"dayOfWeek":4,"date":null,"confidence":0.95}
"mai có ca nào không" -> {"intent":"schedule_lookup","rangeType":"day","weekOffset":null,"dayOffset":1,"dayOfWeek":null,"date":null,"confidence":0.9}
"tôi đang quản lý những lớp nào" -> {"intent":"managed_classes_lookup","rangeType":null,"weekOffset":null,"dayOffset":null,"dayOfWeek":null,"date":null,"confidence":0.95}
"các lớp tôi phụ trách" -> {"intent":"managed_classes_lookup","rangeType":null,"weekOffset":null,"dayOffset":null,"dayOfWeek":null,"date":null,"confidence":0.95}
Nếu câu hỏi không liên quan lịch học/lịch dạy/thời khóa biểu hoặc danh sách lớp phụ trách, intent là "other".`,
      prompt: text
    }))
    const parsed = parseJsonObject(result?.text)
    if (!parsed) return fallbackScheduleIntent(text)
    if (parsed.intent === 'managed_classes_lookup') {
      return { intent: 'managed_classes_lookup', confidence: Number(parsed.confidence) || 0 }
    }
    if (parsed.intent !== 'schedule_lookup') return { intent: 'other', confidence: parsed?.confidence || 0 }
    return {
      intent: 'schedule_lookup',
      rangeType: parsed.rangeType === 'week' ? 'week' : 'day',
      weekOffset: Number.isFinite(Number(parsed.weekOffset)) ? Number(parsed.weekOffset) : null,
      dayOffset: Number.isFinite(Number(parsed.dayOffset)) ? Number(parsed.dayOffset) : null,
      dayOfWeek: Number.isFinite(Number(parsed.dayOfWeek)) ? Number(parsed.dayOfWeek) : null,
      date: typeof parsed.date === 'string' && parsed.date ? parsed.date : null,
      confidence: Number(parsed.confidence) || 0
    }
  } catch (error) {
    console.warn('[AIChatbot] Intent classification failed:', error)
    return fallbackScheduleIntent(text)
  }
}

function scheduleDayNumber(dayOfWeek) {
  const dayMap = { Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3, Thursday: 4, Friday: 5, Saturday: 6 }
  const dayNumber = typeof dayOfWeek === 'number' ? dayOfWeek : dayMap[dayOfWeek] ?? Number(dayOfWeek)
  return Number.isFinite(dayNumber) ? dayNumber : null
}

function scheduleDateForWeek(dayOfWeek, weekOffset = 0) {
  const dayNumber = scheduleDayNumber(dayOfWeek)
  if (dayNumber === null) return null
  const monday = dayjs().startOf('isoWeek').add(weekOffset, 'week')
  return monday.add(dayNumber === 0 ? 6 : dayNumber - 1, 'day')
}

function scheduleLine(item, weekOffset = 0) {
  const date = scheduleDateForWeek(item.dayOfWeek, weekOffset)
  const label = date ? `${DAY_OF_WEEK_VN[date.day()]} ${date.format('DD/MM/YYYY')}` : (item.dayOfWeek || 'Chưa rõ ngày')
  const className = item.classNameSnapshot || item.className || 'Lớp học'
  const room = item.room || 'Chưa xếp'
  return `- ${label}: ${className}, ${formatterTime(item.startTime)} - ${formatterTime(item.endTime)}, phòng ${room}`
}

function scheduleLineForDate(item, date) {
  const label = `${DAY_OF_WEEK_VN[date.day()]} ${date.format('DD/MM/YYYY')}`
  const className = item.classNameSnapshot || item.className || 'Lớp học'
  const room = item.room || 'Chưa xếp'
  return `- ${label}: ${className}, ${formatterTime(item.startTime)} - ${formatterTime(item.endTime)}, phòng ${room}`
}

function sortSchedulesForWeek(items, weekOffset = 0) {
  return items.sort((a, b) => {
    const da = scheduleDateForWeek(a.dayOfWeek, weekOffset)?.valueOf() || 0
    const db = scheduleDateForWeek(b.dayOfWeek, weekOffset)?.valueOf() || 0
    return da - db || String(a.startTime || '').localeCompare(String(b.startTime || ''))
  })
}

function answerScheduleFromLocalData(text, intent = null) {
  const parsedIntent = intent?.intent === 'schedule_lookup' ? intent : fallbackScheduleIntent(text)
  if (parsedIntent.intent !== 'schedule_lookup') return ''

  const items = [...weekScheduleItems.value]
  if (items.length === 0) {
    return props.role === 'teacher'
      ? 'Hiện chưa có dữ liệu lịch dạy của Thầy/Cô trong hệ thống.'
      : 'Hiện chưa có dữ liệu lịch học trong hệ thống.'
  }

  if (parsedIntent.rangeType === 'week') {
    const weekOffset = Number.isFinite(Number(parsedIntent.weekOffset)) ? Number(parsedIntent.weekOffset) : 0
    const weekStart = dayjs().startOf('isoWeek').add(weekOffset, 'week')
    const start = weekStart.format('DD/MM/YYYY')
    const end = weekStart.endOf('isoWeek').format('DD/MM/YYYY')
    const label = weekOffset === 0 ? 'tuần này' : weekOffset === 1 ? 'tuần sau' : `tuần ${start} - ${end}`
    const title = props.role === 'teacher'
      ? `Lịch dạy ${label} (${start} - ${end}) của Thầy/Cô:`
      : `Lịch ${label} (${start} - ${end}) của bạn:`
    return `${title}\n${sortSchedulesForWeek(items, weekOffset).map(item => scheduleLine(item, weekOffset)).join('\n')}`
  }

  let targetDate = null
  if (parsedIntent.date) {
    const explicitDate = dayjs(parsedIntent.date)
    if (explicitDate.isValid()) targetDate = explicitDate
  }
  if (!targetDate && Number.isFinite(Number(parsedIntent.dayOfWeek))) {
    const weekOffset = Number.isFinite(Number(parsedIntent.weekOffset)) ? Number(parsedIntent.weekOffset) : 0
    targetDate = scheduleDateForWeek(Number(parsedIntent.dayOfWeek), weekOffset)
  }
  if (!targetDate) {
    const dayOffset = Number.isFinite(Number(parsedIntent.dayOffset)) ? Number(parsedIntent.dayOffset) : 0
    targetDate = dayjs().add(dayOffset, 'day')
  }

  const targetDay = targetDate.day()
  const dayItems = items.filter(item => scheduleDayNumber(item.dayOfWeek) === targetDay)
  const label = targetDate.isSame(dayjs(), 'day')
    ? 'hôm nay'
    : targetDate.isSame(dayjs().add(1, 'day'), 'day')
      ? 'ngày mai'
      : `${DAY_OF_WEEK_VN[targetDate.day()]} ${targetDate.format('DD/MM/YYYY')}`
  if (dayItems.length === 0) return `${label[0].toUpperCase()}${label.slice(1)} không có lịch trong dữ liệu hiện có.`

  const title = props.role === 'teacher'
    ? `Lịch dạy ${label} (${targetDate.format('DD/MM/YYYY')}) của Thầy/Cô:`
    : `Lịch ${label} (${targetDate.format('DD/MM/YYYY')}) của bạn:`
  return `${title}\n${dayItems
    .sort((a, b) => String(a.startTime || '').localeCompare(String(b.startTime || '')))
    .map(item => scheduleLineForDate(item, targetDate))
    .join('\n')}`
}

function classLine(item, index) {
  const name = item.className || item.classNameSnapshot || item.name || `Lớp ${index + 1}`
  const course = item.courseNameSnapshot || item.courseName || item.courseTitle || ''
  const room = item.room ? `, phòng ${item.room}` : ''
  const start = item.startDate ? `, khai giảng ${dayjs(item.startDate).format('DD/MM/YYYY')}` : ''
  const end = item.endDate ? `, kết thúc ${dayjs(item.endDate).format('DD/MM/YYYY')}` : ''
  const size = item.currentStudentCount != null || item.maxStudents != null
    ? `, sĩ số ${item.currentStudentCount ?? 0}/${item.maxStudents ?? '-'}`
    : ''
  return `- ${name}${course ? ` (${course})` : ''}${room}${start}${end}${size}`
}

function answerManagedClassesFromLocalData(intent = null) {
  if (intent?.intent !== 'managed_classes_lookup') return ''

  const items = [...managedClassItems.value]
  if (items.length === 0) {
    return props.role === 'teacher'
      ? 'Hiện chưa có dữ liệu lớp Thầy/Cô đang phụ trách trong hệ thống.'
      : 'Hiện chưa có dữ liệu lớp của bạn trong hệ thống.'
  }

  const title = props.role === 'teacher'
    ? `Thầy/Cô đang phụ trách ${items.length} lớp:`
    : `Bạn đang có ${items.length} lớp:`
  return `${title}\n${items.map(classLine).join('\n')}`
}

function queryTodaySchedule() {
  if (isTyping.value || streamingText.value) return
  inputValue.value = 'Hôm nay tôi có lịch học hoặc lịch dạy gì không?'
  sendMessage()
}

function toggleChat() {
  isOpen.value = !isOpen.value
  if (isOpen.value) {
    scrollToBottom()
    nextTick(() => chatInput.value?.focus())
  }
}

function scrollToBottom() {
  nextTick(() => {
    if (messageContainer.value) {
      messageContainer.value.scrollTop = messageContainer.value.scrollHeight
    }
  })
}

function saveMessages() {
  localStorage.setItem(localKey.value, JSON.stringify(messages.value))
}

function isLowValueNoDataReply(text) {
  return /không có thêm thông tin trong dữ liệu hiện có|không có thông tin .* trong dữ liệu hiện có/i.test(String(text || ''))
}

function chatHistoryForAi() {
  return messages.value
    .filter(m => m.text !== roleTheme.value.welcome)
    .filter(m => !(m.role === 'model' && isLowValueNoDataReply(m.text)))
    .slice(0, -1)
    .slice(-10)
    .map(m => ({ role: m.role, content: m.text }))
}

function clearHistory() {
  messages.value = [{
    role: 'model',
    text: roleTheme.value.welcome,
    timestamp: new Date().toISOString()
  }]
  saveMessages()
}

// Parse simple markdown tags
function parseMarkdown(text) {
  if (!text) return ''
  let html = text
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
  // Code block
  html = html.replace(/```([\s\S]*?)```/g, '<pre class="bg-slate-100 dark:bg-slate-900/60 p-2.5 rounded-lg text-xs font-mono my-2 overflow-x-auto select-all text-base-primary">$1</pre>')
  // Inline code
  html = html.replace(/`([^`\n]+)`/g, '<code class="bg-slate-100 dark:bg-slate-900/60 px-1 py-0.5 rounded text-[11px] font-mono mx-0.5 text-base-primary">$1</code>')
  // Bold
  html = html.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
  // Italic
  html = html.replace(/\*([^*]+)\*/g, '<em>$1</em>')
  // Line break
  html = html.replace(/\n/g, '<br />')
  return html
}

function formatTime(isoString) {
  if (!isoString) return ''
  return dayjs(isoString).format('HH:mm')
}

//  Send message with SSE streaming to AI Router 2.0 Flash
async function sendMessage() {
  const text = inputValue.value.trim()
  if (!text || isTyping.value || streamingText.value) return

  // Push user message
  messages.value.push({ role: 'user', text, timestamp: new Date().toISOString() })
  inputValue.value = ''
  isTyping.value = true
  streamingText.value = ''
  scrollToBottom()

  try {
    const eduIntent = await classifyEduIntent(text)
    const localClassAnswer = answerManagedClassesFromLocalData(eduIntent)
    if (localClassAnswer) {
      messages.value.push({ role: 'model', text: localClassAnswer, timestamp: new Date().toISOString() })
      saveMessages()
      return
    }

    const localScheduleAnswer = answerScheduleFromLocalData(text, eduIntent)
    if (localScheduleAnswer) {
      messages.value.push({ role: 'model', text: localScheduleAnswer, timestamp: new Date().toISOString() })
      saveMessages()
      return
    }

    const result = await callApi(http.post('/gateway/ai/knowledge/complete', {
      prompt: text,
      system: `${activeSystemInstruction.value}

Quy tắc hội thoại bắt buộc:
- Nếu người dùng chào hỏi ngắn như "xin chào", "alo", "hello", hãy chào lại tự nhiên và hỏi họ cần hỗ trợ gì.
- Không trả lời lời chào bằng câu "không có thêm thông tin trong dữ liệu hiện có".
- Chỉ nói thiếu dữ liệu khi người dùng hỏi một dữ liệu nội bộ cụ thể mà hệ thống chưa cung cấp.`,
      history: chatHistoryForAi(),
      maxOutputTokens: 1200
    }))
    messages.value.push({ role: 'model', text: result.text || 'AI không trả về nội dung.', timestamp: new Date().toISOString() })
    saveMessages()
  } catch (err) {
    messages.value.push({ role: 'model', text: `**Lỗi AI Assistant**: ${err.message || 'Không thể kết nối dịch vụ AI.'}`, timestamp: new Date().toISOString() })
  } finally {
    isTyping.value = false
    streamingText.value = ''
    scrollToBottom()
  }
}
</script>

<style scoped>
.ai-chatbot-wrapper {
  position: relative;
}

/* Floating Action Button */
.floating-chatbot-btn {
  position: fixed;
  bottom: 24px;
  right: 24px;
  width: 56px;
  height: 56px;
  border-radius: 999px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.2);
  border: none;
  z-index: 9999;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}
.floating-chatbot-btn:hover {
  transform: scale(1.08);
  box-shadow: 0 12px 35px rgba(0, 0, 0, 0.25);
}
.floating-chatbot-btn:active {
  transform: scale(0.94);
}

/* Pulse animation around the floating button */
.pulse-glow {
  position: absolute;
  top: 0; left: 0;
  width: 100%; height: 100%;
  border-radius: 999px;
  opacity: 0.3;
  z-index: -1;
  animation: pulse-ring 2.2s cubic-bezier(0.215, 0.610, 0.355, 1) infinite;
}

/* Chat Dialog Window */
.chat-dialog-window {
  position: fixed;
  bottom: 96px;
  right: 24px;
  width: 390px;
  max-width: calc(100vw - 48px);
  height: 540px;
  max-height: calc(100vh - 120px);
  border-radius: 24px;
  z-index: 9999;
  overflow: hidden;
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
  display: flex;
  flex-direction: column;
}

/* Streaming cursor blink */
.streaming-cursor {
  display: inline-block;
  color: currentColor;
  opacity: 0.7;
  animation: blink 0.7s step-end infinite;
  font-size: 14px;
  line-height: 1;
  margin-left: 1px;
}

@keyframes blink {
  0%, 100% { opacity: 0.7; }
  50% { opacity: 0; }
}

.bg-card-base { background: var(--bg-card); }
.border-base { border-color: var(--border-color); }

.animate-bounce-slow { animation: bounce-slow 3s infinite; }

@keyframes pulse-ring {
  0% { transform: scale(0.95); opacity: 0.5; }
  50% { opacity: 0.2; }
  100% { transform: scale(1.35); opacity: 0; }
}

@keyframes bounce-slow {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-4px); }
}

/* Transition animations */
.chat-fade-enter-active,
.chat-fade-leave-active {
  transition: opacity 0.25s ease, transform 0.25s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.chat-fade-enter-from,
.chat-fade-leave-to {
  opacity: 0;
  transform: translateY(20px) scale(0.9);
}

/* Scrollbar styling */
.custom-scrollbar::-webkit-scrollbar { width: 5px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #cbd5e1; border-radius: 99px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #94a3b8; }
.dark .custom-scrollbar::-webkit-scrollbar-thumb { background: #334155; }
.dark .custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #475569; }
</style>

