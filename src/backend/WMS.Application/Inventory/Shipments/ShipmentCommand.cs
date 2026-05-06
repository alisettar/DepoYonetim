using MediatR;
using WMS.Application.Inventory.Dtos;
using WMS.Domain.Inventory;
using WMS.Domain.Inventory.Services;
using WMS.Shared.Exceptions;
using WMS.Shared.Result;

namespace WMS.Application.Inventory.Shipments;

public record ShipmentItem(Guid ProductId, Guid WarehouseId, decimal Quantity, Guid UnitId);

public record ShipmentCommand(
    IReadOnlyList<ShipmentItem> Items,
    string? Note = null) : IRequest<Result<List<StockMovementDto>>>;

public class ShipmentHandler(
    IStockMovementRepository movementRepo,
    IStockBalanceRepository balanceRepo,
    IFifoLayerRepository fifoRepo)
    : IRequestHandler<ShipmentCommand, Result<List<StockMovementDto>>>
{
    private static readonly Guid SystemUserId = Guid.Empty;

    public async Task<Result<List<StockMovementDto>>> Handle(ShipmentCommand request, CancellationToken ct)
    {
        var movements = new List<StockMovement>();

        foreach (var item in request.Items)
        {
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

                var movement = StockMovement.CreateShipment(
                    item.ProductId, c.LotId, item.WarehouseId,
                    c.Quantity, item.UnitId, c.UnitCost, SystemUserId, request.Note);
                movementRepo.Add(movement);
                movements.Add(movement);

                var balance = await balanceRepo.FindAsync(item.ProductId, c.LotId, item.WarehouseId, null, ct);
                balance?.Apply(-c.Quantity);
            }
        }

        await movementRepo.SaveAsync(ct);
        return Result.Success(movements.Select(StockMovementDto.FromEntity).ToList());
    }
}
