# Incident report — database credentials exposed in a public repository

**System:** this repository and the live application it deploys
**Exposure window:** 2026-05-21 → 2026-07-31 (~10 weeks)
**Discovered:** 2026-07-30, by me, during a self-audit
**Closed:** 2026-09-28
**Outcome:** credentials rotated, git history rewritten and independently verified, no evidence of unauthorized access

> This is a real incident on my own live system, not a lab exercise. Identifying
> values — passwords, exact hostnames, account names — are redacted. It is
> published because the response is the part worth reading.

## Summary

A configuration file containing a plaintext database connection string — host,
database name, username and password — together with a hosting deployment
profile, was committed to this repository while it was public. It sat there for
roughly ten weeks before I found it during a review of what my public
repositories actually expose.

I rotated the credentials, removed the files, checked for signs of misuse, and
hardened the repository. Two months later I found that the job was **not
finished**, and closed it properly. That second part is the more useful half of
this document.

## Environment

ASP.NET Core Web API, Vue 3 frontend, MySQL database, managed .NET hosting.
Four commits at the time of exposure.

## Timeline

| When | Event |
|---|---|
| ~2026-05-21 | Repository made public; config file with live credentials committed in a bulk initial upload |
| 2026-07-30 | Found during a self-audit of my public repositories |
| 2026-07-31 | **Credentials rotated** — database and hosting deploy account |
| 2026-07-31 | Repository set to private; secrets removed from tracked files; `.gitignore` hardened; example config added |
| 2026-07-31 | Live application verified working on the new credentials |
| 2026-07-31 | Database and available access logs reviewed for the exposure window — nothing unexpected found |
| 2026-09-28 | **Found that the git history was never rewritten**; the secret was still readable in four commits |
| 2026-09-28 | History rewritten, orphaned branch deleted, result verified from a fresh clone |
| 2026-09-28 | Found that the local working copy still carried the old history; transplanted and verified |

## Detection

Not an automated alert, not a third-party report — my own review. That is itself
a finding: **GitHub secret scanning and push protection were not enabled** on the
account, which is a preventable gap and free to close.

## Severity

| Finding | Severity | Why |
|---|---|---|
| Plaintext DB connection string in a public repo | **Critical** | Directly usable by anyone who found it; live database access |
| Hosting deploy profile with a usable deploy password | **High** | Would allow redeploying over the live application |
| Deploy password encrypted against my local OS user account | **Medium** | Not independently usable, but it confirmed the deploy endpoint and username |
| IDE and environment files tracked | **Low** | No secrets, but should not have been committed |

I deliberately did not inflate this. The encrypted deploy password was not
exploitable on its own, and saying so is part of assessing an incident honestly.

## Impact

The database was reviewed for unexpected records, unfamiliar accounts and
deletions, and the available hosting access logs were checked across the full
exposure window.

**No evidence of unauthorized access or data manipulation was found.** A negative
cannot be proven with certainty — the provider's logging is limited — but nothing
observed is inconsistent with normal use, and the credential is invalid either
way.

## Root cause

- No pre-commit or CI secret scanning
- An environment-specific config file committed in an early bulk "add files"
  commit, before a proper `.gitignore` existed
- No habit of auditing my own public repositories

## What the second round taught me

In July the file stopped appearing in the current tree, the notes said "closed",
and it looked finished. In September I checked again and **the secret was still
in the git history, on a public repository.** Three things had gone wrong
quietly, and each of them fails without an error message:

**1. A command that does not exist still "succeeds" overall.**
`git filter-repo` is not a built-in git command. After it failed,
`git push` reported *"Everything up-to-date"* — which was true, because nothing
had changed. A successful command and a successful outcome are not the same
thing.

**2. `git push --force --all` only pushes local branches.**
One branch existed only as a remote ref. It was never rewritten, so it kept the
old history — and the password — while `main` looked clean. Anyone checking only
the default branch would have concluded the job was done.

**3. A typo in a branch name looks like success if you do not read the output.**
`git push origin --delete codex/add-readme-fil` errored on a missing final
letter. Glance past it and you believe the branch is gone.

**And the part that is easiest to miss entirely:** the cleanup was done in a
fresh clone and pushed from there. The original working copy on my machine was
never touched — it still had the old history, including the secret, and the next
push from that folder would have restored it. Cleaning up in a separate clone is
safe, but it is not finished at the push. The working copy has to be moved over
too, or you are left with two versions of the truth and the older one is the one
under your hands.

**Therefore the closing step was measurement, not a command.** Verified from a
fresh clone taken from the server, not from any local copy:

```
remote branches:      origin/main only
commits:              all new SHAs
appsettings.json:     absent from the entire history
secret host string:   0 matches
```

## What a history rewrite does not fix

GitHub can still serve old commits by direct SHA for a while, and any fork would
have kept them. Full removal needs GitHub Support.

**The rotation was the real protection.** Rewriting history restores the
credibility of the repository — worth doing, but a different category. It is easy
to do it in the wrong order: clean the history, feel relieved, and never rotate.

## Prevention

- [x] Credentials rotated
- [x] Hardened `.gitignore` covering config files, publish profiles and environment files
- [x] Local development uses `dotnet user-secrets` instead of checked-in config
- [x] Git history rewritten and verified from a fresh clone
- [x] Local working copy transplanted onto the clean history
- [x] Rate limiting, account lockout and security headers added (2026-09-28)
- [ ] Enable GitHub secret scanning and push protection account-wide
- [ ] Before making any repository public, grep for secret-bearing filenames and read the output

## What I would tell another developer

Committing a config file with real credentials is common and easy, especially
early in a project before habits exist. The response is what matters.

**Rotate first, clean up second.** Removing a file from git does not undo the
fact that it was public; only invalidating the credential does that.

**Then go back and measure.** Most of what went wrong here went wrong silently,
and every one of those failures would have stayed invisible if I had trusted the
command output instead of checking the result.
