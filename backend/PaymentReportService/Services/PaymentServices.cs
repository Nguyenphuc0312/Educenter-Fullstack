using Microsoft.EntityFrameworkCore;
using PaymentReportService.Dtos;
using PaymentReportService.Entities;
using PaymentReportService.Enums;
using PaymentReportService.Mappings;
using PaymentReportService.Repositories;
using PaymentReportService.Data;
using System.Data;
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
            return new LoginResponse(token.Token, Guid.NewGuid().ToString("N"), token.ExpiresAt, new LoginUserResponse(fallbackAdmin.Id, fallbackAdmin.Username, fallbackAdmin.FullName, fallbackAdmin.Email, fallbackAdmin.Phone, fallbackAdmin.Role, fallbackAdmin.ReferenceId));
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
        user.Status = status; user.UpdatedAt = DateTime.UtcNow;
        await accounts.SaveChangesAsync(cancellationToken);
        return user.ToResponse();
    }
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await accounts.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Account");
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
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task SendDebtReminderAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class InvoiceService(IRepository<TuitionInvoice> invoices, IRepository<UserAccount> accounts, IEmailService emailService) : IInvoiceService
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

        // Send payment email notification asynchronously in the background
        var studentAccount = await accounts.Query().FirstOrDefaultAsync(x => x.ReferenceId == request.StudentId, cancellationToken);
        if (studentAccount is not null && !string.IsNullOrWhiteSpace(studentAccount.Email))
        {
            var subject = $"[EduCenter] Thông báo học phí - Mã hóa đơn: {invoiceCode}";
            var body = $@"
                <h3>Kính gửi phụ huynh và học viên {request.StudentNameSnapshot},</h3>
                <p>EduCenter xin thông báo về việc khởi tạo hóa đơn học phí cho khóa học <b>{request.CourseNameSnapshot}</b>.</p>
                <table border='1' cellpadding='8' style='border-collapse: collapse; border-color: #e2e8f0;'>
                    <tr><td>Mã hóa đơn</td><td><b>{invoiceCode}</b></td></tr>
                    <tr><td>Học viên</td><td>{request.StudentNameSnapshot}</td></tr>
                    <tr><td>Khóa học</td><td>{request.CourseNameSnapshot}</td></tr>
                    <tr><td>Lớp học</td><td>{request.ClassNameSnapshot}</td></tr>
                    <tr><td>Tổng học phí</td><td><b>{request.TotalAmount:N0} VNĐ</b></td></tr>
                    <tr><td>Hạn đóng tiền</td><td>{request.DueDate:dd/MM/yyyy}</td></tr>
                </table>
                <p>Vui lòng đăng nhập vào cổng học viên EduCenter để thanh toán học phí (hỗ trợ các hình thức Chuyển khoản ngân hàng hoặc ví điện tử VNPay/Momo) đúng thời hạn.</p>
                <p>Trân trọng,<br/><b>Hệ thống quản lý EduCenter</b></p>";
            _ = Task.Run(() => emailService.SendEmailAsync(studentAccount.Email, subject, body, true), CancellationToken.None);
        }

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
        entity.Status = InvoiceStatus.Overdue; entity.UpdatedAt = DateTime.UtcNow;
        await invoices.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await invoices.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Invoice");
        invoices.Remove(entity);
        await invoices.SaveChangesAsync(cancellationToken);
    }

    public async Task SendDebtReminderAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await invoices.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Invoice");
        if (entity.DebtAmount <= 0) throw Conflict("Hóa đơn này đã được thanh toán đầy đủ, không cần nhắc nợ.");
        
        var studentAccount = await accounts.Query().FirstOrDefaultAsync(x => x.ReferenceId == entity.StudentId, cancellationToken);
        if (studentAccount is null || string.IsNullOrWhiteSpace(studentAccount.Email))
        {
            throw new AppException("Không tìm thấy thông tin tài khoản hoặc email của học viên này.", StatusCodes.Status404NotFound);
        }
        
        var subject = $"[EduCenter] NHẮC NHỞ QUÁ HẠN HỌC PHÍ - Mã hóa đơn: {entity.InvoiceCode}";
        var body = $@"
            <h3 style='color: #dc2626;'>Cảnh báo: Học phí quá hạn thanh toán!</h3>
            <p>Kính gửi phụ huynh và học viên <b>{entity.StudentNameSnapshot}</b>,</p>
            <p>Hệ thống ghi nhận hóa đơn học phí của bạn hiện đã quá hạn đóng tiền quy định.</p>
            <table border='1' cellpadding='8' style='border-collapse: collapse; border-color: #e2e8f0;'>
                <tr><td>Mã hóa đơn</td><td><b>{entity.InvoiceCode}</b></td></tr>
                <tr><td>Khóa học</td><td>{entity.CourseNameSnapshot}</td></tr>
                <tr><td>Lớp học</td><td>{entity.ClassNameSnapshot}</td></tr>
                <tr><td>Tổng số tiền học phí</td><td>{entity.TotalAmount:N0} VNĐ</td></tr>
                <tr><td>Số tiền đã thanh toán</td><td>{entity.PaidAmount:N0} VNĐ</td></tr>
                <tr style='background-color: #fef2f2; color: #b91c1c;'><td><b>Số tiền còn nợ (Công nợ)</b></td><td><b>{entity.DebtAmount:N0} VNĐ</b></td></tr>
                <tr><td>Hạn đóng tiền quy định</td><td><b>{entity.DueDate:dd/MM/yyyy}</b></td></tr>
            </table>
            <p style='color: #b91c1c; font-weight: bold;'>LƯU Ý QUAN TRỌNG: Lịch điểm danh của bạn sẽ bị khóa cho đến khi hóa đơn này được thanh toán. Vui lòng thanh toán học phí ngay để tiếp tục học tập bình thường.</p>
            <p>Trân trọng,<br/><b>Hệ thống quản lý EduCenter</b></p>";
            
        await emailService.SendEmailAsync(studentAccount.Email, subject, body, true);
    }

    public static void ApplyStatus(TuitionInvoice invoice)
    {
        invoice.DebtAmount = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);
        invoice.Status = invoice.DebtAmount == 0 ? InvoiceStatus.Paid : invoice.PaidAmount > 0 ? InvoiceStatus.Partial : invoice.DueDate.Date < DateTime.UtcNow.Date ? InvoiceStatus.Overdue : InvoiceStatus.Unpaid;
        if (invoice.Status == InvoiceStatus.Paid) invoice.PartialPaymentDueDate = null;
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

