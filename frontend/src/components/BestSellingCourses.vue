<template>
  <section id="best-selling" class="py-20 bg-page">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="reveal-on-scroll fade-up flex flex-col gap-6">
        <div class="text-center mb-12">
          <span class="inline-block text-xs font-semibold text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-950/40 px-3 py-1 rounded-full mb-3">
            🔥 Được yêu thích nhất
          </span>
          <h2 class="text-3xl sm:text-4xl font-bold text-base-primary mb-3">
            Khóa học bán chạy
          </h2>
          <p class="text-base-secondary max-w-xl mx-auto">
            Những khóa học được hàng trăm học viên lựa chọn và đánh giá cao nhất tại EduCenter.
          </p>
        </div>

        <div class="grid sm:grid-cols-2 lg:grid-cols-4 gap-6">
          <CourseCard
            v-for="course in bestSelling"
            :key="course.id"
            :course="course"
            variant="best"
          />
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { computed } from 'vue'
import { courses } from '../data/mockData'
import CourseCard from './CourseCard.vue'
import { useReveal } from '../composables/useReveal'

useReveal()

const bestSelling = computed(() => courses.filter(c => c.bestSelling))
</script>
