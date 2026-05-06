using MediatR;
using WMS.Application.Dashboard;
using WMS.Application.Dashboard.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Dashboard.Queries;

public record LotSearchQuery(string Query, Guid? ProductId = null) : IRequest<Result<List<LotSearchItem>>>;

public class LotSearchHandler(IDashboardRepository repo)
    : IRequestHandler<LotSearchQuery, Result<List<LotSearchItem>>>
{
    public async Task<Result<List<LotSearchItem>>> Handle(LotSearchQuery request, CancellationToken ct)
    {
        var items = await repo.SearchLotsAsync(request.Query, request.ProductId, ct);
        return Result.Success(items);
    }
}
