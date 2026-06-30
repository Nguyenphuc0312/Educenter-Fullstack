using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using PaymentReportService.Data;
using PaymentReportService.Entities;
using PaymentReportService.Enums;
using PaymentReportService.Services;

namespace PaymentReportService.Seed;

public static class PaymentDbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<PaymentDbContext>();
        await db.Database.EnsureCreatedAsync();
        await EnsureSchemaAsync(db);
        var hasher = services.GetRequiredService<IPasswordHasher>();
        var protector = services.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector(SettingsService.SecretPurpose);
        var now = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);

        if (!await db.UserAccounts.AnyAsync())
        {
            var accounts = new[]
            {
                Account("admin", "Admin@123", "System Admin", "admin@educenter.vn", UserRole.Admin, null),
                Account("teacher01", "Teacher@123", "Teacher One", "teacher01@educenter.vn", UserRole.Teacher, Guid.Parse("11111111-1111-1111-1111-000000000001")),
                Account("teacher02", "Teacher@123", "Teacher Two", "teacher02@educenter.vn", UserRole.Teacher, Guid.Parse("11111111-1111-1111-1111-000000000002")),
                Account("student01", "Student@123", "Student 01", "student01@educenter.vn", UserRole.Student, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000001")),
                Account("student02", "Student@123", "Student 02", "student02@educenter.vn", UserRole.Student, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000002"))
            };
            await db.UserAccounts.AddRangeAsync(accounts);
        }

        var defaultSettings = new Dictionary<string, (string Value, string Description)>
        {
            [SettingsService.EmailFromKey] = ("mingiot999@gmail.com", "Gmail dùng để gửi thông báo"),
            [SettingsService.EmailFromNameKey] = ("EduCenter", "Tên người gửi hiển thị trong email"),
            [SettingsService.EmailSmtpHostKey] = ("smtp.gmail.com", "Máy chủ SMTP"),
            [SettingsService.EmailSmtpPortKey] = ("587", "Cổng SMTP"),
            [SettingsService.EmailEnableSslKey] = ("true", "Bật SSL/TLS khi gửi SMTP")
        };
        var initialAppPassword = Environment.GetEnvironmentVariable("EDUCENTER_NOTIFICATION_GMAIL_APP_PASSWORD");
        if (!string.IsNullOrWhiteSpace(initialAppPassword))
        {
            defaultSettings[SettingsService.EmailAppPasswordKey] = (
                protector.Protect(SettingsService.NormalizeSecret(initialAppPassword)),
                "Gmail app password đã được mã hóa");
        }
        var existingSettingKeys = await db.SystemSettings
            .Where(x => defaultSettings.Keys.Contains(x.SettingKey))
            .Select(x => x.SettingKey)
            .ToListAsync();
        foreach (var setting in defaultSettings.Where(x => !existingSettingKeys.Contains(x.Key)))
        {
            db.SystemSettings.Add(new SystemSetting
            {
                Id = Guid.NewGuid(),
                SettingKey = setting.Key,
                SettingValue = setting.Value.Value,
                Description = setting.Value.Description,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await db.SaveChangesAsync();

        UserAccount Account(string username, string password, string name, string email, UserRole role, Guid? referenceId) => new()
        {
            Id = Guid.NewGuid(),
            Username = username,
            PasswordHash = hasher.Hash(password),
            FullName = name,
            Email = email,
            Phone = "0900000000",
            Role = role,
            ReferenceId = referenceId,
            Status = AccountStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static async Task EnsureSchemaAsync(PaymentDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID('SystemSettings', 'U') IS NULL
            BEGIN
                CREATE TABLE SystemSettings
                (
                    Id uniqueidentifier NOT NULL CONSTRAINT PK_SystemSettings PRIMARY KEY,
                    SettingKey nvarchar(120) NOT NULL,
                    SettingValue nvarchar(4000) NOT NULL,
                    Description nvarchar(500) NULL,
                    CreatedAt datetime2 NOT NULL,
                    UpdatedAt datetime2 NOT NULL
                );
                CREATE UNIQUE INDEX IX_SystemSettings_SettingKey ON SystemSettings(SettingKey);
            END;
            """);
        await db.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('TuitionInvoices', 'EnrollmentId') IS NULL
                ALTER TABLE TuitionInvoices ADD EnrollmentId uniqueidentifier NULL;
            """);
        await db.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('TuitionInvoices', 'PartialPaymentDueDate') IS NULL
                ALTER TABLE TuitionInvoices ADD PartialPaymentDueDate datetime2 NULL;
            """);
        await db.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('TuitionInvoices', 'LastReminderSentAt') IS NULL
                ALTER TABLE TuitionInvoices ADD LastReminderSentAt datetime2 NULL;
            """);
        await db.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH('TuitionInvoices', 'ReminderCount') IS NULL
                ALTER TABLE TuitionInvoices ADD ReminderCount int NOT NULL CONSTRAINT DF_TuitionInvoices_ReminderCount DEFAULT 0;
            """);
        await db.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TuitionInvoices_EnrollmentId' AND object_id = OBJECT_ID('TuitionInvoices'))
                CREATE UNIQUE INDEX IX_TuitionInvoices_EnrollmentId ON TuitionInvoices(EnrollmentId) WHERE EnrollmentId IS NOT NULL;
            """);
    }
}
