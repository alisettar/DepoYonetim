using MediatR;
using WMS.Application.Recipes.Dtos;
using WMS.Domain.Recipes;
using WMS.Shared.Exceptions;
using WMS.Shared.Result;

namespace WMS.Application.Recipes.Commands;

public record AddAlternativeMaterialCommand(
    Guid RecipeId,
    Guid VersionId,
    Guid ItemId,
    Guid ProductId,
    int Priority,
    decimal Quantity,
    Guid UnitId) : IRequest<Result<AlternativeMaterialDto>>;

public class AddAlternativeMaterialHandler(IRecipeRepository repo)
    : IRequestHandler<AddAlternativeMaterialCommand, Result<AlternativeMaterialDto>>
{
    public async Task<Result<AlternativeMaterialDto>> Handle(AddAlternativeMaterialCommand request, CancellationToken ct)
    {
        var recipe = await repo.GetByIdWithVersionsAsync(request.RecipeId, ct);
        if (recipe is null)
            return Result.Failure<AlternativeMaterialDto>("NOT_FOUND", "Reçete bulunamadı.");

        try
        {
            var item = recipe.GetVersion(request.VersionId)
                .GetItem(request.ItemId);

            var alt = AlternativeMaterial.Create(
                item.Id,
                request.ProductId,
                request.Priority,
                request.Quantity,
                request.UnitId);
            await repo.AddAlternativeAsync(alt, ct);
            await repo.SaveAsync(ct);
            return Result.Success(AlternativeMaterialDto.FromEntity(alt));
        }
        catch (BusinessException ex)
        {
            return Result.Failure<AlternativeMaterialDto>(ex.ErrorCode, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<AlternativeMaterialDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
