using MediatR;
using WMS.Application.Catalog.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Units.Commands;

public record UpdateUnitCommand(Guid Id, string Name) : IRequest<Result<UnitDto>>;

public class UpdateUnitHandler(IUnitRepository repo) : IRequestHandler<UpdateUnitCommand, Result<UnitDto>>
{
    public async Task<Result<UnitDto>> Handle(UpdateUnitCommand request, CancellationToken ct)
    {
        var unit = await repo.GetByIdAsync(request.Id, ct);
        if (unit is null)
            return Result.Failure<UnitDto>("NOT_FOUND", "Birim bulunamadı.");

        try
        {
            unit.Update(request.Name);
            await repo.SaveAsync(ct);
            return Result.Success(UnitDto.FromEntity(unit));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<UnitDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
