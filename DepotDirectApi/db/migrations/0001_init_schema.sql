-- =====================================================================
-- V1__depotdirect_init.sql
-- DepotDirect initial schema (fresh start)
-- Schema: depotdirect
-- Owner: depotdirect_user  -- ensure this DB role exists
-- =====================================================================

SET search_path = public;

-- 0. create schema
CREATE SCHEMA IF NOT EXISTS depotdirect AUTHORIZATION depotdirect_user;
COMMENT ON SCHEMA depotdirect IS 'DepotDirect initial schema - master data (countries, companies, regions, depots, sites, parkings, mappings, users)';

SET search_path = depotdirect, public;

-- 0.1 extensions
CREATE EXTENSION IF NOT EXISTS citext;

-- =====================================================================
-- 1. Helper trigger functions (idempotent replacements)
-- =====================================================================

-- 1.1 updated_at setter
CREATE OR REPLACE FUNCTION depotdirect.fn_set_updated_at()
RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
  NEW.updated_at = now();
  RETURN NEW;
END;
$$;
ALTER FUNCTION depotdirect.fn_set_updated_at() OWNER TO depotdirect_user;

-- 1.2 soft-delete: converts DELETE -> active=false + deleted_at (only for tables that have 'active' column)
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

-- 1.3 operating_hours validator - SAFE: first check column existence
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
-- 2. Core lookups: countries, companies, regions
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
  country_id integer NOT NULL REFERENCES depotdirect.countries(id) ON DELETE RESTRICT,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz
);
ALTER TABLE depotdirect.regions OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_regions_name ON depotdirect.regions (name);
CREATE INDEX IF NOT EXISTS idx_regions_country ON depotdirect.regions (country_id);

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
-- 3. Master location tables: depots, sites, parkings
--    - codes NOT NULL and UNIQUE per country (UNIQUE(country_id, code))
--    - country_id NOT NULL
--    - no company_id or region_id here
-- =====================================================================

-- depots
CREATE SEQUENCE IF NOT EXISTS depotdirect.depots_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.depots (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.depots_id_seq'),
  depot_code text NOT NULL,
  depot_name text NOT NULL,
  latitude numeric(10,7),
  longitude numeric(10,7),
  latlong text GENERATED ALWAYS AS (
    CASE WHEN latitude IS NOT NULL AND longitude IS NOT NULL THEN (latitude::text || ',' || longitude::text) ELSE NULL END
  ) STORED,
  street text,
  postal_code text,
  town text,
  country_id integer NOT NULL REFERENCES depotdirect.countries(id) ON DELETE RESTRICT,
  active boolean NOT NULL DEFAULT true,
  priority text NOT NULL DEFAULT 'Medium',
  is_parking boolean NOT NULL DEFAULT false,
  manager_name text,
  manager_phone text,
  manager_email text,
  emergency_contact text,
  loading_bays integer,
  average_loading_time integer,
  max_truck_size text,
  certifications text,
  operating_hours jsonb DEFAULT '{}'::jsonb,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,
  CONSTRAINT depots_priority_chk CHECK (priority IN ('High','Medium','Low')),
  CONSTRAINT depots_country_code_uniq UNIQUE (country_id, depot_code)
);
ALTER TABLE depotdirect.depots OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_depots_depot_code ON depotdirect.depots (depot_code);
CREATE INDEX IF NOT EXISTS idx_depots_country ON depotdirect.depots (country_id);
CREATE INDEX IF NOT EXISTS idx_depots_town ON depotdirect.depots (town);

-- sites
CREATE SEQUENCE IF NOT EXISTS depotdirect.sites_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.sites (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.sites_id_seq'),
  site_code text NOT NULL,
  site_name text NOT NULL,
  latitude numeric(10,7),
  longitude numeric(10,7),
  latlong text GENERATED ALWAYS AS (
    CASE WHEN latitude IS NOT NULL AND longitude IS NOT NULL THEN (latitude::text || ',' || longitude::text) ELSE NULL END
  ) STORED,
  street text,
  postal_code text,
  town text,
  country_id integer NOT NULL REFERENCES depotdirect.countries(id) ON DELETE RESTRICT,
  active boolean NOT NULL DEFAULT true,
  priority text NOT NULL DEFAULT 'Medium',
  depot_id integer REFERENCES depotdirect.depots(id) ON DELETE SET NULL,
  contact_person text,
  phone text,
  email text,
  operating_hours jsonb DEFAULT '{}'::jsonb,
  delivery_stopped boolean NOT NULL DEFAULT false,
  pumped_required boolean NOT NULL DEFAULT false,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,
  CONSTRAINT sites_priority_chk CHECK (priority IN ('High','Medium','Low')),
  CONSTRAINT sites_country_code_uniq UNIQUE (country_id, site_code)
);
ALTER TABLE depotdirect.sites OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_sites_site_code ON depotdirect.sites (site_code);
CREATE INDEX IF NOT EXISTS idx_sites_country ON depotdirect.sites (country_id);
CREATE INDEX IF NOT EXISTS idx_sites_town ON depotdirect.sites (town);

