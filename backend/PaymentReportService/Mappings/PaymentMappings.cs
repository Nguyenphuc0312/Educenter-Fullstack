using PaymentReportService.Dtos;
using PaymentReportService.Entities;

namespace PaymentReportService.Mappings;

public static class PaymentMappings
{
    public static AccountResponse ToResponse(this UserAccount x) => new(x.Id, x.Username, x.FullName, x.Email, x.Phone, x.Role, x.ReferenceId, x.Status, x.CreatedAt, x.UpdatedAt);
    public static TuitionInvoiceResponse ToResponse(this TuitionInvoice x) => new(x.Id, x.EnrollmentId, x.InvoiceCode, x.StudentId, x.StudentNameSnapshot, x.CourseId, x.CourseNameSnapshot, x.ClassId, x.ClassNameSnapshot, x.TotalAmount, x.PaidAmount, x.DebtAmount, x.DueDate, x.Status, x.CreatedAt, x.UpdatedAt);
    public static PaymentTransactionResponse ToResponse(this PaymentTransaction x) => new(
        x.Id,
        x.InvoiceId,
        x.Invoice?.InvoiceCode,
        x.Invoice?.StudentId,
        x.Invoice?.StudentNameSnapshot,
        x.Amount,
        x.Method,
        x.PaymentDate,
        x.Status,
        x.Note,
        x.CreatedBy,
        x.CreatedAt);
}
