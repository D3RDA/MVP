<script setup>
import { RouterLink } from 'vue-router'

const features = [
  {
    title: 'Álláskövető',
    text: 'Minden jelentkezés egy helyen: pozíció, cég, helyszín, fizetési sáv, kapcsolattartó, jegyzet. A státusz végigvezethető a jelentkezéstől az ajánlatig — listában vagy Kanban-táblán, ahogy jobban látod.'
  },
  {
    title: 'Állásimport',
    text: 'A hirdetés adatait nem kell kézzel átgépelni. Az import után a jelentkezés pár másodperc alatt megvan, és ugyanúgy követhető, mint a többi.'
  },
  {
    title: 'Cégek',
    text: 'A cégek külön nyilvántartásban élnek, kapcsolattartóval és jegyzettel. Ha ugyanoda máskor is jelentkezel, már tudod, kivel beszéltél és mi hangzott el.'
  },
  {
    title: 'Áttekintés',
    text: 'A dashboard megmutatja, hány jelentkezés van nyitva, mennyi vár válaszra, és hogyan oszlanak meg a státuszok. Időszakra szűrve azt is, mennyit mozdultál egy adott hónapban.'
  }
]

const steps = [
  { n: '1', title: 'Rögzíted a jelentkezést', text: 'Cég és pozíció egyetlen űrlapon. Nem kell előbb céget létrehoznod — ha új, magától létrejön.' },
  { n: '2', title: 'Vezeted a státuszt', text: 'Jelentkezve → válaszra vár → interjú → ajánlat. Egy kattintás lépteti tovább, a tábla pedig mutatja, hol torlódik.' },
  { n: '3', title: 'Látod, hol tartasz', text: 'Időszakra szűrve megnézed, mennyit jelentkeztél, mikor voltak interjúid, és melyik jelentkezésnél kell utánamenni.' }
]
</script>

<template>
  <div class="landing">
    <header class="landing-hero">
      <div class="landing-brand">
        <img src="/logo.svg" alt="MVP for You logó" />
        <span>MVP for You</span>
      </div>
      <h1>Ne a fejedben tartsd, hová jelentkeztél.</h1>
      <p class="landing-lead">
        Álláskövető webalkalmazás: egy helyen látod, hova adtad be a jelentkezést, melyik hol tart,
        kivel beszéltél, és mikor kell utánamenni.
      </p>
      <div class="landing-actions">
        <RouterLink class="landing-cta" to="/register">Kipróbálom — regisztráció</RouterLink>
        <RouterLink class="landing-ghost" to="/login">Bejelentkezés</RouterLink>
      </div>
      <p class="muted landing-note">
        Ez nem bemutató verzió. A saját álláskeresésemet követem benne, valódi jelentkezésekkel.
      </p>
    </header>

    <section class="landing-section">
      <h2>Mi a gond a jelenlegi megoldásoddal</h2>
      <div class="two-columns">
        <article class="card">
          <h2>Szétszórva</h2>
          <p class="muted">
            Néhány jelentkezés az e-mailben, néhány egy táblázatban, a többi sehol.
            Két hét múlva már nem tudod, melyik cégtől vártál választ, és melyiket felejtetted el.
          </p>
        </article>
        <article class="card">
          <h2>Együtt</h2>
          <p class="muted">
            Egy lista, egy státusz, egy helyen. Látod, mennyi van nyitva, mi vár válaszra,
            és melyik jelentkezésed áll hetek óta mozdulatlanul.
          </p>
        </article>
      </div>
    </section>

    <section class="landing-section">
      <h2>Így néz ki</h2>
      <div class="preview-window" aria-hidden="true">
        <div class="preview-bar">
          <span class="preview-dot"></span><span class="preview-dot"></span><span class="preview-dot"></span>
        </div>
        <div class="preview-body">
          <div class="preview-row">
            <strong>Acme Kft. — Junior SOC analyst</strong>
            <span class="badge badge-waiting_response">Válaszra vár</span>
          </div>
          <div class="preview-row">
            <strong>Globex Zrt. — IT support</strong>
            <span class="badge badge-interview_scheduled">Interjú egyeztetve</span>
          </div>
          <div class="preview-row">
            <strong>Initech — Rendszergazda</strong>
            <span class="badge badge-offer_received">Ajánlatot kaptam</span>
          </div>
          <div class="preview-row">
            <strong>Umbrella — Sysadmin</strong>
            <span class="badge badge-rejected">Elutasítva</span>
          </div>
        </div>
      </div>
      <p class="muted">Illusztráció — a valódi adataidat csak te látod, bejelentkezés után.</p>
    </section>

    <section class="landing-section">
      <h2>Mit tud</h2>
      <div class="grid landing-features">
        <article v-for="feature in features" :key="feature.title" class="card">
          <h2>{{ feature.title }}</h2>
          <p class="muted">{{ feature.text }}</p>
        </article>
      </div>
    </section>

    <section class="landing-section">
      <h2>Hogyan működik</h2>
      <div class="grid landing-steps">
        <article v-for="step in steps" :key="step.n" class="card step">
          <span class="step-number">{{ step.n }}</span>
          <h2>{{ step.title }}</h2>
          <p class="muted">{{ step.text }}</p>
        </article>
      </div>
    </section>

    <section class="landing-section">
      <h2>Amit szándékosan nem tud</h2>
      <article class="card">
        <p class="muted">
          Az alkalmazás korábban projekt-, feladat-, naptár- és jegyzetkezelést is tartalmazott.
          Két hónap valódi használat után ezekből egyet sem nyitottam meg, ezért kikerültek.
        </p>
        <p class="muted">
          Egy alkalmazás, ami egy dolgot jól csinál, többet ér, mint amelyik ötöt félig.
          Ez álláskövető — és csak az akar lenni.
        </p>
      </article>
    </section>

    <section class="landing-section">
      <h2>Mi van mögötte</h2>
      <article class="card">
        <p class="muted">
          Vue 3 + Vite frontend, ASP.NET Core Web API backend, MySQL adatbázis, JWT alapú
          hitelesítés. A sötét mód a rendszered beállítását követi, nincs kapcsolgatni való.
        </p>
        <p class="muted">
          A forráskód nyilvános:
          <a href="https://github.com/D3RDA/MVP" target="_blank" rel="noopener noreferrer">github.com/D3RDA/MVP</a>
        </p>
      </article>
    </section>

    <section class="landing-section landing-closing">
      <h2>Kezdjük el</h2>
      <p class="muted">A regisztráció pár másodperc, és az első jelentkezésedet rögtön rögzítheted.</p>
      <div class="landing-actions">
        <RouterLink class="landing-cta" to="/register">Regisztráció</RouterLink>
        <RouterLink class="landing-ghost" to="/login">Már van fiókom</RouterLink>
      </div>
    </section>

    <footer class="landing-footer">
      <span>MVP for You</span>
      <span>
        <RouterLink to="/felhasznalasi-feltetelek">Felhasználási feltételek</RouterLink>
        ·
        <RouterLink to="/adatkezelesi-tajekoztato">Adatkezelési tájékoztató</RouterLink>
      </span>
    </footer>
  </div>
</template>
