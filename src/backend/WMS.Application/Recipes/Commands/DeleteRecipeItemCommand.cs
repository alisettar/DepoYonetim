using MediatR;
using WMS.Shared.Exceptions;
using WMS.Shared.Result;

namespace WMS.Application.Recipes.Commands;

public record DeleteRecipeItemCommand(Guid RecipeId, Guid VersionId, Guid ItemId) : IRequest<Result>;

public class DeleteRecipeItemHandler(IRecipeRepository repo)
    : IRequestHandler<DeleteRecipeItemCommand, Result>
{
    public async Task<Result> Handle(DeleteRecipeItemCommand request, CancellationToken ct)
    {
        var recipe = await repo.GetByIdWithVersionsAsync(request.RecipeId, ct);
        if (recipe is null)
            return Result.Failure("NOT_FOUND", "Reçete bulunamadı.");

        try
        {
            var version = recipe.GetVersion(request.VersionId);
            version.RemoveItem(request.ItemId);
            await repo.SaveAsync(ct);
            return Result.Success();
        }
        catch (BusinessException ex)
        {
            return Result.Failure(ex.ErrorCode, ex.Message);
        }
    }
}
