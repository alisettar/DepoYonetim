using WMS.Application.Dashboard.Dtos;

namespace WMS.Application.Dashboard;

public interface IDashboardRepository
{
    Task<List<CriticalStockItem>> GetCriticalStockAsync(CancellationToken ct);
    Task<List<WarehouseFillItem>> GetWarehouseFillAsync(CancellationToken ct);
    Task<List<RecentMovementDto>> GetRecentMovementsAsync(int count, CancellationToken ct);
    Task<List<LotSearchItem>> SearchLotsAsync(string query, Guid? productId, CancellationToken ct);
}
