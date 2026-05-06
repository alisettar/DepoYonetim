using MediatR;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Categories.Commands;

public record DeleteCategoryCommand(Guid Id) : IRequest<Result>;

public class DeleteCategoryHandler(ICategoryRepository repo) : IRequestHandler<DeleteCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await repo.GetByIdAsync(request.Id, ct);
        if (category is null)
            return Result.Failure("NOT_FOUND", "Kategori bulunamadı.");

        repo.Remove(category);
        await repo.SaveAsync(ct);
        return Result.Success();
    }
}
