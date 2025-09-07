# DepotDirect Database Schema

This repository contains the **initial PostgreSQL schema** for the DepotDirect application.  
The schema is designed for global usage across multiple countries, companies, and regions, with clear separation of master data and membership mappings.

---

## 📐 Design Principles

- **Schema**: All objects live in the `depotdirect` schema (owner: `depotdirect_user`).
- **Primary keys**: Integer sequences (e.g., `id`).
- **Codes**:  
  - `site_code`, `depot_code`, `parking_code` live on master rows.  
  - Codes are **unique per country**: `(country_id, code)` is unique.  
- **Country scoping**: All master rows (`sites`, `depots`, `parkings`, `companies`, `regions`) require a `country_id`.
- **Companies & Regions**:  
  - Companies belong to a country.  
  - Regions belong to a country.  
  - Companies ↔ Regions is **many-to-many** via `company_regions`.
- **Master tables** (`sites`, `depots`, `parkings`) do **not** hold `company_id` or `region_id`.  
  - Membership is modeled with join tables:  
    - `*_companies` → master to company  
    - `*_regions` → master to region  
- **Users & Roles**:  
  - `roles` table stores role definitions (e.g., `admin`, `planner`).  
  - `users.role_id` references `roles`.  
  - `users.company_id` is optional (user may or may not belong to a company).
- **Audit metadata**:  
  - Common fields: `created_by`, `last_updated_by`, `created_at`, `updated_at`, `deleted_at`.  
  - `active` boolean present on most tables.  
- **Triggers**:  
  - `fn_set_updated_at` → auto-updates `updated_at`.  
  - `fn_soft_delete` → converts DELETE into `active=false, deleted_at=now()` (where applicable).  
  - `fn_validate_operating_hours` → validates `operating_hours` JSON on tables that have it.

---

## 📊 Entities

### Core lookups
- **countries**: Country list (with `iso_code`).
- **companies**: Companies, scoped to a country.
- **regions**: Regions, scoped to a country.
- **company_regions**: Many-to-many mapping between companies and regions.

### Master location data
- **depots**: Storage/dispatch facilities.
- **sites**: Customer delivery sites.
- **parkings**: Parking areas.  
  - Each has `*_code`, `*_name`, address, coordinates, etc.  
  - Codes are unique per country.

### Mappings
- **site_regions**, **depot_regions**, **parking_regions**: Master → Region mappings.  
- **site_companies**, **depot_companies**, **parking_companies**: Master → Company mappings.  
- Each mapping can optionally carry an override code.

### Users & Roles
- **roles**: Role definitions with optional permissions JSON.  
- **users**: Application users, linked to a role and optionally a company.

---

## 🗂️ Example Workflows

### 1. Create a new site
```sql
-- Insert into sites (code unique within the country)
INSERT INTO depotdirect.sites
  (site_code, site_name, country_id, town, priority, created_by)
VALUES
  ('SITE-100', 'New Delhi Warehouse', 1, 'Delhi', 'High', 1);
