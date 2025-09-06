-- Migration: depotdirect master-data schema and tables (lowercase identifiers)
-- Run as a superuser or a role that can CREATE SCHEMA / CREATE TABLE
-- Assumes role "depotdirect_user" exists.

-- 0) Safety: set search path explicitly for this script
SET search_path = public;

-- 1) Create schema
CREATE SCHEMA IF NOT EXISTS depotdirect AUTHORIZATION depotdirect_user;
COMMENT ON SCHEMA depotdirect IS 'Master data for DepotDirect application (sites, parkings, depots, regions, companies, countries)';

SET search_path = depotdirect, public;

-- 2) Trigger function to keep updated_at fresh
CREATE OR REPLACE FUNCTION depotdirect.fn_set_updated_at()
RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
  NEW.updated_at = now();
  RETURN NEW;
END;
$$;

-- 3) Sequences and tables (order matters for FKs)

-- Countries
CREATE SEQUENCE IF NOT EXISTS depotdirect.countries_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.countries (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.countries_id_seq'),
  name text NOT NULL,
  iso_code varchar(8),
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);
ALTER TABLE depotdirect.countries OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS countries_name_idx ON depotdirect.countries (name);

-- Companies
CREATE SEQUENCE IF NOT EXISTS depotdirect.companies_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.companies (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.companies_id_seq'),
  name text NOT NULL,
  company_code text UNIQUE,
  description text,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);
ALTER TABLE depotdirect.companies OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS companies_name_idx ON depotdirect.companies (name);

-- Regions
CREATE SEQUENCE IF NOT EXISTS depotdirect.regions_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.regions (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.regions_id_seq'),
  name text NOT NULL,
  region_code text,
  country_id integer REFERENCES depotdirect.countries(id) ON DELETE SET NULL,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);
ALTER TABLE depotdirect.regions OWNER TO depotdirect_user;
CREATE INDEX IF NOT EXISTS regions_name_idx ON depotdirect.regions (name);
CREATE INDEX IF NOT EXISTS regions_country_idx ON depotdirect.regions (country_id);

-- Depots (created before sites because sites references depot_id)
CREATE SEQUENCE IF NOT EXISTS depotdirect.depots_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.depots (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.depots_id_seq'),
  depot_code text NOT NULL UNIQUE,
  depot_name text NOT NULL,
  latitude numeric(10,7),
  longitude numeric(10,7),
  latlong text GENERATED ALWAYS AS (
    CASE
      WHEN latitude IS NOT NULL AND longitude IS NOT NULL THEN (latitude::text || ',' || longitude::text)
      ELSE NULL
    END
  ) STORED,
  street text,
  postal_code text,
  town text,
  active boolean NOT NULL DEFAULT true,
  priority text NOT NULL DEFAULT 'Medium',
  is_parking boolean NOT NULL DEFAULT false,
  manager_name text,
  manager_phone text,
  manager_email text,
  emergency_contact text,
  loading_bays integer,
  average_loading_time integer, -- minutes
  max_truck_size text,
  certifications text,
  operating_hours jsonb DEFAULT '{}'::jsonb,
  company_id integer NOT NULL REFERENCES depotdirect.companies(id) ON DELETE SET NULL,
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE SET NULL,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);
ALTER TABLE depotdirect.depots OWNER TO depotdirect_user;
ALTER TABLE depotdirect.depots ADD CONSTRAINT depots_priority_chk CHECK (priority IN ('High','Medium','Low'));
CREATE INDEX IF NOT EXISTS depots_company_idx ON depotdirect.depots (company_id);
CREATE INDEX IF NOT EXISTS depots_region_idx ON depotdirect.depots (region_id);
CREATE INDEX IF NOT EXISTS depots_town_idx ON depotdirect.depots (town);
CREATE INDEX IF NOT EXISTS depots_postalcode_idx ON depotdirect.depots (postal_code);

