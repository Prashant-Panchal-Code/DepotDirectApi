-- =====================================================================
-- Fix latlong computed column to use 0,0 instead of NULL
-- Run this after 0004_Sites.sql has been applied
-- =====================================================================

SET search_path = depotdirect, public;

-- Drop the existing computed column
ALTER TABLE depotdirect.sites DROP COLUMN IF EXISTS latlong;

-- Add it back with default 0,0 instead of NULL
ALTER TABLE depotdirect.sites 
ADD COLUMN latlong text 
GENERATED ALWAYS AS (
  COALESCE(latitude, 0)::text || ',' || COALESCE(longitude, 0)::text
) STORED;

-- =====================================================================
-- Explanation:
-- COALESCE(latitude, 0) returns the latitude value, or 0 if it's NULL
-- COALESCE(longitude, 0) returns the longitude value, or 0 if it's NULL
-- This ensures latlong is always "0,0" when coordinates are NULL
-- and "lat,long" when they have values
-- =====================================================================
