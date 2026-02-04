-- =====================================================================
-- 0013_Fix_Asset_Ownership.sql
-- Adds Region context to vehicles for filtering and RLS
-- =====================================================================

SET search_path = depotdirect, public;

-- 1. Update Tractors
-- Adding region allows us to say "This is a Northern Region Truck"
ALTER TABLE depotdirect.tractors
ADD COLUMN region_id integer REFERENCES depotdirect.regions(id);

-- Index for filtering "Show me all trucks in Region X"
CREATE INDEX IF NOT EXISTS idx_tractors_region ON depotdirect.tractors(region_id);


-- 2. Update Trailers
ALTER TABLE depotdirect.trailers
ADD COLUMN region_id integer REFERENCES depotdirect.regions(id);

CREATE INDEX IF NOT EXISTS idx_trailers_region ON depotdirect.trailers(region_id);


-- 3. Update Drivers
-- We already have 'home_depot_id', but direct region access is faster for permissions
ALTER TABLE depotdirect.drivers
ADD COLUMN region_id integer REFERENCES depotdirect.regions(id);

CREATE INDEX IF NOT EXISTS idx_drivers_region ON depotdirect.drivers(region_id);


-- =====================================================================
-- Data Integrity Trigger (Optional but Recommended)
-- Ensure the Asset's Region belongs to the Asset's Company
-- =====================================================================
CREATE OR REPLACE FUNCTION depotdirect.fn_validate_asset_region_company()
RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE
  reg_company integer;
BEGIN
  -- If region is not set, skip validation
  IF NEW.region_id IS NULL THEN RETURN NEW; END IF;

  -- Get the company the region belongs to
  SELECT company_id INTO reg_company FROM depotdirect.regions WHERE id = NEW.region_id;

  -- Check against the asset's company
  -- Note: We use COALESCE to handle the different column names (company_id vs haulier_company_id)
  IF reg_company <> COALESCE(NEW.company_id, NEW.haulier_company_id) THEN
    RAISE EXCEPTION 'Region (Company %) does not match Asset Owner (Company %)', reg_company, COALESCE(NEW.company_id, NEW.haulier_company_id);
  END IF;

  RETURN NEW;
END;
$$;
ALTER FUNCTION depotdirect.fn_validate_asset_region_company() OWNER TO depotdirect_user;

-- Attach to Tractors
CREATE TRIGGER trg_validate_tractor_region
BEFORE INSERT OR UPDATE ON depotdirect.tractors
FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_asset_region_company();

-- Attach to Trailers
CREATE TRIGGER trg_validate_trailer_region
BEFORE INSERT OR UPDATE ON depotdirect.trailers
FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_asset_region_company();

-- Attach to Drivers
CREATE TRIGGER trg_validate_driver_region
BEFORE INSERT OR UPDATE ON depotdirect.drivers
FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_asset_region_company();