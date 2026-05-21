<script setup>
import { ref } from "vue";
import { useRouter, RouterLink } from "vue-router";
import api from "../services/api";
import { setSession } from "../services/auth";
import ErrorMessage from "../components/ErrorMessage.vue";
import { getErrorMessage } from "../services/errorMessage";

const router = useRouter();
const name = ref("");
const email = ref("");
const password = ref("");
const acceptTerms = ref(false);
const acceptPrivacy = ref(false);
const error = ref("");
const loading = ref(false);

async function submit() {
  error.value = "";

  if (!acceptTerms.value || !acceptPrivacy.value) {
    error.value =
      "A regisztrációhoz el kell fogadnod a felhasználási feltételeket és az adatkezelési tájékoztatót.";
    return;
  }

  loading.value = true;
  try {
    const { data } = await api.post("/auth/register", {
      name: name.value,
      email: email.value,
      password: password.value,
      acceptTerms: acceptTerms.value,
      acceptPrivacy: acceptPrivacy.value,
    });
    setSession(data);
    router.push("/dashboard");
  } catch (err) {
    error.value = getErrorMessage(err, "Nem sikerült regisztrálni.");
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <section class="auth-card">
    <h1>Regisztráció</h1>
    <p>Hozz létre saját fiókot az adataid elkülönítéséhez.</p>
    <ErrorMessage :message="error" />
    <form @submit.prevent="submit" class="form">
      <label>Név<input v-model="name" required maxlength="100" /></label>
      <label>Email<input v-model="email" type="email" required /></label>
      <label
        >Jelszó<input v-model="password" type="password" required minlength="6"
      /></label>

      <label class="checkbox-label">
        <input v-model="acceptTerms" type="checkbox" required />
        <span>
          Elfogadom a
          <RouterLink to="/felhasznalasi-feltetelek">
            Felhasználási feltételeket </RouterLink
          >.
        </span>
      </label>

      <label class="checkbox-label">
        <input v-model="acceptPrivacy" type="checkbox" required />
        <span>
          Elfogadom az
          <RouterLink to="/adatkezelesi-tajekoztato">
            Adatkezelési tájékoztatót </RouterLink
          >.
        </span>
      </label>

      <button :disabled="loading">
        {{ loading ? "Mentés..." : "Regisztráció" }}
      </button>
    </form>
    <p class="muted">
      Van fiókod? <RouterLink to="/login">Bejelentkezés</RouterLink>
    </p>
  </section>
</template>
