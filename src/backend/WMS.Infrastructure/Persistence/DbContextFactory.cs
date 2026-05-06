using Microsoft.EntityFrameworkCore;
using WMS.Infrastructure.Services;
using WMS.Shared.Common;

namespace WMS.Infrastructure.Persistence;

public class AppDbContextFactory(
    ITenantContext tenantContext,
    ICachedTenantConnectionFactory connectionFactory) : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext()
    {
        var connectionString = connectionFactory.GetConnectionString(tenantContext.TenantId)
            ?? throw new InvalidOperationException($"No connection string for tenant {tenantContext.TenantId}");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(tenantContext, connectionFactory);
    }
}
