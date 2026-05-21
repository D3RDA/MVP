<script setup>
import { ref } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import api from '../services/api'
import { setSession } from '../services/auth'
import ErrorMessage from '../components/ErrorMessage.vue'
import { getErrorMessage } from '../services/errorMessage'

const router = useRouter()
const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function submit() {
  error.value = ''
  loading.value = true
  try {
    const { data } = await api.post('/auth/login', { email: email.value, password: password.value })
    setSession(data)
    router.push('/dashboard')
  } catch (err) {
    error.value = getErrorMessage(err, 'Nem sikerült bejelentkezni.')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <section class="auth-card">
    <h1>Bejelentkezés</h1>
    <p>Lépj be a projekt- és álláskövető rendszeredbe.</p>
    <ErrorMessage :message="error" />
    <form @submit.prevent="submit" class="form">
      <label>Email<input v-model="email" type="email" required /></label>
      <label>Jelszó<input v-model="password" type="password" required /></label>
      <button :disabled="loading">{{ loading ? 'Belépés...' : 'Belépés' }}</button>
    </form>
    <p class="muted">Nincs fiókod? <RouterLink to="/register">Regisztráció</RouterLink></p>
  </section>
</template>
