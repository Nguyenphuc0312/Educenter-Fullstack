using Microsoft.EntityFrameworkCore;
using StudentAttendanceService.Dtos;
using StudentAttendanceService.Entities;
using StudentAttendanceService.Enums;
using StudentAttendanceService.Mappings;
using StudentAttendanceService.Repositories;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

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
    Task<EnrollmentResponse> ConfirmAsync(Guid id, string? bearerToken, CancellationToken cancellationToken);
    Task<EnrollmentResponse> SetStatusAsync(Guid id, EnrollmentStatus status, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface IClassCapacityClient
{
    Task IncreaseStudentCountAsync(Guid classId, string? bearerToken, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<Guid, int>> GetCourseTotalSessionsAsync(IEnumerable<Guid> courseIds, CancellationToken cancellationToken);
    Task<decimal?> GetCourseTuitionFeeAsync(Guid courseId, CancellationToken cancellationToken);
}

public sealed class ClassCapacityClient(HttpClient httpClient) : IClassCapacityClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task IncreaseStudentCountAsync(Guid classId, string? bearerToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/classes/{classId}/increase-student-count");
        if (!string.IsNullOrWhiteSpace(bearerToken)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode) return;

        var detail = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new AppException($"Cannot increase class student count. Course service returned {(int)response.StatusCode}: {detail}", StatusCodes.Status502BadGateway);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetCourseTotalSessionsAsync(IEnumerable<Guid> courseIds, CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, int>();
        foreach (var courseId in courseIds.Distinct())
        {
            using var response = await httpClient.GetAsync($"/api/courses/{courseId}", cancellationToken);
            if (!response.IsSuccessStatusCode) continue;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<ApiResponse<CourseSnapshot>>(stream, JsonOptions, cancellationToken);
            if (payload?.Data is { TotalSessions: > 0 } course) result[courseId] = course.TotalSessions;
        }

        return result;
    }

    public async Task<decimal?> GetCourseTuitionFeeAsync(Guid courseId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"/api/courses/{courseId}", cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ApiResponse<CourseSnapshot>>(stream, JsonOptions, cancellationToken);
        return payload?.Data?.TuitionFee;
    }

    private sealed record CourseSnapshot(Guid Id, int TotalSessions, decimal TuitionFee);
}

public interface IPaymentInvoiceClient
{
    Task CreateInvoiceForEnrollmentAsync(Enrollment enrollment, decimal tuitionFee, string? bearerToken, CancellationToken cancellationToken);
}

public sealed class PaymentInvoiceClient(HttpClient httpClient) : IPaymentInvoiceClient
{
    private sealed record CreateInvoiceFromEnrollmentRequest(Guid EnrollmentId, Guid StudentId, string StudentNameSnapshot, Guid CourseId, string CourseNameSnapshot, Guid ClassId, string ClassNameSnapshot, decimal TotalAmount, DateTime DueDate);

    public async Task CreateInvoiceForEnrollmentAsync(Enrollment enrollment, decimal tuitionFee, string? bearerToken, CancellationToken cancellationToken)
    {
        if (tuitionFee <= 0) return;

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/tuition-invoices/from-enrollment");
        if (!string.IsNullOrWhiteSpace(bearerToken)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        request.Content = JsonContent.Create(new CreateInvoiceFromEnrollmentRequest(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.StudentNameSnapshot,
            enrollment.CourseId,
            enrollment.CourseNameSnapshot,
            enrollment.ClassId,
            enrollment.ClassNameSnapshot,
            tuitionFee,
            DateTime.UtcNow.AddDays(14)));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode) return;

        var detail = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new AppException($"Cannot create tuition invoice. Payment service returned {(int)response.StatusCode}: {detail}", StatusCodes.Status502BadGateway);
    }
}

public sealed class EnrollmentService(IRepository<Enrollment> enrollments, IRepository<Student> students, IClassCapacityClient classCapacity, IPaymentInvoiceClient paymentInvoices) : IEnrollmentService
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

    public async Task<EnrollmentResponse> ConfirmAsync(Guid id, string? bearerToken, CancellationToken cancellationToken)
    {
        var entity = await enrollments.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Enrollment");
        if (entity.Status == EnrollmentStatus.Confirmed) return entity.ToResponse();

        EnsureValidTransition(entity.Status, EnrollmentStatus.Confirmed);
        await classCapacity.IncreaseStudentCountAsync(entity.ClassId, bearerToken, cancellationToken);
        var tuitionFee = await classCapacity.GetCourseTuitionFeeAsync(entity.CourseId, cancellationToken);
        if (tuitionFee is > 0) await paymentInvoices.CreateInvoiceForEnrollmentAsync(entity, tuitionFee.Value, bearerToken, cancellationToken);

        entity.Status = EnrollmentStatus.Confirmed;
        entity.UpdatedAt = DateTime.UtcNow;
        await enrollments.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task<EnrollmentResponse> SetStatusAsync(Guid id, EnrollmentStatus status, CancellationToken cancellationToken)
    {
        var entity = await enrollments.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Enrollment");
        EnsureValidTransition(entity.Status, status);
        entity.Status = status;
        entity.UpdatedAt = DateTime.UtcNow;
        await enrollments.SaveChangesAsync(cancellationToken);
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
    private static void EnsureValidTransition(EnrollmentStatus current, EnrollmentStatus next)
    {
        if (current == next) return;
        var valid = current switch
        {
            EnrollmentStatus.Pending => next is EnrollmentStatus.Confirmed or EnrollmentStatus.Cancelled,
            EnrollmentStatus.Confirmed => next is EnrollmentStatus.Studying or EnrollmentStatus.Cancelled,
            EnrollmentStatus.Studying => next is EnrollmentStatus.Completed or EnrollmentStatus.Cancelled,
            EnrollmentStatus.Completed => false,
            EnrollmentStatus.Cancelled => false,
            _ => false
        };
        if (!valid) throw Conflict($"Cannot change enrollment status from {current} to {next}");
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
    Task<IReadOnlyList<MyCourseResponse>> MyCoursesAsync(Guid studentId, CancellationToken cancellationToken);
}

public sealed class StudentPortalService(IStudentService students, IEnrollmentService enrollments, IResultService results, IAttendanceService attendance, IRepository<AttendanceSession> sessions, IClassCapacityClient courseClient) : IStudentPortalService
{
    public async Task<LearningProfileResponse> LearningProfileAsync(Guid studentId, CancellationToken cancellationToken) =>
        new(await students.GetByIdAsync(studentId, cancellationToken), await MyCoursesAsync(studentId, cancellationToken), await results.ByStudentAsync(studentId, cancellationToken), await attendance.StudentSummaryAsync(studentId, cancellationToken));

    public async Task<IReadOnlyList<MyCourseResponse>> MyCoursesAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var courses = await enrollments.ByStudentAsync(studentId, cancellationToken);
        if (courses.Count == 0) return courses.Select(ToMyCourseWithoutProgress).ToList();

        var classIds = courses.Select(x => x.ClassId).Distinct().ToList();
        var courseIds = courses.Select(x => x.CourseId).Distinct().ToList();
        var totalSessionsByCourse = await courseClient.GetCourseTotalSessionsAsync(courseIds, cancellationToken);
        var completedSessionsByClass = await sessions.Query()
            .Where(x => classIds.Contains(x.ClassId))
            .GroupBy(x => x.ClassId)
            .Select(g => new { ClassId = g.Key, Completed = g.Count() })
            .ToDictionaryAsync(x => x.ClassId, x => x.Completed, cancellationToken);

        return courses.Select(course =>
        {
            var totalSessions = totalSessionsByCourse.GetValueOrDefault(course.CourseId);
            var completedSessions = completedSessionsByClass.GetValueOrDefault(course.ClassId);
            if (totalSessions > 0) completedSessions = Math.Min(completedSessions, totalSessions);
            var canShowProgress = course.Status is EnrollmentStatus.Studying or EnrollmentStatus.Completed;
            decimal? progressPercent = course.Status switch
            {
                EnrollmentStatus.Completed => 100m,
                EnrollmentStatus.Studying when totalSessions > 0 => Math.Round((decimal)completedSessions / totalSessions * 100, 2),
                EnrollmentStatus.Studying => 0m,
                _ => null
            };

            return new MyCourseResponse(course.Id, course.StudentId, course.StudentNameSnapshot, course.CourseId, course.CourseNameSnapshot, course.ClassId, course.ClassNameSnapshot, course.EnrolledAt, course.Status, course.Note, course.CreatedAt, course.UpdatedAt, totalSessions, completedSessions, progressPercent, canShowProgress);
        }).ToList();
    }

    private static MyCourseResponse ToMyCourseWithoutProgress(EnrollmentResponse course) =>
        new(course.Id, course.StudentId, course.StudentNameSnapshot, course.CourseId, course.CourseNameSnapshot, course.ClassId, course.ClassNameSnapshot, course.EnrolledAt, course.Status, course.Note, course.CreatedAt, course.UpdatedAt, 0, 0, null, false);
}