-- parkings
CREATE SEQUENCE IF NOT EXISTS depotdirect.parkings_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.parkings (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.parkings_id_seq'),
  parking_code text NOT NULL,
  parking_name text NOT NULL,
  latitude numeric(10,7),
  longitude numeric(10,7),
  latlong text GENERATED ALWAYS AS (
    CASE WHEN latitude IS NOT NULL AND longitude IS NOT NULL THEN (latitude::text || ',' || longitude::text) ELSE NULL END
  ) STORED,
  street text,
  postal_code text,
  town text,
  country_id integer NOT NULL REFERENCES depotdirect.countries(id) ON DELETE RESTRICT,
  active boolean NOT NULL DEFAULT true,
  priority text NOT NULL DEFAULT 'Medium',
  is_depot boolean NOT NULL DEFAULT false,
  manager_name text,
  manager_phone text,
  manager_email text,
  emergency_contact text,
  parking_spaces integer,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,
  CONSTRAINT parkings_priority_chk CHECK (priority IN ('High','Medium','Low')),
  CONSTRAINT parkings_country_code_uniq UNIQUE (country_id, parking_code)
);
ALTER TABLE depotdirect.parkings OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_parkings_parking_code ON depotdirect.parkings (parking_code);
CREATE INDEX IF NOT EXISTS idx_parkings_country ON depotdirect.parkings (country_id);
CREATE INDEX IF NOT EXISTS idx_parkings_town ON depotdirect.parkings (town);

-- Attach updated_at / operating_hours / soft-delete triggers ONLY if table has columns
DO $$
BEGIN
  -- depots: attach triggers
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_depots') THEN
    CREATE TRIGGER trg_set_updated_at_depots BEFORE UPDATE ON depotdirect.depots FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema='depotdirect' AND table_name='depots' AND column_name='operating_hours'
  ) THEN
    -- operating_hours exists (should), attach validator
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_validate_operating_hours_depots') THEN
      CREATE TRIGGER trg_validate_operating_hours_depots BEFORE INSERT OR UPDATE ON depotdirect.depots FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_operating_hours();
    END IF;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_depots') THEN
    CREATE TRIGGER trg_soft_delete_depots BEFORE DELETE ON depotdirect.depots FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;

  -- sites
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_sites') THEN
    CREATE TRIGGER trg_set_updated_at_sites BEFORE UPDATE ON depotdirect.sites FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
  IF EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema='depotdirect' AND table_name='sites' AND column_name='operating_hours'
  ) THEN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_validate_operating_hours_sites') THEN
      CREATE TRIGGER trg_validate_operating_hours_sites BEFORE INSERT OR UPDATE ON depotdirect.sites FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_operating_hours();
    END IF;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_sites') THEN
    CREATE TRIGGER trg_soft_delete_sites BEFORE DELETE ON depotdirect.sites FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;

  -- parkings
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_parkings') THEN
    CREATE TRIGGER trg_set_updated_at_parkings BEFORE UPDATE ON depotdirect.parkings FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
  IF EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema='depotdirect' AND table_name='parkings' AND column_name='operating_hours'
  ) THEN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_validate_operating_hours_parkings') THEN
      CREATE TRIGGER trg_validate_operating_hours_parkings BEFORE INSERT OR UPDATE ON depotdirect.parkings FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_operating_hours();
    END IF;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_parkings') THEN
    CREATE TRIGGER trg_soft_delete_parkings BEFORE DELETE ON depotdirect.parkings FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================
-- 4. Many-to-many mapping tables (companies<->regions, master->regions, master->companies)
--    join tables do not duplicate master-level codes but can optionally carry overrides
-- =====================================================================

