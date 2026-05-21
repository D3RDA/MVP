# Planner App — backend + Vue frontend

Ez a ZIP két részt tartalmaz:

- `MVP/` — ASP.NET Core Web API backend MySQL/MariaDB adatbázissal
- `frontend/` — Vue.js + Vite frontend

## 1. Adatbázis

Indítsd el a MySQL-t vagy MariaDB-t, majd importáld:

```bash
mysql -u root < MVP/MVP/planner_app_database.sql
```

XAMPP esetén phpMyAdminból is importálhatod.

Ha már korábbi adatbázist használsz, futtasd le a jogi elfogadási mezőket hozzáadó frissítést is:

```bash
mysql -u root planner_app < MVP/MVP/database_update_terms_privacy.sql
```

## 2. Backend indítása

### Fejlesztői konfiguráció létrehozása

Első indítás előtt hozd létre a fejlesztői konfigurációt a sablonból:

```bash
cd MVP/MVP
cp appsettings.Development.json.example appsettings.Development.json
```

Windows PowerShellben:

```powershell
cd MVP/MVP
Copy-Item appsettings.Development.json.example appsettings.Development.json
```

Ezután az `appsettings.Development.json` fájlban állítsd be a saját MySQL/MariaDB kapcsolatodat.
XAMPP alapértelmezett, jelszó nélküli `root` felhasználó esetén használható például:

```json
"DefaultConnection": "server=127.0.0.1;port=3306;database=planner_app;user=root;password=;"
```

### JWT kulcs beállítása (első indítás előtt)

A JWT kulcsot NE tedd az `appsettings` fájlokba. Fejlesztői környezetben állítsd be .NET User Secrets-szel:

```bash
cd MVP/MVP
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<legalább 32 karakter hosszú titkos kulcs>"
```

PowerShellben generálhatsz kulcsot például így:

```powershell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))
```

Vagy bash-ben:

```bash
openssl rand -base64 32
```

Indítás:

```bash
cd MVP/MVP
dotnet restore
dotnet run --launch-profile http
```

Alap API cím:

```text
http://localhost:5111/api
```

## 3. Frontend indítása

```bash
cd MVP/frontend
npm install
npm run dev
```

Frontend cím:

```text
http://localhost:5173
```

## 4. Használati sorrend

1. Backend fusson: `http://localhost:5111`
2. Frontend fusson: `http://localhost:5173`
3. Regisztrálj egy új felhasználót
4. Hozz létre projektet, álláskövető bejegyzést, eseményt vagy jegyzetet

## Elkészült fő funkciók

- regisztráció
- bejelentkezés JWT tokennel
- dashboard statisztika
- projekt CRUD
- projekthez tartozó feladat CRUD
- cég CRUD
- álláskövető: cég + jelentkezés egy űrlapon
- naptáresemény CRUD
- jegyzet CRUD
- Vue frontend alapoldalakkal
