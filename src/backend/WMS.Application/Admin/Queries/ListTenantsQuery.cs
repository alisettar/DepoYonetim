using MediatR;
using WMS.Application.Admin.Commands;
using WMS.Shared.Result;

namespace WMS.Application.Admin.Queries;

public record ListTenantsQuery(
    string? Status = null,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<PaginatedResponse<TenantDto>>>;

public record PaginatedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages) where T : class;

public class ListTenantsHandler : IRequestHandler<ListTenantsQuery, Result<PaginatedResponse<TenantDto>>>
{
    public Task<Result<PaginatedResponse<TenantDto>>> Handle(ListTenantsQuery request, CancellationToken ct)
    {
        var items = Enumerable.Empty<TenantDto>();
        var response = new PaginatedResponse<TenantDto>(items, request.Page, request.PageSize, 0, 0);
        return Result<PaginatedResponse<TenantDto>>.Success(response).ToTask();
    }
}
