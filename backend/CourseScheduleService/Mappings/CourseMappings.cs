using CourseScheduleService.Dtos;
using CourseScheduleService.Entities;

namespace CourseScheduleService.Mappings;

public static class CourseMappings
{
    public static CourseResponse ToResponse(this Course x) => new(x.Id, x.Code, x.Name, x.Slug, x.ShortDescription, x.Description, x.Category, x.Level, x.TuitionFee, x.TotalSessions, x.DurationText, x.ThumbnailUrl, x.Status, x.IsBestSeller, x.IsPopularThisWeek, x.ViewCount, x.EnrolledCount, x.Rating, x.CreatedAt, x.UpdatedAt);
    public static ClassResponse ToResponse(this Class x)
    {
        var teachers = (x.ClassTeachers?.Count > 0 ? x.ClassTeachers.OrderBy(t => t.SortOrder).ToList() : null) ?? [];
        var teacherIds = teachers.Count > 0 ? teachers.Select(t => t.TeacherId).ToList() : new List<Guid> { x.TeacherId };
        var teacherNames = teachers.Count > 0 ? teachers.Select(t => t.TeacherNameSnapshot).ToList() : new List<string> { x.TeacherNameSnapshot };
        return new(x.Id, x.CourseId, x.CourseNameSnapshot, x.ClassCode, x.ClassName, x.TeacherId, x.TeacherNameSnapshot, x.RoomId, x.Room, x.MaxStudents, x.CurrentStudents, x.StartDate, x.EndDate, x.LearningMode, x.Status, x.CreatedAt, x.UpdatedAt, teacherIds, teacherNames);
    }

    public static ScheduleResponse ToResponse(this Schedule x)
    {
        var assignedTeacherId = x.AssignedTeacherId ?? x.Class?.TeacherId;
        var assignedTeacherName = x.AssignedTeacherNameSnapshot ?? x.Class?.TeacherNameSnapshot;
        var effectiveTeacherId = x.SubstituteTeacherId ?? assignedTeacherId;
        var effectiveTeacherName = x.SubstituteTeacherNameSnapshot ?? assignedTeacherName;
        return new(x.Id, x.ClassId, x.ClassNameSnapshot, effectiveTeacherId, effectiveTeacherName, x.DayOfWeek, x.StudyShift, x.StartTime, x.EndTime, x.Room, x.Topic, x.SessionNumber, x.Status, x.CreatedAt, x.UpdatedAt, assignedTeacherId, assignedTeacherName, x.SubstituteTeacherId, x.SubstituteTeacherNameSnapshot, effectiveTeacherId, effectiveTeacherName, x.SubstituteTeacherId.HasValue);
    }
    public static TeacherResponse ToResponse(this Teacher x) => new(x.Id, x.FullName, x.Email, x.Phone, x.AvatarUrl, x.Specialization, x.Bio, x.ExperienceYears, x.Rating, x.Status, x.CreatedAt, x.UpdatedAt);
    public static RoomResponse ToResponse(this Room x) => new(x.Id, x.Code, x.Name, x.Capacity, x.Note, x.IsActive, x.Classes?.Count ?? 0, x.CreatedAt, x.UpdatedAt);
    public static TeachingSubstitutionResponse ToResponse(this TeachingSubstitutionRequest x) => new(x.Id, x.ScheduleId, x.Schedule?.ClassNameSnapshot ?? string.Empty, x.Schedule?.DayOfWeek ?? default, x.Schedule?.StudyShift ?? default, x.Schedule?.StartTime ?? default, x.Schedule?.EndTime ?? default, x.Schedule?.Room ?? string.Empty, x.RequestingTeacherId, x.RequestingTeacherNameSnapshot, x.SubstituteTeacherId, x.SubstituteTeacherNameSnapshot, x.Reason, x.Status, x.AdminNote, x.CreatedAt, x.UpdatedAt);
}
