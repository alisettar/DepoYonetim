using Microsoft.EntityFrameworkCore;
using WMS.Domain.Catalog;
using WMS.Domain.Identity;
using WMS.Infrastructure.Services;
using WMS.Shared.Common;

namespace WMS.Infrastructure.Persistence;

public class AppDbContext(
    ITenantContext tenantContext,
    ICachedTenantConnectionFactory connectionFactory)
    : DbContext(ResolveOptions(tenantContext, connectionFactory))
{
    private static DbContextOptions<AppDbContext> ResolveOptions(
        ITenantContext tenantContext,
        ICachedTenantConnectionFactory connectionFactory)
    {
        var connectionString = connectionFactory.GetConnectionString(tenantContext.TenantId)
            ?? throw new InvalidOperationException(
                $"No connection string for tenant {tenantContext.TenantId}");

        return new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;
    }

    public Guid TenantId => tenantContext.TenantId;
    public Guid UserId => tenantContext.UserId;

    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();
    public DbSet<SuperAdmin> SuperAdmins => Set<SuperAdmin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantUser>(entity =>
        {
            entity.ToTable("tenant_users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.RoleCode).IsRequired().HasMaxLength(64);
        });

        modelBuilder.Entity<SuperAdmin>(entity =>
        {
            entity.ToTable("super_admins");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
        });

        base.OnModelCreating(modelBuilder);
    }
}
