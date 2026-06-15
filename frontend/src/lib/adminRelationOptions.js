export function asList(response) {
  if (Array.isArray(response)) return response
  if (Array.isArray(response?.items)) return response.items
  if (Array.isArray(response?.data)) return response.data
  return []
}

export function courseName(course) {
  return course?.name || course?.courseName || course?.courseNameSnapshot || ''
}

export function className(cls) {
  return cls?.className || cls?.name || cls?.classNameSnapshot || ''
}

export function studentName(student) {
  return student?.fullName || student?.studentNameSnapshot || student?.name || ''
}

export function studentCode(student) {
  return student?.studentCode || student?.studentCodeSnapshot || ''
}

export function courseOptions(courses) {
  return courses.map((course) => ({
    value: course.id,
    label: [courseName(course), course.code || course.courseCode].filter(Boolean).join(' - '),
    item: course,
  }))
}

export function classOptions(classes) {
  return classes.map((cls) => ({
    value: cls.id,
    label: [
      className(cls),
      cls.classCode,
      cls.courseNameSnapshot || cls.courseName,
    ].filter(Boolean).join(' - '),
    item: cls,
  }))
}

export function studentOptions(students) {
  return students.map((student) => ({
    value: student.id,
    label: [
      studentName(student),
      studentCode(student),
      student.email,
    ].filter(Boolean).join(' - '),
    item: student,
  }))
}

export function findById(items, id) {
  return items.find((item) => item.id === id)
}

export function applyStudentSnapshot(formState, student) {
  formState.studentId = student?.id || ''
  formState.studentNameSnapshot = studentName(student)
  formState.studentCodeSnapshot = studentCode(student)
}

export function applyCourseSnapshot(formState, course) {
  formState.courseId = course?.id || ''
  formState.courseNameSnapshot = courseName(course)
  if (course?.tuitionFee != null && !Number(formState.totalAmount || 0)) {
    formState.totalAmount = course.tuitionFee
  }
}

export function applyClassSnapshot(formState, cls, courses = []) {
  formState.classId = cls?.id || ''
  formState.classNameSnapshot = className(cls)

  const course = findById(courses, cls?.courseId)
  formState.courseId = cls?.courseId || course?.id || formState.courseId || ''
  formState.courseNameSnapshot = cls?.courseNameSnapshot || courseName(course) || formState.courseNameSnapshot || ''
  if (course?.tuitionFee != null && !Number(formState.totalAmount || 0)) {
    formState.totalAmount = course.tuitionFee
  }
}
