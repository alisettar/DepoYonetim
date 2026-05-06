using MediatR;
using WMS.Application.Inventory.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Inventory.Queries;

public record GetLotTraceQuery(Guid LotId) : IRequest<Result<List<TraceChainEntry>>>;

public class GetLotTraceHandler(
    IStockMovementRepository movementRepo)
    : IRequestHandler<GetLotTraceQuery, Result<List<TraceChainEntry>>>
{
    public async Task<Result<List<TraceChainEntry>>> Handle(GetLotTraceQuery request, CancellationToken ct)
    {
        var movements = await movementRepo.GetTraceChainAsync(request.LotId, ct);
        if (!movements.Any())
            return Result.Success<List<TraceChainEntry>>([]);

        // Sort chronologically (oldest first) for trace
        var sorted = movements.OrderByDescending(m => m.OccurredAt).ToList();

        // Calculate cumulative running balance and consumed quantity
        var result = new List<TraceChainEntry>();
        decimal runningBalance = 0;
        decimal cumulativeConsumed = 0;

        foreach (var m in sorted.OrderBy(m => m.OccurredAt))
        {
            if (m.Quantity > 0)
            {
                runningBalance += m.Quantity;
            }
            else
            {
                runningBalance += m.Quantity; // m.Quantity is negative
                cumulativeConsumed += Math.Abs(m.Quantity);
            }

            result.Add(new TraceChainEntry(
                StockMovementDto.FromEntity(m),
                m.Quantity < 0,
                m.Quantity > 0,
                cumulativeConsumed,
                Math.Max(0, runningBalance)));
        }

        return Result.Success(result);
    }
}