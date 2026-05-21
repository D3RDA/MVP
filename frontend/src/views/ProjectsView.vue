<script setup>
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import api from '../services/api'
import ErrorMessage from '../components/ErrorMessage.vue'
import LoadingBox from '../components/LoadingBox.vue'
import StatusBadge from '../components/StatusBadge.vue'
import ConfirmModal from '../components/ConfirmModal.vue'
import EmptyState from '../components/EmptyState.vue'
import { useToast } from '../composables/useToast'
import { priorityOptions, projectStatusOptions } from '../utils/labels'

const { show: toast } = useToast()
const projects = ref([])
const error = ref('')
const loading = ref(true)
const saving = ref(false)
const editingId = ref(null)
const editForm = ref({})
const deleteTarget = ref(null)
const statusFilter = ref('')
const searchText = ref('')
const form = ref({ title: '', description: '', startDate: '', endDate: '', dailyHours: null, status: 'planned', priority: 'medium' })

const filteredProjects = computed(() => {
  const q = searchText.value.trim().toLowerCase()
  return projects.value.filter((project) => {
    const matchesStatus = !statusFilter.value || project.status === statusFilter.value
    const matchesSearch = !q || project.title?.toLowerCase().includes(q)
    return matchesStatus && matchesSearch
  })
})

async function load() {
  loading.value = true
  try {
    const { data } = await api.get('/projects')
    projects.value = data
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült betölteni a projekteket.'
  } finally {
    loading.value = false
  }
}

async function createProject() {
  error.value = ''
  saving.value = true
  try {
    await api.post('/projects', clean(form.value))
    form.value = { title: '', description: '', startDate: '', endDate: '', dailyHours: null, status: 'planned', priority: 'medium' }
    toast('Projekt sikeresen létrehozva ✓')
    await load()
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült létrehozni a projektet.'
  } finally {
    saving.value = false
  }
}

function startEdit(project) {
  editingId.value = project.id
  editForm.value = {
    title: project.title || '',
    description: project.description || '',
    startDate: project.startDate || '',
    endDate: project.endDate || '',
    dailyHours: project.dailyHours ?? null,
    status: project.status || 'planned',
    priority: project.priority || 'medium'
  }
}

function cancelEdit() {
  editingId.value = null
  editForm.value = {}
}

async function updateProject(id) {
  error.value = ''
  saving.value = true
  try {
    await api.put(`/projects/${id}`, clean(editForm.value))
    cancelEdit()
    toast('Projekt sikeresen mentve ✓')
    await load()
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült módosítani a projektet.'
  } finally {
    saving.value = false
  }
}

function askDelete(project) {
  deleteTarget.value = project
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  await api.delete(`/projects/${deleteTarget.value.id}`)
  deleteTarget.value = null
  toast('Projekt törölve ✓')
  await load()
}

function clean(value) {
  return {
    ...value,
    startDate: value.startDate || null,
    endDate: value.endDate || null,
    dailyHours: value.dailyHours === '' ? null : value.dailyHours
  }
}

function scrollToId(id) {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' })
}

onMounted(load)
</script>

<template>
  <section>
    <h1>Projektek</h1>
    <ErrorMessage :message="error" />

    <article class="card" id="new-project">
      <h2>Új projekt</h2>
      <form class="form grid-form" @submit.prevent="createProject">
        <label>Cím<input v-model="form.title" required maxlength="150" /><span v-if="!form.title" class="field-error">A cím kötelező.</span></label>
        <label>Leírás<input v-model="form.description" /></label>
        <label>Kezdés<input v-model="form.startDate" type="date" /></label>
        <label>Befejezés<input v-model="form.endDate" type="date" /></label>
        <label>Napi óra<input v-model.number="form.dailyHours" type="number" step="0.25" min="0" /></label>
        <label>Státusz<select v-model="form.status"><option v-for="option in projectStatusOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
        <label>Prioritás<select v-model="form.priority"><option v-for="option in priorityOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
        <button :disabled="saving">{{ saving ? 'Mentés...' : 'Projekt mentése' }}</button>
      </form>
    </article>

    <div class="toolbar card compact-card">
      <label>Keresés<input v-model="searchText" placeholder="Projekt címe..." /></label>
      <label>Státusz<select v-model="statusFilter"><option value="">Összes</option><option value="planned">Tervezett</option><option value="in_progress">Folyamatban</option><option value="completed">Befejezett</option></select></label>
    </div>

    <LoadingBox v-if="loading" />
    <EmptyState v-else-if="filteredProjects.length === 0" title="Még nincs megjeleníthető projekt" message="Hozd létre az első projektedet, vagy módosítsd a szűrőket." action-text="Új projekt létrehozása" @action="scrollToId('new-project')" />

    <div v-else class="card-list">
      <article v-for="project in filteredProjects" :key="project.id" class="card row-card">
        <form v-if="editingId === project.id" class="form edit-form" @submit.prevent="updateProject(project.id)">
          <div class="grid-form">
            <label>Cím<input v-model="editForm.title" required maxlength="150" /></label>
            <label>Leírás<input v-model="editForm.description" /></label>
            <label>Kezdés<input v-model="editForm.startDate" type="date" /></label>
            <label>Befejezés<input v-model="editForm.endDate" type="date" /></label>
            <label>Napi óra<input v-model.number="editForm.dailyHours" type="number" step="0.25" min="0" /></label>
            <label>Státusz<select v-model="editForm.status"><option v-for="option in projectStatusOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
            <label>Prioritás<select v-model="editForm.priority"><option v-for="option in priorityOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          </div>
          <div class="actions"><button :disabled="saving">{{ saving ? 'Mentés...' : 'Mentés' }}</button><button type="button" class="secondary" @click="cancelEdit">Mégsem</button></div>
        </form>
        <template v-else>
          <div>
            <h2><RouterLink :to="`/projects/${project.id}`">{{ project.title }}</RouterLink></h2>
            <p>{{ project.description || 'Nincs leírás.' }}</p>
            <p class="muted">{{ project.startDate || 'Nincs kezdés' }} — {{ project.endDate || 'Nincs befejezés' }}</p>
            <div class="inline"><StatusBadge :value="project.status" /><StatusBadge :value="project.priority" /></div>
          </div>
          <div class="actions"><button class="secondary" @click="startEdit(project)">Szerkesztés</button><button class="danger" @click="askDelete(project)">Törlés</button></div>
        </template>
      </article>
    </div>

    <ConfirmModal :show="Boolean(deleteTarget)" title="Projekt törlése" :message="`Biztosan törlöd ezt a projektet: ${deleteTarget?.title || ''}?`" @cancel="deleteTarget = null" @confirm="confirmDelete" />
  </section>
</template>
