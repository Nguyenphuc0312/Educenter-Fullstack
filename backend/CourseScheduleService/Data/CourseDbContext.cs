using CourseScheduleService.Entities;
using CourseScheduleService.Enums;
using Microsoft.EntityFrameworkCore;

namespace CourseScheduleService.Data;

public sealed class CourseDbContext(DbContextOptions<CourseDbContext> options) : DbContext(options)
{
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Teacher> Teachers => Set<Teacher>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<Course>().HasIndex(x => x.Slug).IsUnique();
        modelBuilder.Entity<Course>().Property(x => x.TuitionFee).HasPrecision(18, 2);
        modelBuilder.Entity<Course>().Property(x => x.Rating).HasPrecision(3, 2);
        modelBuilder.Entity<Teacher>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Teacher>().Property(x => x.Rating).HasPrecision(3, 2);
        modelBuilder.Entity<Class>().HasIndex(x => x.ClassCode).IsUnique();
        modelBuilder.Entity<Class>().HasOne(x => x.Course).WithMany(x => x.Classes).HasForeignKey(x => x.CourseId);
        modelBuilder.Entity<Class>().HasOne(x => x.Teacher).WithMany(x => x.Classes).HasForeignKey(x => x.TeacherId);
        modelBuilder.Entity<Schedule>().HasOne(x => x.Class).WithMany(x => x.Schedules).HasForeignKey(x => x.ClassId);

        var now = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var teacherIds = Enumerable.Range(1, 4).Select(i => Guid.Parse($"11111111-1111-1111-1111-{i:000000000000}")).ToArray();
        var courseIds = Enumerable.Range(1, 8).Select(i => Guid.Parse($"22222222-2222-2222-2222-{i:000000000000}")).ToArray();
        var classIds = Enumerable.Range(1, 6).Select(i => Guid.Parse($"33333333-3333-3333-3333-{i:000000000000}")).ToArray();

        modelBuilder.Entity<Teacher>().HasData(
            new Teacher { Id = teacherIds[0], FullName = "Nguyen Van Teacher", Email = "teacher01@educenter.vn", Phone = "0901000001", Specialization = "Frontend", Bio = "React/Vue mentor", ExperienceYears = 6, Rating = 4.8m, Status = TeacherStatus.Active, CreatedAt = now, UpdatedAt = now },
            new Teacher { Id = teacherIds[1], FullName = "Tran Thi Backend", Email = "teacher02@educenter.vn", Phone = "0901000002", Specialization = "Backend", Bio = "ASP.NET Core mentor", ExperienceYears = 8, Rating = 4.9m, Status = TeacherStatus.Active, CreatedAt = now, UpdatedAt = now },
            new Teacher { Id = teacherIds[2], FullName = "Le SQL Master", Email = "teacher03@educenter.vn", Phone = "0901000003", Specialization = "Database", Bio = "SQL Server trainer", ExperienceYears = 7, Rating = 4.7m, Status = TeacherStatus.Active, CreatedAt = now, UpdatedAt = now },
            new Teacher { Id = teacherIds[3], FullName = "Pham UI Mentor", Email = "teacher04@educenter.vn", Phone = "0901000004", Specialization = "UI/UX", Bio = "Product design trainer", ExperienceYears = 5, Rating = 4.6m, Status = TeacherStatus.Active, CreatedAt = now, UpdatedAt = now }
        );

        var courseNames = new[] { "ReactJS Co ban", "VueJS Co ban", "SQL Server cho nguoi moi", "ASP.NET Core API", "Fullstack Web Developer", "Node.js Backend", "UI/UX Design Co ban", "Tin hoc van phong" };
        var slugs = new[] { "reactjs-co-ban", "vuejs-co-ban", "sql-server-cho-nguoi-moi", "aspnet-core-api", "fullstack-web-developer", "nodejs-backend", "ui-ux-design-co-ban", "tin-hoc-van-phong" };
        modelBuilder.Entity<Course>().HasData(courseNames.Select((name, i) => new Course
        {
            Id = courseIds[i],
            Code = $"COURSE{i + 1:00}",
            Name = name,
            Slug = slugs[i],
            ShortDescription = $"Khoa hoc {name}",
            Description = $"Noi dung dao tao thuc hanh cho {name}.",
            Category = i is 6 or 7 ? "Office" : "Programming",
            Level = i % 3 == 0 ? "Beginner" : "Intermediate",
            TuitionFee = 2500000 + i * 350000,
            TotalSessions = 12 + i,
            DurationText = $"{6 + i} tuan",
            ThumbnailUrl = null,
            Status = i == 7 ? CourseStatus.ComingSoon : CourseStatus.Opening,
            IsBestSeller = i is 0 or 4,
            IsPopularThisWeek = i is 1 or 3 or 5,
            ViewCount = 100 + i * 30,
            EnrolledCount = 20 + i * 5,
            Rating = 4.2m + i % 4 * 0.1m,
            CreatedAt = now,
            UpdatedAt = now
        }));

        modelBuilder.Entity<Class>().HasData(Enumerable.Range(0, 6).Select(i => new Class
        {
            Id = classIds[i],
            CourseId = courseIds[i],
            CourseNameSnapshot = courseNames[i],
            ClassCode = $"CLS{i + 1:00}",
            ClassName = $"{courseNames[i]} - Lop {i + 1}",
            TeacherId = teacherIds[i % teacherIds.Length],
            TeacherNameSnapshot = i % 2 == 0 ? "Nguyen Van Teacher" : "Tran Thi Backend",
            Room = i % 2 == 0 ? "A101" : "B202",
            MaxStudents = 30,
            CurrentStudents = 10 + i * 3,
            StartDate = now.AddDays(i * 7),
            EndDate = now.AddDays(i * 7 + 60),
            LearningMode = i % 2 == 0 ? LearningMode.Offline : LearningMode.Hybrid,
            Status = ClassStatus.Open,
            CreatedAt = now,
            UpdatedAt = now
        }));

        modelBuilder.Entity<Schedule>().HasData(Enumerable.Range(0, 12).Select(i =>
        {
            var classId = classIds[i % classIds.Length];
            return new Schedule
            {
                Id = Guid.Parse($"44444444-4444-4444-4444-{i + 1:000000000000}"),
                ClassId = classId,
                ClassNameSnapshot = $"Lop {i % classIds.Length + 1}",
                DayOfWeek = (DayOfWeek)((i % 6) + 1),
                StudyShift = i % 3 == 0 ? StudyShift.Morning : i % 3 == 1 ? StudyShift.Afternoon : StudyShift.Evening,
                StartTime = new TimeOnly(8 + (i % 3) * 4, 0),
                EndTime = new TimeOnly(10 + (i % 3) * 4, 0),
                Room = i % 2 == 0 ? "A101" : "B202",
                Topic = $"Buoi {i + 1}",
                SessionNumber = i + 1,
                Status = ScheduleStatus.Scheduled,
                CreatedAt = now,
                UpdatedAt = now
            };
        }));
    }
}
