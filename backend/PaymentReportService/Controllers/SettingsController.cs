using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReportService.Dtos;
using PaymentReportService.Services;

namespace PaymentReportService.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize(Roles = "Admin")]
public sealed class SettingsController(
    ISettingsService settingsService,
    IEmailNotificationService emailNotificationService) : ControllerBase
{
    [HttpGet("notification-email")]
    public async Task<IActionResult> GetNotificationEmail(CancellationToken cancellationToken) =>
        Ok(ApiResponse<NotificationEmailSettingsResponse>.Ok(
            await settingsService.GetNotificationEmailAsync(cancellationToken)));

    [HttpPut("notification-email")]
    public async Task<IActionResult> UpdateNotificationEmail(
        UpdateNotificationEmailSettingsRequest request,
        CancellationToken cancellationToken) =>
        Ok(ApiResponse<NotificationEmailSettingsResponse>.Ok(
            await settingsService.UpdateNotificationEmailAsync(request, cancellationToken),
            "Đã lưu cấu hình Gmail"));

    [HttpPost("notification-email/test")]
    public async Task<IActionResult> SendTestEmail(
        SendTestEmailRequest request,
        CancellationToken cancellationToken)
    {
        await emailNotificationService.SendAsync(
            request.ToEmail,
            "Kiểm tra thông báo từ EduCenter",
            """
            <div style="font-family:Arial,sans-serif;line-height:1.6;color:#172033">
              <h2 style="margin:0 0 12px;color:#155eef">EduCenter</h2>
              <p>Email kiểm tra đã được gửi thành công từ cấu hình thông báo của hệ thống.</p>
              <p style="color:#667085;font-size:13px">Đây là thư tự động, vui lòng không trả lời email này.</p>
            </div>
            """,
            cancellationToken);

        return Ok(ApiResponse<object>.Ok(null, "Đã gửi email kiểm tra"));
    }
}
