const COURSE_IMAGES = {
  'reactjs-co-ban': 'https://images.unsplash.com/photo-1633356122544-f134324a6cee?w=900&h=600&fit=crop',
  'vuejs-co-ban': 'https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=900&h=600&fit=crop',
  'sql-server-cho-nguoi-moi': 'https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=900&h=600&fit=crop',
  'aspnet-core-api': 'https://images.unsplash.com/photo-1515879218367-8466d910aaa4?w=900&h=600&fit=crop',
  'fullstack-web-developer': 'https://images.unsplash.com/photo-1498050108023-c5249f4df085?w=900&h=600&fit=crop',
  'nodejs-backend': 'https://images.unsplash.com/photo-1516321318423-f06f85e504b3?w=900&h=600&fit=crop',
  'ui-ux-design-co-ban': 'https://images.unsplash.com/photo-1581291518857-4e27b48ff24e?w=900&h=600&fit=crop',
  'tin-hoc-van-phong': 'https://images.unsplash.com/photo-1497366754035-f200968a6e72?w=900&h=600&fit=crop',
}

const FALLBACK_IMAGES = [
  'https://images.unsplash.com/photo-1522202176988-66273c2fd55f?w=900&h=600&fit=crop',
  'https://images.unsplash.com/photo-1519389950473-47ba0277781c?w=900&h=600&fit=crop',
  'https://images.unsplash.com/photo-1523240795612-9a054b0db644?w=900&h=600&fit=crop',
]

function slugify(value) {
  return String(value || '')
    .trim()
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
}

function normalizeStatus(status) {
  if (status === 'ComingSoon' || status === 3 || status === '3') return 'upcoming'
  if (status === 'Opening' || status === 1 || status === '1') return 'selling'
  return 'selling'
}

export function normalizeCourse(course, classes = []) {
  const title = course?.title || course?.name || course?.courseName || 'Khóa học'
  const slug = course?.slug || slugify(title)
  const firstClass = classes.find(cls => cls.courseId === course?.id || cls.courseID === course?.id)
  const imageIndex = Math.abs(slug.split('').reduce((sum, char) => sum + char.charCodeAt(0), 0)) % FALLBACK_IMAGES.length

  return {
    ...course,
    id: course?.id || course?.courseID,
    title,
    slug,
    description: course?.shortDescription || course?.description || 'Chương trình học thực chiến, bám sát nhu cầu doanh nghiệp.',
    category: course?.category || 'Programming',
    level: course?.level || 'Beginner',
    sessions: course?.sessions || course?.totalSessions || 12,
    price: course?.price ?? course?.tuitionFee ?? 0,
    originalPrice: course?.originalPrice ?? course?.tuitionFee ?? 0,
    rating: Number(course?.rating || 4.8),
    students: course?.students ?? course?.enrolledCount ?? firstClass?.currentStudents ?? 0,
    teacher: course?.teacher || firstClass?.teacherNameSnapshot || 'Theo lớp khai giảng',
    image: course?.image || course?.thumbnailUrl || COURSE_IMAGES[slug] || FALLBACK_IMAGES[imageIndex],
    status: normalizeStatus(course?.status),
  }
}

export function normalizeClass(cls) {
  return {
    ...cls,
    id: cls?.id || cls?.classID,
    courseId: cls?.courseId || cls?.courseID,
    className: cls?.className || cls?.name || cls?.classCode || 'Lớp học',
    teacherName: cls?.teacherName || cls?.teacherNameSnapshot || 'Chưa phân công',
    startDate: cls?.startDate,
    endDate: cls?.endDate,
    currentStudents: cls?.currentStudents ?? cls?.enrolledStudents ?? 0,
    maxStudents: cls?.maxStudents ?? cls?.capacity ?? 30,
  }
}
