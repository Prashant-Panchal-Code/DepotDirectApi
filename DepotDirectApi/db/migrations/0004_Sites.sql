-- =====================================================================
-- Add sites master table + region_sites mapping
-- Idempotent; requires schema depotdirect and functions:
--   depotdirect.fn_set_updated_at()
--   depotdirect.fn_validate_operating_hours()
--   depotdirect.fn_soft_delete()
-- If those don't exist, create them first (your previous script has them).
-- =====================================================================

SET search_path = public;

-- ensure schema exists
CREATE SCHEMA IF NOT EXISTS depotdirect AUTHORIZATION depotdirect_user;
SET search_path = depotdirect, public;

-- 1) sequence + sites table
CREATE SEQUENCE IF NOT EXISTS depotdirect.sites_id_seq START 1;

CREATE TABLE IF NOT EXISTS depotdirect.sites (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.sites_id_seq'),

  -- requested fields (normalized names)
  site_code text NOT NULL,           -- Code
  site_name text NOT NULL,           -- Name
  shortcode text,                    -- shortcode (short human-friendly code)
  latitude numeric(10,7),
  longitude numeric(10,7),
  latlong text GENERATED ALWAYS AS (
    CASE WHEN latitude IS NOT NULL AND longitude IS NOT NULL THEN (latitude::text || ',' || longitude::text) ELSE NULL END
  ) STORED,

  street text,
  postal_code text,                  -- postalCode
  town text,

  active boolean NOT NULL DEFAULT true,
  priority text NOT NULL DEFAULT 'Medium', -- HIGH, Medium, Low

  -- additional details
  contact_person text,
  phone text,
  email citext,

  operating_hours jsonb DEFAULT '{}'::jsonb, -- { mon: { open, close, closed }, ... }

  depot_id integer,                   -- optional, FK added below if depots exist
  delivery_stopped boolean NOT NULL DEFAULT false,
  pumped_required boolean NOT NULL DEFAULT false,

  country_id integer NOT NULL REFERENCES depotdirect.countries(id) ON DELETE RESTRICT,
  company_id integer NOT NULL REFERENCES depotdirect.companies(id) ON DELETE RESTRICT,

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

-- indexes
CREATE INDEX IF NOT EXISTS idx_sites_site_code ON depotdirect.sites (site_code);
CREATE INDEX IF NOT EXISTS idx_sites_country ON depotdirect.sites (country_id);
CREATE INDEX IF NOT EXISTS idx_sites_town ON depotdirect.sites (town);
CREATE INDEX IF NOT EXISTS idx_sites_company ON depotdirect.sites (company_id);
CREATE INDEX IF NOT EXISTS idx_sites_shortcode ON depotdirect.sites (shortcode);

-- 2) Add depot FK only if depots table exists (safe idempotent)
DO $$
BEGIN
  IF EXISTS (
    SELECT 1 FROM information_schema.tables
    WHERE table_schema = 'depotdirect' AND table_name = 'depots'
  ) THEN
    -- add foreign key constraint if not exists
    IF NOT EXISTS (
      SELECT 1 FROM pg_constraint
      WHERE conname = 'sites_depot_id_fkey' AND conrelid = 'depotdirect.sites'::regclass
    ) THEN
      ALTER TABLE depotdirect.sites
        ADD CONSTRAINT sites_depot_id_fkey FOREIGN KEY (depot_id) REFERENCES depotdirect.depots(id) ON DELETE SET NULL;
    END IF;
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 3) Attach triggers: updated_at + operating_hours validator + soft-delete
DO $$
BEGIN
  -- updated_at
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_sites') THEN
    CREATE TRIGGER trg_set_updated_at_sites BEFORE UPDATE ON depotdirect.sites FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;

  -- operating_hours validator (only attach if column exists)
  IF EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema='depotdirect' AND table_name='sites' AND column_name='operating_hours'
  ) THEN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_validate_operating_hours_sites') THEN
      CREATE TRIGGER trg_validate_operating_hours_sites BEFORE INSERT OR UPDATE ON depotdirect.sites FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_operating_hours();
    END IF;
  END IF;

  -- soft-delete (only applied if 'active' exists, function handles missing column)
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_sites') THEN
    CREATE TRIGGER trg_soft_delete_sites BEFORE DELETE ON depotdirect.sites FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 4) region_sites mapping table (site -> region)
