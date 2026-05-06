using MediatR;
using WMS.Application.Warehousing.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Warehousing.Commands;

public record UpdateWarehouseCommand(
    Guid Id,
    string Name,
    string? Address = null) : IRequest<Result<WarehouseDto>>;

public class UpdateWarehouseHandler(IWarehouseRepository repo)
    : IRequestHandler<UpdateWarehouseCommand, Result<WarehouseDto>>
{
    public async Task<Result<WarehouseDto>> Handle(UpdateWarehouseCommand request, CancellationToken ct)
    {
        var warehouse = await repo.GetByIdAsync(request.Id, ct);
        if (warehouse is null)
            return Result.Failure<WarehouseDto>("NOT_FOUND", "Depo bulunamadı.");

        try
        {
            warehouse.Update(request.Name, request.Address);
            await repo.SaveAsync(ct);
            return Result.Success(WarehouseDto.FromEntity(warehouse));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<WarehouseDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