-- Sites
CREATE SEQUENCE IF NOT EXISTS depotdirect.sites_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.sites (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.sites_id_seq'),
  site_code text NOT NULL UNIQUE,
  site_name text NOT NULL,
  latitude numeric(10,7),
  longitude numeric(10,7),
  latlong text GENERATED ALWAYS AS (
    CASE 
      WHEN latitude IS NOT NULL AND longitude IS NOT NULL THEN (latitude::text || ',' || longitude::text)
      ELSE NULL
    END
  ) STORED,
  street text,
  postal_code text,
  town text,
  active boolean NOT NULL DEFAULT true,
  priority text NOT NULL DEFAULT 'Medium',
  depot_id integer REFERENCES depotdirect.depots(id) ON DELETE SET NULL,
  contact_person text,
  phone text,
  email text,
  operating_hours jsonb DEFAULT '{}'::jsonb,
  delivery_stopped boolean NOT NULL DEFAULT false,
  pumped_required boolean NOT NULL DEFAULT false,
  company_id integer NOT NULL REFERENCES depotdirect.companies(id) ON DELETE SET NULL,
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE SET NULL,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);
ALTER TABLE depotdirect.sites OWNER TO depotdirect_user;
ALTER TABLE depotdirect.sites ADD CONSTRAINT sites_priority_chk CHECK (priority IN ('High','Medium','Low'));
CREATE INDEX IF NOT EXISTS sites_company_idx ON depotdirect.sites (company_id);
CREATE INDEX IF NOT EXISTS sites_region_idx ON depotdirect.sites (region_id);
CREATE INDEX IF NOT EXISTS sites_town_idx ON depotdirect.sites (town);
CREATE INDEX IF NOT EXISTS sites_postalcode_idx ON depotdirect.sites (postal_code);

-- Parkings
CREATE SEQUENCE IF NOT EXISTS depotdirect.parkings_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.parkings (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.parkings_id_seq'),
  parking_code text NOT NULL UNIQUE,
  parking_name text NOT NULL,
  latitude numeric(10,7),
  longitude numeric(10,7),
  latlong text GENERATED ALWAYS AS (
    CASE
      WHEN latitude IS NOT NULL AND longitude IS NOT NULL THEN (latitude::text || ',' || longitude::text)
      ELSE NULL
    END
  ) STORED,
  street text,
  postal_code text,
  town text,
  active boolean NOT NULL DEFAULT true,
  priority text NOT NULL DEFAULT 'Medium',
  is_depot boolean NOT NULL DEFAULT false,
  manager_name text,
  manager_phone text,
  manager_email text,
  emergency_contact text,
  parking_spaces integer,
  company_id integer NOT NULL REFERENCES depotdirect.companies(id) ON DELETE SET NULL,
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE SET NULL,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);
ALTER TABLE depotdirect.parkings OWNER TO depotdirect_user;
ALTER TABLE depotdirect.parkings ADD CONSTRAINT parkings_priority_chk CHECK (priority IN ('High','Medium','Low'));
CREATE INDEX IF NOT EXISTS parkings_company_idx ON depotdirect.parkings (company_id);
CREATE INDEX IF NOT EXISTS parkings_region_idx ON depotdirect.parkings (region_id);
CREATE INDEX IF NOT EXISTS parkings_town_idx ON depotdirect.parkings (town);
CREATE INDEX IF NOT EXISTS parkings_postalcode_idx ON depotdirect.parkings (postal_code);

-- 4) Attach triggers to auto-update updated_at on each table
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_countries') THEN
    CREATE TRIGGER trg_set_updated_at_countries
    BEFORE UPDATE ON depotdirect.countries
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_companies') THEN
    CREATE TRIGGER trg_set_updated_at_companies
    BEFORE UPDATE ON depotdirect.companies
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_regions') THEN
    CREATE TRIGGER trg_set_updated_at_regions
    BEFORE UPDATE ON depotdirect.regions
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_depots') THEN
    CREATE TRIGGER trg_set_updated_at_depots
    BEFORE UPDATE ON depotdirect.depots
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_sites') THEN
    CREATE TRIGGER trg_set_updated_at_sites
    BEFORE UPDATE ON depotdirect.sites
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_parkings') THEN
    CREATE TRIGGER trg_set_updated_at_parkings
    BEFORE UPDATE ON depotdirect.parkings
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 5) Optional: PostGIS alternative (commented)
-- Uncomment and run if you want spatial geometry and indices (requires superuser to create extension)
-- /*
-- CREATE EXTENSION IF NOT EXISTS postgis;
-- ALTER TABLE depotdirect.sites ADD COLUMN IF NOT EXISTS geom geometry(Point,4326);
-- UPDATE depotdirect.sites SET geom = ST_SetSRID(ST_Point(longitude::double precision, latitude::double precision),4326)
--   WHERE latitude IS NOT NULL AND longitude IS NOT NULL;
-- CREATE INDEX IF NOT EXISTS sites_geom_gist ON depotdirect.sites USING GIST (geom);
-- -- repeat for depots and parkings
-- */

