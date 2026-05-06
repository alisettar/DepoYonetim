using MediatR;
using WMS.Application.Catalog.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Categories.Commands;

public record UpdateCategoryCommand(Guid Id, string Name) : IRequest<Result<CategoryDto>>;

public class UpdateCategoryHandler(ICategoryRepository repo) : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await repo.GetByIdAsync(request.Id, ct);
        if (category is null)
            return Result.Failure<CategoryDto>("NOT_FOUND", "Kategori bulunamadı.");

        try
        {
            category.Update(request.Name);
            await repo.SaveAsync(ct);
            return Result.Success(CategoryDto.FromEntity(category));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<CategoryDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
