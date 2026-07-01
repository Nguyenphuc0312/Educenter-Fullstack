<template>
  <div class="space-y-6">
    <PageHeader 
      title="Tạo tài liệu giảng dạy tự động (AI)" 
      subtitle="Sử dụng Generative AI để tạo bộ câu hỏi trắc nghiệm (Quiz), Flashcards hoặc tóm tắt bài giảng từ tài liệu gốc."
    />

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- Cấu hình đầu vào (Form) -->
      <div class="lg:col-span-1 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-sm space-y-4">
        <h2 class="text-sm font-bold text-slate-800 dark:text-slate-200 flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-blue-600 rounded-full"></span>
          Cấu hình tài liệu AI
        </h2>

        <div class="space-y-4">
          <!-- Định dạng tài liệu -->
          <div class="space-y-2">
            <label class="text-xs font-bold text-slate-500 uppercase tracking-wider block">1. Loại tài liệu cần tạo:</label>
            <div class="grid grid-cols-3 gap-2">
              <button 
                v-for="type in materialTypes" 
                :key="type.value"
                type="button"
                class="px-2.5 py-3.5 rounded-xl border flex flex-col items-center gap-2 transition-all duration-200 cursor-pointer"
                :class="selectedType === type.value 
                  ? 'border-blue-500 bg-blue-50/50 text-blue-600 dark:bg-blue-950/20 dark:text-blue-400 font-bold' 
                  : 'border-slate-200 text-slate-600 dark:border-slate-800 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-slate-800/40'"
                @click="selectedType = type.value"
              >
                <span class="text-xl">{{ type.icon }}</span>
                <span class="text-[10px] text-center leading-tight">{{ type.label }}</span>
              </button>
            </div>
          </div>

          <!-- Môn học / Chủ đề -->
          <div class="space-y-2">
            <label class="text-xs font-bold text-slate-500 uppercase tracking-wider block">2. Môn học / Chủ đề chính:</label>
            <a-input 
              v-model:value="topic" 
              placeholder="Ví dụ: Lập trình hướng đối tượng (OOP), React Hooks, v.v." 
              size="large"
              class="text-xs rounded-xl"
            />
          </div>

          <!-- Mức độ khó -->
          <div class="space-y-2">
            <label class="text-xs font-bold text-slate-500 uppercase tracking-wider block">3. Mức độ khó:</label>
            <a-select v-model:value="difficulty" class="w-full custom-select" size="large">
              <a-select-option value="easy">Cơ bản (Easy)</a-select-option>
              <a-select-option value="medium">Trung bình (Medium)</a-select-option>
              <a-select-option value="hard">Nâng cao (Hard)</a-select-option>
            </a-select>
          </div>

          <!-- Nội dung tài liệu gốc -->
          <div class="space-y-2">
            <div class="flex items-center justify-between">
              <label class="text-xs font-bold text-slate-500 uppercase tracking-wider block">4. Nội dung hoặc tài liệu gốc:</label>
              <span class="text-[10px] text-slate-400">{{ rawText.length }}/8000 ký tự</span>
            </div>
            <a-textarea 
              v-model:value="rawText" 
              placeholder="Dán đề cương, ghi chú bài giảng hoặc tài liệu đọc của học viên tại đây làm căn cứ chấm và tạo..." 
              :rows="8"
              class="text-xs rounded-xl custom-scrollbar"
              maxlength="8000"
            />
          </div>

          <!-- Button Tạo -->
          <button
            type="button"
            class="w-full py-3 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-xl shadow-md transition-all active:scale-[0.98] cursor-pointer disabled:opacity-50 flex items-center justify-center gap-2"
            :disabled="generating || !topic.trim() || !rawText.trim()"
            @click="generateMaterial"
          >
            <LoadingSpinner v-if="generating" size="sm" class="!text-white" />
            <span v-else>🤖 Bắt đầu tạo bằng AI</span>
          </button>
        </div>
      </div>

      <!-- Khung hiển thị Kết quả (Preview & Actions) -->
      <div class="lg:col-span-2 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-sm flex flex-col min-h-[500px]">
        <div class="flex items-center justify-between border-b border-slate-100 dark:border-slate-800 pb-4 mb-4">
          <h2 class="text-sm font-bold text-slate-800 dark:text-slate-200 flex items-center gap-2">
            <span class="w-1.5 h-3.5 bg-emerald-500 rounded-full"></span>
            Kết quả xem trước tài liệu
          </h2>
          <div v-if="resultContent" class="flex items-center gap-2">
            <a-button size="small" class="rounded-lg text-xs" @click="copyResult">Sao chép Raw</a-button>
            <a-button size="small" type="primary" class="bg-blue-600 text-white rounded-lg text-xs border-none" @click="downloadResult">Tải xuống file</a-button>
          </div>
        </div>

        <div class="flex-grow flex flex-col justify-center">
          <!-- Trạng thái chờ -->
          <div v-if="!resultContent && !generating" class="py-24 text-center flex flex-col items-center space-y-3">
            <div class="w-16 h-16 rounded-full bg-slate-50 dark:bg-slate-800 flex items-center justify-center text-slate-400">
              <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
            </div>
            <h3 class="text-sm font-bold text-slate-700 dark:text-slate-300">Chưa có tài liệu được tạo</h3>
            <p class="text-xs text-slate-500 max-w-sm">Điền cấu hình ở cột bên trái và bấm tạo để nhận bộ tài liệu từ AI.</p>
          </div>

          <!-- Trạng thái Loading -->
          <div v-else-if="generating" class="py-24 text-center flex flex-col items-center space-y-4">
            <LoadingSpinner size="lg" />
            <p class="text-xs text-slate-500 animate-pulse">AI đang đọc nội dung gốc và soạn thảo tài liệu, vui lòng đợi giây lát...</p>
          </div>

          <!-- Kết quả: Bộ câu hỏi trắc nghiệm (Quiz) -->
          <div v-else-if="selectedType === 'quiz' && quizData.length" class="space-y-4 flex-grow">
            <div 
              v-for="(q, index) in quizData" 
              :key="index"
              class="p-4 rounded-xl border border-slate-100 dark:border-slate-800 bg-slate-50/40 dark:bg-slate-900/40 space-y-2.5"
            >
              <div class="font-bold text-slate-850 dark:text-slate-200">
                Câu {{ index + 1 }}: {{ q.question }}
              </div>
              <div class="grid grid-cols-1 md:grid-cols-2 gap-2 text-xs">
                <div 
                  v-for="(opt, oIdx) in q.options" 
                  :key="oIdx"
                  class="p-2.5 rounded-lg border border-slate-200/60 dark:border-slate-800 bg-white dark:bg-slate-950 flex items-center gap-2"
                  :class="showAnswers && oIdx === q.correctAnswerIndex ? 'border-emerald-500 bg-emerald-50/20 text-emerald-700 dark:text-emerald-400 font-bold' : ''"
                >
                  <span class="w-5 h-5 rounded-full bg-slate-100 dark:bg-slate-800 text-slate-500 flex items-center justify-center font-bold text-[10px] shrink-0"
                    :class="showAnswers && oIdx === q.correctAnswerIndex ? '!bg-emerald-500 !text-white' : ''">
                    {{ String.fromCharCode(65 + oIdx) }}
                  </span>
                  <span>{{ opt }}</span>
                </div>
              </div>
              
              <!-- Giải thích -->
              <div v-if="showAnswers" class="p-2.5 rounded-lg bg-blue-50/40 dark:bg-blue-950/10 border border-blue-100/60 dark:border-blue-900/40 text-[11px] leading-relaxed text-blue-800 dark:text-blue-300">
                <strong>Giải thích:</strong> {{ q.explanation }}
              </div>
            </div>

            <div class="flex justify-center pt-2">
              <a-button 
                type="primary" 
                class="bg-emerald-600 text-white rounded-lg h-9 font-bold border-none cursor-pointer" 
                @click="showAnswers = !showAnswers"
              >
                {{ showAnswers ? 'Ẩn đáp án & giải thích' : 'Hiện đáp án & giải thích' }}
              </a-button>
            </div>
          </div>

          <!-- Kết quả: Flashcards -->
          <div v-else-if="selectedType === 'flashcard' && flashcardsData.length" class="space-y-6 flex-grow flex flex-col items-center justify-between py-6">
            <!-- Card View container -->
            <div class="w-full max-w-sm h-64 flashcard-container cursor-pointer" @click="isFlipped = !isFlipped">
              <div class="flashcard-inner relative w-full h-full text-center" :class="{ 'is-flipped': isFlipped }">
                <!-- Front Side -->
                <div class="flashcard-front absolute inset-0 bg-gradient-to-tr from-blue-500 to-indigo-600 text-white rounded-2xl p-6 shadow-lg flex flex-col justify-between items-center">
                  <span class="text-[9px] uppercase tracking-widest font-black opacity-75">Flashcard {{ activeCardIndex + 1 }} / {{ flashcardsData.length }} (Mặt trước)</span>
                  <div class="text-base font-bold text-center px-4">
                    {{ flashcardsData[activeCardIndex]?.front }}
                  </div>
                  <span class="text-[10px] opacity-50 block mt-2">Bấm để lật mặt xem đáp án ↺</span>
                </div>
                <!-- Back Side -->
                <div class="flashcard-back absolute inset-0 bg-emerald-600 text-white rounded-2xl p-6 shadow-lg flex flex-col justify-between items-center transform rotate-y-180">
                  <span class="text-[9px] uppercase tracking-widest font-black opacity-75">Đáp án (Mặt sau)</span>
                  <div class="text-xs text-center px-2 leading-relaxed">
                    {{ flashcardsData[activeCardIndex]?.back }}
                  </div>
                  <span class="text-[10px] opacity-50 block mt-2">Bấm để quay lại mặt trước ↺</span>
                </div>
              </div>
            </div>

            <!-- Navigation Controls -->
            <div class="flex items-center gap-4">
              <a-button 
                class="rounded-lg h-9 cursor-pointer" 
                :disabled="activeCardIndex === 0" 
                @click="prevCard"
              >
                Bài trước
              </a-button>
              <span class="text-xs font-bold text-slate-500">{{ activeCardIndex + 1 }} / {{ flashcardsData.length }}</span>
              <a-button 
                type="primary"
                class="bg-blue-600 text-white rounded-lg h-9 border-none cursor-pointer"
                :disabled="activeCardIndex === flashcardsData.length - 1" 
                @click="nextCard"
              >
                Bài sau
              </a-button>
            </div>
          </div>

          <!-- Kết quả: Tóm tắt bài giảng (Markdown) -->
          <div v-else-if="selectedType === 'summary' && resultContent" class="prose dark:prose-invert max-w-none text-xs leading-relaxed space-y-4 flex-grow p-4 bg-slate-50 dark:bg-slate-900/50 rounded-xl border border-slate-100 dark:border-slate-800 select-text overflow-y-auto max-h-[500px] custom-scrollbar">
            <div v-html="parsedSummaryMarkdown"></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { message } from 'ant-design-vue'
