using MediatR;
using WMS.Application.Inventory.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Inventory.Queries;

public record GetBalancesQuery(
    Guid? ProductId = null,
    Guid? WarehouseId = null) : IRequest<Result<List<StockBalanceDto>>>;

public class GetBalancesHandler(IStockBalanceRepository repo)
    : IRequestHandler<GetBalancesQuery, Result<List<StockBalanceDto>>>
{
    public async Task<Result<List<StockBalanceDto>>> Handle(GetBalancesQuery request, CancellationToken ct)
    {
        var balances = await repo.GetAllAsync(request.ProductId, request.WarehouseId, ct);
        return Result.Success(balances.Select(StockBalanceDto.FromEntity).ToList());
    }
}
