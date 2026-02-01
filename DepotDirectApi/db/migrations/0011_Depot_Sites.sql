-- =====================================================================
-- 0011_Depot_Sites.sql
-- Network topology: Valid routes from Depots to Sites
-- =====================================================================

SET search_path = depotdirect, public;

CREATE SEQUENCE IF NOT EXISTS depotdirect.depot_sites_id_seq START 1;

CREATE TABLE IF NOT EXISTS depotdirect.depot_sites (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.depot_sites_id_seq'),

  -- 1. The Route (Edge)
  depot_id integer NOT NULL REFERENCES depotdirect.depots(id) ON DELETE CASCADE,
  site_id integer NOT NULL REFERENCES depotdirect.sites(id) ON DELETE CASCADE,

  -- 2. Route Physics (Critical for Scheduling)
  distance_km numeric(10,2) NOT NULL,    -- One-way distance
  travel_time_mins integer NOT NULL,     -- Standard one-way trip time (loaded)
  
  -- Optional: Precision scheduling fields
  return_time_mins integer,              -- If empty truck returns faster (optional)
  
  -- 3. Logic & Preference
  active boolean NOT NULL DEFAULT true,  -- "Is this route currently drivable?"
  is_primary boolean NOT NULL DEFAULT false, -- Preference: "Serve from here first"
  
  -- 4. Financials (Optional)
  transport_rate numeric(10,2),          -- Specific cost for this leg (if not distance based)

  metadata jsonb DEFAULT '{}'::jsonb,
  
  created_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,

  -- Constraint: A site can only be mapped to a specific depot once
  CONSTRAINT depot_sites_uniq UNIQUE (depot_id, site_id)
);

-- Indexes for performance (Route lookups are frequent)
CREATE INDEX IF NOT EXISTS idx_depot_sites_depot ON depotdirect.depot_sites (depot_id);
CREATE INDEX IF NOT EXISTS idx_depot_sites_site ON depotdirect.depot_sites (site_id);
-- Helpful for finding the "Best" depot for a site
CREATE INDEX IF NOT EXISTS idx_depot_sites_primary ON depotdirect.depot_sites (site_id, is_primary);

-- =====================================================================
-- Trigger Attachments
-- =====================================================================

-- updated_at
DROP TRIGGER IF EXISTS trg_set_updated_at_depot_sites ON depotdirect.depot_sites;
CREATE TRIGGER trg_set_updated_at_depot_sites
    BEFORE UPDATE ON depotdirect.depot_sites
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();

-- soft-delete
DROP TRIGGER IF EXISTS trg_soft_delete_depot_sites ON depotdirect.depot_sites;
CREATE TRIGGER trg_soft_delete_depot_sites
    BEFORE DELETE ON depotdirect.depot_sites
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();

-- =====================================================================
-- Validation Trigger: Ensure Depot and Site belong to same Company
-- =====================================================================
CREATE OR REPLACE FUNCTION depotdirect.fn_validate_depot_site_company()
RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE
  depot_company integer;
  site_company integer;
BEGIN
  IF TG_OP NOT IN ('INSERT','UPDATE') THEN RETURN NEW; END IF;

  SELECT company_id INTO depot_company FROM depotdirect.depots WHERE id = NEW.depot_id;
  SELECT company_id INTO site_company FROM depotdirect.sites WHERE id = NEW.site_id;

  IF depot_company <> site_company THEN
    RAISE EXCEPTION 'Company Mismatch: Depot (Company %) cannot serve Site (Company %)', depot_company, site_company;
  END IF;

  RETURN NEW;
END;
$$;
ALTER FUNCTION depotdirect.fn_validate_depot_site_company() OWNER TO depotdirect_user;

DROP TRIGGER IF EXISTS trg_validate_depot_site_company ON depotdirect.depot_sites;
CREATE TRIGGER trg_validate_depot_site_company
  BEFORE INSERT OR UPDATE ON depotdirect.depot_sites
  FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_depot_site_company();

-- =====================================================================
-- Ownership & Permissions
-- =====================================================================
ALTER TABLE IF EXISTS depotdirect.depot_sites OWNER TO depotdirect_user;
ALTER SEQUENCE depotdirect.depot_sites_id_seq OWNER TO depotdirect_user;