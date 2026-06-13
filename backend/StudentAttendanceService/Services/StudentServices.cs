using Microsoft.EntityFrameworkCore;
using StudentAttendanceService.Dtos;
using StudentAttendanceService.Entities;
using StudentAttendanceService.Enums;
using StudentAttendanceService.Mappings;
using StudentAttendanceService.Repositories;
using System.Net.Http.Json;

namespace StudentAttendanceService.Services;

public interface IStudentService
{
    Task<IReadOnlyList<StudentResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<PagedResult<StudentResponse>> SearchAsync(StudentSearchQuery query, CancellationToken cancellationToken);
    Task<StudentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken);
    Task<StudentResponse> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class StudentService(IRepository<Student> students) : IStudentService
{
    public async Task<IReadOnlyList<StudentResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        await students.Query().OrderBy(x => x.StudentCode).Select(x => x.ToResponse()).ToListAsync(cancellationToken);

    public async Task<PagedResult<StudentResponse>> SearchAsync(StudentSearchQuery query, CancellationToken cancellationToken)
    {
        var source = students.Query();
        if (!string.IsNullOrWhiteSpace(query.Keyword)) source = source.Where(x => x.StudentCode.Contains(query.Keyword) || x.FullName.Contains(query.Keyword) || x.Email.Contains(query.Keyword));
        if (query.Status.HasValue) source = source.Where(x => x.Status == query.Status);
        var total = await source.CountAsync(cancellationToken);
        var items = await source.OrderBy(x => x.StudentCode).Skip((query.SafePageIndex - 1) * query.SafePageSize).Take(query.SafePageSize).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
        return new PagedResult<StudentResponse> { Items = items, PageIndex = query.SafePageIndex, PageSize = query.SafePageSize, TotalItems = total };
    }

    public async Task<StudentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => (await students.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Student")).ToResponse();

    public async Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken)
    {
        if (await students.AnyAsync(x => x.StudentCode == request.StudentCode || x.Email == request.Email, cancellationToken)) throw Conflict("Student code or email already exists");
        var now = DateTime.UtcNow;
        var entity = new Student { Id = Guid.NewGuid(), StudentCode = request.StudentCode, FullName = request.FullName, Email = request.Email, Phone = request.Phone, DateOfBirth = request.DateOfBirth, Gender = request.Gender, Address = request.Address, AvatarUrl = request.AvatarUrl, Status = request.Status, CreatedAt = now, UpdatedAt = now };
        await students.AddAsync(entity, cancellationToken);
        await students.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task<StudentResponse> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken cancellationToken)
    {
        var entity = await students.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Student");
        if (await students.Query().AnyAsync(x => x.Id != id && (x.StudentCode == request.StudentCode || x.Email == request.Email), cancellationToken)) throw Conflict("Student code or email already exists");
        entity.StudentCode = request.StudentCode; entity.FullName = request.FullName; entity.Email = request.Email; entity.Phone = request.Phone; entity.DateOfBirth = request.DateOfBirth; entity.Gender = request.Gender; entity.Address = request.Address; entity.AvatarUrl = request.AvatarUrl; entity.Status = request.Status; entity.UpdatedAt = DateTime.UtcNow;
        await students.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await students.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Student");
        students.Remove(entity);
        await students.SaveChangesAsync(cancellationToken);
    }

    private static AppException NotFound(string name) => new($"{name} not found", StatusCodes.Status404NotFound);
    private static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
}

