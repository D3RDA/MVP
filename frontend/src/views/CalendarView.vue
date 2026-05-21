<script setup>
import { computed, onMounted, ref } from 'vue'
import FullCalendar from '@fullcalendar/vue3'
import dayGridPlugin from '@fullcalendar/daygrid'
import timeGridPlugin from '@fullcalendar/timegrid'
import huLocale from '@fullcalendar/core/locales/hu'
import api from '../services/api'
import ErrorMessage from '../components/ErrorMessage.vue'
import LoadingBox from '../components/LoadingBox.vue'
import StatusBadge from '../components/StatusBadge.vue'
import ConfirmModal from '../components/ConfirmModal.vue'
import EmptyState from '../components/EmptyState.vue'
import { useToast } from '../composables/useToast'
import { eventTypeLabels, eventTypeOptions } from '../utils/labels'

const { show: toast } = useToast()
const events = ref([])
const projects = ref([])
const error = ref('')
const loading = ref(true)
const saving = ref(false)
const typeFilter = ref('')
const selectedEvent = ref(null)
const editingId = ref(null)
const editForm = ref({})
const deleteTarget = ref(null)
const showPast = ref(false)
const form = ref({ projectId: '', taskId: null, title: '', description: '', startDateTime: '', endDateTime: '', eventType: 'other' })

const now = computed(() => new Date())
const filteredEvents = computed(() => events.value.filter((event) => !typeFilter.value || event.eventType === typeFilter.value))
const futureEvents = computed(() => filteredEvents.value.filter((event) => new Date(event.endDateTime) >= now.value))
const pastEvents = computed(() => filteredEvents.value.filter((event) => new Date(event.endDateTime) < now.value))

const calendarEvents = computed(() => filteredEvents.value.map((event) => ({
  id: String(event.id),
  title: event.title,
  start: event.startDateTime,
  end: event.endDateTime,
  backgroundColor: colorFor(event.eventType),
  borderColor: colorFor(event.eventType),
  extendedProps: event
})))

const calendarOptions = computed(() => ({
  plugins: [dayGridPlugin, timeGridPlugin],
  initialView: 'dayGridMonth',
  headerToolbar: { left: 'prev,next today', center: 'title', right: 'dayGridMonth,timeGridWeek' },
  locale: huLocale,
  events: calendarEvents.value,
  eventClick: (info) => { selectedEvent.value = info.event.extendedProps }
}))

async function load() {
  loading.value = true
  try {
    const [eventsResponse, projectsResponse] = await Promise.all([api.get('/calendarevents'), api.get('/projects')])
    events.value = eventsResponse.data
    projects.value = projectsResponse.data
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült betölteni az eseményeket.'
  } finally {
    loading.value = false
  }
}

async function createEvent() {
  error.value = ''
  saving.value = true
  try {
    await api.post('/calendarevents', clean(form.value))
    form.value = { projectId: '', taskId: null, title: '', description: '', startDateTime: '', endDateTime: '', eventType: 'other' }
    toast('Esemény sikeresen mentve ✓')
    await load()
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült menteni az eseményt.'
  } finally {
    saving.value = false
  }
}

function openDetails(event) {
  selectedEvent.value = event
}

function startEdit(event) {
  editingId.value = event.id
  editForm.value = {
    projectId: event.projectId || '',
    taskId: event.taskId || null,
    title: event.title || '',
    description: event.description || '',
    startDateTime: toDatetimeLocal(event.startDateTime),
    endDateTime: toDatetimeLocal(event.endDateTime),
    eventType: event.eventType || 'other'
  }
}

function cancelEdit() {
  editingId.value = null
  editForm.value = {}
}

async function updateEvent(id) {
  error.value = ''
  saving.value = true
  try {
    await api.put(`/calendarevents/${id}`, clean(editForm.value))
    cancelEdit()
    selectedEvent.value = null
    toast('Esemény sikeresen módosítva ✓')
    await load()
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült módosítani az eseményt.'
  } finally {
    saving.value = false
  }
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  await api.delete(`/calendarevents/${deleteTarget.value.id}`)
  deleteTarget.value = null
  selectedEvent.value = null
  toast('Esemény törölve ✓')
  await load()
}

function clean(value) {
  return {
    projectId: value.projectId ? Number(value.projectId) : null,
    taskId: null,
    title: value.title,
    description: value.description || null,
    startDateTime: value.startDateTime,
    endDateTime: value.endDateTime,
    eventType: value.eventType
  }
}

