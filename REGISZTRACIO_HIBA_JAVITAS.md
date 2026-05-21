# Regisztrációs hiba gyors ellenőrzés

1. Indítsd el a MySQL-t XAMPP-ban.
2. Ellenőrizd, hogy létezik a `planner_app` adatbázis.
3. Indítsd el a backendet:

```bash
cd MVP
dotnet run --launch-profile http
```

4. Böngészőben nyisd meg:

```text
http://localhost:5111/api/health
```

Jó válasz esetén ezt kell látnod:

```json
{ "api": "ok", "database": "ok" }
```

5. Indítsd el a frontendet:

```bash
cd frontend
npm install
npm run dev
```

6. A frontendet lehetőleg ezen nyisd meg:

```text
http://localhost:5173
```

Ne a `127.0.0.1:5173` címet használd, bár ebben a javított verzióban már az is engedélyezve van.
