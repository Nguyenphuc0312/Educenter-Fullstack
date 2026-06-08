<template>
  <section class="py-20 bg-section">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="reveal-on-scroll fade-up">
        <div class="grid lg:grid-cols-2 gap-12 items-center">
          <!-- Text left -->
          <div>
            <div>
              <span class="inline-block text-xs font-semibold text-teal-600 dark:text-teal-400 bg-teal-50 dark:bg-teal-950/40 px-3 py-1 rounded-full mb-3">
                Cổng học viên
              </span>
              <h2 class="text-3xl sm:text-4xl font-bold text-base-primary mb-4">
                Sau khi đăng ký, bạn
                <span class="bg-gradient-to-r from-blue-600 to-teal-500 bg-clip-text text-transparent">
                  theo dõi học tập
                </span>
                dễ dàng
              </h2>
              <p class="text-base-secondary leading-relaxed mb-6">
                Cổng học viên EduCenter giúp bạn theo dõi tiến độ, lịch học, kết quả và quản lý toàn bộ hành trình học tập trong một nơi duy nhất.
              </p>
            </div>

            <div class="flex flex-col gap-3">
              <div v-for="(item, i) in benefits" :key="i" class="flex items-start gap-3">
                <component :is="item.icon" :class="[item.color, 'mt-0.5 shrink-0 w-[18px] h-[18px]']" />
                <p class="text-sm text-base-secondary">{{ item.text }}</p>
              </div>
            </div>
          </div>

          <!-- Portal mockup right -->
          <div class="reveal-on-scroll fade-left">
            <div class="bg-card-base border border-base rounded-2xl shadow-card-base overflow-hidden">
              <!-- Header bar -->
              <div class="gradient-primary px-5 py-4 flex items-center gap-3">
                <div class="w-8 h-8 rounded-lg bg-white/20 flex items-center justify-center">
                  <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z"/>
                    <path d="M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z"/>
                  </svg>
                </div>
                <div>
                  <p class="text-white font-semibold text-sm">Cổng học viên EduCenter</p>
                  <p class="text-white/70 text-xs">Xin chào, Học viên!</p>
                </div>
                <div class="ml-auto w-7 h-7 rounded-full bg-white/20 overflow-hidden">
                  <img src="https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=28&h=28&fit=crop&crop=face" alt="avatar" class="w-full h-full object-cover" />
                </div>
              </div>

              <div class="p-5 space-y-5">
                <!-- Stat cards -->
                <div class="grid grid-cols-2 gap-3">
                  <div class="bg-section rounded-xl p-3">
                    <div class="flex items-center gap-1.5 mb-1">
                      <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="green" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-green-500">
                        <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                        <polyline points="22 4 12 14.01 9 11.01"/>
                      </svg>
                      <span class="text-xs text-base-muted">Điểm danh</span>
                    </div>
                    <p class="text-xl font-bold text-base-primary">{{ data.attendance }}%</p>
                  </div>
                  <div class="bg-section rounded-xl p-3">
                    <div class="flex items-center gap-1.5 mb-1">
                      <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-blue-500">
                        <line x1="18" y1="20" x2="18" y2="10"/>
                        <line x1="12" y1="20" x2="12" y2="4"/>
                        <line x1="6" y1="20" x2="6" y2="14"/>
                      </svg>
                      <span class="text-xs text-base-muted">Điểm gần nhất</span>
                    </div>
                    <p class="text-xl font-bold text-base-primary">{{ data.recentScore }}/10</p>
                  </div>
                  <div class="bg-section rounded-xl p-3">
                    <div class="flex items-center gap-1.5 mb-1">
                      <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-teal-500">
                        <circle cx="12" cy="12" r="10"/>
                        <polyline points="12 6 12 12 16 14"/>
                      </svg>
                      <span class="text-xs text-base-muted">Buổi đã học</span>
                    </div>
                    <p class="text-xl font-bold text-base-primary">{{ data.completedSessions }}/{{ data.totalSessions }}</p>
                  </div>
                  <div class="bg-section rounded-xl p-3">
                    <div class="flex items-center gap-1.5 mb-1">
                      <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-indigo-500">
                        <rect x="1" y="4" width="22" height="16" rx="2" ry="2"/>
                        <line x1="1" y1="10" x2="23" y2="10"/>
                      </svg>
                      <span class="text-xs text-base-muted">Học phí</span>
                    </div>
                    <p class="text-sm font-bold text-base-primary">{{ formatPrice(data.paidAmount) }}</p>
                  </div>
                </div>

                <!-- Current courses -->
                <div>
                  <p class="text-xs font-semibold text-base-muted uppercase tracking-wide mb-3">Khóa học đang học</p>
                  <div class="space-y-3">
                    <div v-for="c in data.currentCourses" :key="c.id" class="bg-section rounded-xl p-3">
                      <div class="flex items-start justify-between mb-2">
                        <p class="text-sm font-semibold text-base-primary leading-snug pr-2">{{ c.title }}</p>
                        <span class="text-xs font-bold text-blue-600 dark:text-blue-400 shrink-0">{{ c.progress }}%</span>
                      </div>
                      <ProgressBar
                        :value="c.progress"
                        color-class="bg-gradient-to-r from-blue-500 to-teal-500"
                      />
                      <div class="flex items-center gap-1 mt-2 text-xs text-base-muted">
                        <svg xmlns="http://www.w3.org/2000/svg" width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                          <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
                          <line x1="16" y1="2" x2="16" y2="6"/>
                          <line x1="8" y1="2" x2="8" y2="6"/>
                          <line x1="3" y1="10" x2="21" y2="10"/>
                        </svg>
                        <span>Buổi tới: <span class="font-medium text-base-secondary">{{ c.nextSession }}</span></span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { h } from 'vue'
import { studentPortalMockData } from '../data/mockData'
import ProgressBar from './ProgressBar.vue'
import { useReveal } from '../composables/useReveal'

useReveal()

const data = studentPortalMockData

const bookOpenSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z' }),
    h('path', { d: 'M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z' })
  ])
}
const calendarSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('rect', { x: '3', y: '4', width: '18', height: '18', rx: '2', ry: '2' }),
    h('line', { x1: '16', y1: '2', x2: '16', y2: '6' }),
    h('line', { x1: '8', y1: '2', x2: '8', y2: '6' }),
    h('line', { x1: '3', y1: '10', x2: '21', y2: '10' })
  ])
}
const checkCircleSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('circle', { cx: '12', cy: '12', r: '10' }),
    h('path', { d: 'm9 12 2 2 4-4' })
  ])
}
const barChartSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('line', { x1: '18', y1: '20', x2: '18', y2: '10' }),
    h('line', { x1: '12', y1: '20', x2: '12', y2: '4' }),
    h('line', { x1: '6', y1: '20', x2: '6', y2: '14' })
  ])
}

const benefits = [
  { icon: bookOpenSvg, text: 'Xem tiến độ từng khóa học theo thời gian thực', color: 'text-blue-500' },
  { icon: calendarSvg, text: 'Xem lịch học và thông báo buổi học sắp tới', color: 'text-teal-500' },
  { icon: checkCircleSvg, text: 'Theo dõi tỷ lệ điểm danh và buổi vắng', color: 'text-green-500' },
  { icon: barChartSvg, text: 'Xem kết quả bài kiểm tra và tiến bộ theo tháng', color: 'text-indigo-500' }
]

function formatPrice(p) {
  if (p === undefined || p === null) return '0đ'
  return p.toLocaleString('vi-VN') + 'đ'
}
</script>
