<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import api from '../services/api'
import ErrorMessage from '../components/ErrorMessage.vue'
import LoadingBox from '../components/LoadingBox.vue'
import StatusBadge from '../components/StatusBadge.vue'
import ConfirmModal from '../components/ConfirmModal.vue'
import EmptyState from '../components/EmptyState.vue'
import { useToast } from '../composables/useToast'
import { jobStatusOptions } from '../utils/labels'

const { show: toast } = useToast()
const jobs = ref([])
const companies = ref([])
const error = ref('')
const loading = ref(true)
const saving = ref(false)
const editingId = ref(null)
const editForm = ref({})
const deleteTarget = ref(null)
const statusFilter = ref('')
const companyFilter = ref('')
const search = ref('')
const dateField = ref('applicationDate')
const dateFrom = ref('')
const dateTo = ref('')
const datePreset = ref('')

const dateFieldLabels = {
  applicationDate: 'jelentkezési',
  interviewDateTime: 'interjú'
}
const viewMode = ref(localStorage.getItem('jobs_view_mode') || 'list')
const showCreateForm = ref(false)
const expandedJobId = ref(null)
const form = ref(emptyForm())

const kanbanColumns = [
  { key: 'new', label: 'Jelentkezve', statuses: ['applied'] },
  { key: 'waiting', label: 'Válaszra vár', statuses: ['waiting_response'] },
  { key: 'interview', label: 'Interjú', statuses: ['interview_scheduled', 'first_interview', 'second_interview'] },
  { key: 'offer', label: 'Ajánlat', statuses: ['offer_received'] },
  { key: 'closed', label: 'Lezárva / elutasítva', statuses: ['rejected', 'declined', 'closed'] }
]
const statusFlow = ['applied', 'waiting_response', 'interview_scheduled', 'first_interview', 'second_interview', 'offer_received', 'closed']

// A datumokat helyi ido szerinti YYYY-MM-DD kulcsra hozzuk, hogy a szoveges
// osszehasonlitas is helyes legyen, es ne csusszon el idozona miatt egy nappal.
function dateKey(value) {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return ''
  const pad = (n) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}

const dateRangeActive = computed(() => Boolean(dateFrom.value || dateTo.value))

function matchesDateRange(job) {
  if (!dateRangeActive.value) return true
  const key = dateKey(job[dateField.value])
  if (!key) return false
  if (dateFrom.value && key < dateFrom.value) return false
  if (dateTo.value && key > dateTo.value) return false
  return true
}

const filteredJobs = computed(() => jobs.value.filter((job) => {
  const q = search.value.trim().toLowerCase()
  const matchesText = !q || [job.companyName, job.positionTitle, job.location, job.website, job.contactName, job.contactEmail, job.contactPhone, job.salaryRange, job.experienceNotes, job.nextStep]
    .some((value) => String(value || '').toLowerCase().includes(q))
  const matchesStatus = !statusFilter.value || job.status === statusFilter.value
  const matchesCompany = !companyFilter.value || job.companyId === Number(companyFilter.value)
  return matchesText && matchesStatus && matchesCompany && matchesDateRange(job)
}))

// Ha datumra szurunk, a kitoltetlen datumu rekordok eltunnek. Ezt kimondjuk,
// hogy ne ugy tunjon, mintha adat veszett volna el.
const hiddenForMissingDate = computed(() => {
  if (!dateRangeActive.value) return 0
  return jobs.value.filter((job) => !dateKey(job[dateField.value])).length
})

const anyFilterActive = computed(() =>
  Boolean(search.value || statusFilter.value || companyFilter.value || dateFrom.value || dateTo.value))

function toInputDate(date) {
  const pad = (n) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
}

const datePresets = [
  { key: '7', label: 'Utolsó 7 nap' },
  { key: '30', label: 'Utolsó 30 nap' },
  { key: '90', label: 'Utolsó 90 nap' },
  { key: 'thisMonth', label: 'Ez a hónap' },
  { key: 'lastMonth', label: 'Előző hónap' },
  { key: 'thisYear', label: 'Ez az év' }
]

