-- =====================================================================
-- 0014_Hauliers_Refactor.sql
-- Separates the "Shipper" (Company) from the "Carrier" (Haulier)
-- =====================================================================

SET search_path = depotdirect, public;

-- 1. Create the Hauliers Table
CREATE SEQUENCE IF NOT EXISTS depotdirect.hauliers_id_seq START 1;

CREATE TABLE IF NOT EXISTS depotdirect.hauliers (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.hauliers_id_seq'),
  
  -- The Link to the Oil Company's Operations
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE CASCADE,
  
  -- Haulier Identity
  haulier_code text NOT NULL,
  haulier_name text NOT NULL,
  
  -- Vendor Management
  tax_id text,                 -- GST/VAT Number
  contract_number text,        -- Reference to the legal contract with Shell
  contract_expiry date,
  
  -- Operational Contacts
  contact_name text,
  contact_email citext,
  contact_phone text,
  
  active boolean DEFAULT true,
  metadata jsonb DEFAULT '{}'::jsonb,
  
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now(),
  deleted_at timestamptz,

  -- Constraint: Code must be unique within the Region
  CONSTRAINT hauliers_code_region_uniq UNIQUE (region_id, haulier_code)
);

-- Index for filtering "Show me all hauliers in the North Region"
CREATE INDEX IF NOT EXISTS idx_hauliers_region ON depotdirect.hauliers(region_id);


-- 2. Update Tractors to point to Hauliers (Breaking Change)
-- We drop the old link to 'companies' and point to 'hauliers'
ALTER TABLE depotdirect.tractors
DROP CONSTRAINT IF EXISTS tractors_haulier_company_id_fkey, -- Drop FK to companies
DROP CONSTRAINT IF EXISTS tractors_code_uniq;               -- Drop old unique constraint

-- Rename the column to be clear (optional but recommended)
ALTER TABLE depotdirect.tractors 
RENAME COLUMN haulier_company_id TO haulier_id;

-- Add new FK to hauliers table
ALTER TABLE depotdirect.tractors
ADD CONSTRAINT tractors_haulier_id_fkey 
FOREIGN KEY (haulier_id) REFERENCES depotdirect.hauliers(id);

-- Re-add unique constraint (Tractor Code must be unique per Haulier)
ALTER TABLE depotdirect.tractors
ADD CONSTRAINT tractors_code_uniq UNIQUE (haulier_id, tractor_code);


-- 3. Update Trailers to point to Hauliers
ALTER TABLE depotdirect.trailers
DROP CONSTRAINT IF EXISTS trailers_haulier_company_id_fkey,
DROP CONSTRAINT IF EXISTS trailers_code_uniq;

ALTER TABLE depotdirect.trailers 
RENAME COLUMN haulier_company_id TO haulier_id;

ALTER TABLE depotdirect.trailers
ADD CONSTRAINT trailers_haulier_id_fkey 
FOREIGN KEY (haulier_id) REFERENCES depotdirect.hauliers(id);

ALTER TABLE depotdirect.trailers
ADD CONSTRAINT trailers_code_uniq UNIQUE (haulier_id, trailer_code);


-- 4. Update Drivers (Optional but Logical)
-- Usually, drivers work for the Haulier, not directly for Shell.
-- If you agree, we should move drivers too. Run this block if yes:
/*
ALTER TABLE depotdirect.drivers
ADD COLUMN haulier_id integer REFERENCES depotdirect.hauliers(id);

-- If drivers are external, remove company_id or make it nullable?
-- Usually, we keep company_id as the "Tenant" (Shell) and haulier_id as the "Employer".
*/

-- =====================================================================
-- Trigger Attachments
-- =====================================================================

-- updated_at
DROP TRIGGER IF EXISTS trg_set_updated_at_hauliers ON depotdirect.hauliers;
CREATE TRIGGER trg_set_updated_at_hauliers
    BEFORE UPDATE ON depotdirect.hauliers
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();

-- soft-delete
DROP TRIGGER IF EXISTS trg_soft_delete_hauliers ON depotdirect.hauliers;
CREATE TRIGGER trg_soft_delete_hauliers
    BEFORE DELETE ON depotdirect.hauliers
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();

-- =====================================================================
-- Ownership
-- =====================================================================
ALTER TABLE IF EXISTS depotdirect.hauliers OWNER TO depotdirect_user;
ALTER SEQUENCE depotdirect.hauliers_id_seq OWNER TO depotdirect_user;