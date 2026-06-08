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
    public List<Schedule> Schedules { get; set; } = [];
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
    [MaxLength(100)] public string Room { get; set; } = string.Empty;
    [MaxLength(250)] public string Topic { get; set; } = string.Empty;
    public int SessionNumber { get; set; }
    public ScheduleStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Class? Class { get; set; }
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

