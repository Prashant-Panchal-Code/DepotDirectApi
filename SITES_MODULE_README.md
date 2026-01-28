# Sites Module - User API

## Overview
The Sites module has been successfully implemented for the User module in the DepotDirect API. This module allows users to create and manage sites with automatic company and country assignment based on the region.

## Key Features

### 1. Simplified Site Creation
When creating a site, users only need to provide:
- `site_code` - The unique code for the site
- `site_name` - The name of the site
- `region_id` - The region to which the site belongs

The system automatically:
- Fetches `company_id` from the region
- Fetches `country_id` from the region's company
- Creates the site with these values
- Creates a mapping between the site and the region

### 2. Complete Site Update
After initial creation, users can update all other fields:
- Shortcode
- Latitude & Longitude (auto-generates LatLong)
- Street address
- Postal code
- Town
- Active status
- Priority (High, Medium, Low)
- Contact person
- Phone
- Email
- Operating hours (JSON)
- Depot ID
- Delivery stopped flag
- Pumped required flag
- Metadata (JSON)

## Database Tables

### Sites Table
- **Primary fields**: site_code, site_name
- **Location**: latitude, longitude, latlong (computed), street, postal_code, town
- **Status**: active, priority
- **Contact**: contact_person, phone, email
- **Operations**: operating_hours, depot_id, delivery_stopped, pumped_required
- **References**: country_id, company_id
- **Audit**: metadata, created_by, last_updated_by, created_at, updated_at, deleted_at

### Region_Sites Mapping Table
- Links sites to regions
- Allows a site to belong to multiple regions
- Optional site_code override per region
- Supports soft delete

## API Endpoints

### User Module Endpoints (`/api/user/sites`)

#### GET Endpoints
- `GET /api/user/sites` - Get all sites
- `GET /api/user/sites/{id}` - Get site by ID
- `GET /api/user/sites/by-company/{companyId}` - Get sites by company
- `GET /api/user/sites/by-country/{countryId}` - Get sites by country
- `GET /api/user/sites/by-region/{regionId}` - Get sites by region
- `GET /api/user/sites/search?query={searchTerm}` - Search sites by code, name, or town
- `GET /api/user/sites/{id}/exists` - Check if site exists
- `GET /api/user/sites/{siteId}/regions/{regionId}/exists` - Check site-region assignment

#### POST Endpoints
- `POST /api/user/sites` - Create a new site
  ```json
  {
    "siteCode": "SITE001",
    "siteName": "Main Distribution Center",
    "regionId": 1
  }
  ```
- `POST /api/user/sites/{siteId}/regions` - Assign a region to a site
  ```json
  {
    "regionId": 2,
    "siteCode": "ALT_CODE",
    "metadata": {}
  }
  ```

#### PUT Endpoints
- `PUT /api/user/sites/{id}` - Update site details
  ```json
  {
    "siteName": "Updated Site Name",
    "shortcode": "MDC",
    "latitude": 51.5074,
    "longitude": -0.1278,
    "street": "123 Main Street",
    "postalCode": "SW1A 1AA",
    "town": "London",
    "active": true,
    "priority": "High",
    "contactPerson": "John Doe",
    "phone": "+44 20 1234 5678",
    "email": "site@example.com",
    "operatingHours": {
      "mon": { "open": "08:00", "close": "18:00" },
      "tue": { "open": "08:00", "close": "18:00" }
    },
    "metadata": {}
  }
  ```

#### DELETE Endpoints
- `DELETE /api/user/sites/{id}` - Delete site (soft delete)
- `DELETE /api/user/sites/{siteId}/regions/{regionId}` - Remove region from site

## Data Models

### CreateSiteDto
```csharp
{
  "siteCode": "string (required, max 100)",
  "siteName": "string (required, max 255)",
  "regionId": "integer (required)"
}
```

