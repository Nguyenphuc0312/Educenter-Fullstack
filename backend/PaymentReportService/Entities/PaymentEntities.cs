using PaymentReportService.Enums;
using System.ComponentModel.DataAnnotations;

namespace PaymentReportService.Entities;

public sealed class UserAccount
{
    public Guid Id { get; set; }
    [MaxLength(80)] public string Username { get; set; } = string.Empty;
    [MaxLength(500)] public string PasswordHash { get; set; } = string.Empty;
    [MaxLength(200)] public string FullName { get; set; } = string.Empty;
    [MaxLength(200)] public string Email { get; set; } = string.Empty;
    [MaxLength(30)] public string Phone { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? ReferenceId { get; set; }
    public AccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class TuitionInvoice
{
    public Guid Id { get; set; }
    public Guid? EnrollmentId { get; set; }
    [MaxLength(50)] public string InvoiceCode { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    [MaxLength(200)] public string StudentNameSnapshot { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    [MaxLength(200)] public string CourseNameSnapshot { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    [MaxLength(200)] public string ClassNameSnapshot { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal DebtAmount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PartialPaymentDueDate { get; set; }
    public DateTime? LastReminderSentAt { get; set; }
    public int ReminderCount { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<PaymentTransaction> Payments { get; set; } = [];
}

public sealed class PaymentTransaction
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentStatus Status { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
    [MaxLength(200)] public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public TuitionInvoice? Invoice { get; set; }
}

public sealed class RevenueReportSnapshot
{
    public Guid Id { get; set; }
    public DateTime ReportDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalDebt { get; set; }
    public int TotalPaidInvoices { get; set; }
    public int TotalUnpaidInvoices { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class SystemSetting
{
    public Guid Id { get; set; }
    [MaxLength(120)] public string SettingKey { get; set; } = string.Empty;
    [MaxLength(4000)] public string SettingValue { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
