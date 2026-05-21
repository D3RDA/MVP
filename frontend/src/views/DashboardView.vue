<script setup>
import { computed, onMounted, ref } from 'vue'
import { Bar, Doughnut } from 'vue-chartjs'
import { Chart as ChartJS, ArcElement, BarElement, CategoryScale, Legend, LinearScale, Tooltip } from 'chart.js'
import api from '../services/api'
import LoadingBox from '../components/LoadingBox.vue'
import ErrorMessage from '../components/ErrorMessage.vue'
import StatusBadge from '../components/StatusBadge.vue'
import { jobStatusLabels, projectStatusLabels } from '../utils/labels'

ChartJS.register(ArcElement, BarElement, CategoryScale, Legend, LinearScale, Tooltip)

const data = ref(null)
const projects = ref([])
const jobs = ref([])
const loading = ref(true)
const error = ref('')

const chartOptions = { responsive: true, maintainAspectRatio: false }

const doughnutOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { position: 'bottom' },
    tooltip: { enabled: true }
  }
}

const projectChartData = computed(() => {
  const keys = ['planned', 'in_progress', 'completed', 'paused']
  return {
    labels: keys.map((key) => projectStatusLabels[key]),
    datasets: [{
      data: keys.map((key) => projects.value.filter((project) => project.status === key).length),
      backgroundColor: ['#94a3b8', '#2563eb', '#16a34a', '#f59e0b'],
      borderWidth: 0
    }]
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
    const [dashboardResponse, projectsResponse, jobsResponse] = await Promise.all([
      api.get('/dashboard'), api.get('/projects'), api.get('/jobapplications')
    ])
    data.value = dashboardResponse.data
    projects.value = projectsResponse.data
    jobs.value = jobsResponse.data
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
        <article class="card"><span>Aktív projektek</span><strong>{{ data.activeProjects }}</strong></article>
        <article class="card"><span>Befejezett projektek</span><strong>{{ data.completedProjects }}</strong></article>
        <article class="card"><span>Nyitott jelentkezések</span><strong>{{ data.openJobApplications }}</strong></article>
        <article class="card"><span>Válaszra vár</span><strong>{{ data.waitingJobApplications }}</strong></article>
        <article class="card"><span>Következő 7 nap eseményei</span><strong>{{ data.upcomingCalendarEvents }}</strong></article>
      </div>

      <div class="two-columns charts-grid">
        <article class="card chart-card">
          <h2>Projekt státusz megoszlás</h2>
          <p v-if="projects.length === 0" class="muted">Még nincsenek projektek.</p>
          <div v-else class="chart-box"><Doughnut :data="projectChartData" :options="doughnutOptions" /></div>
        </article>
        <article class="card chart-card">
          <h2>Állásjelentkezések státusz szerint</h2>
          <div class="chart-box"><Bar :data="jobChartData" :options="chartOptions" /></div>
        </article>
      </div>

      <div class="two-columns">
        <article class="card">
          <h2>Közelgő események</h2>
          <ul class="list">
            <li v-for="event in data.upcomingEvents" :key="event.id">
              <strong>{{ event.title }}</strong><span>{{ new Date(event.startDateTime).toLocaleString('hu-HU') }}</span>
            </li>
            <li v-if="data.upcomingEvents.length === 0" class="muted">Nincs közelgő esemény.</li>
          </ul>
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
