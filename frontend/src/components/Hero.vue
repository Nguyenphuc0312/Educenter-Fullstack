<template>
  <section id="home" class="relative min-h-screen flex items-center overflow-hidden gradient-hero pt-16">
    <!-- Decorative shapes -->
    <div
      class="absolute top-24 right-12 w-64 h-64 bg-blue-200/25 dark:bg-blue-700/10 rounded-full blur-3xl pointer-events-none animate-float"
      style="animation-duration: 5s;"
    />
    <div
      class="absolute bottom-20 left-10 w-80 h-80 bg-teal-200/20 dark:bg-teal-700/10 rounded-full blur-3xl pointer-events-none animate-float"
      style="animation-duration: 7s; animation-delay: 1s;"
    />
    <div
      class="absolute top-1/2 left-1/3 w-40 h-40 bg-indigo-200/15 dark:bg-indigo-700/10 rounded-full blur-2xl pointer-events-none animate-float"
      style="animation-duration: 6s; animation-delay: 2s;"
    />

    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-20 w-full">
      <div class="grid lg:grid-cols-2 gap-14 items-center">
        <!-- Left -->
        <div class="flex flex-col gap-6 reveal-on-scroll fade-up">
          <!-- Tag -->
          <div>
            <span class="inline-flex items-center gap-2 px-3 py-1.5 rounded-full text-xs font-semibold bg-blue-100 dark:bg-blue-900/40 text-blue-700 dark:text-blue-300 border border-blue-200 dark:border-blue-800">
              <span class="w-1.5 h-1.5 rounded-full bg-blue-500 animate-pulse" />
              Trung tâm đào tạo uy tín hàng đầu
            </span>
          </div>

          <!-- Title -->
          <h1 class="text-4xl sm:text-5xl lg:text-5xl xl:text-6xl font-bold text-base-primary leading-tight tracking-tight">
            Bắt đầu hành trình
            <span class="bg-gradient-to-r from-blue-600 to-teal-500 bg-clip-text text-transparent">
              học tập
            </span>
            của bạn cùng EduCenter
          </h1>

          <!-- Subtitle -->
          <p class="text-lg text-base-secondary leading-relaxed max-w-lg">
            Khám phá các khóa học chất lượng, học cùng giảng viên giàu kinh nghiệm và theo dõi lộ trình học tập của bạn một cách dễ dàng.
          </p>

          <!-- Badges -->
          <div class="flex flex-wrap gap-2">
            <div
              v-for="(b, i) in badges"
              :key="i"
              :class="`flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs font-semibold ${b.bg} ${b.color}`"
            >
              <component :is="b.icon" class="w-3.5 h-3.5" />
              {{ b.label }}
            </div>
          </div>

          <!-- CTA -->
          <div class="flex flex-wrap gap-3">
            <button
              id="btn-hero-explore"
              @click="scrollTo('courses')"
              class="inline-flex items-center gap-2 px-6 py-3.5 text-sm font-semibold text-white text-on-primary gradient-primary rounded-xl hover:opacity-90 transition-all cursor-pointer transform hover:scale-[1.03] active:scale-[0.97]"
            >
              Khám phá khóa học
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <line x1="5" y1="12" x2="19" y2="12"/>
                <polyline points="12 5 19 12 12 19"/>
              </svg>
            </button>
            <button
              id="btn-hero-consult"
              @click="scrollTo('contact')"
              class="inline-flex items-center gap-2 px-6 py-3.5 text-sm font-semibold text-base-primary bg-card-base border border-base rounded-xl hover:border-blue-300 dark:hover:border-blue-700 transition-all cursor-pointer transform hover:scale-[1.03] active:scale-[0.97]"
            >
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#3b82f6" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="12" r="10"/>
                <polygon points="10 8 16 12 10 16 10 8"/>
              </svg>
              Đăng ký tư vấn
            </button>
          </div>

          <!-- Trust row -->
          <div class="flex items-center gap-3 pt-2">
            <div class="flex -space-x-2">
              <img
                v-for="(src, i) in studentImages"
                :key="i"
                :src="src"
                alt="student"
                class="w-8 h-8 rounded-full border-2 border-white dark:border-slate-800 object-cover"
              />
            </div>
            <p class="text-sm text-base-secondary">
              <span class="font-semibold text-base-primary">500+</span> học viên đang theo học
            </p>
          </div>
        </div>

        <!-- Right — Mockup -->
        <div class="relative flex justify-center lg:justify-end reveal-on-scroll fade-left">
          <!-- Main image -->
          <div class="relative w-full max-w-md">
            <div class="rounded-2xl overflow-hidden border border-base shadow-card-base">
              <img
                src="https://images.unsplash.com/photo-1522202176988-66273c2fd55f?w=700&h=460&fit=crop"
                alt="Học viên học online tại EduCenter"
                class="w-full h-72 object-cover"
              />
            </div>

            <!-- Floating course card -->
            <div
              class="absolute -bottom-6 -left-6 bg-card-base border border-base shadow-card-base rounded-xl p-4 w-56 animate-float"
              style="animation-duration: 4s;"
            >
              <div class="flex items-start gap-2.5">
                <div class="w-8 h-8 rounded-lg gradient-primary flex items-center justify-center shrink-0">
                  <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z"/>
                    <path d="M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z"/>
                  </svg>
                </div>
                <div class="min-w-0">
                  <p class="text-xs font-semibold text-base-primary leading-snug truncate">{{ mockCourseCard.title }}</p>
                  <p class="text-[11px] text-base-muted mt-0.5">{{ mockCourseCard.teacher }}</p>
                </div>
              </div>
              <div class="mt-3">
                <div class="flex justify-between text-[11px] text-base-muted mb-1">
                  <span>Tiến độ</span>
                  <span class="font-semibold text-blue-600 dark:text-blue-400">{{ mockCourseCard.progress }}%</span>
                </div>
                <div class="w-full bg-slate-100 dark:bg-slate-700 rounded-full h-1.5">
                  <div class="h-full rounded-full bg-gradient-to-r from-blue-500 to-teal-500" :style="{ width: `${mockCourseCard.progress}%` }" />
                </div>
              </div>
            </div>

            <!-- Floating achievement badge -->
            <div
              class="absolute -top-4 -right-4 bg-card-base border border-base shadow-card-base rounded-xl px-3 py-2.5 flex items-center gap-2 animate-float"
              style="animation-duration: 5s; animation-delay: 1s;"
            >
              <div class="w-7 h-7 rounded-full bg-amber-100 dark:bg-amber-900/30 flex items-center justify-center">
                <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="amber" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-amber-500 fill-amber-500">
                  <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/>
                </svg>
              </div>
              <div>
                <p class="text-xs font-bold text-base-primary">4.9/5.0</p>
                <p class="text-[10px] text-base-muted">Đánh giá TB</p>
              </div>
            </div>

            <!-- Floating stat -->
            <div
              class="absolute top-1/2 -right-8 bg-card-base border border-base shadow-card-base rounded-xl px-3 py-2 flex items-center gap-2 animate-float"
              style="animation-duration: 4.5s; animation-delay: 2s;"
            >
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#14b8a6" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="shrink-0">
                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                <polyline points="22 4 12 14.01 9 11.01"/>
              </svg>
              <p class="text-xs font-semibold text-base-primary whitespace-nowrap">95% hài lòng</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { h } from 'vue'