function computeRange(key) {
  const today = new Date()
  let from
  let to = today
  if (key === 'thisMonth') {
    from = new Date(today.getFullYear(), today.getMonth(), 1)
  } else if (key === 'lastMonth') {
    from = new Date(today.getFullYear(), today.getMonth() - 1, 1)
    to = new Date(today.getFullYear(), today.getMonth(), 0)
  } else if (key === 'thisYear') {
    from = new Date(today.getFullYear(), 0, 1)
  } else {
    from = new Date(today)
    from.setDate(from.getDate() - (Number(key) - 1))
  }
  return { from: toInputDate(from), to: toInputDate(to) }
}

function applyPreset(key) {
  // Ugyanarra a gombra ujra kattintva a szuro kikapcsol.
  if (datePreset.value === key) {
    datePreset.value = ''
    dateFrom.value = ''
    dateTo.value = ''
    return
  }
  const range = computeRange(key)
  datePreset.value = key
  dateFrom.value = range.from
  dateTo.value = range.to
}

function clearFilters() {
  search.value = ''
  statusFilter.value = ''
  companyFilter.value = ''
  datePreset.value = ''
  dateFrom.value = ''
  dateTo.value = ''
}

// Ha kezzel irja at a datumot, a gyorsszuro kijelolese mar nem igaz ra.
watch([dateFrom, dateTo], ([from, to]) => {
  if (!datePreset.value) return
  const range = computeRange(datePreset.value)
  if (range.from !== from || range.to !== to) datePreset.value = ''
})

function emptyForm() {
  return {
    companyName: '',
    website: '',
    location: '',
    contactName: '',
    contactEmail: '',
    contactPhone: '',
    companyNotes: '',
    positionTitle: '',
    applicationDate: '',
    status: 'applied',
    jobAdUrl: '',
    salaryRange: '',
    interviewDateTime: '',
    experienceNotes: '',
    nextStep: ''
  }
}

function jobsByColumn(column) {
  return filteredJobs.value.filter((job) => column.statuses.includes(job.status))
}

watch(viewMode, (value) => localStorage.setItem('jobs_view_mode', value))

async function load() {
  loading.value = true
  try {
    const [jobsResponse, companiesResponse] = await Promise.all([api.get('/jobapplications'), api.get('/companies')])
    jobs.value = jobsResponse.data
    companies.value = companiesResponse.data
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült betölteni az álláskövetőt.'
  } finally {
    loading.value = false
  }
}

async function createJob() {
  error.value = ''
  saving.value = true
  try {
    await api.post('/jobapplications/with-company', cleanWithCompany(form.value))
    form.value = emptyForm()
    showCreateForm.value = false
    toast('Jelentkezés sikeresen mentve ✓')
    await load()
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült menteni az állásjelentkezést.'
  } finally {
    saving.value = false
  }
}

function startEdit(job) {
  editingId.value = job.id
  editForm.value = {
    companyName: job.companyName || '',
    website: job.website || '',
    location: job.location || '',
    contactName: job.contactName || '',
    contactEmail: job.contactEmail || '',
    contactPhone: job.contactPhone || '',
    companyNotes: job.companyNotes || '',
    positionTitle: job.positionTitle || '',
    applicationDate: job.applicationDate || '',
    status: job.status || 'applied',
    jobAdUrl: job.jobAdUrl || '',
    salaryRange: job.salaryRange || '',
    interviewDateTime: toDatetimeLocal(job.interviewDateTime),
    experienceNotes: job.experienceNotes || '',
    nextStep: job.nextStep || ''
  }
}

function cancelEdit() {
  editingId.value = null
  editForm.value = {}
}

async function updateJob(id, payload = editForm.value, successText = 'Jelentkezés sikeresen módosítva ✓') {
  error.value = ''
  saving.value = true
  try {
    await api.put(`/jobapplications/${id}/with-company`, cleanWithCompany(payload))
    cancelEdit()
    toast(successText)
    await load()
  } catch (err) {
    error.value = err.response?.data || 'Nem sikerült módosítani az állásjelentkezést.'
  } finally {
    saving.value = false
  }
}

function updateStatus(job, status) {
  updateJob(job.id, { ...job, companyName: job.companyName || '', companyNotes: job.companyNotes || '', status }, 'Státusz frissítve ✓')
}

