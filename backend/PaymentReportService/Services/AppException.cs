using PaymentReportService.Dtos;

namespace PaymentReportService.Services;

public sealed class AppException(string message, int statusCode = StatusCodes.Status400BadRequest, IReadOnlyList<ApiError>? errors = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public IReadOnlyList<ApiError>? Errors { get; } = errors;
}
