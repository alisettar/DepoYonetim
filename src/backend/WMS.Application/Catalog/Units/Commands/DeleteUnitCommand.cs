using MediatR;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Units.Commands;

public record DeleteUnitCommand(Guid Id) : IRequest<Result>;

public class DeleteUnitHandler(IUnitRepository repo) : IRequestHandler<DeleteUnitCommand, Result>
{
    public async Task<Result> Handle(DeleteUnitCommand request, CancellationToken ct)
    {
        var unit = await repo.GetByIdAsync(request.Id, ct);
        if (unit is null)
            return Result.Failure("NOT_FOUND", "Birim bulunamadı.");

        repo.Remove(unit);
        await repo.SaveAsync(ct);
        return Result.Success();
    }
}
