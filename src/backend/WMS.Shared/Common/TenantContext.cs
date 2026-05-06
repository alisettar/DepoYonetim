namespace WMS.Shared.Common;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; set; }
    public string TenantCode { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string ActorType { get; set; } = "tenant_user";

    public static TenantContext FromClaims(
        Guid tenantId,
        string tenantCode,
        Guid userId,
        string email,
        string actorType)
    {
        return new TenantContext
        {
            TenantId = tenantId,
            TenantCode = tenantCode,
            UserId = userId,
            Email = email,
            ActorType = actorType
        };
    }
}
