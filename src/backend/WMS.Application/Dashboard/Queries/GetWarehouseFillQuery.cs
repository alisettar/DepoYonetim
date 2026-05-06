using MediatR;
using WMS.Application.Dashboard;
using WMS.Application.Dashboard.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Dashboard.Queries;

public record GetWarehouseFillQuery : IRequest<Result<List<WarehouseFillItem>>>;

public class GetWarehouseFillHandler(IDashboardRepository repo)
    : IRequestHandler<GetWarehouseFillQuery, Result<List<WarehouseFillItem>>>
{
    public async Task<Result<List<WarehouseFillItem>>> Handle(GetWarehouseFillQuery request, CancellationToken ct)
    {
        var items = await repo.GetWarehouseFillAsync(ct);
        return Result.Success(items);
    }
}
