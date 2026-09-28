# MVP for You — álláskövető

> **Webalkalmazás álláspályázatok nyomon követésére: hova jelentkeztél, hol tart, mikor kell utánamenni.**

Élő példány: **https://mvpforyou.tryasp.net/**

Ez nem demó. A saját álláskeresésemet követem benne, valódi jelentkezésekkel — a funkciók azért néznek ki így, mert használat közben derült ki, mire van szükség és mire nincs.

## Mit tud

| | |
|---|---|
| **Álláskövető** | jelentkezés rögzítése, státusz végigvezetése (jelentkezve → válaszra vár → interjú → ajánlat / elutasítás), jegyzetek, határidők |
| **Állásimport** | hirdetés adatainak beemelése kézi gépelés helyett |
| **Cégek** | céglista kapcsolattartóval, a jelentkezésekhez kötve |
| **Dashboard** | összes / nyitott / válaszra váró jelentkezés, státusz szerinti megoszlás, legutóbbi jelentkezések |

**Sötét mód**: a rendszer beállítását követi (`prefers-color-scheme`), nincs kapcsolgatni való.

## Amit szándékosan nem tud

Az app korábban planner volt: projekt-, feladat-, naptár- és jegyzetkezeléssel. Két hónap valódi használat után ezekből **egyet sem nyitottam meg** — 2026 szeptemberében kikerültek.

Egy app, ami egy dolgot jól csinál, többet ér, mint amelyik ötöt félig. A backend controllerek egyelőre megvannak; a frontend már nem hívja őket.

## Tech stack

| Réteg | Technológia |
|---|---|
| Backend | ASP.NET Core Web API (`/MVP`) |
| Frontend | Vue 3 + Vite, Vue Router (`/frontend`) |
| Adatbázis | SQL Server |
| Auth | JWT |
| Diagramok | Chart.js (vue-chartjs) |

```text
.
├── MVP/                 # .NET backend API
├── frontend/            # Vue frontend
├── MVP.slnx             # Solution
└── README.md
```

## Előfeltételek

- .NET SDK (ajánlott: .NET 8)
- Node.js 18+ és npm
- SQL Server (vagy kompatibilis SQL backend)

## Backend indítása

1. Másold le a példa konfigot és töltsd ki:

   ```bash
   cd MVP
   cp appsettings.example.json appsettings.json
   ```

   Kitöltendő: a connection string és a JWT beállítások.
   Az `appsettings.json` **gitignore-olva van, és annak is kell maradnia** — lásd lent.

2. Indítás:

   ```bash
   dotnet run
   ```

A fejlesztői URL-eket a `MVP/Properties/launchSettings.json` tartalmazza.

## Frontend indítása

```bash
cd frontend
npm install
npm run dev
```

Az API végpont a `frontend/src/services/api.js` fájlban állítható.

## Build

```bash
cd frontend && npm run build      # frontend production build
cd MVP && dotnet publish -c Release
```

## Biztonság

Ennek a repónak volt egy valódi incidense: az `appsettings.json` élő adatbázis-connection stringgel felkerült a publikus git történetbe. A jelszó rotálva lett, a történet átírva, az elárvult branch törölve, és az eredmény **friss klónnal, nem a helyi másolatból** ellenőrizve.

Amit ebből érdemes átvenni:

- **rotálj először, takaríts utána** — a történet átírása a hitelességet állítja helyre, nem a hozzáférést zárja le
- a `git push --force --all` **csak a helyi branch-eket** tolja fel; a csak távolon létező branch a régi történettel együtt életben marad
- **a parancs sikere nem a művelet sikere** — utána mérni kell, nem a kimenetet elhinni

Ezért van a repóban `appsettings.example.json` valódi `appsettings.json` helyett.

## Hasznos fájlok

- `MVP/MVP.http` — API végpontok gyors teszteléséhez
- `MVP/planner_app_database.sql` — adatbázis script
- `MVP/database_update_terms_privacy.sql` — Terms/Privacy frissítő script

## Éles használat előtt

- szenzitív beállítások környezeti változóba
- részletesebb hibakezelés és naplózás
- unit/integration tesztek
