using Microsoft.EntityFrameworkCore;
using WMS.Infrastructure.Services;
using WMS.Shared.Common;

namespace WMS.Infrastructure.Persistence;

public class AppDbContextFactory : IDbContextFactory<AppDbContext>
{
    private readonly ITenantContext _tenantContext;
    private readonly ICachedTenantConnectionFactory _connectionFactory;

    public AppDbContextFactory(
        ITenantContext tenantContext,
        ICachedTenantConnectionFactory connectionFactory)
    {
        _tenantContext = tenantContext;
        _connectionFactory = connectionFactory;
    }

    public AppDbContext CreateDbContext()
    {
        var connectionString = _connectionFactory.GetConnectionString(_tenantContext.TenantId)
            ?? throw new InvalidOperationException($"No connection string for tenant {_tenantContext.TenantId}");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options, _tenantContext, connectionString);
    }
}
