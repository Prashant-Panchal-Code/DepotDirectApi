# DepotDirect API Schema Migration Summary

## Overview
This document summarizes the changes made to update the DepotDirect API to match the new database schema where:
- Regions now belong directly to Companies (not Countries)
- Users and Roles tables have been added
- CompanyRegion mapping table has been removed
- Depots table has been removed

## Database Schema Changes
Based on the new migration file `0001_init_schema.sql`:

### New Relationships:
- `regions.company_id` ? `companies.id` (was previously `regions.country_id` ? `countries.id`)
- `users.company_id` ? `companies.id` (new)
- `users.role_id` ? `roles.id` (new)

### Removed Tables:
- `depots` - completely removed
- `company_regions` - no longer needed as regions belong directly to companies

## Code Changes Made

### 1. Entity Models Updated

#### `Country.cs`
- ? Removed navigation properties for `Regions` and `Depots`
- ? Kept `Companies` navigation property

#### `Company.cs`
- ? Replaced `CompanyRegions` with direct `Regions` navigation property
- ? Added `Users` navigation property
- ? Kept `Country` navigation property

#### `Region.cs`
- ? Changed from `CountryId` to `CompanyId`
- ? Updated navigation property from `Country` to `Company`
- ? Removed `CompanyRegions` navigation property

#### New Entities Added:
- ? `Role.cs` - for user roles (admin, planner, driver, viewer)
- ? `User.cs` - for system users with company and role relationships

#### Removed Entities:
- ? `CompanyRegion.cs` - deleted (no longer needed)
- ? `Depot.cs` - deleted (removed from schema)

### 2. DbContext Updated (`DepotDirectDbContext.cs`)
- ? Removed `CompanyRegions` and `Depots` DbSets
- ? Added `Roles` and `Users` DbSets
- ? Updated entity configurations for new relationships
- ? Added proper foreign key constraints and indexes

### 3. DTOs Updated

#### `RegionDtos.cs`
- ? Changed `CountryId` to `CompanyId` in all DTOs
- ? Updated navigation properties to use `Company` instead of `Country`
- ? Removed CompanyRegion-related DTOs

#### New DTOs Added:
- ? `RoleDtos.cs` - CreateRoleDto, UpdateRoleDto, RoleDto
- ? `UserDtos.cs` - CreateUserDto, UpdateUserDto, UserDto

#### `CompanyDtos.cs`
- ? Added `CompanyDto` class for use in navigation properties

#### `CountryDtos.cs`
- ? Removed `RegionsCount` and `DepotsCount` from `CountryWithStatsDto`

### 4. Repositories Updated

#### `RegionRepository.cs`
- ? Updated to work with `CompanyId` instead of `CountryId`
- ? Removed all CompanyRegion mapping methods
- ? Updated validation logic for company-region relationships
- ? Fixed LINQ queries to use Company navigation

#### `CompanyRepository.cs`
- ? Removed CompanyRegion references
- ? Updated `GetRegionsByCompanyIdAsync` to use direct region relationships
- ? Fixed all LINQ queries

#### `CountryRepository.cs`
- ? Removed all references to Regions and Depots
- ? Updated statistics methods to only count Companies

#### New Repositories Added:
- ? `IRoleRepository.cs` and `RoleRepository.cs`
- ? `IUserRepository.cs` and `UserRepository.cs`

### 5. Controllers Updated

#### `RegionsController.cs`
- ? Removed company-region mapping endpoints (no longer needed)
- ? Updated endpoint from `by-country/{countryId}` to `by-company/{companyId}`
- ? Simplified controller to match new direct relationships

#### New Controllers Added:
- ? `RolesController.cs` - full CRUD operations for roles
- ? `UsersController.cs` - full CRUD operations for users with password hashing

### 6. Dependencies Updated
- ? Added `BCrypt.Net-Next` package for password hashing
- ? Updated `Program.cs` to register new repositories

## API Endpoint Changes

### Regions
- **Changed**: `GET /api/admin/regions/by-country/{countryId}` ? `GET /api/admin/regions/by-company/{companyId}`
- **Removed**: All company-region mapping endpoints
  - `POST /api/admin/regions/{regionId}/companies/{companyId}`
  - `DELETE /api/admin/regions/{regionId}/companies/{companyId}`
  - `GET /api/admin/regions/{regionId}/companies`
  - `GET /api/admin/regions/by-company/{companyId}` (was for mapping)
  - `GET /api/admin/regions/{regionId}/companies/{companyId}/is-assigned`

### New Endpoints Added
#### Roles
- `GET /api/admin/roles` - Get all roles
- `GET /api/admin/roles/{id}` - Get role by ID
- `GET /api/admin/roles/by-name/{name}` - Get role by name
- `POST /api/admin/roles` - Create role
- `PUT /api/admin/roles/{id}` - Update role
- `DELETE /api/admin/roles/{id}` - Delete role
- `GET /api/admin/roles/{id}/exists` - Check if role exists

#### Users
- `GET /api/admin/users` - Get all users
- `GET /api/admin/users/{id}` - Get user by ID
- `GET /api/admin/users/by-email/{email}` - Get user by email
- `GET /api/admin/users/by-company/{companyId}` - Get users by company
- `GET /api/admin/users/by-role/{roleId}` - Get users by role
- `POST /api/admin/users` - Create user
- `PUT /api/admin/users/{id}` - Update user
- `DELETE /api/admin/users/{id}` - Soft delete user
- `GET /api/admin/users/{id}/exists` - Check if user exists

## Key Features

### User Management
- Password hashing using BCrypt
- Soft delete functionality
- Company and role associations
- Email uniqueness validation

### Role Management
- Built-in roles: admin, planner, driver, viewer
- JSON permissions support
- Role name uniqueness

### Simplified Region Management
- Direct company ownership (no many-to-many mapping)
- Simplified API surface
- Better performance with direct relationships

## Migration Notes
- All existing CompanyRegion data would need to be migrated to direct region ownership
- Existing region-country relationships need to be converted to region-company relationships
- New user and role data needs to be populated

## Next Steps
1. Run database migration to apply schema changes
2. Test all API endpoints
3. Update any client applications to use new endpoint structures
4. Populate initial user and role data