using MediatR;
using WMS.Shared.Exceptions;
using WMS.Shared.Result;

namespace WMS.Application.Recipes.Commands;

public record ArchiveRecipeCommand(Guid Id) : IRequest<Result>;

public class ArchiveRecipeHandler(IRecipeRepository repo) : IRequestHandler<ArchiveRecipeCommand, Result>
{
    public async Task<Result> Handle(ArchiveRecipeCommand request, CancellationToken ct)
    {
        var recipe = await repo.GetByIdWithVersionsAsync(request.Id, ct);
        if (recipe is null)
            return Result.Failure("NOT_FOUND", "Reçete bulunamadı.");

        try
        {
            recipe.Archive();
            await repo.SaveAsync(ct);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}
