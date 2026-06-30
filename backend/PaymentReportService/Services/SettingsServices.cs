using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using PaymentReportService.Data;
using PaymentReportService.Dtos;
using PaymentReportService.Entities;
using System.Net;
using System.Net.Mail;

namespace PaymentReportService.Services;

public interface ISettingsService
{
    Task<NotificationEmailSettingsResponse> GetNotificationEmailAsync(CancellationToken cancellationToken);
    Task<NotificationEmailSettingsResponse> UpdateNotificationEmailAsync(UpdateNotificationEmailSettingsRequest request, CancellationToken cancellationToken);
}

public interface IEmailNotificationService
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken);
}

public static class NotificationTemplate
{
    public static string Wrap(string content) =>
        $$"""
        <div style="font-family:Arial,sans-serif;line-height:1.6;color:#172033">
          <h2 style="margin:0 0 12px;color:#155eef">EduCenter</h2>
          <div>{{content}}</div>
          <p style="margin-top:20px;color:#667085;font-size:13px">Đây là thư tự động từ hệ thống EduCenter.</p>
        </div>
        """;
}

public sealed class SettingsService(PaymentDbContext db, IDataProtectionProvider dataProtectionProvider) : ISettingsService
{
    public const string SecretPurpose = "EduCenter.NotificationEmail.AppPassword.v1";
    public const string EmailFromKey = "Notification.Email.FromEmail";
    public const string EmailFromNameKey = "Notification.Email.FromName";
    public const string EmailAppPasswordKey = "Notification.Email.AppPassword";
    public const string EmailSmtpHostKey = "Notification.Email.SmtpHost";
    public const string EmailSmtpPortKey = "Notification.Email.SmtpPort";
    public const string EmailEnableSslKey = "Notification.Email.EnableSsl";

    private readonly IDataProtector protector = dataProtectionProvider.CreateProtector(SecretPurpose);

    public async Task<NotificationEmailSettingsResponse> GetNotificationEmailAsync(CancellationToken cancellationToken)
    {
        List<SystemSetting> rows;
        try
        {
            rows = await db.SystemSettings.AsNoTracking().ToListAsync(cancellationToken);
        }
        catch
        {
            return ToEmailResponse(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), DateTime.UtcNow);
        }

        var settings = rows
            .GroupBy(x => x.SettingKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(setting => setting.UpdatedAt).First().SettingValue,
                StringComparer.OrdinalIgnoreCase);
        var updatedAt = rows.Count == 0 ? DateTime.UtcNow : rows.Max(x => x.UpdatedAt);
        return ToEmailResponse(settings, updatedAt);
    }

    public async Task<NotificationEmailSettingsResponse> UpdateNotificationEmailAsync(
        UpdateNotificationEmailSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        await SetAsync(EmailFromKey, request.FromEmail.Trim(), "Gmail dùng để gửi thông báo", now, cancellationToken);
        await SetAsync(EmailFromNameKey, request.FromName.Trim(), "Tên người gửi hiển thị trong email", now, cancellationToken);
        await SetAsync(EmailSmtpHostKey, request.SmtpHost.Trim(), "Máy chủ SMTP", now, cancellationToken);
        await SetAsync(EmailSmtpPortKey, request.SmtpPort.ToString(), "Cổng SMTP", now, cancellationToken);
        await SetAsync(EmailEnableSslKey, request.EnableSsl ? "true" : "false", "Bật SSL/TLS khi gửi SMTP", now, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.AppPassword))
        {
            var encryptedPassword = protector.Protect(NormalizeSecret(request.AppPassword));
            await SetAsync(EmailAppPasswordKey, encryptedPassword, "Gmail app password đã được mã hóa", now, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetNotificationEmailAsync(cancellationToken);
    }

    public static NotificationEmailSettingsResponse ToEmailResponse(
        IReadOnlyDictionary<string, string> settings,
        DateTime updatedAt)
    {
        var hasPassword = !string.IsNullOrWhiteSpace(Get(settings, EmailAppPasswordKey, string.Empty));
        return new NotificationEmailSettingsResponse(
            Get(settings, EmailFromKey, "mingiot999@gmail.com"),
            Get(settings, EmailFromNameKey, "EduCenter"),
            Get(settings, EmailSmtpHostKey, "smtp.gmail.com"),
            int.TryParse(Get(settings, EmailSmtpPortKey, "587"), out var port) ? port : 587,
            bool.TryParse(Get(settings, EmailEnableSslKey, "true"), out var ssl) ? ssl : true,
            hasPassword,
            hasPassword ? "************" : string.Empty,
            updatedAt);
    }

    public static string NormalizeSecret(string value) =>
        string.Concat(value.Where(character => !char.IsWhiteSpace(character)));

    private async Task SetAsync(
        string key,
        string value,
        string description,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var row = await db.SystemSettings.FirstOrDefaultAsync(x => x.SettingKey == key, cancellationToken);
        if (row is null)
        {
            db.SystemSettings.Add(new SystemSetting
            {
                Id = Guid.NewGuid(),
                SettingKey = key,
                SettingValue = value,
                Description = description,
                CreatedAt = now,
                UpdatedAt = now
            });
            return;
        }

        row.SettingValue = value;
        row.Description = description;
        row.UpdatedAt = now;
    }

    private static string Get(IReadOnlyDictionary<string, string> settings, string key, string fallback) =>
        settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
}

public sealed class EmailNotificationService(
    PaymentDbContext db,
    IDataProtectionProvider dataProtectionProvider) : IEmailNotificationService
{
    private readonly IDataProtector protector = dataProtectionProvider.CreateProtector(SettingsService.SecretPurpose);

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        List<SystemSetting> rows;
        try
        {
            rows = await db.SystemSettings.AsNoTracking().ToListAsync(cancellationToken);
        }
        catch
        {
            throw new AppException("Chưa cấu hình Gmail gửi thông báo", StatusCodes.Status409Conflict);
        }

        var settings = rows
            .GroupBy(x => x.SettingKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(setting => setting.UpdatedAt).First().SettingValue,
                StringComparer.OrdinalIgnoreCase);
        var fromEmail = Value(settings, SettingsService.EmailFromKey, "mingiot999@gmail.com");
        var fromName = Value(settings, SettingsService.EmailFromNameKey, "EduCenter");
        var protectedPassword = Value(settings, SettingsService.EmailAppPasswordKey, string.Empty);
        var host = Value(settings, SettingsService.EmailSmtpHostKey, "smtp.gmail.com");
        var port = int.TryParse(Value(settings, SettingsService.EmailSmtpPortKey, "587"), out var parsedPort) ? parsedPort : 587;
        var enableSsl = bool.TryParse(Value(settings, SettingsService.EmailEnableSslKey, "true"), out var ssl) ? ssl : true;

        if (string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(protectedPassword))
        {
            throw new AppException("Chưa cấu hình Gmail gửi thông báo", StatusCodes.Status409Conflict);
        }

        string password;
        try
        {
            password = protector.Unprotect(protectedPassword);
        }
        catch
        {
            throw new AppException("Khóa Gmail đã lưu không hợp lệ. Vui lòng nhập lại app password.", StatusCodes.Status409Conflict);
        }

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        using var smtp = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            Credentials = new NetworkCredential(fromEmail, password)
        };

        await smtp.SendMailAsync(message).WaitAsync(cancellationToken);
    }

    private static string Value(IReadOnlyDictionary<string, string> settings, string key, string fallback) =>
        settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
}
