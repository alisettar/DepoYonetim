using MediatR;
using WMS.Application.Warehousing.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Warehousing.Queries;

public record GetWarehouseQuery(Guid Id) : IRequest<Result<WarehouseDto>>;

public class GetWarehouseHandler(IWarehouseRepository repo)
    : IRequestHandler<GetWarehouseQuery, Result<WarehouseDto>>
{
    public async Task<Result<WarehouseDto>> Handle(GetWarehouseQuery request, CancellationToken ct)
    {
        var warehouse = await repo.GetByIdAsync(request.Id, ct);
        if (warehouse is null)
            return Result.Failure<WarehouseDto>("NOT_FOUND", "Depo bulunamadı.");

        return Result.Success(WarehouseDto.FromEntity(warehouse));
    }
}
