import { createRouter, createWebHistory } from 'vue-router'
import { isLoggedIn } from '../services/auth'
import LoginView from '../views/LoginView.vue'
import RegisterView from '../views/RegisterView.vue'
import DashboardView from '../views/DashboardView.vue'
import ProjectsView from '../views/ProjectsView.vue'
import ProjectDetailsView from '../views/ProjectDetailsView.vue'
import CompaniesView from '../views/CompaniesView.vue'
import JobsView from '../views/JobsView.vue'
import JobImportView from '../views/JobImportView.vue'
import CalendarView from '../views/CalendarView.vue'
import NotesView from '../views/NotesView.vue'
import TermsView from '../views/TermsView.vue'
import PrivacyView from '../views/PrivacyView.vue'

const routes = [
  { path: '/', redirect: '/dashboard' },
  { path: '/login', component: LoginView, meta: { guest: true } },
  { path: '/register', component: RegisterView, meta: { guest: true } },
  { path: '/felhasznalasi-feltetelek', component: TermsView },
  { path: '/adatkezelesi-tajekoztato', component: PrivacyView },
  { path: '/dashboard', component: DashboardView, meta: { requiresAuth: true } },
  { path: '/projects', component: ProjectsView, meta: { requiresAuth: true } },
  { path: '/projects/:id', component: ProjectDetailsView, meta: { requiresAuth: true } },
  { path: '/companies', component: CompaniesView, meta: { requiresAuth: true } },
  { path: '/jobs', component: JobsView, meta: { requiresAuth: true } },
  { path: '/job-import', component: JobImportView, meta: { requiresAuth: true } },
  { path: '/calendar', component: CalendarView, meta: { requiresAuth: true } },
  { path: '/notes', component: NotesView, meta: { requiresAuth: true } }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to) => {
  const loggedIn = isLoggedIn()
  if (to.meta.requiresAuth && !loggedIn) return '/login'
  if (to.meta.guest && loggedIn) return '/dashboard'
})

export default router
