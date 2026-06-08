import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import { createPinia } from 'pinia'
import Antd from 'ant-design-vue'

// Styles — Ant Design Vue reset MUST come before Tailwind so Tailwind can override
import 'ant-design-vue/dist/reset.css'
import './index.css'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(Antd)

app.mount('#root')
