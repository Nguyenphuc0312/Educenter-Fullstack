using CourseScheduleService.Data;
using CourseScheduleService.Extensions;
using CourseScheduleService.Middlewares;
using CourseScheduleService.Repositories;
using CourseScheduleService.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddDataProtection()
    .SetApplicationName("EduCenter")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")));

builder.Services.AddEduCenterApi(builder.Configuration, "EduCenter Course Schedule Service");
builder.Services.AddDbContext<CourseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CourseDB")));
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<ICourseCatalogService, CourseCatalogService>();
builder.Services.AddScoped<IClassManagementService, ClassManagementService>();
builder.Services.AddScoped<IScheduleManagementService, ScheduleManagementService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IClassroomService, ClassroomService>();
builder.Services.AddScoped<IScheduleChangeService, ScheduleChangeService>();

var app = builder.Build();

if (!string.Equals(Environment.GetEnvironmentVariable("EDUCENTER_SKIP_DB_INIT"), "true", StringComparison.OrdinalIgnoreCase))
{
    await EnsureDatabaseAsync(app.Services);
}

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("EduCenterCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task EnsureDatabaseAsync(IServiceProvider services)
{
    for (var attempt = 1; attempt <= 15; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CourseDbContext>();
            await db.Database.EnsureCreatedAsync();
            await db.Database.ExecuteSqlRawAsync("""
                IF OBJECT_ID('ClassSeatReservations', 'U') IS NULL
                BEGIN
                    CREATE TABLE ClassSeatReservations (
                        EnrollmentId uniqueidentifier NOT NULL PRIMARY KEY,
                        ClassId uniqueidentifier NOT NULL,
                        CreatedAt datetime2 NOT NULL
                    );
                    CREATE INDEX IX_ClassSeatReservations_ClassId ON ClassSeatReservations(ClassId);
                END

                IF OBJECT_ID('Classrooms', 'U') IS NULL
                BEGIN
                    CREATE TABLE Classrooms (
                        Id uniqueidentifier NOT NULL PRIMARY KEY,
                        Code nvarchar(50) NOT NULL,
                        Name nvarchar(200) NOT NULL,
                        Building nvarchar(100) NOT NULL,
                        Floor nvarchar(20) NOT NULL,
                        Capacity int NOT NULL,
                        Type int NOT NULL,
                        Status int NOT NULL,
                        Description nvarchar(500) NULL,
                        HasProjector bit NOT NULL,
                        HasAirConditioner bit NOT NULL,
                        IsOnline bit NOT NULL,
                        OnlineMeetingUrl nvarchar(500) NULL,
                        CreatedAt datetime2 NOT NULL,
                        UpdatedAt datetime2 NOT NULL
                    );
                END

                IF COL_LENGTH('Classes', 'ClassroomId') IS NULL
                BEGIN
                    ALTER TABLE Classes ADD ClassroomId uniqueidentifier NULL;
                END

                IF COL_LENGTH('Classes', 'MinStudents') IS NULL
                BEGIN
                    ALTER TABLE Classes ADD MinStudents int NOT NULL DEFAULT 5;
                END

                IF COL_LENGTH('Schedules', 'TeacherId') IS NULL
                BEGIN
                    ALTER TABLE Schedules ADD TeacherId uniqueidentifier NULL;
                END

                IF COL_LENGTH('Schedules', 'TeacherNameSnapshot') IS NULL
                BEGIN
                    ALTER TABLE Schedules ADD TeacherNameSnapshot nvarchar(200) NULL;
                END

                IF OBJECT_ID('ScheduleChangeRequests', 'U') IS NULL
                BEGIN
                    CREATE TABLE ScheduleChangeRequests (
                        Id uniqueidentifier NOT NULL PRIMARY KEY,
                        ScheduleId uniqueidentifier NOT NULL,
                        Type int NOT NULL,
                        OriginalTeacherId uniqueidentifier NOT NULL,
                        OriginalTeacherName nvarchar(200) NOT NULL,
                        ProposedTeacherId uniqueidentifier NULL,
                        ProposedTeacherName nvarchar(200) NULL,
                        ProposedDayOfWeek int NULL,
                        ProposedStudyShift int NULL,
                        ProposedRoom nvarchar(100) NULL,
                        Status int NOT NULL,
                        Reason nvarchar(500) NULL,
                        CreatedAt datetime2 NOT NULL,
                        UpdatedAt datetime2 NOT NULL
                    );
                END

                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Classes_Classrooms_ClassroomId')
                BEGIN
                    ALTER TABLE Classes ADD CONSTRAINT FK_Classes_Classrooms_ClassroomId FOREIGN KEY (ClassroomId) REFERENCES Classrooms(Id);
                END

                IF NOT EXISTS (SELECT 1 FROM Classrooms)
                BEGIN
                    DECLARE @now datetime2 = SYSUTCDATETIME();
                    INSERT INTO Classrooms (Id, Code, Name, Building, Floor, Capacity, Type, Status, Description, HasProjector, HasAirConditioner, IsOnline, OnlineMeetingUrl, CreatedAt, UpdatedAt)
                    VALUES 
                    ('55555555-5555-5555-5555-000000000001', 'A101', N'Phòng A101', N'Tòa A', N'Tầng 1', 40, 0, 0, N'Phòng lý thuyết tiêu chuẩn', 1, 1, 0, NULL, @now, @now),
                    ('55555555-5555-5555-5555-000000000002', 'A102', N'Phòng A102', N'Tòa A', N'Tầng 1', 35, 0, 0, N'Phòng lý thuyết nhỏ', 1, 1, 0, NULL, @now, @now),
                    ('55555555-5555-5555-5555-000000000003', 'B201', N'Phòng máy B201', N'Tòa B', N'Tầng 2', 30, 1, 0, N'Phòng thực hành lập trình', 1, 1, 0, NULL, @now, @now),
                    ('55555555-5555-5555-5555-000000000004', 'B202', N'Phòng máy B202', N'Tòa B', N'Tầng 2', 25, 1, 0, N'Phòng thực hành thiết kế', 1, 1, 0, NULL, @now, @now),
                    ('55555555-5555-5555-5555-000000000005', 'C301', N'Hội trường C301', N'Tòa C', N'Tầng 3', 100, 2, 0, N'Hội trường lớn', 1, 1, 0, NULL, @now, @now),
                    ('55555555-5555-5555-5555-000000000006', 'C302', N'Phòng seminar C302', N'Tòa C', N'Tầng 3', 20, 2, 0, N'Phòng thảo luận nhóm nhỏ', 1, 1, 0, NULL, @now, @now),
                    ('55555555-5555-5555-5555-000000000007', 'ZOOM01', N'Online Zoom 01', '', '', 100, 3, 0, N'Phòng học trực tuyến Zoom 01', 0, 0, 1, 'https://zoom.us/j/room01', @now, @now),
                    ('55555555-5555-5555-5555-000000000008', 'ZOOM02', N'Online Zoom 02', '', '', 100, 3, 0, N'Phòng học trực tuyến Zoom 02', 0, 0, 1, 'https://zoom.us/j/room02', @now, @now),
                    ('55555555-5555-5555-5555-000000000009', 'A103', N'Phòng A103', N'Tòa A', N'Tầng 1', 40, 0, 2, N'Đang bảo trì máy chiếu', 0, 1, 0, NULL, @now, @now),
                    ('55555555-5555-5555-5555-000000000010', 'MEET01', N'Google Meet 01', '', '', 50, 3, 0, N'Phòng học Google Meet', 0, 0, 1, 'https://meet.google.com/room01', @now, @now);
                END
                """);
            return;
        }
        catch when (attempt < 15)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}
