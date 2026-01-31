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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure schema
        modelBuilder.HasDefaultSchema("depotdirect");

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
    }
}