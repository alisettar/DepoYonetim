using MediatR;
using WMS.Application.Inventory.Dtos;
using WMS.Domain.Inventory.Enums;
using WMS.Shared.Result;

namespace WMS.Application.Inventory.Queries;

public record ListMovementsQuery(
    MovementType? Type = null,
    Guid? ProductId = null,
    Guid? WarehouseId = null) : IRequest<Result<List<StockMovementDto>>>;

public class ListMovementsHandler(IStockMovementRepository repo)
    : IRequestHandler<ListMovementsQuery, Result<List<StockMovementDto>>>
{
    public async Task<Result<List<StockMovementDto>>> Handle(ListMovementsQuery request, CancellationToken ct)
    {
        var movements = await repo.GetAllAsync(request.Type, request.ProductId, request.WarehouseId, ct);
        return Result.Success(movements.Select(StockMovementDto.FromEntity).ToList());
    }
}
