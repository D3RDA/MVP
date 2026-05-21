import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import './assets/style.css'

window.addEventListener('auth:logout', () => {
  if (router.currentRoute.value.path !== '/login') {
    router.push('/login')
  }
})

createApp(App).use(router).mount('#app')