function moveStatus(job, direction) {
  const index = statusFlow.indexOf(job.status)
  const nextIndex = Math.min(Math.max(index + direction, 0), statusFlow.length - 1)
  updateStatus(job, statusFlow[nextIndex])
}

async function confirmDelete() {
  if (!deleteTarget.value) return
  await api.delete(`/jobapplications/${deleteTarget.value.id}`)
  deleteTarget.value = null
  toast('Jelentkezés törölve ✓')
  await load()
}

function cleanWithCompany(value) {
  return {
    companyName: value.companyName,
    website: value.website || null,
    location: value.location || null,
    contactName: value.contactName || null,
    contactEmail: value.contactEmail || null,
    contactPhone: value.contactPhone || null,
    companyNotes: value.companyNotes || null,
    positionTitle: value.positionTitle,
    applicationDate: value.applicationDate || null,
    status: value.status,
    jobAdUrl: value.jobAdUrl || null,
    salaryRange: value.salaryRange || null,
    interviewDateTime: value.interviewDateTime || null,
    experienceNotes: value.experienceNotes || null,
    nextStep: value.nextStep || null
  }
}

function toDatetimeLocal(value) {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return ''
  const pad = (n) => String(n).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`
}

function scrollToId(id) {
  showCreateForm.value = true
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' })
}

function toggleCreateForm() {
  showCreateForm.value = !showCreateForm.value
}

function toggleJobDetails(id) {
  expandedJobId.value = expandedJobId.value === id ? null : id
}

onMounted(load)
</script>

<template>
  <section>
    <h1>Álláskövető</h1>
    <p class="muted page-intro">Egy helyen rögzítheted a céget és a hozzá tartozó jelentkezést. Nem kell külön céget létrehoznod előtte.</p>
    <ErrorMessage :message="error" />

    <article class="card compact-create-card" id="new-job">
      <div class="collapsible-header">
        <div>
          <h2>Új jelentkezés</h2>
          <p class="muted">Cég és jelentkezés rögzítése egy helyen. Alapból összecsukva marad, hogy ne foglaljon helyet.</p>
        </div>
        <button type="button" class="secondary" @click="toggleCreateForm">
          {{ showCreateForm ? 'Bezárás' : 'Új jelentkezés megnyitása' }}
        </button>
      </div>

      <form v-if="showCreateForm" class="form create-job-form" @submit.prevent="createJob">
        <fieldset class="form-section">
          <legend>Cég adatai</legend>
          <div class="grid-form">
            <label>Cégnév<input v-model="form.companyName" required maxlength="150" /><span v-if="!form.companyName" class="field-error">A cégnév kötelező.</span></label>
            <label>Weboldal<input v-model="form.website" placeholder="https://..." /></label>
            <label>Lokáció<input v-model="form.location" placeholder="Budapest / Remote" /></label>
            <label>Kapcsolattartó neve<input v-model="form.contactName" /></label>
            <label>Kapcsolattartó email<input v-model="form.contactEmail" type="email" /></label>
            <label>Kapcsolattartó telefon<input v-model="form.contactPhone" /></label>
            <label class="wide-field">Céghez kapcsolódó jegyzet<textarea v-model="form.companyNotes" rows="2" /></label>
          </div>
        </fieldset>

        <fieldset class="form-section">
          <legend>Jelentkezés adatai</legend>
          <div class="grid-form">
            <label>Pozíció<input v-model="form.positionTitle" required maxlength="150" /><span v-if="!form.positionTitle" class="field-error">A pozíció kötelező.</span></label>
            <label>Álláshirdetés URL<input v-model="form.jobAdUrl" placeholder="https://..." /></label>
            <label>Jelentkezés dátuma<input v-model="form.applicationDate" type="date" /></label>
            <label>Státusz<select v-model="form.status"><option v-for="option in jobStatusOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
            <label>Fizetés<input v-model="form.salaryRange" placeholder="pl. br. 700-900k" /></label>
            <label>Interjú időpont<input v-model="form.interviewDateTime" type="datetime-local" /></label>
            <label class="wide-field">Tapasztalat<textarea v-model="form.experienceNotes" rows="2" /></label>
            <label class="wide-field">Következő lépés<textarea v-model="form.nextStep" rows="2" /></label>
          </div>
        </fieldset>

        <button :disabled="saving">{{ saving ? 'Mentés...' : 'Jelentkezés mentése' }}</button>
      </form>
    </article>

    <div class="toolbar card compact-card">
      <label>Keresés<input v-model="search" placeholder="Cég, pozíció, város/helyszín, fizetés, jegyzet..." /></label>
      <label>Státusz<select v-model="statusFilter"><option value="">Összes</option><option v-for="option in jobStatusOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
      <label>Cég<select v-model="companyFilter"><option value="">Összes cég</option><option v-for="company in companies" :key="company.id" :value="company.id">{{ company.name }}</option></select></label>
      <div class="segmented"><button :class="{ active: viewMode === 'list' }" type="button" @click="viewMode = 'list'">Lista</button><button :class="{ active: viewMode === 'kanban' }" type="button" @click="viewMode = 'kanban'">Kanban</button></div>
    </div>

    <div class="toolbar card compact-card date-toolbar">
      <label>Időszak alapja<select v-model="dateField">
        <option value="applicationDate">Jelentkezés dátuma</option>
        <option value="interviewDateTime">Interjú időpontja</option>
      </select></label>
      <label>Ettől<input type="date" v-model="dateFrom" /></label>
      <label>Eddig<input type="date" v-model="dateTo" /></label>
      <div class="chip-row">
        <button
          v-for="preset in datePresets"
          :key="preset.key"
          type="button"
          :class="{ active: datePreset === preset.key }"
          @click="applyPreset(preset.key)"
        >{{ preset.label }}</button>
      </div>
    </div>

    <p class="muted filter-summary">
      <span><strong>{{ filteredJobs.length }}</strong> / {{ jobs.length }} jelentkezés látszik.</span>
      <span v-if="hiddenForMissingDate > 0">
        {{ hiddenForMissingDate }} jelentkezésnél nincs kitöltve a {{ dateFieldLabels[dateField] }} dátum, ezért időszakra szűrve nem jelenik meg.
      </span>
      <button v-if="anyFilterActive" type="button" class="secondary small-button" @click="clearFilters">Szűrők törlése</button>
    </p>

    <LoadingBox v-if="loading" />
    <EmptyState v-else-if="filteredJobs.length === 0" title="Nincs megjeleníthető jelentkezés" message="Hozd létre az első állásjelentkezésedet, vagy módosítsd a szűrőket." action-text="Új jelentkezés" @action="scrollToId('new-job')" />

    <div v-else-if="viewMode === 'kanban'" class="kanban-board">
      <section v-for="column in kanbanColumns" :key="column.key" class="kanban-column">
        <h2>{{ column.label }}</h2>
        <article v-for="job in jobsByColumn(column)" :key="job.id" class="kanban-card">
          <strong>{{ job.companyName }}</strong>
          <span>{{ job.positionTitle }}</span>
          <StatusBadge :value="job.status" />
          <div class="kanban-actions"><button class="secondary" type="button" @click="moveStatus(job, -1)">← Vissza</button><button type="button" @click="moveStatus(job, 1)">Tovább →</button></div>
        </article>
        <p v-if="jobsByColumn(column).length === 0" class="muted">Üres oszlop.</p>
      </section>
    </div>

    <div v-else class="job-compact-list">
      <article v-for="job in filteredJobs" :key="job.id" class="card job-compact-card">
        <form v-if="editingId === job.id" class="form edit-form" @submit.prevent="updateJob(job.id)">
          <fieldset class="form-section">
            <legend>Cég adatai</legend>
            <div class="grid-form">
              <label>Cégnév<input v-model="editForm.companyName" required /></label>
              <label>Weboldal<input v-model="editForm.website" /></label>
              <label>Lokáció<input v-model="editForm.location" /></label>
              <label>Kapcsolattartó neve<input v-model="editForm.contactName" /></label>
              <label>Kapcsolattartó email<input v-model="editForm.contactEmail" type="email" /></label>
              <label>Kapcsolattartó telefon<input v-model="editForm.contactPhone" /></label>
              <label class="wide-field">Cégjegyzet<textarea v-model="editForm.companyNotes" rows="2" /></label>
            </div>
          </fieldset>
          <fieldset class="form-section">
            <legend>Jelentkezés adatai</legend>
            <div class="grid-form">
              <label>Pozíció<input v-model="editForm.positionTitle" required /></label>
              <label>Jelentkezés dátuma<input v-model="editForm.applicationDate" type="date" /></label>
              <label>Státusz<select v-model="editForm.status"><option v-for="option in jobStatusOptions" :key="option.value" :value="option.value">{{ option.label }}</option></select></label>
              <label>Álláshirdetés URL<input v-model="editForm.jobAdUrl" /></label>
              <label>Fizetés<input v-model="editForm.salaryRange" /></label>
              <label>Interjú időpont<input v-model="editForm.interviewDateTime" type="datetime-local" /></label>
              <label class="wide-field">Tapasztalat<textarea v-model="editForm.experienceNotes" rows="2" /></label>
              <label class="wide-field">Következő lépés<textarea v-model="editForm.nextStep" rows="2" /></label>
            </div>
          </fieldset>
          <div class="actions"><button :disabled="saving">{{ saving ? 'Mentés...' : 'Mentés' }}</button><button type="button" class="secondary" @click="cancelEdit">Mégsem</button></div>
        </form>

        <template v-else>
          <div class="job-compact-row">
            <button type="button" class="job-expand-button" @click="toggleJobDetails(job.id)" :aria-expanded="expandedJobId === job.id">
              <span class="job-chevron">{{ expandedJobId === job.id ? '▾' : '▸' }}</span>
              <StatusBadge :value="job.status" />
              <span class="job-row-company">{{ job.companyName }}</span>
              <span class="job-row-position">{{ job.positionTitle }}</span>
              <span class="job-row-location">{{ job.location || 'Nincs helyszín' }}</span>
              <span v-if="job.applicationDate" class="job-row-date">{{ job.applicationDate }}</span>
            </button>

            <select class="job-row-status" :value="job.status" @change="updateStatus(job, $event.target.value)">
              <option v-for="option in jobStatusOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
            </select>
          </div>

          <div v-if="expandedJobId === job.id" class="job-expanded-panel">
            <div class="job-expanded-grid">
              <div class="job-expanded-details">
                <p><strong>Cég:</strong> {{ job.companyName }}</p>
                <p><strong>Pozíció:</strong> {{ job.positionTitle }}</p>
                <p><strong>Helyszín:</strong> {{ job.location || 'Nincs helyszín' }}</p>
                <p v-if="job.applicationDate"><strong>Jelentkezés dátuma:</strong> {{ job.applicationDate }}</p>
                <p v-if="job.salaryRange"><strong>Fizetés:</strong> {{ job.salaryRange }}</p>
                <p v-if="job.contactName || job.contactEmail || job.contactPhone">
                  <strong>Kapcsolattartó:</strong>
                  {{ job.contactName || '—' }}
                  <template v-if="job.contactEmail"> {{ job.contactEmail }}</template>
                  <template v-if="job.contactPhone"> · {{ job.contactPhone }}</template>
                </p>
                <p v-if="job.website"><a :href="job.website" target="_blank" rel="noreferrer">Weboldal megnyitása</a></p>
                <p v-if="job.jobAdUrl"><a :href="job.jobAdUrl" target="_blank" rel="noreferrer">Álláshirdetés megnyitása</a></p>
              </div>

              <div v-if="job.experienceNotes || job.nextStep" class="job-note-grid">
                <div v-if="job.experienceNotes" class="job-note-card">
                  <span class="job-note-label">Tapasztalat</span>
                  <p>{{ job.experienceNotes }}</p>
                </div>
                <div v-if="job.nextStep" class="job-note-card">
                  <span class="job-note-label">Következő lépés</span>
                  <p>{{ job.nextStep }}</p>
                </div>
              </div>
            </div>

            <div class="job-expanded-actions">
              <button class="secondary" @click="startEdit(job)">Szerkesztés</button>
              <button class="danger-soft" @click="deleteTarget = job">Törlés</button>
            </div>
          </div>
        </template>
      </article>
    </div>

    <ConfirmModal :show="Boolean(deleteTarget)" title="Jelentkezés törlése" :message="`Biztosan törlöd ezt a jelentkezést: ${deleteTarget?.companyName || ''} - ${deleteTarget?.positionTitle || ''}?`" @cancel="deleteTarget = null" @confirm="confirmDelete" />
  </section>
</template>
