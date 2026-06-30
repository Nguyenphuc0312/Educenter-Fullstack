using CourseScheduleService.Enums;
using System.ComponentModel.DataAnnotations;

namespace CourseScheduleService.Dtos;

public sealed record CourseResponse(Guid Id, string Code, string Name, string Slug, string ShortDescription, string Description, string Category, string Level, decimal TuitionFee, int TotalSessions, string DurationText, string? ThumbnailUrl, CourseStatus Status, bool IsBestSeller, bool IsPopularThisWeek, int ViewCount, int EnrolledCount, decimal Rating, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record ClassResponse(Guid Id, Guid CourseId, string CourseNameSnapshot, string ClassCode, string ClassName, Guid TeacherId, string TeacherNameSnapshot, Guid? RoomId, string Room, int MaxStudents, int CurrentStudents, DateTime StartDate, DateTime EndDate, LearningMode LearningMode, ClassStatus Status, DateTime CreatedAt, DateTime UpdatedAt, IReadOnlyList<Guid> TeacherIds, IReadOnlyList<string> TeacherNames);
public sealed record ScheduleResponse(Guid Id, Guid ClassId, string ClassNameSnapshot, Guid? TeacherId, string? TeacherNameSnapshot, DayOfWeek DayOfWeek, StudyShift StudyShift, TimeOnly StartTime, TimeOnly EndTime, string Room, string Topic, int SessionNumber, ScheduleStatus Status, DateTime CreatedAt, DateTime UpdatedAt, Guid? AssignedTeacherId, string? AssignedTeacherNameSnapshot, Guid? SubstituteTeacherId, string? SubstituteTeacherNameSnapshot, Guid? EffectiveTeacherId, string? EffectiveTeacherNameSnapshot, bool HasSubstitute);
public sealed record TeacherResponse(Guid Id, string FullName, string Email, string Phone, string? AvatarUrl, string Specialization, string Bio, int ExperienceYears, decimal Rating, TeacherStatus Status, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record RoomResponse(Guid Id, string Code, string Name, int Capacity, string? Note, bool IsActive, int ClassCount, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record RoomScheduleUsageResponse(Guid Id, DayOfWeek DayOfWeek, StudyShift StudyShift, TimeOnly StartTime, TimeOnly EndTime, string Topic, ScheduleStatus Status);
public sealed record RoomClassUsageResponse(Guid ClassId, string ClassCode, string ClassName, string TeacherName, int CurrentStudents, int MaxStudents, DateTime StartDate, DateTime EndDate, ClassStatus Status, IReadOnlyList<RoomScheduleUsageResponse> Schedules);
public sealed record RoomDetailResponse(RoomResponse Room, IReadOnlyList<RoomClassUsageResponse> Classes);
public sealed record TeachingSubstitutionResponse(Guid Id, Guid ScheduleId, string ClassNameSnapshot, DayOfWeek DayOfWeek, StudyShift StudyShift, TimeOnly StartTime, TimeOnly EndTime, string Room, Guid RequestingTeacherId, string RequestingTeacherNameSnapshot, Guid SubstituteTeacherId, string SubstituteTeacherNameSnapshot, string? Reason, SubstituteRequestStatus Status, string? AdminNote, DateTime CreatedAt, DateTime UpdatedAt);

public class CreateCourseRequest
{
    [MaxLength(50)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(220)] public string Slug { get; set; } = string.Empty;
    [MaxLength(500)] public string ShortDescription { get; set; } = string.Empty;
    [MaxLength(4000)] public string Description { get; set; } = string.Empty;
    [Required] public string Category { get; set; } = string.Empty;
    [Required] public string Level { get; set; } = string.Empty;
    [Range(0, double.MaxValue)] public decimal TuitionFee { get; set; }
    [Range(1, 200)] public int TotalSessions { get; set; }
    public string DurationText { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public CourseStatus Status { get; set; } = CourseStatus.Draft;
    public bool IsBestSeller { get; set; }
    public bool IsPopularThisWeek { get; set; }
}

public sealed class UpdateCourseRequest : CreateCourseRequest;

public class CreateClassRequest
{
    [Required] public Guid CourseId { get; set; }
    [MaxLength(80)] public string ClassCode { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string ClassName { get; set; } = string.Empty;
    [Required] public Guid TeacherId { get; set; }
    public List<Guid> TeacherIds { get; set; } = [];
    public Guid? RoomId { get; set; }
    [MaxLength(100)] public string Room { get; set; } = string.Empty;
    [Range(1, 500)] public int MaxStudents { get; set; }
    [Range(0, 500)] public int CurrentStudents { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LearningMode LearningMode { get; set; }
    public ClassStatus Status { get; set; } = ClassStatus.Open;
}

public sealed class UpdateClassRequest : CreateClassRequest;

public class CreateScheduleRequest
{
    [Required] public Guid ClassId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public StudyShift StudyShift { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    [Required] public string Room { get; set; } = string.Empty;
    [Required] public string Topic { get; set; } = string.Empty;
    [Range(1, 300)] public int SessionNumber { get; set; }
    public Guid? AssignedTeacherId { get; set; }
    public ScheduleStatus Status { get; set; } = ScheduleStatus.Scheduled;
}

public sealed class UpdateScheduleRequest : CreateScheduleRequest;

public class CreateRoomRequest
{
    [MaxLength(40)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Range(1, 500)] public int Capacity { get; set; } = 30;
    [MaxLength(500)] public string? Note { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateRoomRequest : CreateRoomRequest;

public sealed class CreateTeachingSubstitutionRequest
{
    [Required] public Guid ScheduleId { get; set; }
    [Required] public Guid RequestingTeacherId { get; set; }
    [Required] public Guid SubstituteTeacherId { get; set; }
    [MaxLength(500)] public string? Reason { get; set; }
}

public sealed class ReviewTeachingSubstitutionRequest
{
    [MaxLength(500)] public string? AdminNote { get; set; }
}

public class CreateTeacherRequest
{
    [Required] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    [Range(0, 60)] public int ExperienceYears { get; set; }
    [Range(0, 5)] public decimal Rating { get; set; }
    public TeacherStatus Status { get; set; } = TeacherStatus.Active;
}

public sealed class UpdateTeacherRequest : CreateTeacherRequest;

public sealed class CourseSearchQuery : PaginationQuery
{
    public string? Keyword { get; init; }
    public string? Category { get; init; }
    public string? Level { get; init; }
    public CourseStatus? Status { get; init; }
}

public sealed record CourseSummaryResponse(int TotalCourses, int OpeningCourses, int TotalClasses, int TotalTeachers, int TotalSchedules);
public sealed record HomeSummaryResponse(IReadOnlyList<CourseResponse> OpeningCourses, IReadOnlyList<CourseResponse> BestSellingCourses, IReadOnlyList<CourseResponse> PopularCourses, CourseSummaryResponse Summary);