CREATE SEQUENCE IF NOT EXISTS depotdirect.region_sites_id_seq START 1;

CREATE TABLE IF NOT EXISTS depotdirect.region_sites (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.region_sites_id_seq'),
  site_id integer NOT NULL REFERENCES depotdirect.sites(id) ON DELETE CASCADE,
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE CASCADE,
  site_code text, -- optional override for this region mapping
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,
  UNIQUE (site_id, region_id)
);
ALTER TABLE depotdirect.region_sites OWNER TO depotdirect_user;

-- indexes
CREATE INDEX IF NOT EXISTS idx_region_sites_site ON depotdirect.region_sites (site_id);
CREATE INDEX IF NOT EXISTS idx_region_sites_region ON depotdirect.region_sites (region_id);

-- 5) triggers for region_sites: updated_at + soft-delete
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_region_sites') THEN
    CREATE TRIGGER trg_set_updated_at_region_sites BEFORE UPDATE ON depotdirect.region_sites FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_region_sites') THEN
    CREATE TRIGGER trg_soft_delete_region_sites BEFORE DELETE ON depotdirect.region_sites FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 6) Validation function: ensure region.company_id == site.company_id
--    (prevents mapping a site into a region belonging to a different company)
CREATE OR REPLACE FUNCTION depotdirect.fn_validate_region_site_company()
RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE
  site_company integer;
  region_company integer;
BEGIN
  -- defensive: check columns exist
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema='depotdirect' AND table_name=TG_TABLE_NAME AND column_name='site_id'
  ) OR NOT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema='depotdirect' AND table_name=TG_TABLE_NAME AND column_name='region_id'
  ) THEN
    RETURN NEW;
  END IF;

  -- Only on INSERT/UPDATE
  IF TG_OP NOT IN ('INSERT','UPDATE') THEN
    RETURN NEW;
  END IF;

  SELECT company_id INTO site_company FROM depotdirect.sites WHERE id = NEW.site_id;
  SELECT company_id INTO region_company FROM depotdirect.regions WHERE id = NEW.region_id;

  IF site_company IS NULL THEN
    RAISE EXCEPTION 'Site % has no company_id assigned; assign company_id before mapping to a region.', NEW.site_id;
  END IF;

  IF region_company IS NULL THEN
    RAISE EXCEPTION 'Region % has no company_id assigned; assign company_id for the region before mapping.', NEW.region_id;
  END IF;

  IF site_company <> region_company THEN
    RAISE EXCEPTION 'Region (company_id=%) does not belong to same company as Site (company_id=%). Operation denied.', region_company, site_company;
  END IF;

  RETURN NEW;
END;
$$;
ALTER FUNCTION depotdirect.fn_validate_region_site_company() OWNER TO depotdirect_user;

-- Attach validator trigger to region_sites if not already attached
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM pg_class WHERE relname = 'region_sites') THEN
    IF NOT EXISTS (
      SELECT 1 FROM pg_trigger WHERE tgname = 'trg_validate_region_site_company'
    ) THEN
      CREATE TRIGGER trg_validate_region_site_company
        BEFORE INSERT OR UPDATE
        ON depotdirect.region_sites
        FOR EACH ROW
        EXECUTE FUNCTION depotdirect.fn_validate_region_site_company();
    END IF;
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 7) ownership of new objects
ALTER TABLE IF EXISTS depotdirect.sites OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.region_sites OWNER TO depotdirect_user;

ALTER SEQUENCE IF EXISTS depotdirect.sites_id_seq OWNER TO depotdirect_user;
ALTER SEQUENCE IF EXISTS depotdirect.region_sites_id_seq OWNER TO depotdirect_user;

-- 8) final search_path safety
SET search_path = depotdirect, public;

-- =====================================================================
-- End of sites + region_sites addition
-- =====================================================================
