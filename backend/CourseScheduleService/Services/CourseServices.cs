using CourseScheduleService.Dtos;
using CourseScheduleService.Entities;
using CourseScheduleService.Enums;
using CourseScheduleService.Mappings;
using CourseScheduleService.Repositories;
using Microsoft.EntityFrameworkCore;
using CourseScheduleService.Data;
using System.Data;
using System.Globalization;
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
        var rows = await classes.Query().OrderBy(x => x.ClassCode).ToListAsync(cancellationToken);
        await NormalizeClassesAsync(rows, cancellationToken);
        return rows.Select(x => x.ToResponse()).ToList();
    }

    public async Task<ClassResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await classes.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Class");
        if (ApplyLifecycleStatus(entity, DateTime.UtcNow)) await classes.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task<IReadOnlyList<ClassResponse>> ByCourseAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var rows = await classes.Query().Where(x => x.CourseId == courseId).ToListAsync(cancellationToken);
        await NormalizeClassesAsync(rows, cancellationToken);
        return rows.Select(x => x.ToResponse()).ToList();
    }

    public async Task<IReadOnlyList<ClassResponse>> ByTeacherAsync(Guid teacherId, CancellationToken cancellationToken)
    {
        var rows = await classes.Query().Where(x => x.TeacherId == teacherId).ToListAsync(cancellationToken);
        await NormalizeClassesAsync(rows, cancellationToken);
        return rows.Select(x => x.ToResponse()).ToList();
    }

    public async Task<IReadOnlyList<ClassResponse>> OpeningAsync(CancellationToken cancellationToken)
    {
        var rows = await classes.Query().ToListAsync(cancellationToken);
        await NormalizeClassesAsync(rows, cancellationToken);
        return rows.Where(x => x.Status == ClassStatus.Open).Select(x => x.ToResponse()).ToList();
    }

    public async Task<ClassResponse> CreateAsync(CreateClassRequest request, CancellationToken cancellationToken)
    {
        var course = await courses.GetByIdAsync(request.CourseId, cancellationToken) ?? throw NotFound("Course");
        var teacher = await teachers.GetByIdAsync(request.TeacherId, cancellationToken) ?? throw NotFound("Teacher");
        ValidateCapacity(request.MaxStudents, request.CurrentStudents);
        var classCode = string.IsNullOrWhiteSpace(request.ClassCode) ? await GenerateClassCodeAsync(cancellationToken) : request.ClassCode.Trim().ToUpperInvariant();
        if (await classes.AnyAsync(x => x.ClassCode == classCode, cancellationToken)) throw Conflict("Class code already exists");
        var now = DateTime.UtcNow;
        var entity = new Class { Id = Guid.NewGuid(), CourseId = course.Id, CourseNameSnapshot = course.Name, ClassCode = classCode, ClassName = request.ClassName, TeacherId = teacher.Id, TeacherNameSnapshot = teacher.FullName, Room = request.Room, MaxStudents = request.MaxStudents, CurrentStudents = request.CurrentStudents, StartDate = request.StartDate, EndDate = request.EndDate, LearningMode = request.LearningMode, Status = request.Status, CreatedAt = now, UpdatedAt = now };
        ApplyLifecycleStatus(entity, now);
        await classes.AddAsync(entity, cancellationToken);
        await classes.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task<ClassResponse> UpdateAsync(Guid id, UpdateClassRequest request, CancellationToken cancellationToken)
    {
        var entity = await classes.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Class");
        var course = await courses.GetByIdAsync(request.CourseId, cancellationToken) ?? throw NotFound("Course");
        var teacher = await teachers.GetByIdAsync(request.TeacherId, cancellationToken) ?? throw NotFound("Teacher");
        ValidateCapacity(request.MaxStudents, request.CurrentStudents);
        entity.CourseId = course.Id; entity.CourseNameSnapshot = course.Name; entity.ClassCode = request.ClassCode; entity.ClassName = request.ClassName; entity.TeacherId = teacher.Id; entity.TeacherNameSnapshot = teacher.FullName; entity.Room = request.Room; entity.MaxStudents = request.MaxStudents; entity.CurrentStudents = request.CurrentStudents; entity.StartDate = request.StartDate; entity.EndDate = request.EndDate; entity.LearningMode = request.LearningMode; entity.Status = request.Status; entity.UpdatedAt = DateTime.UtcNow;
        ApplyLifecycleStatus(entity, DateTime.UtcNow);
        classes.Update(entity);
        await classes.SaveChangesAsync(cancellationToken);
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

public sealed class ScheduleManagementService(IRepository<Schedule> schedules, IRepository<Class> classes) : IScheduleManagementService
{
    public async Task<IReadOnlyList<ScheduleResponse>> GetAllAsync(CancellationToken cancellationToken) => await schedules.Query().Include(x => x.Class).OrderBy(x => x.SessionNumber).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<ScheduleResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) => (await schedules.Query().Include(x => x.Class).FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ?? throw NotFound("Schedule")).ToResponse();
    public async Task<IReadOnlyList<ScheduleResponse>> ByClassAsync(Guid classId, CancellationToken cancellationToken) => await schedules.Query().Include(x => x.Class).Where(x => x.ClassId == classId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<ScheduleResponse>> ByTeacherAsync(Guid teacherId, CancellationToken cancellationToken) => await schedules.Query().Include(x => x.Class).Where(x => x.Class != null && x.Class.TeacherId == teacherId).Select(x => x.ToResponse()).ToListAsync(cancellationToken);

    public async Task<ScheduleResponse> CreateAsync(CreateScheduleRequest request, CancellationToken cancellationToken)
    {
        var classEntity = await classes.GetByIdAsync(request.ClassId, cancellationToken) ?? throw NotFound("Class");
        await ValidateNoConflict(Guid.Empty, request, cancellationToken);
        var now = DateTime.UtcNow;
        var entity = new Schedule { Id = Guid.NewGuid(), ClassId = classEntity.Id, ClassNameSnapshot = classEntity.ClassName, Class = classEntity, DayOfWeek = request.DayOfWeek, StudyShift = request.StudyShift, StartTime = request.StartTime, EndTime = request.EndTime, Room = request.Room, Topic = request.Topic, SessionNumber = request.SessionNumber, Status = request.Status, CreatedAt = now, UpdatedAt = now };
        await schedules.AddAsync(entity, cancellationToken);
        await schedules.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task<ScheduleResponse> UpdateAsync(Guid id, UpdateScheduleRequest request, CancellationToken cancellationToken)
    {
        var entity = await schedules.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Schedule");
        var classEntity = await classes.GetByIdAsync(request.ClassId, cancellationToken) ?? throw NotFound("Class");
        await ValidateNoConflict(id, request, cancellationToken);
        entity.ClassId = classEntity.Id; entity.ClassNameSnapshot = classEntity.ClassName; entity.Class = classEntity; entity.DayOfWeek = request.DayOfWeek; entity.StudyShift = request.StudyShift; entity.StartTime = request.StartTime; entity.EndTime = request.EndTime; entity.Room = request.Room; entity.Topic = request.Topic; entity.SessionNumber = request.SessionNumber; entity.Status = request.Status; entity.UpdatedAt = DateTime.UtcNow;
        await schedules.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await schedules.GetByIdAsync(id, cancellationToken) ?? throw NotFound("Schedule");
        schedules.Remove(entity);
        await schedules.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateNoConflict(Guid id, CreateScheduleRequest request, CancellationToken cancellationToken)
    {
        if (await schedules.Query().AnyAsync(x => x.Id != id && x.ClassId == request.ClassId && x.DayOfWeek == request.DayOfWeek && x.StartTime == request.StartTime && x.EndTime == request.EndTime, cancellationToken)) throw Conflict("Schedule overlaps in same class");
        if (await schedules.Query().AnyAsync(x => x.Id != id && x.Room == request.Room && x.DayOfWeek == request.DayOfWeek && x.StartTime == request.StartTime && x.EndTime == request.EndTime, cancellationToken)) throw Conflict("Room already booked at this time");
    }
    private static AppException NotFound(string name) => new($"{name} not found", StatusCodes.Status404NotFound);
    private static AppException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
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
