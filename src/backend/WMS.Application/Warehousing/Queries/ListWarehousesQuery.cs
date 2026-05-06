using MediatR;
using WMS.Application.Warehousing.Dtos;
using WMS.Domain.Warehousing;

namespace WMS.Application.Warehousing.Queries;

public record ListWarehousesQuery(WarehouseType? Type = null) : IRequest<List<WarehouseDto>>;

public class ListWarehousesHandler(IWarehouseRepository repo)
    : IRequestHandler<ListWarehousesQuery, List<WarehouseDto>>
{
    public async Task<List<WarehouseDto>> Handle(ListWarehousesQuery request, CancellationToken ct)
    {
        var warehouses = await repo.GetAllAsync(request.Type, ct);
        return warehouses.Select(WarehouseDto.FromEntity).ToList();
    }
}
