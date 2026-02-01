# Depot-Site Routes CRUD API Documentation

This document describes the CRUD operations for managing routes between depots and sites in the DepotDirect API.

## Overview

The depot-site routes API manages the network topology and valid routes from depots to sites. Each route defines:

- **Distance**: Physical distance in kilometers
- **Travel Time**: Standard one-way trip time in minutes (loaded)
- **Return Time**: Optional faster return time for empty trucks
- **Active Status**: Whether the route is currently drivable
- **Primary Flag**: Indicates preferred depot for a site
- **Transport Rate**: Optional specific cost for this route

## Base URL

All endpoints are under: `/api/user/depot-sites`

## Authentication

All endpoints require JWT Bearer token authentication.

## Important Behavior: Soft Delete Recovery

When creating a new depot-site route, if a soft-deleted route with the same depot-site combination already exists, the system will **automatically reactivate** the existing route with the new data instead of creating a duplicate. This prevents unique constraint violations and maintains data consistency.

## Endpoints

### 1. Get All Routes

```http
GET /api/user/depot-sites
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": 1,
    "depotId": 1,
    "depotCode": "DEP001",
    "depotName": "Main Depot",
    "siteId": 1,
    "siteCode": "SITE001",
    "siteName": "Customer Site A",
    "distanceKm": 15.50,
    "travelTimeMins": 25,
    "returnTimeMins": 20,
    "active": true,
    "isPrimary": true,
    "transportRate": 2.50,
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
]
```

### 2. Get Route by ID

```http
GET /api/user/depot-sites/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "id": 1,
  "depotId": 1,
  "siteId": 1,
  "distanceKm": 15.50,
  "travelTimeMins": 25,
  "returnTimeMins": 20,
  "active": true,
  "isPrimary": true,
  "transportRate": 2.50,
  "metadata": {},
  "createdBy": 1,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z",
  "deletedAt": null,
  "depot": {
    "id": 1,
    "depotCode": "DEP001",
    "depotName": "Main Depot",
    "town": "Industrial City",
    "active": true,
    "priority": "High",
    "companyId": 1,
    "companyName": "ACME Corp"
  },
  "site": {
    "id": 1,
    "siteCode": "SITE001",
    "siteName": "Customer Site A",
    "town": "Customer City",
    "active": true,
    "priority": "High",
    "companyId": 1,
    "companyName": "ACME Corp"
  }
}
```

### 3. Create New Route

```http
POST /api/user/depot-sites
Authorization: Bearer {token}
Content-Type: application/json

{
  "depotId": 1,
  "siteId": 2,
  "distanceKm": 22.75,
  "travelTimeMins": 35,
  "returnTimeMins": 30,
  "active": true,
  "isPrimary": false,
  "transportRate": 3.25
}
```

**Response:** 201 Created
```json
{
  "id": 2,
  "depotId": 1,
  "siteId": 2,
  "distanceKm": 22.75,
  "travelTimeMins": 35,
  "returnTimeMins": 30,
  "active": true,
  "isPrimary": false,
  "transportRate": 3.25,
  "metadata": {},
  "createdBy": 1,
  "createdAt": "2024-01-15T11:00:00Z",
  "updatedAt": "2024-01-15T11:00:00Z",
  "deletedAt": null,
  "depot": { "..." },
  "site": { "..." }
}
```

**Smart Recovery Behavior:**
If you try to create a route that was previously deleted:
1. **First deletion:** Route is soft-deleted (deletedAt is set)
2. **Subsequent creation:** Same depot-site combination reactivates the existing route
3. **New data:** All fields are updated with the new values provided
4. **Reactivation:** deletedAt is set to null, making the route active again

**Validation Rules:**
- `depotId`: Required, must exist
- `siteId`: Required, must exist
- `distanceKm`: Required, must be > 0
- `travelTimeMins`: Required, must be >= 1
- `returnTimeMins`: Optional, must be >= 0 if provided
- Depot and Site must belong to the same company
- Route combination must be unique (considering active routes only)

### 4. Update Route

```http
PUT /api/user/depot-sites/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "distanceKm": 18.25,
  "travelTimeMins": 28,
  "transportRate": 2.75,
  "active": false
}
```

