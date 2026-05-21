ALTER TABLE users
  ADD COLUMN accepted_terms_version VARCHAR(20) NULL AFTER updated_at,
  ADD COLUMN accepted_privacy_version VARCHAR(20) NULL AFTER accepted_terms_version,
  ADD COLUMN terms_accepted_at DATETIME NULL AFTER accepted_privacy_version,
  ADD COLUMN privacy_accepted_at DATETIME NULL AFTER terms_accepted_at;
