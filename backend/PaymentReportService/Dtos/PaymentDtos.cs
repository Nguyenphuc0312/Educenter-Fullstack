using PaymentReportService.Enums;
using System.ComponentModel.DataAnnotations;

namespace PaymentReportService.Dtos;

public sealed record AccountResponse(Guid Id, string Username, string FullName, string Email, string Phone, UserRole Role, Guid? ReferenceId, AccountStatus Status, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record TuitionInvoiceResponse(Guid Id, Guid? EnrollmentId, string InvoiceCode, Guid StudentId, string StudentNameSnapshot, Guid CourseId, string CourseNameSnapshot, Guid ClassId, string ClassNameSnapshot, decimal TotalAmount, decimal PaidAmount, decimal DebtAmount, DateTime DueDate, DateTime? PartialPaymentDueDate, DateTime? LastReminderSentAt, int ReminderCount, InvoiceStatus Status, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record PaymentTransactionResponse(Guid Id, Guid InvoiceId, string? InvoiceCode, Guid? StudentId, string? StudentNameSnapshot, decimal Amount, PaymentMethod Method, DateTime PaymentDate, PaymentStatus Status, string? Note, string CreatedBy, DateTime CreatedAt);
public sealed record LoginUserResponse(Guid Id, string Username, string FullName, string Email, string Phone, UserRole Role, Guid? ReferenceId);
public sealed record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, LoginUserResponse User);
public sealed record RevenueOverviewResponse(decimal TotalRevenue, decimal TotalDebt, int PaidInvoices, int UnpaidInvoices, int PartialInvoices, int OverdueInvoices);
public sealed record GroupAmountResponse(Guid Id, string Name, decimal TotalRevenue, decimal TotalDebt);
public sealed record DashboardResponse(RevenueOverviewResponse Overview, IReadOnlyList<GroupAmountResponse> RevenueByCourse, IReadOnlyList<GroupAmountResponse> DebtByClass);
public sealed record OverdueScanResponse(int ScannedInvoices, int UpdatedInvoices, IReadOnlyList<TuitionInvoiceResponse> Items);
public sealed record LearningHoldResponse(Guid InvoiceId, Guid? EnrollmentId, string InvoiceCode, Guid StudentId, string StudentNameSnapshot, Guid CourseId, string CourseNameSnapshot, Guid ClassId, string ClassNameSnapshot, decimal DebtAmount, DateTime DueDate, DateTime? PartialPaymentDueDate, InvoiceStatus Status, string Reason);
public sealed record NotificationEmailSettingsResponse(string FromEmail, string FromName, string SmtpHost, int SmtpPort, bool EnableSsl, bool HasAppPassword, string MaskedAppPassword, DateTime UpdatedAt);

public sealed class LoginRequest { [Required] public string Username { get; set; } = string.Empty; [Required] public string Password { get; set; } = string.Empty; }
public class CreateAccountRequest
{
    [Required] public string Username { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    [Required] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? ReferenceId { get; set; }
}
public sealed class UpdateAccountRequest
{
    [Required] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Password { get; set; }
    public UserRole Role { get; set; }
    public Guid? ReferenceId { get; set; }
}

public sealed class CompleteStudentProfileRequest
{
    [Required] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Gender { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

public class CreateTuitionInvoiceRequest
{
    public Guid? EnrollmentId { get; set; }
    public string InvoiceCode { get; set; } = string.Empty;
    [Required] public Guid StudentId { get; set; }
    [Required] public string StudentNameSnapshot { get; set; } = string.Empty;
    [Required] public Guid CourseId { get; set; }
    [Required] public string CourseNameSnapshot { get; set; } = string.Empty;
    [Required] public Guid ClassId { get; set; }
    [Required] public string ClassNameSnapshot { get; set; } = string.Empty;
    [Range(0, double.MaxValue)] public decimal TotalAmount { get; set; }
    [Range(0, double.MaxValue)] public decimal PaidAmount { get; set; }
    public DateTime DueDate { get; set; }
}
public sealed class UpdateTuitionInvoiceRequest : CreateTuitionInvoiceRequest;

public sealed class CreateInvoiceFromEnrollmentRequest
{
    [Required] public Guid EnrollmentId { get; set; }
    [Required] public Guid StudentId { get; set; }
    [Required] public string StudentNameSnapshot { get; set; } = string.Empty;
    [Required] public Guid CourseId { get; set; }
    [Required] public string CourseNameSnapshot { get; set; } = string.Empty;
    [Required] public Guid ClassId { get; set; }
    [Required] public string ClassNameSnapshot { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue)] public decimal TotalAmount { get; set; }
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(14);
}

public sealed class CreatePaymentRequest
{
    [Required] public Guid InvoiceId { get; set; }
    [Range(0.01, double.MaxValue)] public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public PaymentStatus Status { get; set; } = PaymentStatus.Success;
    public string? Note { get; set; }
    public string CreatedBy { get; set; } = "system";
}

public sealed class StudentPaymentRequest
{
    [Required] public Guid InvoiceId { get; set; }
    [Range(50, 100)] public int Percent { get; set; } = 100;
    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;
    public string? Note { get; set; }
}

public sealed class UpdateNotificationEmailSettingsRequest
{
    [Required, EmailAddress] public string FromEmail { get; set; } = string.Empty;
    [Required] public string FromName { get; set; } = "EduCenter";
    [Required] public string SmtpHost { get; set; } = "smtp.gmail.com";
    [Range(1, 65535)] public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string? AppPassword { get; set; }
}

public sealed class SendTestEmailRequest
{
    [Required, EmailAddress] public string ToEmail { get; set; } = string.Empty;
}

public sealed class SendNotificationRequest
{
    [Required, EmailAddress] public string ToEmail { get; set; } = string.Empty;
    [Required] public string Subject { get; set; } = string.Empty;
    [Required] public string Body { get; set; } = string.Empty;
}
