<template>
  <section class="py-14 bg-page">
    <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="reveal-on-scroll fade-up flex flex-col gap-6">
        <div>
          <h2 class="text-2xl font-bold text-center text-base-primary mb-2">
            Tìm khóa học phù hợp với bạn
          </h2>
          <p class="text-center text-base-secondary text-sm mb-8">
            Hơn 20 khóa học đa dạng, từ cơ bản đến nâng cao
          </p>
        </div>

        <!-- Search bar -->
        <div>
          <div class="flex gap-3 mb-6">
            <div class="flex-1 relative">
              <svg xmlns="http://www.w3.org/2000/svg" width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-base-muted">
                <circle cx="11" cy="11" r="8"/>
                <line x1="21" y1="21" x2="16.65" y2="16.65"/>
              </svg>
              <input
                id="course-search-input"
                type="text"
                v-model="query"
                @keydown.enter="handleSearch"
                placeholder="Bạn muốn học gì? (React, SQL, Excel...)"
                class="w-full pl-10 pr-4 py-3 rounded-xl border border-base bg-card-base text-base-primary placeholder:text-base-muted text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 dark:focus:border-blue-600 transition-all"
              />
            </div>
            <button
              id="btn-search-courses"
              @click="handleSearch"
              class="px-5 py-3 gradient-primary text-white text-sm font-semibold rounded-xl hover:opacity-90 transition-all cursor-pointer transform hover:scale-[1.03] active:scale-[0.97] whitespace-nowrap"
            >
              Tìm khóa học
            </button>
          </div>
        </div>

        <!-- Category chips -->
        <div>
          <div class="flex flex-wrap gap-2 justify-center">
            <button
              @click="setActiveCategory(null)"
              class="px-4 py-2 rounded-full text-sm font-medium transition-all duration-200 cursor-pointer"
              :class="activeCategory === null
                ? 'gradient-primary text-white'
                : 'bg-section text-base-secondary border border-base hover:border-blue-300 dark:hover:border-blue-700 hover:text-blue-600 dark:hover:text-blue-400'"
            >
              Tất cả
            </button>
            <button
              v-for="cat in categories"
              :key="cat.id"
              :id="`cat-${cat.id}`"
              @click="setActiveCategory(cat.label === activeCategory ? null : cat.label)"
              class="px-4 py-2 rounded-full text-sm font-medium transition-all duration-200 cursor-pointer transform active:scale-95"
              :class="activeCategory === cat.label
                ? 'gradient-primary text-white'
                : 'bg-section text-base-secondary border border-base hover:border-blue-300 dark:hover:border-blue-700 hover:text-blue-600 dark:hover:text-blue-400'"
            >
              {{ cat.label }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { categories } from '../data/mockData'
import { useReveal } from '../composables/useReveal'

useReveal()

const props = defineProps({
  onSearch: {
    type: Function,
    default: null
  }
})

const query = ref('')
const activeCategory = ref(null)
const router = useRouter()

function handleSearch() {
  if (props.onSearch) {
    props.onSearch({ query: query.value, category: activeCategory.value })
  } else {
    // Navigate to courses page with query parameters
    router.push({
      path: '/courses',
      query: {
        search: query.value || undefined,
        category: activeCategory.value || undefined
      }
    }).then(() => {
      setTimeout(() => {
        const el = document.getElementById('courses')
        if (el) el.scrollIntoView({ behavior: 'smooth' })
      }, 200)
    })
  }
}

function setActiveCategory(catName) {
  activeCategory.value = catName
}
</script>
