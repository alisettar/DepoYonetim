using MediatR;
using WMS.Application.Dashboard;
using WMS.Application.Dashboard.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Dashboard.Queries;

public record GetCriticalStockQuery : IRequest<Result<List<CriticalStockItem>>>;

public class GetCriticalStockHandler(IDashboardRepository repo)
    : IRequestHandler<GetCriticalStockQuery, Result<List<CriticalStockItem>>>
{
    public async Task<Result<List<CriticalStockItem>>> Handle(GetCriticalStockQuery request, CancellationToken ct)
    {
        var items = await repo.GetCriticalStockAsync(ct);
        return Result.Success(items);
    }
}
