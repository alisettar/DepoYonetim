namespace WMS.Shared.Common;

public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantCode { get; }
    Guid UserId { get; }
    string Email { get; }
    string ActorType { get; } // "super_admin" or "tenant_user"
}
