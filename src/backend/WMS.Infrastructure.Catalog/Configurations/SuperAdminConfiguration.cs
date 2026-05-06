namespace WMS.Infrastructure.Catalog.Configurations;

// Already configured inline in CatalogDbContext.OnModelCreating
// This file is a marker for architectural clarity
public static class SuperAdminConfiguration
{
    public const string EmailComputedColumn = "lower(email)";
}
