<template>
  <div class="space-y-6">
    <PageHeader 
      title="Táº¡o tÃ i liá»‡u giáº£ng dáº¡y tá»± Ä‘á»™ng (AI)" 
      subtitle="Sá»­ dá»¥ng Generative AI Ä‘á»ƒ táº¡o bá»™ cÃ¢u há»i tráº¯c nghiá»‡m (Quiz), Flashcards hoáº·c tÃ³m táº¯t bÃ i giáº£ng tá»« tÃ i liá»‡u gá»‘c."
    />

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- Cáº¥u hÃ¬nh Ä‘áº§u vÃ o (Form) -->
      <div class="lg:col-span-1 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-sm space-y-4">
        <h2 class="text-sm font-bold text-slate-800 dark:text-slate-200 flex items-center gap-2">
          <span class="w-1.5 h-3.5 bg-blue-600 rounded-full"></span>
          Cáº¥u hÃ¬nh tÃ i liá»‡u AI
        </h2>

        <div class="space-y-4">
          <!-- Äá»‹nh dáº¡ng tÃ i liá»‡u -->
          <div class="space-y-2">
            <label class="text-xs font-bold text-slate-500 uppercase tracking-wider block">1. Loáº¡i tÃ i liá»‡u cáº§n táº¡o:</label>
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

          <!-- MÃ´n há»c / Chá»§ Ä‘á» -->
          <div class="space-y-2">
            <label class="text-xs font-bold text-slate-500 uppercase tracking-wider block">2. MÃ´n há»c / Chá»§ Ä‘á» chÃ­nh:</label>
            <a-input 
              v-model:value="topic" 
              placeholder="VÃ­ dá»¥: Láº­p trÃ¬nh hÆ°á»›ng Ä‘á»‘i tÆ°á»£ng (OOP), React Hooks, v.v." 
              size="large"
              class="text-xs rounded-xl"
            />
          </div>

          <!-- Má»©c Ä‘á»™ khÃ³ -->
          <div class="space-y-2">
            <label class="text-xs font-bold text-slate-500 uppercase tracking-wider block">3. Má»©c Ä‘á»™ khÃ³:</label>
            <a-select v-model:value="difficulty" class="w-full custom-select" size="large">
              <a-select-option value="easy">CÆ¡ báº£n (Easy)</a-select-option>
              <a-select-option value="medium">Trung bÃ¬nh (Medium)</a-select-option>
              <a-select-option value="hard">NÃ¢ng cao (Hard)</a-select-option>
            </a-select>
          </div>

          <!-- Ná»™i dung tÃ i liá»‡u gá»‘c -->
          <div class="space-y-2">
            <div class="flex items-center justify-between">
              <label class="text-xs font-bold text-slate-500 uppercase tracking-wider block">4. Ná»™i dung hoáº·c tÃ i liá»‡u gá»‘c:</label>
              <span class="text-[10px] text-slate-400">{{ rawText.length }}/8000 kÃ½ tá»±</span>
            </div>
            <a-textarea 
              v-model:value="rawText" 
              placeholder="DÃ¡n Ä‘á» cÆ°Æ¡ng, ghi chÃº bÃ i giáº£ng hoáº·c tÃ i liá»‡u Ä‘á»c cá»§a há»c viÃªn táº¡i Ä‘Ã¢y lÃ m cÄƒn cá»© cháº¥m vÃ  táº¡o..." 
              :rows="8"
              class="text-xs rounded-xl custom-scrollbar"
              maxlength="8000"
            />
          </div>

          <!-- Button Táº¡o -->
          <button
            type="button"
            class="w-full py-3 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-xl shadow-md transition-all active:scale-[0.98] cursor-pointer disabled:opacity-50 flex items-center justify-center gap-2"
            :disabled="generating || !topic.trim() || !rawText.trim()"
            @click="generateMaterial"
          >
            <LoadingSpinner v-if="generating" size="sm" class="!text-white" />
            <span v-else>ðŸ¤– Báº¯t Ä‘áº§u táº¡o báº±ng AI</span>
          </button>
        </div>
      </div>

      <!-- Khung hiá»ƒn thá»‹ Káº¿t quáº£ (Preview & Actions) -->
      <div class="lg:col-span-2 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-2xl p-5 shadow-sm flex flex-col min-h-[500px]">
        <div class="flex items-center justify-between border-b border-slate-100 dark:border-slate-800 pb-4 mb-4">
          <h2 class="text-sm font-bold text-slate-800 dark:text-slate-200 flex items-center gap-2">
            <span class="w-1.5 h-3.5 bg-emerald-500 rounded-full"></span>
            Káº¿t quáº£ xem trÆ°á»›c tÃ i liá»‡u
          </h2>
          <div v-if="resultContent" class="flex items-center gap-2">
            <a-button size="small" class="rounded-lg text-xs" @click="copyResult">Sao chÃ©p Raw</a-button>
            <a-button size="small" type="primary" class="bg-blue-600 text-white rounded-lg text-xs border-none" @click="downloadResult">Táº£i xuá»‘ng file</a-button>
          </div>
        </div>

        <div class="flex-grow flex flex-col justify-center">
          <!-- Tráº¡ng thÃ¡i chá» -->
          <div v-if="!resultContent && !generating" class="py-24 text-center flex flex-col items-center space-y-3">
            <div class="w-16 h-16 rounded-full bg-slate-50 dark:bg-slate-800 flex items-center justify-center text-slate-400">
              <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
            </div>
            <h3 class="text-sm font-bold text-slate-700 dark:text-slate-300">ChÆ°a cÃ³ tÃ i liá»‡u Ä‘Æ°á»£c táº¡o</h3>
            <p class="text-xs text-slate-500 max-w-sm">Äiá»n cáº¥u hÃ¬nh á»Ÿ cá»™t bÃªn trÃ¡i vÃ  báº¥m táº¡o Ä‘á»ƒ nháº­n bá»™ tÃ i liá»‡u tá»« AI.</p>
          </div>

          <!-- Tráº¡ng thÃ¡i Loading -->
          <div v-else-if="generating" class="py-24 text-center flex flex-col items-center space-y-4">
            <LoadingSpinner size="lg" />
            <p class="text-xs text-slate-500 animate-pulse">AI Ä‘ang Ä‘á»c ná»™i dung gá»‘c vÃ  soáº¡n tháº£o tÃ i liá»‡u, vui lÃ²ng Ä‘á»£i giÃ¢y lÃ¡t...</p>
          </div>

          <!-- Káº¿t quáº£: Bá»™ cÃ¢u há»i tráº¯c nghiá»‡m (Quiz) -->
          <div v-else-if="selectedType === 'quiz' && quizData.length" class="space-y-4 flex-grow">
            <div 
              v-for="(q, index) in quizData" 
              :key="index"
              class="p-4 rounded-xl border border-slate-100 dark:border-slate-800 bg-slate-50/40 dark:bg-slate-900/40 space-y-2.5"
            >
              <div class="font-bold text-slate-850 dark:text-slate-200">
                CÃ¢u {{ index + 1 }}: {{ q.question }}
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
              
              <!-- Giáº£i thÃ­ch -->
              <div v-if="showAnswers" class="p-2.5 rounded-lg bg-blue-50/40 dark:bg-blue-950/10 border border-blue-100/60 dark:border-blue-900/40 text-[11px] leading-relaxed text-blue-800 dark:text-blue-300">
                <strong>Giáº£i thÃ­ch:</strong> {{ q.explanation }}
              </div>
            </div>

            <div class="flex justify-center pt-2">
              <a-button 
                type="primary" 
                class="bg-emerald-600 text-white rounded-lg h-9 font-bold border-none cursor-pointer" 
                @click="showAnswers = !showAnswers"
              >
                {{ showAnswers ? 'áº¨n Ä‘Ã¡p Ã¡n & giáº£i thÃ­ch' : 'Hiá»‡n Ä‘Ã¡p Ã¡n & giáº£i thÃ­ch' }}
              </a-button>
            </div>
          </div>

          <!-- Káº¿t quáº£: Flashcards -->
          <div v-else-if="selectedType === 'flashcard' && flashcardsData.length" class="space-y-6 flex-grow flex flex-col items-center justify-between py-6">
            <!-- Card View container -->
            <div class="w-full max-w-sm h-64 flashcard-container cursor-pointer" @click="isFlipped = !isFlipped">
              <div class="flashcard-inner relative w-full h-full text-center" :class="{ 'is-flipped': isFlipped }">
                <!-- Front Side -->
                <div class="flashcard-front absolute inset-0 bg-gradient-to-tr from-blue-500 to-indigo-600 text-white rounded-2xl p-6 shadow-lg flex flex-col justify-between items-center">
                  <span class="text-[9px] uppercase tracking-widest font-black opacity-75">Flashcard {{ activeCardIndex + 1 }} / {{ flashcardsData.length }} (Máº·t trÆ°á»›c)</span>
                  <div class="text-base font-bold text-center px-4">
                    {{ flashcardsData[activeCardIndex]?.front }}
                  </div>
                  <span class="text-[10px] opacity-50 block mt-2">Báº¥m Ä‘á»ƒ láº­t máº·t xem Ä‘Ã¡p Ã¡n â†º</span>
                </div>
                <!-- Back Side -->
                <div class="flashcard-back absolute inset-0 bg-emerald-600 text-white rounded-2xl p-6 shadow-lg flex flex-col justify-between items-center transform rotate-y-180">
                  <span class="text-[9px] uppercase tracking-widest font-black opacity-75">ÄÃ¡p Ã¡n (Máº·t sau)</span>
                  <div class="text-xs text-center px-2 leading-relaxed">
                    {{ flashcardsData[activeCardIndex]?.back }}
                  </div>
                  <span class="text-[10px] opacity-50 block mt-2">Báº¥m Ä‘á»ƒ quay láº¡i máº·t trÆ°á»›c â†º</span>
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
                BÃ i trÆ°á»›c
              </a-button>
              <span class="text-xs font-bold text-slate-500">{{ activeCardIndex + 1 }} / {{ flashcardsData.length }}</span>
              <a-button 
                type="primary"
                class="bg-blue-600 text-white rounded-lg h-9 border-none cursor-pointer"
                :disabled="activeCardIndex === flashcardsData.length - 1" 
                @click="nextCard"
              >
                BÃ i sau
              </a-button>
            </div>
          </div>

          <!-- Káº¿t quáº£: TÃ³m táº¯t bÃ i giáº£ng (Markdown) -->
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
  { value: 'quiz', label: 'Bá»™ cÃ¢u há»i Quiz', icon: 'ðŸ“' },
  { value: 'flashcard', label: 'Flashcards', icon: 'ðŸŽ´' },
  { value: 'summary', label: 'TÃ³m táº¯t bÃ i giáº£ng', icon: 'ðŸ“–' }
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
Báº¡n lÃ  chuyÃªn gia thiáº¿t káº¿ cÃ¢u há»i tráº¯c nghiá»‡m giÃ¡o dá»¥c. 
Nhiá»‡m vá»¥ cá»§a báº¡n lÃ  Ä‘á»c ká»¹ tÃ i liá»‡u gá»‘c vÃ  táº¡o ra bá»™ gá»“m 5 cÃ¢u há»i tráº¯c nghiá»‡m (multiple choice) vá» chá»§ Ä‘á» "${topic.value}" vá»›i Ä‘á»™ khÃ³ "${difficulty.value}".
Má»—i cÃ¢u há»i pháº£i cÃ³ 4 Ä‘Ã¡p Ã¡n lá»±a chá»n (A, B, C, D) vÃ  giáº£i thÃ­ch táº¡i sao Ä‘Ã¡p Ã¡n Ä‘Ã³ Ä‘Ãºng.

