<template>
  <section class="py-20 bg-page">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="reveal-on-scroll fade-up flex flex-col gap-6">
        <div class="text-center mb-14">
          <span class="inline-block text-xs font-semibold text-indigo-600 dark:text-indigo-400 bg-indigo-50 dark:bg-indigo-950/40 px-3 py-1 rounded-full mb-3">
            Lộ trình học tập
          </span>
          <h2 class="text-3xl sm:text-4xl font-bold text-base-primary mb-3">
            Lộ trình học tại EduCenter
          </h2>
          <p class="text-base-secondary max-w-xl mx-auto">
            Quy trình học tập rõ ràng, có hệ thống — từ khi đăng ký đến khi nhận chứng chỉ.
          </p>
        </div>

        <div class="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
          <div
            v-for="(item, idx) in learningPath"
            :key="item.step"
            class="relative bg-card-base border border-card-base hover:border-blue-200 dark:hover:border-blue-800 rounded-2xl p-6 shadow-card-base hover:shadow-md transition-all duration-300 group"
          >
            <!-- Step number -->
            <div class="absolute -top-3 -right-3 w-7 h-7 rounded-full bg-slate-100 dark:bg-slate-700 border-2 border-white dark:border-slate-800 flex items-center justify-center text-xs font-bold text-base-secondary">
              {{ item.step }}
            </div>

            <!-- Icon -->
            <div :class="['w-12 h-12 rounded-xl bg-gradient-to-br flex items-center justify-center mb-4 group-hover:scale-110 transition-transform duration-300', stepColors[idx] || 'from-blue-500 to-blue-600']">
              <component :is="getIconComponent(item.icon)" class="w-[22px] h-[22px] text-white" />
            </div>

            <h3 class="font-bold text-base text-base-primary mb-2 group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
              {{ item.title }}
            </h3>
            <p class="text-sm text-base-secondary leading-relaxed">
              {{ item.description }}
            </p>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup>
import { h } from 'vue'
import { learningPath } from '../data/mockData'
import { useReveal } from '../composables/useReveal'

useReveal()

const stepColors = [
  'from-blue-500 to-blue-600',
  'from-teal-500 to-teal-600',
  'from-indigo-500 to-indigo-600',
  'from-cyan-500 to-cyan-600',
  'from-violet-500 to-violet-600',
  'from-amber-500 to-amber-600'
]

// Icon mapping to custom SVGs to match Lucide style
const searchSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('circle', { cx: '11', cy: '11', r: '8' }),
    h('line', { x1: '21', y1: '21', x2: '16.65', y2: '16.65' })
  ])
}
const clipboardListSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('rect', { x: '8', y: '2', width: '8', height: '4', rx: '1', ry: '1' }),
    h('path', { d: 'M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2' }),
    h('path', { d: 'M12 11h4' }),
    h('path', { d: 'M12 16h4' }),
    h('path', { d: 'M8 11h.01' }),
    h('path', { d: 'M8 16h.01' })
  ])
}
const videoSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('path', { d: 'm22 8-6 4 6 4V8Z' }),
    h('rect', { x: '2', y: '6', width: '14', height: '12', rx: '2', ry: '2' })
  ])
}
const barChartSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('line', { x1: '18', x2: '18', y1: '20', y2: '10' }),
    h('line', { x1: '12', x2: '12', y1: '20', y2: '4' }),
    h('line', { x1: '6', x2: '6', y1: '20', y2: '14' })
  ])
}
const checkSquareSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('polyline', { points: '9 11 12 14 22 4' }),
    h('path', { d: 'M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11' })
  ])
}
const awardSvg = {
  render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', viewBox: '0 0 24 24', fill: 'none', stroke: 'currentColor', 'stroke-width': '2', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }, [
    h('circle', { cx: '12', cy: '8', r: '7' }),
    h('polyline', { points: '8.21 13.89 7 23 12 20 17 23 15.79 13.88' })
  ])
}

function getIconComponent(iconName) {
  if (iconName === 'Search') return searchSvg
  if (iconName === 'ClipboardList') return clipboardListSvg
  if (iconName === 'Video') return videoSvg
  if (iconName === 'BarChart2') return barChartSvg
  if (iconName === 'CheckSquare') return checkSquareSvg
  return awardSvg
}
</script>
