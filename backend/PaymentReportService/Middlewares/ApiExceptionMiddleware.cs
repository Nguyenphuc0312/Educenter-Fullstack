using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaymentReportService.Dtos;
using PaymentReportService.Services;

namespace PaymentReportService.Middlewares;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (AppException exception)
        {
            context.Response.StatusCode = exception.StatusCode;
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(exception.Message, exception.Errors));
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException)
        {
            logger.LogError(exception, "Database error");
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("Database conflict"));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled error");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("Internal server error"));
        }
    }
}
