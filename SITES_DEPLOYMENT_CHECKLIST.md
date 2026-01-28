# Sites Module - Deployment Checklist

## ? Pre-Deployment Verification

### Code Compilation
- [x] Build successful with no errors
- [x] All dependencies resolved
- [x] Repository registered in DI container
- [x] DbContext updated with new entities
- [x] Controllers registered and accessible

### Files Created
- [x] Site.cs entity model
- [x] RegionSite.cs entity model
- [x] SiteDtos.cs with all required DTOs
- [x] ISiteRepository.cs interface
- [x] SiteRepository.cs implementation
- [x] SitesController.cs user controller

### Files Modified
- [x] DepotDirectDbContext.cs - Added Sites and RegionSites
- [x] ServiceExtensions.cs - Registered ISiteRepository

### Database Migration
- [x] Migration file exists: db/migrations/0004_Sites.sql
- [ ] Migration applied to development database
- [ ] Migration applied to test database
- [ ] Migration applied to production database

---

## ?? Deployment Steps

### Step 1: Database Migration
```bash
# Connect to your PostgreSQL database
psql -U depotdirect_user -d depotdirect_db -f DepotDirectApi/db/migrations/0004_Sites.sql

# Verify tables created
psql -U depotdirect_user -d depotdirect_db -c "\dt depotdirect.sites"
psql -U depotdirect_user -d depotdirect_db -c "\dt depotdirect.region_sites"
```

Expected output:
```
Schema       | Name         | Type  | Owner
depotdirect  | sites        | table | depotdirect_user
depotdirect  | region_sites | table | depotdirect_user
```

### Step 2: Verify Database Structure
```sql
-- Check sites table
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_schema = 'depotdirect' AND table_name = 'sites';

-- Check region_sites table
SELECT column_name, data_type, is_nullable 
FROM information_schema.columns 
WHERE table_schema = 'depotdirect' AND table_name = 'region_sites';

-- Check constraints
SELECT constraint_name, constraint_type 
FROM information_schema.table_constraints 
WHERE table_schema = 'depotdirect' AND table_name IN ('sites', 'region_sites');

-- Check triggers
SELECT trigger_name, event_manipulation, event_object_table 
FROM information_schema.triggers 
WHERE event_object_schema = 'depotdirect' 
  AND event_object_table IN ('sites', 'region_sites');
```

### Step 3: Build & Deploy Application
```bash
# Clean build
dotnet clean
dotnet build --configuration Release

# Run tests (if applicable)
dotnet test

# Publish
dotnet publish --configuration Release --output ./publish

# Deploy to server
# (Your deployment process here)
```

### Step 4: Verify API Endpoints
Use Swagger UI or test manually:

1. Start the application
2. Navigate to `/swagger` (if enabled)
3. Find "User/Sites" section
4. Verify all endpoints are listed

Expected endpoints:
- GET /api/user/sites
- GET /api/user/sites/{id}
- GET /api/user/sites/by-company/{companyId}
- GET /api/user/sites/by-country/{countryId}
- GET /api/user/sites/by-region/{regionId}
- GET /api/user/sites/search
- POST /api/user/sites
- PUT /api/user/sites/{id}
- DELETE /api/user/sites/{id}
- POST /api/user/sites/{siteId}/regions
- DELETE /api/user/sites/{siteId}/regions/{regionId}
- GET /api/user/sites/{id}/exists
- GET /api/user/sites/{siteId}/regions/{regionId}/exists

---

## ?? Testing Checklist

### Manual Testing

#### Test 1: Create Site (Happy Path)
```http
POST /api/user/sites
Authorization: Bearer {valid_token}
Content-Type: application/json

{
  "siteCode": "TEST001",
  "siteName": "Test Site",
  "regionId": {existing_region_id}
}
```

**Expected Result:**
- Status: 201 Created
- Response includes site with auto-populated company_id and country_id
- Region mapping created

#### Test 2: Duplicate Site Code in Same Country
```http
POST /api/user/sites
{
  "siteCode": "TEST001",  // Same as Test 1
  "siteName": "Another Site",
  "regionId": {existing_region_id}  // Same country
}
```

**Expected Result:**
- Status: 400 Bad Request
- Error message about duplicate site code

#### Test 3: Update Site
```http
PUT /api/user/sites/{site_id_from_test_1}
Authorization: Bearer {valid_token}
Content-Type: application/json

{
  "latitude": 51.5074,
  "longitude": -0.1278,
  "town": "London",
  "priority": "High"
}
```

**Expected Result:**
- Status: 200 OK
- Site updated with new values
- latlong computed automatically

#### Test 4: Get Site by ID
```http
GET /api/user/sites/{site_id}
Authorization: Bearer {valid_token}
```

**Expected Result:**
- Status: 200 OK
- Full site details returned
- Includes company and country nested objects
- Includes regions array

#### Test 5: Search Sites
```http
GET /api/user/sites/search?query=london
Authorization: Bearer {valid_token}
```

**Expected Result:**
- Status: 200 OK
- Sites matching "london" in code, name, or town

