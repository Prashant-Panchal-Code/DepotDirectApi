-- =====================================================================
-- Add depots master table + region_depots mapping (idempotent)
-- Schema: depotdirect (assumes depotdirect.fn_set_updated_at, fn_soft_delete, fn_validate_operating_hours exist)
-- =====================================================================

SET search_path = public;

-- ensure schema exists
CREATE SCHEMA IF NOT EXISTS depotdirect AUTHORIZATION depotdirect_user;
SET search_path = depotdirect, public;

-- 1) sequence + depots table
CREATE SEQUENCE IF NOT EXISTS depotdirect.depots_id_seq START 1;

CREATE TABLE IF NOT EXISTS depotdirect.depots (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.depots_id_seq'),

  -- core fields (normalized snake_case)
  depot_code text NOT NULL,         -- Code
  depot_name text NOT NULL,         -- Name
  shortcode text,                   -- shortcode
  latitude numeric(10,7),
  longitude numeric(10,7),
  latlong text GENERATED ALWAYS AS (
    CASE WHEN latitude IS NOT NULL AND longitude IS NOT NULL THEN (latitude::text || ',' || longitude::text) ELSE NULL END
  ) STORED,

  street text,
  postal_code text,                 -- postalCode
  town text,

  active boolean NOT NULL DEFAULT true,
  priority text NOT NULL DEFAULT 'Medium', -- High, Medium, Low

  loading_bays integer,
  operating_hours jsonb DEFAULT '{}'::jsonb, -- { mon: { open, close, closed }, ... }

  manager_name text,
  manager_phone text,
  manager_email citext,
  emergency_contact text,
  average_loading_time integer,
  max_truck_size text,
  certifications text,

  -- ownership / scope
  country_id integer NOT NULL REFERENCES depotdirect.countries(id) ON DELETE RESTRICT,
  company_id integer NOT NULL REFERENCES depotdirect.companies(id) ON DELETE RESTRICT,

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

-- indexes
CREATE INDEX IF NOT EXISTS idx_depots_depot_code ON depotdirect.depots (depot_code);
CREATE INDEX IF NOT EXISTS idx_depots_country ON depotdirect.depots (country_id);
CREATE INDEX IF NOT EXISTS idx_depots_town ON depotdirect.depots (town);
CREATE INDEX IF NOT EXISTS idx_depots_company ON depotdirect.depots (company_id);
CREATE INDEX IF NOT EXISTS idx_depots_shortcode ON depotdirect.depots (shortcode);

-- 2) Attach triggers: updated_at + operating_hours validator + soft-delete
DO $$
BEGIN
  -- updated_at
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_depots') THEN
    CREATE TRIGGER trg_set_updated_at_depots BEFORE UPDATE ON depotdirect.depots FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;

  -- operating_hours validator (only attach if column exists)
  IF EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema='depotdirect' AND table_name='depots' AND column_name='operating_hours'
  ) THEN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_validate_operating_hours_depots') THEN
      CREATE TRIGGER trg_validate_operating_hours_depots BEFORE INSERT OR UPDATE ON depotdirect.depots FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_operating_hours();
    END IF;
  END IF;

  -- soft-delete (function handles absence of 'active' column)
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_depots') THEN
    CREATE TRIGGER trg_soft_delete_depots BEFORE DELETE ON depotdirect.depots FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 3) region_depots mapping table (depot -> region)
CREATE SEQUENCE IF NOT EXISTS depotdirect.region_depots_id_seq START 1;

CREATE TABLE IF NOT EXISTS depotdirect.region_depots (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.region_depots_id_seq'),
  depot_id integer NOT NULL REFERENCES depotdirect.depots(id) ON DELETE CASCADE,
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE CASCADE,
  depot_code text, -- optional region-specific override
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,
  UNIQUE (depot_id, region_id)
);
ALTER TABLE depotdirect.region_depots OWNER TO depotdirect_user;

-- indexes
CREATE INDEX IF NOT EXISTS idx_region_depots_depot ON depotdirect.region_depots (depot_id);
CREATE INDEX IF NOT EXISTS idx_region_depots_region ON depotdirect.region_depots (region_id);

-- 4) triggers for region_depots: updated_at + soft-delete
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_region_depots') THEN
    CREATE TRIGGER trg_set_updated_at_region_depots BEFORE UPDATE ON depotdirect.region_depots FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_region_depots') THEN
    CREATE TRIGGER trg_soft_delete_region_depots BEFORE DELETE ON depotdirect.region_depots FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 5) Validation function: ensure region.company_id == depot.company_id
--    Prevents mapping a depot into a region belonging to a different company
CREATE OR REPLACE FUNCTION depotdirect.fn_validate_region_depot_company()
RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE
  depot_company integer;
  region_company integer;
BEGIN
  -- defensive: check columns exist
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema='depotdirect' AND table_name=TG_TABLE_NAME AND column_name='depot_id'
  ) OR NOT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema='depotdirect' AND table_name=TG_TABLE_NAME AND column_name='region_id'
  ) THEN
    RETURN NEW;
  END IF;

  -- Only validate on INSERT/UPDATE
  IF TG_OP NOT IN ('INSERT','UPDATE') THEN
    RETURN NEW;
  END IF;

  SELECT company_id INTO depot_company FROM depotdirect.depots WHERE id = NEW.depot_id;
  SELECT company_id INTO region_company FROM depotdirect.regions WHERE id = NEW.region_id;

  IF depot_company IS NULL THEN
    RAISE EXCEPTION 'Depot % has no company_id assigned; assign company_id before mapping to a region.', NEW.depot_id;
  END IF;

  IF region_company IS NULL THEN
    RAISE EXCEPTION 'Region % has no company_id assigned; assign company_id for the region before mapping.', NEW.region_id;
  END IF;

  IF depot_company <> region_company THEN
    RAISE EXCEPTION 'Region (company_id=%) does not belong to same company as Depot (company_id=%). Operation denied.', region_company, depot_company;
  END IF;

  RETURN NEW;
END;
$$;
ALTER FUNCTION depotdirect.fn_validate_region_depot_company() OWNER TO depotdirect_user;

-- Attach validator trigger to region_depots if not already attached
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM pg_class WHERE relname = 'region_depots') THEN
    IF NOT EXISTS (
      SELECT 1 FROM pg_trigger WHERE tgname = 'trg_validate_region_depot_company'
    ) THEN
      CREATE TRIGGER trg_validate_region_depot_company
        BEFORE INSERT OR UPDATE
        ON depotdirect.region_depots
        FOR EACH ROW
        EXECUTE FUNCTION depotdirect.fn_validate_region_depot_company();
    END IF;
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 6) ownership of new objects
ALTER TABLE IF EXISTS depotdirect.depots OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.region_depots OWNER TO depotdirect_user;

ALTER SEQUENCE IF EXISTS depotdirect.depots_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.region_depots_id_seq OWNER TO depotdirect_user;

-- ensure search_path set back
SET search_path = depotdirect, public;

-- =====================================================================
-- End of depots + region_depots addition
-- =====================================================================
