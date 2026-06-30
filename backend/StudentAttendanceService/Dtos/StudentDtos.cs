using StudentAttendanceService.Enums;
using System.ComponentModel.DataAnnotations;

namespace StudentAttendanceService.Dtos;

public sealed record StudentResponse(Guid Id, string StudentCode, string FullName, string Email, string Phone, DateTime DateOfBirth, Gender Gender, string Address, string? AvatarUrl, StudentStatus Status, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record EnrollmentResponse(Guid Id, Guid StudentId, string StudentNameSnapshot, Guid CourseId, string CourseNameSnapshot, Guid ClassId, string ClassNameSnapshot, DateTime EnrolledAt, EnrollmentStatus Status, string? Note, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record MyCourseResponse(Guid Id, Guid StudentId, string StudentNameSnapshot, Guid CourseId, string CourseNameSnapshot, Guid ClassId, string ClassNameSnapshot, DateTime EnrolledAt, EnrollmentStatus Status, string? Note, DateTime CreatedAt, DateTime UpdatedAt, DateTime? ClassStartDate, DateTime? ClassEndDate, int TotalSessions, int CompletedSessions, decimal? ProgressPercent, bool CanShowProgress, IReadOnlyList<Guid> TeacherIds, IReadOnlyList<string> TeacherNames);
public sealed record AttendanceSessionResponse(Guid Id, Guid ClassId, string ClassNameSnapshot, Guid ScheduleId, int SessionNumber, DateTime AttendanceDate, string Topic, Guid CreatedByTeacherId, string CreatedByTeacherName, AttendanceSessionStatus Status, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record AttendanceRecordResponse(Guid Id, Guid AttendanceSessionId, Guid StudentId, string StudentNameSnapshot, AttendanceStatus Status, string? Note, DateTime MarkedAt);
public sealed record StudentResultResponse(Guid Id, Guid StudentId, string StudentNameSnapshot, Guid CourseId, string CourseNameSnapshot, Guid ClassId, string ClassNameSnapshot, decimal MidtermScore, decimal FinalScore, decimal AverageScore, decimal AttendancePercent, ResultStatus ResultStatus, string? Feedback, Guid EvaluatedByTeacherId, string EvaluatedByTeacherName, DateTime EvaluatedAt, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record TeacherReviewResponse(Guid Id, Guid TeacherId, string TeacherNameSnapshot, decimal Rating, string? Comment);
public sealed record CourseReviewResponse(Guid Id, Guid EnrollmentId, Guid StudentId, string StudentNameSnapshot, Guid CourseId, string CourseNameSnapshot, Guid ClassId, string ClassNameSnapshot, decimal CourseRating, string? CourseComment, IReadOnlyList<TeacherReviewResponse> TeacherReviews, DateTime CreatedAt, DateTime UpdatedAt);

public class CreateStudentRequest
{
    public string StudentCode { get; set; } = string.Empty;
    [Required] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public StudentStatus Status { get; set; } = StudentStatus.Active;
}
public sealed class UpdateStudentRequest : CreateStudentRequest;

public sealed class CompleteStudentProfileRequest
{
    [Required] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; } = Gender.Unknown;
    public string Address { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

public sealed class StudentSearchQuery : PaginationQuery { public string? Keyword { get; init; } public StudentStatus? Status { get; init; } }

public sealed class CreateEnrollmentRequest
{
    [Required] public Guid StudentId { get; set; }
    [Required] public Guid CourseId { get; set; }
    [Required] public string CourseNameSnapshot { get; set; } = string.Empty;
    [Required] public Guid ClassId { get; set; }
    [Required] public string ClassNameSnapshot { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public sealed class CreateAttendanceSessionRequest
{
    [Required] public Guid ClassId { get; set; }
    [Required] public string ClassNameSnapshot { get; set; } = string.Empty;
    [Required] public Guid ScheduleId { get; set; }
    [Range(1, 300)] public int SessionNumber { get; set; }
    public DateTime AttendanceDate { get; set; }
    [Required] public string Topic { get; set; } = string.Empty;
    [Required] public Guid CreatedByTeacherId { get; set; }
    [Required] public string CreatedByTeacherName { get; set; } = string.Empty;
}

public sealed class AttendanceRecordItemRequest
{
    [Required] public Guid StudentId { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Note { get; set; }
}

public sealed class BulkAttendanceRecordRequest
{
    [Required] public Guid AttendanceSessionId { get; set; }
    public List<AttendanceRecordItemRequest> Records { get; set; } = [];
}

public sealed class UpdateAttendanceRecordRequest
{
    public AttendanceStatus Status { get; set; }
    public string? Note { get; set; }
}

public class CreateStudentResultRequest
{
    [Required] public Guid StudentId { get; set; }
    [Required] public Guid CourseId { get; set; }
    [Required] public string CourseNameSnapshot { get; set; } = string.Empty;
    [Required] public Guid ClassId { get; set; }
    [Required] public string ClassNameSnapshot { get; set; } = string.Empty;
    [Range(0, 10)] public decimal MidtermScore { get; set; }
    [Range(0, 10)] public decimal FinalScore { get; set; }
    public decimal AttendancePercent { get; set; }
    public string? Feedback { get; set; }
    [Required] public Guid EvaluatedByTeacherId { get; set; }
    [Required] public string EvaluatedByTeacherName { get; set; } = string.Empty;
}
public sealed class UpdateStudentResultRequest : CreateStudentResultRequest;

public sealed class TeacherReviewItemRequest
{
    [Required] public Guid TeacherId { get; set; }
    [Required] public string TeacherNameSnapshot { get; set; } = string.Empty;
    [Range(1, 5)] public decimal Rating { get; set; }
    public string? Comment { get; set; }
}

public class CreateCourseReviewRequest
{
    [Required] public Guid EnrollmentId { get; set; }
    [Range(1, 5)] public decimal CourseRating { get; set; }
    public string? CourseComment { get; set; }
    public List<TeacherReviewItemRequest> TeacherReviews { get; set; } = [];
}

public sealed class UpdateCourseReviewRequest
{
    [Range(1, 5)] public decimal CourseRating { get; set; }
    public string? CourseComment { get; set; }
    public List<TeacherReviewItemRequest> TeacherReviews { get; set; } = [];
}

public sealed record AttendanceSummaryResponse(Guid ScopeId, int TotalSessions, decimal AttendancePercent);
public sealed record LearningProfileResponse(StudentResponse Student, IReadOnlyList<MyCourseResponse> Courses, IReadOnlyList<StudentResultResponse> Results, AttendanceSummaryResponse AttendanceSummary);
