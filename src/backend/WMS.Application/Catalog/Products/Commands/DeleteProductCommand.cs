using MediatR;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Products.Commands;

public record DeleteProductCommand(Guid Id) : IRequest<Result>;

public class DeleteProductHandler(IProductRepository repo) : IRequestHandler<DeleteProductCommand, Result>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(request.Id, ct);
        if (product is null)
            return Result.Failure("NOT_FOUND", "Ürün bulunamadı.");

        repo.Remove(product);
        await repo.SaveAsync(ct);
        return Result.Success();
    }
}
