using MediatR;
using WMS.Application.Inventory.Dtos;
using WMS.Domain.Inventory.Enums;
using WMS.Shared.Common.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Inventory.Commands;

public record UpdateLotQualityStatusCommand(Guid LotId, string NewStatus)
    : IRequest<Result<LotDto>>;

public class UpdateLotQualityStatusHandler(ILotRepository lotRepo)
    : IRequestHandler<UpdateLotQualityStatusCommand, Result<LotDto>>
{
    public async Task<Result<LotDto>> Handle(UpdateLotQualityStatusCommand request, CancellationToken ct)
    {
        var lot = await lotRepo.GetByIdAsync(request.LotId, ct);
        if (lot == null)
            return Result.Failure<LotDto>("LOT_NOT_FOUND", "Lot bulunamadi.");

        if (!Enum.TryParse<QualityStatus>(request.NewStatus, true, out var status))
            return Result.Failure<LotDto>("INVALID_STATUS", "Geçerli bir durum seçin: OK, Quarantine, Rejected");

        if (lot.QualityStatus == QualityStatus.Rejected && status == QualityStatus.OK)
            return Result.Failure<LotDto>("STATUS_FORBIDDEN", "Reddedilen lot durumu OK olarak degistirilemez.");

        lot.UpdateQualityStatus(status);
        await lotRepo.SaveAsync(ct);
        return Result.Success(LotDto.FromEntity(lot));
    }
}