using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReportService.Dtos;
using PaymentReportService.Services;

namespace PaymentReportService.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = "Admin")]
public sealed class NotificationsController(IEmailNotificationService emailNotificationService) : ControllerBase
{
    [HttpPost("email")]
    public async Task<IActionResult> SendEmail(SendNotificationRequest request, CancellationToken cancellationToken)
    {
        await emailNotificationService.SendAsync(
            request.ToEmail,
            request.Subject.Trim(),
            NotificationTemplate.Wrap(request.Body),
            cancellationToken);

        return Ok(ApiResponse<object>.Ok(null, "Đã gửi thông báo"));
    }
}
