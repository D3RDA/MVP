<script setup>
import { useRouter } from 'vue-router'

const router = useRouter()

function goBack() {
  if (window.history.length > 1) {
    router.back()
  } else {
    router.push('/register')
  }
}
</script>

<template>
  <section class="legal-page card">
    <button type="button" class="secondary back-button" @click="goBack">← Vissza</button>
    <h1>Felhasználási feltételek</h1>
    <p class="muted">Verzió: 1.0</p>

    <h2>1. A szolgáltatás célja</h2>
    <p>Az alkalmazás személyes projekttervezésre, naptári események kezelésére, cégek és állásjelentkezések nyilvántartására, valamint jegyzetelésre szolgál.</p>

    <h2>2. Felhasználói fiók</h2>
    <p>A felhasználó saját fiókot hozhat létre név, email cím és jelszó megadásával. A felhasználó felelős a belépési adatai biztonságos kezeléséért.</p>

    <h2>3. Megengedett használat</h2>
    <p>Az alkalmazás kizárólag jogszerű, személyes vagy szakmai célú használatra készült. Tilos a rendszer működésének megzavarása, jogosulatlan hozzáférési kísérlet, automatizált támadás vagy más felhasználó adatainak megszerzése.</p>

    <h2>4. Felelősség</h2>
    <p>Az alkalmazás MVP állapotú fejlesztés, ezért hibák előfordulhatnak. Fontos adatokról a felhasználónak érdemes saját biztonsági másolatot is készítenie.</p>

    <h2>5. Módosítás</h2>
    <p>A feltételek később módosulhatnak. Jelentős módosítás esetén a felhasználókat az alkalmazáson belül vagy emailben lehet tájékoztatni.</p>

    <p class="muted">Ez egy fejlesztői sablon, nem minősül jogi tanácsadásnak. Éles publikálás előtt érdemes jogásszal ellenőriztetni.</p>
  </section>
</template>
