using MediatR;
using WMS.Shared.Common;
using WMS.Shared.Result;
using UserInfo = WMS.Application.Identity.Commands.UserInfo;

namespace WMS.Application.Identity.Queries;

public record GetMeQuery : IRequest<Result<UserInfo>>;

public class GetMeHandler(
    ITenantContext tenantContext) : IRequestHandler<GetMeQuery, Result<UserInfo>>
{
    public Task<Result<UserInfo>> Handle(GetMeQuery request, CancellationToken ct)
    {
        var userInfo = new UserInfo(
            tenantContext.UserId,
            tenantContext.Email,
            tenantContext.TenantId,
            tenantContext.TenantCode,
            "tenant_user",
            [],
            "tenant_user");

        return Result<UserInfo>.Success(userInfo).ToTask();
    }
}
