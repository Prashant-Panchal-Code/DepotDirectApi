# Sites Module - Database Schema & Validation

## Database Schema

### Sites Table (`depotdirect.sites`)

```sql
CREATE TABLE depotdirect.sites (
  id                INTEGER PRIMARY KEY,
  site_code         TEXT NOT NULL,
  site_name         TEXT NOT NULL,
  shortcode         TEXT,
  latitude          NUMERIC(10,7),
  longitude         NUMERIC(10,7),
  latlong           TEXT GENERATED ALWAYS AS (
    CASE 
      WHEN latitude IS NOT NULL AND longitude IS NOT NULL 
      THEN (latitude::text || ',' || longitude::text) 
      ELSE NULL 
    END
  ) STORED,
  street            TEXT,
  postal_code       TEXT,
  town              TEXT,
  active            BOOLEAN NOT NULL DEFAULT TRUE,
  priority          TEXT NOT NULL DEFAULT 'Medium',
  contact_person    TEXT,
  phone             TEXT,
  email             CITEXT,
  operating_hours   JSONB DEFAULT '{}'::jsonb,
  depot_id          INTEGER,
  delivery_stopped  BOOLEAN NOT NULL DEFAULT FALSE,
  pumped_required   BOOLEAN NOT NULL DEFAULT FALSE,
  country_id        INTEGER NOT NULL REFERENCES depotdirect.countries(id),
  company_id        INTEGER NOT NULL REFERENCES depotdirect.companies(id),
  metadata          JSONB DEFAULT '{}'::jsonb,
  created_by        INTEGER,
  last_updated_by   INTEGER,
  created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  deleted_at        TIMESTAMPTZ,
  
  CONSTRAINT sites_priority_chk CHECK (priority IN ('High','Medium','Low')),
  CONSTRAINT sites_country_code_uniq UNIQUE (country_id, site_code)
);
```

### Region Sites Mapping Table (`depotdirect.region_sites`)

```sql
CREATE TABLE depotdirect.region_sites (
  id          INTEGER PRIMARY KEY,
  site_id     INTEGER NOT NULL REFERENCES depotdirect.sites(id) ON DELETE CASCADE,
  region_id   INTEGER NOT NULL REFERENCES depotdirect.regions(id) ON DELETE CASCADE,
  site_code   TEXT,
  metadata    JSONB DEFAULT '{}'::jsonb,
  created_by  INTEGER,
  created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  deleted_at  TIMESTAMPTZ,
  
  UNIQUE (site_id, region_id)
);
```

## Indexes

### Sites Table Indexes
- `idx_sites_site_code` on `site_code` - Fast lookups by code
- `idx_sites_country` on `country_id` - Filter by country
- `idx_sites_town` on `town` - Search by town
- `idx_sites_company` on `company_id` - Filter by company
- `idx_sites_shortcode` on `shortcode` - Quick lookups by shortcode
- `sites_country_code_uniq` UNIQUE on `(country_id, site_code)` - Enforce uniqueness

### Region Sites Indexes
- `idx_region_sites_site` on `site_id` - Fast region lookups for a site
- `idx_region_sites_region` on `region_id` - Fast site lookups for a region
- UNIQUE on `(site_id, region_id)` - Prevent duplicate assignments

## Automatic Database Features

### 1. Computed Column: latlong
```sql
latlong TEXT GENERATED ALWAYS AS (
  CASE 
    WHEN latitude IS NOT NULL AND longitude IS NOT NULL 
    THEN (latitude::text || ',' || longitude::text) 
    ELSE NULL 
  END
) STORED
```

**Behavior:**
- Automatically generated from latitude and longitude
- Stored in the database (not computed on query)
- Updates automatically when lat/long change
- NULL if either coordinate is missing
- Format: "40.7128,-74.0060"

**Usage:**
```csharp
var site = new Site {
    Latitude = 40.7128m,
    Longitude = -74.0060m
};
// latlong will automatically be "40.7128,-74.0060"
```

### 2. Trigger: Auto-Update Timestamp
```sql
CREATE TRIGGER trg_set_updated_at_sites 
BEFORE UPDATE ON depotdirect.sites 
FOR EACH ROW 
EXECUTE FUNCTION depotdirect.fn_set_updated_at();
```

