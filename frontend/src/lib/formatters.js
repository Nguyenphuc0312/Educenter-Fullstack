export function formatVnd(amount) {
  if (amount == null) return '-'
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount)
}

export function formatNumber(value) {
  if (value == null) return '-'
  return new Intl.NumberFormat('vi-VN').format(value)
}

export function formatDate(dateStr) {
  if (!dateStr) return '-'
  const date = new Date(dateStr)
  if (Number.isNaN(date.getTime())) return '-'
  return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

export function formatDateTime(dateStr) {
  if (!dateStr) return '-'
  const date = new Date(dateStr)
  if (Number.isNaN(date.getTime())) return '-'
  return date.toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function formatTime(timeStr) {
  if (!timeStr) return '-'
  return String(timeStr).slice(0, 5)
}

export function formatPercent(value, digits = 1) {
  if (value == null) return '-'
  return `${Number(value).toFixed(digits)}%`
}

export function formatScore(value) {
  if (value == null) return '-'
  return Number(value).toFixed(1)
}

export function truncate(value, length = 80) {
  if (!value) return ''
  return value.length > length ? `${value.slice(0, length)}...` : value
}

export function getInitials(name = '') {
  return name
    .split(' ')
    .filter(Boolean)
    .slice(-2)
    .map((word) => word[0])
    .join('')
    .toUpperCase()
}

export function formatRelative(dateStr) {
  if (!dateStr) return '-'
  const date = new Date(dateStr)
  const now = new Date()
  const minutes = Math.floor((now - date) / 60000)
  if (minutes < 1) return 'vừa xong'
  if (minutes < 60) return `${minutes} phút trước`
  const hours = Math.floor(minutes / 60)
  if (hours < 24) return `${hours} giờ trước`
  const days = Math.floor(hours / 24)
  if (days < 30) return `${days} ngày trước`
  return formatDate(dateStr)
}

/**
 * Rút gọn UUID/code thành dạng `INV-XXXX` (4 ký tự đầu của phần ngẫu nhiên, viết hoa)
 * Dùng cho mã hóa đơn/ID hiển thị trên UI, tránh lộ UUID dài.
 *   shortInvoiceCode('99999999-1234-5678-9abc-def012345678') => 'INV-9999'
 *   shortInvoiceCode('INV-2024-0005')                       => 'INV-2024'
 *   shortInvoiceCode(null)                                   => null
 */
export function shortInvoiceCode(value) {
  if (value === null || value === undefined || value === '') return null
  const s = String(value).trim()
  if (!s) return null
  // Nếu đã có dạng INV-XXXX thì lấy phần sau gạch ngang đầu tiên (4 ký tự)
  if (/^INV[-_]/i.test(s)) {
    const parts = s.split(/[-_]/)
    if (parts.length >= 2 && parts[1]) {
      return `INV-${parts[1].slice(0, 4).toUpperCase()}`
    }
  }
  // Nếu là UUID dạng xxxxxxxx-xxxx-... thì lấy 4 ký tự đầu của phần đầu
  if (s.length >= 8) {
    const cleaned = s.replace(/-/g, '')
    if (cleaned.length >= 4) {
      return `INV-${cleaned.slice(0, 4).toUpperCase()}`
    }
  }
  // Ngắn (<= 8 ký tự) thì trả thẳng
  return s.toUpperCase()
}

/**
 * Status localization — map raw value sang label tiếng Việt với semantic color.
 * Dùng cho StatusBadge / StatusPill để đảm bảo UI không bao giờ hiển thị tiếng Anh.
 *
 * Trả về: { label, color } hoặc null nếu không match.
 *
 * @param {string|number} value  Raw status value
 * @param {Array} options  Output của `toOptions(enumMap, colorMap)` — { value, label, text, color }
 */
export function localizeStatus(value, options) {
  if (value === null || value === undefined) return null
  if (!options || !options.length) return null
  const opt = options.find(o => String(o.value) === String(value))
  if (!opt) return null
  return {
    label: opt.text || opt.label || String(value),
    color: opt.color || 'blue'
  }
}

/**
 * Format short date dd/MM/yyyy (no time, no en-dash), dùng cho insight card / activity feed.
 *   shortDateVN('2026-06-15T10:30:00') => '15/06/2026'
 */
export function shortDateVN(dateStr) {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  if (Number.isNaN(date.getTime())) return ''
  const dd = String(date.getDate()).padStart(2, '0')
  const mm = String(date.getMonth() + 1).padStart(2, '0')
  const yy = date.getFullYear()
  return `${dd}/${mm}/${yy}`
}

/**
 * Format ngắn cho ngày + giờ (dd/MM HH:mm), dùng cho timestamp "Cập nhật lúc".
 */
export function shortDateTimeVN(dateStr) {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  if (Number.isNaN(date.getTime())) return ''
  const dd = String(date.getDate()).padStart(2, '0')
  const mm = String(date.getMonth() + 1).padStart(2, '0')
  const hh = String(date.getHours()).padStart(2, '0')
  const min = String(date.getMinutes()).padStart(2, '0')
  return `${dd}/${mm} ${hh}:${min}`
}
