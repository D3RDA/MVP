<script setup>
import { onMounted, ref } from 'vue'
import api from '../services/api'
import ErrorMessage from '../components/ErrorMessage.vue'
import LoadingBox from '../components/LoadingBox.vue'
import ConfirmModal from '../components/ConfirmModal.vue'
import EmptyState from '../components/EmptyState.vue'
import { useToast } from '../composables/useToast'

const { show: toast } = useToast()
const companies = ref([])
const error = ref('')
const loading = ref(true)
const saving = ref(false)
const editingId = ref(null)
const editForm = ref({})
const deleteTarget = ref(null)
const form = ref({ name: '', website: '', location: '', contactName: '', contactEmail: '', contactPhone: '', notes: '' })

async function load() {
  loading.value = true
  try {
    const { data } = await api.get('/companies')
    companies.value = data
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült betölteni a cégeket.'
  } finally {
    loading.value = false
  }
}

async function createCompany() {
  error.value = ''
  saving.value = true
  try {
    await api.post('/companies', form.value)
    form.value = { name: '', website: '', location: '', contactName: '', contactEmail: '', contactPhone: '', notes: '' }
    toast('Cég sikeresen mentve ✓')
    await load()
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült menteni a céget.'
  } finally {
    saving.value = false
  }
}

function startEdit(company) {
  editingId.value = company.id
  editForm.value = {
    name: company.name || '',
    website: company.website || '',
    location: company.location || '',
    contactName: company.contactName || '',
    contactEmail: company.contactEmail || '',
    contactPhone: company.contactPhone || '',
    notes: company.notes || ''
  }
}

function cancelEdit() {
  editingId.value = null
  editForm.value = {}
}

async function updateCompany(id) {
  error.value = ''
  saving.value = true
  try {
    await api.put(`/companies/${id}`, editForm.value)
    cancelEdit()
    toast('Cég sikeresen módosítva ✓')
    await load()
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült módosítani a céget.'
  } finally {
    saving.value = false
  }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  await api.delete(`/companies/${deleteTarget.value.id}`)
  deleteTarget.value = null
  toast('Cég törölve ✓')
  await load()
}

function scrollToId(id) {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' })
}

onMounted(load)
</script>

<template>
  <section>
    <h1>Cégek</h1>
    <ErrorMessage :message="error" />
    <article class="card" id="new-company">
      <h2>Új cég</h2>
      <form class="form grid-form" @submit.prevent="createCompany">
        <label>Cégnév<input v-model="form.name" required /><span v-if="!form.name" class="field-error">A cégnév kötelező.</span></label>
        <label>Weboldal<input v-model="form.website" /></label>
        <label>Helyszín<input v-model="form.location" /></label>
        <label>Kapcsolattartó<input v-model="form.contactName" /></label>
        <label>Email<input v-model="form.contactEmail" type="email" /></label>
        <label>Telefon<input v-model="form.contactPhone" /></label>
        <label>Jegyzet<input v-model="form.notes" /></label>
        <button :disabled="saving">{{ saving ? 'Mentés...' : 'Cég mentése' }}</button>
      </form>
    </article>

    <LoadingBox v-if="loading" />
    <EmptyState v-else-if="companies.length === 0" title="Még nincs cég mentve" message="Mentsd el az első céget, hogy később jelentkezést kapcsolhass hozzá." action-text="Első cég létrehozása" @action="scrollToId('new-company')" />

    <div v-else class="card-list">
      <article v-for="company in companies" :key="company.id" class="card row-card">
        <form v-if="editingId === company.id" class="form edit-form" @submit.prevent="updateCompany(company.id)">
          <div class="grid-form">
            <label>Cégnév<input v-model="editForm.name" required /></label>
            <label>Weboldal<input v-model="editForm.website" /></label>
            <label>Helyszín<input v-model="editForm.location" /></label>
            <label>Kapcsolattartó<input v-model="editForm.contactName" /></label>
            <label>Email<input v-model="editForm.contactEmail" type="email" /></label>
            <label>Telefon<input v-model="editForm.contactPhone" /></label>
            <label>Jegyzet<input v-model="editForm.notes" /></label>
          </div>
          <div class="actions"><button :disabled="saving">{{ saving ? 'Mentés...' : 'Mentés' }}</button><button type="button" class="secondary" @click="cancelEdit">Mégsem</button></div>
        </form>
        <template v-else>
          <div>
            <h2>{{ company.name }}</h2>
            <p>{{ company.location || 'Nincs helyszín' }} <a v-if="company.website" :href="company.website" target="_blank" rel="noreferrer">{{ company.website }}</a></p>
            <p class="muted">{{ company.contactName || 'Nincs kapcsolattartó' }} {{ company.contactEmail || '' }}</p>
            <p v-if="company.notes">{{ company.notes }}</p>
          </div>
          <div class="actions"><button class="secondary" @click="startEdit(company)">Szerkesztés</button><button class="danger" @click="deleteTarget = company">Törlés</button></div>
        </template>
      </article>
    </div>

    <ConfirmModal :show="Boolean(deleteTarget)" title="Cég törlése" :message="`Biztosan törlöd ezt a céget: ${deleteTarget?.name || ''}?`" @cancel="deleteTarget = null" @confirm="confirmDelete" />
  </section>
</template>
