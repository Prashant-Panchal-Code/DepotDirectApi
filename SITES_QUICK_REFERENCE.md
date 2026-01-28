# Sites Module - Quick Reference Card

## ?? Quick Start

### Create a Site (3 fields only!)
```json
POST /api/user/sites
{
  "siteCode": "NYC001",
  "siteName": "New York Depot",
  "regionId": 5
}
```
? **Company & Country auto-populated from region!**

### Update Site (all other fields)
```json
PUT /api/user/sites/42
{
  "latitude": 40.7128,
  "longitude": -74.0060,
  "town": "New York",
  "priority": "High"
}
```

---

## ?? Key Endpoints

| What | Method | URL |
|------|--------|-----|
| List all sites | GET | `/api/user/sites` |
| Get one site | GET | `/api/user/sites/{id}` |
| Create site | POST | `/api/user/sites` |
| Update site | PUT | `/api/user/sites/{id}` |
| Delete site | DELETE | `/api/user/sites/{id}` |
| Search | GET | `/api/user/sites/search?query=london` |
| By region | GET | `/api/user/sites/by-region/{id}` |
| By company | GET | `/api/user/sites/by-company/{id}` |

---

## ?? Site Fields at a Glance

### Required on Create
- ? `siteCode` - Unique per country
- ? `siteName` - Site display name
- ? `regionId` - Which region owns this site

### Auto-Populated
- ?? `companyId` - From region
- ?? `countryId` - From region's company
- ?? `latlong` - From lat + long
- ?? `active` - Defaults to true
- ?? `priority` - Defaults to "Medium"

### Optional (update later)
- ?? Location: `latitude`, `longitude`, `street`, `postalCode`, `town`
- ?? Contact: `contactPerson`, `phone`, `email`
- ? Operations: `operatingHours` (JSON)
- ??? Details: `shortcode`, `priority`, `depotId`
- ?? Flags: `deliveryStopped`, `pumpedRequired`
- ?? Custom: `metadata` (JSON)

---

## ?? Priority Values

| Value | Use Case |
|-------|----------|
| `High` | Critical distribution centers |
| `Medium` | Regular sites (default) |
| `Low` | Secondary/backup locations |

---

## ?? Common Validations

| Field | Rule |
|-------|------|
| `siteCode` | Unique per country, max 100 chars |
| `siteName` | Required, max 255 chars |
| `priority` | Must be: High, Medium, or Low |
| `latitude` | -90 to 90 |
| `longitude` | -180 to 180 |
| `email` | Valid email format |

---

## ?? Authentication

All endpoints require JWT token:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

Get token from:
```http
POST /api/auth/login
{ "email": "user@example.com", "password": "pwd" }
```

---

## ?? Response Types

### SiteListItemDto (for lists)
```json
{
  "id": 42,
  "siteCode": "NYC001",
  "siteName": "New York Depot",
  "town": "New York",
  "active": true,
  "priority": "High",
  "companyName": "Acme Corp",
  "countryName": "United States"
}
```

### SiteResponseDto (for details)
```json
{
  "id": 42,
  "siteCode": "NYC001",
  "siteName": "New York Depot",
  "latitude": 40.7128,
  "longitude": -74.0060,
  "latlong": "40.7128,-74.0060",
  "town": "New York",
  "active": true,
  "priority": "High",
  "company": { ... },
  "country": { ... },
  "regions": [ ... ]
}
```

---

## ??? Multi-Region Sites

Assign site to additional regions:
```http
POST /api/user/sites/42/regions
{ "regionId": 11 }
```

Remove site from region:
```http
DELETE /api/user/sites/42/regions/11
```

---

## ?? Search & Filter

### Search (code, name, or town)
```http
GET /api/user/sites/search?query=depot
```

### Filter by Region
```http
GET /api/user/sites/by-region/5
```

### Filter by Company
```http
GET /api/user/sites/by-company/10
```

### Filter by Country
```http
GET /api/user/sites/by-country/1
```

---

## ? Operating Hours Format

```json
{
  "operatingHours": {
    "mon": { "open": "08:00", "close": "18:00" },
    "tue": { "open": "08:00", "close": "18:00" },
    "wed": { "open": "08:00", "close": "18:00" },
    "thu": { "open": "08:00", "close": "18:00" },
    "fri": { "open": "08:00", "close": "18:00" },
    "sat": { "open": "09:00", "close": "14:00" },
    "sun": { "closed": true }
  }
}
```

---

## ??? Soft Delete

```http
DELETE /api/user/sites/42
```

- Site not deleted from database
- Sets `deleted_at` timestamp
- Sets `active` to false
- Site won't appear in normal queries
- Can be restored if needed

---

## ? Common Errors

| Status | Error | Solution |
|--------|-------|----------|
| 400 | Site code exists | Use different code or region |
| 400 | Region not found | Verify region ID exists |
| 400 | Invalid priority | Use High, Medium, or Low |
| 401 | Unauthorized | Include valid JWT token |
| 404 | Site not found | Check site ID is correct |

---

## ?? Database Tables

### sites (27 columns)
Main site data with all details

### region_sites (mapping)
Links sites to regions (many-to-many)

---

## ?? Typical Workflow

1. **Create** site with minimal info ? `POST /api/user/sites`
2. **Get** site details to verify ? `GET /api/user/sites/{id}`
3. **Update** with full details ? `PUT /api/user/sites/{id}`
4. **Assign** to more regions if needed ? `POST /api/user/sites/{id}/regions`
5. **Search/Filter** to find sites ? `GET /api/user/sites/search`

---

## ?? Related Endpoints

| Need | Endpoint |
|------|----------|
| List regions | GET `/api/admin/regions` |
| List companies | GET `/api/admin/companies` |
| List countries | GET `/api/admin/countries` |

---

## ?? Status Codes

| Code | Meaning | When |
|------|---------|------|
| 200 | OK | Successful GET/PUT |
| 201 | Created | Successful POST |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Validation error |
| 401 | Unauthorized | Missing/invalid token |
| 404 | Not Found | Site doesn't exist |
| 500 | Server Error | Something went wrong |

---

## ??? Data Flow

```
User Input
   ?
(site_code, site_name, region_id)
   ?
API validates region exists
   ?
Fetch region ? company_id, country_id
   ?
Create site with all IDs
   ?
Create region-site mapping
   ?
Return complete site details
```

---

## ?? Example: Complete Site Setup

### Step 1: Create
```http
POST /api/user/sites
{
  "siteCode": "LON001",
  "siteName": "London Main Depot",
  "regionId": 5
}
```
Response: Site ID = 100

### Step 2: Update
```http
PUT /api/user/sites/100
{
  "latitude": 51.5074,
  "longitude": -0.1278,
  "street": "123 Industrial Way",
  "postalCode": "E1 6AN",
  "town": "London",
  "priority": "High",
  "contactPerson": "Jane Smith",
  "phone": "+44 20 1234 5678",
  "email": "london@acme.com"
}
```

### Step 3: Done! ??
Your site is fully configured and ready to use.

---

## ?? Developer Notes

- Built with .NET 9
- Uses Entity Framework Core
- PostgreSQL database
- JWT authentication
- Repository pattern
- Async/await throughout
- Comprehensive logging
- Soft delete support

---

## ?? Full Documentation

- **SITES_API_EXAMPLES.md** - Full examples with responses
- **SITES_MODULE_README.md** - Complete technical docs
- **SITES_DATABASE_SCHEMA.md** - Database details
- **SITES_IMPLEMENTATION_SUMMARY.md** - Implementation overview

---

**Quick Tip:** Start simple! Create sites with just 3 fields, then update later with full details. ??
