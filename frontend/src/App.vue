<script setup>
import { computed, ref, watch } from "vue";
import { useRouter, useRoute } from "vue-router";
import { clearSession, getUser, isLoggedIn } from "./services/auth";
import Toast from "./components/Toast.vue";

const router = useRouter();
const route = useRoute();
const menuOpen = ref(false);
const loggedIn = computed(() => {
  route.fullPath;
  return isLoggedIn();
});
const user = computed(() => {
  route.fullPath;
  return getUser();
});

function logout() {
  clearSession();
  menuOpen.value = false;
  router.push("/login");
}

watch(
  () => route.fullPath,
  () => {
    menuOpen.value = false;
  },
);
</script>

<template>
  <div class="app-shell">
    <header v-if="loggedIn" class="mobile-header">
      <div class="brand">
        <img class="brand-logo" src="/logo.svg" alt="MVP for You logó" />
        <span>MVP for You</span>
      </div>
      <button
        class="menu-button"
        type="button"
        @click="menuOpen = !menuOpen"
        aria-label="Menü megnyitása"
      >
        ☰
      </button>
    </header>

    <aside v-if="loggedIn" class="sidebar" :class="{ open: menuOpen }">
      <div class="brand desktop-brand">
        <img class="brand-logo" src="/logo.svg" alt="MVP for You logó" />
        <span>MVP for You</span>
      </div>
      <div class="user-box">
        <strong>{{ user?.name || "Felhasználó" }}</strong>
        <span>{{ user?.email }}</span>
      </div>
      <nav>
        <RouterLink to="/dashboard">Dashboard</RouterLink>
        <RouterLink to="/projects">Projektek</RouterLink>
        <RouterLink to="/jobs">Álláskövető</RouterLink>
        <RouterLink to="/job-import">Állásimport</RouterLink>
        <RouterLink to="/calendar">Naptár</RouterLink>
        <RouterLink to="/notes">Jegyzetek</RouterLink>
      </nav>
      <button class="logout" @click="logout">Kijelentkezés</button>
    </aside>

    <main :class="loggedIn ? 'content' : 'auth-content'">
      <RouterView :key="route.fullPath" />
    </main>
    <Toast />
  </div>
</template>
