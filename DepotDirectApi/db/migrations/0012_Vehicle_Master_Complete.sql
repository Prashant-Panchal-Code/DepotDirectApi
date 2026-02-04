-- =====================================================================
-- 0012_Vehicle_Master_Complete.sql
-- Complete Vehicle & Resource Management Schema
-- Includes: Drivers, Break Rules, Tractors, Trailers, Combinations, and Rosters.
-- =====================================================================

SET search_path = depotdirect, public;

-- =====================================================================
-- 1. DRIVER & FATIGUE MANAGEMENT
-- =====================================================================

-- 1.1 Break Rules (Legislation/Policy)
CREATE SEQUENCE IF NOT EXISTS depotdirect.break_rules_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.break_rules (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.break_rules_id_seq'),
  rule_name text NOT NULL,
  company_id integer NOT NULL REFERENCES depotdirect.companies(id),
  
  max_continuous_drive_mins integer NOT NULL, -- e.g., 270 (4.5 hours)
  min_break_duration_mins integer NOT NULL,   -- e.g., 45
  max_daily_drive_mins integer NOT NULL,      -- e.g., 540 (9 hours)
  min_daily_rest_mins integer NOT NULL,       -- e.g., 660 (11 hours)
  
  active boolean DEFAULT true,
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now(),
  
  CONSTRAINT break_rules_uniq UNIQUE (company_id, rule_name)
);

-- 1.2 Drivers
CREATE SEQUENCE IF NOT EXISTS depotdirect.drivers_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.drivers (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.drivers_id_seq'),
  
  driver_code text NOT NULL,
  first_name text NOT NULL,
  last_name text NOT NULL,
  
  company_id integer NOT NULL REFERENCES depotdirect.companies(id),
  home_depot_id integer REFERENCES depotdirect.depots(id),
  
  license_number text NOT NULL,
  license_expiry date NOT NULL,
  hazmat_certified boolean DEFAULT true,
  
  break_rule_id integer REFERENCES depotdirect.break_rules(id),
  
  active boolean DEFAULT true,
  status text DEFAULT 'Available' CHECK (status IN ('Available', 'On Trip', 'On Leave', 'Sick')),
  
  mobile_number text,
  email citext,
  metadata jsonb DEFAULT '{}'::jsonb,
  
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now(),
  deleted_at timestamptz,
  
  CONSTRAINT drivers_license_uniq UNIQUE (company_id, license_number)
);

-- 1.3 Driver Shifts (Template Availability)
CREATE TABLE IF NOT EXISTS depotdirect.driver_shifts (
  id SERIAL PRIMARY KEY,
  driver_id integer NOT NULL REFERENCES depotdirect.drivers(id) ON DELETE CASCADE,
  day_of_week integer CHECK (day_of_week BETWEEN 0 AND 6),
  start_time time NOT NULL,
  end_time time NOT NULL,
  start_depot_id integer REFERENCES depotdirect.depots(id),
  active boolean DEFAULT true,
  UNIQUE (driver_id, day_of_week, start_time)
);

-- 1.4 Driver Time Off
CREATE TABLE IF NOT EXISTS depotdirect.driver_time_off (
  id SERIAL PRIMARY KEY,
  driver_id integer NOT NULL REFERENCES depotdirect.drivers(id) ON DELETE CASCADE,
  start_date timestamptz NOT NULL,
  end_date timestamptz NOT NULL,
  reason text,
  created_at timestamptz DEFAULT now()
);

-- =====================================================================
-- 2. VEHICLE ASSETS (TRACTORS & TRAILERS)
-- =====================================================================

-- 2.1 Tractors (Prime Movers)
CREATE SEQUENCE IF NOT EXISTS depotdirect.tractors_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.tractors (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.tractors_id_seq'),
  
  tractor_code text NOT NULL,
  tractor_name text NOT NULL,
  license_plate text NOT NULL,
  haulier_company_id integer NOT NULL REFERENCES depotdirect.companies(id),
  
  status text NOT NULL DEFAULT 'Active' CHECK (status IN ('Active', 'Maintenance', 'Inactive')),
  
  -- Physical Properties
  pump_available boolean NOT NULL DEFAULT false,
  pump_flow_rate_lpm numeric(10,2),
  curb_weight_kg numeric(12,2),
  number_of_axles integer,
  
  -- Axle Logic (JSONB: e.g. {'axle_1': 6000, 'axle_2': 11000})
  axle_configuration jsonb DEFAULT '{}'::jsonb,
  
  metadata jsonb DEFAULT '{}'::jsonb,
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now(),
  deleted_at timestamptz,
  
  CONSTRAINT tractors_code_uniq UNIQUE (haulier_company_id, tractor_code)
);

