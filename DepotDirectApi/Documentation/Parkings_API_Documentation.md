# Parkings CRUD API Documentation

This document describes the CRUD operations for managing parkings in the DepotDirect API.

## Overview

The parkings API manages parking facilities that serve various regions. Each parking defines:

- **Basic Info**: Code, name, and location details
- **Geographic Data**: Coordinates, address information
- **Capacity**: Number of parking spaces available
- **Management**: Contact information for managers
- **Region Mapping**: Assignment to one or more regions

## Base URL

All endpoints are under: `/api/user/parkings`

## Authentication

All endpoints require JWT Bearer token authentication.

## Endpoints

### 1. Get All Parkings

```http
GET /api/user/parkings
Authorization: Bearer {token}
```

**Response:**
```json
[
  {
    "id": 1,
    "parkingCode": "PARK001",
    "parkingName": "Central Parking Hub",
    "town": "Business District",
    "active": true,
    "parkingSpaces": 150,
    "companyId": 1,
    "companyName": "ACME Corp",
    "countryId": 1,
    "countryName": "United Kingdom",
    "latitude": 51.5074,
    "longitude": -0.1278,
    "latLong": "51.5074,-0.1278",
    "street": "123 Business Avenue",
    "postalCode": "SW1A 1AA",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
]
```

### 2. Get Parking by ID

```http
GET /api/user/parkings/{id}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "id": 1,
  "parkingCode": "PARK001",
  "parkingName": "Central Parking Hub",
  "shortcode": "CPH",
  "latitude": 51.5074,
  "longitude": -0.1278,
  "latLong": "51.5074,-0.1278",
  "street": "123 Business Avenue",
  "postalCode": "SW1A 1AA",
  "town": "Business District",
  "active": true,
  "managerName": "John Smith",
  "managerPhone": "+44 20 7946 0958",
  "managerEmail": "john.smith@parking.com",
  "emergencyContact": "Emergency Line: +44 20 7946 0999",
  "parkingSpaces": 150,
  "countryId": 1,
  "companyId": 1,
  "metadata": {},
  "createdBy": 1,
  "lastUpdatedBy": 1,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z",
  "deletedAt": null,
  "country": {
    "id": 1,
    "name": "United Kingdom",
    "isoCode": "GB",
    "metadata": {},
    "createdBy": 1,
    "lastUpdatedBy": 1,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "company": {
    "id": 1,
    "name": "ACME Corp",
    "companyCode": "ACME",
    "countryId": 1,
    "description": "Leading logistics company",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z",
    "createdBy": 1,
    "lastUpdatedBy": 1
  },
  "regions": [
    {
      "id": 1,
      "name": "London Region",
      "regionCode": "LON",
      "companyId": 1,
      "metadata": {},
      "createdBy": 1,
      "lastUpdatedBy": 1,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-01T00:00:00Z"
    }
  ]
}
```

### 3. Create New Parking

```http
POST /api/user/parkings
Authorization: Bearer {token}
Content-Type: application/json

{
  "parkingCode": "PARK002",
  "parkingName": "North Side Parking",
  "regionId": 1
}
```

**Response:** 201 Created
```json
{
  "id": 2,
  "parkingCode": "PARK002",
  "parkingName": "North Side Parking",
  "shortcode": null,
  "latitude": null,
  "longitude": null,
  "latLong": null,
  "street": null,
  "postalCode": null,
  "town": null,
  "active": true,
  "managerName": null,
  "managerPhone": null,
  "managerEmail": null,
  "emergencyContact": null,
  "parkingSpaces": null,
  "countryId": 1,
  "companyId": 1,
  "metadata": {},
  "createdBy": 1,
  "lastUpdatedBy": 1,
  "createdAt": "2024-01-15T11:00:00Z",
  "updatedAt": "2024-01-15T11:00:00Z",
  "deletedAt": null,
  "country": { "..." },
  "company": { "..." },
  "regions": [ { "..." } ]
}
```

**Validation Rules:**
- `parkingCode`: Required, must be unique per country
- `parkingName`: Required
- `regionId`: Required, must exist and be active
- Company and Country automatically set from region

### 4. Update Parking

```http
PUT /api/user/parkings/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "shortcode": "NSP",
  "latitude": 51.5155,
  "longitude": -0.0922,
  "street": "456 North Avenue",
  "postalCode": "N1 7GU",
  "town": "North District",
  "managerName": "Jane Doe",
  "managerPhone": "+44 20 7946 1234",
  "managerEmail": "jane.doe@parking.com",
  "parkingSpaces": 200
}
```

**Response:** 200 OK
```json
{
  "id": 2,
  "parkingCode": "PARK002",
  "parkingName": "North Side Parking",
  "shortcode": "NSP",
  "latitude": 51.5155,
  "longitude": -0.0922,
  "latLong": "51.5155,-0.0922",
  "street": "456 North Avenue",
  "postalCode": "N1 7GU",
  "town": "North District",
  "active": true,
  "managerName": "Jane Doe",
  "managerPhone": "+44 20 7946 1234",
  "managerEmail": "jane.doe@parking.com",
  "emergencyContact": null,
  "parkingSpaces": 200,
  "countryId": 1,
  "companyId": 1,
  "metadata": {},
  "createdBy": 1,
  "lastUpdatedBy": 1,
  "createdAt": "2024-01-15T11:00:00Z",
  "updatedAt": "2024-01-15T11:30:00Z",
  "deletedAt": null,
  "country": { "..." },
  "company": { "..." },
  "regions": [ { "..." } ]
}
```

