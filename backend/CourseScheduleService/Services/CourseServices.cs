using CourseScheduleService.Dtos;
using CourseScheduleService.Entities;
using CourseScheduleService.Enums;
using CourseScheduleService.Mappings;
using CourseScheduleService.Repositories;
using Microsoft.EntityFrameworkCore;
using CourseScheduleService.Data;
using System.Data;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace CourseScheduleService.Services;

public interface ICourseCatalogService
{
    Task<PagedResult<CourseResponse>> SearchAsync(CourseSearchQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyList<CourseResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<CourseResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CourseResponse> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<CourseResponse> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken);
    Task<CourseResponse> UpdateAsync(Guid id, UpdateCourseRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<CourseResponse>> GetOpeningAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<CourseResponse>> GetBestSellingAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<CourseResponse>> GetPopularAsync(CancellationToken cancellationToken);
    Task<CourseSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken);
    Task<HomeSummaryResponse> GetHomeSummaryAsync(CancellationToken cancellationToken);
}

public sealed class CourseCatalogService(IRepository<Course> courses, IRepository<Class> classes, IRepository<Teacher> teachers, IRepository<Schedule> schedules) : ICourseCatalogService
{
    public async Task<IReadOnlyList<CourseResponse>> GetAllAsync(CancellationToken cancellationToken) => await courses.Query().OrderBy(x => x.Name).Select(x => x.ToResponse()).ToListAsync(cancellationToken);

    public async Task<PagedResult<CourseResponse>> SearchAsync(CourseSearchQuery query, CancellationToken cancellationToken)
    {
        var source = courses.Query();
        if (!string.IsNullOrWhiteSpace(query.Keyword)) source = source.Where(x => x.Name.Contains(query.Keyword) || x.Code.Contains(query.Keyword));
        if (!string.IsNullOrWhiteSpace(query.Category)) source = source.Where(x => x.Category == query.Category);
        if (!string.IsNullOrWhiteSpace(query.Level)) source = source.Where(x => x.Level == query.Level);
        if (query.Status.HasValue) source = source.Where(x => x.Status == query.Status);

        var total = await source.CountAsync(cancellationToken);
        var items = await source.OrderBy(x => x.Name).Skip((query.SafePageIndex - 1) * query.SafePageSize).Take(query.SafePageSize).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
        return new PagedResult<CourseResponse> { Items = items, PageIndex = query.SafePageIndex, PageSize = query.SafePageSize, TotalItems = total };
    }

    public async Task<CourseResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => (await courses.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Course")).ToResponse();
    public async Task<CourseResponse> GetBySlugAsync(string slug, CancellationToken cancellationToken) => (await courses.Query().FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken) ?? throw NotFound("Course")).ToResponse();

    public async Task<CourseResponse> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var code = string.IsNullOrWhiteSpace(request.Code) ? await GenerateCourseCodeAsync(request.Category, cancellationToken) : request.Code.Trim().ToUpperInvariant();
        var slug = string.IsNullOrWhiteSpace(request.Slug) ? Slugify(request.Name) : request.Slug.Trim();
        if (await courses.AnyAsync(x => x.Code == code || x.Slug == slug, cancellationToken)) throw Conflict("Course code or slug already exists");
        var now = DateTime.UtcNow;
        var entity = new Course { Id = Guid.NewGuid(), Code = code, Name = request.Name.Trim(), Slug = slug, ShortDescription = request.ShortDescription, Description = request.Description, Category = request.Category, Level = request.Level, TuitionFee = request.TuitionFee, TotalSessions = request.TotalSessions, DurationText = request.DurationText, ThumbnailUrl = request.ThumbnailUrl, Status = request.Status, IsBestSeller = request.IsBestSeller, IsPopularThisWeek = request.IsPopularThisWeek, Rating = 0, CreatedAt = now, UpdatedAt = now };
        await courses.AddAsync(entity, cancellationToken);
        await courses.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task<CourseResponse> UpdateAsync(Guid id, UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        var entity = await courses.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Course");
        if (await courses.Query().AnyAsync(x => x.Id != id && (x.Code == request.Code || x.Slug == request.Slug), cancellationToken)) throw Conflict("Course code or slug already exists");
        entity.Code = request.Code.Trim(); entity.Name = request.Name.Trim(); entity.Slug = request.Slug.Trim(); entity.ShortDescription = request.ShortDescription; entity.Description = request.Description; entity.Category = request.Category; entity.Level = request.Level; entity.TuitionFee = request.TuitionFee; entity.TotalSessions = request.TotalSessions; entity.DurationText = request.DurationText; entity.ThumbnailUrl = request.ThumbnailUrl; entity.Status = request.Status; entity.IsBestSeller = request.IsBestSeller; entity.IsPopularThisWeek = request.IsPopularThisWeek; entity.UpdatedAt = DateTime.UtcNow;
        courses.Update(entity);
        await courses.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await courses.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Course");
        courses.Remove(entity);
        await courses.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseResponse>> GetOpeningAsync(CancellationToken cancellationToken) => await courses.Query().Where(x => x.Status == CourseStatus.Opening).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<CourseResponse>> GetBestSellingAsync(CancellationToken cancellationToken) => await courses.Query().Where(x => x.IsBestSeller).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<CourseResponse>> GetPopularAsync(CancellationToken cancellationToken) => await courses.Query().Where(x => x.IsPopularThisWeek).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<CourseSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken) => new(await courses.Query().CountAsync(cancellationToken), await courses.Query().CountAsync(x => x.Status == CourseStatus.Opening, cancellationToken), await classes.Query().CountAsync(cancellationToken), await teachers.Query().CountAsync(cancellationToken), await schedules.Query().CountAsync(cancellationToken));
    public async Task<HomeSummaryResponse> GetHomeSummaryAsync(CancellationToken cancellationToken) => new(await GetOpeningAsync(cancellationToken), await GetBestSellingAsync(cancellationToken), await GetPopularAsync(cancellationToken), await GetSummaryAsync(cancellationToken));

