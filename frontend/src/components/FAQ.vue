<template>
  <section class="py-20 bg-section">
    <div class="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="reveal-on-scroll fade-up flex flex-col gap-6">
        <div class="text-center mb-12">
          <span class="inline-block text-xs font-semibold text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-950/40 px-3 py-1 rounded-full mb-3">
            Câu hỏi thường gặp
          </span>
          <h2 class="text-3xl sm:text-4xl font-bold text-base-primary mb-3">
            Bạn có thắc mắc?
          </h2>
          <p class="text-base-secondary">
            Những câu hỏi phổ biến nhất từ học viên về chương trình học tại EduCenter.
          </p>
        </div>

        <div class="flex flex-col gap-3">
          <div
            v-for="(faq, idx) in faqItems"
            :key="faq.id"
            class="border border-base rounded-2xl overflow-hidden bg-card-base"
          >
            <button
              :id="`faq-${faq.id}`"
              @click="toggleOpen(idx)"
              class="w-full flex items-center justify-between px-5 py-4 text-left group cursor-pointer"
            >
              <span
                :class="[
                  'text-sm font-semibold pr-4 transition-colors duration-200',
                  faq.open ? 'text-blue-600 dark:text-blue-400' : 'text-base-primary group-hover:text-blue-600 dark:group-hover:text-blue-400'
                ]"
              >
                {{ faq.question }}
              </span>
              <div
                :class="[
                  'shrink-0 w-7 h-7 rounded-full flex items-center justify-center transition-all duration-250',
                  faq.open ? 'bg-blue-600 text-white rotate-180' : 'bg-section text-base-muted group-hover:bg-blue-50 dark:group-hover:bg-blue-950/40 group-hover:text-blue-500'
                ]"
              >
                <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <polyline points="6 9 12 15 18 9"/>
                </svg>
              </div>
            </button>

            <transition
              name="expand"
              @enter="enter"
              @after-enter="afterEnter"
              @leave="leave"
            >
              <div v-if="faq.open" class="overflow-hidden">
                <div class="px-5 pb-4 border-t border-base">
                  <p class="text-sm text-base-secondary leading-relaxed pt-3">
                    {{ faq.answer }}
                  </p>
                </div>
              </div>
            </transition>
          </div>
        </div>

        <div class="text-center mt-10">
          <p class="text-sm text-base-secondary">
            Vẫn còn thắc mắc?
            <a
              href="#contact"
              @click="scrollToContact"
              class="text-blue-600 dark:text-blue-400 font-semibold hover:underline"
            >
              Liên hệ với chúng tôi
            </a>
          </p>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref } from 'vue'
import { faqs } from '../data/mockData'
import { useReveal } from '../composables/useReveal'

useReveal()

const faqItems = ref(faqs.map(faq => ({ ...faq, open: false })))

function toggleOpen(idx) {
  faqItems.value[idx].open = !faqItems.value[idx].open
}

function scrollToContact(e) {
  e.preventDefault()
  document.getElementById('contact')?.scrollIntoView({ behavior: 'smooth' })
}

// Expand animation helpers for smooth height transitions
function enter(element) {
  element.style.height = 'auto';
  const height = getComputedStyle(element).height;
  element.style.height = '0px';
  // Force repaint
  element.offsetHeight;
  requestAnimationFrame(() => {
    element.style.height = height;
  });
}

function afterEnter(element) {
  element.style.height = 'auto';
}

function leave(element) {
  const height = getComputedStyle(element).height;
  element.style.height = height;
  // Force repaint
  element.offsetHeight;
  requestAnimationFrame(() => {
    element.style.height = '0px';
  });
}
</script>

<style scoped>
.expand-enter-active,
.expand-leave-active {
  transition: height 0.3s cubic-bezier(0.25, 1, 0.5, 1), opacity 0.3s ease;
}
.expand-enter-from,
.expand-leave-to {
  height: 0px;
  opacity: 0;
}
</style>