-- 2.2 Trailers
CREATE SEQUENCE IF NOT EXISTS depotdirect.trailers_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.trailers (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.trailers_id_seq'),
  
  trailer_code text NOT NULL,
  trailer_name text NOT NULL,
  license_plate text NOT NULL,
  haulier_company_id integer NOT NULL REFERENCES depotdirect.companies(id),
  
  unladen_weight_kg numeric(12,2),
  max_payload_kg numeric(12,2),
  max_payload_liters numeric(12,2),
  number_of_axles integer,
  
  -- Axle Logic
  axle_configuration jsonb DEFAULT '{}'::jsonb,
  
  status text NOT NULL DEFAULT 'Active',
  metadata jsonb DEFAULT '{}'::jsonb,
  
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now(),
  deleted_at timestamptz,
  
  CONSTRAINT trailers_code_uniq UNIQUE (haulier_company_id, trailer_code)
);

-- 2.3 Trailer Compartments
CREATE TABLE IF NOT EXISTS depotdirect.trailer_compartments (
  id SERIAL PRIMARY KEY,
  trailer_id integer NOT NULL REFERENCES depotdirect.trailers(id) ON DELETE CASCADE,
  compartment_number integer NOT NULL,
  
  capacity_l numeric(12,2) NOT NULL,
  min_volume_l numeric(12,2) DEFAULT 0,
  safe_fill_l numeric(12,2),
  
  must_use boolean DEFAULT false,
  partial_load_allowed boolean DEFAULT true,
  metadata jsonb DEFAULT '{}'::jsonb,
  
  UNIQUE (trailer_id, compartment_number)
);

-- 2.4 Allowed Products in Compartment
CREATE TABLE IF NOT EXISTS depotdirect.compartment_allowed_products (
  compartment_id integer NOT NULL REFERENCES depotdirect.trailer_compartments(id) ON DELETE CASCADE,
  product_id integer NOT NULL REFERENCES depotdirect.products(id) ON DELETE CASCADE,
  PRIMARY KEY (compartment_id, product_id)
);

-- =====================================================================
-- 3. COMBINATIONS & SCHEDULES
-- =====================================================================

-- 3.1 Vehicle Combinations (Tractor + Trailers)
CREATE SEQUENCE IF NOT EXISTS depotdirect.vehicle_combinations_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.vehicle_combinations (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.vehicle_combinations_id_seq'),
  
  combination_code text NOT NULL,
  tractor_id integer NOT NULL REFERENCES depotdirect.tractors(id),
  
  gross_weight_limit_kg numeric(12,2), 
  total_capacity_l numeric(12,2),
  
  active boolean DEFAULT true,
  is_default boolean DEFAULT false,
  
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now(),
  deleted_at timestamptz,
  
  UNIQUE (tractor_id, combination_code)
);

-- 3.2 Combination Links
CREATE TABLE IF NOT EXISTS depotdirect.vehicle_combination_trailers (
  combination_id integer NOT NULL REFERENCES depotdirect.vehicle_combinations(id) ON DELETE CASCADE,
  trailer_id integer NOT NULL REFERENCES depotdirect.trailers(id) ON DELETE RESTRICT,
  sequence_number integer NOT NULL DEFAULT 1,
  PRIMARY KEY (combination_id, trailer_id)
);

-- 3.3 Tractor Schedules (The Roster)
CREATE SEQUENCE IF NOT EXISTS depotdirect.tractor_schedules_id_seq START 1;
CREATE TABLE IF NOT EXISTS depotdirect.tractor_schedules (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.tractor_schedules_id_seq'),
  
  tractor_id integer NOT NULL REFERENCES depotdirect.tractors(id) ON DELETE CASCADE,
  driver_id integer REFERENCES depotdirect.drivers(id), -- Nullable if assigning asset only
  
  day_of_week integer NOT NULL CHECK (day_of_week BETWEEN 0 AND 6),
  shift_start_time time NOT NULL,
  shift_end_time time NOT NULL,
  
  -- Locations (Exclusive Arc Check)
  start_depot_id integer REFERENCES depotdirect.depots(id),
  start_parking_id integer REFERENCES depotdirect.parkings(id),
  end_depot_id integer REFERENCES depotdirect.depots(id),
  end_parking_id integer REFERENCES depotdirect.parkings(id),

  is_overtime boolean DEFAULT false,
  active boolean DEFAULT true,
  
  created_by integer,
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now(),
  deleted_at timestamptz,

  CONSTRAINT schedule_start_loc_check CHECK (
    (start_depot_id IS NOT NULL AND start_parking_id IS NULL) OR 
    (start_depot_id IS NULL AND start_parking_id IS NOT NULL)
  ),
  CONSTRAINT schedule_end_loc_check CHECK (
    (end_depot_id IS NOT NULL AND end_parking_id IS NULL) OR 
    (end_depot_id IS NULL AND end_parking_id IS NOT NULL)
  ),
  CONSTRAINT schedule_time_check CHECK (shift_start_time < shift_end_time)
);