-- company_regions
CREATE SEQUENCE IF NOT EXISTS depotdirect.company_regions_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.company_regions (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.company_regions_id_seq'),
  company_id integer NOT NULL REFERENCES depotdirect.companies(id) ON DELETE CASCADE,
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE CASCADE,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,
  UNIQUE (company_id, region_id)
);
ALTER TABLE depotdirect.company_regions OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_cr_company ON depotdirect.company_regions (company_id);
CREATE INDEX IF NOT EXISTS idx_cr_region ON depotdirect.company_regions (region_id);

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname='trg_set_updated_at_company_regions') THEN
    CREATE TRIGGER trg_set_updated_at_company_regions BEFORE UPDATE ON depotdirect.company_regions FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname='trg_soft_delete_company_regions') THEN
    CREATE TRIGGER trg_soft_delete_company_regions BEFORE DELETE ON depotdirect.company_regions FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- site_regions
CREATE SEQUENCE IF NOT EXISTS depotdirect.site_regions_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.site_regions (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.site_regions_id_seq'),
  site_id integer NOT NULL REFERENCES depotdirect.sites(id) ON DELETE CASCADE,
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE CASCADE,
  site_code text, -- optional region-specific override
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,
  UNIQUE (site_id, region_id)
);
ALTER TABLE depotdirect.site_regions OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_site_regions_site ON depotdirect.site_regions (site_id);
CREATE INDEX IF NOT EXISTS idx_site_regions_region ON depotdirect.site_regions (region_id);

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname='trg_set_updated_at_site_regions') THEN
    CREATE TRIGGER trg_set_updated_at_site_regions BEFORE UPDATE ON depotdirect.site_regions FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname='trg_soft_delete_site_regions') THEN
    CREATE TRIGGER trg_soft_delete_site_regions BEFORE DELETE ON depotdirect.site_regions FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- depot_regions
CREATE SEQUENCE IF NOT EXISTS depotdirect.depot_regions_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.depot_regions (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.depot_regions_id_seq'),
  depot_id integer NOT NULL REFERENCES depotdirect.depots(id) ON DELETE CASCADE,
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE CASCADE,
  depot_code text,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,
  UNIQUE (depot_id, region_id)
);
ALTER TABLE depotdirect.depot_regions OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_depot_regions_depot ON depotdirect.depot_regions (depot_id);
CREATE INDEX IF NOT EXISTS idx_depot_regions_region ON depotdirect.depot_regions (region_id);

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname='trg_set_updated_at_depot_regions') THEN
    CREATE TRIGGER trg_set_updated_at_depot_regions BEFORE UPDATE ON depotdirect.depot_regions FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname='trg_soft_delete_depot_regions') THEN
    CREATE TRIGGER trg_soft_delete_depot_regions BEFORE DELETE ON depotdirect.depot_regions FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- parking_regions
CREATE SEQUENCE IF NOT EXISTS depotdirect.parking_regions_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.parking_regions (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.parking_regions_id_seq'),
  parking_id integer NOT NULL REFERENCES depotdirect.parkings(id) ON DELETE CASCADE,
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE CASCADE,
  parking_code text,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,
  UNIQUE (parking_id, region_id)
);
ALTER TABLE depotdirect.parking_regions OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_parking_regions_parking ON depotdirect.parking_regions (parking_id);
CREATE INDEX IF NOT EXISTS idx_parking_regions_region ON depotdirect.parking_regions (region_id);

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname='trg_set_updated_at_parking_regions') THEN
    CREATE TRIGGER trg_set_updated_at_parking_regions BEFORE UPDATE ON depotdirect.parking_regions FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname='trg_soft_delete_parking_regions') THEN
    CREATE TRIGGER trg_soft_delete_parking_regions BEFORE DELETE ON depotdirect.parking_regions FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- site_companies
CREATE SEQUENCE IF NOT EXISTS depotdirect.site_companies_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.site_companies (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.site_companies_id_seq'),
  site_id integer NOT NULL REFERENCES depotdirect.sites(id) ON DELETE CASCADE,
  company_id integer NOT NULL REFERENCES depotdirect.companies(id) ON DELETE CASCADE,
  site_code text,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,
  UNIQUE (site_id, company_id)
);
ALTER TABLE depotdirect.site_companies OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_site_companies_site ON depotdirect.site_companies (site_id);
CREATE INDEX IF NOT EXISTS idx_site_companies_company ON depotdirect.site_companies (company_id);

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname='trg_set_updated_at_site_companies') THEN
    CREATE TRIGGER trg_set_updated_at_site_companies BEFORE UPDATE ON depotdirect.site_companies FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname='trg_soft_delete_site_companies') THEN
    CREATE TRIGGER trg_soft_delete_site_companies BEFORE DELETE ON depotdirect.site_companies FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- depot_companies
