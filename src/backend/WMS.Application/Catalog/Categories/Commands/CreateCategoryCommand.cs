using MediatR;
using WMS.Application.Catalog.Dtos;
using WMS.Domain.Catalog;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Categories.Commands;

public record CreateCategoryCommand(string Code, string Name) : IRequest<Result<CategoryDto>>;

public class CreateCategoryHandler(ICategoryRepository repo) : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        if (await repo.CodeExistsAsync(request.Code, ct))
            return Result.Failure<CategoryDto>("CODE_EXISTS", $"'{request.Code}' kodlu kategori zaten mevcut.");

        try
        {
            var category = Category.Create(Guid.NewGuid(), request.Code, request.Name);
            repo.Add(category);
            await repo.SaveAsync(ct);
            return Result.Success(CategoryDto.FromEntity(category));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<CategoryDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
