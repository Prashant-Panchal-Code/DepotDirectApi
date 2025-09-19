-- =====================================================================
-- V1__depotdirect_init_slim.sql
-- DepotDirect slim schema (no depots/sites/parkings)
--  - regions belong to companies (regions.company_id)
--  - users belong to companies (users.company_id)
--  - removed depots, sites, parkings and all related mapping tables
-- Schema: depotdirect
-- Owner: depotdirect_user
-- =====================================================================

SET search_path = public;

-- create schema
CREATE SCHEMA IF NOT EXISTS depotdirect AUTHORIZATION depotdirect_user;
COMMENT ON SCHEMA depotdirect IS 'DepotDirect slim schema - countries, companies, regions, roles, users, helper functions';

SET search_path = depotdirect, public;

-- extensions
CREATE EXTENSION IF NOT EXISTS citext;

-- =====================================================================
-- Helper trigger functions
-- =====================================================================

-- updated_at setter
CREATE OR REPLACE FUNCTION depotdirect.fn_set_updated_at()
RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
  NEW.updated_at = now();
  RETURN NEW;
END;
$$;
ALTER FUNCTION depotdirect.fn_set_updated_at() OWNER TO depotdirect_user;

-- soft-delete (only affects tables with 'active' column)
CREATE OR REPLACE FUNCTION depotdirect.fn_soft_delete()
RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
  IF TG_OP = 'DELETE' THEN
    BEGIN
      EXECUTE format('UPDATE depotdirect.%I SET active = false, deleted_at = now() WHERE id = $1', TG_TABLE_NAME) USING OLD.id;
      RETURN NULL; -- prevent physical delete
    EXCEPTION WHEN undefined_column THEN
      -- no 'active' column on this table — allow physical delete
      RETURN OLD;
    END;
  END IF;
  RETURN OLD;
END;
$$;
ALTER FUNCTION depotdirect.fn_soft_delete() OWNER TO depotdirect_user;

-- operating_hours validator (kept for future tables that may use it)
CREATE OR REPLACE FUNCTION depotdirect.fn_validate_operating_hours()
RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE
  keys text[];
  allowed_keys text[] := ARRAY['mon','tue','wed','thu','fri','sat','sun'];
  k text;
  has_col boolean;
BEGIN
  SELECT EXISTS (
    SELECT 1
    FROM information_schema.columns
    WHERE table_schema = 'depotdirect'
      AND table_name = TG_TABLE_NAME
      AND column_name = 'operating_hours'
  ) INTO has_col;

  IF NOT has_col THEN
    RETURN NEW;
  END IF;

  IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
    IF NEW.operating_hours IS NULL THEN
      RETURN NEW;
    END IF;

    IF jsonb_typeof(NEW.operating_hours) <> 'object' THEN
      RAISE EXCEPTION 'operating_hours must be a JSON object';
    END IF;

    SELECT array_agg(key) INTO keys FROM jsonb_object_keys(NEW.operating_hours) AS key;
    IF keys IS NULL THEN
      RETURN NEW;
    END IF;

    FOREACH k IN ARRAY keys LOOP
      IF NOT (k = ANY (allowed_keys)) THEN
        RAISE EXCEPTION 'operating_hours contains invalid key: % (allowed: %)', k, allowed_keys;
      END IF;
    END LOOP;
  END IF;

  RETURN NEW;
END;
$$;
ALTER FUNCTION depotdirect.fn_validate_operating_hours() OWNER TO depotdirect_user;

-- =====================================================================
-- Core lookups: countries, companies, regions (regions -> companies)
-- =====================================================================

CREATE SEQUENCE IF NOT EXISTS depotdirect.countries_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.countries (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.countries_id_seq'),
  name text NOT NULL,
  iso_code varchar(8),
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz
);
ALTER TABLE depotdirect.countries OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_countries_name ON depotdirect.countries (name);

CREATE SEQUENCE IF NOT EXISTS depotdirect.companies_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.companies (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.companies_id_seq'),
  name text NOT NULL,
  company_code text,
  country_id integer NOT NULL REFERENCES depotdirect.countries(id) ON DELETE RESTRICT,
  description text,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz
);
ALTER TABLE depotdirect.companies OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_companies_name ON depotdirect.companies (name);
CREATE INDEX IF NOT EXISTS idx_companies_country ON depotdirect.companies (country_id);

