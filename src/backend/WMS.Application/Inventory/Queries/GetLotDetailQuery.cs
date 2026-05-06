using MediatR;
using WMS.Application.Inventory.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Inventory.Queries;

public record GetLotDetailQuery(Guid LotId) : IRequest<Result<LotDetailResponse>>;

public class GetLotDetailHandler(
    ILotRepository lotRepo,
    IStockMovementRepository movementRepo,
    IStockBalanceRepository balanceRepo)
    : IRequestHandler<GetLotDetailQuery, Result<LotDetailResponse>>
{
    public async Task<Result<LotDetailResponse>> Handle(GetLotDetailQuery request, CancellationToken ct)
    {
        var lot = await lotRepo.GetByIdAsync(request.LotId, ct);
        if (lot == null)
            return Result.Failure<LotDetailResponse>("LOT_NOT_FOUND", "Lot bulunamadi.");

        var movements = await movementRepo.GetTraceChainAsync(request.LotId, ct);

        var balance = await balanceRepo.FindAsync(lot.ProductId, lot.Id, null, null, ct);

        return Result.Success(new LotDetailResponse(
            LotDto.FromEntity(lot),
            movements.Select(StockMovementDto.FromEntity).ToList(),
            balance != null ? StockBalanceDto.FromEntity(balance) : null));
    }
}