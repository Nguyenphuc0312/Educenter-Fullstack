<template>
  <section id="contact" class="py-20 bg-section">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="reveal-on-scroll fade-up flex flex-col gap-6">
        <div class="text-center mb-12">
          <span class="inline-block text-xs font-semibold text-teal-600 dark:text-teal-400 bg-teal-50 dark:bg-teal-950/40 px-3 py-1 rounded-full mb-3">
            Liên hệ ngay
          </span>
          <h2 class="text-3xl sm:text-4xl font-bold text-base-primary mb-3">
            Đăng ký tư vấn miễn phí
          </h2>
          <p class="text-base-secondary max-w-xl mx-auto">
            Để lại thông tin, đội ngũ tư vấn EduCenter sẽ liên hệ với bạn trong vòng 24 giờ.
          </p>
        </div>

        <div class="grid lg:grid-cols-2 gap-10 items-start">
          <!-- Contact info -->
          <div class="flex flex-col gap-6">
            <div>
              <img
                src="https://images.unsplash.com/photo-1521737604893-d14cc237f11d?w=600&h=350&fit=crop"
                alt="EduCenter tư vấn học viên"
                class="w-full rounded-2xl object-cover h-52 border border-base"
                @error="handleImgError"
              />
            </div>
            <a
              v-for="(item, i) in contactInfo"
              :key="i"
              :href="item.href"
              class="flex items-center gap-4 group"
            >
              <div class="w-11 h-11 rounded-xl gradient-primary flex items-center justify-center shrink-0 group-hover:scale-110 transition-transform duration-200">
                <component :is="item.icon" class="w-[18px] h-[18px] text-white" />
              </div>
              <div>
                <p class="text-xs text-base-muted">{{ item.label }}</p>
                <p class="text-sm font-semibold text-base-primary group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
                  {{ item.value }}
                </p>
              </div>
            </a>
          </div>

          <!-- Form -->
          <div class="bg-card-base border border-base rounded-2xl p-6 shadow-card-base">
            <div v-if="submitted" class="flex flex-col items-center justify-center py-12 gap-4 text-center">
              <div class="w-16 h-16 rounded-full bg-green-100 dark:bg-green-900/30 flex items-center justify-center">
                <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-green-500">
                  <polyline points="20 6 9 17 4 12"/>
                </svg>
              </div>
              <h3 class="text-xl font-bold text-base-primary">Đăng ký thành công!</h3>
              <p class="text-sm text-base-secondary max-w-xs">
                Cảm ơn bạn đã đăng ký. Tư vấn viên EduCenter sẽ liên hệ với bạn sớm nhất.
              </p>
              <button
                @click="resetForm"
                class="mt-2 text-sm font-semibold text-blue-600 dark:text-blue-400 hover:underline cursor-pointer"
              >
                Gửi thêm đăng ký
              </button>
            </div>

            <form v-else @submit.prevent="handleSubmit" class="flex flex-col gap-4">
              <h3 class="text-lg font-bold text-base-primary mb-1">Thông tin đăng ký</h3>
              <div class="grid sm:grid-cols-2 gap-4">
                <div>
                  <label class="text-xs font-semibold text-base-muted block mb-1.5">Họ và tên *</label>
                  <input
                    id="form-name"
                    v-model="form.name"
                    required
                    placeholder="Nguyễn Văn A"
                    class="w-full px-3.5 py-2.5 rounded-xl border border-base bg-section text-base-primary text-sm placeholder:text-base-muted focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 dark:focus:border-blue-600 transition-all"
                  />
                </div>
                <div>
                  <label class="text-xs font-semibold text-base-muted block mb-1.5">Số điện thoại *</label>
                  <input
                    id="form-phone"
                    v-model="form.phone"
                    required
                    placeholder="0901 234 567"
                    class="w-full px-3.5 py-2.5 rounded-xl border border-base bg-section text-base-primary text-sm placeholder:text-base-muted focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 dark:focus:border-blue-600 transition-all"
                  />
                </div>
              </div>
              <div>
                <label class="text-xs font-semibold text-base-muted block mb-1.5">Email</label>
                <input
                  id="form-email"
                  type="email"
                  v-model="form.email"
                  placeholder="email@example.com"
                  class="w-full px-3.5 py-2.5 rounded-xl border border-base bg-section text-base-primary text-sm placeholder:text-base-muted focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 dark:focus:border-blue-600 transition-all"
                />
              </div>
              <div>
                <label class="text-xs font-semibold text-base-muted block mb-1.5">Khóa học quan tâm</label>
                <select
                  id="form-course"
                  v-model="form.course"
                  class="w-full px-3.5 py-2.5 rounded-xl border border-base bg-section text-base-primary text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 dark:focus:border-blue-600 transition-all"
                >
                  <option value="">-- Chọn khóa học --</option>
                  <option>ReactJS Cơ bản đến Nâng cao</option>
                  <option>VueJS Cơ bản</option>
                  <option>SQL Server cho người mới</option>
                  <option>ASP.NET Core API</option>
                  <option>Fullstack Web Developer</option>
                  <option>Tin học văn phòng</option>
                  <option>Node.js Backend</option>
                  <option>UI/UX Design</option>
                </select>
              </div>
              <div>
                <label class="text-xs font-semibold text-base-muted block mb-1.5">Ghi chú thêm</label>
                <textarea
                  id="form-note"
                  v-model="form.note"
                  rows="3"
                  placeholder="Bạn muốn hỏi gì thêm? Thời gian học phù hợp?"
                  class="w-full px-3.5 py-2.5 rounded-xl border border-base bg-section text-base-primary text-sm placeholder:text-base-muted focus:outline-none focus:ring-2 focus:ring-blue-500/30 focus:border-blue-400 dark:focus:border-blue-600 transition-all resize-none"
                />
              </div>
              <button
                id="btn-submit-form"
                type="submit"
                class="w-full py-3 gradient-primary text-white font-semibold text-sm rounded-xl hover:opacity-90 transition-all cursor-pointer transform hover:scale-[1.02] active:scale-[0.97] flex items-center justify-center gap-2"
              >
                <svg xmlns="http://www.w3.org/2000/svg" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <line x1="22" y1="2" x2="11" y2="13"/>
                  <polygon points="22 2 15 22 11 13 2 9 22 2"/>
                </svg>
                Gửi đăng ký tư vấn
              </button>
              <p class="text-xs text-center text-base-muted">
                Tư vấn miễn phí · Không ràng buộc · Phản hồi trong 24h
              </p>
            </form>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { ref, h } from 'vue'
import { useReveal } from '../composables/useReveal'

useReveal()

const phoneSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z' })
  ])
}
const mailSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z' }),
    h('polyline', { points: '22,6 12,13 2,6' })
  ])
}
const mapPinSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z' }),
    h('circle', { cx: '12', cy: '10', r: '3' })
  ])
}

const contactInfo = [
  { icon: phoneSvg, label: 'Hotline', value: '1900 1234', href: 'tel:19001234' },
  { icon: mailSvg, label: 'Email', value: 'hello@educenter.vn', href: 'mailto:hello@educenter.vn' },
  { icon: mapPinSvg, label: 'Địa chỉ', value: '123 Nguyễn Trãi, Q.1, TP.HCM', href: '#' }
]

const form = ref({ name: '', phone: '', email: '', course: '', note: '' })
const submitted = ref(false)

function handleSubmit() {
  submitted.value = true
}

function resetForm() {
  submitted.value = false
  form.value = { name: '', phone: '', email: '', course: '', note: '' }
}

function handleImgError(e) {
  e.target.src = 'https://placehold.co/600x350/e2e8f0/94a3b8?text=EduCenter'
}
</script>
