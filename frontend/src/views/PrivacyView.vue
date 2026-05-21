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
    <h1>Adatkezelési tájékoztató</h1>
    <p class="muted">Verzió: 1.0</p>

    <h2>1. Kezelt adatok</h2>
    <p>Az alkalmazás a regisztrációhoz nevet, email címet és jelszó hash-t tárol. A használat során projektek, feladatok, naptári események, cégek, állásjelentkezések és jegyzetek adatai kerülhetnek mentésre.</p>

    <h2>2. Az adatkezelés célja</h2>
    <p>Az adatkezelés célja a felhasználói fiók működtetése, a felhasználó saját adatainak elkülönítése, valamint a projekt- és álláskövető funkciók biztosítása.</p>

    <h2>3. Adatbiztonság</h2>
    <p>A jelszavak nem nyílt szövegként, hanem hash-elve kerülnek tárolásra. A védett API végpontok JWT alapú autentikációval érhetők el.</p>

    <h2>4. Tárolási idő</h2>
    <p>Az adatok a felhasználói fiók fennállásáig kerülnek tárolásra. Későbbi fejlesztési lépésként érdemes fióktörlési és adatexport funkciót is készíteni.</p>

    <h2>5. Felhasználói jogok</h2>
    <p>A felhasználónak joga lehet hozzáférést, helyesbítést, törlést vagy exportot kérni a saját adataival kapcsolatban. Ezeket éles rendszerben külön elérhetőségen vagy alkalmazáson belüli funkcióval kell biztosítani.</p>

    <p class="muted">Ez egy fejlesztői sablon, nem minősül jogi tanácsadásnak. Éles publikálás előtt érdemes jogásszal ellenőriztetni.</p>
  </section>
</template>
