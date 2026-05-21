<script setup>
import { computed, onMounted, ref } from 'vue'
import api from '../services/api'
import ErrorMessage from '../components/ErrorMessage.vue'
import LoadingBox from '../components/LoadingBox.vue'
import ConfirmModal from '../components/ConfirmModal.vue'
import EmptyState from '../components/EmptyState.vue'
import { useToast } from '../composables/useToast'

const { show: toast } = useToast()
const notes = ref([])
const projects = ref([])
const companies = ref([])
const error = ref('')
const loading = ref(true)
const saving = ref(false)
const editingId = ref(null)
const editForm = ref({})
const deleteTarget = ref(null)
const searchText = ref('')
const form = ref({ projectId: '', companyId: '', taskId: null, jobApplicationId: null, title: '', content: '' })

const filteredNotes = computed(() => {
  const q = searchText.value.trim().toLowerCase()
  return notes.value.filter((note) => !q || `${note.title || ''} ${note.content || ''}`.toLowerCase().includes(q))
})

async function load() {
  loading.value = true
  try {
    const [notesResponse, projectsResponse, companiesResponse] = await Promise.all([
      api.get('/notes'), api.get('/projects'), api.get('/companies')
    ])
    notes.value = notesResponse.data
    projects.value = projectsResponse.data
    companies.value = companiesResponse.data
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült betölteni a jegyzeteket.'
  } finally {
    loading.value = false
  }
}

async function createNote() {
  error.value = ''
  saving.value = true
  try {
    await api.post('/notes', clean(form.value))
    form.value = { projectId: '', companyId: '', taskId: null, jobApplicationId: null, title: '', content: '' }
    toast('Jegyzet sikeresen mentve ✓')
    await load()
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült menteni a jegyzetet.'
  } finally {
    saving.value = false
  }
}

function startEdit(note) {
  editingId.value = note.id
  editForm.value = {
    projectId: note.projectId || '',
    companyId: note.companyId || '',
    taskId: note.taskId || null,
    jobApplicationId: note.jobApplicationId || null,
    title: note.title || '',
    content: note.content || ''
  }
}

function cancelEdit() {
  editingId.value = null
  editForm.value = {}
}

async function updateNote(id) {
  error.value = ''
  saving.value = true
  try {
    await api.put(`/notes/${id}`, clean(editForm.value))
    cancelEdit()
    toast('Jegyzet sikeresen módosítva ✓')
    await load()
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült módosítani a jegyzetet.'
  } finally {
    saving.value = false
  }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  await api.delete(`/notes/${deleteTarget.value.id}`)
  deleteTarget.value = null
  toast('Jegyzet törölve ✓')
  await load()
}

function clean(value) {
  return {
    projectId: value.projectId ? Number(value.projectId) : null,
    companyId: value.companyId ? Number(value.companyId) : null,
    taskId: null,
    jobApplicationId: null,
    title: value.title || null,
    content: value.content
  }
}

function scrollToId(id) {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' })
}

onMounted(load)
</script>

<template>
  <section>
    <h1>Jegyzetek</h1>
    <ErrorMessage :message="error" />
    <article class="card" id="new-note">
      <h2>Új jegyzet</h2>
      <form class="form" @submit.prevent="createNote">
        <div class="grid-form">
          <label>Projekt<select v-model="form.projectId"><option value="">Nincs projekt</option><option v-for="project in projects" :key="project.id" :value="project.id">{{ project.title }}</option></select></label>
          <label>Cég<select v-model="form.companyId"><option value="">Nincs cég</option><option v-for="company in companies" :key="company.id" :value="company.id">{{ company.name }}</option></select></label>
          <label>Cím<input v-model="form.title" /></label>
        </div>
        <label>Tartalom<textarea v-model="form.content" required rows="5"></textarea><span v-if="!form.content" class="field-error">A tartalom kötelező.</span></label>
        <button :disabled="saving">{{ saving ? 'Mentés...' : 'Jegyzet mentése' }}</button>
      </form>
    </article>

    <div class="toolbar card compact-card"><label>Keresés<input v-model="searchText" placeholder="Cím vagy tartalom..." /></label></div>

    <LoadingBox v-if="loading" />
    <EmptyState v-else-if="filteredNotes.length === 0" title="Nincs megjeleníthető jegyzet" message="Írd meg az első jegyzetedet, vagy módosítsd a keresést." action-text="Első jegyzet létrehozása" @action="scrollToId('new-note')" />

    <div v-else class="card-list">
      <article v-for="note in filteredNotes" :key="note.id" class="card row-card">
        <form v-if="editingId === note.id" class="form edit-form" @submit.prevent="updateNote(note.id)">
          <div class="grid-form">
            <label>Projekt<select v-model="editForm.projectId"><option value="">Nincs projekt</option><option v-for="project in projects" :key="project.id" :value="project.id">{{ project.title }}</option></select></label>
            <label>Cég<select v-model="editForm.companyId"><option value="">Nincs cég</option><option v-for="company in companies" :key="company.id" :value="company.id">{{ company.name }}</option></select></label>
            <label>Cím<input v-model="editForm.title" /></label>
          </div>
          <label>Tartalom<textarea v-model="editForm.content" required rows="5"></textarea></label>
          <div class="actions"><button :disabled="saving">{{ saving ? 'Mentés...' : 'Mentés' }}</button><button type="button" class="secondary" @click="cancelEdit">Mégsem</button></div>
        </form>
        <template v-else>
          <div><h2>{{ note.title || 'Cím nélküli jegyzet' }}</h2><p>{{ note.content }}</p></div>
          <div class="actions"><button class="secondary" @click="startEdit(note)">Szerkesztés</button><button class="danger" @click="deleteTarget = note">Törlés</button></div>
        </template>
      </article>
    </div>

    <ConfirmModal :show="Boolean(deleteTarget)" title="Jegyzet törlése" :message="`Biztosan törlöd ezt a jegyzetet: ${deleteTarget?.title || 'Cím nélküli jegyzet'}?`" @cancel="deleteTarget = null" @confirm="confirmDelete" />
  </section>
</template>
