using Microsoft.EntityFrameworkCore;
using WMS.Application.Dashboard;
using WMS.Application.Dashboard.Dtos;
using WMS.Infrastructure.Persistence;

namespace WMS.Infrastructure.Services;

public class DashboardRepository(AppDbContext db) : IDashboardRepository
{
    public async Task<List<CriticalStockItem>> GetCriticalStockAsync(CancellationToken ct)
    {
        var products = await db.Products
            .Where(p => p.Status == WMS.Domain.Catalog.ProductStatus.Active && p.MinStock.HasValue)
            .ToListAsync(ct);

        if (!products.Any())
            return [];

        var productIds = products.Select(p => p.Id).ToList();

        var balances = await db.StockBalances
            .Where(b => productIds.Contains(b.ProductId))
            .GroupBy(b => b.ProductId)
            .Select(g => new { ProductId = g.Key, TotalQty = g.Sum(b => b.Quantity) })
            .ToListAsync(ct);

        var units = await db.Units.ToListAsync(ct);

        return products
            .Select(p => new
            {
                Product = p,
                Qty = balances.FirstOrDefault(b => b.ProductId == p.Id)?.TotalQty ?? 0
            })
            .Where(x => x.Qty < x.Product.MinStock!.Value)
            .OrderBy(x => (double)x.Qty / (double)x.Product.MinStock!.Value)
            .Select(x => new CriticalStockItem(
                x.Product.Id,
                x.Product.Code,
                x.Product.Name,
                x.Product.PrimaryUnitId,
                units.FirstOrDefault(u => u.Id == x.Product.PrimaryUnitId)?.Code ?? "",
                x.Qty,
                x.Product.MinStock.Value,
                (double)(x.Qty / x.Product.MinStock.Value)))
            .ToList();
    }

    public async Task<List<WarehouseFillItem>> GetWarehouseFillAsync(CancellationToken ct)
    {
        var locations = await db.WarehouseLocations
            .Where(l => l.IsActive)
            .ToListAsync(ct);

        if (!locations.Any())
            return [];

        var warehouseIds = locations.Select(l => l.WarehouseId).Distinct().ToList();
        var warehouses = await db.Warehouses
            .Where(w => warehouseIds.Contains(w.Id))
            .ToListAsync(ct);

        var warehouseIdLookup = warehouses.ToDictionary(w => w.Id, w => w.Name);

        var balances = await db.StockBalances
            .Where(b => warehouseIds.Contains(b.WarehouseId!.Value))
            .GroupBy(b => new { b.ProductId, b.WarehouseId })
            .Select(g => new { ProductId = g.Key.ProductId, WarehouseId = g.Key.WarehouseId, TotalQty = g.Sum(b => b.Quantity) })
            .ToListAsync(ct);

        var warehouseFill = new Dictionary<Guid, decimal>();
        foreach (var b in balances)
        {
            if (b.WarehouseId.HasValue)
            {
                var key = b.WarehouseId.Value;
                warehouseFill.TryGetValue(key, out var existing);
                warehouseFill[key] = existing + b.TotalQty;
            }
        }

        return locations.Select(l => new WarehouseFillItem(
            l.WarehouseId,
            warehouseIdLookup.TryGetValue(l.WarehouseId, out var wname) ? wname : "",
            l.Id,
            l.Code,
            l.Name,
            warehouseFill.TryGetValue(l.WarehouseId, out var filled) ? filled : 0,
            l.Capacity,
            l.Capacity.HasValue && l.Capacity.Value > 0
                ? (double)Math.Clamp(warehouseFill.TryGetValue(l.WarehouseId, out var f) ? (double)(f / l.Capacity.Value) : 0.0, 0.0, 1.0)
                : (double?)null)).ToList();
    }

    public async Task<List<RecentMovementDto>> GetRecentMovementsAsync(int count, CancellationToken ct)
    {
        var movements = await db.StockMovements
            .OrderByDescending(m => m.OccurredAt)
            .Take(count)
            .ToListAsync(ct);

        var productIds = movements.Select(m => m.ProductId).Distinct().ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p);

        var unitIds = movements.Select(m => m.UnitId).Distinct().ToList();
        var units = await db.Units
            .Where(u => unitIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u);

        var warehouseIds = movements.Where(m => m.WarehouseId.HasValue).Select(m => m.WarehouseId!.Value).Distinct().ToList();
        var warehouses = await db.Warehouses
            .Where(w => warehouseIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, w => w);

        var lotIds = movements.Where(m => m.LotId.HasValue).Select(m => m.LotId!.Value).Distinct().ToList();
        var lots = await db.Lots
            .Where(l => lotIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l);

        return movements.Select(m => new RecentMovementDto(
            m.Id,
            m.OccurredAt,
            m.Type.ToString(),
            products.TryGetValue(m.ProductId, out var p) ? p?.Code ?? "" : "",
            products.TryGetValue(m.ProductId, out var p2) ? p2?.Name ?? "" : "",
            m.Quantity,
            units.TryGetValue(m.UnitId, out var u) ? u?.Code ?? "" : "",
            m.LotId,
            m.LotId.HasValue && lots.TryGetValue(m.LotId.Value, out var lot) ? lot?.LotNumber : null,
            m.WarehouseId.HasValue && warehouses.TryGetValue(m.WarehouseId.Value, out var wh) ? wh?.Name : null,
            m.Note,
            m.CreatedByUserId)).ToList();
    }

    public async Task<List<LotSearchItem>> SearchLotsAsync(string query, Guid? productId, CancellationToken ct)
    {
        var queryable = db.Lots.AsQueryable();

        if (productId.HasValue)
            queryable = queryable.Where(l => l.ProductId == productId.Value);

        if (!string.IsNullOrEmpty(query))
            queryable = queryable.Where(l => l.LotNumber.Contains(query));

        var lots = await queryable
            .OrderByDescending(l => l.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

        var productIds = lots.Select(l => l.ProductId).Distinct().ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p);

        var lotIds = lots.Select(l => l.Id).ToList();
        var balances = await db.StockBalances
            .Where(b => b.LotId.HasValue && lotIds.Contains(b.LotId.Value))
            .GroupBy(b => b.LotId!.Value)
            .Select(g => new { LotId = g.Key, TotalQty = g.Sum(b => b.Quantity) })
            .ToListAsync(ct);

        return lots.Select(l => new LotSearchItem(
            l.Id,
            l.LotNumber,
            l.ProductId,
            products.TryGetValue(l.ProductId, out var p) ? p?.Code ?? "" : "",
            products.TryGetValue(l.ProductId, out var p2) ? p2?.Name ?? "" : "",
            l.QualityStatus.ToString(),
            l.ExpiryDate,
            l.ProductionDate,
            balances.FirstOrDefault(b => b.LotId == l.Id)?.TotalQty)).ToList();
    }
}