Äáº§u ra Báº®T BUá»˜C lÃ  má»™t máº£ng JSON cÃ¡c Ä‘á»‘i tÆ°á»£ng cÃ³ cáº¥u trÃºc chÃ­nh xÃ¡c nhÆ° sau:
[
  {
    "question": "CÃ¢u há»i sá»‘ 1...",
    "options": ["Lá»±a chá»n A", "Lá»±a chá»n B", "Lá»±a chá»n C", "Lá»±a chá»n D"],
    "correctAnswerIndex": 0,
    "explanation": "Giáº£i thÃ­ch táº¡i sao Ä‘Ã¡p Ã¡n A Ä‘Ãºng..."
  }
]
    `
  } else if (selectedType.value === 'flashcard') {
    systemPrompt = `
Báº¡n lÃ  giÃ¡o viÃªn biÃªn soáº¡n tháº» há»c táº­p thÃ´ng minh (flashcard).
Nhiá»‡m vá»¥ cá»§a báº¡n lÃ  Ä‘á»c ká»¹ tÃ i liá»‡u gá»‘c vÃ  táº¡o ra bá»™ gá»“m 5 tháº» há»c flashcard Ä‘á»ƒ ghi nhá»› kiáº¿n thá»©c cá»‘t lÃµi vá» chá»§ Ä‘á» "${topic.value}" vá»›i Ä‘á»™ khÃ³ "${difficulty.value}".
Má»—i tháº» pháº£i cÃ³:
- Máº·t trÆ°á»›c (front): Má»™t cÃ¢u há»i Ä‘á»‹nh nghÄ©a, thuáº­t ngá»¯ cáº§n giáº£i thÃ­ch hoáº·c má»™t Ä‘oáº¡n code cáº§n dá»± Ä‘oÃ¡n káº¿t quáº£.
- Máº·t sau (back): ÄÃ¡p Ã¡n chÃ­nh xÃ¡c giáº£i thÃ­ch ngáº¯n gá»n, sÃºc tÃ­ch (tá»‘i Ä‘a 2-3 cÃ¢u).

