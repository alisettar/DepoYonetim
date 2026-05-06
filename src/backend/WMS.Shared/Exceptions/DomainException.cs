namespace WMS.Shared.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string code, string message) : base(message) => ErrorCode = code;
    public string? ErrorCode { get; }
}
