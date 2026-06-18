using CourseScheduleService.Dtos;
using CourseScheduleService.Entities;

namespace CourseScheduleService.Mappings;

public static class CourseMappings
{
    public static CourseResponse ToResponse(this Course x) => new(x.Id, x.Code, x.Name, x.Slug, x.ShortDescription, x.Description, x.Category, x.Level, x.TuitionFee, x.TotalSessions, x.DurationText, x.ThumbnailUrl, x.Status, x.IsBestSeller, x.IsPopularThisWeek, x.ViewCount, x.EnrolledCount, x.Rating, x.CreatedAt, x.UpdatedAt);
    public static ClassResponse ToResponse(this Class x) => new(x.Id, x.CourseId, x.CourseNameSnapshot, x.ClassCode, x.ClassName, x.TeacherId, x.TeacherNameSnapshot, x.Room, x.ClassroomId, x.MinStudents, x.MaxStudents, x.CurrentStudents, x.StartDate, x.EndDate, x.LearningMode, x.Status, x.CreatedAt, x.UpdatedAt);
    public static ScheduleResponse ToResponse(this Schedule x) => new(x.Id, x.ClassId, x.ClassNameSnapshot, x.TeacherId ?? x.Class?.TeacherId, x.TeacherNameSnapshot ?? x.Class?.TeacherNameSnapshot, x.DayOfWeek, x.StudyShift, x.StartTime, x.EndTime, x.Room, x.Topic, x.SessionNumber, x.Status, x.CreatedAt, x.UpdatedAt);
    public static TeacherResponse ToResponse(this Teacher x) => new(x.Id, x.FullName, x.Email, x.Phone, x.AvatarUrl, x.Specialization, x.Bio, x.ExperienceYears, x.Rating, x.Status, x.CreatedAt, x.UpdatedAt);
    public static ClassroomResponse ToResponse(this Classroom x) => new(x.Id, x.Code, x.Name, x.Building, x.Floor, x.Capacity, x.Type, x.Status, x.Description, x.HasProjector, x.HasAirConditioner, x.IsOnline, x.OnlineMeetingUrl, x.CreatedAt, x.UpdatedAt);
    public static ScheduleChangeResponse ToResponse(this ScheduleChangeRequest x) => new(
        x.Id,
        x.ScheduleId,
        x.Type,
        x.OriginalTeacherId,
        x.OriginalTeacherName,
        x.ProposedTeacherId,
        x.ProposedTeacherName,
        x.ProposedDayOfWeek,
        x.ProposedStudyShift,
        x.ProposedRoom,
        x.Status,
        x.Reason,
        x.CreatedAt,
        x.UpdatedAt
    );
}
