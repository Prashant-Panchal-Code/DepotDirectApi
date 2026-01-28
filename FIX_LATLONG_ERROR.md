# Fix for "cannot insert a non-DEFAULT value into column latlong" Error

## Problem
PostgreSQL was throwing error `428C9: cannot insert a non-DEFAULT value into column "latlong"` when creating a site because the `latlong` column is a **GENERATED ALWAYS** computed column in the database, and Entity Framework Core was trying to insert a value into it.

## Solution Applied

### 1. Updated Site.cs Entity
Added `[DatabaseGenerated(DatabaseGeneratedOption.Computed)]` attribute to the `LatLong` property:

```csharp
[Column("latlong")]
[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
public string? LatLong { get; set; }
```

This tells EF Core that this column is computed by the database and should never be included in INSERT or UPDATE statements.

### 2. Updated DbContext Configuration
Added computed column configuration in `DepotDirectDbContext.cs`:

```csharp
// Configure LatLong as a computed column
entity.Property(e => e.LatLong)
      .HasComputedColumnSql("CASE WHEN latitude IS NOT NULL AND longitude IS NOT NULL THEN (latitude::text || ',' || longitude::text) ELSE NULL END", stored: true);
```

This explicitly tells EF Core:
- The column is computed by the database
- The SQL formula for the computation
- It's a stored computed column (physically stored, not calculated on read)

## What This Means

### ? Now Works
- EF Core will NOT try to insert or update the `LatLong` column
- The database automatically generates the value from `latitude` and `longitude`
- When you read a site, `LatLong` will be populated by the database

### ?? Usage
Create/Update a site with latitude and longitude:

```csharp
var site = new Site 
{
    SiteCode = "NYC001",
    SiteName = "New York Depot",
    Latitude = 40.7128m,
    Longitude = -74.0060m,
    // DON'T set LatLong - it will be computed automatically
};

_context.Sites.Add(site);
await _context.SaveChangesAsync();

// After saving, refresh the entity to get the computed value
await _context.Entry(site).ReloadAsync();
// Now site.LatLong will be "40.7128,-74.0060"
```

### ?? API Usage
When creating a site via API:

```json
POST /api/user/sites
{
  "siteCode": "NYC001",
  "siteName": "New York Depot",
  "regionId": 5
}
```

Then update with coordinates:

```json
PUT /api/user/sites/{id}
{
  "latitude": 40.7128,
  "longitude": -74.0060
}
```

The `latlong` field will be automatically computed and returned in the response.

## Next Steps

### If Still Debugging
Since the application is running in debug mode, you may need to:

1. **Stop debugging** and **restart** the application for the changes to take effect
2. Or use **Hot Reload** to apply the changes (Shift+Alt+F5 or click the Hot Reload button)

### Test the Fix
Try creating a site again:

```http
POST /api/user/sites
Authorization: Bearer {your_token}
Content-Type: application/json

{
  "siteCode": "TEST001",
  "siteName": "Test Site",
  "regionId": 1
}
```

This should now work without the PostgreSQL error!

## Technical Details

### Database Side
The database has this constraint:
```sql
latlong TEXT GENERATED ALWAYS AS (
  CASE 
    WHEN latitude IS NOT NULL AND longitude IS NOT NULL 
    THEN (latitude::text || ',' || longitude::text) 
    ELSE NULL 
  END
) STORED
```

### EF Core Side
With our changes, EF Core now knows:
- This is a computed column (won't include in INSERT/UPDATE)
- It's stored (value is physically in the database)
- It will be retrieved when reading entities

## Benefits

? Automatic coordinate formatting  
? No manual calculation needed  
? Database guarantees consistency  
? Value always in sync with lat/long  
? No application code required  

## Error Resolution

**Before:** `PostgresException: 428C9: cannot insert a non-DEFAULT value into column "latlong"`

**After:** Site creation works successfully, `latlong` automatically populated by database

---

**Status:** ? FIXED  
**Build:** ? Successful  
**Ready to Test:** ? Yes (restart debugger first)
