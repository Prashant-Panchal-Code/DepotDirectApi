-- =====================================================================
-- 0009_Notes.sql
-- Unified Notes system for Sites, Depots, Parkings, and Vehicles
-- Uses "Exclusive Arc" pattern for referential integrity
-- =====================================================================

SET search_path = depotdirect, public;

CREATE SEQUENCE IF NOT EXISTS depotdirect.notes_id_seq START 1;

CREATE TABLE IF NOT EXISTS depotdirect.notes (
  id integer PRIMARY KEY DEFAULT nextval('depotdirect.notes_id_seq'),

  -- 1. Categorization & Content
  category text NOT NULL CHECK (category IN ('General', 'Maintenance', 'Safety', 'Delivery Operations')),
  priority text NOT NULL DEFAULT 'Medium' CHECK (priority IN ('High', 'Medium', 'Low')),
  comment text NOT NULL, -- The main content

  -- 2. Workflow / Status
  status text NOT NULL DEFAULT 'Open' CHECK (status IN ('Open', 'In Review', 'Closed')),
  
  -- Closing details (nullable until closed)
  closing_comment text,
  closed_at timestamptz,
  closed_by integer, -- Reference to user_id usually, but integer for now

  -- 3. Polymorphic Associations (The "Exclusive Arc")
  -- Only ONE of these should be set.
  site_id integer REFERENCES depotdirect.sites(id) ON DELETE CASCADE,
  depot_id integer REFERENCES depotdirect.depots(id) ON DELETE CASCADE,
  parking_id integer REFERENCES depotdirect.parkings(id) ON DELETE CASCADE,
  
  -- Note: Vehicle table doesn't exist yet, so we define column but add FK later
  vehicle_id integer, 
  
  -- 4. Scope & Ownership
  company_id integer NOT NULL REFERENCES depotdirect.companies(id) ON DELETE RESTRICT,
  
  created_by integer,
  created_at timestamptz NOT NULL DEFAULT now(),
  updated_at timestamptz NOT NULL DEFAULT now(),
  deleted_at timestamptz,

  -- 5. Data Integrity Constraint
  -- Ensure exactly one target is selected (not 0, not 2)
  CONSTRAINT notes_target_check CHECK (
    (site_id IS NOT NULL)::int +
    (depot_id IS NOT NULL)::int +
    (parking_id IS NOT NULL)::int +
    (vehicle_id IS NOT NULL)::int = 1
  )
);

-- =====================================================================
-- Indexes for Performance
-- =====================================================================
-- We query notes by their parent entity frequently
CREATE INDEX IF NOT EXISTS idx_notes_site ON depotdirect.notes (site_id) WHERE site_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_notes_depot ON depotdirect.notes (depot_id) WHERE depot_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_notes_parking ON depotdirect.notes (parking_id) WHERE parking_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_notes_vehicle ON depotdirect.notes (vehicle_id) WHERE vehicle_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_notes_company ON depotdirect.notes (company_id);
CREATE INDEX IF NOT EXISTS idx_notes_status ON depotdirect.notes (status);

-- =====================================================================
-- Trigger Attachments
-- =====================================================================

-- updated_at
DROP TRIGGER IF EXISTS trg_set_updated_at_notes ON depotdirect.notes;
CREATE TRIGGER trg_set_updated_at_notes
    BEFORE UPDATE ON depotdirect.notes
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_set_updated_at();

-- soft-delete
DROP TRIGGER IF EXISTS trg_soft_delete_notes ON depotdirect.notes;
CREATE TRIGGER trg_soft_delete_notes
    BEFORE DELETE ON depotdirect.notes
    FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_soft_delete();

-- Ownership
ALTER TABLE IF EXISTS depotdirect.notes OWNER TO depotdirect_user;
-- Fix permission by setting the correct owner
ALTER SEQUENCE depotdirect.notes_id_seq OWNER TO depotdirect_user;

-- Alternatively, if you want to keep ownership separate, explicitly grant usage:
-- GRANT USAGE, SELECT ON SEQUENCE depotdirect.notes_id_seq TO depotdirect_user;