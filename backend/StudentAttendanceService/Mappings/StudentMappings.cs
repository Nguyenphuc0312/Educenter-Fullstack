using StudentAttendanceService.Dtos;
using StudentAttendanceService.Entities;

namespace StudentAttendanceService.Mappings;

public static class StudentMappings
{
    public static StudentResponse ToResponse(this Student x) => new(x.Id, x.StudentCode, x.FullName, x.Email, x.Phone, x.DateOfBirth, x.Gender, x.Address, x.AvatarUrl, x.Status, x.CreatedAt, x.UpdatedAt);
    public static EnrollmentResponse ToResponse(this Enrollment x) => new(x.Id, x.StudentId, x.StudentNameSnapshot, x.CourseId, x.CourseNameSnapshot, x.ClassId, x.ClassNameSnapshot, x.EnrolledAt, x.Status, x.Note, x.CreatedAt, x.UpdatedAt);
    public static AttendanceSessionResponse ToResponse(this AttendanceSession x) => new(x.Id, x.ClassId, x.ClassNameSnapshot, x.ScheduleId, x.SessionNumber, x.AttendanceDate, x.Topic, x.CreatedByTeacherId, x.CreatedByTeacherName, x.Status, x.CreatedAt, x.UpdatedAt);
    public static AttendanceRecordResponse ToResponse(this AttendanceRecord x) => new(x.Id, x.AttendanceSessionId, x.StudentId, x.StudentNameSnapshot, x.Status, x.Note, x.MarkedAt);
    public static StudentResultResponse ToResponse(this StudentResult x) => new(x.Id, x.StudentId, x.StudentNameSnapshot, x.CourseId, x.CourseNameSnapshot, x.ClassId, x.ClassNameSnapshot, x.MidtermScore, x.FinalScore, x.AverageScore, x.AttendancePercent, x.ResultStatus, x.Feedback, x.EvaluatedByTeacherId, x.EvaluatedByTeacherName, x.EvaluatedAt, x.CreatedAt, x.UpdatedAt);
}
