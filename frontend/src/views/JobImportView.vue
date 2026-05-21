<script setup>
import { computed, ref } from 'vue'
import { onBeforeRouteLeave } from 'vue-router'
import api from '../services/api'
import ErrorMessage from '../components/ErrorMessage.vue'
import { useToast } from '../composables/useToast'

const { show: toast } = useToast()
const rawText = ref('')
const rows = ref([])
const error = ref('')
const saving = ref(false)
const importResult = ref(null)
const cancelRequested = ref(false)

const validRows = computed(() => rows.value.filter((row) => row.companyName.trim() && row.positionTitle.trim() && row.location.trim()))
const invalidRows = computed(() => rows.value.length - validRows.value.length)

const exampleText = `Cégnév;Pozíció;Helyszín\nExample Kft.;Junior Software Tester;Budapest\nRemoteTech Zrt.;Frontend fejlesztő;Remote\nCyberLab Kft.;Junior Security Analyst;Debrecen`

function parseText() {
  error.value = ''
  importResult.value = null
  const lines = rawText.value
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean)

  if (lines.length === 0) {
    rows.value = []
    error.value = 'Nincs feldolgozható sor. Illessz be szöveget, vagy tölts fel TXT/CSV fájlt.'
    return
  }

  rows.value = lines
    .map((line) => parseLine(line))
    .filter((row, index) => !isHeaderRow(row, index))
}

function parseLine(line) {
  const delimiter = detectDelimiter(line)
  const values = delimiter ? parseDelimitedLine(line, delimiter) : line.split(/\s{2,}/)

  return {
    companyName: (values[0] || '').trim(),
    positionTitle: (values[1] || '').trim(),
    location: (values[2] || '').trim()
  }
}

function detectDelimiter(line) {
  const delimiters = [';', '\t', '|', ',']
  return delimiters.find((delimiter) => line.includes(delimiter)) || null
}

function parseDelimitedLine(line, delimiter) {
  const result = []
  let current = ''
  let insideQuotes = false

  for (let i = 0; i < line.length; i += 1) {
    const char = line[i]
    const next = line[i + 1]

    if (char === '"' && next === '"') {
      current += '"'
      i += 1
    } else if (char === '"') {
      insideQuotes = !insideQuotes
    } else if (char === delimiter && !insideQuotes) {
      result.push(current)
      current = ''
    } else {
      current += char
    }
  }

  result.push(current)
  return result
}

function isHeaderRow(row, index) {
  if (index !== 0) return false
  const normalized = [row.companyName, row.positionTitle, row.location].join(' ').toLowerCase()
  return normalized.includes('cégnév') || normalized.includes('cegnev') || normalized.includes('company')
}

async function handleFileChange(event) {
  const file = event.target.files?.[0]
  if (!file) return

  if (!file.name.toLowerCase().match(/\.(txt|csv)$/)) {
    error.value = 'Csak .txt vagy .csv fájl tölthető be.'
    event.target.value = ''
    return
  }

  rawText.value = await file.text()
  parseText()
  event.target.value = ''
}

function useExample() {
  rawText.value = exampleText
  parseText()
}

function removeRow(index) {
  rows.value.splice(index, 1)
}

function todayIsoDate() {
  return new Date().toISOString().slice(0, 10)
}

function cancelImport() {
  if (!saving.value) return
  cancelRequested.value = true
}

async function importRows() {
  error.value = ''
  importResult.value = null
  cancelRequested.value = false

  if (validRows.value.length === 0) {
    error.value = 'Nincs importálható sor. A cégnév, pozíció és helyszín kötelező.'
    return
  }

  saving.value = true
  let successCount = 0
  const failed = []

  for (const [index, row] of validRows.value.entries()) {
    if (cancelRequested.value) {
      break
    }

    try {
      await api.post('/jobapplications/with-company', {
        companyName: row.companyName.trim(),
        positionTitle: row.positionTitle.trim(),
        location: row.location.trim(),
        status: 'applied',
        applicationDate: todayIsoDate(),
        website: null,
        contactName: null,
        contactEmail: null,
        contactPhone: null,
        companyNotes: null,
        jobAdUrl: null,
        salaryRange: null,
        interviewDateTime: null,
        experienceNotes: null,
        nextStep: null
      })
      successCount += 1
    } catch (err) {
      failed.push({ row: index + 1, message: err.response?.data || 'Ismeretlen mentési hiba.' })
    }
  }

  saving.value = false
  importResult.value = { successCount, failed, cancelled: cancelRequested.value }

  if (cancelRequested.value) {
    toast('Importálás megszakítva.')
  } else if (successCount > 0) {
    toast(`${successCount} jelentkezés importálva ✓`)
  }
}

