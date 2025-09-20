-- Migration: user_regions mapping + validation using users.company_id -> companies.country_id
-- Safe / idempotent: won't overwrite existing objects
BEGIN;

-- 1) Create sequence & table for user_regions
CREATE SEQUENCE IF NOT EXISTS depotdirect.user_regions_id_seq START 1;

CREATE TABLE IF NOT EXISTS depotdirect.user_regions (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.user_regions_id_seq'),
  user_id integer NOT NULL REFERENCES depotdirect.users(id) ON DELETE CASCADE,
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE CASCADE,
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  UNIQUE (user_id, region_id)
);
ALTER TABLE depotdirect.user_regions OWNER TO depotdirect_user;

-- indexes
CREATE INDEX IF NOT EXISTS idx_user_regions_user ON depotdirect.user_regions (user_id);
CREATE INDEX IF NOT EXISTS idx_user_regions_region ON depotdirect.user_regions (region_id);

COMMIT;

-- 2) Create trigger function to validate region belongs to same country as user's company
CREATE OR REPLACE FUNCTION depotdirect.fn_validate_user_region_matches_company_country()
RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE
  reg_country integer;
  comp_country integer;
  comp_id integer;
BEGIN
  IF TG_OP NOT IN ('INSERT','UPDATE') THEN
    RETURN NEW;
  END IF;

  -- get region country
  SELECT country_id INTO reg_country FROM depotdirect.regions WHERE id = NEW.region_id;
  IF reg_country IS NULL THEN
    RAISE EXCEPTION 'region id % does not exist or has no country_id', NEW.region_id;
  END IF;

  -- get user's company_id
  SELECT company_id INTO comp_id FROM depotdirect.users WHERE id = NEW.user_id;
  IF comp_id IS NULL THEN
    RAISE EXCEPTION 'user id % has no company_id; assign a company before adding regions', NEW.user_id;
  END IF;

  -- get company's country_id
  SELECT country_id INTO comp_country FROM depotdirect.companies WHERE id = comp_id;
  IF comp_country IS NULL THEN
    RAISE EXCEPTION 'company id % does not exist or has no country_id', comp_id;
  END IF;

  IF reg_country <> comp_country THEN
    RAISE EXCEPTION 'cannot assign region (id=%, country=%) to user (id=%) who belongs to company (id=%, country=%)', NEW.region_id, reg_country, NEW.user_id, comp_id, comp_country;
  END IF;

  RETURN NEW;
END;
$$;
ALTER FUNCTION depotdirect.fn_validate_user_region_matches_company_country() OWNER TO depotdirect_user;

-- 3) Attach trigger if not present
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_trigger
    JOIN pg_class c ON tgrelid = c.oid
    JOIN pg_namespace n ON c.relnamespace = n.oid
    WHERE tgname = 'trg_validate_user_region_matches_company_country'
      AND n.nspname = 'depotdirect'
      AND c.relname = 'user_regions'
  ) THEN
    CREATE TRIGGER trg_validate_user_region_matches_company_country
      BEFORE INSERT OR UPDATE ON depotdirect.user_regions
      FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_user_region_matches_company_country();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 4) Attach updated_at trigger if fn_set_updated_at exists
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM pg_proc p JOIN pg_namespace n ON p.pronamespace = n.oid WHERE p.proname = 'fn_set_updated_at' AND n.nspname = 'depotdirect')
     AND NOT EXISTS (
       SELECT 1 FROM pg_trigger
       JOIN pg_class c ON tgrelid = c.oid
       JOIN pg_namespace n ON c.relnamespace = n.oid
       WHERE tgname = 'trg_set_updated_at_user_regions'
         AND n.nspname = 'depotdirect'
         AND c.relname = 'user_regions'
     ) THEN
    CREATE TRIGGER trg_set_updated_at_user_regions BEFORE UPDATE ON depotdirect.user_regions FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;
END;
$$ LANGUAGE plpgsql;

-- 5) Helpful example queries (no-op block for copy/paste)
-- List regions for a user:
-- SELECT r.* FROM depotdirect.regions r JOIN depotdirect.user_regions ur ON ur.region_id = r.id WHERE ur.user_id = <user_id>;

-- List users in a region:
-- SELECT u.* FROM depotdirect.users u JOIN depotdirect.user_regions ur ON ur.user_id = u.id WHERE ur.region_id = <region_id>;

-- Add membership (will fail if company.country_id != region.country_id):
-- INSERT INTO depotdirect.user_regions (user_id, region_id, created_by) VALUES (<user_id>, <region_id>, <admin_id>);

-- Remove membership:
-- DELETE FROM depotdirect.user_regions WHERE user_id = <user_id> AND region_id = <region_id>;