### UpdateSiteDto
All fields are optional - update only what you need:
```csharp
{
  "siteCode": "string (max 100)",
  "siteName": "string (max 255)",
  "shortcode": "string (max 50)",
  "latitude": "decimal (-90 to 90)",
  "longitude": "decimal (-180 to 180)",
  "street": "string (max 500)",
  "postalCode": "string (max 20)",
  "town": "string (max 100)",
  "active": "boolean",
  "priority": "string (High|Medium|Low)",
  "contactPerson": "string (max 100)",
  "phone": "string (max 50)",
  "email": "string (email, max 255)",
  "operatingHours": "json",
  "depotId": "integer",
  "deliveryStopped": "boolean",
  "pumpedRequired": "boolean",
  "metadata": "json"
}
```

### SiteResponseDto
Complete site information including:
- All site fields
- Country information (nested)
- Company information (nested)
- List of associated regions (nested)

### SiteListItemDto
Simplified site information for list views:
- Basic site details (code, name, town)
- Status (active, priority)
- Company and country names
- Timestamps

## Validation Rules

1. **Site Code Uniqueness**: Site code must be unique within a country
2. **Region Validation**: Region must exist and be active
3. **Company Matching**: Site automatically gets company from region
4. **Country Matching**: Site automatically gets country from region's company
5. **Priority Values**: Must be one of: High, Medium, Low
6. **Latitude Range**: -90 to 90
7. **Longitude Range**: -180 to 180
8. **Email Format**: Must be a valid email address

## Database Constraints

The SQL migration includes:
- Unique constraint on (country_id, site_code)
- Foreign key constraints to countries and companies
- Check constraint on priority values
- Soft delete support (deleted_at)
- Automatic timestamp triggers (created_at, updated_at)
- Operating hours validation trigger
- Computed latlong field

## Usage Example

### Creating a Site
```http
POST /api/user/sites
Authorization: Bearer {token}
Content-Type: application/json

{
  "siteCode": "LON001",
  "siteName": "London Main Depot",
  "regionId": 5
}
```

Response:
```json
{
  "id": 123,
  "siteCode": "LON001",
  "siteName": "London Main Depot",
  "companyId": 10,
  "countryId": 3,
  "active": true,
  "priority": "Medium",
  "company": {
    "id": 10,
    "name": "Acme Ltd"
  },
  "country": {
    "id": 3,
    "name": "United Kingdom"
  },
  "regions": [
    {
      "id": 5,
      "name": "South East Region"
    }
  ],
  "createdAt": "2024-01-15T10:30:00Z"
}
```

### Updating a Site
```http
PUT /api/user/sites/123
Authorization: Bearer {token}
Content-Type: application/json

{
  "latitude": 51.5074,
  "longitude": -0.1278,
  "street": "123 High Street",
  "postalCode": "E1 6AN",
  "town": "London",
  "priority": "High",
  "contactPerson": "Jane Smith",
  "phone": "+44 20 7946 0958",
  "email": "london.depot@acme.com"
}
```

## Files Created

1. **Models/Entities/Site.cs** - Site entity
2. **Models/Entities/RegionSite.cs** - Region-Site mapping entity
3. **Models/DTOs/SiteDtos.cs** - All DTOs for sites
4. **Repositories/ISiteRepository.cs** - Site repository interface
5. **Repositories/SiteRepository.cs** - Site repository implementation
6. **Controllers/User/SitesController.cs** - User sites controller

## Files Modified

1. **Data/DepotDirectDbContext.cs** - Added Sites and RegionSites DbSets
2. **Extensions/ServiceExtensions.cs** - Registered ISiteRepository

## Notes

- All operations require authentication (JWT token)
- Soft delete is implemented (deleted_at field)
- Audit fields track who created/updated records
- The latlong field is auto-generated in the database
- Operating hours use JSONB format for flexibility
- Priority defaults to "Medium" if not specified
- All endpoints follow the existing API patterns

## Next Steps

To use this module:
1. The database migration (0004_Sites.sql) should already be applied
2. Test the endpoints with a valid JWT token
3. Create sites by providing minimal information
4. Update sites with additional details as needed
