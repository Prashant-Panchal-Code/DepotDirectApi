# UserRegion Assignment Issue Fix

## Problem Description

The user-region assignment API was failing with the error:
```
"Database error while assigning user 1 to region 1: 42703: column \"country_id\" does not exist"
```

## Root Cause Analysis

The issue was in the SQL trigger function `fn_validate_user_region_matches_company_country()` that was created in the migration file `0002_init_schema.sql`. The trigger was trying to access a `country_id` column directly from the `regions` table:

```sql
-- INCORRECT: This line was trying to get country_id directly from regions table
SELECT country_id INTO reg_country FROM depotdirect.regions WHERE id = NEW.region_id;
```

However, based on the Entity Framework model, the database schema doesn't have a direct `country_id` column in the `regions` table. The relationship is:

**Region ? Company ? Country**

Not: **Region ? Country** (direct)

## Solution Applied

### 1. Fixed SQL Migration (0003_fix_user_regions_trigger.sql)

Created a corrected SQL migration that:
- Drops the incorrect trigger and function
- Creates a new trigger function that follows the correct relationship path
- Fixes the permissions on the sequence

**Key Fix:**
```sql
-- CORRECT: Get region's country through company relationship
SELECT c.country_id INTO reg_country 
FROM depotdirect.regions r 
JOIN depotdirect.companies c ON r.company_id = c.id 
WHERE r.id = NEW.region_id;
```

### 2. Updated C# Validation Method

Updated `ValidateUserRegionAssignmentAsync()` in `UserRegionRepository.cs` to use the correct relationship path with proper Entity Framework includes:

```csharp
var regionQuery = await _context.Regions
    .Where(r => r.Id == regionId && r.DeletedAt == null)
    .Include(r => r.Company)
    .ThenInclude(c => c.Country)  // Follow the correct path
    .FirstOrDefaultAsync();
```

### 3. Added Debug and Fix Endpoints

- **Debug endpoint:** `GET /api/admin/userregions/{userId}/{regionId}/debug`
- **Fix endpoint:** `POST /api/admin/userregions/fix-database-trigger`
- **Test endpoint:** `POST /api/admin/userregions/{userId}/{regionId}/test-assign`

## Steps to Apply the Fix

### Option 1: Use the API Endpoint (Recommended)
1. Run the application
2. Call the fix endpoint: `POST /api/admin/userregions/fix-database-trigger`
3. This will automatically apply the SQL fix to your database

### Option 2: Run SQL Manually
Execute the SQL file `DepotDirectApi/db/migrations/0003_fix_user_regions_trigger.sql` against your PostgreSQL database.

### Option 3: Use PostgreSQL Client
```sql
-- Run this SQL in your PostgreSQL database
DROP TRIGGER IF EXISTS trg_validate_user_region_matches_company_country ON depotdirect.user_regions;
DROP FUNCTION IF EXISTS depotdirect.fn_validate_user_region_matches_company_country();

-- Create corrected trigger function
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

-- Recreate the trigger
CREATE TRIGGER trg_validate_user_region_matches_company_country
  BEFORE INSERT OR UPDATE ON depotdirect.user_regions
  FOR EACH ROW EXECUTE FUNCTION depotdirect.fn_validate_user_region_matches_company_country();

-- Ensure proper permissions
GRANT USAGE, SELECT ON SEQUENCE depotdirect.user_regions_id_seq TO depotdirect_user;
GRANT INSERT, SELECT, UPDATE, DELETE ON TABLE depotdirect.user_regions TO depotdirect_user;
```

## Testing

After applying the fix, test with:

1. **Debug endpoint**: `GET /api/admin/userregions/1/1/debug` - Check user and region details
2. **Test assignment**: `POST /api/admin/userregions/1/1/test-assign` - Simple test assignment
3. **Full assignment**: `POST /api/admin/userregions` - Full validation assignment

## Files Modified

1. `DepotDirectApi/db/migrations/0003_fix_user_regions_trigger.sql` - New SQL fix
2. `DepotDirectApi/Repositories/UserRegionRepository.cs` - Updated validation logic
3. `DepotDirectApi/Controllers/Admin/UserRegionsController.cs` - Added debug and fix endpoints
4. `DepotDirectApi/Data/DepotDirectDbContext.cs` - Updated EF configuration
5. `DepotDirectApi/api-tests.http` - Added test endpoints

The fix ensures that the database trigger correctly validates that users can only be assigned to regions that belong to companies in the same country as the user's company.