#### Test 6: Invalid Priority
```http
PUT /api/user/sites/{site_id}
{
  "priority": "Critical"  // Invalid (not High/Medium/Low)
}
```

**Expected Result:**
- Status: 400 Bad Request
- Validation error

#### Test 7: Assign Additional Region
```http
POST /api/user/sites/{site_id}/regions
{
  "regionId": {another_region_id}
}
```

**Expected Result:**
- Status: 200 OK
- Region-site mapping created

#### Test 8: Soft Delete
```http
DELETE /api/user/sites/{site_id}
Authorization: Bearer {valid_token}
```

**Expected Result:**
- Status: 204 No Content
- Site not returned in GET requests
- Still exists in database with deleted_at set

#### Test 9: Unauthorized Access
```http
GET /api/user/sites
# No Authorization header
```

**Expected Result:**
- Status: 401 Unauthorized

#### Test 10: Invalid Region
```http
POST /api/user/sites
{
  "siteCode": "INVALID001",
  "siteName": "Invalid Site",
  "regionId": 999999  // Non-existent
}
```

**Expected Result:**
- Status: 400 Bad Request
- Error message about invalid region

---

## ?? Post-Deployment Verification

### Database Checks
- [ ] Sites table exists and is accessible
- [ ] Region_sites table exists and is accessible
- [ ] All indexes created
- [ ] All triggers active
- [ ] Foreign key constraints working
- [ ] Check constraints working

### Application Checks
- [ ] Application starts without errors
- [ ] Swagger documentation includes new endpoints
- [ ] Authentication working
- [ ] Logging working
- [ ] All endpoints responding

### Functional Checks
- [ ] Can create site with minimal info
- [ ] Company and country auto-populated
- [ ] Can update site with full details
- [ ] Search functionality working
- [ ] Filter by company/country/region working
- [ ] Region assignment working
- [ ] Soft delete working
- [ ] Validation working (unique codes, priority, etc.)

### Performance Checks
- [ ] Site creation < 500ms
- [ ] Site retrieval < 200ms
- [ ] Search performance acceptable
- [ ] No N+1 query issues
- [ ] Indexes being used

---

## ?? Troubleshooting Guide

### Issue: Build Error - Site entity not found
**Solution:** Ensure you've added the files and they're included in the project:
```bash
dotnet build -v detailed
```

### Issue: Database constraint violation
**Solution:** Check that the migration ran completely:
```sql
SELECT * FROM pg_constraint WHERE conrelid = 'depotdirect.sites'::regclass;
```

### Issue: Can't create site - Region not found
**Solution:** Verify regions exist in the database:
```sql
SELECT id, name, company_id FROM depotdirect.regions WHERE deleted_at IS NULL;
```

### Issue: Foreign key constraint error
**Solution:** Check that referenced entities exist:
```sql
-- Check if region exists and has company
SELECT r.id, r.name, r.company_id, c.id, c.name, c.country_id
FROM depotdirect.regions r
LEFT JOIN depotdirect.companies c ON r.company_id = c.id
WHERE r.id = {your_region_id};
```

### Issue: 401 Unauthorized on all requests
**Solution:** Verify JWT token is valid and not expired. Get new token from /api/auth/login

### Issue: Duplicate key violation on site_code
**Solution:** This is correct behavior - site codes must be unique per country. Use different code or different region (different country).

### Issue: Priority validation error
**Solution:** Ensure priority is exactly "High", "Medium", or "Low" (case-sensitive)

### Issue: Operating hours validation error
**Solution:** Check JSON format matches expected structure:
```json
{
  "mon": { "open": "08:00", "close": "18:00" },
  "tue": { "open": "08:00", "close": "18:00" }
}
```

---

## ?? Monitoring Recommendations

### Logs to Monitor
- Site creation events
- Failed validation attempts
- Database constraint violations
- Performance slow queries
- Authentication failures

### Metrics to Track
- Sites created per day
- Average site creation time
- Search query performance
- Failed creation attempts
- Popular search terms

### Alerts to Set Up
- High error rate on site creation
- Database connection issues
- Slow query performance (> 1 second)
- Unusual spike in site deletions

---

## ?? Reference Documentation

- **SITES_MODULE_README.md** - Technical documentation
- **SITES_API_EXAMPLES.md** - API usage examples
- **SITES_DATABASE_SCHEMA.md** - Database details
- **SITES_IMPLEMENTATION_SUMMARY.md** - Overview

---

## ? Sign-Off

### Development
- [ ] Code reviewed
- [ ] Build successful
- [ ] Unit tests pass
- [ ] Manual testing complete
- [ ] Documentation complete

### Staging
- [ ] Database migration applied
- [ ] Application deployed
- [ ] Smoke tests pass
- [ ] Integration tests pass
- [ ] UAT approved

### Production
- [ ] Database migration applied
- [ ] Application deployed
- [ ] Health check pass
- [ ] Monitoring configured
- [ ] Rollback plan ready

---

**Deployment Date**: _______________  
**Deployed By**: _______________  
**Approved By**: _______________  
**Version**: 1.0
