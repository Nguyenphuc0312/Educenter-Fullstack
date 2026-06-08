<template>
  <footer class="bg-section border-t border-base">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-14">
      <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-8 mb-12">
        <!-- Brand col — spans 2 -->
        <div class="col-span-2 flex flex-col gap-4">
          <a href="#" class="flex items-center gap-2.5">
            <div class="w-9 h-9 rounded-xl gradient-primary flex items-center justify-center">
              <svg xmlns="http://www.w3.org/2000/svg" width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-white">
                <path d="M21.42 10.922a1 1 0 0 0-.019-1.838L12.83 5.18a2 2 0 0 0-1.66 0L2.6 9.08a1 1 0 0 0 0 1.832l8.57 3.908a2 2 0 0 0 1.66 0z"/>
                <path d="M6 12v5c0 2 2 3 6 3s6-1 6-3v-5"/>
              </svg>
            </div>
            <span class="text-xl font-bold text-base-primary">
              Edu<span class="text-blue-600 dark:text-blue-400">Center</span>
            </span>
          </a>
          <p class="text-sm text-base-secondary leading-relaxed max-w-xs">
            Trung tâm đào tạo công nghệ thông tin hàng đầu, với đội ngũ giảng viên giàu kinh nghiệm và chương trình học bám sát thực tế.
          </p>

          <!-- Contact info -->
          <div class="flex flex-col gap-2.5 mt-1">
            <a href="mailto:hello@educenter.vn" class="flex items-center gap-2 text-xs text-base-muted hover:text-blue-600 dark:hover:text-blue-400 transition-colors group">
              <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-blue-500 group-hover:scale-110 transition-transform">
                <rect width="20" height="16" x="2" y="4" rx="2"/>
                <path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"/>
              </svg>
              hello@educenter.vn
            </a>
            <a href="tel:19001234" class="flex items-center gap-2 text-xs text-base-muted hover:text-blue-600 dark:hover:text-blue-400 transition-colors group">
              <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-teal-500 group-hover:scale-110 transition-transform">
                <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"/>
              </svg>
              1900 1234
            </a>
            <div class="flex items-start gap-2 text-xs text-base-muted">
              <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-indigo-500 mt-0.5 shrink-0">
                <path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0Z"/>
                <circle cx="12" cy="10" r="3"/>
              </svg>
              <span>123 Nguyễn Trãi, Quận 1, TP. Hồ Chí Minh</span>
            </div>
          </div>

          <!-- Socials -->
          <div class="flex items-center gap-2 mt-1">
            <a
              v-for="s in socials"
              :key="s.label"
              :href="s.href"
              :aria-label="s.label"
              class="w-8 h-8 rounded-lg border border-base bg-card-base flex items-center justify-center text-base-muted hover:text-blue-600 dark:hover:text-blue-400 hover:border-blue-300 dark:hover:border-blue-700 transition-all duration-200 hover:scale-110"
            >
              <component :is="s.Icon" />
            </a>
          </div>
        </div>

        <!-- Link columns -->
        <div v-for="[title, links] in Object.entries(footerLinks)" :key="title" class="flex flex-col gap-3">
          <h4 class="text-sm font-bold text-base-primary">{{ title }}</h4>
          <ul class="flex flex-col gap-2">
            <li v-for="link in links" :key="link">
              <a
                href="#"
                class="text-xs text-base-muted hover:text-blue-600 dark:hover:text-blue-400 transition-colors leading-relaxed"
              >
                {{ link }}
              </a>
            </li>
          </ul>
        </div>
      </div>

      <!-- Bottom bar -->
      <div class="border-t border-base pt-6 flex flex-col sm:flex-row items-center justify-between gap-3">
        <p class="text-xs text-base-muted text-center sm:text-left">
          © 2025 EduCenter. Tất cả quyền được bảo lưu.
        </p>
        <div class="flex items-center gap-1 text-xs text-base-muted">
          <span>Made with</span>
          <span class="text-red-500 mx-0.5">❤</span>
          <span>by EduCenter Team</span>
        </div>
      </div>
    </div>
  </footer>
</template>

<script setup>
import { h } from 'vue';

const SvgFacebook = {
  render: () => h('svg', { width: '14', height: '14', viewBox: '0 0 24 24', fill: 'currentColor' }, [
    h('path', { d: 'M18 2h-3a5 5 0 0 0-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 0 1 1-1h3z' })
  ])
};

const SvgYoutube = {
  render: () => h('svg', { width: '14', height: '14', viewBox: '0 0 24 24', fill: 'currentColor' }, [
    h('path', { d: 'M22.54 6.42a2.78 2.78 0 0 0-1.95-1.96C18.88 4 12 4 12 4s-6.88 0-8.59.46A2.78 2.78 0 0 0 1.46 6.42 29 29 0 0 0 1 12a29 29 0 0 0 .46 5.58 2.78 2.78 0 0 0 1.95 1.96C5.12 20 12 20 12 20s6.88 0 8.59-.46a2.78 2.78 0 0 0 1.95-1.96A29 29 0 0 0 23 12a29 29 0 0 0-.46-5.58z' }),
    h('polygon', { points: '9.75 15.02 15.5 12 9.75 8.98 9.75 15.02', fill: 'white' })
  ])
};

const SvgLinkedin = {
  render: () => h('svg', { width: '14', height: '14', viewBox: '0 0 24 24', fill: 'currentColor' }, [
    h('path', { d: 'M16 8a6 6 0 0 1 6 6v7h-4v-7a2 2 0 0 0-2-2 2 2 0 0 0-2 2v7h-4v-7a6 6 0 0 1 6-6z' }),
    h('rect', { x: '2', y: '9', width: '4', height: '12' }),
    h('circle', { cx: '4', cy: '4', r: '2' })
  ])
};

const SvgInstagram = {
  render: () => h('svg', { width: '14', height: '14', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('rect', { x: '2', y: '2', width: '20', height: '20', rx: '5', ry: '5' }),
    h('path', { d: 'M16 11.37A4 4 0 1 1 12.63 8 4 4 0 0 1 16 11.37z' }),
    h('line', { x1: '17.5', y1: '6.5', x2: '17.51', y2: '6.5' })
  ])
};

const footerLinks = {
  'Khóa học': ['ReactJS Cơ bản', 'VueJS Cơ bản', 'SQL Server', 'ASP.NET Core API', 'Fullstack Web', 'Tin học văn phòng'],
  'Hỗ trợ học viên': ['Cổng học viên', 'Lịch học', 'Tài liệu học tập', 'Kiểm tra tiến độ', 'Cộng đồng học viên'],
  'EduCenter': ['Giới thiệu', 'Đội ngũ giảng viên', 'Chứng chỉ & cam kết', 'Tuyển dụng', 'Tin tức'],
  'Liên hệ': ['Đăng ký tư vấn', 'Chính sách hoàn phí', 'Điều khoản sử dụng', 'Chính sách bảo mật'],
};

const socials = [
  { Icon: SvgFacebook, href: '#', label: 'Facebook' },
  { Icon: SvgYoutube, href: '#', label: 'YouTube' },
  { Icon: SvgLinkedin, href: '#', label: 'LinkedIn' },
  { Icon: SvgInstagram, href: '#', label: 'Instagram' },
];
</script>
