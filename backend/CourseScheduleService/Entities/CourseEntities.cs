using CourseScheduleService.Enums;
using System.ComponentModel.DataAnnotations;

namespace CourseScheduleService.Entities;

public sealed class Course
{
    public Guid Id { get; set; }
    [MaxLength(50)] public string Code { get; set; } = string.Empty;
    [MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(220)] public string Slug { get; set; } = string.Empty;
    [MaxLength(500)] public string ShortDescription { get; set; } = string.Empty;
    [MaxLength(4000)] public string Description { get; set; } = string.Empty;
    [MaxLength(100)] public string Category { get; set; } = string.Empty;
    [MaxLength(80)] public string Level { get; set; } = string.Empty;
    public decimal TuitionFee { get; set; }
    public int TotalSessions { get; set; }
    [MaxLength(100)] public string DurationText { get; set; } = string.Empty;
    [MaxLength(600)] public string? ThumbnailUrl { get; set; }
    public CourseStatus Status { get; set; }
    public bool IsBestSeller { get; set; }
    public bool IsPopularThisWeek { get; set; }
    public int ViewCount { get; set; }
    public int EnrolledCount { get; set; }
    public decimal Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Class> Classes { get; set; } = [];
}

public sealed class Class
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    [MaxLength(200)] public string CourseNameSnapshot { get; set; } = string.Empty;
    [MaxLength(80)] public string ClassCode { get; set; } = string.Empty;
    [MaxLength(200)] public string ClassName { get; set; } = string.Empty;
    public Guid TeacherId { get; set; }
    [MaxLength(200)] public string TeacherNameSnapshot { get; set; } = string.Empty;
    public Guid? RoomId { get; set; }
    [MaxLength(100)] public string Room { get; set; } = string.Empty;
    public int MaxStudents { get; set; }
    public int CurrentStudents { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LearningMode LearningMode { get; set; }
    public ClassStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Course? Course { get; set; }
    public Teacher? Teacher { get; set; }
    public Room? RoomRef { get; set; }
    public List<ClassTeacher> ClassTeachers { get; set; } = [];
    public List<Schedule> Schedules { get; set; } = [];
}

public sealed class ClassTeacher
{
    public Guid ClassId { get; set; }
    public Guid TeacherId { get; set; }
    [MaxLength(200)] public string TeacherNameSnapshot { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public Class? Class { get; set; }
    public Teacher? Teacher { get; set; }
}

public sealed class Room
{
    public Guid Id { get; set; }
    [MaxLength(40)] public string Code { get; set; } = string.Empty;
    [MaxLength(100)] public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Class> Classes { get; set; } = [];
}

public sealed class ClassSeatReservation
{
    public Guid EnrollmentId { get; set; }
    public Guid ClassId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class Schedule
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    [MaxLength(200)] public string ClassNameSnapshot { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public StudyShift StudyShift { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public Guid? AssignedTeacherId { get; set; }
    [MaxLength(200)] public string? AssignedTeacherNameSnapshot { get; set; }
    public Guid? SubstituteTeacherId { get; set; }
    [MaxLength(200)] public string? SubstituteTeacherNameSnapshot { get; set; }
    [MaxLength(100)] public string Room { get; set; } = string.Empty;
    [MaxLength(250)] public string Topic { get; set; } = string.Empty;
    public int SessionNumber { get; set; }
    public ScheduleStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Class? Class { get; set; }
    public Teacher? AssignedTeacher { get; set; }
    public Teacher? SubstituteTeacher { get; set; }
    public List<TeachingSubstitutionRequest> SubstituteRequests { get; set; } = [];
}

public sealed class TeachingSubstitutionRequest
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public Guid RequestingTeacherId { get; set; }
    [MaxLength(200)] public string RequestingTeacherNameSnapshot { get; set; } = string.Empty;
    public Guid SubstituteTeacherId { get; set; }
    [MaxLength(200)] public string SubstituteTeacherNameSnapshot { get; set; } = string.Empty;
    [MaxLength(500)] public string? Reason { get; set; }
    public SubstituteRequestStatus Status { get; set; }
    [MaxLength(500)] public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Schedule? Schedule { get; set; }
    public Teacher? RequestingTeacher { get; set; }
    public Teacher? SubstituteTeacher { get; set; }
}

public sealed class Teacher
{
    public Guid Id { get; set; }
    [MaxLength(200)] public string FullName { get; set; } = string.Empty;
    [MaxLength(200)] public string Email { get; set; } = string.Empty;
    [MaxLength(30)] public string Phone { get; set; } = string.Empty;
    [MaxLength(600)] public string? AvatarUrl { get; set; }
    [MaxLength(200)] public string Specialization { get; set; } = string.Empty;
    [MaxLength(1000)] public string Bio { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public decimal Rating { get; set; }
    public TeacherStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<Class> Classes { get; set; } = [];
}
