using MediatR;
using WMS.Application.Inventory.Dtos;
using WMS.Domain.Inventory;
using WMS.Domain.Inventory.Services;
using WMS.Shared.Exceptions;
using WMS.Shared.Result;

namespace WMS.Application.Inventory.Adjustments;

public record AdjustmentItem(
    Guid ProductId,
    Guid? LotId,
    Guid WarehouseId,
    bool IsIn,
    decimal Quantity,
    Guid UnitId,
    decimal? UnitCost,
    string? Reason);

public record AdjustmentCommand(
    IReadOnlyList<AdjustmentItem> Items) : IRequest<Result<List<StockMovementDto>>>;

public class AdjustmentHandler(
    IStockMovementRepository movementRepo,
    IStockBalanceRepository balanceRepo,
    IFifoLayerRepository fifoRepo)
    : IRequestHandler<AdjustmentCommand, Result<List<StockMovementDto>>>
{
    private static readonly Guid SystemUserId = Guid.Empty;

    public async Task<Result<List<StockMovementDto>>> Handle(AdjustmentCommand request, CancellationToken ct)
    {
        var movements = new List<StockMovement>();

        foreach (var item in request.Items)
        {
            if (item.IsIn)
            {
                // Adjustment In
                var movement = StockMovement.CreateAdjustmentIn(
                    item.ProductId, item.LotId, item.WarehouseId,
                    item.Quantity, item.UnitId, item.UnitCost, item.Reason, SystemUserId);
                movementRepo.Add(movement);
                movements.Add(movement);

                var balance = await balanceRepo.FindAsync(item.ProductId, item.LotId, item.WarehouseId, null, ct);
                if (balance == null)
                {
                    balance = StockBalance.Create(item.ProductId, item.LotId, item.WarehouseId, null, item.Quantity);
                    balanceRepo.Add(balance);
                }
                else
                {
                    balance.Apply(item.Quantity);
                }

                var layer = FifoLayer.Create(
                    item.ProductId, item.WarehouseId, null, null, item.LotId,
                    DateTime.UtcNow, item.Quantity, item.UnitCost, movement.Id);
                fifoRepo.Add(layer);
            }
            else
            {
                // Adjustment Out
                var layers = await fifoRepo.GetAvailableLayersAsync(item.ProductId, item.WarehouseId, null, ct);

                IReadOnlyList<FifoConsumption> consumptions;
                try
                {
                    consumptions = FifoEngine.Consume(layers, item.Quantity);
                }
                catch (BusinessException ex)
                {
                    return Result.Failure<List<StockMovementDto>>(ex.ErrorCode ?? "INSUFFICIENT_STOCK", ex.Message);
                }

                foreach (var c in consumptions)
                {
                    var layer = layers.First(l => l.Id == c.LayerId);
                    layer.Consume(c.Quantity);

                    var movement = StockMovement.CreateAdjustmentOut(
                        item.ProductId, c.LotId, item.WarehouseId,
                        c.Quantity, item.UnitId, SystemUserId, item.Reason);
                    movementRepo.Add(movement);
                    movements.Add(movement);

                    var balance = await balanceRepo.FindAsync(item.ProductId, c.LotId, item.WarehouseId, null, ct);
                    balance?.Apply(-c.Quantity);
                }
            }
        }

        await movementRepo.SaveAsync(ct);
        return Result.Success(movements.Select(StockMovementDto.FromEntity).ToList());
    }
}
