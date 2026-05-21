# JWT kulcs beállítása User Secrets-ben

A JWT titkos kulcs szándékosan nincs az `appsettings.json` vagy az `appsettings.Development.json` fájlban.
Fejlesztéshez állítsd be .NET User Secrets-ben.

## Parancsok

```bash
cd MVP/MVP
dotnet user-secrets set "Jwt:Key" "CSERELD_LE_EGY_EROS_VELETLENSZERU_64_KARAKTERES_KULCSRA"
```

Példa erős kulcs generálására PowerShellben:

```powershell
[Convert]::ToHexString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32)).ToLower()
```

Majd a kapott értéket használd:

```bash
dotnet user-secrets set "Jwt:Key" "ide_jon_a_generalt_kulcs"
```

Produkcióban inkább környezeti változót használj:

```bash
Jwt__Key=ide_jon_az_eros_kulcs
```

Megjegyzés: a `Jwt:Issuer`, `Jwt:Audience` és `Jwt:ExpiresInMinutes` maradhat konfigurációban, ezek nem titkok.