CREATE SEQUENCE IF NOT EXISTS depotdirect.depot_companies_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.depot_companies (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.depot_companies_id_seq'),
  depot_id integer NOT NULL REFERENCES depotdirect.depots(id) ON DELETE CASCADE,
  company_id integer NOT NULL REFERENCES depotdirect.companies(id) ON DELETE CASCADE,
  depot_code text,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,
  UNIQUE (depot_id, company_id)
);
ALTER TABLE depotdirect.depot_companies OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_depot_companies_depot ON depotdirect.depot_companies (depot_id);
CREATE INDEX IF NOT EXISTS idx_depot_companies_company ON depotdirect.depot_companies (company_id);

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname='trg_set_updated_at_depot_companies') THEN
    CREATE TRIGGER trg_set_updated_at_depot_companies BEFORE UPDATE ON depotdirect.depot_companies FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname='trg_soft_delete_depot_companies') THEN
    CREATE TRIGGER trg_soft_delete_depot_companies BEFORE DELETE ON depotdirect.depot_companies FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- parking_companies
CREATE SEQUENCE IF NOT EXISTS depotdirect.parking_companies_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.parking_companies (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.parking_companies_id_seq'),
  parking_id integer NOT NULL REFERENCES depotdirect.parkings(id) ON DELETE CASCADE,
  company_id integer NOT NULL REFERENCES depotdirect.companies(id) ON DELETE CASCADE,
  parking_code text,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,
  UNIQUE (parking_id, company_id)
);
ALTER TABLE depotdirect.parking_companies OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_parking_companies_parking ON depotdirect.parking_companies (parking_id);
CREATE INDEX IF NOT EXISTS idx_parking_companies_company ON depotdirect.parking_companies (company_id);

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname='trg_set_updated_at_parking_companies') THEN
    CREATE TRIGGER trg_set_updated_at_parking_companies BEFORE UPDATE ON depotdirect.parking_companies FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname='trg_soft_delete_parking_companies') THEN
    CREATE TRIGGER trg_soft_delete_parking_companies BEFORE DELETE ON depotdirect.parking_companies FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- =====================================================================
-- 5. roles & users (roles table and users.role_id FK)
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
-- 6. Seed basic roles & sample data (optional)
-- =====================================================================

INSERT INTO depotdirect.roles (name, description) VALUES
  ('admin','Admin - full access'),
  ('planner','Planner - creates/edits routes and master data'),
  ('driver','Driver - mobile user'),
  ('viewer','Read-only user')
ON CONFLICT (name) DO NOTHING;

-- 1) Add a unique constraint on (name, iso_code) if it doesn't exist
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

WITH c AS (SELECT id FROM depotdirect.countries LIMIT 1)
INSERT INTO depotdirect.companies (name, company_code, country_id) SELECT 'Acme Logistics','ACME', c.id FROM c ON CONFLICT DO NOTHING;

WITH c AS (SELECT id FROM depotdirect.countries LIMIT 1)
INSERT INTO depotdirect.regions (name, region_code, country_id) SELECT 'Karnataka','KA', c.id FROM c ON CONFLICT DO NOTHING;

-- Sample master rows (codes unique by country)
WITH cnt AS (SELECT id FROM depotdirect.countries LIMIT 1)
INSERT INTO depotdirect.depots (depot_code, depot_name, latitude, longitude, street, postal_code, town, country_id, active, priority, loading_bays, average_loading_time, max_truck_size, operating_hours)
SELECT 'DPT-001','Acme Depot Bengaluru',12.9715987,77.5945627,'1 Sample St','560001','Bengaluru', cnt.id, true, 'High', 4, 60, 'large','{"mon":{"open":"09:00","close":"18:00"}}'::jsonb FROM cnt
ON CONFLICT DO NOTHING;

