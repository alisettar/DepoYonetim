using Microsoft.EntityFrameworkCore;
using WMS.Application.Catalog;
using WMS.Domain.Catalog;
using WMS.Infrastructure.Persistence;

namespace WMS.Infrastructure.Repositories;

public class UnitRepository(AppDbContext db) : IUnitRepository
{
    public Task<List<Unit>> GetAllAsync(CancellationToken ct) =>
        db.Units.OrderBy(u => u.Code).ToListAsync(ct);

    public Task<Unit?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Units.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.Units.AnyAsync(u => u.Code == code.Trim().ToUpperInvariant(), ct);

    public void Add(Unit unit) => db.Units.Add(unit);
    public void Remove(Unit unit) => db.Units.Remove(unit);
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