CREATE SEQUENCE IF NOT EXISTS depotdirect.regions_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.regions (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.regions_id_seq'),
  name text NOT NULL,
  region_code text,
  company_id integer NOT NULL REFERENCES depotdirect.companies(id) ON DELETE RESTRICT,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz
);
ALTER TABLE depotdirect.regions OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_regions_name ON depotdirect.regions (name);
CREATE INDEX IF NOT EXISTS idx_regions_company ON depotdirect.regions (company_id);

-- updated_at triggers for core lookups
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_countries') THEN
    CREATE TRIGGER trg_set_updated_at_countries BEFORE UPDATE ON depotdirect.countries FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_companies') THEN
    CREATE TRIGGER trg_set_updated_at_companies BEFORE UPDATE ON depotdirect.companies FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_regions') THEN
    CREATE TRIGGER trg_set_updated_at_regions BEFORE UPDATE ON depotdirect.regions FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================
-- roles & users
-- =====================================================================

CREATE SEQUENCE IF NOT EXISTS depotdirect.roles_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.roles (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.roles_id_seq'),
  name text NOT NULL UNIQUE,
  description text,
  permissions jsonb DEFAULT '{}'::jsonb,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);
ALTER TABLE depotdirect.roles OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_roles_name ON depotdirect.roles (name);

CREATE SEQUENCE IF NOT EXISTS depotdirect.users_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.users (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.users_id_seq'),
  company_id integer REFERENCES depotdirect.companies(id) ON DELETE SET NULL,
  role_id integer NOT NULL REFERENCES depotdirect.roles(id) ON DELETE RESTRICT,
  email citext NOT NULL UNIQUE,
  password_hash text NOT NULL,
  full_name text NOT NULL,
  phone text,
  active boolean NOT NULL DEFAULT true,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz
);
ALTER TABLE depotdirect.users OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_users_company ON depotdirect.users (company_id);
CREATE INDEX IF NOT EXISTS idx_users_role ON depotdirect.users (role_id);

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_users') THEN
    CREATE TRIGGER trg_set_updated_at_users BEFORE UPDATE ON depotdirect.users FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_users') THEN
    CREATE TRIGGER trg_soft_delete_users BEFORE DELETE ON depotdirect.users FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================
-- Seed: roles + minimal sample country/company/region (optional)
-- =====================================================================

INSERT INTO depotdirect.roles (name, description) VALUES
  ('admin','Admin - full access'),
  ('planner','Planner - creates/edits routes and master data'),
  ('driver','Driver - mobile user'),
  ('viewer','Read-only user')
ON CONFLICT (name) DO NOTHING;

DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint
    WHERE conname = 'countries_name_iso_unique'
      AND conrelid = 'depotdirect.countries'::regclass
  ) THEN
    ALTER TABLE depotdirect.countries
      ADD CONSTRAINT countries_name_iso_unique UNIQUE (name, iso_code);
  END IF;
END;
$$ LANGUAGE plpgsql;

INSERT INTO depotdirect.countries (name, iso_code) VALUES ('India','IN') ON CONFLICT DO NOTHING;

WITH c AS (SELECT id as country_id FROM depotdirect.countries LIMIT 1)
INSERT INTO depotdirect.companies (name, company_code, country_id)
SELECT 'Acme Logistics','ACME', c.country_id FROM c
ON CONFLICT DO NOTHING;

WITH comp AS (SELECT id FROM depotdirect.companies LIMIT 1)
INSERT INTO depotdirect.regions (name, region_code, company_id)
SELECT 'Karnataka','KA', comp.id FROM comp ON CONFLICT DO NOTHING;

-- =====================================================================
-- Ownership (defensive)
-- =====================================================================

ALTER TABLE IF EXISTS depotdirect.countries OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.companies OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.regions OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.roles OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.users OWNER TO depotdirect_user;

ALTER SEQUENCE IF EXISTS depotdirect.countries_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.companies_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.regions_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.roles_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.users_id_seq OWNER TO depotdirect_user;

SET search_path = depotdirect, public;

-- =====================================================================
-- Notes:
--  - depots/sites/parkings and site_regions/depot_regions/parking_regions and
--    their related mapping tables have been removed in this slim version.
--  - If you need a migration to drop those objects from an existing DB (keep backups),
--    I can create a safe drop/move script that preserves data before deletion.
-- =====================================================================

-- End of V1__depotdirect_init_slim.sql
-- =====================================================================
