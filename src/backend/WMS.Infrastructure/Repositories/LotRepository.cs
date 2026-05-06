using Microsoft.EntityFrameworkCore;
using WMS.Application.Inventory;
using WMS.Domain.Inventory;
using WMS.Infrastructure.Persistence;

namespace WMS.Infrastructure.Repositories;

public class LotRepository(AppDbContext db) : ILotRepository
{
    public Task<Lot?> FindByNumberAsync(Guid productId, string lotNumber, CancellationToken ct)
        => db.Lots.FirstOrDefaultAsync(l => l.ProductId == productId && l.LotNumber == lotNumber, ct);

    public Task<List<Lot>> GetAllAsync(CancellationToken ct)
        => db.Lots.OrderBy(l => l.CreatedAt).ToListAsync(ct);

    public Task<Lot?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Lots.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<(List<Lot> Items, int TotalCount)> GetAllPagedAsync(
        Guid? productId, string? lotNumberFilter, string? qualityStatus,
        int page, int pageSize, CancellationToken ct)
    {
        var query = db.Lots.AsQueryable();

        if (productId.HasValue)
            query = query.Where(l => l.ProductId == productId.Value);

        if (!string.IsNullOrEmpty(lotNumberFilter))
            query = query.Where(l => l.LotNumber.Contains(lotNumberFilter));

        if (!string.IsNullOrEmpty(qualityStatus))
            query = query.Where(l => l.QualityStatus.ToString() == qualityStatus);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public void Add(Lot lot) => db.Lots.Add(lot);

    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
