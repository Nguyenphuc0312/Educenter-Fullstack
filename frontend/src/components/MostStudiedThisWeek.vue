<template>
  <section class="py-20 bg-section">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="reveal-on-scroll fade-up flex flex-col gap-6">
        <div class="text-center mb-12">
          <span class="inline-block text-xs font-semibold text-teal-600 dark:text-teal-400 bg-teal-50 dark:bg-teal-950/40 px-3 py-1 rounded-full mb-3">
            📈 Xu hướng tuần này
          </span>
          <h2 class="text-3xl sm:text-4xl font-bold text-base-primary mb-3">
            Được học nhiều nhất tuần này
          </h2>
          <p class="text-base-secondary max-w-xl mx-auto">
            Bảng xếp hạng các khóa học được học viên tham gia nhiều nhất trong 7 ngày qua.
          </p>
        </div>

        <div class="grid md:grid-cols-2 gap-4">
          <div
            v-for="(course, idx) in weekly"
            :key="course.id"
            class="flex items-center gap-4 bg-card-base border border-card-base hover:border-blue-200 dark:hover:border-blue-800 rounded-2xl p-4 shadow-card-base hover:shadow-md transition-all duration-300 group"
          >
            <!-- Rank -->
            <div :class="['w-9 h-9 rounded-xl flex items-center justify-center text-sm font-bold shrink-0', rankColors[idx] ?? 'bg-slate-100 dark:bg-slate-700 text-base-secondary']">
              {{ idx + 1 }}
            </div>

            <!-- Image -->
            <img
              :src="course.image"
              :alt="course.title"
              class="w-14 h-14 rounded-xl object-cover shrink-0 group-hover:scale-105 transition-transform duration-300"
              @error="handleImgError"
            />

            <!-- Info -->
            <div class="flex-1 min-w-0">
              <h3 class="font-semibold text-sm text-base-primary leading-snug mb-1 line-clamp-1 group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
                {{ course.title }}
              </h3>

              <div class="flex items-center gap-3 text-xs text-base-muted mb-2">
                <span class="flex items-center gap-1">
                  <svg xmlns="http://www.w3.org/2000/svg" width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/>
                    <circle cx="9" cy="7" r="4"/>
                    <path d="M22 21v-2a4 4 0 0 0-3-3.87"/>
                    <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
                  </svg>
                  <span class="font-semibold text-teal-600 dark:text-teal-400">{{ course.weeklyStudents }}</span> HV/tuần
                </span>
                <span class="flex items-center gap-1">
                  <svg xmlns="http://www.w3.org/2000/svg" width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="green" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-green-500">
                    <polyline points="20 6 9 17 4 12"/>
                  </svg>
                  {{ course.completionRate }}% hoàn thành
                </span>
              </div>

              <div class="flex items-center gap-2">
                <ProgressBar
                  :value="course.completionRate"
                  color-class="bg-gradient-to-r from-teal-400 to-blue-500"
                  class="flex-1"
                />
                <RatingStars :rating="course.rating" :size="11" :show-value="false" />
                <span class="text-xs font-semibold text-amber-500">{{ course.rating }}</span>
              </div>
            </div>

            <!-- Trending icon -->
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-teal-500 shrink-0 opacity-60 group-hover:opacity-100 transition-opacity">
              <polyline points="23 6 13.5 15.5 8.5 10.5 1 18"/>
              <polyline points="17 6 23 6 23 12"/>
            </svg>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { computed } from 'vue'
import { courses } from '../data/mockData'
import ProgressBar from './ProgressBar.vue'
import RatingStars from './RatingStars.vue'
import { useReveal } from '../composables/useReveal'

useReveal()

const weekly = computed(() => {
  return [...courses]
    .sort((a, b) => a.weeklyRank - b.weeklyRank)
    .slice(0, 6)
})

const rankColors = [
  'bg-amber-400 text-white',
  'bg-slate-400 text-white',
  'bg-orange-400 text-white'
]

function handleImgError(e) {
  e.target.src = 'https://placehold.co/56x56/e2e8f0/94a3b8?text=Course'
}
</script>