**Behavior:**
- Automatically sets `updated_at` to current timestamp on every update
- Triggered before the update is committed
- No need to set this field in your code

### 3. Trigger: Operating Hours Validation
```sql
CREATE TRIGGER trg_validate_operating_hours_sites 
BEFORE INSERT OR UPDATE ON depotdirect.sites 
FOR EACH ROW 
EXECUTE FUNCTION depotdirect.fn_validate_operating_hours();
```

**Expected Format:**
```json
{
  "mon": { "open": "08:00", "close": "18:00", "closed": false },
  "tue": { "open": "08:00", "close": "18:00", "closed": false },
  "wed": { "open": "08:00", "close": "18:00", "closed": false },
  "thu": { "open": "08:00", "close": "18:00", "closed": false },
  "fri": { "open": "08:00", "close": "18:00", "closed": false },
  "sat": { "closed": true },
  "sun": { "closed": true }
}
```

**Validation Rules:**
- Must be valid JSON
- Days: mon, tue, wed, thu, fri, sat, sun
- Each day can have: open, close, closed
- Times in HH:MM format
- If closed is true, open/close can be omitted

### 4. Trigger: Soft Delete
```sql
CREATE TRIGGER trg_soft_delete_sites 
BEFORE DELETE ON depotdirect.sites 
FOR EACH ROW 
EXECUTE FUNCTION depotdirect.fn_soft_delete();
```

**Behavior:**
- Intercepts DELETE operations
- Sets `deleted_at` timestamp instead of actually deleting
- Sets `active` to false
- Preserves all data for audit purposes

**Application Code:**
```csharp
// This will soft delete (set deleted_at)
_context.Sites.Remove(site);
await _context.SaveChangesAsync();

// To query active sites only
var activeSites = await _context.Sites
    .Where(s => s.DeletedAt == null)
    .ToListAsync();
```

### 5. Trigger: Region-Site Company Validation
```sql
CREATE TRIGGER trg_validate_region_site_company
BEFORE INSERT OR UPDATE ON depotdirect.region_sites
FOR EACH ROW
EXECUTE FUNCTION depotdirect.fn_validate_region_site_company();
```

**Behavior:**
- Validates that region and site belong to the same company
- Checks: region.company_id == site.company_id
- Prevents cross-company assignments
- Raises exception if validation fails

## Constraints & Validation

### 1. Priority Check Constraint
```sql
CONSTRAINT sites_priority_chk CHECK (priority IN ('High','Medium','Low'))
```

**Valid Values:**
- "High"
- "Medium" (default)
- "Low"

**Error if violated:**
```
ERROR: new row for relation "sites" violates check constraint "sites_priority_chk"
```

### 2. Unique Site Code per Country
```sql
CONSTRAINT sites_country_code_uniq UNIQUE (country_id, site_code)
```

**Allows:**
- Country 1, Code "SITE001" ?
- Country 2, Code "SITE001" ?

**Prevents:**
- Country 1, Code "SITE001" (first)
- Country 1, Code "SITE001" (duplicate) ?

**Error if violated:**
```
ERROR: duplicate key value violates unique constraint "sites_country_code_uniq"
```

### 3. Foreign Key Constraints

#### Country Reference
```sql
country_id INTEGER NOT NULL REFERENCES depotdirect.countries(id) ON DELETE RESTRICT
```

**Behavior:**
- Cannot delete a country if sites reference it
- Enforces referential integrity
- Prevents orphaned sites

#### Company Reference
```sql
company_id INTEGER NOT NULL REFERENCES depotdirect.companies(id) ON DELETE RESTRICT
```

**Behavior:**
- Cannot delete a company if sites reference it
- Enforces referential integrity
- Prevents orphaned sites

### 4. Unique Region-Site Assignment
```sql
UNIQUE (site_id, region_id)
```

**Prevents:**
- Assigning the same site to the same region twice

**Allows:**
- Same site to multiple different regions
- Same region to multiple different sites

## Data Types

### CITEXT for Email
```sql
email CITEXT
```