-- 6) Sample INSERTs for smoke test (idempotent where possible)
INSERT INTO depotdirect.countries (name, iso_code, created_by)
VALUES ('India','IN', NULL)
ON CONFLICT DO NOTHING;

INSERT INTO depotdirect.companies (name, company_code, created_by)
VALUES ('Acme Logistics','ACME', NULL)
ON CONFLICT DO NOTHING;

-- Create region linked to country
WITH c AS (SELECT id FROM depotdirect.countries LIMIT 1),
     comp AS (SELECT id FROM depotdirect.companies LIMIT 1)
INSERT INTO depotdirect.regions (name, region_code, country_id, created_by)
SELECT 'Karnataka','KA', c.id, NULL FROM c
ON CONFLICT DO NOTHING;

-- Insert a depot (needs company & region)
WITH comp AS (SELECT id FROM depotdirect.companies LIMIT 1),
     reg AS (SELECT id FROM depotdirect.regions LIMIT 1)
INSERT INTO depotdirect.depots (
  depot_code, depot_name, latitude, longitude, street, postal_code, town,
  active, priority, is_parking, loading_bays, average_loading_time, max_truck_size, operating_hours, company_id, region_id, created_by
)
SELECT
  'dpt-001', 'acme depot bengaluru', 12.9715987, 77.5945627, '1 Sample St', '560001', 'Bengaluru',
  true, 'High', false, 4, 60, 'large',
  '{"mon":{"open":"09:00","close":"18:00"},"sat":{"closed":true}}'::jsonb,
  comp.id, reg.id, NULL
FROM comp, reg
ON CONFLICT DO NOTHING;

-- Insert a site (references depot & comp/reg)
WITH comp AS (SELECT id FROM depotdirect.companies LIMIT 1),
     reg AS (SELECT id FROM depotdirect.regions LIMIT 1),
     dp AS (SELECT id FROM depotdirect.depots WHERE depot_code='dpt-001' LIMIT 1)
INSERT INTO depotdirect.sites (
  site_code, site_name, latitude, longitude, street, postal_code, town, active,
  priority, depot_id, contact_person, phone, email, operating_hours, company_id, region_id, created_by
)
SELECT
  'site-001','sample site near depot', 12.9720, 77.5950, '2 Sample Lane', '560001', 'Bengaluru', true,
  'Medium', dp.id, 'Ravi Kumar', '+91-9876543210','ravi@example.com',
  '{"mon":{"open":"08:00","close":"17:00"},"sun":{"closed":true}}'::jsonb,
  comp.id, reg.id, NULL
FROM comp, reg, dp
ON CONFLICT DO NOTHING;

-- Insert parking
WITH comp AS (SELECT id FROM depotdirect.companies LIMIT 1),
     reg AS (SELECT id FROM depotdirect.regions LIMIT 1)
INSERT INTO depotdirect.parkings (
  parking_code, parking_name, latitude, longitude, street, postal_code, town, active, priority,
  is_depot, manager_name, manager_phone, parking_spaces, company_id, region_id, created_by
)
SELECT
  'pkg-001','acme west parking', 12.9705, 77.5900, '3 Parking Rd', '560002', 'Bengaluru', true, 'Low',
  false, 'Suresh', '+91-9998887777', 25, comp.id, reg.id, NULL
FROM comp, reg
ON CONFLICT DO NOTHING;

-- 7) Ownership (ensure all objects owned by depotdirect_user)
ALTER TABLE depotdirect.countries OWNER TO depotdirect_user;
ALTER TABLE depotdirect.companies OWNER TO depotdirect_user;
ALTER TABLE depotdirect.regions OWNER TO depotdirect_user;
ALTER TABLE depotdirect.depots OWNER TO depotdirect_user;
ALTER TABLE depotdirect.sites OWNER TO depotdirect_user;
ALTER TABLE depotdirect.parkings OWNER TO depotdirect_user;
ALTER FUNCTION depotdirect.fn_set_updated_at() OWNER TO depotdirect_user;

