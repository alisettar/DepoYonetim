using WMS.Domain.Common;

namespace WMS.Domain.Catalog;

public record TenantCreatedEvent(Guid TenantId, string Code, string Name) : DomainEvent;
public record TenantStatusChangedEvent(Guid TenantId, string OldStatus, string NewStatus) : DomainEvent;