**Benefits:**
- Case-insensitive comparison
- "john@EXAMPLE.com" == "john@example.com"
- Stores original case
- Perfect for email addresses

### JSONB for Operating Hours & Metadata
```sql
operating_hours JSONB DEFAULT '{}'::jsonb
metadata JSONB DEFAULT '{}'::jsonb
```

**Benefits:**
- Flexible schema
- Indexable (can create GIN indexes if needed)
- Queryable with JSON operators
- Compressed storage
- Default to empty object

**Query Examples:**
```sql
-- Get sites open on Monday
SELECT * FROM sites 
WHERE operating_hours->'mon'->>'closed' = 'false';

-- Get sites with specific metadata
SELECT * FROM sites 
WHERE metadata->>'capacity' = '50000 sq ft';
```

### NUMERIC for Coordinates
```sql
latitude NUMERIC(10,7)
longitude NUMERIC(10,7)
```

**Precision:**
- 10 total digits
- 7 decimal places
- Range: -999.9999999 to 999.9999999
- Accurate to ~1.1 cm on Earth's surface

**Examples:**
- Valid: 40.7127837
- Valid: -74.0059413
- Invalid: 1234.5678 (too many integer digits)

## Default Values

| Field | Default | Notes |
|-------|---------|-------|
| active | true | New sites are active by default |
| priority | 'Medium' | Balanced default priority |
| delivery_stopped | false | Deliveries enabled by default |
| pumped_required | false | Standard delivery by default |
| operating_hours | '{}' | Empty JSON object |
| metadata | '{}' | Empty JSON object |
| created_at | NOW() | Automatic timestamp |
| updated_at | NOW() | Automatic timestamp |

## Cascade Behaviors

### Region Sites Mapping
```sql
site_id REFERENCES depotdirect.sites(id) ON DELETE CASCADE
region_id REFERENCES depotdirect.regions(id) ON DELETE CASCADE
```

**Behavior:**
- Deleting a site ? Deletes all its region mappings
- Deleting a region ? Deletes all its site mappings
- Maintains data consistency automatically

## Query Optimization

### Recommended Queries

#### Fast Lookup by Code
```sql
-- Uses idx_sites_site_code
SELECT * FROM sites WHERE site_code = 'NYC001';
```

#### Filter by Company
```sql
-- Uses idx_sites_company
SELECT * FROM sites WHERE company_id = 5 AND deleted_at IS NULL;
```

#### Search by Town
```sql
-- Uses idx_sites_town
SELECT * FROM sites WHERE town ILIKE '%london%' AND deleted_at IS NULL;
```

#### Sites for Region
```sql
-- Uses idx_region_sites_region
SELECT s.* 
FROM sites s
JOIN region_sites rs ON s.id = rs.site_id
WHERE rs.region_id = 10 
  AND s.deleted_at IS NULL 
  AND rs.deleted_at IS NULL;
```

## Migration Notes

The migration script (`0004_Sites.sql`) is **idempotent**, meaning:
- Can be run multiple times safely
- Uses `CREATE TABLE IF NOT EXISTS`
- Uses `CREATE INDEX IF NOT EXISTS`
- Checks for existing triggers before creating
- Safe for production environments

## Security

### Ownership
All objects owned by `depotdirect_user`:
```sql
ALTER TABLE depotdirect.sites OWNER TO depotdirect_user;
ALTER SEQUENCE depotdirect.sites_id_seq OWNER TO depotdirect_user;
```

### Permissions
The application user has:
- SELECT, INSERT, UPDATE, DELETE on tables
- USAGE, SELECT on sequences
- EXECUTE on trigger functions

## Best Practices

1. **Always Query with deleted_at Filter**
   ```sql
   WHERE deleted_at IS NULL
   ```

2. **Use Indexes for Searches**
   - Site code, town, company for filtering
   - Avoid full table scans

3. **Validate Operating Hours Format**
   - Follow the expected JSON structure
   - Database trigger will validate

4. **Respect Foreign Keys**
   - Ensure region exists before creating site
   - Check company/country match

5. **Use Transactions**
   - When creating site + region mapping
   - Ensures atomicity
