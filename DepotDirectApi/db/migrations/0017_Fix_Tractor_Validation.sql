-- =====================================================================
-- 0017_Fix_Tractor_Validation.sql
-- Fixes the "record new has no field company_id" error by updating triggers
-- =====================================================================

SET search_path = depotdirect, public;

-- 1. Drop the broken triggers on Tractors and Trailers
DROP TRIGGER IF EXISTS trg_validate_tractor_region ON depotdirect.tractors;
DROP TRIGGER IF EXISTS trg_validate_trailer_region ON depotdirect.trailers;

-- (Optional) Drop the shared function if you aren't using it for other custom tables
-- DROP FUNCTION IF EXISTS depotdirect.fn_validate_asset_region_company();


-- 2. Create a specific validation function for Haulier-based assets
-- This checks: Does the Tractor's Region match its Haulier's Region?
CREATE OR REPLACE FUNCTION depotdirect.fn_validate_asset_haulier_region()
RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE
  haulier_region integer;
BEGIN
  -- If region or haulier is missing, skip check (or enforce NOT NULL via table constraints)
  IF NEW.region_id IS NULL OR NEW.haulier_id IS NULL THEN 
    RETURN NEW; 
  END IF;

  -- Look up the Region of the assigned Haulier
  SELECT region_id INTO haulier_region 
  FROM depotdirect.hauliers 
  WHERE id = NEW.haulier_id;

  -- Validation: The Asset must belong to the same region as the Haulier
  IF haulier_region <> NEW.region_id THEN
    RAISE EXCEPTION 'Asset Region (ID %) does not match Haulier Region (ID %)', NEW.region_id, haulier_region;
  END IF;

  RETURN NEW;
END;
$$;

ALTER FUNCTION depotdirect.fn_validate_asset_haulier_region() OWNER TO depotdirect_user;


-- 3. Attach the new correct trigger to Tractors and Trailers
CREATE TRIGGER trg_validate_tractor_haulier_region
BEFORE INSERT OR UPDATE ON depotdirect.tractors
FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_asset_haulier_region();

CREATE TRIGGER trg_validate_trailer_haulier_region
BEFORE INSERT OR UPDATE ON depotdirect.trailers
FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_asset_haulier_region();