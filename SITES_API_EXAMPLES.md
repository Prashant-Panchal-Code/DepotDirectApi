# Sites API - Quick Start Examples

## Authentication
All requests require a JWT token. First, login to get your token:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "your_password"
}
```

Use the returned token in all subsequent requests:
```
Authorization: Bearer {your_token_here}
```

---

## 1. Create a Site (Minimal Required Fields)

### Request
```http
POST /api/user/sites
Authorization: Bearer {token}
Content-Type: application/json

{
  "siteCode": "NYC001",
  "siteName": "New York Main Distribution Center",
  "regionId": 10
}
```

### Response
```json
{
  "id": 42,
  "siteCode": "NYC001",
  "siteName": "New York Main Distribution Center",
  "shortcode": null,
  "latitude": null,
  "longitude": null,
  "latLong": null,
  "street": null,
  "postalCode": null,
  "town": null,
  "active": true,
  "priority": "Medium",
  "contactPerson": null,
  "phone": null,
  "email": null,
  "operatingHours": null,
  "depotId": null,
  "deliveryStopped": false,
  "pumpedRequired": false,
  "countryId": 1,
  "companyId": 5,
  "metadata": null,
  "createdBy": 123,
  "lastUpdatedBy": 123,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z",
  "deletedAt": null,
  "country": {
    "id": 1,
    "name": "United States",
    "isoCode": "US"
  },
  "company": {
    "id": 5,
    "name": "Acme Corporation",
    "companyCode": "ACME"
  },
  "regions": [
    {
      "id": 10,
      "name": "Northeast Region",
      "regionCode": "NE"
    }
  ]
}
```

**Note:** The `countryId` and `companyId` are automatically fetched from the region. You don't need to provide them!

---

## 2. Update Site with Full Details

### Request
```http
PUT /api/user/sites/42
Authorization: Bearer {token}
Content-Type: application/json

{
  "shortcode": "NYC",
  "latitude": 40.7128,
  "longitude": -74.0060,
  "street": "500 Industrial Parkway",
  "postalCode": "10001",
  "town": "New York",
  "priority": "High",
  "contactPerson": "John Smith",
  "phone": "+1 212-555-0100",
  "email": "nyc.depot@acme.com",
  "operatingHours": {
    "mon": { "open": "06:00", "close": "20:00", "closed": false },
    "tue": { "open": "06:00", "close": "20:00", "closed": false },
    "wed": { "open": "06:00", "close": "20:00", "closed": false },
    "thu": { "open": "06:00", "close": "20:00", "closed": false },
    "fri": { "open": "06:00", "close": "20:00", "closed": false },
    "sat": { "open": "08:00", "close": "16:00", "closed": false },
    "sun": { "closed": true }
  },
  "metadata": {
    "capacity": "50000 sq ft",
    "loadingDocks": 10,
    "refrigerated": true
  }
}
```

### Response
```json
{
  "id": 42,
  "siteCode": "NYC001",
  "siteName": "New York Main Distribution Center",
  "shortcode": "NYC",
  "latitude": 40.7128,
  "longitude": -74.0060,
  "latLong": "40.7128,-74.0060",
  "street": "500 Industrial Parkway",
  "postalCode": "10001",
  "town": "New York",
  "active": true,
  "priority": "High",
  "contactPerson": "John Smith",
  "phone": "+1 212-555-0100",
  "email": "nyc.depot@acme.com",
  "operatingHours": { ... },
  "metadata": { ... },
  "updatedAt": "2024-01-15T14:20:00Z",
  ...
}
```

---

## 3. Get All Sites

### Request
```http
GET /api/user/sites
Authorization: Bearer {token}
```

### Response
```json
[
  {
    "id": 42,
    "siteCode": "NYC001",
    "siteName": "New York Main Distribution Center",
    "town": "New York",
    "active": true,
    "priority": "High",
    "companyId": 5,
    "companyName": "Acme Corporation",
    "countryId": 1,
    "countryName": "United States",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T14:20:00Z"
  },
  ...
]
```

---

## 4. Get Site by ID (with full details)

### Request
```http
GET /api/user/sites/42
Authorization: Bearer {token}
```

### Response
Full SiteResponseDto with all fields, company, country, and regions included.

---

## 5. Search Sites

### Request
```http
GET /api/user/sites/search?query=new york
Authorization: Bearer {token}
```

Searches in: site_code, site_name, and town fields.

---

## 6. Get Sites by Region

### Request
```http
GET /api/user/sites/by-region/10
Authorization: Bearer {token}
```

Returns all sites assigned to region ID 10.

---

## 7. Get Sites by Company

### Request
```http
GET /api/user/sites/by-company/5
Authorization: Bearer {token}
```

Returns all sites belonging to company ID 5.

---

## 8. Get Sites by Country

### Request
```http
GET /api/user/sites/by-country/1
Authorization: Bearer {token}
```

Returns all sites in country ID 1.

---

## 9. Assign Additional Region to Site

If a site needs to serve multiple regions:

### Request
```http
POST /api/user/sites/42/regions
Authorization: Bearer {token}
Content-Type: application/json

