-- Create/patch users schema without audit triggers
-- Run with search_path = public (we'll set depotdirect in script)
SET search_path = public;
CREATE EXTENSION IF NOT EXISTS citext;
-- ensure depotdirect schema exists
CREATE SCHEMA IF NOT EXISTS depotdirect AUTHORIZATION depotdirect_user;
SET search_path = depotdirect, public;

-- 0) ensure citext extension (for case-insensitive email)
CREATE EXTENSION IF NOT EXISTS citext;

-- 1) cleanup: drop audit trigger on users if it exists (safe no-op otherwise)
DO $$
BEGIN
  IF EXISTS (
    SELECT 1 FROM pg_trigger t
    JOIN pg_class c ON t.tgrelid = c.oid
    WHERE t.tgname = 'trg_audit_users' AND c.relname = 'users'
  ) THEN
    EXECUTE 'DROP TRIGGER IF EXISTS trg_audit_users ON depotdirect.users';
  END IF;
END;
$$ LANGUAGE plpgsql;

-- Also drop any reference to the audit function trigger we might have created by mistake
-- (this does not drop the function itself, only cleans the trigger)
-- If you later create audit infrastructure, you can re-add triggers.

-- 2) create sequence and users table (idempotent)
CREATE SEQUENCE IF NOT EXISTS depotdirect.users_id_seq START 1;

CREATE TABLE IF NOT EXISTS depotdirect.users (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.users_id_seq'),
  company_id integer REFERENCES depotdirect.companies(id) ON DELETE SET NULL,
  email citext NOT NULL UNIQUE,
  password_hash text NOT NULL,
  full_name text NOT NULL,
  phone text,
  role text NOT NULL DEFAULT 'user',
  active boolean NOT NULL DEFAULT true,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz
);
ALTER TABLE depotdirect.users OWNER TO depotdirect_user;

-- 3) indexes
CREATE INDEX IF NOT EXISTS users_company_idx ON depotdirect.users (company_id);
CREATE INDEX IF NOT EXISTS users_role_idx ON depotdirect.users (role);

-- 4) user_regions join table
CREATE SEQUENCE IF NOT EXISTS depotdirect.user_regions_id_seq START 1;

CREATE TABLE IF NOT EXISTS depotdirect.user_regions (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.user_regions_id_seq'),
  user_id integer NOT NULL REFERENCES depotdirect.users(id) ON DELETE CASCADE,
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE CASCADE,
  created_at timestamptz NOT NULL DEFAULT now(),
  UNIQUE (user_id, region_id)
);
ALTER TABLE depotdirect.user_regions OWNER TO depotdirect_user;

CREATE INDEX IF NOT EXISTS user_regions_user_idx ON depotdirect.user_regions (user_id);
CREATE INDEX IF NOT EXISTS user_regions_region_idx ON depotdirect.user_regions (region_id);

-- 5) attach updated_at trigger (uses existing depotdirect.fn_set_updated_at)
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_users') THEN
    CREATE TRIGGER trg_set_updated_at_users
    BEFORE UPDATE ON depotdirect.users
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 6) optional: attach soft-delete trigger if fn_soft_delete exists; otherwise skip
DO $$
BEGIN
  -- attach only if function exists
  IF EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'fn_soft_delete' AND pronamespace = (SELECT oid FROM pg_namespace WHERE nspname='depotdirect')) THEN
    IF NOT EXISTS (
      SELECT 1 FROM pg_trigger t
      JOIN pg_class c ON t.tgrelid = c.oid
      WHERE t.tgname = 'trg_soft_delete_users' AND c.relname = 'users'
    ) THEN
      EXECUTE 'CREATE TRIGGER trg_soft_delete_users BEFORE DELETE ON depotdirect.users FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete()';
    END IF;
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 7) sample admin insert (idempotent)
WITH comp AS (SELECT id FROM depotdirect.companies LIMIT 1)
INSERT INTO depotdirect.users (company_id, email, password_hash, full_name, phone, role, created_by)
SELECT comp.id, 'admin@example.com', '$2b$12$examplebcryptplaceholder', 'System Admin', '+91-9000000000', 'admin', NULL
FROM comp
ON CONFLICT (email) DO NOTHING;

-- 8) ensure ownership of sequences
ALTER SEQUENCE IF EXISTS depotdirect.users_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.user_regions_id_seq OWNER TO depotdirect_user;

-- Done.
