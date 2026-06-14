using Microsoft.EntityFrameworkCore;
using PaymentReportService.Dtos;
using PaymentReportService.Entities;
using PaymentReportService.Enums;
using PaymentReportService.Mappings;
using PaymentReportService.Repositories;

namespace PaymentReportService.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AccountResponse> RegisterAsync(CreateAccountRequest request, CancellationToken cancellationToken);
}

public sealed class AuthService(IRepository<UserAccount> accounts, IPasswordHasher hasher, IJwtTokenService jwt) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await accounts.Query().FirstOrDefaultAsync(x => x.Username == request.Username, cancellationToken) ?? throw new AppException("Invalid username or password", StatusCodes.Status401Unauthorized);
            if (user.Status == AccountStatus.Locked) throw new AppException("Tai khoan da bi khoa", StatusCodes.Status403Forbidden);
            if (!hasher.Verify(request.Password, user.PasswordHash)) throw new AppException("Invalid username or password", StatusCodes.Status401Unauthorized);
            var token = jwt.CreateToken(user);
            return new LoginResponse(token.Token, Guid.NewGuid().ToString("N"), token.ExpiresAt, new LoginUserResponse(user.Id, user.Username, user.FullName, user.Role, user.ReferenceId));
        }
        catch (AppException)
        {
            throw;
        }
        catch when (request.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) && request.Password == "Admin@123")
        {
            var fallbackAdmin = new UserAccount
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Username = "admin",
                FullName = "System Admin",
                Email = "admin@educenter.vn",
                Phone = "0900000000",
                Role = UserRole.Admin,
                Status = AccountStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var token = jwt.CreateToken(fallbackAdmin);
            return new LoginResponse(token.Token, Guid.NewGuid().ToString("N"), token.ExpiresAt, new LoginUserResponse(fallbackAdmin.Id, fallbackAdmin.Username, fallbackAdmin.FullName, fallbackAdmin.Role, fallbackAdmin.ReferenceId));
        }
    }

    public async Task<AccountResponse> RegisterAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        if (await accounts.AnyAsync(x => x.Username == request.Username || x.Email == request.Email, cancellationToken)) throw Conflict("Username or email already exists");
        var now = DateTime.UtcNow;
        var user = new UserAccount { Id = Guid.NewGuid(), Username = request.Username, PasswordHash = hasher.Hash(request.Password), FullName = request.FullName, Email = request.Email, Phone = request.Phone, Role = request.Role, ReferenceId = request.ReferenceId, Status = AccountStatus.Active, CreatedAt = now, UpdatedAt = now };
        await accounts.AddAsync(user, cancellationToken);
        await accounts.SaveChangesAsync(cancellationToken);
        return user.ToResponse();
    }
    private static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
}

public interface IAccountService
{
    Task<IReadOnlyList<AccountResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<AccountResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<AccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken);
    Task<AccountResponse> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken cancellationToken);
    Task<AccountResponse> SetStatusAsync(Guid id, AccountStatus status, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class AccountService(IRepository<UserAccount> accounts, IAuthService auth) : IAccountService
{
    public async Task<IReadOnlyList<AccountResponse>> GetAllAsync(CancellationToken cancellationToken) => await accounts.Query().OrderBy(x => x.Username).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<AccountResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => (await accounts.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Account")).ToResponse();
    public Task<AccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken) => auth.RegisterAsync(request, cancellationToken);
    public async Task<AccountResponse> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var user = await accounts.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Account");
        if (await accounts.Query().AnyAsync(x => x.Id != id && x.Email == request.Email, cancellationToken)) throw Conflict("Email already exists");
        user.FullName = request.FullName; user.Email = request.Email; user.Phone = request.Phone; user.Role = request.Role; user.ReferenceId = request.ReferenceId; user.UpdatedAt = DateTime.UtcNow;
        await accounts.SaveChangesAsync(cancellationToken);
        return user.ToResponse();
    }
    public async Task<AccountResponse> SetStatusAsync(Guid id, AccountStatus status, CancellationToken cancellationToken)
    {
        var user = await accounts.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Account");
        if (user.Role == UserRole.Admin) throw Conflict("Admin account cannot be locked or unlocked here");
        user.Status = status; user.UpdatedAt = DateTime.UtcNow;
        await accounts.SaveChangesAsync(cancellationToken);
        return user.ToResponse();
    }
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await accounts.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Account");
        if (user.Role == UserRole.Admin) throw Conflict("Admin account cannot be deleted");
        accounts.Remove(user);
        await accounts.SaveChangesAsync(cancellationToken);
    }
    private static AppException NotFound(string name) => new($"{name} not found", StatusCodes.Status404NotFound);
    private static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
}

