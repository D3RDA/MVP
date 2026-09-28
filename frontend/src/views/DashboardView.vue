<script setup>
import { computed, onMounted, ref } from 'vue'
import { Bar } from 'vue-chartjs'
import { Chart as ChartJS, BarElement, CategoryScale, Legend, LinearScale, Tooltip } from 'chart.js'
import api from '../services/api'
import LoadingBox from '../components/LoadingBox.vue'
import ErrorMessage from '../components/ErrorMessage.vue'
import StatusBadge from '../components/StatusBadge.vue'
import { jobStatusLabels } from '../utils/labels'

ChartJS.register(BarElement, CategoryScale, Legend, LinearScale, Tooltip)

const data = ref(null)
const jobs = ref([])
const companies = ref([])
const loading = ref(true)
const error = ref('')

// A Chart.js sajat alapszinei nem kovetik a temat, ezert a tengelyfeliratot
// es a racsot a rendszer szinsemajabol allitjuk be, es valtaskor frissitjuk.
const darkQuery = window.matchMedia('(prefers-color-scheme: dark)')
const isDark = ref(darkQuery.matches)
darkQuery.addEventListener('change', (event) => { isDark.value = event.matches })

const chartOptions = computed(() => {
  const label = isDark.value ? '#b4c0d6' : '#475569'
  const grid = isDark.value ? 'rgba(255, 255, 255, .08)' : 'rgba(15, 23, 42, .08)'
  return {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { labels: { color: label } } },
    scales: {
      x: { ticks: { color: label }, grid: { color: grid } },
      y: { ticks: { color: label }, grid: { color: grid } }
    }
  }
})

const jobChartData = computed(() => {
  const keys = Object.keys(jobStatusLabels)
  return {
    labels: keys.map((key) => jobStatusLabels[key]),
    datasets: [{ label: 'Jelentkezések', data: keys.map((key) => jobs.value.filter((job) => job.status === key).length) }]
  }
})

async function load() {
  try {
    const [dashboardResponse, jobsResponse, companiesResponse] = await Promise.all([
      api.get('/dashboard'), api.get('/jobapplications'), api.get('/companies')
    ])
    data.value = dashboardResponse.data
    jobs.value = jobsResponse.data
    companies.value = companiesResponse.data
  } catch {
    error.value = 'Nem sikerült betölteni a dashboard adatokat.'
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <section>
    <h1>Dashboard</h1>
    <ErrorMessage :message="error" />
    <LoadingBox v-if="loading" />
    <template v-else-if="data">
      <div class="grid stats-grid">
        <article class="card"><span>Összes jelentkezés</span><strong>{{ jobs.length }}</strong></article>
        <article class="card"><span>Nyitott jelentkezések</span><strong>{{ data.openJobApplications }}</strong></article>
        <article class="card"><span>Válaszra vár</span><strong>{{ data.waitingJobApplications }}</strong></article>
        <article class="card"><span>Nyilvántartott cégek</span><strong>{{ companies.length }}</strong></article>
      </div>

      <div class="two-columns charts-grid">
        <article class="card chart-card">
          <h2>Állásjelentkezések státusz szerint</h2>
          <div class="chart-box"><Bar :data="jobChartData" :options="chartOptions" /></div>
        </article>
        <article class="card">
          <h2>Legutóbbi állásjelentkezések</h2>
          <ul class="list">
            <li v-for="job in data.recentJobApplications" :key="job.id">
              <strong>{{ job.companyName }} — {{ job.positionTitle }}</strong><StatusBadge :value="job.status" />
            </li>
            <li v-if="data.recentJobApplications.length === 0" class="muted">Még nincs jelentkezés.</li>
          </ul>
        </article>
      </div>
    </template>
  </section>
</template>
