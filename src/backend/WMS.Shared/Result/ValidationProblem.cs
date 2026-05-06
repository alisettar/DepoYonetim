namespace WMS.Shared.Result;

public record ValidationError(string Field, string Code, string Message);

public record ValidationProblem(IEnumerable<ValidationError> Errors, string? ErrorCode = null);
