<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
    <div class="flex items-center gap-1.5 text-xs text-base-muted mb-6">
      <router-link to="/" class="hover:text-blue-600 dark:hover:text-blue-400">Trang chủ</router-link>
      <span>/</span>
      <router-link to="/courses" class="hover:text-blue-600 dark:hover:text-blue-400">Khóa học</router-link>
      <span>/</span>
      <span class="text-base-secondary font-medium truncate max-w-xs">{{ course?.title }}</span>
    </div>

    <div v-if="isLoading" class="flex justify-center items-center py-20">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500" />
    </div>

    <div v-else-if="error" class="text-center py-16 bg-card-base border border-base rounded-2xl p-8 max-w-xl mx-auto">
      <h3 class="text-lg font-bold text-red-600 mb-2">Lỗi tải khóa học</h3>
      <p class="text-sm text-base-secondary mb-4">{{ error }}</p>
      <button @click="fetchCourseDetails" class="px-5 py-2 gradient-primary text-white text-sm font-semibold rounded-xl">
        Thử lại
      </button>
    </div>

    <div v-else-if="course" class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      <div class="lg:col-span-2 space-y-8">
        <div>
          <div class="flex flex-wrap gap-2 mb-3">
            <span class="text-xs font-semibold text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-950/40 px-2.5 py-1 rounded-full">
              {{ course.category }}
            </span>
            <span class="text-xs font-semibold text-purple-700 dark:text-purple-300 bg-purple-50 dark:bg-purple-950/40 px-2.5 py-1 rounded-full">
              {{ course.level }}
            </span>
          </div>
          <h1 class="text-3xl font-extrabold text-base-primary tracking-tight leading-snug">{{ course.title }}</h1>
          <p class="text-sm text-base-secondary mt-3 leading-relaxed">{{ course.description }}</p>
        </div>

        <div class="relative aspect-[16/9] rounded-2xl overflow-hidden border border-base bg-slate-100 dark:bg-slate-900">
          <img :src="course.image" :alt="course.title" class="w-full h-full object-cover" @error="handleImgError" />
        </div>

        <div class="bg-card-base border border-base rounded-2xl p-6">
          <h2 class="text-lg font-bold text-base-primary mb-4">Nội dung khóa học</h2>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div v-for="item in overviewItems" :key="item.title" class="flex items-center gap-3">
              <div class="w-9 h-9 rounded-xl bg-blue-50 dark:bg-blue-950/40 text-blue-600 dark:text-blue-400 flex items-center justify-center font-bold">
                {{ item.index }}
              </div>
              <div>
                <p class="text-xs font-semibold text-base-primary">{{ item.title }}</p>
                <p class="text-xs text-base-secondary">{{ item.value }}</p>
              </div>
            </div>
          </div>
        </div>

        <div class="space-y-4">
          <div class="flex items-center justify-between gap-3">
            <h2 class="text-lg font-bold text-base-primary">Lớp học đang tuyển sinh</h2>
            <span class="text-xs text-base-muted">{{ classes.length }} lớp mở</span>
          </div>

          <div v-if="classes.length === 0" class="p-6 bg-card-base border border-base border-dashed rounded-2xl text-center">
            <p class="text-sm text-base-secondary">Hiện tại khóa học này chưa có lớp mở tuyển sinh.</p>
          </div>

          <div v-else class="space-y-4">
            <div
              v-for="cls in classes"
              :key="cls.id"
              class="bg-card-base border border-base rounded-2xl p-5 shadow-sm flex flex-col md:flex-row md:items-center justify-between gap-4"
            >
              <div class="space-y-2 min-w-0">
                <div class="flex items-center gap-2">
                  <span class="text-xs font-bold text-blue-600 dark:text-blue-400 border border-blue-200 dark:border-blue-800 bg-blue-50 dark:bg-blue-950/40 px-2 py-0.5 rounded-lg">
                    {{ cls.classCode }}
                  </span>
                  <h3 class="text-sm font-bold text-base-primary truncate">{{ cls.className }}</h3>
                </div>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-x-6 gap-y-1 text-xs text-base-secondary">
                  <span>Giảng viên: {{ cls.teacherName }}</span>
                  <span>Sĩ số: {{ cls.currentStudents }}/{{ cls.maxStudents }} học viên</span>
                  <span>Phòng: {{ cls.room || 'Đang cập nhật' }}</span>
                  <span>Khai giảng: {{ formatDate(cls.startDate) }}</span>
                </div>
              </div>

              <div class="flex items-center justify-between md:justify-end gap-3 pt-3 md:pt-0 border-t md:border-t-0 border-base shrink-0">
                <span class="text-xs font-semibold text-emerald-700 dark:text-emerald-300 bg-emerald-50 dark:bg-emerald-950/40 px-2.5 py-1 rounded-lg">
                  Đang mở
                </span>
                <button
                  :disabled="isEnrolling || cls.currentStudents >= cls.maxStudents"
                  @click="handleEnroll(cls.id)"
                  class="px-4 py-2 text-xs font-bold text-white gradient-primary rounded-xl hover:opacity-95 disabled:opacity-50 transition-all"
                >
                  {{ isEnrolling ? 'Đang xử lý...' : 'Đăng ký lớp' }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="space-y-6">
        <div class="bg-card-base border border-base rounded-2xl p-6 shadow-sm space-y-6">
          <div>
            <p class="text-xs text-base-muted uppercase font-bold tracking-wider mb-1">Học phí khóa học</p>
            <div class="flex items-baseline gap-2">
              <span class="text-3xl font-extrabold text-blue-600 dark:text-blue-400">{{ formatVnd(course.price) }}</span>
              <span v-if="course.originalPrice > course.price" class="text-sm text-base-muted line-through">
                {{ formatVnd(course.originalPrice) }}
              </span>
            </div>
          </div>

          <div class="border-t border-base pt-4 space-y-3 text-xs text-base-secondary">
            <div class="flex justify-between gap-4">
              <span>Cấp độ:</span>
              <span class="font-bold text-base-primary">{{ course.level }}</span>
            </div>
            <div class="flex justify-between gap-4">
              <span>Thời gian:</span>
              <span class="font-bold text-base-primary">{{ course.sessions }} buổi học</span>
            </div>
            <div class="flex justify-between gap-4">
              <span>Lớp gần nhất:</span>
              <span class="font-bold text-base-primary">{{ formatDate(classes[0]?.startDate) || 'Sắp mở' }}</span>
            </div>
          </div>

          <button
            v-if="classes[0]"
            @click="handleEnroll(classes[0].id)"
            class="w-full py-3 text-center font-bold text-white gradient-primary rounded-xl hover:opacity-90 transition-opacity block shadow-lg shadow-blue-500/10"
          >
            Đăng ký lớp gần nhất
          </button>
        </div>

        <div v-if="teacher" class="bg-card-base border border-base rounded-2xl p-6 shadow-sm">
          <h3 class="text-sm font-bold text-base-primary mb-4">Giảng viên phụ trách</h3>
          <div class="flex items-center gap-3 mb-3">
            <img :src="teacher.avatar" :alt="teacher.name" class="w-12 h-12 rounded-xl object-cover" />
            <div>
              <h4 class="text-xs font-bold text-base-primary">{{ teacher.name }}</h4>
              <p class="text-[10px] text-base-muted">{{ teacher.specialty }}</p>
            </div>
          </div>
          <p class="text-xs text-base-secondary leading-relaxed mb-2">{{ teacher.description }}</p>
          <div class="flex gap-4 text-[10px] text-base-muted">
            <span>{{ teacher.rating }} rating</span>
            <span>{{ teacher.experience }} năm KN</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { message } from 'ant-design-vue'
import { courseApi } from '../../api/courseApi'
import { classApi } from '../../api/classApi'
import { enrollmentApi } from '../../api/enrollmentApi'
import { useAuthStore } from '../../stores/auth'
import { courses as mockCourses, teachers as mockTeachers } from '../../data/mockData'
import { formatVnd } from '../../lib/formatters'
import { roleEquals } from '../../lib/constants'
import { normalizeClass, normalizeCourse } from '../../lib/courseMapper'

const props = defineProps({
  slug: {
    type: String,
    required: true
  }
})

const router = useRouter()
const authStore = useAuthStore()

const course = ref(null)
const classes = ref([])
const teacher = ref(null)
const isLoading = ref(true)
const error = ref(null)
const isEnrolling = ref(false)

const overviewItems = computed(() => [
  { index: 1, title: 'Tổng số buổi', value: `${course.value?.sessions || 0} buổi học thực chiến` },
  { index: 2, title: 'Cấp độ', value: course.value?.level || 'Beginner' },
  { index: 3, title: 'Chứng chỉ', value: 'Cấp chứng chỉ sau khi hoàn thành' },
  { index: 4, title: 'Hỗ trợ', value: 'Theo dõi tiến độ và tư vấn học tập' },
])

onMounted(fetchCourseDetails)
watch(() => props.slug, fetchCourseDetails)

function slugOf(value) {
  return String(value || '')
    .toLowerCase()
    .replace(/ /g, '-')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
}

async function fetchCourseDetails() {
  isLoading.value = true
  error.value = null
  try {
    let courseData = null
    try {
      courseData = await courseApi.getBySlug(props.slug)
    } catch (err) {
      console.warn('API getBySlug failed, finding in mock data...', err)
    }

    if (!courseData) {
      courseData = mockCourses.find(c => (c.slug || slugOf(c.title)) === props.slug)
    }

    if (!courseData) {
      throw new Error('Không tìm thấy khóa học yêu cầu.')
    }

    let classData = []
    try {
      classData = await classApi.getByCourse(courseData.id)
    } catch (classErr) {
      console.warn('Failed to fetch classes by course, using opening classes:', classErr)
      const openingClasses = await classApi.getOpening().catch(() => [])
      classData = (openingClasses || []).filter(cls => (cls.courseId || cls.courseID) === courseData.id)
    }

    classes.value = (classData || []).map(normalizeClass)
    course.value = normalizeCourse(courseData, classes.value)

    const firstClass = classes.value[0]
    const teacherObj = mockTeachers.find(t => t.name === course.value.teacher || t.id === courseData.teacherId)
    teacher.value = teacherObj || (firstClass ? {
      name: firstClass.teacherName,
      specialty: course.value.category,
      rating: course.value.rating,
      experience: 5,
      description: 'Giảng viên phụ trách lớp khai giảng gần nhất.',
      avatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=300&h=300&fit=crop&crop=face'
    } : null)
  } catch (err) {
    console.error(err)
    error.value = err.message || 'Có lỗi xảy ra khi tải thông tin khóa học.'
  } finally {
    isLoading.value = false
  }
}

async function handleEnroll(classId) {
  if (!authStore.isAuthenticated) {
    message.error('Vui lòng đăng nhập bằng tài khoản học viên để đăng ký lớp học.')
    router.push({ path: '/login', query: { redirect: router.currentRoute.value.fullPath } })
    return
  }

  if (!roleEquals(authStore.role, 'Student')) {
    message.error('Chỉ tài khoản học viên mới có thể đăng ký lớp học.')
    return
  }

  if (!authStore.user?.referenceId) {
    message.error('Tài khoản của bạn chưa được liên kết với hồ sơ học viên.')
    return
  }

  isEnrolling.value = true
  try {
    const selectedClass = classes.value.find(cls => cls.id === classId)
    if (!selectedClass || !course.value) {
      throw new Error('Không tìm thấy thông tin lớp học để đăng ký.')
    }

    await enrollmentApi.create({
      studentId: authStore.user.referenceId,
      courseId: course.value.id,
      courseNameSnapshot: course.value.title,
      classId: selectedClass.id,
      classNameSnapshot: selectedClass.className,
      note: 'Học viên đăng ký từ cổng khóa học'
    })

    message.success('Đăng ký lớp học thành công. Hồ sơ đã chuyển sang Admin chờ duyệt.')
    router.push('/student/courses')
  } catch (err) {
    console.error(err)
    message.error(err.message || 'Đăng ký thất bại. Lớp có thể đã đầy hoặc bạn đã đăng ký trước đó.')
  } finally {
    isEnrolling.value = false
  }
}

function formatDate(value) {
  if (!value) return ''
  return new Date(value).toLocaleDateString('vi-VN')
}

function handleImgError(e) {
  e.target.src = `https://placehold.co/900x600/e2e8f0/475569?text=${encodeURIComponent(course.value?.title || 'Course')}`
}
</script>
