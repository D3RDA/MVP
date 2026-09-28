# Security Policy

## Reporting a vulnerability

If you find a security problem in this project, please report it privately
rather than opening a public issue.

- Open a **draft security advisory**: https://github.com/D3RDA/MVP/security/advisories/new
- Or email the address on my GitHub profile.

Please include what you found, how to reproduce it, and what you think the
impact is. I will confirm receipt and tell you what I intend to do about it.

## Scope

This is a personal project with a single live instance. The parts most worth
looking at are authentication (`MVP/Controllers/AuthController.cs`), the JWT
issuing service, and whether any API endpoint returns data belonging to a
different user.

## What this project already does

| Control | Where |
|---|---|
| Passwords stored with ASP.NET Core `PasswordHasher` (PBKDF2, per-password salt) | `AuthController` |
| Rate limiting on login and registration — 5 requests per minute per IP | `Program.cs` |
| Account lockout — 5 failed logins lock the account for 15 minutes | `AuthController` |
| Equal response time for unknown and known email addresses, to avoid user enumeration | `AuthController` |
| Security headers on the API: CSP, `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy`, HSTS | `Program.cs` |
| Security headers on the frontend, including a CSP that names the API origin | `frontend/public/web.config` |
| Secrets kept out of the repository; an example config documents the shape | `MVP/appsettings.example.json` |

## Known limitations

Stated plainly, because pretending otherwise would be worse:

- **Rate limiting is per IP and in-memory.** It resets when the process restarts, and it is not shared across instances. The account lockout is what covers distributed guessing.
- **Telling a user that their account is locked reveals that the account exists.** This is a deliberate trade-off: without the message, a locked-out user has no way to understand why a correct password is rejected. The attacker has to spend five failed attempts on that specific address to learn it.
- **There is no multi-factor authentication.**
- **There is no password complexity policy**, only the client-side minimum.
- **A JWT cannot be revoked before it expires.** Logging out drops the token on the client; it stays valid on the server until it expires.

## Past incidents

A real credential exposure in this repository is documented in
[`docs/SECURITY-INCIDENT-2026-07.md`](docs/SECURITY-INCIDENT-2026-07.md),
including what went wrong, what was done about it, and what was verified
afterwards.
