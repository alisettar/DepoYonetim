namespace WMS.Shared.Common.Dtos;

public record PaginatedResponse<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages) where T : class;
