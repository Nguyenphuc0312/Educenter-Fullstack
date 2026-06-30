import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

export default defineConfig({
  plugins: [
    vue(),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  build: {
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes('node_modules')) {
            if (id.includes('ant-design-vue') || id.includes('@ant-design/icons-vue') || id.includes('@ant-design/colors') || id.includes('dayjs')) {
              return 'antd'
            }
            if (id.includes('chart.js') || id.includes('vue-chartjs')) {
              return 'chart'
            }
            if (id.includes('axios') || id.includes('pinia') || id.includes('vue-router') || id.includes('vue')) {
              return 'core'
            }
            return 'vendor'
          }
        }
      }
    }
  }
})