-- 8) Helpful comments
-- If you later wish created_by/last_updated_by to reference a users table (for example public.users),
-- run AFTER the users table exists:
-- ALTER TABLE depotdirect.sites ADD CONSTRAINT sites_created_by_fk FOREIGN KEY (created_by) REFERENCES public.users(id) ON DELETE SET NULL;
-- Repeat for last_updated_by and other tables as needed.

-- Done.


-- Migration: add many-to-many relation between companies and regions
-- Run in the same database where depotdirect schema exists.

SET search_path = depotdirect, public;

-- Create sequence for company_regions
CREATE SEQUENCE IF NOT EXISTS depotdirect.company_regions_id_seq START 1;

-- Create join table (idempotent)
CREATE TABLE IF NOT EXISTS depotdirect.company_regions (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.company_regions_id_seq'),
  company_id integer NOT NULL REFERENCES depotdirect.companies(id) ON DELETE CASCADE,
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE CASCADE,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now()
);

-- Prevent duplicates
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint
    WHERE conname = 'company_regions_company_region_uniq'
  ) THEN
    ALTER TABLE depotdirect.company_regions
      ADD CONSTRAINT company_regions_company_region_uniq UNIQUE (company_id, region_id);
  END IF;
END;
$$;

-- Indexes for lookups
CREATE INDEX IF NOT EXISTS company_regions_company_idx ON depotdirect.company_regions (company_id);
CREATE INDEX IF NOT EXISTS company_regions_region_idx ON depotdirect.company_regions (region_id);

-- Trigger to auto-update updated_at (re-uses existing fn_set_updated_at)
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger t
    JOIN pg_class c ON t.tgrelid = c.oid
    WHERE t.tgname = 'trg_set_updated_at_company_regions' AND c.relname = 'company_regions'
  ) THEN
    CREATE TRIGGER trg_set_updated_at_company_regions
      BEFORE UPDATE ON depotdirect.company_regions
      FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
END;
$$;

-- Ensure ownership
ALTER TABLE depotdirect.company_regions OWNER TO depotdirect_user;
ALTER SEQUENCE depotdirect.company_regions_id_seq OWNER TO depotdirect_user;


SET search_path = depotdirect, public;

DO $$
DECLARE
  tbl text;
  col text;
  conname text;
BEGIN
  -- helper to find and drop single-column unique constraint for given table & column
  FOR tbl, col IN
    SELECT * FROM (VALUES
      ('sites','site_code'),
      ('depots','depot_code'),
      ('parkings','parking_code')
    ) AS t(tbl, col)
  LOOP
    SELECT c.conname
    INTO conname
    FROM pg_constraint c
    JOIN pg_class cl ON c.conrelid = cl.oid
    JOIN pg_namespace n ON cl.relnamespace = n.oid
    WHERE n.nspname = 'depotdirect'
      AND cl.relname = tbl
      AND c.contype = 'u'
      AND array_length(c.conkey,1) = 1
      AND (SELECT attname FROM pg_attribute WHERE attrelid = cl.oid AND attnum = c.conkey[1]) = col
    LIMIT 1;

    IF conname IS NOT NULL THEN
      EXECUTE format('ALTER TABLE depotdirect.%I DROP CONSTRAINT %I', tbl, conname);
    END IF;
  END LOOP;
END;
$$ LANGUAGE plpgsql;



SET search_path = depotdirect, public;

-- sites: UNIQUE(region_id, site_code)
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'sites_region_sitecode_uniq'
  ) THEN
    ALTER TABLE depotdirect.sites
      ADD CONSTRAINT sites_region_sitecode_uniq UNIQUE (region_id, site_code);
  END IF;
END;
$$ LANGUAGE plpgsql;

-- depots: UNIQUE(region_id, depot_code)
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'depots_region_depotcode_uniq'
  ) THEN
    ALTER TABLE depotdirect.depots
      ADD CONSTRAINT depots_region_depotcode_uniq UNIQUE (region_id, depot_code);
  END IF;
END;
$$ LANGUAGE plpgsql;

-- parkings: UNIQUE(region_id, parking_code)
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'parkings_region_parkingcode_uniq'
  ) THEN
    ALTER TABLE depotdirect.parkings
      ADD CONSTRAINT parkings_region_parkingcode_uniq UNIQUE (region_id, parking_code);
  END IF;
END;
$$ LANGUAGE plpgsql;



