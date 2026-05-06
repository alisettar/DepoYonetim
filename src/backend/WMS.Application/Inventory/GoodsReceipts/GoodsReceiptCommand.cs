using MediatR;
using WMS.Application.Catalog;
using WMS.Application.Inventory.Dtos;
using WMS.Domain.Inventory;
using WMS.Domain.Inventory.Enums;
using WMS.Shared.Result;

namespace WMS.Application.Inventory.GoodsReceipts;

public record GoodsReceiptItem(
    Guid ProductId,
    string? LotNumber,
    DateTime? ProductionDate,
    DateTime? ExpiryDate,
    Guid WarehouseId,
    Guid? LocationId,
    decimal Quantity,
    Guid UnitId,
    decimal? UnitCost);

public record GoodsReceiptCommand(
    IReadOnlyList<GoodsReceiptItem> Items,
    string? Note = null,
    Guid? ReferenceId = null) : IRequest<Result<List<StockMovementDto>>>;

public class GoodsReceiptHandler(
    IProductRepository productRepo,
    ILotRepository lotRepo,
    IStockMovementRepository movementRepo,
    IStockBalanceRepository balanceRepo,
    IFifoLayerRepository fifoRepo)
    : IRequestHandler<GoodsReceiptCommand, Result<List<StockMovementDto>>>
{
    private static readonly Guid SystemUserId = Guid.Empty;

    public async Task<Result<List<StockMovementDto>>> Handle(GoodsReceiptCommand request, CancellationToken ct)
    {
        var movements = new List<StockMovement>();

        foreach (var item in request.Items)
        {
            var product = await productRepo.GetByIdAsync(item.ProductId, ct);
            if (product == null)
                return Result.Failure<List<StockMovementDto>>("PRODUCT_NOT_FOUND", $"Ürün bulunamadı: {item.ProductId}");

            if (product.LotRequired && string.IsNullOrEmpty(item.LotNumber))
                return Result.Failure<List<StockMovementDto>>("LOT_REQUIRED", $"'{product.Name}' ürünü için lot numarası zorunludur.");

            Guid? lotId = null;
            if (!string.IsNullOrEmpty(item.LotNumber))
            {
                var lot = await lotRepo.FindByNumberAsync(item.ProductId, item.LotNumber, ct);
                if (lot == null)
                {
                    lot = Lot.Create(Guid.NewGuid(), item.ProductId, item.LotNumber, LotSource.Receipt,
                        item.ProductionDate, item.ExpiryDate);
                    lotRepo.Add(lot);
                }
                lotId = lot.Id;
            }

            var movement = StockMovement.CreateGoodsReceipt(
                item.ProductId, lotId, item.WarehouseId, item.LocationId,
                item.Quantity, item.UnitId, item.UnitCost, SystemUserId,
                request.Note, request.ReferenceId);
            movementRepo.Add(movement);
            movements.Add(movement);

            var balance = await balanceRepo.FindAsync(item.ProductId, lotId, item.WarehouseId, null, ct);
            if (balance == null)
            {
                balance = StockBalance.Create(item.ProductId, lotId, item.WarehouseId, null, item.Quantity);
                balanceRepo.Add(balance);
            }
            else
            {
                balance.Apply(item.Quantity);
            }

            var layer = FifoLayer.Create(item.ProductId, item.WarehouseId, null, item.LocationId, lotId,
                DateTime.UtcNow, item.Quantity, item.UnitCost, movement.Id);
            fifoRepo.Add(layer);
        }

        await movementRepo.SaveAsync(ct);
        return Result.Success(movements.Select(StockMovementDto.FromEntity).ToList());
    }
}
