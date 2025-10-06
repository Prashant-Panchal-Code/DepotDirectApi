-- =====================================================================
-- Add parkings master table + region_parkings mapping (idempotent)
-- Assumes schema depotdirect and helper functions exist:
--   depotdirect.fn_set_updated_at, depotdirect.fn_soft_delete, depotdirect.fn_validate_operating_hours (optional)
-- =====================================================================

SET search_path = public;

-- ensure schema exists
CREATE SCHEMA IF NOT EXISTS depotdirect AUTHORIZATION depotdirect_user;
SET search_path = depotdirect, public;

-- 1) sequence + parkings table
CREATE SEQUENCE IF NOT EXISTS depotdirect.parkings_id_seq START 1;

CREATE TABLE IF NOT EXISTS depotdirect.parkings (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.parkings_id_seq'),

  -- core fields
  parking_code text NOT NULL,       -- Code
  parking_name text NOT NULL,       -- Name
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

  -- manager / contacts
  manager_name text,
  manager_phone text,
  manager_email citext,
  emergency_contact text,

  parking_spaces integer,           -- number of parking spaces

  -- ownership / scope
  country_id integer NOT NULL REFERENCES depotdirect.countries(id) ON DELETE RESTRICT,
  company_id integer NOT NULL REFERENCES depotdirect.companies(id) ON DELETE RESTRICT,

  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,

  CONSTRAINT parkings_country_code_uniq UNIQUE (country_id, parking_code)
);
ALTER TABLE depotdirect.parkings OWNER TO depotdirect_user;

-- indexes
CREATE INDEX IF NOT EXISTS idx_parkings_parking_code ON depotdirect.parkings (parking_code);
CREATE INDEX IF NOT EXISTS idx_parkings_country ON depotdirect.parkings (country_id);
CREATE INDEX IF NOT EXISTS idx_parkings_town ON depotdirect.parkings (town);
CREATE INDEX IF NOT EXISTS idx_parkings_company ON depotdirect.parkings (company_id);
CREATE INDEX IF NOT EXISTS idx_parkings_shortcode ON depotdirect.parkings (shortcode);

-- 2) Attach triggers: updated_at + soft-delete
DO $$
BEGIN
  -- updated_at
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_parkings') THEN
    CREATE TRIGGER trg_set_updated_at_parkings BEFORE UPDATE ON depotdirect.parkings FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;

  -- soft-delete (function handles absence of 'active' column)
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_parkings') THEN
    CREATE TRIGGER trg_soft_delete_parkings BEFORE DELETE ON depotdirect.parkings FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 3) region_parkings mapping table (parking -> region)
CREATE SEQUENCE IF NOT EXISTS depotdirect.region_parkings_id_seq START 1;

CREATE TABLE IF NOT EXISTS depotdirect.region_parkings (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.region_parkings_id_seq'),
  parking_id integer NOT NULL REFERENCES depotdirect.parkings(id) ON DELETE CASCADE,
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE CASCADE,
  parking_code text, -- optional region-specific override
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,
  UNIQUE (parking_id, region_id)
);
ALTER TABLE depotdirect.region_parkings OWNER TO depotdirect_user;

-- indexes
CREATE INDEX IF NOT EXISTS idx_region_parkings_parking ON depotdirect.region_parkings (parking_id);
CREATE INDEX IF NOT EXISTS idx_region_parkings_region ON depotdirect.region_parkings (region_id);

-- 4) triggers for region_parkings: updated_at + soft-delete
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_region_parkings') THEN
    CREATE TRIGGER trg_set_updated_at_region_parkings BEFORE UPDATE ON depotdirect.region_parkings FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_region_parkings') THEN
    CREATE TRIGGER trg_soft_delete_region_parkings BEFORE DELETE ON depotdirect.region_parkings FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 5) Validation function: ensure region.company_id == parking.company_id
--    Prevents mapping a parking into a region belonging to a different company
CREATE OR REPLACE FUNCTION depotdirect.fn_validate_region_parking_company()
RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE
  parking_company integer;
  region_company integer;
BEGIN
  -- defensive: check columns exist
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema='depotdirect' AND table_name=TG_TABLE_NAME AND column_name='parking_id'
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

  SELECT company_id INTO parking_company FROM depotdirect.parkings WHERE id = NEW.parking_id;
  SELECT company_id INTO region_company FROM depotdirect.regions WHERE id = NEW.region_id;

  IF parking_company IS NULL THEN
    RAISE EXCEPTION 'Parking % has no company_id assigned; assign company_id before mapping to a region.', NEW.parking_id;
  END IF;

  IF region_company IS NULL THEN
    RAISE EXCEPTION 'Region % has no company_id assigned; assign company_id for the region before mapping.', NEW.region_id;
  END IF;

  IF parking_company <> region_company THEN
    RAISE EXCEPTION 'Region (company_id=%) does not belong to same company as Parking (company_id=%). Operation denied.', region_company, parking_company;
  END IF;

  RETURN NEW;
END;
$$;
ALTER FUNCTION depotdirect.fn_validate_region_parking_company() OWNER TO depotdirect_user;

-- Attach validator trigger to region_parkings if not already attached
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM pg_class WHERE relname = 'region_parkings') THEN
    IF NOT EXISTS (
      SELECT 1 FROM pg_trigger WHERE tgname = 'trg_validate_region_parking_company'
    ) THEN
      CREATE TRIGGER trg_validate_region_parking_company
        BEFORE INSERT OR UPDATE
        ON depotdirect.region_parkings
        FOR EACH ROW
        EXECUTE FUNCTION depotdirect.fn_validate_region_parking_company();
    END IF;
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 6) ownership of new objects
ALTER TABLE IF EXISTS depotdirect.parkings OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.region_parkings OWNER TO depotdirect_user;

ALTER SEQUENCE IF EXISTS depotdirect.parkings_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.region_parkings_id_seq OWNER TO depotdirect_user;

-- ensure search_path set back
SET search_path = depotdirect, public;

-- =====================================================================
-- End of parkings + region_parkings addition
-- =====================================================================
