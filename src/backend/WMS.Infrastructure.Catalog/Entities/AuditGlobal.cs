namespace WMS.Infrastructure.Catalog.Entities;

public class AuditGlobal
{
    public Guid Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public Guid ActorId { get; set; }
    public string ActorType { get; set; } = string.Empty; // "super_admin" | "system"
    public string Action { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public string? Ip { get; set; }
}
