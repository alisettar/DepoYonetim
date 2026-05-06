using MediatR;
using WMS.Application.Catalog.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Units.Queries;

public record ListUnitsQuery : IRequest<Result<List<UnitDto>>>;

public class ListUnitsHandler(IUnitRepository repo) : IRequestHandler<ListUnitsQuery, Result<List<UnitDto>>>
{
    public async Task<Result<List<UnitDto>>> Handle(ListUnitsQuery request, CancellationToken ct)
    {
        var units = await repo.GetAllAsync(ct);
        return Result.Success(units.Select(UnitDto.FromEntity).ToList());
    }
}