import PageHeader from '@/components/ui/PageHeader.vue'
import LoadingSpinner from '@/components/ui/LoadingSpinner.vue'
import { aiApi } from '@/api/aiApi'

const materialTypes = [
  { value: 'quiz', label: 'Bộ câu hỏi Quiz', icon: '📝' },
  { value: 'flashcard', label: 'Flashcards', icon: '🎴' },
  { value: 'summary', label: 'Tóm tắt bài giảng', icon: '📖' }
]

const selectedType = ref('quiz')
const topic = ref('')
const difficulty = ref('medium')
const rawText = ref('')
const generating = ref(false)
const resultContent = ref('')

// Quiz details
const quizData = ref([])
const showAnswers = ref(false)

// Flashcards details
const flashcardsData = ref([])
const activeCardIndex = ref(0)
const isFlipped = ref(false)

function prevCard() {
  if (activeCardIndex.value > 0) {
    isFlipped.value = false
    setTimeout(() => {
      activeCardIndex.value--
    }, 150)
  }
}

function nextCard() {
  if (activeCardIndex.value < flashcardsData.value.length - 1) {
    isFlipped.value = false
    setTimeout(() => {
      activeCardIndex.value++
    }, 150)
  }
}

// Convert markdown text to HTML simple parser
const parsedSummaryMarkdown = computed(() => {
  if (!resultContent.value) return ''
  let html = resultContent.value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
  
  // Headers
  html = html.replace(/^### (.*$)/gim, '<h4 class="text-xs font-bold text-slate-800 dark:text-slate-200 mt-3 mb-1.5">$1</h4>')
  html = html.replace(/^## (.*$)/gim, '<h3 class="text-sm font-bold text-blue-600 dark:text-blue-400 mt-4 mb-2 border-b border-slate-200 pb-1">$1</h3>')
  html = html.replace(/^# (.*$)/gim, '<h2 class="text-base font-black text-slate-800 dark:text-slate-200 mt-6 mb-3">$1</h2>')
  
  // Bold/Italic
  html = html.replace(/\*\*([^\*]+)\*\*/g, '<strong>$1</strong>')
  html = html.replace(/\*([^\*]+)\*/g, '<em>$1</em>')
  
  // Unordered list
  html = html.replace(/^\s*-\s+(.*$)/gim, '<li class="ml-4 list-disc">$1</li>')
  
  // Line breaks
  html = html.replace(/\n/g, '<br />')
  
  return html
})

async function generateMaterial() {
  if (!topic.value.trim() || !rawText.value.trim()) return
  generating.value = true
  resultContent.value = ''
  quizData.value = []
  flashcardsData.value = []
  activeCardIndex.value = 0
  isFlipped.value = false
  showAnswers.value = false

  let systemPrompt = ''
  if (selectedType.value === 'quiz') {
    systemPrompt = `
Bạn là chuyên gia thiết kế câu hỏi trắc nghiệm giáo dục. 
Nhiệm vụ của bạn là đọc kỹ tài liệu gốc và tạo ra bộ gồm 5 câu hỏi trắc nghiệm (multiple choice) về chủ đề "${topic.value}" với độ khó "${difficulty.value}".
Mỗi câu hỏi phải có 4 đáp án lựa chọn (A, B, C, D) và giải thích tại sao đáp án đó đúng.

Đầu ra BẮT BUỘC là một mảng JSON các đối tượng có cấu trúc chính xác như sau:
[
  {
    "question": "Câu hỏi số 1...",
    "options": ["Lựa chọn A", "Lựa chọn B", "Lựa chọn C", "Lựa chọn D"],
    "correctAnswerIndex": 0,
    "explanation": "Giải thích tại sao đáp án A đúng..."
  }
]
    `
  } else if (selectedType.value === 'flashcard') {
    systemPrompt = `
Bạn là giáo viên biên soạn thẻ học tập thông minh (flashcard).
Nhiệm vụ của bạn là đọc kỹ tài liệu gốc và tạo ra bộ gồm 5 thẻ học flashcard để ghi nhớ kiến thức cốt lõi về chủ đề "${topic.value}" với độ khó "${difficulty.value}".
Mỗi thẻ phải có:
- Mặt trước (front): Một câu hỏi định nghĩa, thuật ngữ cần giải thích hoặc một đoạn code cần dự đoán kết quả.
- Mặt sau (back): Đáp án chính xác giải thích ngắn gọn, súc tích (tối đa 2-3 câu).

Đầu ra BẮT BUỘC là một mảng JSON các đối tượng có cấu trúc chính xác như sau:
[
  {
    "front": "Thuật ngữ hoặc câu hỏi mặt trước...",
    "back": "Lời giải thích hoặc định nghĩa mặt sau..."
  }
]
    `
  } else {
    systemPrompt = `
Bạn là giáo vụ cao cấp soạn thảo tài liệu học tập.
Hãy tóm tắt bài giảng một cách khoa học, chuyên nghiệp về chủ đề "${topic.value}" dựa trên tài liệu gốc, với mức độ "${difficulty.value}".
Bản tóm tắt cần phân loại theo cấu trúc rõ ràng:
1. Giới thiệu tổng quan
2. Các khái niệm cốt lõi cần nhớ
3. Ví dụ thực tiễn hoặc bài tập minh họa
4. Lời khuyên ôn luyện từ thầy cô

Hãy viết ở dạng văn bản Markdown chuẩn mực bằng tiếng Việt.
    `
  }

  const promptContext = `
Nội dung tài liệu gốc:
"${rawText.value}"

Yêu cầu thực thi:
${systemPrompt}
  `

  try {
    const data = await aiApi.complete({
      prompt: promptContext,
      jsonMode: selectedType.value !== 'summary',
      maxOutputTokens: 1800
    })
    const text = data.text || ''
    
    resultContent.value = text

    if (selectedType.value === 'quiz') {
      quizData.value = JSON.parse(text.trim())
    } else if (selectedType.value === 'flashcard') {
      flashcardsData.value = JSON.parse(text.trim())
    }
    
    message.success('Đã tạo tài liệu thành công bằng AI!')
  } catch (err) {
    console.error(err)
    message.error('Gặp lỗi khi tạo tài liệu học tập bằng AI.')
    simulateFallback()
  } finally {
    generating.value = false
  }
}

function simulateFallback() {
  if (selectedType.value === 'quiz') {
    quizData.value = [
      {
        question: `Câu hỏi ôn tập cơ bản về ${topic.value || 'Bài học'} là gì?`,
        options: ['Lựa chọn A - Đáp án mẫu', 'Lựa chọn B', 'Lựa chọn C', 'Lựa chọn D'],
        correctAnswerIndex: 0,
        explanation: 'Đây là giải thích mẫu cho câu trả lời trắc nghiệm ôn tập.'
      }
    ]
    resultContent.value = JSON.stringify(quizData.value)
  } else if (selectedType.value === 'flashcard') {
    flashcardsData.value = [
      {
        front: `Khái niệm cốt lõi trong ${topic.value || 'Bài học'} là gì?`,
        back: 'Đây là định nghĩa chi tiết của mặt sau flashcard để hỗ trợ ghi nhớ nhanh chóng.'
      }
    ]
    resultContent.value = JSON.stringify(flashcardsData.value)
  } else {
    resultContent.value = `# Tóm tắt bài học: ${topic.value}\n\n## 1. Tổng quan\nBài học cung cấp các kiến thức cơ bản về chủ đề được yêu cầu.\n\n## 2. Kiến thức cốt lõi\n- Ghi nhớ các khái niệm chính trong tài liệu đọc.\n- Xem xét các ví dụ thực hành.`
  }
}

function copyResult() {
  if (!resultContent.value) return
  navigator.clipboard.writeText(resultContent.value)
  message.success('Đã sao chép nội dung raw vào bộ nhớ tạm!')
}

function downloadResult() {
  if (!resultContent.value) return
  const blob = new Blob([resultContent.value], { type: 'text/plain;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement("a")
  link.setAttribute("href", url)
  
  const ext = selectedType.value === 'summary' ? 'md' : 'json'
  link.setAttribute("download", `TaiLieu_${selectedType.value}_${topic.value.replace(/\s+/g, '_')}.${ext}`)
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}
</script>

<style scoped>
/* Flashcard Styling with 3D Flip animation */
.flashcard-container {
  perspective: 1000px;
}
.flashcard-inner {
  transition: transform 0.6s;
  transform-style: preserve-3d;
}
.flashcard-inner.is-flipped {
  transform: rotateY(180deg);
}
.flashcard-front, .flashcard-back {
  -webkit-backface-visibility: hidden;
  backface-visibility: hidden;
  border-radius: 1rem;
}
.rotate-y-180 {
  transform: rotateY(180deg);
}

.animate-fade-in {
  animation: fadeIn 0.3s ease-in-out forwards;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(5px); }
  to { opacity: 1; transform: translateY(0); }
}

:deep(.custom-select .ant-select-selector) {
  border-radius: 12px !important;
  padding: 4px 12px !important;
  height: auto !important;
  border-color: #e2e8f0 !important;
}
</style>
