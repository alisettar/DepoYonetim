using MediatR;
using WMS.Shared.Result;

namespace WMS.Application.Admin.Commands;

public record UpdateTenantStatusCommand(
    Guid TenantId,
    string NewStatus) : IRequest<Result>;

public class UpdateTenantStatusHandler : IRequestHandler<UpdateTenantStatusCommand, Result>
{
    public Task<Result> Handle(UpdateTenantStatusCommand request, CancellationToken ct)
    {
        return Result.Success().ToTask();
    }
}
