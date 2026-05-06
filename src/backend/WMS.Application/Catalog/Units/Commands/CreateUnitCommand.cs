using MediatR;
using WMS.Application.Catalog.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Units.Commands;

public record CreateUnitCommand(string Code, string Name) : IRequest<Result<UnitDto>>;

public class CreateUnitHandler(IUnitRepository repo) : IRequestHandler<CreateUnitCommand, Result<UnitDto>>
{
    public async Task<Result<UnitDto>> Handle(CreateUnitCommand request, CancellationToken ct)
    {
        if (await repo.CodeExistsAsync(request.Code, ct))
            return Result.Failure<UnitDto>("CODE_EXISTS", $"'{request.Code}' kodlu birim zaten mevcut.");

        try
        {
            var unit = Domain.Catalog.Unit.Create(Guid.NewGuid(), request.Code, request.Name);
            repo.Add(unit);
            await repo.SaveAsync(ct);
            return Result.Success(UnitDto.FromEntity(unit));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<UnitDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
