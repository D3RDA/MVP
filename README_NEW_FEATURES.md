# Új biztonsági és UX módosítások

## Jogi elfogadás regisztrációnál

A regisztrációhoz most már kötelező elfogadni:

- Felhasználási feltételek
- Adatkezelési tájékoztató

Frontend oldalak:

- `/felhasznalasi-feltetelek`
- `/adatkezelesi-tajekoztato`

Backend oldalon a `RegisterRequest` tartalmazza:

```json
{
  "acceptTerms": true,
  "acceptPrivacy": true
}
```

A `users` táblába bekerültek az elfogadási mezők:

- `accepted_terms_version`
- `accepted_privacy_version`
- `terms_accepted_at`
- `privacy_accepted_at`

Ha már létező adatbázist használsz, futtasd le:

```bash
mysql -u root -p planner_app < MVP/database_update_terms_privacy.sql
```

XAMPP jelszó nélküli root esetén:

```bash
mysql -u root planner_app < MVP/database_update_terms_privacy.sql
```

## Login/register rate limit

A backend IP-alapú rate limitet kapott az auth végpontokra:

- `POST /api/auth/login`
- `POST /api/auth/register`

Limit: 5 kérés / perc / IP.

Túl sok próbálkozás esetén a válasz: `429 Too Many Requests`.

## Álláskövető UX

A frontendben az `Állásjelentkezések` menüpont neve `Álláskövető` lett.

Új jelentkezésnél már nem kell külön céget létrehozni. Egyetlen űrlapon lehet megadni:

- cég adatai
- kapcsolattartó adatai
- pozíció
- álláshirdetés URL
- fizetés
- tapasztalat
- következő lépés

A backend új végpontjai:

- `POST /api/jobapplications/with-company`
- `PUT /api/jobapplications/{id}/with-company`

Az adatbázis szerkezet továbbra is normalizált: a cégek és jelentkezések külön táblában maradnak, csak a felhasználói felület lett egyszerűbb.

## Fontos jogi megjegyzés

A jogi szövegek fejlesztői sablonok. Éles internetes publikálás előtt ügyvéddel vagy adatvédelmi szakemberrel ellenőriztesd.
