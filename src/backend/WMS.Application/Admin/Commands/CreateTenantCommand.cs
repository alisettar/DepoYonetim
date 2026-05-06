using MediatR;
using WMS.Domain.Catalog.Services;
using WMS.Shared.Common.Cryptography;
using WMS.Shared.Result;

namespace WMS.Application.Admin.Commands;

public record CreateTenantCommand(
    string Code,
    string Name,
    string? Plan = null,
    string? AdminEmail = null,
    string? AdminPassword = null) : IRequest<Result<TenantDto>>;

public record TenantDto(
    Guid Id,
    string Code,
    string Name,
    string Status,
    string? Plan,
    DateTime CreatedAt);

public class CreateTenantHandler(
    ICatalogRepository catalogRepository,
    JwtTokenService tokenService) : IRequestHandler<CreateTenantCommand, Result<TenantDto>>
{
    public Task<Result<TenantDto>> Handle(CreateTenantCommand request, CancellationToken ct)
    {
        // TODO: Implement tenant creation with catalog repository
        // This will use CatalogDbContext to insert into super_admins/tenants tables
        return Result<TenantDto>.Failure<TenantDto>("NOT_IMPLEMENTED", "Tenant creation requires catalog repository implementation").ToTask();
    }
}
