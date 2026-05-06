using MediatR;
using WMS.Application.Dashboard;
using WMS.Application.Dashboard.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Dashboard.Queries;

public record GetRecentMovementsQuery(int Count = 20) : IRequest<Result<List<RecentMovementDto>>>;

public class GetRecentMovementsHandler(IDashboardRepository repo)
    : IRequestHandler<GetRecentMovementsQuery, Result<List<RecentMovementDto>>>
{
    public async Task<Result<List<RecentMovementDto>>> Handle(GetRecentMovementsQuery request, CancellationToken ct)
    {
        var items = await repo.GetRecentMovementsAsync(request.Count, ct);
        return Result.Success(items);
    }
}