public interface IEnrollmentService
{
    Task<IReadOnlyList<EnrollmentResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<EnrollmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<EnrollmentResponse>> ByStudentAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<EnrollmentResponse>> ByClassAsync(Guid classId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StudentResponse>> ClassStudentsAsync(Guid classId, CancellationToken cancellationToken);
    Task<EnrollmentResponse> CreateAsync(CreateEnrollmentRequest request, CancellationToken cancellationToken);
    Task<EnrollmentResponse> SetStatusAsync(Guid id, EnrollmentStatus status, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class EnrollmentService(IRepository<Enrollment> enrollments, IRepository<Student> students, IEnrollmentPaymentSyncService paymentSync) : IEnrollmentService
{
    public async Task<IReadOnlyList<EnrollmentResponse>> GetAllAsync(CancellationToken cancellationToken) => await enrollments.Query().OrderByDescending(x => x.EnrolledAt).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<EnrollmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => (await enrollments.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Enrollment")).ToResponse();
    public async Task<IReadOnlyList<EnrollmentResponse>> ByStudentAsync(Guid studentId, CancellationToken cancellationToken) => await enrollments.Query().Where(x => x.StudentId == studentId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<EnrollmentResponse>> ByClassAsync(Guid classId, CancellationToken cancellationToken) => await enrollments.Query().Where(x => x.ClassId == classId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<StudentResponse>> ClassStudentsAsync(Guid classId, CancellationToken cancellationToken) =>
        await enrollments.Query().Where(x => x.ClassId == classId && (x.Status == EnrollmentStatus.Confirmed || x.Status == EnrollmentStatus.Studying)).Select(x => x.Student!).Select(x => x.ToResponse()).ToListAsync(cancellationToken);

    public async Task<EnrollmentResponse> CreateAsync(CreateEnrollmentRequest request, CancellationToken cancellationToken)
    {
        var student = await students.GetByIdAsync(request.StudentId, cancellationToken) ?? throw NotFound("Student");
        if (await enrollments.AnyAsync(x => x.StudentId == request.StudentId && x.ClassId == request.ClassId, cancellationToken)) throw Conflict("Student already enrolled in this class");
        var now = DateTime.UtcNow;
        var entity = new Enrollment { Id = Guid.NewGuid(), StudentId = student.Id, StudentNameSnapshot = student.FullName, CourseId = request.CourseId, CourseNameSnapshot = request.CourseNameSnapshot, ClassId = request.ClassId, ClassNameSnapshot = request.ClassNameSnapshot, EnrolledAt = now, Status = EnrollmentStatus.Pending, Note = request.Note, CreatedAt = now, UpdatedAt = now };
        await enrollments.AddAsync(entity, cancellationToken);
        await enrollments.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task<EnrollmentResponse> SetStatusAsync(Guid id, EnrollmentStatus status, CancellationToken cancellationToken)
    {
        var entity = await enrollments.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Enrollment");
        entity.Status = status;
        entity.UpdatedAt = DateTime.UtcNow;
        await enrollments.SaveChangesAsync(cancellationToken);
        if (status is EnrollmentStatus.Confirmed or EnrollmentStatus.Studying)
        {
            await paymentSync.SyncInvoiceAsync(entity, cancellationToken);
        }
        return entity.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await enrollments.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Enrollment");
        enrollments.Remove(entity);
        await enrollments.SaveChangesAsync(cancellationToken);
    }

    private static AppException NotFound(string name) => new($"{name} not found", StatusCodes.Status404NotFound);
    private static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
}

public interface IEnrollmentPaymentSyncService
{
    Task SyncInvoiceAsync(Enrollment enrollment, CancellationToken cancellationToken);
}

public sealed class EnrollmentPaymentSyncService(HttpClient httpClient, IConfiguration configuration, ILogger<EnrollmentPaymentSyncService> logger) : IEnrollmentPaymentSyncService
{
    public async Task SyncInvoiceAsync(Enrollment enrollment, CancellationToken cancellationToken)
    {
        if (!configuration.GetValue("PaymentIntegration:Enabled", true)) return;

        var baseUrl = configuration["PaymentIntegration:BaseUrl"]?.TrimEnd('/');
        var internalKey = configuration["PaymentIntegration:InternalKey"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(internalKey))
        {
            logger.LogWarning("Payment integration is missing BaseUrl or InternalKey.");
            return;
        }

        var totalAmount = configuration.GetValue("PaymentIntegration:DefaultTuitionAmount", 2500000m);
        var dueDays = configuration.GetValue("PaymentIntegration:DefaultDueDays", 14);
        var request = new
        {
            enrollmentId = enrollment.Id,
            studentId = enrollment.StudentId,
            studentNameSnapshot = enrollment.StudentNameSnapshot,
            courseId = enrollment.CourseId,
            courseNameSnapshot = enrollment.CourseNameSnapshot,
            classId = enrollment.ClassId,
            classNameSnapshot = enrollment.ClassNameSnapshot,
            totalAmount,
            dueDate = DateTime.UtcNow.AddDays(dueDays)
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/tuition-invoices/from-enrollment")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("X-EduCenter-Internal-Key", internalKey);

        try
        {
            var response = await httpClient.SendAsync(message, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Payment invoice sync failed with status {StatusCode}.", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Payment invoice sync failed. Enrollment status was still updated.");
        }
    }
}

public interface IAttendanceService
{
    Task<IReadOnlyList<AttendanceSessionResponse>> SessionsByClassAsync(Guid classId, CancellationToken cancellationToken);
    Task<AttendanceSessionResponse> GetSessionAsync(Guid id, CancellationToken cancellationToken);
    Task<AttendanceSessionResponse> CreateSessionAsync(CreateAttendanceSessionRequest request, CancellationToken cancellationToken);
    Task<AttendanceSessionResponse> LockSessionAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<AttendanceRecordResponse>> RecordsBySessionAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AttendanceRecordResponse>> RecordsByStudentAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AttendanceRecordResponse>> BulkAsync(BulkAttendanceRecordRequest request, CancellationToken cancellationToken);
    Task<AttendanceRecordResponse> UpdateRecordAsync(Guid id, UpdateAttendanceRecordRequest request, CancellationToken cancellationToken);
    Task DeleteRecordAsync(Guid id, CancellationToken cancellationToken);
    Task<AttendanceSummaryResponse> ClassSummaryAsync(Guid classId, CancellationToken cancellationToken);
    Task<AttendanceSummaryResponse> StudentSummaryAsync(Guid studentId, CancellationToken cancellationToken);
}

public sealed class AttendanceService(IRepository<AttendanceSession> sessions, IRepository<AttendanceRecord> records, IRepository<Enrollment> enrollments, IRepository<Student> students) : IAttendanceService
{
    public async Task<IReadOnlyList<AttendanceSessionResponse>> SessionsByClassAsync(Guid classId, CancellationToken cancellationToken) => await sessions.Query().Where(x => x.ClassId == classId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<AttendanceSessionResponse> GetSessionAsync(Guid id, CancellationToken cancellationToken) => (await sessions.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Attendance session")).ToResponse();
    public async Task<AttendanceSessionResponse> CreateSessionAsync(CreateAttendanceSessionRequest request, CancellationToken cancellationToken)
    {
        if (await sessions.AnyAsync(x => x.ClassId == request.ClassId && x.AttendanceDate.Date == request.AttendanceDate.Date && x.SessionNumber == request.SessionNumber, cancellationToken)) throw Conflict("Attendance session already exists");
        var now = DateTime.UtcNow;
        var entity = new AttendanceSession { Id = Guid.NewGuid(), ClassId = request.ClassId, ClassNameSnapshot = request.ClassNameSnapshot, ScheduleId = request.ScheduleId, SessionNumber = request.SessionNumber, AttendanceDate = request.AttendanceDate, Topic = request.Topic, CreatedByTeacherId = request.CreatedByTeacherId, CreatedByTeacherName = request.CreatedByTeacherName, Status = AttendanceSessionStatus.Open, CreatedAt = now, UpdatedAt = now };
        await sessions.AddAsync(entity, cancellationToken);
        await sessions.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task<AttendanceSessionResponse> LockSessionAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await sessions.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Attendance session");
        entity.Status = AttendanceSessionStatus.Locked;
        entity.UpdatedAt = DateTime.UtcNow;
        await sessions.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task<IReadOnlyList<AttendanceRecordResponse>> RecordsBySessionAsync(Guid sessionId, CancellationToken cancellationToken) => await records.Query().Where(x => x.AttendanceSessionId == sessionId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<AttendanceRecordResponse>> RecordsByStudentAsync(Guid studentId, CancellationToken cancellationToken) => await records.Query().Where(x => x.StudentId == studentId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<AttendanceRecordResponse>> BulkAsync(BulkAttendanceRecordRequest request, CancellationToken cancellationToken)
    {
        var session = await sessions.GetByIdAsync(request.AttendanceSessionId, cancellationToken) ?? throw NotFound("Attendance session");
        if (session.Status == AttendanceSessionStatus.Locked) throw Conflict("Attendance session is locked");
        var now = DateTime.UtcNow;
        foreach (var item in request.Records)
        {
            if (!await enrollments.AnyAsync(x => x.StudentId == item.StudentId && x.ClassId == session.ClassId && (x.Status == EnrollmentStatus.Confirmed || x.Status == EnrollmentStatus.Studying), cancellationToken)) throw Conflict("Only confirmed or studying students can be marked");
            var student = await students.GetByIdAsync(item.StudentId, cancellationToken) ?? throw NotFound("Student");
            var existing = await records.Query().FirstOrDefaultAsync(x => x.AttendanceSessionId == request.AttendanceSessionId && x.StudentId == item.StudentId, cancellationToken);
            if (existing is null)
            {
                await records.AddAsync(new AttendanceRecord { Id = Guid.NewGuid(), AttendanceSessionId = request.AttendanceSessionId, StudentId = item.StudentId, StudentNameSnapshot = student.FullName, Status = item.Status, Note = item.Note, MarkedAt = now }, cancellationToken);
            }
            else
            {
                existing.Status = item.Status; existing.Note = item.Note; existing.MarkedAt = now;
            }
        }
        await records.SaveChangesAsync(cancellationToken);
        return await RecordsBySessionAsync(request.AttendanceSessionId, cancellationToken);
    }
    public async Task<AttendanceRecordResponse> UpdateRecordAsync(Guid id, UpdateAttendanceRecordRequest request, CancellationToken cancellationToken)
    {
        var entity = await records.Query().Include(x => x.AttendanceSession).FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw NotFound("Attendance record");
        if (entity.AttendanceSession?.Status == AttendanceSessionStatus.Locked) throw Conflict("Attendance session is locked");
        entity.Status = request.Status; entity.Note = request.Note; entity.MarkedAt = DateTime.UtcNow;
        await records.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task DeleteRecordAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await records.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Attendance record");
        records.Remove(entity);
        await records.SaveChangesAsync(cancellationToken);
    }
    public async Task<AttendanceSummaryResponse> ClassSummaryAsync(Guid classId, CancellationToken cancellationToken)
    {
        var classSessionIds = await sessions.Query().Where(x => x.ClassId == classId).Select(x => x.Id).ToListAsync(cancellationToken);
        var classRecords = await records.Query().Where(x => classSessionIds.Contains(x.AttendanceSessionId)).ToListAsync(cancellationToken);
        return new AttendanceSummaryResponse(classId, classSessionIds.Count, Percent(classRecords));
    }
    public async Task<AttendanceSummaryResponse> StudentSummaryAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var studentRecords = await records.Query().Where(x => x.StudentId == studentId).ToListAsync(cancellationToken);
        return new AttendanceSummaryResponse(studentId, studentRecords.Count, Percent(studentRecords));
    }
    private static decimal Percent(IReadOnlyList<AttendanceRecord> rows)
    {
        if (rows.Count == 0) return 0;
        decimal score = rows.Sum(x => x.Status switch { AttendanceStatus.Present => 1m, AttendanceStatus.Late => 0.5m, AttendanceStatus.Excused => 0.5m, _ => 0m });
        return Math.Round(score / rows.Count * 100, 2);
    }
    private static AppException NotFound(string name) => new($"{name} not found", StatusCodes.Status404NotFound);
    private static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
}

public interface IResultService
{
    Task<IReadOnlyList<StudentResultResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<StudentResultResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<StudentResultResponse>> ByStudentAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StudentResultResponse>> ByClassAsync(Guid classId, CancellationToken cancellationToken);
    Task<StudentResultResponse> CreateAsync(CreateStudentResultRequest request, CancellationToken cancellationToken);
    Task<StudentResultResponse> UpdateAsync(Guid id, UpdateStudentResultRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class ResultService(IRepository<StudentResult> results, IRepository<Student> students) : IResultService
{
    public async Task<IReadOnlyList<StudentResultResponse>> GetAllAsync(CancellationToken cancellationToken) => await results.Query().Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<StudentResultResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => (await results.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Result")).ToResponse();
    public async Task<IReadOnlyList<StudentResultResponse>> ByStudentAsync(Guid studentId, CancellationToken cancellationToken) => await results.Query().Where(x => x.StudentId == studentId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<StudentResultResponse>> ByClassAsync(Guid classId, CancellationToken cancellationToken) => await results.Query().Where(x => x.ClassId == classId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<StudentResultResponse> CreateAsync(CreateStudentResultRequest request, CancellationToken cancellationToken)
    {
        var student = await students.GetByIdAsync(request.StudentId, cancellationToken) ?? throw NotFound("Student");
        var now = DateTime.UtcNow;
        var entity = new StudentResult { Id = Guid.NewGuid(), StudentId = student.Id, StudentNameSnapshot = student.FullName, CourseId = request.CourseId, CourseNameSnapshot = request.CourseNameSnapshot, ClassId = request.ClassId, ClassNameSnapshot = request.ClassNameSnapshot, MidtermScore = request.MidtermScore, FinalScore = request.FinalScore, AttendancePercent = request.AttendancePercent, Feedback = request.Feedback, EvaluatedByTeacherId = request.EvaluatedByTeacherId, EvaluatedByTeacherName = request.EvaluatedByTeacherName, EvaluatedAt = now, CreatedAt = now, UpdatedAt = now };
        ApplyResult(entity);
        await results.AddAsync(entity, cancellationToken);
        await results.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task<StudentResultResponse> UpdateAsync(Guid id, UpdateStudentResultRequest request, CancellationToken cancellationToken)
    {
        var entity = await results.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Result");
        entity.CourseId = request.CourseId; entity.CourseNameSnapshot = request.CourseNameSnapshot; entity.ClassId = request.ClassId; entity.ClassNameSnapshot = request.ClassNameSnapshot; entity.MidtermScore = request.MidtermScore; entity.FinalScore = request.FinalScore; entity.AttendancePercent = request.AttendancePercent; entity.Feedback = request.Feedback; entity.EvaluatedByTeacherId = request.EvaluatedByTeacherId; entity.EvaluatedByTeacherName = request.EvaluatedByTeacherName; entity.EvaluatedAt = DateTime.UtcNow; entity.UpdatedAt = DateTime.UtcNow;
        ApplyResult(entity);
        await results.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await results.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Result");
        results.Remove(entity);
        await results.SaveChangesAsync(cancellationToken);
    }
    private static void ApplyResult(StudentResult x)
    {
        x.AverageScore = Math.Round(x.MidtermScore * 0.4m + x.FinalScore * 0.6m, 2);
        x.ResultStatus = x.AverageScore >= 5 && x.AttendancePercent >= 70 ? ResultStatus.Passed : ResultStatus.Failed;
    }
    private static AppException NotFound(string name) => new($"{name} not found", StatusCodes.Status404NotFound);
}

public interface IStudentPortalService
{
    Task<LearningProfileResponse> LearningProfileAsync(Guid studentId, CancellationToken cancellationToken);
}

public sealed class StudentPortalService(IStudentService students, IEnrollmentService enrollments, IResultService results, IAttendanceService attendance) : IStudentPortalService
{
    public async Task<LearningProfileResponse> LearningProfileAsync(Guid studentId, CancellationToken cancellationToken) =>
        new(await students.GetByIdAsync(studentId, cancellationToken), await enrollments.ByStudentAsync(studentId, cancellationToken), await results.ByStudentAsync(studentId, cancellationToken), await attendance.StudentSummaryAsync(studentId, cancellationToken));
}
