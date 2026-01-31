-- =====================================================================
-- 0010_Depot_Products.sql
-- Mapping products to depots with location-specific physics & limits
-- =====================================================================

SET search_path = depotdirect, public;

CREATE SEQUENCE IF NOT EXISTS depotdirect.depot_products_id_seq START 1;

CREATE TABLE IF NOT EXISTS depotdirect.depot_products (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.depot_products_id_seq'),

  -- 1. Relationships
  depot_id integer NOT NULL REFERENCES depotdirect.depots(id) ON DELETE CASCADE,
  product_id integer NOT NULL REFERENCES depotdirect.products(id) ON DELETE RESTRICT,

  -- 2. Depot-Specific Physics (Overrides Product Master)
  -- If NULL, logic should fall back to Product Master values, but usually specific is better.
  density numeric(10,4),             -- Specific density for this depot's supply (kg/m3)
  planning_temperature numeric(5,2), -- Average temp for scheduling volume correction
  
  -- 3. Operational Constraints
  loading_rate_lpm numeric(10,2) NOT NULL DEFAULT 1500.00, -- Speed of loading pumps
  product_available boolean NOT NULL DEFAULT true,         -- "Is the tap on?"

  -- 4. Commercial & Limits
  cost_per_litre numeric(10,4),      -- Base cost at this location
  
  offtake_limit_active boolean NOT NULL DEFAULT false, -- Master switch for limits
  daily_min_limit_l numeric(12,2),   -- Take-or-pay contract minimums
  daily_max_limit_l numeric(12,2),   -- Rationing / allocation limit
  
  metadata jsonb DEFAULT '{}'::jsonb,
  
  created_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,

  -- Prevent adding the same product twice to the same depot
  CONSTRAINT depot_products_uniq UNIQUE (depot_id, product_id)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_depot_products_depot ON depotdirect.depot_products (depot_id);
CREATE INDEX IF NOT EXISTS idx_depot_products_product ON depotdirect.depot_products (product_id);

-- =====================================================================
-- Trigger Attachments
-- =====================================================================

-- updated_at
DROP TRIGGER IF EXISTS trg_set_updated_at_depot_products ON depotdirect.depot_products;
CREATE TRIGGER trg_set_updated_at_depot_products
    BEFORE UPDATE ON depotdirect.depot_products
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();

-- soft-delete
DROP TRIGGER IF EXISTS trg_soft_delete_depot_products ON depotdirect.depot_products;
CREATE TRIGGER trg_soft_delete_depot_products
    BEFORE DELETE ON depotdirect.depot_products
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();

-- =====================================================================
-- Validation Trigger: Ensure Product and Depot belong to same Company
-- =====================================================================
CREATE OR REPLACE FUNCTION depotdirect.fn_validate_depot_product_company()
RETURNS trigger LANGUAGE plpgsql AS $$
DECLARE
  depot_company integer;
  product_company integer;
BEGIN
  IF TG_OP NOT IN ('INSERT','UPDATE') THEN RETURN NEW; END IF;

  SELECT company_id INTO depot_company FROM depotdirect.depots WHERE id = NEW.depot_id;
  SELECT company_id INTO product_company FROM depotdirect.products WHERE id = NEW.product_id;

  IF depot_company <> product_company THEN
    RAISE EXCEPTION 'Company Mismatch: Depot (Company %) cannot stock Product (Company %)', depot_company, product_company;
  END IF;

  RETURN NEW;
END;
$$;
ALTER FUNCTION depotdirect.fn_validate_depot_product_company() OWNER TO depotdirect_user;

DROP TRIGGER IF EXISTS trg_validate_depot_product_company ON depotdirect.depot_products;
CREATE TRIGGER trg_validate_depot_product_company
  BEFORE INSERT OR UPDATE ON depotdirect.depot_products
  FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_depot_product_company();

-- =====================================================================
-- Ownership & Permissions (Fixing the Sequence Issue)
-- =====================================================================
ALTER TABLE IF EXISTS depotdirect.depot_products OWNER TO depotdirect_user;
-- Explicitly set sequence owner to prevent permission errors
ALTER SEQUENCE depotdirect.depot_products_id_seq OWNER TO depotdirect_user;