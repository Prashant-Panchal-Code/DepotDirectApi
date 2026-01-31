-- =====================================================================
-- 0007_Products.sql
-- Add products master table (idempotent)
-- =====================================================================

SET search_path = depotdirect, public;

CREATE SEQUENCE IF NOT EXISTS depotdirect.products_id_seq START 1;

CREATE TABLE IF NOT EXISTS depotdirect.products (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.products_id_seq'),
  
  -- Core Identity
  product_code text NOT NULL,        -- e.g., 'UL95', 'AGO', 'JET-A1'
  product_name text NOT NULL,        -- e.g., 'Unleaded 95', 'Automotive Gas Oil'
  short_name varchar(50),            -- For UI/Reports
  
  -- Physical Properties (Critical for Oil/Gas)
  density numeric(10,4),             -- Standard density at 15°C (kg/m³)
  base_temperature numeric(5,2) DEFAULT 15.00, -- Reference temp for density
  viscosity numeric(10,4),           -- Optional: for flow rate calcs
  
  -- Scope & Hierarchy
  region_id integer NOT NULL REFERENCES depotdirect.regions(id) ON DELETE RESTRICT,
  company_id integer NOT NULL REFERENCES depotdirect.companies(id) ON DELETE RESTRICT,
  
  -- Flags & Metadata
  active boolean NOT NULL DEFAULT true,
  is_hazardous boolean NOT NULL DEFAULT true, -- For routing/safety constraints
  color_code varchar(7),             -- Hex code for UI scheduling (e.g., #FFD700)
  
  metadata jsonb DEFAULT '{}'::jsonb,
  created_by integer,
  last_updated_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,

  -- Constraints
  CONSTRAINT products_company_code_uniq UNIQUE (company_id, product_code)
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_products_code ON depotdirect.products (product_code);
CREATE INDEX IF NOT EXISTS idx_products_region ON depotdirect.products (region_id);
CREATE INDEX IF NOT EXISTS idx_products_company ON depotdirect.products (company_id);

-- Attach standard triggers
DO $$
BEGIN
  -- updated_at
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_products') THEN
    CREATE TRIGGER trg_set_updated_at_products BEFORE UPDATE ON depotdirect.products 
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
  END IF;

  -- soft-delete
  IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_products') THEN
    CREATE TRIGGER trg_soft_delete_products BEFORE DELETE ON depotdirect.products 
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
  END IF;
END;
$$ LANGUAGE plpgsql;

ALTER TABLE depotdirect.products OWNER TO depotdirect_user;