Äáº§u ra Báº®T BUá»˜C lÃ  má»™t máº£ng JSON cÃ¡c Ä‘á»‘i tÆ°á»£ng cÃ³ cáº¥u trÃºc chÃ­nh xÃ¡c nhÆ° sau:
[
  {
    "front": "Thuáº­t ngá»¯ hoáº·c cÃ¢u há»i máº·t trÆ°á»›c...",
    "back": "Lá»i giáº£i thÃ­ch hoáº·c Ä‘á»‹nh nghÄ©a máº·t sau..."
  }
]
    `
  } else {
    systemPrompt = `
Báº¡n lÃ  giÃ¡o vá»¥ cao cáº¥p soáº¡n tháº£o tÃ i liá»‡u há»c táº­p.
HÃ£y tÃ³m táº¯t bÃ i giáº£ng má»™t cÃ¡ch khoa há»c, chuyÃªn nghiá»‡p vá» chá»§ Ä‘á» "${topic.value}" dá»±a trÃªn tÃ i liá»‡u gá»‘c, vá»›i má»©c Ä‘á»™ "${difficulty.value}".
Báº£n tÃ³m táº¯t cáº§n phÃ¢n loáº¡i theo cáº¥u trÃºc rÃµ rÃ ng:
1. Giá»›i thiá»‡u tá»•ng quan
2. CÃ¡c khÃ¡i niá»‡m cá»‘t lÃµi cáº§n nhá»›
3. VÃ­ dá»¥ thá»±c tiá»…n hoáº·c bÃ i táº­p minh há»a
4. Lá»i khuyÃªn Ã´n luyá»‡n tá»« tháº§y cÃ´

