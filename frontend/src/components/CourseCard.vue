<template>
  <div
    class="group bg-card-base border border-card-base hover:border-blue-200 dark:hover:border-blue-800 rounded-2xl overflow-hidden shadow-card-base hover:shadow-md transition-all duration-300 flex flex-col"
  >
    <div class="relative overflow-hidden aspect-[16/9]">
      <img
        :src="course.image"
        :alt="course.title"
        class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
        @error="handleImgError"
      />
      <div class="absolute inset-0 bg-gradient-to-t from-black/30 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
      <div class="absolute top-3 left-3">
        <StatusBadge :status="course.status" />
      </div>
      <div
        v-if="variant === 'best'"
        class="absolute top-3 right-3 bg-amber-500 text-white text-xs font-bold px-2.5 py-1 rounded-full flex items-center gap-1"
      >
        Bán chạy
      </div>
      <div
        v-if="discount > 0"
        class="absolute bottom-3 right-3 bg-red-500 text-white text-xs font-bold px-2 py-0.5 rounded-md"
      >
        -{{ discount }}%
      </div>
    </div>

    <div class="p-4 flex flex-col flex-1 gap-3">
      <div class="flex items-center gap-2 flex-wrap">
        <span class="text-xs font-semibold text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-950/40 px-2.5 py-0.5 rounded-full">
          {{ course.category }}
        </span>
        <span :class="['text-xs font-semibold px-2.5 py-0.5 rounded-full', levelColor(course.level)]">
          {{ course.level }}
        </span>
      </div>

      <h3 class="font-bold text-base text-base-primary leading-snug line-clamp-2 group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
        {{ course.title }}
      </h3>

      <p class="text-sm text-base-secondary line-clamp-2 leading-relaxed">{{ course.description }}</p>

      <p class="text-xs text-base-muted font-medium">
        Giảng viên: <span class="text-base-secondary font-semibold">{{ course.teacher }}</span>
      </p>

      <div class="flex items-center gap-4 text-xs text-base-muted">
        <span class="flex items-center gap-1">
          <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"/>
            <polyline points="12 6 12 12 16 14"/>
          </svg>
          {{ course.sessions }} buổi
        </span>
        <span class="flex items-center gap-1">
          <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/>
            <circle cx="9" cy="7" r="4"/>
            <path d="M22 21v-2a4 4 0 0 0-3-3.87"/>
            <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
          </svg>
          {{ (course.students || 0).toLocaleString() }} HV
        </span>
      </div>

      <RatingStars :rating="course.rating || 5" />

      <div class="flex items-baseline gap-2 mt-auto">
        <span class="text-lg font-bold text-blue-600 dark:text-blue-400">{{ formatPrice(course.price) }}</span>
        <span v-if="course.originalPrice > course.price" class="text-xs text-base-muted line-through">
          {{ formatPrice(course.originalPrice) }}
        </span>
      </div>

      <div class="grid grid-cols-2 gap-2 mt-1">
        <router-link
          :id="`btn-detail-${course.id}`"
          :to="`/courses/${slug}`"
          class="py-2 text-sm text-center font-semibold rounded-xl border border-base text-base-secondary hover:border-blue-300 dark:hover:border-blue-700 hover:text-blue-600 dark:hover:text-blue-400 transition-all"
        >
          Xem chi tiết
        </router-link>
        <router-link
          :id="`btn-register-${course.id}`"
          :to="`/courses/${slug}`"
          class="py-2 text-sm text-center font-semibold rounded-xl gradient-primary text-white hover:opacity-90 transition-opacity flex items-center justify-center gap-1"
        >
          Đăng ký
          <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <line x1="5" y1="12" x2="19" y2="12"/>
            <polyline points="12 5 19 12 12 19"/>
          </svg>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import RatingStars from './RatingStars.vue'
import StatusBadge from './StatusBadge.vue'

const props = defineProps({
  course: {
    type: Object,
    required: true
  },
  variant: {
    type: String,
    default: 'default'
  }
})

const discount = computed(() => {
  if (!props.course.originalPrice || !props.course.price) return 0
  return Math.round((1 - props.course.price / props.course.originalPrice) * 100)
})

const slug = computed(() => {
  if (props.course.slug) return props.course.slug
  return props.course.title
    .toLowerCase()
    .replace(/ /g, '-')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
})

function formatPrice(p) {
  if (p === undefined || p === null) return '0đ'
  return p.toLocaleString('vi-VN') + 'đ'
}

function levelColor(level) {
  if (level === 'Cơ bản' || level === 'Beginner') return 'bg-green-50 dark:bg-green-950/40 text-green-700 dark:text-green-400'
  if (level === 'Trung cấp' || level === 'Intermediate') return 'bg-blue-50 dark:bg-blue-950/40 text-blue-700 dark:text-blue-400'
  return 'bg-purple-50 dark:bg-purple-950/40 text-purple-700 dark:text-purple-400'
}

function handleImgError(e) {
  e.target.src = `https://placehold.co/600x400/e2e8f0/94a3b8?text=${encodeURIComponent(props.course.title)}`
}
</script>
