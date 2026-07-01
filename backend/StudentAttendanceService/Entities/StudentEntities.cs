using StudentAttendanceService.Enums;
using System.ComponentModel.DataAnnotations;

namespace StudentAttendanceService.Entities;

public sealed class Student
{
    public Guid Id { get; set; }
    [MaxLength(50)] public string StudentCode { get; set; } = string.Empty;
    [MaxLength(200)] public string FullName { get; set; } = string.Empty;
    [MaxLength(200)] public string Email { get; set; } = string.Empty;
    [MaxLength(30)] public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    [MaxLength(500)] public string Address { get; set; } = string.Empty;
    [MaxLength(600)] public string? AvatarUrl { get; set; }
    public StudentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Enrollment> Enrollments { get; set; } = [];
}

public sealed class Enrollment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    [MaxLength(200)] public string StudentNameSnapshot { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    [MaxLength(200)] public string CourseNameSnapshot { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    [MaxLength(200)] public string ClassNameSnapshot { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public EnrollmentStatus Status { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Student? Student { get; set; }
}

public sealed class AttendanceSession
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    [MaxLength(200)] public string ClassNameSnapshot { get; set; } = string.Empty;
    public Guid ScheduleId { get; set; }
    public int SessionNumber { get; set; }
    public DateTime AttendanceDate { get; set; }
    [MaxLength(250)] public string Topic { get; set; } = string.Empty;
    public Guid CreatedByTeacherId { get; set; }
    [MaxLength(200)] public string CreatedByTeacherName { get; set; } = string.Empty;
    public AttendanceSessionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AttendanceRecord> Records { get; set; } = [];
}

public sealed class AttendanceRecord
{
    public Guid Id { get; set; }
    public Guid AttendanceSessionId { get; set; }
    public Guid StudentId { get; set; }
    [MaxLength(200)] public string StudentNameSnapshot { get; set; } = string.Empty;
    public AttendanceStatus Status { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
    public DateTime MarkedAt { get; set; }
    public AttendanceSession? AttendanceSession { get; set; }
}

public sealed class StudentResult
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    [MaxLength(200)] public string StudentNameSnapshot { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    [MaxLength(200)] public string CourseNameSnapshot { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    [MaxLength(200)] public string ClassNameSnapshot { get; set; } = string.Empty;
    public decimal MidtermScore { get; set; }
    public decimal FinalScore { get; set; }
    public decimal AverageScore { get; set; }
    public decimal AttendancePercent { get; set; }
    public ResultStatus ResultStatus { get; set; }
    [MaxLength(1000)] public string? Feedback { get; set; }
    public Guid EvaluatedByTeacherId { get; set; }
    [MaxLength(200)] public string EvaluatedByTeacherName { get; set; } = string.Empty;
    public DateTime EvaluatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class CourseReview
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid StudentId { get; set; }
    [MaxLength(200)] public string StudentNameSnapshot { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    [MaxLength(200)] public string CourseNameSnapshot { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    [MaxLength(200)] public string ClassNameSnapshot { get; set; } = string.Empty;
    public decimal CourseRating { get; set; }
    [MaxLength(1000)] public string? CourseComment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Enrollment? Enrollment { get; set; }
    public List<TeacherReview> TeacherReviews { get; set; } = [];
}

public sealed class TeacherReview
{
    public Guid Id { get; set; }
    public Guid CourseReviewId { get; set; }
    public Guid TeacherId { get; set; }
    [MaxLength(200)] public string TeacherNameSnapshot { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    [MaxLength(1000)] public string? Comment { get; set; }
    public CourseReview? CourseReview { get; set; }
}

public sealed class AiKnowledgeDocument
{
    public Guid Id { get; set; }
    [MaxLength(250)] public string Title { get; set; } = string.Empty;
    [MaxLength(40)] public string SourceType { get; set; } = "Text";
    [MaxLength(40)] public string Scope { get; set; } = "Role";
    [MaxLength(30)] public string? AudienceRole { get; set; }
    public Guid? OwnerReferenceId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid UploadedByAccountId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AiKnowledgeChunk> Chunks { get; set; } = [];
}

public sealed class AiKnowledgeChunk
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public AiKnowledgeDocument? Document { get; set; }
}

public sealed class AiEmailDraft
{
    public Guid Id { get; set; }
    [MaxLength(500)] public string Recipients { get; set; } = string.Empty;
    [MaxLength(300)] public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    [MaxLength(30)] public string Status { get; set; } = "Draft";
    public Guid CreatedByAccountId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
}