### 5. Delete Parking (Soft Delete)

```http
DELETE /api/user/parkings/{id}
Authorization: Bearer {token}
```

**Response:** 204 No Content

### 6. Check Parking Exists

```http
GET /api/user/parkings/{id}/exists
Authorization: Bearer {token}
```

**Response:**
```json
true
```

## Additional Endpoints

### Get Parkings by Company

```http
GET /api/user/parkings/by-company/{companyId}
Authorization: Bearer {token}
```

Returns all parkings for a specific company.

### Get Parkings by Country

```http
GET /api/user/parkings/by-country/{countryId}
Authorization: Bearer {token}
```

Returns all parkings in a specific country.

### Get Parkings by Region

```http
GET /api/user/parkings/by-region/{regionId}
Authorization: Bearer {token}
```

Returns all parkings assigned to a specific region.

### Search Parkings

```http
GET /api/user/parkings/search?query=central
Authorization: Bearer {token}
```

Searches parkings by code, name, or town.

### Assign Parking to Region

```http
POST /api/user/parkings/{parkingId}/regions
Authorization: Bearer {token}
Content-Type: application/json

{
  "regionId": 2,
  "parkingCode": "PARK002-R2",
  "metadata": {
    "assignment_reason": "Expansion to new region"
  }
}
```

**Response:**
```json
{
  "id": 5,
  "parkingId": 2,
  "parkingName": "North Side Parking",
  "parkingCode": "PARK002",
  "regionId": 2,
  "regionName": "South Region",
  "regionParkingCode": "PARK002-R2",
  "metadata": {
    "assignment_reason": "Expansion to new region"
  },
  "createdBy": 1,
  "createdAt": "2024-01-15T12:00:00Z",
  "updatedAt": "2024-01-15T12:00:00Z"
}
```

### Remove Parking from Region

```http
DELETE /api/user/parkings/{parkingId}/regions/{regionId}
Authorization: Bearer {token}
```

**Response:** 204 No Content

### Check Parking Assignment to Region

```http
GET /api/user/parkings/{parkingId}/regions/{regionId}/exists
Authorization: Bearer {token}
```

**Response:**
```json
true
```

## Error Responses

### 400 Bad Request
```json
{
  "error": "Validation failed",
  "details": {
    "parkingCode": ["Parking code is required"],
    "parkingName": ["Parking name is required"]
  }
}
```

### 404 Not Found
```json
{
  "error": "Parking with ID 999 not found"
}
```

### 409 Conflict
```json
{
  "error": "Parking code 'PARK001' already exists in this country."
}
```

### 422 Unprocessable Entity
```json
{
  "error": "Parking and Region must belong to the same company."
}
```

## Business Rules

1. **Company Validation**: Parkings and Regions must belong to the same company
2. **Unique Codes**: Parking codes must be unique within each country
3. **Region Assignment**: Parkings can be assigned to multiple regions within the same company
4. **Soft Delete**: Deleted parkings are marked with `deletedAt` timestamp
5. **Audit Trail**: All parkings track `createdBy`, `lastUpdatedBy`, and timestamps
6. **Location Data**: Coordinates automatically generate `latLong` computed field
7. **Manager Contacts**: Email validation enforced for manager email addresses

## Use Cases

### Parking Management
- Track available parking spaces across facilities
- Manage contact information for each parking location
- Monitor parking capacity and utilization

### Regional Operations
- Assign parkings to multiple operational regions
- Override parking codes for region-specific operations
- Track metadata for assignment reasons and configurations

### Location Services
- Geocoding support with latitude/longitude
- Address management with street, postal code, town
- Generated coordinate strings for mapping integration

## Integration Notes

This API integrates with:
- **Regions API**: Validates region existence and company membership
- **Companies API**: Inherits company from region assignment
- **Countries API**: Inherits country from company
- **Mapping Services**: Uses coordinate data for location services
- **Scheduling System**: Parking availability used for logistics planning

Parkings are automatically validated to ensure they belong to the same company as their assigned regions through database triggers.

## Two-Phase Creation Pattern

Like other entities in the system, parkings follow a two-phase creation pattern:

### Phase 1: Initial Creation (Minimal)
```json
{
  "parkingCode": "PARK003",
  "parkingName": "East District Parking",
  "regionId": 1
}
```

### Phase 2: Update with Details (Complete)
```json
{
  "shortcode": "EDP",
  "latitude": 51.5321,
  "longitude": -0.0555,
  "street": "789 East Street",
  "postalCode": "E1 6AN",
  "town": "East District",
  "managerName": "Bob Johnson",
  "managerPhone": "+44 20 7946 5678",
  "managerEmail": "bob.johnson@parking.com",
  "emergencyContact": "Emergency: +44 20 7946 9999",
  "parkingSpaces": 300,
  "active": true
}
```

This pattern allows for quick setup during initial data entry, followed by detailed configuration as information becomes available.