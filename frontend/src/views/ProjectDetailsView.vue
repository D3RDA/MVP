<script setup>
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import api from '../services/api'
import ErrorMessage from '../components/ErrorMessage.vue'
import LoadingBox from '../components/LoadingBox.vue'
import ConfirmModal from '../components/ConfirmModal.vue'
import StatusBadge from '../components/StatusBadge.vue'
import EmptyState from '../components/EmptyState.vue'
import { useToast } from '../composables/useToast'
import { taskStatusOptions } from '../utils/labels'

const route = useRoute()
const { show: toast } = useToast()
const project = ref(null)
const tasks = ref([])
const error = ref('')
const loading = ref(true)
const saving = ref(false)
const editingId = ref(null)
const editForm = ref({})
const deleteTarget = ref(null)
const taskForm = ref({ title: '', description: '', dueDate: '', estimatedHours: null, status: 'todo' })

async function load() {
  loading.value = true
  const projectId = route.params.id
  try {
    const [projectResponse, tasksResponse] = await Promise.all([
      api.get(`/projects/${projectId}`),
      api.get(`/projects/${projectId}/tasks`)
    ])
    project.value = projectResponse.data
    tasks.value = tasksResponse.data
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült betölteni a projekt részleteit.'
  } finally {
    loading.value = false
  }
}

async function createTask() {
  saving.value = true
  try {
    await api.post(`/projects/${route.params.id}/tasks`, clean(taskForm.value))
    taskForm.value = { title: '', description: '', dueDate: '', estimatedHours: null, status: 'todo' }
    toast('Feladat sikeresen mentve ✓')
    await load()
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült menteni a feladatot.'
  } finally {
    saving.value = false
  }
}

function startEdit(task) {
  editingId.value = task.id
  editForm.value = {
    title: task.title || '',
    description: task.description || '',
    dueDate: task.dueDate || '',
    estimatedHours: task.estimatedHours ?? null,
    status: task.status || 'todo'
  }
}

function cancelEdit() {
  editingId.value = null
  editForm.value = {}
}

async function updateTask(id) {
  saving.value = true
  try {
    await api.put(`/projects/${route.params.id}/tasks/${id}`, clean(editForm.value))
    cancelEdit()
    toast('Feladat sikeresen módosítva ✓')
    await load()
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült módosítani a feladatot.'
  } finally {
    saving.value = false
  }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  await api.delete(`/projects/${route.params.id}/tasks/${deleteTarget.value.id}`)
  deleteTarget.value = null
  toast('Feladat törölve ✓')
  await load()
}

function clean(value) {
  return {
    ...value,
    dueDate: value.dueDate || null,
    estimatedHours: value.estimatedHours === '' ? null : value.estimatedHours
  }
}


async function loadDescriptionFromFile(event, target) {
  const file = event.target.files?.[0]
  if (!file) return

  if (!file.name.toLowerCase().endsWith('.txt')) {
    error.value = 'Csak .txt fájlt tudsz beolvasni a feladat leírásába.'
    event.target.value = ''
    return
  }

  const text = await file.text()
  if (target === 'new') {
    taskForm.value.description = text
  } else {
    editForm.value.description = text
  }
  event.target.value = ''
}

function scrollToId(id) {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' })
}

onMounted(load)
</script>

<template>
  <LoadingBox v-if="loading" />
  <section v-else-if="project">
    <h1>{{ project.title }}</h1>
    <p class="muted">{{ project.description }}</p>
    <ErrorMessage :message="error" />

    <article class="card" id="new-task">
      <h2>Új feladat</h2>
      <form class="form grid-form" @submit.prevent="createTask">
        <label>Cím<input v-model="taskForm.title" required /><span v-if="!taskForm.title" class="field-error">A cím kötelező.</span></label>
        <label class="wide-field">Leírás / dokumentum tartalma<textarea v-model="taskForm.description" class="large-textarea mono-textarea" rows="10" placeholder="Ide beillesztheted például a könyvlistát, tanulási jegyzetet vagy hosszabb feladatleírást." /><span class="char-counter">{{ taskForm.description.length }} karakter</span></label>
        <label class="wide-field">TXT fájl beolvasása a leírásba<input type="file" accept=".txt,text/plain" @change="loadDescriptionFromFile($event, 'new')" /></label>
        <label>Határidő<input v-model="taskForm.dueDate" type="date" /></label>
        <label>Becsült óra<input v-model.number="taskForm.estimatedHours" type="number" step="0.25" /></label>
        <label>Státusz<select v-model="taskForm.status"><option v-for="option in taskStatusOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
        <button :disabled="saving">{{ saving ? 'Mentés...' : 'Feladat mentése' }}</button>
      </form>
    </article>

    <EmptyState v-if="tasks.length === 0" title="Még nincs feladat" message="Bontsd kisebb feladatokra a projektet." action-text="Első feladat létrehozása" @action="scrollToId('new-task')" />

    <div v-else class="card-list">
      <article v-for="task in tasks" :key="task.id" class="card row-card">
        <form v-if="editingId === task.id" class="form edit-form" @submit.prevent="updateTask(task.id)">
          <div class="grid-form">
            <label>Cím<input v-model="editForm.title" required /></label>
            <label class="wide-field">Leírás / dokumentum tartalma<textarea v-model="editForm.description" class="large-textarea mono-textarea" rows="10" /><span class="char-counter">{{ (editForm.description || '').length }} karakter</span></label>
            <label class="wide-field">TXT fájl beolvasása a leírásba<input type="file" accept=".txt,text/plain" @change="loadDescriptionFromFile($event, 'edit')" /></label>
            <label>Határidő<input v-model="editForm.dueDate" type="date" /></label>
            <label>Becsült óra<input v-model.number="editForm.estimatedHours" type="number" step="0.25" /></label>
            <label>Státusz<select v-model="editForm.status"><option v-for="option in taskStatusOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
          </div>
          <div class="actions"><button :disabled="saving">{{ saving ? 'Mentés...' : 'Mentés' }}</button><button type="button" class="secondary" @click="cancelEdit">Mégsem</button></div>
        </form>
        <template v-else>
          <div class="task-card-body"><h2>{{ task.title }}</h2><div v-if="task.description" class="rich-text task-description">{{ task.description }}</div><p v-else class="muted">Nincs leírás megadva.</p><StatusBadge :value="task.status" /></div>
          <div class="actions"><button class="secondary" @click="startEdit(task)">Szerkesztés</button><button class="danger" @click="deleteTarget = task">Törlés</button></div>
        </template>
      </article>
    </div>

    <ConfirmModal :show="Boolean(deleteTarget)" title="Feladat törlése" :message="`Biztosan törlöd ezt a feladatot: ${deleteTarget?.title || ''}?`" @cancel="deleteTarget = null" @confirm="confirmDelete" />
  </section>
</template>
