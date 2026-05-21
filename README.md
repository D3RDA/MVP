# MVP Planner App

Ez a repository egy **planner / productivity** alkalmazás MVP-je, két részből:

- **Backend**: ASP.NET Core Web API (`/MVP`)
- **Frontend**: Vue 3 + Vite (`/frontend`)

## Fő funkciók

- Felhasználói regisztráció és bejelentkezés JWT auth-tal
- Dashboard nézet
- Feladatok kezelése
- Jegyzetek kezelése
- Projektek kezelése
- Cégek kezelése
- Állásjelentkezések kezelése
- Naptár események kezelése
- Terms/Privacy oldalak

## Projekt struktúra

```text
.
├── MVP/                 # .NET backend API
├── frontend/            # Vue frontend
├── MVP.slnx             # Solution
└── README.md
```

## Előfeltételek

- .NET SDK (ajánlott: .NET 8)
- Node.js 18+
- npm
- SQL Server (vagy kompatibilis SQL backend a connection string alapján)

## Backend indítása

1. Lépj a backend mappába:

   ```bash
   cd MVP
   ```

2. Konfiguráld az `appsettings.json` fájlt (különösen a connection stringet és JWT beállításokat).

3. Indítás:

   ```bash
   dotnet run
   ```

Alapértelmezett fejlesztői URL-eket a `MVP/Properties/launchSettings.json` tartalmazza.

## Frontend indítása

1. Lépj a frontend mappába:

   ```bash
   cd frontend
   ```

2. Függőségek telepítése:

   ```bash
   npm install
   ```

3. Fejlesztői szerver indítása:

   ```bash
   npm run dev
   ```

A frontend API endpoint beállítása a `frontend/src/services/api.js` fájlban található.

## Build

### Frontend production build

```bash
cd frontend
npm run build
```

### Backend publish (példa)

```bash
cd MVP
dotnet publish -c Release
```

## Hasznos fájlok

- `MVP/MVP.http` – API endpointok gyors teszteléséhez
- `MVP/planner_app_database.sql` – adatbázis script
- `MVP/database_update_terms_privacy.sql` – Terms/Privacy frissítő script

## Megjegyzés

Ez egy MVP állapotú projekt, ezért éles környezetbe deploy előtt javasolt:

- környezeti változókra átállítani a szenzitív beállításokat,
- részletesebb hibakezelést és naplózást bevezetni,
- teszteket hozzáadni (unit/integration).
