-- Migration: Fix user_regions validation trigger to use correct relationship
-- This fixes the trigger function to get country_id through the company relationship
-- Region -> Company -> Country (not direct Region -> Country)

-- Drop the existing trigger and function first
DROP TRIGGER IF EXISTS trg_validate_user_region_matches_company_country ON depotdirect.user_regions;
DROP FUNCTION IF EXISTS depotdirect.fn_validate_user_region_matches_company_country();

-- Create corrected trigger function that follows the proper relationship
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

  -- Get region's country through company relationship: region -> company -> country
  SELECT c.country_id INTO reg_country 
  FROM depotdirect.regions r 
  JOIN depotdirect.companies c ON r.company_id = c.id 
  WHERE r.id = NEW.region_id;
  
  IF reg_country IS NULL THEN
    RAISE EXCEPTION 'region id % does not exist or its company has no country_id', NEW.region_id;
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

-- Recreate the trigger
CREATE TRIGGER trg_validate_user_region_matches_company_country
  BEFORE INSERT OR UPDATE ON depotdirect.user_regions
  FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_user_region_matches_company_country();

-- Also ensure the sequence permissions are correct
GRANT USAGE, SELECT ON SEQUENCE depotdirect.user_regions_id_seq TO depotdirect_user;
GRANT INSERT, SELECT, UPDATE, DELETE ON TABLE depotdirect.user_regions TO depotdirect_user;