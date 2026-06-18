export const COURSE_STATUS = { 0: 'Bản nháp', 1: 'Đang mở', 2: 'Đã đóng', 3: 'Sắp khai giảng' }
export const CLASS_STATUS = { 0: 'Đang mở', 1: 'Đã đầy', 2: 'Đang học', 3: 'Hoàn thành', 4: 'Đã hủy' }
export const ENROLLMENT_STATUS = { 1: 'Chờ xác nhận', 2: 'Đã xác nhận', 3: 'Đang học', 4: 'Hoàn thành', 5: 'Đã hủy' }
export const ATTENDANCE_STATUS = { 1: 'Có mặt', 2: 'Vắng', 3: 'Đi muộn', 4: 'Có phép' }
export const RESULT_STATUS = { 1: 'Đang học', 2: 'Đạt', 3: 'Không đạt' }
export const INVOICE_STATUS = { 1: 'Chưa thanh toán', 2: 'Một phần', 3: 'Đã thanh toán', 4: 'Quá hạn' }
export const PAYMENT_METHOD = { 1: 'Tiền mặt', 2: 'Chuyển khoản', 3: 'Momo', 4: 'VNPay' }
export const PAYMENT_STATUS = { 1: 'Thành công', 2: 'Đang xử lý', 3: 'Thất bại', 4: 'Đã hủy' }
export const LEARNING_MODE = { 0: 'Trực tiếp', 1: 'Trực tuyến', 2: 'Kết hợp' }
export const STUDY_SHIFT = { 0: 'Sáng', 1: 'Chiều', 2: 'Tối' }
export const SCHEDULE_STATUS = { 0: 'Đã lên lịch', 1: 'Hoàn thành', 2: 'Đã hủy' }
export const GENDER = { 0: 'Không rõ', 1: 'Nam', 2: 'Nữ', 3: 'Khác' }
export const USER_ROLE = { 1: 'Admin', 2: 'Giảng viên', 3: 'Học viên' }
export const ACCOUNT_STATUS = { 1: 'Hoạt động', 2: 'Bị khóa' }
export const STUDENT_STATUS = { 1: 'Hoạt động', 2: 'Không hoạt động', 3: 'Tạm dừng' }
export const TEACHER_STATUS = { 0: 'Hoạt động', 1: 'Không hoạt động' }
export const SESSION_STATUS = { 1: 'Đang mở', 2: 'Đã khóa' }
export const CLASSROOM_TYPE = { 0: 'Lý thuyết', 1: 'Phòng máy', 2: 'Hội thảo', 3: 'Trực tuyến' }
export const CLASSROOM_STATUS = { 0: 'Sẵn sàng', 1: 'Đang sử dụng', 2: 'Đang bảo trì', 3: 'Đã đóng' }
export const DAY_OF_WEEK_VN = ['Chủ nhật', 'Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7']

/**
 * Defensive map: nếu backend trả về status string tiếng Anh thay vì int, vẫn map sang tiếng Việt.
 * Dùng cho `localizeStatus(value, options)` kết hợp — nếu value là string, tra trong map này trước.
 * Màu semantic theo chuẩn: success (green), warning (amber), danger (red), info (blue), neutral (slate).
 */
export const STATUS_LOCALES = {
  // Class / Course
  Open:         { label: 'Đang mở',     color: 'blue' },
  Draft:        { label: 'Bản nháp',     color: 'slate' },
  Closed:       { label: 'Đã đóng',     color: 'slate' },
  Full:         { label: 'Đã đầy',      color: 'amber' },
  InProgress:   { label: 'Đang học',    color: 'indigo' },
  Completed:    { label: 'Hoàn thành',  color: 'green' },
  Cancelled:    { label: 'Đã hủy',      color: 'red' },
  Scheduled:    { label: 'Đã lên lịch', color: 'blue' },
  // Enrollment
  Pending:      { label: 'Chờ xác nhận', color: 'orange' },
  Confirmed:    { label: 'Đã xác nhận',  color: 'blue' },
  // Attendance
  Present:      { label: 'Có mặt',       color: 'green' },
  Late:         { label: 'Đi muộn',      color: 'amber' },
  Absent:       { label: 'Vắng',         color: 'red' },
  Excused:      { label: 'Có phép',      color: 'blue' },
  // Result
  Passed:       { label: 'Đạt',          color: 'green' },
  Failed:       { label: 'Không đạt',    color: 'red' },
  // Invoice / Payment
  Paid:         { label: 'Đã thanh toán', color: 'green' },
  Partial:      { label: 'Thanh toán một phần', color: 'amber' },
  Unpaid:       { label: 'Chưa thanh toán', color: 'orange' },
  Overdue:      { label: 'Quá hạn',      color: 'red' },
  // Account
  Active:       { label: 'Hoạt động',   color: 'green' },
  Inactive:     { label: 'Ngừng hoạt động', color: 'slate' },
  Locked:       { label: 'Đã khóa',      color: 'rose' },
  Suspended:    { label: 'Tạm dừng',     color: 'amber' },
  // Payment status
  Success:      { label: 'Thành công',   color: 'green' },
  Processing:   { label: 'Đang xử lý',  color: 'blue' },
  Failed:       { label: 'Thất bại',     color: 'red' },
  // Classroom status
  Available:    { label: 'Sẵn sàng',     color: 'green' },
  InUse:        { label: 'Đang sử dụng',  color: 'blue' },
  Maintenance:  { label: 'Đang bảo trì',  color: 'orange' },
  Closed:       { label: 'Đã đóng',       color: 'slate' }
}

export const ROLE_INT = { Admin: 1, Teacher: 2, Student: 3 }
export const ROLE = { Admin: 'Admin', Teacher: 'Teacher', Student: 'Student' }

export const toOptions = (map, colors = {}) =>
  Object.entries(map).map(([value, label]) => ({
    value: Number(value),
    text: label,
    label,
    color: colors[value] || colors[Number(value)] || 'blue',
  }))

export const STATUS_COLORS = {
  neutral: 'default',
  blue: 'blue',
  green: 'green',
  red: 'red',
  orange: 'orange',
  purple: 'purple',
}

export const normalizeRole = (role) => {
  if (role === 1 || role === '1' || role === 'Admin') return ROLE.Admin
  if (role === 2 || role === '2' || role === 'Teacher') return ROLE.Teacher
  if (role === 3 || role === '3' || role === 'Student') return ROLE.Student
  return role
}

export const roleEquals = (role, expectedRole) => normalizeRole(role) === normalizeRole(expectedRole)

export const getRoleLabel = (role) => USER_ROLE[role] || USER_ROLE[ROLE_INT[normalizeRole(role)]] || normalizeRole(role)

export const getRoleHomePath = (role) => {
  const normalizedRole = normalizeRole(role)
  if (normalizedRole === ROLE.Admin) return '/admin'
  if (normalizedRole === ROLE.Teacher) return '/teacher'
  return '/student'
}

export const getRoleProfilePath = (role) => `${getRoleHomePath(role)}/profile`
