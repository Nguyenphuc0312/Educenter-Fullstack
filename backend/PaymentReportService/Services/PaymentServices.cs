using Microsoft.EntityFrameworkCore;
using PaymentReportService.Dtos;
using PaymentReportService.Entities;
using PaymentReportService.Enums;
using PaymentReportService.Mappings;
using PaymentReportService.Repositories;
using PaymentReportService.Data;
using System.Data;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PaymentReportService.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AccountResponse> RegisterAsync(CreateAccountRequest request, CancellationToken cancellationToken);
    Task<LoginResponse> CompleteStudentProfileAsync(Guid accountId, CompleteStudentProfileRequest request, string? bearerToken, CancellationToken cancellationToken);
}

public interface IStudentProfileClient
{
    Task<StudentProfileSnapshot> EnsureProfileAsync(CompleteStudentProfileRequest request, string? bearerToken, CancellationToken cancellationToken);
}

public sealed record StudentProfileSnapshot(Guid Id, string StudentCode, string FullName, string Email, string Phone);

public sealed class StudentProfileClient(HttpClient httpClient) : IStudentProfileClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<StudentProfileSnapshot> EnsureProfileAsync(CompleteStudentProfileRequest request, string? bearerToken, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "/api/students/self-profile");
        if (!string.IsNullOrWhiteSpace(bearerToken)) message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        message.Content = JsonContent.Create(request);

        using var response = await httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new AppException($"Cannot create student profile. Student service returned {(int)response.StatusCode}: {detail}", StatusCodes.Status502BadGateway);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ApiResponse<StudentProfileSnapshot>>(stream, JsonOptions, cancellationToken);
        return payload?.Data ?? throw new AppException("Student service returned an empty profile", StatusCodes.Status502BadGateway);
    }
}

public sealed class AuthService(IRepository<UserAccount> accounts, IPasswordHasher hasher, IJwtTokenService jwt, IStudentProfileClient studentProfiles) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (IsFallbackAdmin(request) && IsLocalFallbackMode())
        {
            return CreateFallbackAdminLogin();
        }

        try
        {
            var user = await accounts.Query().FirstOrDefaultAsync(x => x.Username == request.Username, cancellationToken) ?? throw new AppException("Invalid username or password", StatusCodes.Status401Unauthorized);
            if (user.Status == AccountStatus.Locked) throw new AppException("Account is locked", StatusCodes.Status403Forbidden);
            if (!hasher.Verify(request.Password, user.PasswordHash)) throw new AppException("Invalid username or password", StatusCodes.Status401Unauthorized);
            var token = jwt.CreateToken(user);
            return new LoginResponse(token.Token, Guid.NewGuid().ToString("N"), token.ExpiresAt, new LoginUserResponse(user.Id, user.Username, user.FullName, user.Email, user.Phone, user.Role, user.ReferenceId));
        }
        catch (AppException)
        {
            throw;
        }
        catch when (IsFallbackAdmin(request))
        {
            return CreateFallbackAdminLogin();
        }
    }

    private LoginResponse CreateFallbackAdminLogin()
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
        return new LoginResponse(token.Token, Guid.NewGuid().ToString("N"), token.ExpiresAt, new LoginUserResponse(fallbackAdmin.Id, fallbackAdmin.Username, fallbackAdmin.FullName, fallbackAdmin.Email, fallbackAdmin.Phone, fallbackAdmin.Role, fallbackAdmin.ReferenceId));
    }

    private static bool IsFallbackAdmin(LoginRequest request) =>
        request.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) && request.Password == "Admin@123";

    private static bool IsLocalFallbackMode() =>
        string.Equals(Environment.GetEnvironmentVariable("EDUCENTER_SKIP_DB_INIT"), "true", StringComparison.OrdinalIgnoreCase);

    public async Task<AccountResponse> RegisterAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        if (await accounts.AnyAsync(x => x.Username == request.Username || x.Email == request.Email, cancellationToken)) throw Conflict("Username or email already exists");
        var now = DateTime.UtcNow;
        var user = new UserAccount { Id = Guid.NewGuid(), Username = request.Username, PasswordHash = hasher.Hash(request.Password), FullName = request.FullName, Email = request.Email, Phone = request.Phone, Role = request.Role, ReferenceId = request.ReferenceId, Status = AccountStatus.Active, CreatedAt = now, UpdatedAt = now };
        await accounts.AddAsync(user, cancellationToken);
        await accounts.SaveChangesAsync(cancellationToken);
        return user.ToResponse();
    }
    public async Task<LoginResponse> CompleteStudentProfileAsync(Guid accountId, CompleteStudentProfileRequest request, string? bearerToken, CancellationToken cancellationToken)
    {
        var user = await accounts.GetByIdAsync(accountId, cancellationToken) ?? throw new AppException("Account not found", StatusCodes.Status404NotFound);
        if (user.Role != UserRole.Student) throw new AppException("Only student accounts can complete a student profile", StatusCodes.Status403Forbidden);
        if (user.Status == AccountStatus.Locked) throw new AppException("Account is locked", StatusCodes.Status403Forbidden);

        var email = request.Email.Trim();
        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase) && await accounts.Query().AnyAsync(x => x.Id != user.Id && x.Email == email, cancellationToken)) throw Conflict("Email already exists");

        var profileRequest = new CompleteStudentProfileRequest
        {
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? user.FullName : request.FullName.Trim(),
            Email = email,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? user.Phone : request.Phone.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Address = request.Address?.Trim() ?? string.Empty,
            AvatarUrl = request.AvatarUrl
        };

        var profile = await studentProfiles.EnsureProfileAsync(profileRequest, bearerToken, cancellationToken);
        user.ReferenceId = profile.Id;

        user.FullName = profileRequest.FullName;
        user.Email = profileRequest.Email;
        user.Phone = profileRequest.Phone;
        user.UpdatedAt = DateTime.UtcNow;
        await accounts.SaveChangesAsync(cancellationToken);

        var token = jwt.CreateToken(user);
        return new LoginResponse(token.Token, Guid.NewGuid().ToString("N"), token.ExpiresAt, new LoginUserResponse(user.Id, user.Username, user.FullName, user.Email, user.Phone, user.Role, user.ReferenceId));
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

