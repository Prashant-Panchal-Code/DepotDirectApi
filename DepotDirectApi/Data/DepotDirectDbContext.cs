using DepotDirectApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepotDirectApi.Data;

public class DepotDirectDbContext : DbContext
{
    public DepotDirectDbContext(DbContextOptions<DepotDirectDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Country> Countries { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Region> Regions { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRegion> UserRegions { get; set; }
    public DbSet<Site> Sites { get; set; }
    public DbSet<RegionSite> RegionSites { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<SiteTank> SiteTanks { get; set; }
    public DbSet<TankReading> TankReadings { get; set; }
    public DbSet<TankDelivery> TankDeliveries { get; set; }
    public DbSet<SalesPattern> SalesPatterns { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Depot> Depots { get; set; }
    public DbSet<RegionDepot> RegionDepots { get; set; }
    public DbSet<DepotProduct> DepotProducts { get; set; }
    public DbSet<DepotSite> DepotSites { get; set; }
    public DbSet<Parking> Parkings { get; set; }
    public DbSet<RegionParking> RegionParkings { get; set; }

    // Hauliers
    public DbSet<Haulier> Hauliers { get; set; }

    // Vehicle Management DbSets
    public DbSet<BreakRule> BreakRules { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<DriverShift> DriverShifts { get; set; }
    public DbSet<DriverTimeOff> DriverTimeOffs { get; set; }
    public DbSet<Tractor> Tractors { get; set; }
    public DbSet<Trailer> Trailers { get; set; }
    public DbSet<TrailerCompartment> TrailerCompartments { get; set; }
    public DbSet<CompartmentAllowedProduct> CompartmentAllowedProducts { get; set; }
    public DbSet<VehicleCombination> VehicleCombinations { get; set; }
    public DbSet<VehicleCombinationTrailer> VehicleCombinationTrailers { get; set; }
    public DbSet<TractorSchedule> TractorSchedules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure schema
        modelBuilder.HasDefaultSchema("depotdirect");

        // Configure Depot entity
        modelBuilder.Entity<Depot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DepotCode).HasDatabaseName("idx_depots_depot_code");
            entity.HasIndex(e => e.CountryId).HasDatabaseName("idx_depots_country");
            entity.HasIndex(e => e.Town).HasDatabaseName("idx_depots_town");
            entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_depots_company");
            entity.HasIndex(e => e.Shortcode).HasDatabaseName("idx_depots_shortcode");

            entity.Property(e => e.DepotCode).IsRequired();
            entity.Property(e => e.DepotName).IsRequired();
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Priority).HasDefaultValue("Medium");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.Property(e => e.LatLong)
                  .HasComputedColumnSql("CASE WHEN latitude IS NOT NULL AND longitude IS NOT NULL THEN (latitude::text || ',' || longitude::text) ELSE NULL END", stored: true);

            entity.HasOne(d => d.Country)
                  .WithMany()
                  .HasForeignKey(d => d.CountryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Company)
                  .WithMany()
                  .HasForeignKey(d => d.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure RegionDepot entity
        modelBuilder.Entity<RegionDepot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DepotId).HasDatabaseName("idx_region_depots_depot");
            entity.HasIndex(e => e.RegionId).HasDatabaseName("idx_region_depots_region");
            entity.HasIndex(e => new { e.DepotId, e.RegionId }).IsUnique();

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAddOrUpdate();

            entity.HasOne(d => d.Depot)
                  .WithMany(p => p.RegionDepots)
                  .HasForeignKey(d => d.DepotId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Region)
                  .WithMany()
                  .HasForeignKey(d => d.RegionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Country entity
        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).HasDatabaseName("idx_countries_name");
            entity.HasIndex(e => new { e.Name, e.IsoCode }).IsUnique().HasDatabaseName("countries_name_iso_unique");
            
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.IsoCode).HasMaxLength(8);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        // Configure Company entity
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).HasDatabaseName("idx_companies_name");
            entity.HasIndex(e => e.CountryId).HasDatabaseName("idx_companies_country");
            
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            
            entity.HasOne(d => d.Country)
                  .WithMany(p => p.Companies)
                  .HasForeignKey(d => d.CountryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Region entity
        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).HasDatabaseName("idx_regions_name");
            entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_regions_company");
            
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            
            entity.HasOne(d => d.Company)
                  .WithMany(p => p.Regions)
                  .HasForeignKey(d => d.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Role entity
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("idx_roles_name");
            
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_users_company");
            entity.HasIndex(e => e.RoleId).HasDatabaseName("idx_users_role");
            
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FullName).IsRequired();
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            
            entity.HasOne(d => d.Company)
                  .WithMany(p => p.Users)
                  .HasForeignKey(d => d.CompanyId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(d => d.Role)
                  .WithMany(p => p.Users)
                  .HasForeignKey(d => d.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure UserRegion entity
        modelBuilder.Entity<UserRegion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).HasDatabaseName("idx_user_regions_user");
            entity.HasIndex(e => e.RegionId).HasDatabaseName("idx_user_regions_region");
            entity.HasIndex(e => new { e.UserId, e.RegionId }).IsUnique();
            
            // Let the database handle ID generation without explicit sequence reference
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            // Configure timestamps to use database defaults
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("now()")
                  .ValueGeneratedOnAdd();
                  
            entity.Property(e => e.UpdatedAt)
                  .HasDefaultValueSql("now()")
                  .ValueGeneratedOnAddOrUpdate();
            
            entity.HasOne(d => d.User)
                  .WithMany(p => p.UserRegions)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(d => d.Region)
                  .WithMany(p => p.UserRegions)
                  .HasForeignKey(d => d.RegionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Site entity
        modelBuilder.Entity<Site>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SiteCode).HasDatabaseName("idx_sites_site_code");
            entity.HasIndex(e => e.CountryId).HasDatabaseName("idx_sites_country");
            entity.HasIndex(e => e.Town).HasDatabaseName("idx_sites_town");
            entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_sites_company");
            entity.HasIndex(e => e.Shortcode).HasDatabaseName("idx_sites_shortcode");
            entity.HasIndex(e => new { e.CountryId, e.SiteCode }).IsUnique().HasDatabaseName("sites_country_code_uniq");
            
            entity.Property(e => e.SiteCode).IsRequired();
            entity.Property(e => e.SiteName).IsRequired();
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Priority).HasDefaultValue("Medium");
            entity.Property(e => e.DeliveryStopped).HasDefaultValue(false);
            entity.Property(e => e.PumpedRequired).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            
            // Configure LatLong as a computed column - always shows 0,0 if coordinates are NULL
            entity.Property(e => e.LatLong)
                  .HasComputedColumnSql("COALESCE(latitude, 0)::text || ',' || COALESCE(longitude, 0)::text", stored: true);
            
            entity.HasOne(d => d.Country)
                  .WithMany()
                  .HasForeignKey(d => d.CountryId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(d => d.Company)
                  .WithMany()
                  .HasForeignKey(d => d.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure RegionSite entity
        modelBuilder.Entity<RegionSite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SiteId).HasDatabaseName("idx_region_sites_site");
            entity.HasIndex(e => e.RegionId).HasDatabaseName("idx_region_sites_region");
            entity.HasIndex(e => new { e.SiteId, e.RegionId }).IsUnique();
            
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("now()")
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt)
                  .HasDefaultValueSql("now()")
                  .ValueGeneratedOnAddOrUpdate();
            
            entity.HasOne(d => d.Site)
                  .WithMany(p => p.RegionSites)
                  .HasForeignKey(d => d.SiteId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(d => d.Region)
                  .WithMany()
                  .HasForeignKey(d => d.RegionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Note entity
        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("notes", "depotdirect");
            entity.Property(e => e.Category).IsRequired();
            entity.Property(e => e.Priority).HasDefaultValue("Medium");
            entity.Property(e => e.Comment).IsRequired();
            entity.Property(e => e.Status).HasDefaultValue("Open");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.HasIndex(e => e.SiteId).HasDatabaseName("idx_notes_site");
            entity.HasIndex(e => e.DepotId).HasDatabaseName("idx_notes_depot");
            entity.HasIndex(e => e.ParkingId).HasDatabaseName("idx_notes_parking");
            entity.HasIndex(e => e.VehicleId).HasDatabaseName("idx_notes_vehicle");
            entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_notes_company");
            entity.HasIndex(e => e.Status).HasDatabaseName("idx_notes_status");
        });

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("products", "depotdirect");

            entity.HasIndex(e => e.ProductCode).HasDatabaseName("idx_products_code");
            entity.HasIndex(e => e.RegionId).HasDatabaseName("idx_products_region");
            entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_products_company");

            entity.Property(e => e.ProductCode).IsRequired();
            entity.Property(e => e.ProductName).IsRequired();
            entity.Property(e => e.IsHazardous).HasDefaultValue(true);
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Company)
                  .WithMany()
                  .HasForeignKey(d => d.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Region)
                  .WithMany()
                  .HasForeignKey(d => d.RegionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure SiteTank entity
        modelBuilder.Entity<SiteTank>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("site_tanks", "depotdirect");

            entity.HasIndex(e => new { e.SiteId, e.TankCode }).IsUnique().HasDatabaseName("tank_site_code_uniq");

            entity.Property(e => e.TankCode).IsRequired();
            entity.Property(e => e.CapacityL).HasDefaultValue(0);
            entity.Property(e => e.SafeFillL).HasDefaultValue(0);
            entity.Property(e => e.DeadstockL).HasDefaultValue(0);
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Site)
                  .WithMany(p => p.SiteTanks)
                  .HasForeignKey(d => d.SiteId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Product)
                  .WithMany()
                  .HasForeignKey(d => d.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure TankReading entity
        modelBuilder.Entity<TankReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("tank_readings", "depotdirect");

            entity.Property(e => e.ReadingTimestamp).HasDefaultValueSql("now()");
            entity.Property(e => e.ReadingMethod).IsRequired();
            entity.Property(e => e.CurrentVolumeL).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Tank)
                  .WithMany(p => p.TankReadings)
                  .HasForeignKey(d => d.TankId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure TankDelivery entity
        modelBuilder.Entity<TankDelivery>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("tank_deliveries", "depotdirect");

            entity.Property(e => e.Status).HasDefaultValue("Planned");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Tank)
                  .WithMany(p => p.TankDeliveries)
                  .HasForeignKey(d => d.TankId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SalesPattern entity
        modelBuilder.Entity<SalesPattern>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("sales_patterns", "depotdirect");

            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.HasIndex(e => new { e.TankId, e.DayOfWeek, e.HourOfDay }).IsUnique();

            entity.HasOne(d => d.Tank)
                  .WithMany()
                  .HasForeignKey(d => d.TankId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure DepotProduct entity
        modelBuilder.Entity<DepotProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("depot_products", "depotdirect");
            entity.HasIndex(e => e.DepotId).HasDatabaseName("idx_depot_products_depot");
            entity.HasIndex(e => e.ProductId).HasDatabaseName("idx_depot_products_product");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAddOrUpdate();

            entity.HasOne(d => d.Depot)
                  .WithMany()
                  .HasForeignKey(d => d.DepotId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Product)
                  .WithMany()
                  .HasForeignKey(d => d.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure DepotSite entity
        modelBuilder.Entity<DepotSite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DepotId).HasDatabaseName("idx_depot_sites_depot");
            entity.HasIndex(e => e.SiteId).HasDatabaseName("idx_depot_sites_site");
            entity.HasIndex(e => new { e.SiteId, e.IsPrimary }).HasDatabaseName("idx_depot_sites_primary");
            entity.HasIndex(e => new { e.DepotId, e.SiteId }).IsUnique().HasDatabaseName("depot_sites_uniq");

            entity.Property(e => e.DistanceKm).IsRequired().HasPrecision(10, 2);
            entity.Property(e => e.TravelTimeMins).IsRequired();
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.IsPrimary).HasDefaultValue(false);
            entity.Property(e => e.TransportRate).HasPrecision(10, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Depot)
                  .WithMany()
                  .HasForeignKey(d => d.DepotId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Site)
                  .WithMany()
                  .HasForeignKey(d => d.SiteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Parking entity
        modelBuilder.Entity<Parking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ParkingCode).HasDatabaseName("idx_parkings_parking_code");
            entity.HasIndex(e => e.CountryId).HasDatabaseName("idx_parkings_country");
            entity.HasIndex(e => e.Town).HasDatabaseName("idx_parkings_town");
            entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_parkings_company");
            entity.HasIndex(e => e.Shortcode).HasDatabaseName("idx_parkings_shortcode");
            entity.HasIndex(e => new { e.CountryId, e.ParkingCode }).IsUnique().HasDatabaseName("parkings_country_code_uniq");

            entity.Property(e => e.ParkingCode).IsRequired();
            entity.Property(e => e.ParkingName).IsRequired();
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Latitude).HasPrecision(10, 7);
            entity.Property(e => e.Longitude).HasPrecision(10, 7);

            entity.Property(e => e.LatLong)
                  .HasComputedColumnSql("CASE WHEN latitude IS NOT NULL AND longitude IS NOT NULL THEN (latitude::text || ',' || longitude::text) ELSE NULL END", stored: true);

            entity.HasOne(d => d.Country)
                  .WithMany()
                  .HasForeignKey(d => d.CountryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Company)
                  .WithMany()
                  .HasForeignKey(d => d.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure RegionParking entity
        modelBuilder.Entity<RegionParking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ParkingId).HasDatabaseName("idx_region_parkings_parking");
            entity.HasIndex(e => e.RegionId).HasDatabaseName("idx_region_parkings_region");
            entity.HasIndex(e => new { e.ParkingId, e.RegionId }).IsUnique();

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAddOrUpdate();

            entity.HasOne(d => d.Parking)
                  .WithMany(p => p.RegionParkings)
                  .HasForeignKey(d => d.ParkingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Region)
                  .WithMany()
                  .HasForeignKey(d => d.RegionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Vehicle Management entities
        ConfigureHaulier(modelBuilder);
        ConfigureBreakRule(modelBuilder);
        ConfigureDriver(modelBuilder);
        ConfigureDriverShift(modelBuilder);
        ConfigureDriverTimeOff(modelBuilder);
        ConfigureTractor(modelBuilder);
        ConfigureTrailer(modelBuilder);
        ConfigureTrailerCompartment(modelBuilder);
        ConfigureCompartmentAllowedProduct(modelBuilder);
        ConfigureVehicleCombination(modelBuilder);
        ConfigureVehicleCombinationTrailer(modelBuilder);
        ConfigureTractorSchedule(modelBuilder);
    }

    private void ConfigureHaulier(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Haulier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RegionId).HasDatabaseName("idx_hauliers_region");
            entity.HasIndex(e => new { e.RegionId, e.HaulierCode }).IsUnique().HasDatabaseName("hauliers_code_region_uniq");

            entity.Property(e => e.HaulierCode).IsRequired();
            entity.Property(e => e.HaulierName).IsRequired();
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Region)
                  .WithMany()
                  .HasForeignKey(d => d.RegionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureBreakRule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BreakRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_break_rules_company");
            entity.HasIndex(e => new { e.CompanyId, e.RuleName }).IsUnique().HasDatabaseName("break_rules_uniq");

            entity.Property(e => e.RuleName).IsRequired();
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Company)
                  .WithMany()
                  .HasForeignKey(d => d.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureDriver(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CompanyId).HasDatabaseName("idx_drivers_company");
            entity.HasIndex(e => e.HomeDepotId).HasDatabaseName("idx_drivers_home_depot");
            entity.HasIndex(e => e.RegionId).HasDatabaseName("idx_drivers_region");
            entity.HasIndex(e => new { e.CompanyId, e.LicenseNumber }).IsUnique().HasDatabaseName("drivers_license_uniq");

            entity.Property(e => e.DriverCode).IsRequired();
            entity.Property(e => e.FirstName).IsRequired();
            entity.Property(e => e.LastName).IsRequired();
            entity.Property(e => e.LicenseNumber).IsRequired();
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Status).HasDefaultValue("Available");
            entity.Property(e => e.HazmatCertified).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Company)
                  .WithMany()
                  .HasForeignKey(d => d.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.HomeDepot)
                  .WithMany()
                  .HasForeignKey(d => d.HomeDepotId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Region)
                  .WithMany()
                  .HasForeignKey(d => d.RegionId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.BreakRule)
                  .WithMany(p => p.Drivers)
                  .HasForeignKey(d => d.BreakRuleId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureDriverShift(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DriverShift>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DriverId).HasDatabaseName("idx_driver_shifts_driver");
            entity.HasIndex(e => new { e.DriverId, e.DayOfWeek, e.StartTime }).IsUnique();

            entity.Property(e => e.Active).HasDefaultValue(true);

            entity.HasOne(d => d.Driver)
                  .WithMany(p => p.DriverShifts)
                  .HasForeignKey(d => d.DriverId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.StartDepot)
                  .WithMany()
                  .HasForeignKey(d => d.StartDepotId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureDriverTimeOff(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DriverTimeOff>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DriverId).HasDatabaseName("idx_driver_time_off_driver");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Driver)
                  .WithMany(p => p.DriverTimeOffs)
                  .HasForeignKey(d => d.DriverId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureTractor(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tractor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.HaulierId).HasDatabaseName("idx_tractors_haulier");
            entity.HasIndex(e => e.RegionId).HasDatabaseName("idx_tractors_region");
            entity.HasIndex(e => new { e.HaulierId, e.TractorCode }).IsUnique().HasDatabaseName("tractors_code_uniq");

            entity.Property(e => e.TractorCode).IsRequired();
            entity.Property(e => e.TractorName).IsRequired();
            entity.Property(e => e.LicensePlate).IsRequired();
            entity.Property(e => e.Status).HasDefaultValue("Active");
            entity.Property(e => e.PumpAvailable).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Haulier)
                  .WithMany(h => h.Tractors)
                  .HasForeignKey(d => d.HaulierId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Region)
                  .WithMany()
                  .HasForeignKey(d => d.RegionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureTrailer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Trailer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.HaulierId).HasDatabaseName("idx_trailers_haulier");
            entity.HasIndex(e => e.RegionId).HasDatabaseName("idx_trailers_region");
            entity.HasIndex(e => new { e.HaulierId, e.TrailerCode }).IsUnique().HasDatabaseName("trailers_code_uniq");

            entity.Property(e => e.TrailerCode).IsRequired();
            entity.Property(e => e.TrailerName).IsRequired();
            entity.Property(e => e.LicensePlate).IsRequired();
            entity.Property(e => e.Status).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Haulier)
                  .WithMany(h => h.Trailers)
                  .HasForeignKey(d => d.HaulierId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Region)
                  .WithMany()
                  .HasForeignKey(d => d.RegionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureTrailerCompartment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrailerCompartment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TrailerId).HasDatabaseName("idx_trailer_compartments_trailer");
            entity.HasIndex(e => new { e.TrailerId, e.CompartmentNumber }).IsUnique();

            entity.Property(e => e.MustUse).HasDefaultValue(false);
            entity.Property(e => e.PartialLoadAllowed).HasDefaultValue(true);
            entity.Property(e => e.MinVolumeL).HasDefaultValue(0m);  // Fixed: Changed from 0 to 0m for decimal

            entity.HasOne(d => d.Trailer)
                  .WithMany(p => p.TrailerCompartments)
                  .HasForeignKey(d => d.TrailerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureCompartmentAllowedProduct(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompartmentAllowedProduct>(entity =>
        {
            entity.HasKey(e => new { e.CompartmentId, e.ProductId });

            entity.HasOne(d => d.Compartment)
                  .WithMany(p => p.CompartmentAllowedProducts)
                  .HasForeignKey(d => d.CompartmentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Product)
                  .WithMany()
                  .HasForeignKey(d => d.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureVehicleCombination(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VehicleCombination>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TractorId).HasDatabaseName("idx_vehicle_combinations_tractor");
            entity.HasIndex(e => new { e.TractorId, e.CombinationCode }).IsUnique();

            entity.Property(e => e.CombinationCode).IsRequired();
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Tractor)
                  .WithMany(p => p.VehicleCombinations)
                  .HasForeignKey(d => d.TractorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureVehicleCombinationTrailer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VehicleCombinationTrailer>(entity =>
        {
            entity.HasKey(e => new { e.CombinationId, e.TrailerId });

            entity.Property(e => e.SequenceNumber).HasDefaultValue(1);

            entity.HasOne(d => d.VehicleCombination)
                  .WithMany(p => p.VehicleCombinationTrailers)
                  .HasForeignKey(d => d.CombinationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Trailer)
                  .WithMany(p => p.VehicleCombinationTrailers)
                  .HasForeignKey(d => d.TrailerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureTractorSchedule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TractorSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TractorId).HasDatabaseName("idx_tractor_schedules_tractor");
            entity.HasIndex(e => e.DriverId).HasDatabaseName("idx_tractor_schedules_driver");
            entity.HasIndex(e => new { e.DayOfWeek, e.ShiftStartTime, e.ShiftEndTime }).HasDatabaseName("idx_tractor_schedules_search");

            entity.Property(e => e.IsOvertime).HasDefaultValue(false);
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(d => d.Tractor)
                  .WithMany(p => p.TractorSchedules)
                  .HasForeignKey(d => d.TractorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Driver)
                  .WithMany(p => p.TractorSchedules)
                  .HasForeignKey(d => d.DriverId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.StartDepot)
                  .WithMany()
                  .HasForeignKey(d => d.StartDepotId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.StartParking)
                  .WithMany()
                  .HasForeignKey(d => d.StartParkingId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.EndDepot)
                  .WithMany()
                  .HasForeignKey(d => d.EndDepotId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.EndParking)
                  .WithMany()
                  .HasForeignKey(d => d.EndParkingId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}