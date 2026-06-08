<template>
  <section id="teachers" class="py-20 bg-section">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="reveal-on-scroll fade-up flex flex-col gap-6">
        <div class="text-center mb-12">
          <span class="inline-block text-xs font-semibold text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-950/40 px-3 py-1 rounded-full mb-3">
            Đội ngũ chuyên gia
          </span>
          <h2 class="text-3xl sm:text-4xl font-bold text-base-primary mb-3">
            Đội ngũ giảng viên
          </h2>
          <p class="text-base-secondary max-w-xl mx-auto">
            Tất cả giảng viên tại EduCenter đều có kinh nghiệm thực chiến tại các công ty công nghệ hàng đầu.
          </p>
        </div>

        <div class="grid sm:grid-cols-2 lg:grid-cols-4 gap-6">
          <div
            v-for="teacher in teachers"
            :key="teacher.id"
            class="bg-card-base border border-card-base hover:border-blue-200 dark:hover:border-blue-800 rounded-2xl p-6 shadow-card-base hover:shadow-md transition-all duration-300 group text-center flex flex-col items-center gap-4"
          >
            <!-- Avatar -->
            <div class="relative">
              <div class="w-20 h-20 rounded-2xl overflow-hidden border-2 border-blue-100 dark:border-blue-900 group-hover:border-blue-300 dark:group-hover:border-blue-700 transition-colors duration-300">
                <img
                  :src="teacher.avatar"
                  :alt="teacher.name"
                  class="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500"
                  @error="handleAvatarError($event, teacher.name)"
                />
              </div>
              <!-- Category badge -->
              <div class="absolute -bottom-2 left-1/2 -translate-x-1/2 bg-blue-600 text-white text-[10px] font-semibold px-2 py-0.5 rounded-full whitespace-nowrap">
                {{ teacher.category }}
              </div>
            </div>

            <!-- Name & specialty -->
            <div class="pt-2">
              <h3 class="font-bold text-base text-base-primary group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
                {{ teacher.name }}
              </h3>
              <p class="text-sm text-base-muted mt-0.5">{{ teacher.specialty }}</p>
            </div>

            <!-- Rating -->
            <RatingStars :rating="teacher.rating" :size="13" />

            <!-- Stats -->
            <div class="w-full grid grid-cols-3 gap-2 text-center">
              <div class="bg-section rounded-xl py-2">
                <div class="flex items-center justify-center text-blue-500 mb-0.5">
                  <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <rect x="2" y="7" width="20" height="14" rx="2" ry="2"/>
                    <path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/>
                  </svg>
                </div>
                <p class="text-sm font-bold text-base-primary">{{ teacher.experience }}</p>
                <p class="text-[10px] text-base-muted">năm KN</p>
              </div>
              <div class="bg-section rounded-xl py-2">
                <div class="flex items-center justify-center text-teal-500 mb-0.5">
                  <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20"/>
                    <path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z"/>
                  </svg>
                </div>
                <p class="text-sm font-bold text-base-primary">{{ teacher.courses }}</p>
                <p class="text-[10px] text-base-muted">khóa học</p>
              </div>
              <div class="bg-section rounded-xl py-2">
                <div class="flex items-center justify-center text-indigo-500 mb-0.5">
                  <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
                    <circle cx="9" cy="7" r="4"/>
                    <path d="M23 21v-2a4 4 0 0 0-3-3.87"/>
                    <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
                  </svg>
                </div>
                <p class="text-sm font-bold text-base-primary">{{ teacher.students }}</p>
                <p class="text-[10px] text-base-muted">học viên</p>
              </div>
            </div>

            <!-- Description -->
            <p class="text-xs text-base-secondary leading-relaxed text-center line-clamp-3">
              {{ teacher.description }}
            </p>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { teachers } from '../data/mockData'
import RatingStars from './RatingStars.vue'
import { useReveal } from '../composables/useReveal'

useReveal()

function handleAvatarError(e, name) {
  e.target.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=3b82f6&color=fff&size=80`
}
</script>