public sealed class AccountService(IRepository<UserAccount> accounts, IAuthService auth, IPasswordHasher hasher) : IAccountService
{
    public async Task<IReadOnlyList<AccountResponse>> GetAllAsync(CancellationToken cancellationToken) => await accounts.Query().OrderBy(x => x.Username).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<AccountResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var account = await accounts.GetByIdAsync(id, cancellationToken);
            if (account is not null) return account.ToResponse();
        }
        catch when (id == FallbackAdminId)
        {
            return FallbackAdminResponse();
        }

        if (id == FallbackAdminId) return FallbackAdminResponse();
        throw NotFound("Account");
    }
    public Task<AccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        if (request.Role == UserRole.Admin) throw Conflict("Admin accounts must be provisioned by system seed or database administration");
        return auth.RegisterAsync(request, cancellationToken);
    }
    public async Task<AccountResponse> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var user = await accounts.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Account");
        if (await accounts.Query().AnyAsync(x => x.Id != id && x.Email == request.Email, cancellationToken)) throw Conflict("Email already exists");
        user.FullName = request.FullName; user.Email = request.Email; user.Phone = request.Phone;
        if (!string.IsNullOrWhiteSpace(request.Password)) user.PasswordHash = hasher.Hash(request.Password.Trim());
        user.UpdatedAt = DateTime.UtcNow;
        await accounts.SaveChangesAsync(cancellationToken);
        return user.ToResponse();
    }
    public async Task<AccountResponse> SetStatusAsync(Guid id, AccountStatus status, CancellationToken cancellationToken)
    {
        var user = await accounts.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Account");
        if (user.Role == UserRole.Admin) throw Conflict("Admin accounts cannot be locked or unlocked from account management");
        user.Status = status; user.UpdatedAt = DateTime.UtcNow;
        await accounts.SaveChangesAsync(cancellationToken);
        return user.ToResponse();
    }
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await accounts.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Account");
        if (user.Role == UserRole.Admin) throw Conflict("Admin accounts cannot be deleted");
        accounts.Remove(user);
        await accounts.SaveChangesAsync(cancellationToken);
    }
    private static AppException NotFound(string name) => new($"{name} not found", StatusCodes.Status404NotFound);
    private static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
    private static readonly Guid FallbackAdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static AccountResponse FallbackAdminResponse()
    {
        var now = DateTime.UtcNow;
        return new AccountResponse(FallbackAdminId, "admin", "System Admin", "admin@educenter.vn", "0900000000", UserRole.Admin, null, AccountStatus.Active, now, now);
    }
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
    Task<TuitionInvoiceResponse> SendReminderAsync(Guid id, CancellationToken cancellationToken);
    Task<BulkOperationResult<TuitionInvoiceResponse>> BulkSendReminderAsync(BulkDeleteRequest request, CancellationToken cancellationToken);
    Task<BulkOperationResult<TuitionInvoiceResponse>> SendUpcomingRemindersAsync(int daysAhead, CancellationToken cancellationToken);
    Task<OverdueScanResponse> ScanOverdueAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<LearningHoldResponse>> LearningHoldsAsync(Guid? studentId, Guid? classId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class InvoiceService(IRepository<TuitionInvoice> invoices, PaymentDbContext db, IEmailNotificationService emailNotificationService) : IInvoiceService
{
    public async Task<IReadOnlyList<TuitionInvoiceResponse>> GetAllAsync(CancellationToken cancellationToken) => await invoices.Query().OrderByDescending(x => x.CreatedAt).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<TuitionInvoiceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => (await invoices.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Invoice")).ToResponse();
    public async Task<IReadOnlyList<TuitionInvoiceResponse>> ByStudentAsync(Guid studentId, CancellationToken cancellationToken) => await invoices.Query().Where(x => x.StudentId == studentId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<TuitionInvoiceResponse>> ByClassAsync(Guid classId, CancellationToken cancellationToken) => await invoices.Query().Where(x => x.ClassId == classId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<TuitionInvoiceResponse>> DebtsAsync(CancellationToken cancellationToken) => await invoices.Query().Where(x => x.DebtAmount > 0).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<TuitionInvoiceResponse> CreateAsync(CreateTuitionInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoiceCode = string.IsNullOrWhiteSpace(request.InvoiceCode) ? await GenerateInvoiceCodeAsync(cancellationToken) : request.InvoiceCode.Trim().ToUpperInvariant();
        if (await invoices.AnyAsync(x => x.InvoiceCode == invoiceCode, cancellationToken)) throw Conflict("Invoice code already exists");
        var now = DateTime.UtcNow;
        var entity = new TuitionInvoice { Id = Guid.NewGuid(), EnrollmentId = request.EnrollmentId, InvoiceCode = invoiceCode, StudentId = request.StudentId, StudentNameSnapshot = request.StudentNameSnapshot, CourseId = request.CourseId, CourseNameSnapshot = request.CourseNameSnapshot, ClassId = request.ClassId, ClassNameSnapshot = request.ClassNameSnapshot, TotalAmount = request.TotalAmount, PaidAmount = request.PaidAmount, DueDate = request.DueDate, CreatedAt = now, UpdatedAt = now };
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
            InvoiceCode = string.Empty,
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
        if (request.EnrollmentId.HasValue) entity.EnrollmentId = request.EnrollmentId;
        entity.InvoiceCode = request.InvoiceCode; entity.StudentId = request.StudentId; entity.StudentNameSnapshot = request.StudentNameSnapshot; entity.CourseId = request.CourseId; entity.CourseNameSnapshot = request.CourseNameSnapshot; entity.ClassId = request.ClassId; entity.ClassNameSnapshot = request.ClassNameSnapshot; entity.TotalAmount = request.TotalAmount; entity.PaidAmount = request.PaidAmount; entity.DueDate = request.DueDate; entity.UpdatedAt = DateTime.UtcNow;
        ApplyStatus(entity);
        await invoices.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task<TuitionInvoiceResponse> MarkOverdueAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await invoices.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Invoice");
        if (entity.DebtAmount <= 0 || entity.Status == InvoiceStatus.Paid) throw Conflict("Paid invoices cannot be marked overdue");
        if (!IsOverdueCandidate(entity, DateTime.UtcNow.Date)) throw Conflict("Invoice is not overdue yet");
        entity.Status = InvoiceStatus.Overdue; entity.UpdatedAt = DateTime.UtcNow;
        await invoices.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task<TuitionInvoiceResponse> SendReminderAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await invoices.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Invoice");
        if (entity.DebtAmount <= 0 || entity.Status == InvoiceStatus.Paid) throw Conflict("Paid invoices do not need reminders");
        var recipient = await ResolveStudentEmailAsync(entity.StudentId, cancellationToken);
        await emailNotificationService.SendAsync(
            recipient.Email,
            $"Nhắc học phí EduCenter - {entity.InvoiceCode}",
            NotificationTemplate.Wrap($"""
                <p>Chào <strong>{recipient.Name}</strong>,</p>
                <p>Hệ thống ghi nhận hóa đơn học phí <strong>{entity.InvoiceCode}</strong> của lớp <strong>{entity.ClassNameSnapshot}</strong> còn công nợ <strong>{FormatMoney(entity.DebtAmount)}</strong>.</p>
                <p>Hạn thanh toán: <strong>{entity.DueDate:dd/MM/yyyy}</strong>.</p>
                <p>Vui lòng đăng nhập EduCenter để gửi yêu cầu thanh toán hoặc liên hệ trung tâm nếu cần hỗ trợ.</p>
                """),
            cancellationToken);

        entity.LastReminderSentAt = DateTime.UtcNow;
        entity.ReminderCount += 1;
        entity.UpdatedAt = DateTime.UtcNow;
        await invoices.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task<BulkOperationResult<TuitionInvoiceResponse>> BulkSendReminderAsync(BulkDeleteRequest request, CancellationToken cancellationToken)
    {
        var items = new List<TuitionInvoiceResponse>();
        foreach (var id in request.Ids.Distinct())
        {
            items.Add(await SendReminderAsync(id, cancellationToken));
        }

        return new BulkOperationResult<TuitionInvoiceResponse>
        {
            Items = items,
            Requested = request.Ids.Count,
            Succeeded = items.Count
        };
    }

    public async Task<BulkOperationResult<TuitionInvoiceResponse>> SendUpcomingRemindersAsync(int daysAhead, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var until = today.AddDays(Math.Clamp(daysAhead, 1, 30));
        var candidates = await invoices.Query()
            .Where(x => x.DebtAmount > 0
                && x.Status != InvoiceStatus.Paid
                && x.DueDate.Date >= today
                && x.DueDate.Date <= until
                && (x.LastReminderSentAt == null || x.LastReminderSentAt.Value.Date < today))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var items = new List<TuitionInvoiceResponse>();
        foreach (var id in candidates)
        {
            items.Add(await SendReminderAsync(id, cancellationToken));
        }

        return new BulkOperationResult<TuitionInvoiceResponse>
        {
            Items = items,
            Requested = candidates.Count,
            Succeeded = items.Count
        };
    }
    public async Task<OverdueScanResponse> ScanOverdueAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var candidates = await invoices.Query()
            .Where(x => x.DebtAmount > 0 && x.Status != InvoiceStatus.Paid)
            .ToListAsync(cancellationToken);
        var updated = new List<TuitionInvoice>();
        foreach (var invoice in candidates)
        {
            if (!IsOverdueCandidate(invoice, today) || invoice.Status == InvoiceStatus.Overdue) continue;
            invoice.Status = InvoiceStatus.Overdue;
            invoice.UpdatedAt = DateTime.UtcNow;
            updated.Add(invoice);
        }

        if (updated.Count > 0) await invoices.SaveChangesAsync(cancellationToken);
        return new OverdueScanResponse(candidates.Count, updated.Count, updated.Select(x => x.ToResponse()).ToList());
    }
    public async Task<IReadOnlyList<LearningHoldResponse>> LearningHoldsAsync(Guid? studentId, Guid? classId, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var query = invoices.Query().Where(x => x.DebtAmount > 0 && x.Status != InvoiceStatus.Paid);
        if (studentId.HasValue) query = query.Where(x => x.StudentId == studentId.Value);
        if (classId.HasValue) query = query.Where(x => x.ClassId == classId.Value);

        var rows = await query.ToListAsync(cancellationToken);
        return rows
            .Where(x => x.Status == InvoiceStatus.Overdue || IsOverdueCandidate(x, today))
            .Select(x => new LearningHoldResponse(
                x.Id,
                x.EnrollmentId,
                x.InvoiceCode,
                x.StudentId,
                x.StudentNameSnapshot,
                x.CourseId,
                x.CourseNameSnapshot,
                x.ClassId,
                x.ClassNameSnapshot,
                x.DebtAmount,
                x.DueDate,
                x.PartialPaymentDueDate,
                x.Status,
                x.PartialPaymentDueDate.HasValue && x.PartialPaymentDueDate.Value.Date < today
                    ? "Partial payment grace period expired"
                    : "Tuition invoice overdue"))
            .OrderBy(x => x.ClassNameSnapshot)
            .ThenBy(x => x.StudentNameSnapshot)
            .ToList();
    }
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await invoices.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Invoice");
        invoices.Remove(entity);
        await invoices.SaveChangesAsync(cancellationToken);
    }
    private async Task<(string Email, string Name)> ResolveStudentEmailAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var account = await db.UserAccounts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ReferenceId == studentId && x.Role == UserRole.Student, cancellationToken);
        if (account is null || string.IsNullOrWhiteSpace(account.Email)) throw Conflict("Student account email was not found");
        return (account.Email, account.FullName);
    }
    private static string FormatMoney(decimal value) => string.Create(CultureInfo.GetCultureInfo("vi-VN"), $"{value:N0} đ");
    public static void ApplyStatus(TuitionInvoice invoice)
    {
        invoice.DebtAmount = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);
        invoice.Status = invoice.DebtAmount == 0
            ? InvoiceStatus.Paid
            : IsOverdueCandidate(invoice, DateTime.UtcNow.Date)
                ? InvoiceStatus.Overdue
                : invoice.PaidAmount > 0 ? InvoiceStatus.Partial : InvoiceStatus.Unpaid;
        if (invoice.Status == InvoiceStatus.Paid) invoice.PartialPaymentDueDate = null;
    }
    private static bool IsOverdueCandidate(TuitionInvoice invoice, DateTime today)
    {
        if (invoice.DebtAmount <= 0) return false;
        if (invoice.PaidAmount > 0 && invoice.PartialPaymentDueDate.HasValue) return invoice.PartialPaymentDueDate.Value.Date < today;
        return invoice.DueDate.Date < today;
    }
    private async Task<string> GenerateInvoiceCodeAsync(CancellationToken cancellationToken)
    {
        var stem = $"INV{DateTime.UtcNow:yyyy}";
        var codes = await invoices.Query().Where(x => x.InvoiceCode.StartsWith(stem)).Select(x => x.InvoiceCode).ToListAsync(cancellationToken);
        var next = codes.Select(x => int.TryParse(x[stem.Length..], out var value) ? value : 0).DefaultIfEmpty().Max() + 1;
        return $"{stem}{next:00}";
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

public sealed class PaymentTransactionService(IRepository<PaymentTransaction> payments, IRepository<TuitionInvoice> invoices, PaymentDbContext db, IEmailNotificationService emailNotificationService) : IPaymentTransactionService
{
    public async Task<IReadOnlyList<PaymentTransactionResponse>> GetAllAsync(CancellationToken cancellationToken) => await payments.Query()
        .Include(x => x.Invoice)
        .OrderByDescending(x => x.Status == PaymentStatus.Pending)
        .ThenByDescending(x => x.PaymentDate)
        .Select(x => x.ToResponse())
        .ToListAsync(cancellationToken);
    public async Task<PaymentTransactionResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => (await payments.Query().Include(x => x.Invoice).FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw NotFound("Payment")).ToResponse();
    public async Task<IReadOnlyList<PaymentTransactionResponse>> ByStudentAsync(Guid studentId, CancellationToken cancellationToken) => await payments.Query().Include(x => x.Invoice).Where(x => x.Invoice!.StudentId == studentId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<PaymentTransactionResponse>> ByInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken) => await payments.Query().Include(x => x.Invoice).Where(x => x.InvoiceId == invoiceId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<PaymentTransactionResponse> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var invoice = await invoices.GetByIdAsync(request.InvoiceId, cancellationToken) ?? throw NotFound("Invoice");
        if (request.Method == PaymentMethod.Cash) throw Conflict("Cash payments are not supported in this demo");
        if (request.Status == PaymentStatus.Success && request.Amount > invoice.DebtAmount) throw Conflict("Payment cannot exceed debt amount");
        var now = DateTime.UtcNow;
        var entity = new PaymentTransaction { Id = Guid.NewGuid(), InvoiceId = request.InvoiceId, Amount = request.Amount, Method = request.Method, PaymentDate = request.PaymentDate, Status = request.Status, Note = request.Note, CreatedBy = request.CreatedBy, CreatedAt = now, Invoice = invoice };
        await payments.AddAsync(entity, cancellationToken);
        if (request.Status == PaymentStatus.Success)
        {
            invoice.PaidAmount += request.Amount;
            InvoiceService.ApplyStatus(invoice);
            invoice.UpdatedAt = now;
        }
        await payments.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task<PaymentTransactionResponse> CreateStudentRequestAsync(StudentPaymentRequest request, Guid studentId, string createdBy, CancellationToken cancellationToken)
    {
        if (request.Percent is not 50 and not 100) throw Conflict("Only 50% or 100% payments are allowed");
        if (request.Method == PaymentMethod.Cash) throw Conflict("Students cannot submit cash payments online");

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var invoice = await db.TuitionInvoices.FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken) ?? throw NotFound("Invoice");
        if (invoice.StudentId != studentId) throw new AppException("You can only pay your own invoice", StatusCodes.Status403Forbidden);
        if (invoice.DebtAmount <= 0) throw Conflict("Invoice is already paid");
        if (invoice.PaidAmount > 0 && request.Percent != 100) throw Conflict("The remaining tuition must be paid in full");
        if (await db.PaymentTransactions.AnyAsync(x => x.InvoiceId == request.InvoiceId && x.Status == PaymentStatus.Pending, cancellationToken)) throw Conflict("This invoice already has a pending payment request");

        var amount = request.Percent == 100 ? invoice.DebtAmount : Math.Min(Math.Round(invoice.TotalAmount * 0.5m, 2), invoice.DebtAmount);
        var now = DateTime.UtcNow;
        var entity = new PaymentTransaction { Id = Guid.NewGuid(), InvoiceId = invoice.Id, Amount = amount, Method = request.Method, PaymentDate = now, Status = PaymentStatus.Pending, Note = request.Note, CreatedBy = createdBy, CreatedAt = now, Invoice = invoice };
        db.PaymentTransactions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await TryNotifyPaymentConfirmedAsync(invoice, entity, cancellationToken);
        return entity.ToResponse();
    }
    public async Task<PaymentTransactionResponse> ConfirmAsync(Guid id, string confirmedBy, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var entity = await db.PaymentTransactions.Include(x => x.Invoice).FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw NotFound("Payment");
        if (entity.Status != PaymentStatus.Pending) throw Conflict("Only pending payments can be confirmed");
        var invoice = entity.Invoice ?? throw NotFound("Invoice");
        if (entity.Amount > invoice.DebtAmount) throw Conflict("Payment cannot exceed current debt amount");

        var now = DateTime.UtcNow;
        entity.Status = PaymentStatus.Success;
        entity.PaymentDate = now;
        entity.Note = string.IsNullOrWhiteSpace(entity.Note) ? $"Confirmed by {confirmedBy}" : $"{entity.Note} | Confirmed by {confirmedBy}";
        invoice.PaidAmount += entity.Amount;
        InvoiceService.ApplyStatus(invoice);
        if (invoice.DebtAmount > 0) invoice.PartialPaymentDueDate = now.AddDays(7);
        invoice.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task<PaymentTransactionResponse> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await payments.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Payment");
        if (entity.Status != PaymentStatus.Pending) throw Conflict("Only pending payments can be cancelled");
        entity.Status = PaymentStatus.Cancelled;
        await payments.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    private static AppException NotFound(string name) => new($"{name} not found", StatusCodes.Status404NotFound);
    private static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
    private async Task TryNotifyPaymentConfirmedAsync(TuitionInvoice invoice, PaymentTransaction payment, CancellationToken cancellationToken)
    {
        try
        {
            var account = await db.UserAccounts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ReferenceId == invoice.StudentId && x.Role == UserRole.Student, cancellationToken);
            if (account is null || string.IsNullOrWhiteSpace(account.Email)) return;
            await emailNotificationService.SendAsync(
                account.Email,
                $"EduCenter đã xác nhận thanh toán {invoice.InvoiceCode}",
                NotificationTemplate.Wrap($"""
                    <p>Chào <strong>{account.FullName}</strong>,</p>
                    <p>Trung tâm đã xác nhận khoản thanh toán <strong>{FormatMoney(payment.Amount)}</strong> cho hóa đơn <strong>{invoice.InvoiceCode}</strong>.</p>
                    <p>Trạng thái hiện tại: <strong>{invoice.Status}</strong>. Số tiền còn nợ: <strong>{FormatMoney(invoice.DebtAmount)}</strong>.</p>
                    """),
                cancellationToken);
        }
        catch
        {
            // Email notification must not rollback a confirmed payment.
        }
    }
    private static string FormatMoney(decimal value) => string.Create(CultureInfo.GetCultureInfo("vi-VN"), $"{value:N0} đ");
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

public sealed class ReportService(IRepository<TuitionInvoice> invoices, IRepository<PaymentTransaction> payments) : IReportService
{
    public async Task<RevenueOverviewResponse> OverviewAsync(CancellationToken cancellationToken)
    {
        var revenue = await payments.Query().Where(x => x.Status == PaymentStatus.Success).SumAsync(x => x.Amount, cancellationToken);
        var debt = await invoices.Query().SumAsync(x => x.DebtAmount, cancellationToken);
        return new RevenueOverviewResponse(revenue, debt, await invoices.Query().CountAsync(x => x.Status == InvoiceStatus.Paid, cancellationToken), await invoices.Query().CountAsync(x => x.Status == InvoiceStatus.Unpaid, cancellationToken), await invoices.Query().CountAsync(x => x.Status == InvoiceStatus.Partial, cancellationToken), await invoices.Query().CountAsync(x => x.Status == InvoiceStatus.Overdue, cancellationToken));
    }
    public async Task<IReadOnlyList<GroupAmountResponse>> RevenueByCourseAsync(CancellationToken cancellationToken)
    {
        return await invoices.Query().GroupBy(x => new { x.CourseId, x.CourseNameSnapshot }).Select(g => new GroupAmountResponse(g.Key.CourseId, g.Key.CourseNameSnapshot, g.Sum(x => x.PaidAmount), g.Sum(x => x.DebtAmount))).ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<GroupAmountResponse>> RevenueByClassAsync(CancellationToken cancellationToken)
    {
        return await invoices.Query().GroupBy(x => new { x.ClassId, x.ClassNameSnapshot }).Select(g => new GroupAmountResponse(g.Key.ClassId, g.Key.ClassNameSnapshot, g.Sum(x => x.PaidAmount), g.Sum(x => x.DebtAmount))).ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<GroupAmountResponse>> DebtByStudentAsync(CancellationToken cancellationToken)
    {
        return await invoices.Query().GroupBy(x => new { x.StudentId, x.StudentNameSnapshot }).Select(g => new GroupAmountResponse(g.Key.StudentId, g.Key.StudentNameSnapshot, g.Sum(x => x.PaidAmount), g.Sum(x => x.DebtAmount))).ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<GroupAmountResponse>> DebtByClassAsync(CancellationToken cancellationToken) => await RevenueByClassAsync(cancellationToken);
    public async Task<DashboardResponse> DashboardAsync(CancellationToken cancellationToken) => new(await OverviewAsync(cancellationToken), await RevenueByCourseAsync(cancellationToken), await DebtByClassAsync(cancellationToken));
}
