using MediatR;
using WMS.Application.Catalog.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Units.Queries;

public record GetUnitQuery(Guid Id) : IRequest<Result<UnitDto>>;

public class GetUnitHandler(IUnitRepository repo) : IRequestHandler<GetUnitQuery, Result<UnitDto>>
{
    public async Task<Result<UnitDto>> Handle(GetUnitQuery request, CancellationToken ct)
    {
        var unit = await repo.GetByIdAsync(request.Id, ct);
        return unit is null
            ? Result.Failure<UnitDto>("NOT_FOUND", "Birim bulunamadı.")
            : Result.Success(UnitDto.FromEntity(unit));
    }
}