    private static AppException NotFound(string name) => new($"{name} not found", StatusCodes.Status404NotFound);
    private static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
    private async Task<string> GenerateCourseCodeAsync(string category, CancellationToken cancellationToken)
    {
        var prefix = category.Trim().ToLowerInvariant() switch
        {
            "frontend" => "FE", "backend" => "BE", "ui/ux" or "uiux" or "design" => "UI",
            "database" => "DB", "mobile" => "MB", "devops" => "DO", "fullstack" => "FS",
            "security" => "SE", _ => "CRS"
        };
        var stem = $"{prefix}{DateTime.UtcNow:yyyy}";
        var codes = await courses.Query().Where(x => x.Code.StartsWith(stem)).Select(x => x.Code).ToListAsync(cancellationToken);
        var next = codes.Select(x => int.TryParse(x[stem.Length..], out var value) ? value : 0).DefaultIfEmpty().Max() + 1;
        return $"{stem}{next:00}";
    }
    private static string Slugify(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            builder.Append(char.IsLetterOrDigit(ch) ? ch : '-');
        }
        return string.Join('-', builder.ToString().Split('-', StringSplitOptions.RemoveEmptyEntries));
    }
}

public interface IClassManagementService
{
    Task<IReadOnlyList<ClassResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<ClassResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClassResponse>> ByCourseAsync(Guid courseId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClassResponse>> ByTeacherAsync(Guid teacherId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClassResponse>> OpeningAsync(CancellationToken cancellationToken);
    Task<ClassResponse> CreateAsync(CreateClassRequest request, CancellationToken cancellationToken);
    Task<ClassResponse> UpdateAsync(Guid id, UpdateClassRequest request, CancellationToken cancellationToken);
    Task<ClassResponse> IncreaseAsync(Guid id, CancellationToken cancellationToken);
    Task<ClassResponse> ReserveSeatAsync(Guid id, Guid enrollmentId, CancellationToken cancellationToken);
    Task<ClassResponse> ReleaseSeatAsync(Guid id, Guid enrollmentId, CancellationToken cancellationToken);
    Task<ClassResponse> DecreaseAsync(Guid id, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class ClassManagementService(IRepository<Class> classes, IRepository<Course> courses, IRepository<Teacher> teachers, CourseDbContext db) : IClassManagementService
{
    public async Task<IReadOnlyList<ClassResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var rows = await classes.Query().Include(x => x.ClassTeachers).OrderBy(x => x.ClassCode).ToListAsync(cancellationToken);
        await NormalizeClassesAsync(rows, cancellationToken);
        return rows.Select(x => x.ToResponse()).ToList();
    }

    public async Task<ClassResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await classes.Query().Include(x => x.ClassTeachers).FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw NotFound("Class");
        if (ApplyLifecycleStatus(entity, DateTime.UtcNow)) await classes.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task<IReadOnlyList<ClassResponse>> ByCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var rows = await classes.Query().Include(x => x.ClassTeachers).Where(x => x.CourseId == courseId).ToListAsync(cancellationToken);
        await NormalizeClassesAsync(rows, cancellationToken);
        return rows.Select(x => x.ToResponse()).ToList();
    }

    public async Task<IReadOnlyList<ClassResponse>> ByTeacherAsync(Guid teacherId, CancellationToken cancellationToken)
    {
        var rows = await classes.Query().Include(x => x.ClassTeachers).Where(x => x.TeacherId == teacherId || x.ClassTeachers.Any(t => t.TeacherId == teacherId)).ToListAsync(cancellationToken);
        await NormalizeClassesAsync(rows, cancellationToken);
        return rows.Select(x => x.ToResponse()).ToList();
    }

    public async Task<IReadOnlyList<ClassResponse>> OpeningAsync(CancellationToken cancellationToken)
    {
        var rows = await classes.Query().Include(x => x.ClassTeachers).ToListAsync(cancellationToken);
        await NormalizeClassesAsync(rows, cancellationToken);
        return rows.Where(x => x.Status == ClassStatus.Open).Select(x => x.ToResponse()).ToList();
    }

    public async Task<ClassResponse> CreateAsync(CreateClassRequest request, CancellationToken cancellationToken)
    {
        var course = await courses.GetByIdAsync(request.CourseId, cancellationToken) ?? throw NotFound("Course");
        var classTeachers = await ResolveClassTeachersAsync(request.TeacherIds, request.TeacherId, cancellationToken);
        var teacher = classTeachers[0];
        ValidateCapacity(request.MaxStudents, request.CurrentStudents);
        var room = await ResolveRoomAsync(request.RoomId, request.Room, cancellationToken);
        ValidateRoomCapacity(request.MaxStudents, request.CurrentStudents, room.Capacity);
        var classCode = string.IsNullOrWhiteSpace(request.ClassCode) ? await GenerateClassCodeAsync(cancellationToken) : request.ClassCode.Trim().ToUpperInvariant();
        if (await classes.AnyAsync(x => x.ClassCode == classCode, cancellationToken)) throw Conflict("Class code already exists");
        var now = DateTime.UtcNow;
        var entity = new Class { Id = Guid.NewGuid(), CourseId = course.Id, CourseNameSnapshot = course.Name, ClassCode = classCode, ClassName = request.ClassName, TeacherId = teacher.Id, TeacherNameSnapshot = teacher.FullName, RoomId = room.Id, Room = room.Name, MaxStudents = request.MaxStudents, CurrentStudents = request.CurrentStudents, StartDate = request.StartDate, EndDate = request.EndDate, LearningMode = request.LearningMode, Status = request.Status, CreatedAt = now, UpdatedAt = now };
        ApplyLifecycleStatus(entity, now);
        await classes.AddAsync(entity, cancellationToken);
        await classes.SaveChangesAsync(cancellationToken);
        await SyncClassTeachersAsync(entity.Id, classTeachers, cancellationToken);
        entity.ClassTeachers = ToClassTeacherSnapshots(entity.Id, classTeachers);
        return entity.ToResponse();
    }

    public async Task<ClassResponse> UpdateAsync(Guid id, UpdateClassRequest request, CancellationToken cancellationToken)
    {
        var entity = await classes.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Class");
        var course = await courses.GetByIdAsync(request.CourseId, cancellationToken) ?? throw NotFound("Course");
        var classTeachers = await ResolveClassTeachersAsync(request.TeacherIds, request.TeacherId, cancellationToken);
        var teacher = classTeachers[0];
        ValidateCapacity(request.MaxStudents, request.CurrentStudents);
        var room = await ResolveRoomAsync(request.RoomId, request.Room, cancellationToken);
        ValidateRoomCapacity(request.MaxStudents, request.CurrentStudents, room.Capacity);
        await ValidateClassRoomSchedulesAsync(entity.Id, room.Name, cancellationToken);
        entity.CourseId = course.Id; entity.CourseNameSnapshot = course.Name; entity.ClassCode = request.ClassCode; entity.ClassName = request.ClassName; entity.TeacherId = teacher.Id; entity.TeacherNameSnapshot = teacher.FullName; entity.RoomId = room.Id; entity.Room = room.Name; entity.MaxStudents = request.MaxStudents; entity.CurrentStudents = request.CurrentStudents; entity.StartDate = request.StartDate; entity.EndDate = request.EndDate; entity.LearningMode = request.LearningMode; entity.Status = request.Status; entity.UpdatedAt = DateTime.UtcNow;
        ApplyLifecycleStatus(entity, DateTime.UtcNow);
        classes.Update(entity);
        var ownSchedules = await db.Schedules.Where(x => x.ClassId == entity.Id).ToListAsync(cancellationToken);
        foreach (var schedule in ownSchedules)
        {
            schedule.Room = room.Name;
            schedule.ClassNameSnapshot = entity.ClassName;
            ApplyScheduleTeacher(schedule, classTeachers);
            schedule.UpdatedAt = DateTime.UtcNow;
        }
        await classes.SaveChangesAsync(cancellationToken);
        await SyncClassTeachersAsync(entity.Id, classTeachers, cancellationToken);
        entity.ClassTeachers = ToClassTeacherSnapshots(entity.Id, classTeachers);
        return entity.ToResponse();
    }

    public async Task<ClassResponse> IncreaseAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await classes.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Class");
        if (entity.CurrentStudents >= entity.MaxStudents) throw Conflict("Class is full");
        entity.CurrentStudents++;
        entity.UpdatedAt = DateTime.UtcNow;
        ApplyLifecycleStatus(entity, DateTime.UtcNow);
        await classes.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task<ClassResponse> ReserveSeatAsync(Guid id, Guid enrollmentId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var existing = await db.ClassSeatReservations.FirstOrDefaultAsync(x => x.EnrollmentId == enrollmentId, cancellationToken);
        if (existing is not null)
        {
            if (existing.ClassId != id) throw Conflict("Enrollment is already reserved in another class");
            var reservedClass = await db.Classes.FindAsync([id], cancellationToken) ?? throw NotFound("Class");
            if (ApplyLifecycleStatus(reservedClass, DateTime.UtcNow)) await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return reservedClass.ToResponse();
        }

        var entity = await db.Classes.FindAsync([id], cancellationToken) ?? throw NotFound("Class");
        if (entity.CurrentStudents >= entity.MaxStudents) throw Conflict("Class is full");
        entity.CurrentStudents++;
        entity.UpdatedAt = DateTime.UtcNow;
        ApplyLifecycleStatus(entity, DateTime.UtcNow);
        db.ClassSeatReservations.Add(new ClassSeatReservation { EnrollmentId = enrollmentId, ClassId = id, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task<ClassResponse> ReleaseSeatAsync(Guid id, Guid enrollmentId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var entity = await db.Classes.FindAsync([id], cancellationToken) ?? throw NotFound("Class");
        var reservation = await db.ClassSeatReservations.FirstOrDefaultAsync(x => x.EnrollmentId == enrollmentId, cancellationToken);
        if (reservation is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return entity.ToResponse();
        }
        if (reservation.ClassId != id) throw Conflict("Enrollment reservation belongs to another class");

        db.ClassSeatReservations.Remove(reservation);
        entity.CurrentStudents = Math.Max(0, entity.CurrentStudents - 1);
        entity.UpdatedAt = DateTime.UtcNow;
        ApplyLifecycleStatus(entity, DateTime.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task<ClassResponse> DecreaseAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await classes.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Class");
        entity.CurrentStudents = Math.Max(0, entity.CurrentStudents - 1);
        entity.UpdatedAt = DateTime.UtcNow;
        ApplyLifecycleStatus(entity, DateTime.UtcNow);
        await classes.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await classes.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Class");
        classes.Remove(entity);
        await classes.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateCapacity(int max, int current)
    {
        if (max <= 0) throw new AppException("MaxStudents must be greater than zero");
        if (current > max) throw Conflict("CurrentStudents cannot exceed MaxStudents");
    }

    private async Task<List<Teacher>> ResolveClassTeachersAsync(IReadOnlyList<Guid> requestedTeacherIds, Guid fallbackTeacherId, CancellationToken cancellationToken)
    {
        var ids = requestedTeacherIds.Where(x => x != Guid.Empty).Distinct().ToList();
        if (ids.Count == 0 && fallbackTeacherId != Guid.Empty) ids.Add(fallbackTeacherId);
        if (ids.Count == 0) throw new AppException("At least one teacher is required");

        var rows = await teachers.Query().Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
        if (rows.Count != ids.Count) throw NotFound("Teacher");

        return ids.Select(id => rows.First(x => x.Id == id)).ToList();
    }

    private async Task SyncClassTeachersAsync(Guid classId, IReadOnlyList<Teacher> selectedTeachers, CancellationToken cancellationToken)
    {
        var existing = await db.ClassTeachers.Where(x => x.ClassId == classId).ToListAsync(cancellationToken);
        db.ClassTeachers.RemoveRange(existing);
        var now = DateTime.UtcNow;
        for (var i = 0; i < selectedTeachers.Count; i++)
        {
            db.ClassTeachers.Add(new ClassTeacher
            {
                ClassId = classId,
                TeacherId = selectedTeachers[i].Id,
                TeacherNameSnapshot = selectedTeachers[i].FullName,
                IsPrimary = i == 0,
                SortOrder = i,
                CreatedAt = now
            });
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private static List<ClassTeacher> ToClassTeacherSnapshots(Guid classId, IReadOnlyList<Teacher> selectedTeachers)
    {
        var now = DateTime.UtcNow;
        return selectedTeachers.Select((teacher, index) => new ClassTeacher
        {
            ClassId = classId,
            TeacherId = teacher.Id,
            TeacherNameSnapshot = teacher.FullName,
            IsPrimary = index == 0,
            SortOrder = index,
            CreatedAt = now
        }).ToList();
    }

    private static void ApplyScheduleTeacher(Schedule schedule, IReadOnlyList<Teacher> classTeachers)
    {
        if (classTeachers.Count == 0) return;
        var index = Math.Max(0, schedule.SessionNumber - 1) % classTeachers.Count;
        var teacher = classTeachers[index];
        schedule.AssignedTeacherId = teacher.Id;
        schedule.AssignedTeacherNameSnapshot = teacher.FullName;
        if (schedule.SubstituteTeacherId.HasValue && classTeachers.All(x => x.Id != schedule.SubstituteTeacherId.Value))
        {
            schedule.SubstituteTeacherId = null;
            schedule.SubstituteTeacherNameSnapshot = null;
        }
    }

    private static void ValidateRoomCapacity(int max, int current, int roomCapacity)
    {
        if (max > roomCapacity) throw Conflict("Sĩ số tối đa của lớp không được vượt quá sức chứa phòng học");
        if (current > roomCapacity) throw Conflict("Sĩ số hiện tại không được vượt quá sức chứa phòng học");
    }

    private async Task NormalizeClassesAsync(IReadOnlyList<Class> rows, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var changed = false;
        foreach (var row in rows)
        {
            changed = ApplyLifecycleStatus(row, now) || changed;
        }
        if (changed) await classes.SaveChangesAsync(cancellationToken);
    }

    private static bool ApplyLifecycleStatus(Class entity, DateTime now)
    {
        if (entity.Status == ClassStatus.Cancelled) return false;
        var next = entity.EndDate < now ? ClassStatus.Completed
            : entity.StartDate <= now ? ClassStatus.InProgress
            : entity.CurrentStudents >= entity.MaxStudents ? ClassStatus.Full
            : ClassStatus.Open;
        if (entity.Status == next) return false;
        entity.Status = next;
        entity.UpdatedAt = now;
        return true;
    }

    private async Task<string> GenerateClassCodeAsync(CancellationToken cancellationToken)
    {
        var stem = $"CLS{DateTime.UtcNow:yyyy}";
        var codes = await classes.Query().Where(x => x.ClassCode.StartsWith(stem)).Select(x => x.ClassCode).ToListAsync(cancellationToken);
        var next = codes.Select(x => int.TryParse(x[stem.Length..], out var value) ? value : 0).DefaultIfEmpty().Max() + 1;
        return $"{stem}{next:00}";
    }

    private async Task ValidateClassRoomSchedulesAsync(Guid classId, string roomName, CancellationToken cancellationToken)
    {
        var ownSchedules = await db.Schedules.Where(x => x.ClassId == classId).ToListAsync(cancellationToken);
        foreach (var schedule in ownSchedules)
        {
            var hasConflict = await db.Schedules.AnyAsync(x =>
                x.Id != schedule.Id
                && x.ClassId != classId
                && x.Room == roomName
                && x.DayOfWeek == schedule.DayOfWeek
                && schedule.StartTime < x.EndTime
                && schedule.EndTime > x.StartTime,
                cancellationToken);
            if (hasConflict) throw Conflict("Phòng học đã có lớp khác trong cùng ngày và khung giờ");
        }
    }

    private async Task<(Guid? Id, string Name, int Capacity)> ResolveRoomAsync(Guid? roomId, string roomText, CancellationToken cancellationToken)
    {
        if (roomId.HasValue)
        {
            var room = await db.Rooms.FirstOrDefaultAsync(x => x.Id == roomId.Value, cancellationToken) ?? throw NotFound("Room");
            if (!room.IsActive) throw Conflict("Room is inactive");
            return (room.Id, room.Name, room.Capacity);
        }

        var normalized = roomText.Trim();
        if (string.IsNullOrWhiteSpace(normalized)) throw new AppException("Room is required");

        var matched = await db.Rooms.FirstOrDefaultAsync(x => x.Name == normalized || x.Code == normalized, cancellationToken);
        return matched is null ? (null, normalized, int.MaxValue) : (matched.Id, matched.Name, matched.Capacity);
    }

    private static AppException NotFound(string name) => new($"{name} not found", StatusCodes.Status404NotFound);
    private static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
}

public interface IRoomService
{
    Task<IReadOnlyList<RoomResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<RoomResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<RoomDetailResponse> GetUsageAsync(Guid id, CancellationToken cancellationToken);
    Task<RoomResponse> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken);
    Task<RoomResponse> UpdateAsync(Guid id, UpdateRoomRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class RoomService(IRepository<Room> rooms, IRepository<Class> classes, CourseDbContext db) : IRoomService
{
    public async Task<IReadOnlyList<RoomResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var rows = await rooms.Query().Include(x => x.Classes).OrderBy(x => x.Code).ToListAsync(cancellationToken);
        return rows.Select(x => x.ToResponse()).ToList();
    }

    public async Task<RoomResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        (await rooms.Query().Include(x => x.Classes).FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw NotFound("Room")).ToResponse();

    public async Task<RoomDetailResponse> GetUsageAsync(Guid id, CancellationToken cancellationToken)
    {
        var room = await rooms.Query().Include(x => x.Classes).FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw NotFound("Room");
        var classRows = await db.Classes
            .Include(x => x.Schedules)
            .Where(x => x.RoomId == id)
            .OrderBy(x => x.StartDate)
            .ThenBy(x => x.ClassName)
            .ToListAsync(cancellationToken);

        var usage = classRows.Select(x => new RoomClassUsageResponse(
            x.Id,
            x.ClassCode,
            x.ClassName,
            x.TeacherNameSnapshot,
            x.CurrentStudents,
            x.MaxStudents,
            x.StartDate,
            x.EndDate,
            x.Status,
            x.Schedules
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .Select(s => new RoomScheduleUsageResponse(s.Id, s.DayOfWeek, s.StudyShift, s.StartTime, s.EndTime, s.Topic, s.Status))
                .ToList()
        )).ToList();

        return new RoomDetailResponse(room.ToResponse(), usage);
    }

    public async Task<RoomResponse> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken)
    {
        var code = string.IsNullOrWhiteSpace(request.Code)
            ? await GenerateRoomCodeAsync(cancellationToken)
            : NormalizeRoomCode(request.Code, request.Name);
        var name = request.Name.Trim();
        if (await rooms.AnyAsync(x => x.Code == code || x.Name == name, cancellationToken)) throw Conflict("Room code or name already exists");
        var now = DateTime.UtcNow;
        var entity = new Room { Id = Guid.NewGuid(), Code = code, Name = name, Capacity = request.Capacity, Note = request.Note, IsActive = request.IsActive, CreatedAt = now, UpdatedAt = now };
        await rooms.AddAsync(entity, cancellationToken);
        await rooms.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task<RoomResponse> UpdateAsync(Guid id, UpdateRoomRequest request, CancellationToken cancellationToken)
    {
        var entity = await rooms.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Room");
        var code = string.IsNullOrWhiteSpace(request.Code) ? entity.Code : NormalizeRoomCode(request.Code, request.Name);
        var name = request.Name.Trim();
        if (await rooms.AnyAsync(x => x.Id != id && (x.Code == code || x.Name == name), cancellationToken)) throw Conflict("Room code or name already exists");
        var largestLinkedClass = await classes.Query()
            .Where(x => x.RoomId == id)
            .Select(x => (int?)x.MaxStudents)
            .MaxAsync(cancellationToken);
        if (largestLinkedClass.HasValue && request.Capacity < largestLinkedClass.Value)
        {
            throw Conflict("Sức chứa phòng không được nhỏ hơn sĩ số tối đa của lớp đang sử dụng phòng");
        }
        entity.Code = code;
        entity.Name = name;
        entity.Capacity = request.Capacity;
        entity.Note = request.Note;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await rooms.SaveChangesAsync(cancellationToken);

        var linkedClasses = await classes.Query().Where(x => x.RoomId == id).ToListAsync(cancellationToken);
        if (linkedClasses.Count > 0)
        {
            foreach (var item in linkedClasses)
            {
                item.Room = entity.Name;
                item.UpdatedAt = DateTime.UtcNow;
            }
            await classes.SaveChangesAsync(cancellationToken);
        }

        return entity.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await rooms.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Room");
        if (await classes.AnyAsync(x => x.RoomId == id, cancellationToken)) throw Conflict("Room is being used by classes");
        rooms.Remove(entity);
        await rooms.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeRoomCode(string code, string name)
    {
        var source = string.IsNullOrWhiteSpace(code) ? name : code;
        var normalized = source.Trim().ToUpperInvariant().Replace(" ", "");
        return normalized.Length > 40 ? normalized[..40] : normalized;
    }

    private async Task<string> GenerateRoomCodeAsync(CancellationToken cancellationToken)
    {
        const string stem = "ROOM";
        var codes = await rooms.Query()
            .Where(x => x.Code.StartsWith(stem))
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);
        var next = codes
            .Select(x => int.TryParse(x[stem.Length..], out var value) ? value : 0)
            .DefaultIfEmpty()
            .Max() + 1;
        string code;
        do
        {
            code = $"{stem}{next:000}";
            next++;
        } while (await rooms.AnyAsync(x => x.Code == code, cancellationToken));

        return code;
    }

    private static AppException NotFound(string name) => new($"{name} not found", StatusCodes.Status404NotFound);
    private static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
}

public interface IScheduleManagementService
{
    Task<IReadOnlyList<ScheduleResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<ScheduleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ScheduleResponse>> ByClassAsync(Guid classId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ScheduleResponse>> ByTeacherAsync(Guid teacherId, CancellationToken cancellationToken);
    Task<ScheduleResponse> CreateAsync(CreateScheduleRequest request, CancellationToken cancellationToken);
    Task<ScheduleResponse> UpdateAsync(Guid id, UpdateScheduleRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class ScheduleManagementService(IRepository<Schedule> schedules, IRepository<Class> classes, IRepository<Teacher> teachers) : IScheduleManagementService
{
    public async Task<IReadOnlyList<ScheduleResponse>> GetAllAsync(CancellationToken cancellationToken) => await schedules.Query().Include(x => x.Class).OrderBy(x => x.SessionNumber).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<ScheduleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => (await schedules.Query().Include(x => x.Class).FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw NotFound("Schedule")).ToResponse();
    public async Task<IReadOnlyList<ScheduleResponse>> ByClassAsync(Guid classId, CancellationToken cancellationToken) => await schedules.Query().Include(x => x.Class).Where(x => x.ClassId == classId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<ScheduleResponse>> ByTeacherAsync(Guid teacherId, CancellationToken cancellationToken) => await schedules.Query().Include(x => x.Class).Where(x => x.AssignedTeacherId == teacherId || x.SubstituteTeacherId == teacherId || (x.AssignedTeacherId == null && x.Class != null && x.Class.TeacherId == teacherId)).Select(x => x.ToResponse()).ToListAsync(cancellationToken);

    public async Task<ScheduleResponse> CreateAsync(CreateScheduleRequest request, CancellationToken cancellationToken)
    {
        var classEntity = await classes.Query().Include(x => x.ClassTeachers).FirstOrDefaultAsync(x => x.Id == request.ClassId, cancellationToken) ?? throw NotFound("Class");
        await ValidateNoConflict(Guid.Empty, request, classEntity.Room, cancellationToken);
        var assignedTeacher = await ResolveScheduleTeacherAsync(classEntity, request.AssignedTeacherId, request.SessionNumber, cancellationToken);
        var now = DateTime.UtcNow;
        var entity = new Schedule { Id = Guid.NewGuid(), ClassId = classEntity.Id, ClassNameSnapshot = classEntity.ClassName, Class = classEntity, DayOfWeek = request.DayOfWeek, StudyShift = request.StudyShift, StartTime = request.StartTime, EndTime = request.EndTime, AssignedTeacherId = assignedTeacher.Id, AssignedTeacherNameSnapshot = assignedTeacher.FullName, Room = classEntity.Room, Topic = request.Topic, SessionNumber = request.SessionNumber, Status = request.Status, CreatedAt = now, UpdatedAt = now };
        await schedules.AddAsync(entity, cancellationToken);
        await schedules.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task<ScheduleResponse> UpdateAsync(Guid id, UpdateScheduleRequest request, CancellationToken cancellationToken)
    {
        var entity = await schedules.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Schedule");
        var classEntity = await classes.Query().Include(x => x.ClassTeachers).FirstOrDefaultAsync(x => x.Id == request.ClassId, cancellationToken) ?? throw NotFound("Class");
        await ValidateNoConflict(id, request, classEntity.Room, cancellationToken);
        var assignedTeacher = await ResolveScheduleTeacherAsync(classEntity, request.AssignedTeacherId, request.SessionNumber, cancellationToken);
        entity.ClassId = classEntity.Id; entity.ClassNameSnapshot = classEntity.ClassName; entity.Class = classEntity; entity.DayOfWeek = request.DayOfWeek; entity.StudyShift = request.StudyShift; entity.StartTime = request.StartTime; entity.EndTime = request.EndTime; entity.AssignedTeacherId = assignedTeacher.Id; entity.AssignedTeacherNameSnapshot = assignedTeacher.FullName; entity.Room = classEntity.Room; entity.Topic = request.Topic; entity.SessionNumber = request.SessionNumber; entity.Status = request.Status; entity.UpdatedAt = DateTime.UtcNow;
        await schedules.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await schedules.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Schedule");
        schedules.Remove(entity);
        await schedules.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateNoConflict(Guid id, CreateScheduleRequest request, string roomName, CancellationToken cancellationToken)
    {
        if (request.EndTime <= request.StartTime) throw new AppException("Giờ kết thúc phải sau giờ bắt đầu");
        if (await schedules.Query().AnyAsync(x =>
            x.Id != id
            && x.ClassId == request.ClassId
            && x.DayOfWeek == request.DayOfWeek
            && request.StartTime < x.EndTime
            && request.EndTime > x.StartTime,
            cancellationToken)) throw Conflict("Lớp học đã có lịch trong khung giờ này");
        if (await schedules.Query().AnyAsync(x =>
            x.Id != id
            && x.Room == roomName
            && x.DayOfWeek == request.DayOfWeek
            && request.StartTime < x.EndTime
            && request.EndTime > x.StartTime,
            cancellationToken)) throw Conflict("Phòng học đã có lớp khác trong cùng ngày và khung giờ");
    }

    private async Task<Teacher> ResolveScheduleTeacherAsync(Class classEntity, Guid? assignedTeacherId, int sessionNumber, CancellationToken cancellationToken)
    {
        var classTeacherIds = classEntity.ClassTeachers.OrderBy(x => x.SortOrder).Select(x => x.TeacherId).ToList();
        if (classTeacherIds.Count == 0) classTeacherIds.Add(classEntity.TeacherId);

        var teacherId = assignedTeacherId ?? classTeacherIds[Math.Max(0, sessionNumber - 1) % classTeacherIds.Count];
        if (!classTeacherIds.Contains(teacherId))
        {
            throw Conflict("Assigned teacher must belong to this class");
        }

        return await teachers.GetByIdAsync(teacherId, cancellationToken) ?? throw NotFound("Teacher");
    }
    private static AppException NotFound(string name) => new($"{name} not found", StatusCodes.Status404NotFound);
    private static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
}

public interface ITeachingSubstitutionService
{
    Task<IReadOnlyList<TeachingSubstitutionResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<TeachingSubstitutionResponse>> GetPendingAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<TeachingSubstitutionResponse>> ByTeacherAsync(Guid teacherId, CancellationToken cancellationToken);
    Task<TeachingSubstitutionResponse> CreateAsync(CreateTeachingSubstitutionRequest request, CancellationToken cancellationToken);
    Task<TeachingSubstitutionResponse> ApproveAsync(Guid id, ReviewTeachingSubstitutionRequest request, string? bearerToken, CancellationToken cancellationToken);
    Task<TeachingSubstitutionResponse> RejectAsync(Guid id, ReviewTeachingSubstitutionRequest request, string? bearerToken, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface INotificationClient
{
    Task SendEmailAsync(string toEmail, string subject, string body, string? bearerToken, CancellationToken cancellationToken);
}

public sealed class NotificationClient(HttpClient httpClient) : INotificationClient
{
    private sealed record SendNotificationRequest(string ToEmail, string Subject, string Body);

    public async Task SendEmailAsync(string toEmail, string subject, string body, string? bearerToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/notifications/email");
        if (!string.IsNullOrWhiteSpace(bearerToken)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        request.Content = JsonContent.Create(new SendNotificationRequest(toEmail, subject, body));
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode) return;
        var detail = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new AppException($"Cannot send notification. Payment service returned {(int)response.StatusCode}: {detail}", StatusCodes.Status502BadGateway);
    }
}

public sealed class TeachingSubstitutionService(CourseDbContext db, IRepository<Teacher> teachers, INotificationClient notifications) : ITeachingSubstitutionService
{
    public async Task<IReadOnlyList<TeachingSubstitutionResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.TeachingSubstitutionRequests.Include(x => x.Schedule).OrderByDescending(x => x.CreatedAt).Select(x => x.ToResponse()).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TeachingSubstitutionResponse>> GetPendingAsync(CancellationToken cancellationToken) =>
        await db.TeachingSubstitutionRequests.Include(x => x.Schedule).Where(x => x.Status == SubstituteRequestStatus.Pending).OrderBy(x => x.CreatedAt).Select(x => x.ToResponse()).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TeachingSubstitutionResponse>> ByTeacherAsync(Guid teacherId, CancellationToken cancellationToken) =>
        await db.TeachingSubstitutionRequests.Include(x => x.Schedule).Where(x => x.RequestingTeacherId == teacherId || x.SubstituteTeacherId == teacherId).OrderByDescending(x => x.CreatedAt).Select(x => x.ToResponse()).ToListAsync(cancellationToken);

    public async Task<TeachingSubstitutionResponse> CreateAsync(CreateTeachingSubstitutionRequest request, CancellationToken cancellationToken)
    {
        var schedule = await db.Schedules.Include(x => x.Class).FirstOrDefaultAsync(x => x.Id == request.ScheduleId, cancellationToken) ?? throw NotFound("Schedule");
        var requestingTeacher = await teachers.GetByIdAsync(request.RequestingTeacherId, cancellationToken) ?? throw NotFound("Requesting teacher");
        var substituteTeacher = await teachers.GetByIdAsync(request.SubstituteTeacherId, cancellationToken) ?? throw NotFound("Substitute teacher");
        if (request.RequestingTeacherId == request.SubstituteTeacherId) throw Conflict("Substitute teacher must be different");
        if (await db.TeachingSubstitutionRequests.AnyAsync(x => x.ScheduleId == request.ScheduleId && x.Status == SubstituteRequestStatus.Pending, cancellationToken)) throw Conflict("This schedule already has a pending substitution request");
        if (schedule.SubstituteTeacherId.HasValue) throw Conflict("This schedule already has an approved substitute teacher");

        var now = DateTime.UtcNow;
        var entity = new TeachingSubstitutionRequest
        {
            Id = Guid.NewGuid(),
            ScheduleId = schedule.Id,
            Schedule = schedule,
            RequestingTeacherId = requestingTeacher.Id,
            RequestingTeacherNameSnapshot = requestingTeacher.FullName,
            SubstituteTeacherId = substituteTeacher.Id,
            SubstituteTeacherNameSnapshot = substituteTeacher.FullName,
            Reason = request.Reason,
            Status = SubstituteRequestStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.TeachingSubstitutionRequests.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task<TeachingSubstitutionResponse> ApproveAsync(Guid id, ReviewTeachingSubstitutionRequest request, string? bearerToken, CancellationToken cancellationToken)
    {
        var entity = await db.TeachingSubstitutionRequests.Include(x => x.Schedule).FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw NotFound("Substitution request");
        if (entity.Status != SubstituteRequestStatus.Pending) throw Conflict("Only pending requests can be approved");
        entity.Status = SubstituteRequestStatus.Approved;
        entity.AdminNote = request.AdminNote;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.Schedule!.SubstituteTeacherId = entity.SubstituteTeacherId;
        entity.Schedule.SubstituteTeacherNameSnapshot = entity.SubstituteTeacherNameSnapshot;
        entity.Schedule.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await TryNotifyReviewAsync(entity, true, bearerToken, cancellationToken);
        return entity.ToResponse();
    }

    public async Task<TeachingSubstitutionResponse> RejectAsync(Guid id, ReviewTeachingSubstitutionRequest request, string? bearerToken, CancellationToken cancellationToken)
    {
        var entity = await db.TeachingSubstitutionRequests.Include(x => x.Schedule).FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw NotFound("Substitution request");
        if (entity.Status != SubstituteRequestStatus.Pending) throw Conflict("Only pending requests can be rejected");
        entity.Status = SubstituteRequestStatus.Rejected;
        entity.AdminNote = request.AdminNote;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await TryNotifyReviewAsync(entity, false, bearerToken, cancellationToken);
        return entity.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.TeachingSubstitutionRequests.Include(x => x.Schedule).FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw NotFound("Substitution request");
        if (entity.Status == SubstituteRequestStatus.Approved && entity.Schedule is not null && entity.Schedule.SubstituteTeacherId == entity.SubstituteTeacherId)
        {
            entity.Schedule.SubstituteTeacherId = null;
            entity.Schedule.SubstituteTeacherNameSnapshot = null;
            entity.Schedule.UpdatedAt = DateTime.UtcNow;
        }

        db.TeachingSubstitutionRequests.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static AppException NotFound(string name) => new($"{name} not found", StatusCodes.Status404NotFound);
    private static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
    private async Task TryNotifyReviewAsync(TeachingSubstitutionRequest request, bool approved, string? bearerToken, CancellationToken cancellationToken)
    {
        try
        {
            var teacherIds = new[] { request.RequestingTeacherId, request.SubstituteTeacherId }.Distinct().ToList();
            var teacherEmails = await teachers.Query()
                .Where(x => teacherIds.Contains(x.Id))
                .Select(x => new { x.Id, x.FullName, x.Email })
                .ToListAsync(cancellationToken);

            var statusText = approved ? "đã được duyệt" : "đã bị từ chối";
            var subject = approved ? "EduCenter duyệt yêu cầu dạy thay" : "EduCenter từ chối yêu cầu dạy thay";
            foreach (var teacher in teacherEmails.Where(x => !string.IsNullOrWhiteSpace(x.Email)))
            {
                await notifications.SendEmailAsync(
                    teacher.Email,
                    subject,
                    $"""
                    <p>Chào <strong>{teacher.FullName}</strong>,</p>
                    <p>Yêu cầu dạy thay cho lớp <strong>{request.Schedule?.ClassNameSnapshot}</strong> {statusText}.</p>
                    <p>Buổi học: <strong>{VietnameseDay(request.Schedule?.DayOfWeek)} {request.Schedule?.StartTime:HH\\:mm} - {request.Schedule?.EndTime:HH\\:mm}</strong>, phòng <strong>{request.Schedule?.Room}</strong>.</p>
                    <p>Giảng viên xin nghỉ: <strong>{request.RequestingTeacherNameSnapshot}</strong>.</p>
                    <p>Giảng viên dạy thay: <strong>{request.SubstituteTeacherNameSnapshot}</strong>.</p>
                    {(string.IsNullOrWhiteSpace(request.AdminNote) ? string.Empty : $"<p>Ghi chú admin: {request.AdminNote}</p>")}
                    """,
                    bearerToken,
                    cancellationToken);
            }
        }
        catch
        {
            // Email notification must not rollback substitution review.
        }
    }

    private static string VietnameseDay(DayOfWeek? day) => day switch
    {
        DayOfWeek.Monday => "Thứ 2",
        DayOfWeek.Tuesday => "Thứ 3",
        DayOfWeek.Wednesday => "Thứ 4",
        DayOfWeek.Thursday => "Thứ 5",
        DayOfWeek.Friday => "Thứ 6",
        DayOfWeek.Saturday => "Thứ 7",
        DayOfWeek.Sunday => "Chủ nhật",
        _ => "Chưa rõ"
    };
}

public interface ITeacherService
{
    Task<IReadOnlyList<TeacherResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<TeacherResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TeacherResponse> CreateAsync(CreateTeacherRequest request, CancellationToken cancellationToken);
    Task<TeacherResponse> UpdateAsync(Guid id, UpdateTeacherRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class TeacherService(IRepository<Teacher> teachers) : ITeacherService
{
    public async Task<IReadOnlyList<TeacherResponse>> GetAllAsync(CancellationToken cancellationToken) => await teachers.Query().OrderBy(x => x.FullName).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<TeacherResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => (await teachers.GetByIdAsync(id, cancellationToken) ?? throw new AppException("Teacher not found", StatusCodes.Status404NotFound)).ToResponse();
    public async Task<TeacherResponse> CreateAsync(CreateTeacherRequest request, CancellationToken cancellationToken)
    {
        if (await teachers.AnyAsync(x => x.Email == request.Email, cancellationToken)) throw new AppException("Email already exists", StatusCodes.Status409Conflict);
        var now = DateTime.UtcNow;
        var entity = new Teacher { Id = Guid.NewGuid(), FullName = request.FullName, Email = request.Email, Phone = request.Phone, AvatarUrl = request.AvatarUrl, Specialization = request.Specialization, Bio = request.Bio, ExperienceYears = request.ExperienceYears, Rating = request.Rating, Status = request.Status, CreatedAt = now, UpdatedAt = now };
        await teachers.AddAsync(entity, cancellationToken);
        await teachers.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task<TeacherResponse> UpdateAsync(Guid id, UpdateTeacherRequest request, CancellationToken cancellationToken)
    {
        var entity = await teachers.GetByIdAsync(id, cancellationToken) ?? throw new AppException("Teacher not found", StatusCodes.Status404NotFound);
        entity.FullName = request.FullName; entity.Email = request.Email; entity.Phone = request.Phone; entity.AvatarUrl = request.AvatarUrl; entity.Specialization = request.Specialization; entity.Bio = request.Bio; entity.ExperienceYears = request.ExperienceYears; entity.Rating = request.Rating; entity.Status = request.Status; entity.UpdatedAt = DateTime.UtcNow;
        await teachers.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await teachers.GetByIdAsync(id, cancellationToken) ?? throw new AppException("Teacher not found", StatusCodes.Status404NotFound);
        teachers.Remove(entity);
        await teachers.SaveChangesAsync(cancellationToken);
    }
}
