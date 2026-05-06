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

    public void Add(Lot lot) => db.Lots.Add(lot);

    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
