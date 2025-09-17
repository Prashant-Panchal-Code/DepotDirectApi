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
    public DbSet<Depot> Depots { get; set; }

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
            entity.HasIndex(e => e.CountryId).HasDatabaseName("idx_regions_country");
            
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            
            entity.HasOne(d => d.Country)
                  .WithMany(p => p.Regions)
                  .HasForeignKey(d => d.CountryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Depot entity
        modelBuilder.Entity<Depot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DepotCode).HasDatabaseName("idx_depots_depot_code");
            entity.HasIndex(e => e.CountryId).HasDatabaseName("idx_depots_country");
            entity.HasIndex(e => e.Town).HasDatabaseName("idx_depots_town");
            
            entity.Property(e => e.DepotCode).IsRequired();
            entity.Property(e => e.DepotName).IsRequired();
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Priority).HasDefaultValue("Medium");
            entity.Property(e => e.IsParking).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            
            // Add check constraint for priority
            entity.HasCheckConstraint("depots_priority_chk", "priority IN ('High','Medium','Low')");
            
            // Add unique constraint for country_id and depot_code
            entity.HasIndex(e => new { e.CountryId, e.DepotCode })
                  .IsUnique()
                  .HasDatabaseName("depots_country_code_uniq");
            
            entity.HasOne(d => d.Country)
                  .WithMany(p => p.Depots)
                  .HasForeignKey(d => d.CountryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}