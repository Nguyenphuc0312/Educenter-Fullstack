using Microsoft.EntityFrameworkCore;
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
        var hasher = services.GetRequiredService<IPasswordHasher>();
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

        if (!await db.TuitionInvoices.AnyAsync())
        {
            var invoices = Enumerable.Range(1, 15).Select(i =>
            {
                var total = 2500000 + i * 100000;
                var paid = i % 4 == 0 ? total : i % 4 == 1 ? total / 2 : 0;
                return new TuitionInvoice
                {
                    Id = Guid.Parse($"99999999-9999-9999-9999-{i:000000000000}"),
                    InvoiceCode = $"INV{i:000}",
                    StudentId = Guid.Parse($"aaaaaaaa-aaaa-aaaa-aaaa-{i:000000000000}"),
                    StudentNameSnapshot = $"Student {i:00}",
                    CourseId = Guid.Parse($"22222222-2222-2222-2222-{(i % 8) + 1:000000000000}"),
                    CourseNameSnapshot = $"Course {(i % 8) + 1}",
                    ClassId = Guid.Parse($"33333333-3333-3333-3333-{(i % 6) + 1:000000000000}"),
                    ClassNameSnapshot = $"Class {(i % 6) + 1}",
                    TotalAmount = total,
                    PaidAmount = paid,
                    DebtAmount = total - paid,
                    DueDate = now.AddDays(i % 5 == 0 ? -10 : 30),
                    Status = paid == total ? InvoiceStatus.Paid : paid > 0 ? InvoiceStatus.Partial : i % 5 == 0 ? InvoiceStatus.Overdue : InvoiceStatus.Unpaid,
                    CreatedAt = now,
                    UpdatedAt = now
                };
            }).ToList();
            await db.TuitionInvoices.AddRangeAsync(invoices);
            await db.PaymentTransactions.AddRangeAsync(Enumerable.Range(1, 20).Select(i =>
            {
                var invoice = invoices[(i - 1) % invoices.Count];
                return new PaymentTransaction
                {
                    Id = Guid.Parse($"88888888-8888-8888-8888-{i:000000000000}"),
                    InvoiceId = invoice.Id,
                    Amount = Math.Min(500000 + i * 50000, invoice.TotalAmount),
                    Method = (PaymentMethod)((i % 4) + 1),
                    PaymentDate = now.AddDays(i),
                    Status = i % 7 == 0 ? PaymentStatus.Pending : PaymentStatus.Success,
                    Note = "Seed payment",
                    CreatedBy = "admin",
                    CreatedAt = now.AddDays(i)
                };
            }));
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
}
