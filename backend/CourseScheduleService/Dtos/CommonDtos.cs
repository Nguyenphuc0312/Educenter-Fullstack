namespace CourseScheduleService.Dtos;

public sealed record ApiError(string Field, string Message);

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = "OK";
    public T? Data { get; init; }
    public IReadOnlyList<ApiError>? Errors { get; init; }

    public static ApiResponse<T> Ok(T? data, string message = "OK") => new()
    {
        Success = true,
        Message = message,
        Data = data,
        Errors = null
    };

    public static ApiResponse<T> Fail(string message, IReadOnlyList<ApiError>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Data = default,
        Errors = errors
    };
}

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}

public class PaginationQuery
{
    public int PageIndex { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public int SafePageIndex => PageIndex <= 0 ? 1 : PageIndex;
    public int SafePageSize => PageSize is <= 0 or > 100 ? 10 : PageSize;
}

public sealed class BulkDeleteRequest
{
    public List<Guid> Ids { get; set; } = [];
}

public sealed class BulkUpdateRequest<TRequest>
{
    public Guid Id { get; set; }
    public TRequest Data { get; set; } = default!;
}

public sealed class BulkOperationResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int Requested { get; init; }
    public int Succeeded { get; init; }
}
