<template>
  <section id="certificates" class="py-20 bg-page">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="reveal-on-scroll fade-up flex flex-col gap-6">
        <div class="text-center mb-12">
          <span class="inline-block text-xs font-semibold text-indigo-600 dark:text-indigo-400 bg-indigo-50 dark:bg-indigo-950/40 px-3 py-1 rounded-full mb-3">
            Cam kết chất lượng
          </span>
          <h2 class="text-3xl sm:text-4xl font-bold text-base-primary mb-3">
            Chứng chỉ & cam kết chất lượng
          </h2>
          <p class="text-base-secondary max-w-xl mx-auto">
            EduCenter cam kết mang đến trải nghiệm học tập tốt nhất, chứng chỉ có giá trị và hỗ trợ toàn diện cho học viên.
          </p>
        </div>

        <!-- Stats bar -->
        <div class="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-12">
          <div
            v-for="(stat, i) in trustStats"
            :key="i"
            class="bg-gradient-to-br from-blue-600 to-teal-500 rounded-2xl p-5 text-center text-white"
          >
            <component :is="stat.icon" class="mx-auto mb-2 opacity-90 w-[22px] h-[22px]" />
            <p class="text-2xl font-bold">{{ stat.value }}</p>
            <p class="text-xs opacity-80 mt-0.5">{{ stat.label }}</p>
          </div>
        </div>

        <!-- Cert cards -->
        <div class="grid sm:grid-cols-2 lg:grid-cols-3 gap-5">
          <div
            v-for="cert in certificates"
            :key="cert.id"
            :class="['bg-card-base border border-card-base rounded-2xl p-5 shadow-card-base hover:shadow-md transition-all duration-300 group flex gap-4 items-start', getColors(cert.color).border]"
          >
            <div :class="['w-11 h-11 rounded-xl flex items-center justify-center shrink-0 group-hover:scale-110 transition-transform duration-300', getColors(cert.color).icon]">
              <component :is="getIconComponent(cert.icon)" class="w-5 h-5" />
            </div>
            <div>
              <h3 class="font-semibold text-sm text-base-primary mb-1">{{ cert.title }}</h3>
              <p class="text-xs text-base-secondary leading-relaxed">{{ cert.description }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { h } from 'vue'
import { certificates } from '../data/mockData'
import { useReveal } from '../composables/useReveal'

useReveal()

// Icon template SVGs
const usersSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2' }),
    h('circle', { cx: '9', cy: '7', r: '4' }),
    h('path', { d: 'M23 21v-2a4 4 0 0 0-3-3.87' }),
    h('path', { d: 'M16 3.13a4 4 0 0 1 0 7.75' })
  ])
}
const awardSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('circle', { cx: '12', cy: '8', r: '7' }),
    h('polyline', { points: '8.21 13.89 7 23 12 20 17 23 15.79 13.88' })
  ])
}
const shieldCheckSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z' }),
    h('path', { d: 'm9 12 2 2 4-4' })
  ])
}
const checkCircleSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('circle', { cx: '12', cy: '12', r: '10' }),
    h('path', { d: 'm9 12 2 2 4-4' })
  ])
}
const mapSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('polygon', { points: '3 6 9 3 15 6 21 3 21 18 15 21 9 18 3 21' }),
    h('line', { x1: '9', y1: '3', x2: '9', y2: '18' }),
    h('line', { x1: '15', y1: '6', x2: '15', y2: '21' })
  ])
}
const userCheckSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2' }),
    h('circle', { cx: '9', cy: '7', r: '4' }),
    h('polyline', { points: '16 11 18 13 22 9' })
  ])
}
const headphonesSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'M3 18v-6a9 9 0 0 1 18 0v6' }),
    h('path', { d: 'M21 19a2 2 0 0 1-2 2h-1a2 2 0 0 1-2-2v-3a2 2 0 0 1 2-2h3zM3 19a2 2 0 0 0 2 2h1a2 2 0 0 0 2-2v-3a2 2 0 0 0-2-2H3z' })
  ])
}
const codeSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('polyline', { points: '16 18 22 12 16 6' }),
    h('polyline', { points: '8 6 2 12 8 18' })
  ])
}

const trustStats = [
  { label: 'Học viên đã tốt nghiệp', value: '500+', icon: usersSvg },
  { label: 'Chứng chỉ đã cấp', value: '480+', icon: awardSvg },
  { label: 'Tỷ lệ hài lòng', value: '95%', icon: shieldCheckSvg },
  { label: 'Khóa học đang mở', value: '20+', icon: checkCircleSvg }
]

const colorMap = {
  blue: {
    icon: 'bg-blue-100 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400',
    border: 'hover:border-blue-200 dark:hover:border-blue-800'
  },
  teal: {
    icon: 'bg-teal-100 dark:bg-teal-900/30 text-teal-600 dark:text-teal-400',
    border: 'hover:border-teal-200 dark:hover:border-teal-800'
  },
  indigo: {
    icon: 'bg-indigo-100 dark:bg-indigo-900/30 text-indigo-600 dark:text-indigo-400',
    border: 'hover:border-indigo-200 dark:hover:border-indigo-800'
  },
  cyan: {
    icon: 'bg-cyan-100 dark:bg-cyan-900/30 text-cyan-600 dark:text-cyan-400',
    border: 'hover:border-cyan-200 dark:hover:border-cyan-800'
  }
}

function getColors(color) {
  return colorMap[color] || colorMap.blue
}

function getIconComponent(iconName) {
  if (iconName === 'Award') return awardSvg
  if (iconName === 'Map') return mapSvg
  if (iconName === 'UserCheck') return userCheckSvg
  if (iconName === 'HeadphonesIcon' || iconName === 'Headphones') return headphonesSvg
  if (iconName === 'Code2') return codeSvg
  return usersSvg
}
</script>