onBeforeRouteLeave((to, from, next) => {
  if (!saving.value) {
    next()
    return
  }

  const shouldLeave = window.confirm('Import folyamatban van. Biztosan elhagyod az oldalt?')
  if (shouldLeave) {
    cancelRequested.value = true
    next()
  } else {
    next(false)
  }
})
</script>

<template>
  <section>
    <h1>Tömeges állásimport</h1>
    <p class="muted page-intro">TXT vagy CSV fájlból gyorsan be tudod hozni a jelentkezéseket. A várt sorrend: cégnév; pozíció; helyszín.</p>
    <ErrorMessage :message="error" />

    <article class="card">
      <h2>Fájl betöltése vagy lista beillesztése</h2>
      <div class="grid-form import-controls">
        <label>TXT / CSV fájl
          <input type="file" accept=".txt,.csv,text/plain,text/csv" @change="handleFileChange" />
        </label>
        <div class="actions import-actions">
          <button type="button" class="secondary" @click="useExample">Példa betöltése</button>
          <button type="button" @click="parseText">Lista feldolgozása</button>
        </div>
      </div>

      <label class="wide-field">Beillesztett tartalom
        <textarea v-model="rawText" class="large-textarea mono-textarea" rows="10" placeholder="Cégnév;Pozíció;Helyszín&#10;Example Kft.;Junior Software Tester;Budapest" />
      </label>

      <div class="hint-box">
        <strong>Támogatott formátumok:</strong>
        <p>Ajánlott: pontosvesszővel elválasztva. Példa: <code>Example Kft.;Junior Backend Developer;Budapest</code></p>
        <p>Elfogadott elválasztók: pontosvessző, tab, vessző vagy | karakter.</p>
      </div>
    </article>

    <article v-if="rows.length > 0" class="card import-preview-card">
      <div class="inline space-between">
        <div>
          <h2>Import előnézet</h2>
          <p class="muted">Importálható sorok: {{ validRows.length }}<span v-if="invalidRows">, hiányos sorok: {{ invalidRows }}</span></p>
        </div>
        <button v-if="!saving" :disabled="validRows.length === 0" @click="importRows">Importálás az álláskövetőbe</button>
          <button v-else type="button" class="danger" @click="cancelImport">Import leállítása</button>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Cégnév</th>
              <th>Pozíció</th>
              <th>Helyszín</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(row, index) in rows" :key="index" :class="{ 'invalid-row': !row.companyName || !row.positionTitle || !row.location }">
              <td>{{ index + 1 }}</td>
              <td><input v-model="row.companyName" /></td>
              <td><input v-model="row.positionTitle" /></td>
              <td><input v-model="row.location" /></td>
              <td><button class="danger small-button" type="button" @click="removeRow(index)">Törlés</button></td>
            </tr>
          </tbody>
        </table>
      </div>
    </article>

    <article v-if="importResult" class="card">
      <h2>Import eredmény</h2>
      <p v-if="importResult.cancelled" class="warning-text"><strong>Az importálás megszakítva.</strong></p>
      <p><strong>Sikeresen importálva:</strong> {{ importResult.successCount }}</p>
      <div v-if="importResult.failed.length">
        <p><strong>Sikertelen sorok:</strong></p>
        <ul>
          <li v-for="failed in importResult.failed" :key="failed.row">{{ failed.row }}. sor: {{ failed.message }}</li>
        </ul>
      </div>
      <p v-else class="muted">Minden sor sikeresen bekerült az álláskövetőbe.</p>
    </article>
  </section>
</template>
