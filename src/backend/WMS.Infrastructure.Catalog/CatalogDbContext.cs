using Microsoft.EntityFrameworkCore;
using WMS.Infrastructure.Catalog.Entities;

namespace WMS.Infrastructure.Catalog;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<SuperAdmin> SuperAdmins => Set<SuperAdmin>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantDatabase> TenantDatabases => Set<TenantDatabase>();
    public DbSet<UserLookup> UserLookups => Set<UserLookup>();
    public DbSet<AuditGlobal> AuditGlobal => Set<AuditGlobal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SuperAdmin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.Email).HasComputedColumnSql("lower(email)", stored: true);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.IsLocked).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Status).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<TenantDatabase>(entity =>
        {
            entity.HasKey(e => e.TenantId);
            entity.Property(e => e.Host).IsRequired();
            entity.Property(e => e.DatabaseName).IsRequired();
            entity.Property(e => e.Username).IsRequired();
            entity.Property(e => e.PasswordEnc).IsRequired();
        });

        modelBuilder.Entity<UserLookup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.Email).HasComputedColumnSql("lower(email)", stored: true);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasOne(e => e.Tenant).WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<AuditGlobal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OccurredAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ActorType).IsRequired();
            entity.Property(e => e.Action).IsRequired();
            entity.Property(e => e.TargetType).IsRequired();
        });
    }
}
