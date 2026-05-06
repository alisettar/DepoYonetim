using MediatR;
using WMS.Application.Warehousing.Dtos;
using WMS.Domain.Warehousing;
using WMS.Shared.Result;

namespace WMS.Application.Warehousing.Commands;

public record CreateWarehouseCommand(
    string Code,
    string Name,
    WarehouseType Type,
    string? Address = null,
    Guid? ParentWarehouseId = null,
    string? MachineCode = null) : IRequest<Result<WarehouseDto>>;

public class CreateWarehouseHandler(IWarehouseRepository repo)
    : IRequestHandler<CreateWarehouseCommand, Result<WarehouseDto>>
{
    public async Task<Result<WarehouseDto>> Handle(CreateWarehouseCommand request, CancellationToken ct)
    {
        if (await repo.CodeExistsAsync(request.Code, ct))
            return Result.Failure<WarehouseDto>("CODE_EXISTS", $"'{request.Code}' kodlu depo zaten mevcut.");

        try
        {
            Warehouse warehouse;
            if (request.Type == WarehouseType.Machine)
            {
                if (string.IsNullOrWhiteSpace(request.MachineCode))
                    return Result.Failure<WarehouseDto>("MACHINE_CODE_REQUIRED", "Makine deposu için makine kodu zorunludur.");

                warehouse = MachineWarehouse.Create(
                    Guid.NewGuid(), request.Code, request.Name, request.MachineCode!,
                    request.Address, request.ParentWarehouseId);
            }
            else
            {
                warehouse = Warehouse.Create(
                    Guid.NewGuid(), request.Code, request.Name, request.Type,
                    request.Address, request.ParentWarehouseId);
            }

            repo.Add(warehouse);
            await repo.SaveAsync(ct);
            return Result.Success(WarehouseDto.FromEntity(warehouse));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<WarehouseDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