WITH cnt AS (SELECT id FROM depotdirect.countries LIMIT 1), d AS (SELECT id FROM depotdirect.depots WHERE depot_code='DPT-001' LIMIT 1)
INSERT INTO depotdirect.sites (site_code, site_name, latitude, longitude, street, postal_code, town, country_id, active, priority, depot_id, contact_person, phone, email, operating_hours)
SELECT 'SITE-001','Acme Site Bengaluru',12.9720,77.5950,'2 Sample Lane','560001','Bengaluru', cnt.id, true, 'Medium', d.id, 'Ravi Kumar','+91-9876543210','ravi@example.com','{"mon":{"open":"08:00","close":"17:00"}}'::jsonb FROM cnt, d
ON CONFLICT DO NOTHING;

WITH cnt AS (SELECT id FROM depotdirect.countries LIMIT 1)
INSERT INTO depotdirect.parkings (parking_code, parking_name, latitude, longitude, street, postal_code, town, country_id, active, priority, manager_name, manager_phone, parking_spaces)
SELECT 'PKG-001','Acme West Parking',12.9705,77.5900,'3 Parking Rd','560002','Bengaluru', cnt.id, true, 'Low','Suresh','+91-9998887777',25 FROM cnt
ON CONFLICT DO NOTHING;

-- =====================================================================
-- 7. Ensure ownership of all objects to depotdirect_user (defensive)
-- =====================================================================

ALTER TABLE IF EXISTS depotdirect.countries OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.companies OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.regions OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.depots OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.sites OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.parkings OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.company_regions OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.site_regions OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.depot_regions OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.parking_regions OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.site_companies OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.depot_companies OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.parking_companies OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.roles OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.users OWNER TO depotdirect_user;

ALTER SEQUENCE IF EXISTS depotdirect.countries_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.companies_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.regions_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.depots_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.sites_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.parkings_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.company_regions_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.site_regions_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.depot_regions_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.parking_regions_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.site_companies_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.depot_companies_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.parking_companies_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.roles_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.users_id_seq OWNER TO depotdirect_user;


-- ensure we are in correct schema
SET search_path = depotdirect, public;

-- 1) create sequence & table (idempotent)
CREATE SEQUENCE IF NOT EXISTS depotdirect.user_regions_id_seq START 1;

CREATE TABLE IF NOT EXISTS depotdirect.user_regions (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.user_regions_id_seq'),
  user_id integer NOT NULL REFERENCES depotdirect.users(id) ON DELETE CASCADE,
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE CASCADE,
  created_by integer,                -- user id who created this mapping
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,
  metadata jsonb DEFAULT '{}'::jsonb,
  UNIQUE (user_id, region_id)
);

ALTER TABLE depotdirect.user_regions OWNER TO depotdirect_user;

-- 2) indexes
CREATE INDEX IF NOT EXISTS idx_user_regions_user ON depotdirect.user_regions (user_id);
CREATE INDEX IF NOT EXISTS idx_user_regions_region ON depotdirect.user_regions (region_id);

-- 3) triggers: updated_at + soft-delete
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_user_regions') THEN
    CREATE TRIGGER trg_set_updated_at_user_regions BEFORE UPDATE ON depotdirect.user_regions FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_user_regions') THEN
    CREATE TRIGGER trg_soft_delete_user_regions BEFORE DELETE ON depotdirect.user_regions FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;


-- added company to user id:
-- use proper schema
SET search_path = depotdirect, public;

-- 1. Add company_id column to users (nullable) with FK
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema='depotdirect' AND table_name='users' AND column_name='company_id'
  ) THEN
    ALTER TABLE depotdirect.users
      ADD COLUMN company_id integer;
  END IF;
END$$;

-- Add FK constraint if not exists
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint
    WHERE conname = 'users_company_id_fkey' AND conrelid = 'depotdirect.users'::regclass
  ) THEN
    ALTER TABLE depotdirect.users
      ADD CONSTRAINT users_company_id_fkey FOREIGN KEY (company_id) REFERENCES depotdirect.companies(id) ON DELETE SET NULL;
  END IF;
END$$;

-- Add index for faster lookups
CREATE INDEX IF NOT EXISTS idx_users_company_id ON depotdirect.users (company_id);

ALTER TABLE depotdirect.users OWNER TO depotdirect_user;

-- 2. Validation trigger for user_regions: ensure region belongs to user's company

-- Create or replace function
CREATE OR REPLACE FUNCTION depotdirect.fn_validate_user_region_company()
RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE
  u_company_id integer;
  exists_link boolean;
