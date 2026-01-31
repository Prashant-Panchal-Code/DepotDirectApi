-- =====================================================================
-- 0008_Tanks_and_Inventory.sql
-- Description: Master data for Site Tanks, Inventory Readings, 
--              Delivery Planning, and Demand Forecasting patterns.
-- =====================================================================

SET search_path = depotdirect, public;

-- 1) Master Data: Site Tanks
CREATE TABLE IF NOT EXISTS depotdirect.site_tanks (
  id SERIAL PRIMARY KEY,
  site_id integer NOT NULL REFERENCES depotdirect.sites(id),
  product_id integer REFERENCES depotdirect.products(id),
  
  tank_code text NOT NULL,           -- Unique identifier for the tank at the site
  capacity_l numeric(12,2) NOT NULL DEFAULT 0, -- Total Shell Capacity
  safe_fill_l numeric(12,2) NOT NULL DEFAULT 0, -- Max limit (e.g., 95% of capacity)
  deadstock_l numeric(12,2) NOT NULL DEFAULT 0, -- Unusable volume at bottom
  
  discharge_rate_lpm numeric(10,2),  -- Flow rate: Truck -> Tank (Liters Per Minute)
  
  active boolean NOT NULL DEFAULT true,
  metadata jsonb DEFAULT '{}'::jsonb,
  
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now(),
  
  CONSTRAINT tank_site_code_uniq UNIQUE (site_id, tank_code)
);

-- 2) Transactional: Stock Records (Readings & Daily Stats)
CREATE TABLE IF NOT EXISTS depotdirect.tank_readings (
  id SERIAL PRIMARY KEY,
  tank_id integer NOT NULL REFERENCES depotdirect.site_tanks(id),
  
  reading_timestamp timestamptz NOT NULL DEFAULT now(),
  reading_method text NOT NULL,      -- 'ATG' or 'Manual'
  
  current_volume_l numeric(12,2) NOT NULL,
  
  -- Stats calculated by .NET and stored here
  sales_since_last_reading_l numeric(12,2) DEFAULT 0,
  avg_daily_sales_l numeric(12,2),   -- Moving average result from .NET
  
  metadata jsonb DEFAULT '{}'::jsonb,
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now()
);

-- 3) Planning: Site Deliveries
CREATE TABLE IF NOT EXISTS depotdirect.tank_deliveries (
  id SERIAL PRIMARY KEY,
  tank_id integer NOT NULL REFERENCES depotdirect.site_tanks(id),
  
  status text NOT NULL DEFAULT 'Planned', -- Planned, Confirmed, Cancelled
  planned_quantity_l numeric(12,2),
  confirmed_quantity_l numeric(12,2),
  
  scheduled_arrival timestamptz,
  actual_arrival timestamptz,
  
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now()
);

-- 4) Analytics: Sales Patterns for Forecast
CREATE TABLE IF NOT EXISTS depotdirect.sales_patterns (
  id SERIAL PRIMARY KEY,
  tank_id integer NOT NULL REFERENCES depotdirect.site_tanks(id),
  
  day_of_week integer CHECK (day_of_week BETWEEN 0 AND 6), -- 0=Sun, 6=Sat
  hour_of_day integer CHECK (hour_of_day BETWEEN 0 AND 23),
  
  weight_factor numeric(5,4) DEFAULT 1.0000, -- e.g., 1.2 for 20% higher demand
  avg_hourly_sales_l numeric(12,2) DEFAULT 0,
  
  updated_at timestamptz DEFAULT now(),
  UNIQUE(tank_id, day_of_week, hour_of_day)
);

-- =====================================================================
-- Trigger Attachments (Idempotent)
-- =====================================================================

DO $$
BEGIN
    -- site_tanks
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_site_tanks') THEN
        CREATE TRIGGER trg_set_updated_at_site_tanks 
        BEFORE UPDATE ON depotdirect.site_tanks 
        FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
    END IF;

    -- tank_readings
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_tank_readings') THEN
        CREATE TRIGGER trg_set_updated_at_tank_readings 
        BEFORE UPDATE ON depotdirect.tank_readings 
        FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
    END IF;

    -- tank_deliveries
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_tank_deliveries') THEN
        CREATE TRIGGER trg_set_updated_at_tank_deliveries 
        BEFORE UPDATE ON depotdirect.tank_deliveries 
        FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
    END IF;

    -- sales_patterns
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_sales_patterns') THEN
        CREATE TRIGGER trg_set_updated_at_sales_patterns 
        BEFORE UPDATE ON depotdirect.sales_patterns 
        FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
    END IF;
END $$;

-- Ownership
ALTER TABLE IF EXISTS depotdirect.site_tanks OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.tank_readings OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.tank_deliveries OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.sales_patterns OWNER TO depotdirect_user;