import { useReveal } from '../composables/useReveal'

useReveal()

// Define Lucide replacement SVG components or configurations
const badges = [
  {
    label: '500+ học viên',
    color: 'text-blue-600 dark:text-blue-400',
    bg: 'bg-blue-50 dark:bg-blue-950/50',
    icon: {
      render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
        h('path', { d: 'M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2' }),
        h('circle', { cx: '9', cy: '7', r: '4' }),
        h('path', { d: 'M22 21v-2a4 4 0 0 0-3-3.87' }),
        h('path', { d: 'M16 3.13a4 4 0 0 1 0 7.75' })
      ])
    }
  },
  {
    label: '20+ khóa học',
    color: 'text-teal-600 dark:text-teal-400',
    bg: 'bg-teal-50 dark:bg-teal-950/50',
    icon: {
      render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
        h('path', { d: 'M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z' }),
        h('path', { d: 'M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z' })
      ])
    }
  },
  {
    label: '92% hài lòng',
    color: 'text-amber-500',
    bg: 'bg-amber-50 dark:bg-amber-950/50',
    icon: {
      render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'amber', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round', class: 'fill-amber-500' }, [
        h('polygon', { points: '12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2' })
      ])
    }
  }
]

const studentImages = [
  'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=40&h=40&fit=crop&crop=face',
  'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=40&h=40&fit=crop&crop=face',
  'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=40&h=40&fit=crop&crop=face',
  'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=40&h=40&fit=crop&crop=face'
]

const mockCourseCard = {
  title: 'ReactJS Cơ bản đến Nâng cao',
  teacher: 'Nguyễn Hoàng Nam',
  progress: 65,
  sessions: 24,
  rating: 4.9,
  tag: 'Frontend'
}

function scrollTo(id) {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' })
}
</script>
