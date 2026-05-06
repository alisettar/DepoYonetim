using System.Threading.Tasks;

namespace WMS.Shared.Result;

public static class ResultExtensions
{
    public static System.Threading.Tasks.Task<Result> ToTask(this Result result) => System.Threading.Tasks.Task.FromResult(result);
    public static System.Threading.Tasks.Task<Result<T>> ToTask<T>(this Result<T> result) => System.Threading.Tasks.Task.FromResult(result);
}

public class Result<T> : Result
{
    public T? Value { get; }

    internal Result(T? value, bool isSuccess, string? message, string? errorCode)
        : base(isSuccess, message, errorCode)
    {
        Value = value;
    }

    public void Match(Action<T> onSuccess, Action onError)
    {
        if (IsSuccess) onSuccess(Value!);
        else onError();
    }

    public T ValueOr(T fallback) => IsSuccess ? Value! : fallback;

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        if (IsFailure)
            return new Result<TOut>(default, false, Message ?? "Unknown error", ErrorCode ?? "ERROR");
        var result = mapper(Value!);
        return new Result<TOut>(result, true, null, null);
    }
}
