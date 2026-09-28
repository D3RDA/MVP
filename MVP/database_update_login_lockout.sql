-- Fiókzárolás sikertelen bejelentkezések után (2026-09-28)
--
-- Miért kell, ha már van rate limit:
-- a meglévő rate limit IP-cím szerint korlátoz (5 kérés / perc). Aki több
-- IP-ről próbálkozik, azt nem fogja meg. Ez a zárolás FIÓK szerint számol,
-- tehát ugyanazt a fiókot akkor sem lehet találgatni, ha minden próbálkozás
-- másik IP-ről érkezik. A kettő együtt véd, egyedül egyik sem elég.

ALTER TABLE users
  ADD COLUMN failed_login_attempts INT NOT NULL DEFAULT 0 AFTER privacy_accepted_at,
  ADD COLUMN last_failed_login_at DATETIME NULL AFTER failed_login_attempts,
  ADD COLUMN lockout_ends_at DATETIME NULL AFTER last_failed_login_at;