function toDatetimeLocal(value) {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return ''
  const pad = (n) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`
}

function formatDate(value) {
  return value ? new Date(value).toLocaleString('hu-HU') : 'Nincs dátum'
}

function colorFor(type) {
  return {
    interview: '#f97316',
    deadline: '#dc2626',
    project: '#2563eb',
    personal: '#16a34a',
    task: '#7c3aed',
    other: '#64748b'
  }[type] || '#64748b'
}

function scrollToId(id) {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' })
}

onMounted(load)
</script>

<template>
  <section>
    <h1>Naptár</h1>
    <ErrorMessage :message="error" />
    <article class="card" id="new-event">
      <h2>Új esemény</h2>
      <form class="form grid-form" @submit.prevent="createEvent">
        <label>Projekt<select v-model="form.projectId"><option value="">Nincs projekthez kötve</option><option v-for="project in projects" :key="project.id" :value="project.id">{{ project.title }}</option></select></label>
        <label>Cím<input v-model="form.title" required /><span v-if="!form.title" class="field-error">A cím kötelező.</span></label>
        <label>Leírás<input v-model="form.description" /></label>
        <label>Kezdés<input v-model="form.startDateTime" type="datetime-local" required /></label>
        <label>Befejezés<input v-model="form.endDateTime" type="datetime-local" required /></label>
        <label>Típus<select v-model="form.eventType"><option v-for="option in eventTypeOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
        <button :disabled="saving">{{ saving ? 'Mentés...' : 'Esemény mentése' }}</button>
      </form>
    </article>

    <div class="toolbar card compact-card">
      <label>Eseménytípus<select v-model="typeFilter"><option value="">Összes</option><option v-for="option in eventTypeOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
    </div>

    <LoadingBox v-if="loading" />
    <template v-else>
      <article class="card calendar-card">
        <FullCalendar :options="calendarOptions" />
      </article>

      <h2>Jövőbeni események</h2>
      <EmptyState v-if="futureEvents.length === 0" title="Nincs közelgő esemény" message="Hozz létre eseményt, határidőt vagy interjúidőpontot." action-text="Új esemény" @action="scrollToId('new-event')" />
      <div v-else class="card-list">
        <article v-for="event in futureEvents" :key="event.id" class="card row-card">
          <div><h2>{{ event.title }}</h2><p>{{ formatDate(event.startDateTime) }} — {{ formatDate(event.endDateTime) }}</p><StatusBadge :value="event.eventType" /></div>
          <div class="actions"><button class="secondary" @click="openDetails(event)">Részletek</button><button class="secondary" @click="startEdit(event)">Szerkesztés</button><button class="danger" @click="deleteTarget = event">Törlés</button></div>
        </article>
      </div>

      <details class="past-events" :open="showPast" @toggle="showPast = $event.target.open">
        <summary>Korábbi események ({{ pastEvents.length }})</summary>
        <div class="card-list">
          <article v-for="event in pastEvents" :key="event.id" class="card row-card muted-card">
            <div><h2>{{ event.title }}</h2><p>{{ formatDate(event.startDateTime) }} — {{ formatDate(event.endDateTime) }}</p><StatusBadge :value="event.eventType" /></div>
            <div class="actions"><button class="secondary" @click="openDetails(event)">Részletek</button><button class="secondary" @click="startEdit(event)">Szerkesztés</button><button class="danger" @click="deleteTarget = event">Törlés</button></div>
          </article>
        </div>
      </details>
    </template>

    <Teleport to="body">
      <div v-if="selectedEvent" class="modal-backdrop" @click.self="selectedEvent = null">
        <section class="modal-card">
          <h2>{{ selectedEvent.title }}</h2>
          <p>{{ formatDate(selectedEvent.startDateTime) }} — {{ formatDate(selectedEvent.endDateTime) }}</p>
          <p v-if="selectedEvent.description">{{ selectedEvent.description }}</p>
          <StatusBadge :value="selectedEvent.eventType" />
          <div class="modal-actions"><button class="secondary" @click="startEdit(selectedEvent); selectedEvent = null">Szerkesztés</button><button class="danger" @click="deleteTarget = selectedEvent">Törlés</button><button @click="selectedEvent = null">Bezárás</button></div>
        </section>
      </div>
    </Teleport>

    <Teleport to="body">
      <div v-if="editingId" class="modal-backdrop" @click.self="cancelEdit">
        <section class="modal-card wide-modal">
          <h2>Esemény szerkesztése</h2>
          <form class="form" @submit.prevent="updateEvent(editingId)">
            <div class="grid-form">
              <label>Projekt<select v-model="editForm.projectId"><option value="">Nincs projekthez kötve</option><option v-for="project in projects" :key="project.id" :value="project.id">{{ project.title }}</option></select></label>
              <label>Cím<input v-model="editForm.title" required /></label>
              <label>Kezdés<input v-model="editForm.startDateTime" type="datetime-local" required /></label>
              <label>Befejezés<input v-model="editForm.endDateTime" type="datetime-local" required /></label>
              <label>Típus<select v-model="editForm.eventType"><option v-for="option in eventTypeOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
            </div>
            <label>Leírás<textarea v-model="editForm.description" rows="3"></textarea></label>
            <div class="actions"><button :disabled="saving">{{ saving ? 'Mentés...' : 'Mentés' }}</button><button type="button" class="secondary" @click="cancelEdit">Mégsem</button></div>
          </form>
        </section>
      </div>
    </Teleport>

    <ConfirmModal :show="Boolean(deleteTarget)" title="Esemény törlése" :message="`Biztosan törlöd ezt az eseményt: ${deleteTarget?.title || ''}?`" @cancel="deleteTarget = null" @confirm="confirmDelete" />
  </section>
</template>
