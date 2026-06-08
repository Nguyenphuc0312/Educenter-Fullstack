<template>
  <section class="py-20 bg-page">
    <div class="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="reveal-on-scroll fade-up">
        <div class="relative overflow-hidden rounded-3xl gradient-primary p-10 sm:p-14 text-center">
          <!-- Decorative blobs -->
          <div
            class="absolute -top-12 -left-12 w-56 h-56 bg-white/10 rounded-full pointer-events-none animate-float"
            style="animation-duration: 5s;"
          />
          <div
            class="absolute -bottom-10 -right-10 w-64 h-64 bg-white/10 rounded-full pointer-events-none animate-float"
            style="animation-duration: 6s; animation-delay: 1s;"
          />

          <div class="relative z-10">
            <span class="inline-block text-xs font-semibold text-white/80 bg-white/15 px-3 py-1 rounded-full mb-5">
              🎓 Bắt đầu ngay hôm nay
            </span>
            <h2 class="text-3xl sm:text-4xl lg:text-5xl font-bold text-white mb-4 leading-tight">
              Sẵn sàng bắt đầu khóa học tiếp theo?
            </h2>
            <p class="text-white/80 text-base sm:text-lg max-w-xl mx-auto mb-8 leading-relaxed">
              Đăng ký tư vấn miễn phí để EduCenter giúp bạn chọn lộ trình học phù hợp với mục tiêu nghề nghiệp của bạn.
            </p>

            <div class="flex flex-wrap justify-center gap-4">
              <button
                id="btn-cta-consult"
                @click="scrollTo('contact')"
                class="inline-flex items-center gap-2 px-7 py-3.5 bg-white text-blue-700 font-semibold text-sm rounded-xl hover:bg-blue-50 transition-all cursor-pointer transform hover:scale-[1.05] active:scale-[0.97]"
              >
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M21 11.5a8.38 8.38 0 0 1-.9 3.8 8.5 8.5 0 0 1-7.6 4.7 8.38 8.38 0 0 1-3.8-.9L3 21l1.9-5.7a8.38 8.38 0 0 1-.9-3.8 8.5 8.5 0 0 1 4.7-7.6 8.38 8.38 0 0 1 3.8-.9h.5a8.48 8.48 0 0 1 8 8v.5z"/>
                </svg>
                Đăng ký tư vấn
              </button>
              <button
                id="btn-cta-courses"
                @click="scrollTo('courses')"
                class="inline-flex items-center gap-2 px-7 py-3.5 bg-white/15 border border-white/30 text-white font-semibold text-sm rounded-xl hover:bg-white/25 transition-all cursor-pointer transform hover:scale-[1.05] active:scale-[0.97]"
              >
                Xem tất cả khóa học
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <line x1="5" y1="12" x2="19" y2="12"/>
                  <polyline points="12 5 19 12 12 19"/>
                </svg>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { useReveal } from '../composables/useReveal'

useReveal()

function scrollTo(id) {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' })
}
</script>
