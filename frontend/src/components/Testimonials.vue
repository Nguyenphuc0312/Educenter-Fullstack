<template>
  <section id="testimonials" class="py-20 bg-page">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="reveal-on-scroll fade-up flex flex-col gap-6">
        <div class="text-center mb-12">
          <span class="inline-block text-xs font-semibold text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-950/40 px-3 py-1 rounded-full mb-3">
            Đánh giá học viên
          </span>
          <h2 class="text-3xl sm:text-4xl font-bold text-base-primary mb-3">
            Học viên nói gì về EduCenter?
          </h2>
          <p class="text-base-secondary max-w-xl mx-auto">
            Hàng trăm học viên đã tin tưởng và đạt được mục tiêu nghề nghiệp cùng EduCenter.
          </p>
        </div>

        <div class="grid sm:grid-cols-2 lg:grid-cols-4 gap-5">
          <div
            v-for="t in testimonials"
            :key="t.id"
            class="bg-card-base border border-card-base hover:border-blue-200 dark:hover:border-blue-800 rounded-2xl p-5 shadow-card-base hover:shadow-md transition-all duration-300 group flex flex-col gap-4"
          >
            <!-- Quote icon -->
            <div class="w-8 h-8 rounded-lg bg-blue-50 dark:bg-blue-950/40 flex items-center justify-center">
              <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="currentColor" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-blue-500">
                <path d="M16 3a5 5 0 0 0 -4.4 7.6 4.7 4.7 0 0 1 2 .4 4.8 4.8 0 0 1 1.9 3.5c0 .1 0 .2-.1.3A4.8 4.8 0 1 1 16 3zm-10 0a5 5 0 0 0 -4.4 7.6 4.7 4.7 0 0 1 2 .4 4.8 4.8 0 0 1 1.9 3.5c0 .1 0 .2-.1.3A4.8 4.8 0 1 1 6 3z"/>
              </svg>
            </div>

            <!-- Stars -->
            <RatingStars :rating="t.rating" :size="13" :show-value="false" />

            <!-- Content -->
            <p class="text-sm text-base-secondary leading-relaxed flex-1">
              "{{ t.content }}"
            </p>

            <!-- Author -->
            <div class="flex items-center gap-3 pt-2 border-t border-base">
              <img
                :src="t.avatar"
                :alt="t.name"
                class="w-9 h-9 rounded-full object-cover border-2 border-blue-100 dark:border-blue-900"
                @error="handleAvatarError($event, t.name)"
              />
              <div class="min-w-0">
                <p class="text-sm font-semibold text-base-primary">{{ t.name }}</p>
                <p class="text-xs text-base-muted truncate">{{ t.course }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { testimonials } from '../data/mockData'
import RatingStars from './RatingStars.vue'
import { useReveal } from '../composables/useReveal'

useReveal()

function handleAvatarError(e, name) {
  e.target.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=3b82f6&color=fff&size=36`
}
</script>