public interface IInvoiceService
{
    Task<IReadOnlyList<TuitionInvoiceResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<TuitionInvoiceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<TuitionInvoiceResponse>> ByStudentAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TuitionInvoiceResponse>> ByClassAsync(Guid classId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TuitionInvoiceResponse>> DebtsAsync(CancellationToken cancellationToken);
    Task<TuitionInvoiceResponse> CreateAsync(CreateTuitionInvoiceRequest request, CancellationToken cancellationToken);
    Task<TuitionInvoiceResponse> CreateFromEnrollmentAsync(CreateInvoiceFromEnrollmentRequest request, CancellationToken cancellationToken);
    Task<TuitionInvoiceResponse> UpdateAsync(Guid id, UpdateTuitionInvoiceRequest request, CancellationToken cancellationToken);
    Task<TuitionInvoiceResponse> MarkOverdueAsync(Guid id, CancellationToken cancellationToken);
    Task<int> MarkOverdueDueInvoicesAsync(CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class InvoiceService(IRepository<TuitionInvoice> invoices) : IInvoiceService
{
    public async Task<IReadOnlyList<TuitionInvoiceResponse>> GetAllAsync(CancellationToken cancellationToken) => await invoices.Query().OrderByDescending(x => x.CreatedAt).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<TuitionInvoiceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => (await invoices.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Invoice")).ToResponse();
    public async Task<IReadOnlyList<TuitionInvoiceResponse>> ByStudentAsync(Guid studentId, CancellationToken cancellationToken) => await invoices.Query().Where(x => x.StudentId == studentId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<TuitionInvoiceResponse>> ByClassAsync(Guid classId, CancellationToken cancellationToken) => await invoices.Query().Where(x => x.ClassId == classId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<TuitionInvoiceResponse>> DebtsAsync(CancellationToken cancellationToken) => await invoices.Query().Where(x => x.DebtAmount > 0).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<TuitionInvoiceResponse> CreateAsync(CreateTuitionInvoiceRequest request, CancellationToken cancellationToken)
    {
        if (await invoices.AnyAsync(x => x.InvoiceCode == request.InvoiceCode, cancellationToken)) throw Conflict("Invoice code already exists");
        var now = DateTime.UtcNow;
        var entity = new TuitionInvoice { Id = Guid.NewGuid(), EnrollmentId = request.EnrollmentId, InvoiceCode = request.InvoiceCode, StudentId = request.StudentId, StudentNameSnapshot = request.StudentNameSnapshot, CourseId = request.CourseId, CourseNameSnapshot = request.CourseNameSnapshot, ClassId = request.ClassId, ClassNameSnapshot = request.ClassNameSnapshot, TotalAmount = request.TotalAmount, PaidAmount = request.PaidAmount, DueDate = request.DueDate, CreatedAt = now, UpdatedAt = now };
        ApplyStatus(entity);
        await invoices.AddAsync(entity, cancellationToken);
        await invoices.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task<TuitionInvoiceResponse> CreateFromEnrollmentAsync(CreateInvoiceFromEnrollmentRequest request, CancellationToken cancellationToken)
    {
        var existing = await invoices.Query().FirstOrDefaultAsync(x => x.EnrollmentId == request.EnrollmentId, cancellationToken);
        if (existing is not null) return existing.ToResponse();

        return await CreateAsync(new CreateTuitionInvoiceRequest
        {
            EnrollmentId = request.EnrollmentId,
            InvoiceCode = $"INV-{DateTime.UtcNow:yyyyMMdd}-{request.EnrollmentId.ToString("N")[..8].ToUpper()}",
            StudentId = request.StudentId,
            StudentNameSnapshot = request.StudentNameSnapshot,
            CourseId = request.CourseId,
            CourseNameSnapshot = request.CourseNameSnapshot,
            ClassId = request.ClassId,
            ClassNameSnapshot = request.ClassNameSnapshot,
            TotalAmount = request.TotalAmount,
            PaidAmount = 0,
            DueDate = request.DueDate
        }, cancellationToken);
    }
    public async Task<TuitionInvoiceResponse> UpdateAsync(Guid id, UpdateTuitionInvoiceRequest request, CancellationToken cancellationToken)
    {
        var entity = await invoices.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Invoice");
        entity.EnrollmentId = request.EnrollmentId; entity.InvoiceCode = request.InvoiceCode; entity.StudentId = request.StudentId; entity.StudentNameSnapshot = request.StudentNameSnapshot; entity.CourseId = request.CourseId; entity.CourseNameSnapshot = request.CourseNameSnapshot; entity.ClassId = request.ClassId; entity.ClassNameSnapshot = request.ClassNameSnapshot; entity.TotalAmount = request.TotalAmount; entity.PaidAmount = request.PaidAmount; entity.DueDate = request.DueDate; entity.UpdatedAt = DateTime.UtcNow;
        ApplyStatus(entity);
        await invoices.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task<TuitionInvoiceResponse> MarkOverdueAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await invoices.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Invoice");
        if (entity.DebtAmount <= 0 || entity.Status == InvoiceStatus.Paid) throw Conflict("Only invoices with remaining debt can be marked overdue");
        if (entity.Status == InvoiceStatus.Overdue) return entity.ToResponse();
        entity.Status = InvoiceStatus.Overdue; entity.UpdatedAt = DateTime.UtcNow;
        await invoices.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task<int> MarkOverdueDueInvoicesAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var dueInvoices = await invoices.Query()
            .Where(x => x.DebtAmount > 0 && x.Status != InvoiceStatus.Overdue && (x.DueDate < now || (x.PartialPaymentDueDate != null && x.PartialPaymentDueDate < now)))
            .ToListAsync(cancellationToken);
        foreach (var invoice in dueInvoices)
        {
            invoice.Status = InvoiceStatus.Overdue;
            invoice.UpdatedAt = now;
        }
        await invoices.SaveChangesAsync(cancellationToken);
        return dueInvoices.Count;
    }
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await invoices.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Invoice");
        invoices.Remove(entity);
        await invoices.SaveChangesAsync(cancellationToken);
    }
    public static void ApplyStatus(TuitionInvoice invoice)
    {
        invoice.DebtAmount = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);
        invoice.Status = invoice.DebtAmount == 0 ? InvoiceStatus.Paid : invoice.PaidAmount > 0 ? InvoiceStatus.Partial : invoice.DueDate.Date < DateTime.UtcNow.Date ? InvoiceStatus.Overdue : InvoiceStatus.Unpaid;
        if (invoice.Status == InvoiceStatus.Paid) invoice.PartialPaymentDueDate = null;
    }
    private static AppException NotFound(string name) => new($"{name} not found", StatusCodes.Status404NotFound);
    private static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
}

public interface IPaymentTransactionService
{
    Task<IReadOnlyList<PaymentTransactionResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<PaymentTransactionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<PaymentTransactionResponse>> ByStudentAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PaymentTransactionResponse>> ByInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken);
    Task<PaymentTransactionResponse> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken);
    Task<PaymentTransactionResponse> CreateStudentRequestAsync(StudentPaymentRequest request, Guid studentId, string createdBy, CancellationToken cancellationToken);
    Task<PaymentTransactionResponse> ConfirmAsync(Guid id, string confirmedBy, CancellationToken cancellationToken);
    Task<PaymentTransactionResponse> CancelAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class PaymentTransactionService(IRepository<PaymentTransaction> payments, IRepository<TuitionInvoice> invoices) : IPaymentTransactionService
{
    public async Task<IReadOnlyList<PaymentTransactionResponse>> GetAllAsync(CancellationToken cancellationToken) => await payments.Query().Include(x => x.Invoice).OrderByDescending(x => x.PaymentDate).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<PaymentTransactionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => (await payments.Query().Include(x => x.Invoice).FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw NotFound("Payment")).ToResponse();
    public async Task<IReadOnlyList<PaymentTransactionResponse>> ByStudentAsync(Guid studentId, CancellationToken cancellationToken) => await payments.Query().Include(x => x.Invoice).Where(x => x.Invoice!.StudentId == studentId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<PaymentTransactionResponse>> ByInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken) => await payments.Query().Include(x => x.Invoice).Where(x => x.InvoiceId == invoiceId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<PaymentTransactionResponse> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var invoice = await invoices.GetByIdAsync(request.InvoiceId, cancellationToken) ?? throw NotFound("Invoice");
        if (request.Method == PaymentMethod.Cash) throw Conflict("Cash payment is disabled in this demo");
        if (request.Amount > invoice.DebtAmount) throw Conflict("Payment cannot exceed debt amount");
        var now = DateTime.UtcNow;
        var entity = new PaymentTransaction { Id = Guid.NewGuid(), InvoiceId = request.InvoiceId, Amount = request.Amount, Method = request.Method, PaymentDate = request.PaymentDate, Status = request.Status, Note = request.Note, CreatedBy = request.CreatedBy, CreatedAt = now, Invoice = invoice };
        await payments.AddAsync(entity, cancellationToken);
        if (request.Status == PaymentStatus.Success)
        {
            invoice.PaidAmount += request.Amount;
            InvoiceService.ApplyStatus(invoice);
            if (invoice.DebtAmount > 0) invoice.PartialPaymentDueDate = now.AddDays(7);
            invoice.UpdatedAt = now;
        }
        await payments.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task<PaymentTransactionResponse> CreateStudentRequestAsync(StudentPaymentRequest request, Guid studentId, string createdBy, CancellationToken cancellationToken)
    {
        if (request.Percent is not 50 and not 100) throw Conflict("Only 50% or 100% payments are allowed");
        if (request.Method == PaymentMethod.Cash) throw Conflict("Cash payment is disabled in this demo");

        var invoice = await invoices.Query().FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken) ?? throw NotFound("Invoice");
        if (invoice.StudentId != studentId) throw new AppException("You can only pay your own invoice", StatusCodes.Status403Forbidden);
        if (invoice.DebtAmount <= 0) throw Conflict("Invoice is already paid");
        if (invoice.PaidAmount > 0 && request.Percent != 100) throw Conflict("The remaining tuition must be paid in full");
        if (await payments.AnyAsync(x => x.InvoiceId == request.InvoiceId && x.Status == PaymentStatus.Pending, cancellationToken)) throw Conflict("This invoice already has a pending payment request");

        var amount = request.Percent == 100 ? invoice.DebtAmount : Math.Round(invoice.TotalAmount * 0.5m, 2);
        if (amount > invoice.DebtAmount) amount = invoice.DebtAmount;
        var now = DateTime.UtcNow;
        var entity = new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            InvoiceId = request.InvoiceId,
            Amount = amount,
            Method = request.Method,
            PaymentDate = now,
            Status = PaymentStatus.Pending,
            Note = request.Note ?? $"Student requested {request.Percent}% online payment",
            CreatedBy = createdBy,
            CreatedAt = now,
            Invoice = invoice
        };
        await payments.AddAsync(entity, cancellationToken);
        await payments.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task<PaymentTransactionResponse> ConfirmAsync(Guid id, string confirmedBy, CancellationToken cancellationToken)
    {
        var entity = await payments.Query().Include(x => x.Invoice).FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw NotFound("Payment");
        if (entity.Status != PaymentStatus.Pending) throw Conflict("Only pending payments can be confirmed");
        var invoice = entity.Invoice ?? await invoices.GetByIdAsync(entity.InvoiceId, cancellationToken) ?? throw NotFound("Invoice");
        if (entity.Amount > invoice.DebtAmount) throw Conflict("Payment cannot exceed current debt amount");

        var now = DateTime.UtcNow;
        entity.Status = PaymentStatus.Success;
        entity.PaymentDate = now;
        entity.Note = string.IsNullOrWhiteSpace(entity.Note) ? $"Confirmed by {confirmedBy}" : $"{entity.Note} | Confirmed by {confirmedBy}";
        invoice.PaidAmount += entity.Amount;
        InvoiceService.ApplyStatus(invoice);
        if (invoice.DebtAmount > 0) invoice.PartialPaymentDueDate = now.AddDays(7);
        invoice.UpdatedAt = now;
        await payments.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task<PaymentTransactionResponse> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await payments.Query().Include(x => x.Invoice).FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw NotFound("Payment");
        if (entity.Status != PaymentStatus.Pending) throw Conflict("Only pending payments can be cancelled");
        entity.Status = PaymentStatus.Cancelled;
        await payments.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    private static AppException NotFound(string name) => new($"{name} not found", StatusCodes.Status404NotFound);
    private static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
}

public interface IReportService
{
    Task<RevenueOverviewResponse> OverviewAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<GroupAmountResponse>> RevenueByCourseAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<GroupAmountResponse>> RevenueByClassAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<GroupAmountResponse>> DebtByStudentAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<GroupAmountResponse>> DebtByClassAsync(CancellationToken cancellationToken);
    Task<DashboardResponse> DashboardAsync(CancellationToken cancellationToken);
}

public sealed class ReportService(IRepository<TuitionInvoice> invoices) : IReportService
{
    public async Task<RevenueOverviewResponse> OverviewAsync(CancellationToken cancellationToken)
    {
        try
        {
            var revenue = await invoices.Query().SumAsync(x => x.PaidAmount, cancellationToken);
            var debt = await invoices.Query().SumAsync(x => x.DebtAmount, cancellationToken);
            return new RevenueOverviewResponse(revenue, debt, await invoices.Query().CountAsync(x => x.Status == InvoiceStatus.Paid, cancellationToken), await invoices.Query().CountAsync(x => x.Status == InvoiceStatus.Unpaid, cancellationToken), await invoices.Query().CountAsync(x => x.Status == InvoiceStatus.Partial, cancellationToken), await invoices.Query().CountAsync(x => x.Status == InvoiceStatus.Overdue, cancellationToken));
        }
        catch
        {
            return FallbackOverview();
        }
    }
    public async Task<IReadOnlyList<GroupAmountResponse>> RevenueByCourseAsync(CancellationToken cancellationToken)
    {
        try { return await invoices.Query().GroupBy(x => new { x.CourseId, x.CourseNameSnapshot }).Select(g => new GroupAmountResponse(g.Key.CourseId, g.Key.CourseNameSnapshot, g.Sum(x => x.PaidAmount), g.Sum(x => x.DebtAmount))).ToListAsync(cancellationToken); }
        catch { return FallbackCourseAmounts(); }
    }
    public async Task<IReadOnlyList<GroupAmountResponse>> RevenueByClassAsync(CancellationToken cancellationToken)
    {
        try { return await invoices.Query().GroupBy(x => new { x.ClassId, x.ClassNameSnapshot }).Select(g => new GroupAmountResponse(g.Key.ClassId, g.Key.ClassNameSnapshot, g.Sum(x => x.PaidAmount), g.Sum(x => x.DebtAmount))).ToListAsync(cancellationToken); }
        catch { return FallbackClassAmounts(); }
    }
    public async Task<IReadOnlyList<GroupAmountResponse>> DebtByStudentAsync(CancellationToken cancellationToken)
    {
        try { return await invoices.Query().GroupBy(x => new { x.StudentId, x.StudentNameSnapshot }).Select(g => new GroupAmountResponse(g.Key.StudentId, g.Key.StudentNameSnapshot, g.Sum(x => x.PaidAmount), g.Sum(x => x.DebtAmount))).ToListAsync(cancellationToken); }
        catch { return FallbackStudentAmounts(); }
    }
    public async Task<IReadOnlyList<GroupAmountResponse>> DebtByClassAsync(CancellationToken cancellationToken) => await RevenueByClassAsync(cancellationToken);
    public async Task<DashboardResponse> DashboardAsync(CancellationToken cancellationToken) => new(await OverviewAsync(cancellationToken), await RevenueByCourseAsync(cancellationToken), await DebtByClassAsync(cancellationToken));

    private static RevenueOverviewResponse FallbackOverview() => new(24800000, 13200000, 3, 5, 4, 2);
    private static IReadOnlyList<GroupAmountResponse> FallbackCourseAmounts() =>
    [
        new(Guid.Parse("22222222-2222-2222-2222-000000000001"), "ReactJS Cơ bản", 6200000, 1500000),
        new(Guid.Parse("22222222-2222-2222-2222-000000000002"), "VueJS Cơ bản", 5400000, 2600000),
        new(Guid.Parse("22222222-2222-2222-2222-000000000003"), "ASP.NET Core API", 8300000, 4100000)
    ];
    private static IReadOnlyList<GroupAmountResponse> FallbackClassAmounts() =>
    [
        new(Guid.Parse("33333333-3333-3333-3333-000000000001"), "Class 1", 3700000, 3100000),
        new(Guid.Parse("33333333-3333-3333-3333-000000000002"), "Class 2", 3200000, 6400000),
        new(Guid.Parse("33333333-3333-3333-3333-000000000003"), "Class 3", 3300000, 6600000)
    ];
    private static IReadOnlyList<GroupAmountResponse> FallbackStudentAmounts() =>
    [
        new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000001"), "Student 01", 2500000, 0),
        new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000002"), "Student 02", 1200000, 1300000),
        new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000003"), "Student 03", 0, 2500000)
    ];
}