SET search_path = depotdirect, public;

-- 1) Master Data: Site Tanks
CREATE TABLE IF NOT EXISTS depotdirect.site_tanks (
  id SERIAL PRIMARY KEY,
  site_id integer NOT NULL REFERENCES depotdirect.sites(id),
  product_id integer REFERENCES depotdirect.products(id),
  
  tank_code text NOT NULL,           -- Unique identifier for the tank at the site
  capacity_l numeric(12,2) NOT NULL DEFAULT 0, -- Total Shell Capacity
  safe_fill_l numeric(12,2) NOT NULL DEFAULT 0, -- Max limit (e.g., 95% of capacity)
  deadstock_l numeric(12,2) NOT NULL DEFAULT 0, -- Unusable volume at bottom
  
  discharge_rate_lpm numeric(10,2),  -- Flow rate: Truck -> Tank (Liters Per Minute)
  
  active boolean NOT NULL DEFAULT true,
  metadata jsonb DEFAULT '{}'::jsonb,
  
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now(),
  
  CONSTRAINT tank_site_code_uniq UNIQUE (site_id, tank_code)
);

-- 2) Transactional: Stock Records (Readings & Daily Stats)
CREATE TABLE IF NOT EXISTS depotdirect.tank_readings (
  id SERIAL PRIMARY KEY,
  tank_id integer NOT NULL REFERENCES depotdirect.site_tanks(id),
  
  reading_timestamp timestamptz NOT NULL DEFAULT now(),
  reading_method text NOT NULL,      -- 'ATG' or 'Manual'
  
  current_volume_l numeric(12,2) NOT NULL,
  
  -- Stats calculated by .NET and stored here
  sales_since_last_reading_l numeric(12,2) DEFAULT 0,
  avg_daily_sales_l numeric(12,2),   -- Moving average result from .NET
  
  metadata jsonb DEFAULT '{}'::jsonb,
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now()
);

-- 3) Planning: Site Deliveries
CREATE TABLE IF NOT EXISTS depotdirect.tank_deliveries (
  id SERIAL PRIMARY KEY,
  tank_id integer NOT NULL REFERENCES depotdirect.site_tanks(id),
  
  status text NOT NULL DEFAULT 'Planned', -- Planned, Confirmed, Cancelled
  planned_quantity_l numeric(12,2),
  confirmed_quantity_l numeric(12,2),
  
  scheduled_arrival timestamptz,
  actual_arrival timestamptz,
  
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now()
);

-- 4) Analytics: Sales Patterns for Forecast
CREATE TABLE IF NOT EXISTS depotdirect.sales_patterns (
  id SERIAL PRIMARY KEY,
  tank_id integer NOT NULL REFERENCES depotdirect.site_tanks(id),
  
  day_of_week integer CHECK (day_of_week BETWEEN 0 AND 6), -- 0=Sun, 6=Sat
  hour_of_day integer CHECK (hour_of_day BETWEEN 0 AND 23),
  
  weight_factor numeric(5,4) DEFAULT 1.0000, -- e.g., 1.2 for 20% higher demand
  avg_hourly_sales_l numeric(12,2) DEFAULT 0,
  
  updated_at timestamptz DEFAULT now(),
  UNIQUE(tank_id, day_of_week, hour_of_day)
);

-- =====================================================================
-- Trigger Attachments
-- =====================================================================

-- We drop and recreate to ensure the script is idempotent and complete
DROP TRIGGER IF EXISTS trg_set_updated_at_site_tanks ON depotdirect.site_tanks;
CREATE TRIGGER trg_set_updated_at_site_tanks 
    BEFORE UPDATE ON depotdirect.site_tanks 
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();

DROP TRIGGER IF EXISTS trg_set_updated_at_tank_readings ON depotdirect.tank_readings;
CREATE TRIGGER trg_set_updated_at_tank_readings 
    BEFORE UPDATE ON depotdirect.tank_readings 
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();

DROP TRIGGER IF EXISTS trg_set_updated_at_tank_deliveries ON depotdirect.tank_deliveries;
CREATE TRIGGER trg_set_updated_at_tank_deliveries 
    BEFORE UPDATE ON depotdirect.tank_deliveries 
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();

DROP TRIGGER IF EXISTS trg_set_updated_at_sales_patterns ON depotdirect.sales_patterns;
CREATE TRIGGER trg_set_updated_at_sales_patterns 
    BEFORE UPDATE ON depotdirect.sales_patterns 
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();

-- Ownership
ALTER TABLE IF EXISTS depotdirect.site_tanks OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.tank_readings OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.tank_deliveries OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.sales_patterns OWNER TO depotdirect_user;