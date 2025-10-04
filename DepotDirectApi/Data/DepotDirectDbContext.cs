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
    }
}