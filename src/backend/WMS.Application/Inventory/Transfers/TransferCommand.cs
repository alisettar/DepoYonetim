using MediatR;
using WMS.Application.Inventory.Dtos;
using WMS.Domain.Inventory;
using WMS.Domain.Inventory.Services;
using WMS.Shared.Exceptions;
using WMS.Shared.Result;

namespace WMS.Application.Inventory.Transfers;

public record TransferItem(
    Guid ProductId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    decimal Quantity,
    Guid UnitId);

public record TransferCommand(
    IReadOnlyList<TransferItem> Items,
    string? Note = null) : IRequest<Result<List<StockMovementDto>>>;

public class TransferHandler(
    IStockMovementRepository movementRepo,
    IStockBalanceRepository balanceRepo,
    IFifoLayerRepository fifoRepo)
    : IRequestHandler<TransferCommand, Result<List<StockMovementDto>>>
{
    private static readonly Guid SystemUserId = Guid.Empty;

    public async Task<Result<List<StockMovementDto>>> Handle(TransferCommand request, CancellationToken ct)
    {
        var movements = new List<StockMovement>();

        foreach (var item in request.Items)
        {
            var sourceLayers = await fifoRepo.GetAvailableLayersAsync(item.ProductId, item.SourceWarehouseId, null, ct);

            IReadOnlyList<FifoConsumption> consumptions;
            try
            {
                consumptions = FifoEngine.Consume(sourceLayers, item.Quantity);
            }
            catch (BusinessException ex)
            {
                return Result.Failure<List<StockMovementDto>>(ex.ErrorCode ?? "INSUFFICIENT_STOCK", ex.Message);
            }

            foreach (var c in consumptions)
            {
                // Consume from source FIFO layer
                var sourceLayer = sourceLayers.First(l => l.Id == c.LayerId);
                sourceLayer.Consume(c.Quantity);

                // Transfer out from source
                var outMovement = StockMovement.CreateTransferOut(
                    item.ProductId, c.LotId, item.SourceWarehouseId,
                    c.Quantity, item.UnitId, c.UnitCost, SystemUserId);
                movementRepo.Add(outMovement);
                movements.Add(outMovement);

                // Reduce source balance
                var sourceBalance = await balanceRepo.FindAsync(item.ProductId, c.LotId, item.SourceWarehouseId, null, ct);
                sourceBalance?.Apply(-c.Quantity);

                // Create new FIFO layer at destination — preserve original ReceiptDate and UnitCost
                var destLayer = FifoLayer.Create(
                    item.ProductId, item.DestinationWarehouseId, null, null, c.LotId,
                    c.ReceiptDate, c.Quantity, c.UnitCost, outMovement.Id);
                fifoRepo.Add(destLayer);

                // Transfer in to destination
                var inMovement = StockMovement.CreateTransferIn(
                    item.ProductId, c.LotId, item.DestinationWarehouseId,
                    c.Quantity, item.UnitId, c.UnitCost, SystemUserId);
                movementRepo.Add(inMovement);
                movements.Add(inMovement);

                // Update destination balance
                var destBalance = await balanceRepo.FindAsync(item.ProductId, c.LotId, item.DestinationWarehouseId, null, ct);
                if (destBalance == null)
                {
                    destBalance = StockBalance.Create(item.ProductId, c.LotId, item.DestinationWarehouseId, null, c.Quantity);
                    balanceRepo.Add(destBalance);
                }
                else
                {
                    destBalance.Apply(c.Quantity);
                }
            }
        }

        await movementRepo.SaveAsync(ct);
        return Result.Success(movements.Select(StockMovementDto.FromEntity).ToList());
    }
}
