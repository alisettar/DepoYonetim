using MediatR;
using WMS.Application.Inventory.Dtos;
using WMS.Shared.Common.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Inventory.Queries;

public record ListLotsQuery(
    Guid? ProductId = null,
    string? LotNumberFilter = null,
    string? QualityStatus = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PaginatedResponse<LotDto>>>;

public class ListLotsHandler(ILotRepository repo)
    : IRequestHandler<ListLotsQuery, Result<PaginatedResponse<LotDto>>>
{
    public async Task<Result<PaginatedResponse<LotDto>>> Handle(ListLotsQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await repo.GetAllPagedAsync(
            request.ProductId, request.LotNumberFilter, request.QualityStatus,
            request.Page, request.PageSize, ct);

        return Result.Success(new PaginatedResponse<LotDto>(
            items.Select(LotDto.FromEntity).ToList(),
            request.Page, request.PageSize, totalCount,
            (int)Math.Ceiling(totalCount / (double)request.PageSize)));
    }
}