BEGIN
  -- For safety: if the table has no user_id/region_id, skip (defensive)
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema='depotdirect' AND table_name=TG_TABLE_NAME AND column_name='user_id'
  ) OR NOT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema='depotdirect' AND table_name=TG_TABLE_NAME AND column_name='region_id'
  ) THEN
    RETURN NEW;
  END IF;

  -- Only validate on INSERT or UPDATE
  IF TG_OP NOT IN ('INSERT','UPDATE') THEN
    RETURN NEW;
  END IF;

  -- obtain user's company
  SELECT company_id INTO u_company_id FROM depotdirect.users WHERE id = NEW.user_id;

  -- If user doesn't exist, reject
  IF u_company_id IS NULL THEN
    RAISE EXCEPTION 'User % has no company assigned. Assign a company before mapping regions.', NEW.user_id;
    -- Alternative: you may want to allow NULL company and skip validation; change behavior as needed.
  END IF;

  -- Check that the company is linked to that region via company_regions (non-deleted)
  SELECT EXISTS (
    SELECT 1 FROM depotdirect.company_regions cr
    WHERE cr.company_id = u_company_id
      AND cr.region_id = NEW.region_id
      AND (cr.deleted_at IS NULL)
  ) INTO exists_link;

  IF NOT exists_link THEN
    RAISE EXCEPTION 'Region % is not assigned to user''s company (company id %). Assign region to company first.', NEW.region_id, u_company_id;
  END IF;

  RETURN NEW;
END;
$$;
ALTER FUNCTION depotdirect.fn_validate_user_region_company() OWNER TO depotdirect_user;

-- Attach trigger to user_regions table if not already attached
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM pg_class WHERE relname = 'user_regions') THEN
    IF NOT EXISTS (
      SELECT 1 FROM pg_trigger WHERE tgname = 'trg_validate_user_regions_company'
    ) THEN
      CREATE TRIGGER trg_validate_user_regions_company
        BEFORE INSERT OR UPDATE
        ON depotdirect.user_regions
        FOR EACH ROW
        EXECUTE FUNCTION depotdirect.fn_validate_user_region_company();
    END IF;
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 3. Optionally add active column to user_regions for soft delete pattern (if you want soft-deletes)
-- (skip if you prefer physical deletes)
-- ALTER TABLE depotdirect.user_regions ADD COLUMN IF NOT EXISTS active boolean DEFAULT true;

-- 4. Optional: Backfill users.company_id from any existing assumptions.
-- NOTE: This is manual and depends on your data. Example: if you can infer company from user email domain or existing site/company mappings you have.
-- For manual backfill, run queries like:
-- UPDATE depotdirect.users SET company_id = <company_id> WHERE id IN (...);

-- 5. Grant ownership & index checks (defensive)
ALTER TABLE IF EXISTS depotdirect.user_regions OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS idx_user_regions_user ON depotdirect.user_regions (user_id);
CREATE INDEX IF NOT EXISTS idx_user_regions_region ON depotdirect.user_regions (region_id);






-- 4) sample: assign regions to a user (example)
-- Replace <user_id> and <region_id> with actual ids
-- INSERT INTO depotdirect.user_regions (user_id, region_id, created_by) VALUES (1, 2, 1) ON CONFLICT DO NOTHING;



-- =====================================================================
-- 8. Quick examples (copy-paste)
-- =====================================================================
/*
-- Add company-region membership:
INSERT INTO depotdirect.company_regions (company_id, region_id) VALUES (<company_id>, <region_id>);

-- Map a site to a region with optional override code:
INSERT INTO depotdirect.site_regions (site_id, region_id, site_code) VALUES (<site_id>, <region_id>, 'SITE-REG-001');

-- Map a site to a company with optional company-scoped code:
INSERT INTO depotdirect.site_companies (site_id, company_id, site_code) VALUES (<site_id>, <company_id>, 'SITE-COMP-001');

-- Query: site master + its regions
SELECT s.*, r.*
FROM depotdirect.sites s
JOIN depotdirect.site_regions sr ON sr.site_id = s.id
JOIN depotdirect.regions r ON r.id = sr.region_id
WHERE s.id = <site_id>;

-- Query: find a depot by country-scoped code
SELECT * FROM depotdirect.depots WHERE country_id = <country_id> AND depot_code = 'DPT-001';
*/

-- =====================================================================
-- End of V1__depotdirect_init.sql
-- =====================================================================