HÃ£y viáº¿t á»Ÿ dáº¡ng vÄƒn báº£n Markdown chuáº©n má»±c báº±ng tiáº¿ng Viá»‡t.
    `
  }

  const promptContext = `
Ná»™i dung tÃ i liá»‡u gá»‘c:
"${rawText.value}"

YÃªu cáº§u thá»±c thi:
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
    
    message.success('ÄÃ£ táº¡o tÃ i liá»‡u thÃ nh cÃ´ng báº±ng AI!')
  } catch (err) {
    console.error(err)
    message.error('Gáº·p lá»—i khi táº¡o tÃ i liá»‡u há»c táº­p báº±ng AI.')
    simulateFallback()
  } finally {
    generating.value = false
  }
}

function simulateFallback() {
  if (selectedType.value === 'quiz') {
    quizData.value = [
      {
        question: `CÃ¢u há»i Ã´n táº­p cÆ¡ báº£n vá» ${topic.value || 'BÃ i há»c'} lÃ  gÃ¬?`,
        options: ['Lá»±a chá»n A - ÄÃ¡p Ã¡n máº«u', 'Lá»±a chá»n B', 'Lá»±a chá»n C', 'Lá»±a chá»n D'],
        correctAnswerIndex: 0,
        explanation: 'ÄÃ¢y lÃ  giáº£i thÃ­ch máº«u cho cÃ¢u tráº£ lá»i tráº¯c nghiá»‡m Ã´n táº­p.'
      }
    ]
    resultContent.value = JSON.stringify(quizData.value)
  } else if (selectedType.value === 'flashcard') {
    flashcardsData.value = [
      {
        front: `KhÃ¡i niá»‡m cá»‘t lÃµi trong ${topic.value || 'BÃ i há»c'} lÃ  gÃ¬?`,
        back: 'ÄÃ¢y lÃ  Ä‘á»‹nh nghÄ©a chi tiáº¿t cá»§a máº·t sau flashcard Ä‘á»ƒ há»— trá»£ ghi nhá»› nhanh chÃ³ng.'
      }
    ]
    resultContent.value = JSON.stringify(flashcardsData.value)
  } else {
    resultContent.value = `# TÃ³m táº¯t bÃ i há»c: ${topic.value}\n\n## 1. Tá»•ng quan\nBÃ i há»c cung cáº¥p cÃ¡c kiáº¿n thá»©c cÆ¡ báº£n vá» chá»§ Ä‘á» Ä‘Æ°á»£c yÃªu cáº§u.\n\n## 2. Kiáº¿n thá»©c cá»‘t lÃµi\n- Ghi nhá»› cÃ¡c khÃ¡i niá»‡m chÃ­nh trong tÃ i liá»‡u Ä‘á»c.\n- Xem xÃ©t cÃ¡c vÃ­ dá»¥ thá»±c hÃ nh.`
  }
}

function copyResult() {
  if (!resultContent.value) return
  navigator.clipboard.writeText(resultContent.value)
  message.success('ÄÃ£ sao chÃ©p ná»™i dung raw vÃ o bá»™ nhá»› táº¡m!')
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
