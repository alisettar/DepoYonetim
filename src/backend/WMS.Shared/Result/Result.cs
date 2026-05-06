namespace WMS.Shared.Result;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Message { get; }
    public string? ErrorCode { get; }

    protected internal Result(bool isSuccess, string? message, string? errorCode)
    {
        IsSuccess = isSuccess;
        Message = message;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string message) => new(false, message, null);
    public static Result Failure(string errorCode, string message) => new(false, message, errorCode);

    public static Result<T> Success<T>(T value) => new(value, true, null, null);
    public static Result<T> Failure<T>(string message) => new(default, false, message, null);
    public static Result<T> Failure<T>(string errorCode, string message) => new(default, false, message, errorCode);

    public static Result<T> From<T>(Func<T> factory)
    {
        try
        {
            return Success(factory());
        }
        catch (Exception ex)
        {
            return Failure<T>(ex.Message);
        }
    }
}
