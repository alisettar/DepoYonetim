using MediatR;
using WMS.Shared.Result;

namespace WMS.Application.Admin.Queries;

public record TenantHealthQuery(Guid TenantId) : IRequest<Result<TenantHealthDto>>;

public record TenantHealthDto(
    Guid TenantId,
    string Status,
    string? ConnectionStatus,
    string? ErrorMessage);

public class TenantHealthHandler : IRequestHandler<TenantHealthQuery, Result<TenantHealthDto>>
{
    public Task<Result<TenantHealthDto>> Handle(TenantHealthQuery request, CancellationToken ct)
    {
        var dto = new TenantHealthDto(request.TenantId, "Unknown", null, null);
        return Result<TenantHealthDto>.Success(dto).ToTask();
    }
}