-- Indexes for Scheduler
CREATE INDEX IF NOT EXISTS idx_tractor_schedules_search ON depotdirect.tractor_schedules (day_of_week, shift_start_time, shift_end_time);
CREATE INDEX IF NOT EXISTS idx_tractor_schedules_driver ON depotdirect.tractor_schedules (driver_id);

-- =====================================================================
-- 4. TRIGGERS & AUTOMATION
-- =====================================================================

DO $$
BEGIN
    -- Drivers
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_drivers') THEN
        CREATE TRIGGER trg_set_updated_at_drivers BEFORE UPDATE ON depotdirect.drivers FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_drivers') THEN
        CREATE TRIGGER trg_soft_delete_drivers BEFORE DELETE ON depotdirect.drivers FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
    END IF;

    -- Tractors
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_tractors') THEN
        CREATE TRIGGER trg_set_updated_at_tractors BEFORE UPDATE ON depotdirect.tractors FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_tractors') THEN
        CREATE TRIGGER trg_soft_delete_tractors BEFORE DELETE ON depotdirect.tractors FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
    END IF;

    -- Trailers
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_trailers') THEN
        CREATE TRIGGER trg_set_updated_at_trailers BEFORE UPDATE ON depotdirect.trailers FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_trailers') THEN
        CREATE TRIGGER trg_soft_delete_trailers BEFORE DELETE ON depotdirect.trailers FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
    END IF;

    -- Combinations
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_vehicle_combinations') THEN
        CREATE TRIGGER trg_set_updated_at_vehicle_combinations BEFORE UPDATE ON depotdirect.vehicle_combinations FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_vehicle_combinations') THEN
        CREATE TRIGGER trg_soft_delete_vehicle_combinations BEFORE DELETE ON depotdirect.vehicle_combinations FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
    END IF;

    -- Schedules
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_set_updated_at_tractor_schedules') THEN
        CREATE TRIGGER trg_set_updated_at_tractor_schedules BEFORE UPDATE ON depotdirect.tractor_schedules FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'trg_soft_delete_tractor_schedules') THEN
        CREATE TRIGGER trg_soft_delete_tractor_schedules BEFORE DELETE ON depotdirect.tractor_schedules FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();
    END IF;
END $$;

-- =====================================================================
-- 5. OWNERSHIP PERMISSIONS
-- =====================================================================
-- Correctly setting owner for Tables AND Sequences to prevent 42501 errors

ALTER TABLE IF EXISTS depotdirect.break_rules OWNER TO depotdirect_user;
ALTER SEQUENCE depotdirect.break_rules_id_seq OWNER TO depotdirect_user;

ALTER TABLE IF EXISTS depotdirect.drivers OWNER TO depotdirect_user;
ALTER SEQUENCE depotdirect.drivers_id_seq OWNER TO depotdirect_user;

ALTER TABLE IF EXISTS depotdirect.driver_shifts OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.driver_time_off OWNER TO depotdirect_user;

ALTER TABLE IF EXISTS depotdirect.tractors OWNER TO depotdirect_user;
ALTER SEQUENCE depotdirect.tractors_id_seq OWNER TO depotdirect_user;

ALTER TABLE IF EXISTS depotdirect.trailers OWNER TO depotdirect_user;
ALTER SEQUENCE depotdirect.trailers_id_seq OWNER TO depotdirect_user;

ALTER TABLE IF EXISTS depotdirect.trailer_compartments OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.compartment_allowed_products OWNER TO depotdirect_user;

ALTER TABLE IF EXISTS depotdirect.vehicle_combinations OWNER TO depotdirect_user;
ALTER SEQUENCE depotdirect.vehicle_combinations_id_seq OWNER TO depotdirect_user;
ALTER TABLE IF EXISTS depotdirect.vehicle_combination_trailers OWNER TO depotdirect_user;

ALTER TABLE IF EXISTS depotdirect.tractor_schedules OWNER TO depotdirect_user;
ALTER SEQUENCE depotdirect.tractor_schedules_id_seq OWNER TO depotdirect_user;