{
  "regionId": 11,
  "siteCode": "NYC001-WEST"
}
```

### Response
```json
{
  "id": 100,
  "siteId": 42,
  "siteName": "New York Main Distribution Center",
  "siteCode": "NYC001",
  "regionId": 11,
  "regionName": "Western District",
  "regionSiteCode": "NYC001-WEST",
  "createdBy": 123,
  "createdAt": "2024-01-15T15:00:00Z"
}
```

---

## 10. Check if Site Exists

### Request
```http
GET /api/user/sites/42/exists
Authorization: Bearer {token}
```

### Response
```json
{
  "exists": true
}
```

---

## 11. Delete a Site (Soft Delete)

### Request
```http
DELETE /api/user/sites/42
Authorization: Bearer {token}
```

### Response
```
204 No Content
```

The site is not physically deleted, just marked with `deleted_at` timestamp.

---

## 12. Remove Site from Region

### Request
```http
DELETE /api/user/sites/42/regions/11
Authorization: Bearer {token}
```

### Response
```
204 No Content
```

---

## Error Responses

### 400 Bad Request
```json
{
  "message": "Site code 'NYC001' already exists in this country."
}
```

### 401 Unauthorized
```json
{
  "message": "Authorization header is missing or invalid"
}
```

### 404 Not Found
```json
{
  "message": "Site not found"
}
```

### 500 Internal Server Error
```json
{
  "message": "Internal server error",
  "details": "Error description..."
}
```

---

## Common Use Cases

### 1. Quick Site Setup
Create a site with just code, name, and region. Update details later.

### 2. Full Site Setup
Create first, then immediately update with all contact and operational details.

### 3. Multi-Region Sites
Create site for one region, then assign it to additional regions as needed.

### 4. Search and Filter
Use search endpoint for quick lookups, or filter by company/country/region for organized views.

---

## Business Logic

1. **Automatic Company & Country**: You only specify the region. The system automatically:
   - Gets the company from the region
   - Gets the country from the company
   - This ensures data consistency

2. **Site Code Uniqueness**: Site codes must be unique per country (not globally). This allows:
   - Different countries to use the same codes
   - Each country to have its own numbering scheme

3. **Priority System**: Sites have three priority levels:
   - High: Critical distribution centers
   - Medium: Regular sites (default)
   - Low: Secondary or backup locations

4. **Soft Delete**: Deleted sites remain in database for audit purposes. They:
   - Don't appear in normal queries
   - Can be restored if needed
   - Maintain referential integrity

---

## Tips

1. Always create sites with minimal info first, then update
2. Use region filtering to see all sites in a specific area
3. Search is case-insensitive and searches multiple fields
4. Operating hours use flexible JSON format
5. Metadata field can store any custom JSON data
6. All timestamps are in UTC
