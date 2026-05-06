using MediatR;
using WMS.Application.Inventory.Adjustments;
using WMS.Application.Inventory.Commands;
using WMS.Application.Inventory.GoodsReceipts;
using WMS.Application.Inventory.Queries;
using WMS.Application.Inventory.Shipments;
using WMS.Application.Inventory.Transfers;
using WMS.Domain.Inventory.Enums;

namespace WMS.Api.Endpoints;

public static class InventoryEndpoints
{
    public static WebApplication MapInventoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/inventory").WithTags("Inventory");

        group.MapPost("/goods-receipts", async (GoodsReceiptRequest req, ISender sender) =>
        {
            var result = await sender.Send(new GoodsReceiptCommand(
                req.Items.Select(i => new GoodsReceiptItem(
                    i.ProductId, i.LotNumber, i.ProductionDate, i.ExpiryDate,
                    i.WarehouseId, i.LocationId, i.Quantity, i.UnitId, i.UnitCost)).ToList(),
                req.Note));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("CreateGoodsReceipt");

        group.MapPost("/shipments", async (ShipmentRequest req, ISender sender) =>
        {
            var result = await sender.Send(new ShipmentCommand(
                req.Items.Select(i => new ShipmentItem(
                    i.ProductId, i.WarehouseId, i.Quantity, i.UnitId)).ToList(),
                req.Note));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("CreateShipment");

        group.MapPost("/transfers", async (TransferRequest req, ISender sender) =>
        {
            var result = await sender.Send(new TransferCommand(
                req.Items.Select(i => new TransferItem(
                    i.ProductId, i.SourceWarehouseId, i.DestinationWarehouseId,
                    i.Quantity, i.UnitId)).ToList(),
                req.Note));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("CreateTransfer");

        group.MapPost("/adjustments", async (AdjustmentRequest req, ISender sender) =>
        {
            var result = await sender.Send(new AdjustmentCommand(
                req.Items.Select(i => new AdjustmentItem(
                    i.ProductId, i.LotId, i.WarehouseId, i.IsIn,
                    i.Quantity, i.UnitId, i.UnitCost, i.Reason)).ToList()));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("CreateAdjustment");

        group.MapGet("/movements", async (string? type, Guid? productId, Guid? warehouseId, ISender sender) =>
        {
            MovementType? mt = type != null && Enum.TryParse<MovementType>(type, out var parsed) ? parsed : null;
            var result = await sender.Send(new ListMovementsQuery(mt, productId, warehouseId));
            return Results.Ok(result.Value);
        }).WithName("ListMovements");

        group.MapGet("/balances", async (Guid? productId, Guid? warehouseId, ISender sender) =>
        {
            var result = await sender.Send(new GetBalancesQuery(productId, warehouseId));
            return Results.Ok(result.Value);
        }).WithName("GetBalances");

        // Lot Traceability: List lots with filters and pagination
        group.MapGet("/lots", async (
            Guid? productId,
            string? lotNumberFilter,
            string? qualityStatus,
            int page,
            int pageSize,
            ISender sender) =>
        {
            var result = await sender.Send(new ListLotsQuery(productId, lotNumberFilter, qualityStatus, page, pageSize));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("ListLots");

        // Lot traceability: Get lot detail with movements
        group.MapGet("/lots/{lotId:guid}/movements", async (Guid lotId, ISender sender) =>
        {
            var result = await sender.Send(new GetLotDetailQuery(lotId));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { result.ErrorCode, result.Message });
        }).WithName("GetLotMovements");

        // Lot traceability: Get full trace chain
        group.MapGet("/lots/{lotId:guid}/trace", async (Guid lotId, ISender sender) =>
        {
            var result = await sender.Send(new GetLotTraceQuery(lotId));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { result.ErrorCode, result.Message });
        }).WithName("GetLotTrace");

        // Lot traceability: Update quality status
        group.MapPatch("/lots/{lotId:guid}/quality-status", async (Guid lotId, UpdateQualityStatusRequest req, ISender sender) =>
        {
            var result = await sender.Send(new UpdateLotQualityStatusCommand(lotId, req.QualityStatus));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ErrorCode == "LOT_NOT_FOUND"
                    ? Results.NotFound(new { result.ErrorCode, result.Message })
                    : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("UpdateLotQualityStatus");

        return app;
    }

    public record GoodsReceiptItemRequest(
        Guid ProductId,
        string? LotNumber,
        DateTime? ProductionDate,
        DateTime? ExpiryDate,
        Guid WarehouseId,
        Guid? LocationId,
        decimal Quantity,
        Guid UnitId,
        decimal? UnitCost);

    public record GoodsReceiptRequest(
        IReadOnlyList<GoodsReceiptItemRequest> Items,
        string? Note = null);

    public record ShipmentItemRequest(Guid ProductId, Guid WarehouseId, decimal Quantity, Guid UnitId);

    public record ShipmentRequest(
        IReadOnlyList<ShipmentItemRequest> Items,
        string? Note = null);

    public record TransferItemRequest(
        Guid ProductId,
        Guid SourceWarehouseId,
        Guid DestinationWarehouseId,
        decimal Quantity,
        Guid UnitId);

    public record TransferRequest(
        IReadOnlyList<TransferItemRequest> Items,
        string? Note = null);

    public record AdjustmentItemRequest(
        Guid ProductId,
        Guid? LotId,
        Guid WarehouseId,
        bool IsIn,
        decimal Quantity,
        Guid UnitId,
        decimal? UnitCost,
        string? Reason);

    public record AdjustmentRequest(IReadOnlyList<AdjustmentItemRequest> Items);

    public record UpdateQualityStatusRequest(string QualityStatus);
}
