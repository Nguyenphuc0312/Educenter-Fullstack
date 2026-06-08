<template>
  <section id="courses" class="py-20 bg-section">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="reveal-on-scroll fade-up flex flex-col gap-6">
        <div class="text-center mb-12">
          <span class="inline-block text-xs font-semibold text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-950/40 px-3 py-1 rounded-full mb-3">
            Đang mở bán
          </span>
          <h2 class="text-3xl sm:text-4xl font-bold text-base-primary mb-3">
            Khóa học đang mở bán
          </h2>
          <p class="text-base-secondary max-w-xl mx-auto">
            Các khóa học chất lượng cao đang được khai giảng, phù hợp cho mọi trình độ từ người mới bắt đầu đến chuyên nghiệp.
          </p>
        </div>

        <div class="grid sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          <CourseCard
            v-for="course in courses"
            :key="course.id"
            :course="course"
          />
        </div>

        <div class="text-center mt-10">
          <button
            id="btn-view-all-courses"
            @click="router.push('/courses')"
            class="px-8 py-3 text-sm font-semibold border-2 border-blue-500 dark:border-blue-600 text-blue-600 dark:text-blue-400 rounded-xl hover:bg-blue-50 dark:hover:bg-blue-950/40 transition-all cursor-pointer transform hover:scale-[1.03] active:scale-[0.97]"
          >
            Xem tất cả khóa học
          </button>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { courses } from '../data/mockData'
import CourseCard from './CourseCard.vue'
import { useReveal } from '../composables/useReveal'

useReveal()

const router = useRouter()
</script>