**Response:** 200 OK
```json
{
  "id": 1,
  "depotId": 1,
  "siteId": 1,
  "distanceKm": 18.25,
  "travelTimeMins": 28,
  "returnTimeMins": 20,
  "active": false,
  "isPrimary": true,
  "transportRate": 2.75,
  "metadata": {},
  "createdBy": 1,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T11:15:00Z",
  "deletedAt": null,
  "depot": { "..." },
  "site": { "..." }
}
```

### 5. Delete Route (Soft Delete)

```http
DELETE /api/user/depot-sites/{id}
Authorization: Bearer {token}
```

**Response:** 204 No Content

**Note:** This is a soft delete operation. The route is marked as deleted but remains in the database. If you create the same depot-site combination again later, the route will be reactivated automatically.

### 6. Check Route Exists

```http
GET /api/user/depot-sites/{id}/exists
Authorization: Bearer {token}
```

**Response:**
```json
true
```

## Additional Endpoints

### Get Routes by Depot

```http
GET /api/user/depot-sites/depot/{depotId}
Authorization: Bearer {token}
```

Returns all routes from a specific depot.

### Get Routes by Site

```http
GET /api/user/depot-sites/site/{siteId}
Authorization: Bearer {token}
```

Returns all routes to a specific site.

### Get Active Routes Only

```http
GET /api/user/depot-sites/active
Authorization: Bearer {token}
```

Returns only routes where `active = true`.

### Get Primary Routes Only

```http
GET /api/user/depot-sites/primary
Authorization: Bearer {token}
```

Returns only routes where `isPrimary = true`.

### Get Routes by Company

```http
GET /api/user/depot-sites/company/{companyId}
Authorization: Bearer {token}
```

Returns all routes for a specific company.

### Search Routes

```http
GET /api/user/depot-sites/search?query=main
Authorization: Bearer {token}
```

Searches routes by depot code, depot name, site code, or site name.

### Set Primary Depot for Site

```http
PUT /api/user/depot-sites/site/{siteId}/primary-depot/{depotId}
Authorization: Bearer {token}
```

Sets a specific depot as the primary depot for a site. This automatically removes the primary flag from any other depots serving this site.

**Response:** 200 OK
```json
{
  "id": 1,
  "depotId": 2,
  "siteId": 1,
  "isPrimary": true,
  "..."
}
```

## Error Responses

### 400 Bad Request
```json
{
  "error": "Validation failed",
  "details": {
    "distanceKm": ["Distance must be greater than 0"],
    "travelTimeMins": ["Travel time must be at least 1 minute"]
  }
}
```

### 404 Not Found
```json
{
  "error": "Depot-site route with ID 999 not found"
}
```

### 422 Unprocessable Entity
```json
{
  "error": "Depot and Site must belong to the same company."
}
```

## Business Rules

1. **Company Validation**: Depots and Sites must belong to the same company
2. **Unique Routes**: Each depot-site combination can only have one active route
3. **Soft Delete Recovery**: Deleted routes are automatically reactivated when recreated
4. **Primary Routes**: Only one depot per site can be marked as primary
5. **Active Status**: Inactive routes are still stored but not used for scheduling
6. **Soft Delete**: Deleted routes are marked with `deletedAt` timestamp
7. **Audit Trail**: All routes track `createdBy` and modification timestamps

## Use Cases

### Logistics Planning
- Find all depots that can serve a site
- Identify the fastest route to a site
- Calculate transport costs

### Route Optimization
- Update travel times based on traffic patterns
- Adjust distances when roads change
- Set seasonal transport rates

### Service Preferences
- Mark preferred depots for sites
- Temporarily disable routes during maintenance
- Override default routing logic

### Data Recovery
- Recreate previously deleted routes automatically
- Maintain historical data while allowing recreation
- Prevent database constraint errors

## Integration Notes

This API integrates with:
- **Depots API**: Validates depot existence and company membership
- **Sites API**: Validates site existence and company membership
- **Scheduling System**: Routes used for delivery planning
- **Cost Calculation**: Transport rates used for pricing

Routes are automatically validated to ensure depot and site belong to the same company through database triggers.

## Special Features

### Smart Route Recovery
The API includes intelligent soft-delete recovery:

1. **Delete Route**: `DELETE /api/user/depot-sites/1` ? Route soft-deleted
2. **Recreate Route**: `POST /api/user/depot-sites` with same depot-site combination ? Existing route reactivated
3. **New Data Applied**: All provided fields update the reactivated route
4. **Seamless Operation**: No error, no duplicate, just smart recovery

This feature ensures data consistency and prevents the common issue of unique constraint violations when working with soft-deleted records