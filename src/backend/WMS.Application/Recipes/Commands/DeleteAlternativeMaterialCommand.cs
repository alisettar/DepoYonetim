using MediatR;
using WMS.Shared.Exceptions;
using WMS.Shared.Result;

namespace WMS.Application.Recipes.Commands;

public record DeleteAlternativeMaterialCommand(
    Guid RecipeId,
    Guid VersionId,
    Guid ItemId,
    Guid AlternativeId) : IRequest<Result>;

public class DeleteAlternativeMaterialHandler(IRecipeRepository repo)
    : IRequestHandler<DeleteAlternativeMaterialCommand, Result>
{
    public async Task<Result> Handle(DeleteAlternativeMaterialCommand request, CancellationToken ct)
    {
        var recipe = await repo.GetByIdWithVersionsAsync(request.RecipeId, ct);
        if (recipe is null)
            return Result.Failure("NOT_FOUND", "Reçete bulunamadı.");

        try
        {
            var version = recipe.GetVersion(request.VersionId);
            var item = version.GetItem(request.ItemId);
            item.RemoveAlternative(request.AlternativeId);
            await repo.SaveAsync(ct);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}
