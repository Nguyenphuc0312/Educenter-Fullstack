<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
    <!-- Header -->
    <div class="text-center max-w-2xl mx-auto mb-10">
      <div
        class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-blue-50 dark:bg-blue-950/40 text-blue-600 dark:text-blue-400 text-xs font-semibold mb-3"
      >
        <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z"/>
          <path d="M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z"/>
        </svg>
        Lộ trình học tập
      </div>
      <h1 class="text-3xl font-extrabold text-base-primary tracking-tight">Danh sách khóa học</h1>
      <p class="text-sm text-base-secondary mt-2">
        Lựa chọn khóa học phù hợp nhất với mục tiêu lập trình của bạn
      </p>
    </div>

    <!-- Search & Filters -->
    <div class="bg-card-base border border-base rounded-2xl p-6 mb-8 shadow-sm space-y-4">
      <div class="flex items-center gap-2 text-xs font-semibold text-base-primary pb-2 border-b border-base">
        <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <line x1="4" y1="21" x2="4" y2="14"/>
          <line x1="4" y1="10" x2="4" y2="3"/>
          <line x1="12" y1="21" x2="12" y2="12"/>
          <line x1="12" y1="8" x2="12" y2="3"/>
          <line x1="20" y1="21" x2="20" y2="16"/>
          <line x1="20" y1="12" x2="20" y2="3"/>
          <line x1="1" y1="14" x2="7" y2="14"/>
          <line x1="9" y1="8" x2="15" y2="8"/>
          <line x1="17" y1="16" x2="23" y2="16"/>
        </svg>
        <span>Bộ lọc tìm kiếm</span>
      </div>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <!-- Search Input -->
        <div class="relative">
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-base-muted">
            <circle cx="11" cy="11" r="8"/>
            <line x1="21" y1="21" x2="16.65" y2="16.65"/>
          </svg>
          <input
            type="text"
            placeholder="Tìm kiếm khóa học..."
            v-model="searchTerm"
            class="w-full pl-10 pr-4 py-2.5 rounded-xl border border-base bg-page text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 text-base-primary"
          />
        </div>

        <!-- Category Dropdown -->
        <select
          v-model="selectedCategory"
          class="w-full px-4 py-2.5 rounded-xl border border-base bg-page text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 text-base-primary"
        >
          <option value="All">Tất cả danh mục</option>
          <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
        </select>

        <!-- Level Dropdown -->
        <select
          v-model="selectedLevel"
          class="w-full px-4 py-2.5 rounded-xl border border-base bg-page text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 text-base-primary"
        >
          <option value="All">Tất cả cấp độ</option>
          <option v-for="lvl in levels" :key="lvl" :value="lvl">{{ lvl }}</option>
        </select>
      </div>
    </div>

    <!-- Courses List -->
    <div v-if="isLoading" class="flex justify-center items-center py-20">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500" />
    </div>

    <div v-else-if="filteredCourses.length === 0" class="text-center py-16 bg-card-base border border-base border-dashed rounded-2xl p-8">
      <p class="text-sm text-base-secondary">Không tìm thấy khóa học nào phù hợp với bộ lọc của bạn.</p>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <CourseCard v-for="course in filteredCourses" :key="course.id" :course="course" />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { courseApi } from '../../api/courseApi'
import { classApi } from '../../api/classApi'
import { courses as mockCourses } from '../../data/mockData'
import CourseCard from '../../components/CourseCard.vue'
import { normalizeClass, normalizeCourse } from '../../lib/courseMapper'

const route = useRoute()

const courses = ref([])
const isLoading = ref(true)
const error = ref(null)

const searchTerm = ref('')
const selectedCategory = ref('All')
const selectedLevel = ref('All')

onMounted(() => {
  // Sync query params from router if navigated from home search
  if (route.query.search) {
    searchTerm.value = route.query.search
  }
  if (route.query.category) {
    selectedCategory.value = route.query.category
  }
  fetchCourses()
})

async function fetchCourses() {
  isLoading.value = true
  error.value = null
  try {
    const [courseData, classData] = await Promise.all([
      courseApi.getAll(),
      classApi.getOpening().catch(() => [])
    ])
    const normalizedClasses = (classData || []).map(normalizeClass)
    courses.value = (courseData || []).map(course => normalizeCourse(course, normalizedClasses))
  } catch (err) {
    console.warn("Failed to fetch courses from API, falling back to mock data:", err)
    courses.value = mockCourses.map(course => normalizeCourse(course))
  } finally {
    isLoading.value = false
  }
}

const categories = computed(() => {
  const allCats = courses.value.map(c => c.category).filter(Boolean)
  return [...new Set(allCats)]
})

const levels = computed(() => {
  const allLvls = courses.value.map(c => c.level).filter(Boolean)
  return [...new Set(allLvls)]
})

const filteredCourses = computed(() => {
  return courses.value.filter(course => {
    const matchesSearch =
      !searchTerm.value ||
      (course.title || '').toLowerCase().includes(searchTerm.value.toLowerCase()) ||
      (course.description && course.description.toLowerCase().includes(searchTerm.value.toLowerCase()))
    
    const matchesCategory =
      selectedCategory.value === 'All' ||
      course.category === selectedCategory.value

    const matchesLevel =
      selectedLevel.value === 'All' ||
      course.level === selectedLevel.value

    return matchesSearch && matchesCategory && matchesLevel
  })
})
</script>
