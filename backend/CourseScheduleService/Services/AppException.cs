using CourseScheduleService.Dtos;

namespace CourseScheduleService.Services;

public sealed class AppException : Exception
{
    public int StatusCode { get; }
    public IReadOnlyList<ApiError>? Errors { get; }

    public AppException(string message, int statusCode = StatusCodes.Status400BadRequest, IReadOnlyList<ApiError>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}

