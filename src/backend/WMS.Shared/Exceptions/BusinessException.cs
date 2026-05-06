namespace WMS.Shared.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
    public BusinessException(string errorCode, string message) : base(message) => ErrorCode = errorCode;
    public string? ErrorCode { get; }
}