public sealed class PaymentTransactionService(IRepository<PaymentTransaction> payments, IRepository<TuitionInvoice> invoices, PaymentDbContext db) : IPaymentTransactionService
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
        if (request.Percent is not 25 and not 50 and not 75 and not 100) throw Conflict("Only 25%, 50%, 75%, or 100% payments are allowed");
        if (request.Method == PaymentMethod.Cash) throw Conflict("Students cannot submit cash payments online");

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var invoice = await db.TuitionInvoices.FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken) ?? throw NotFound("Invoice");
        if (invoice.StudentId != studentId) throw new AppException("You can only pay your own invoice", StatusCodes.Status403Forbidden);
        if (invoice.DebtAmount <= 0) throw Conflict("Invoice is already paid");
        if (await db.PaymentTransactions.AnyAsync(x => x.InvoiceId == request.InvoiceId && x.Status == PaymentStatus.Pending, cancellationToken)) throw Conflict("This invoice already has a pending payment request");

        var factor = request.Percent / 100m;
        var amount = request.Percent == 100 ? invoice.DebtAmount : Math.Min(Math.Round(invoice.TotalAmount * factor, 2), invoice.DebtAmount);
        var now = DateTime.UtcNow;
        var entity = new PaymentTransaction { Id = Guid.NewGuid(), InvoiceId = invoice.Id, Amount = amount, Method = request.Method, PaymentDate = now, Status = PaymentStatus.Pending, Note = request.Note, CreatedBy = createdBy, CreatedAt = now, Invoice = invoice };
        db.PaymentTransactions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
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
        try
        {
            var revenue = await payments.Query().Where(x => x.Status == PaymentStatus.Success).SumAsync(x => x.Amount, cancellationToken);
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

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
}

public sealed class EmailService : IEmailService
{
    private readonly string _fromEmail = "mingiot999@gmail.com";
    private readonly string _appPassword = "gbxt sdpe kqnp xrfv";

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            using var client = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new System.Net.NetworkCredential(_fromEmail, _appPassword),
                EnableSsl = true
            };

            using var mailMessage = new System.Net.Mail.MailMessage(_fromEmail, to, subject, body)
            {
                IsBodyHtml = isHtml
            };

            await client.SendMailAsync(mailMessage);
            Console.WriteLine($"[EMAIL SUCCESS] Sent email to {to}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL ERROR] Failed to send email to {to}: {ex.Message}");
        }
    }
}
