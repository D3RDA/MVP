# MVP backend beállítás

Ez a verzió ASP.NET Core Web API backend MySQL/MariaDB adatbázissal.

## 1. Adatbázis létrehozása

Importáld a `planner_app_database.sql` fájlt MySQL-be vagy MariaDB-be.

Példa parancssorból:

```bash
mysql -u root -p < planner_app_database.sql
```

## 2. Connection string beállítása

Fejlesztői környezetben az `appsettings.Development.json` fájlt a sablonból hozd létre:

```bash
cp appsettings.Development.json.example appsettings.Development.json
```

Windows PowerShellben:

```powershell
Copy-Item appsettings.Development.json.example appsettings.Development.json
```

Ezután az `appsettings.Development.json` fájlban írd át ezt a saját MySQL/MariaDB beállításaidra:

```json
"DefaultConnection": "server=127.0.0.1;port=3306;database=planner_app;user=root;password=YOUR_PASSWORD"
```

XAMPP alapértelmezett, jelszó nélküli `root` felhasználó esetén a `password` értéke maradhat üres.

## 3. JWT kulcs beállítása (User Secrets)

A JWT kulcsot NE tedd az `appsettings` fájlokba. Fejlesztői környezetben használj .NET User Secrets-t:

```bash
cd MVP
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<legalább 32 karakter hosszú véletlenszerű kulcs>"
```

Például egy biztonságos kulcs generálása PowerShellben:

```powershell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))
```

Vagy bash-ben:

```bash
openssl rand -base64 32
```

## 4. NuGet csomagok visszaállítása

Visual Studio általában automatikusan megcsinálja. Parancssorból:

```bash
dotnet restore
```

## 5. Indítás

```bash
dotnet run
```

Alap HTTP cím:

```text
http://localhost:5111
```

## 6. Első teszt

1. `POST /api/auth/register`
2. `POST /api/auth/login`
3. A kapott JWT tokent használd: `Authorization: Bearer TOKEN`
4. Például: `GET /api/projects`

A `MVP.http` fájlban van pár előkészített teszthívás.

## Elkészült API részek

- Auth: regisztráció, login, aktuális user
- Projects: CRUD
- Tasks: projekthez tartozó CRUD
- CalendarEvents: CRUD
- Companies: CRUD
- JobApplications: CRUD
- Notes: CRUD
