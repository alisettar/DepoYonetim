using MediatR;
using WMS.Shared.Result;

namespace WMS.Application.Warehousing.Commands;

public record DeleteWarehouseCommand(Guid Id) : IRequest<Result>;

public class DeleteWarehouseHandler(IWarehouseRepository repo)
    : IRequestHandler<DeleteWarehouseCommand, Result>
{
    public async Task<Result> Handle(DeleteWarehouseCommand request, CancellationToken ct)
    {
        var warehouse = await repo.GetByIdAsync(request.Id, ct);
        if (warehouse is null)
            return Result.Failure("NOT_FOUND", "Depo bulunamadı.");

        repo.Remove(warehouse);
        await repo.SaveAsync(ct);
        return Result.Success();
    }
}
