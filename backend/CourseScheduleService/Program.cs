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
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IScheduleManagementService, ScheduleManagementService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<ITeachingSubstitutionService, TeachingSubstitutionService>();
builder.Services.AddHttpClient<INotificationClient, NotificationClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:PaymentReport"] ?? "http://127.0.0.1:5003");
});

var app = builder.Build();

if (!string.Equals(Environment.GetEnvironmentVariable("EDUCENTER_SKIP_DB_INIT"), "true", StringComparison.OrdinalIgnoreCase))
{
    await TryEnsureDatabaseAsync(app.Services);
}

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("EduCenterCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task TryEnsureDatabaseAsync(IServiceProvider services)
{
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseStartup");
    for (var attempt = 1; attempt <= 3; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CourseDbContext>();
            db.Database.SetCommandTimeout(5);
            await db.Database.EnsureCreatedAsync();
            await db.Database.ExecuteSqlRawAsync("""
                IF OBJECT_ID('Rooms', 'U') IS NULL
                BEGIN
                    CREATE TABLE Rooms (
                        Id uniqueidentifier NOT NULL PRIMARY KEY,
                        Code nvarchar(40) NOT NULL,
                        Name nvarchar(100) NOT NULL,
                        Capacity int NOT NULL,
                        Note nvarchar(500) NULL,
                        IsActive bit NOT NULL,
                        CreatedAt datetime2 NOT NULL,
                        UpdatedAt datetime2 NOT NULL
                    );
                    CREATE UNIQUE INDEX IX_Rooms_Code ON Rooms(Code);
                    CREATE UNIQUE INDEX IX_Rooms_Name ON Rooms(Name);
                END
                """);

            await db.Database.ExecuteSqlRawAsync("""
                IF COL_LENGTH('Classes', 'RoomId') IS NULL
                BEGIN
                    ALTER TABLE Classes ADD RoomId uniqueidentifier NULL;
                END
                """);

            await db.Database.ExecuteSqlRawAsync("""
                IF NOT EXISTS (SELECT 1 FROM Rooms WHERE Code = N'A101')
                    INSERT INTO Rooms (Id, Code, Name, Capacity, Note, IsActive, CreatedAt, UpdatedAt)
                    VALUES ('55555555-5555-5555-5555-000000000001', N'A101', N'Phòng A101', 30, N'Phòng học lý thuyết', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                IF NOT EXISTS (SELECT 1 FROM Rooms WHERE Code = N'B202')
                    INSERT INTO Rooms (Id, Code, Name, Capacity, Note, IsActive, CreatedAt, UpdatedAt)
                    VALUES ('55555555-5555-5555-5555-000000000002', N'B202', N'Phòng B202', 35, N'Phòng học thực hành', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                IF NOT EXISTS (SELECT 1 FROM Rooms WHERE Code = N'C303')
                    INSERT INTO Rooms (Id, Code, Name, Capacity, Note, IsActive, CreatedAt, UpdatedAt)
                    VALUES ('55555555-5555-5555-5555-000000000003', N'C303', N'Phòng C303', 25, N'Phòng học nhóm nhỏ', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                IF NOT EXISTS (SELECT 1 FROM Rooms WHERE Code = N'LAB01')
                    INSERT INTO Rooms (Id, Code, Name, Capacity, Note, IsActive, CreatedAt, UpdatedAt)
                    VALUES ('55555555-5555-5555-5555-000000000004', N'LAB01', N'Phòng Lab 01', 28, N'Phòng máy tính', 1, SYSUTCDATETIME(), SYSUTCDATETIME());
                IF NOT EXISTS (SELECT 1 FROM Rooms WHERE Code = N'ONLINE')
                    INSERT INTO Rooms (Id, Code, Name, Capacity, Note, IsActive, CreatedAt, UpdatedAt)
                    VALUES ('55555555-5555-5555-5555-000000000005', N'ONLINE', N'Lớp trực tuyến', 100, N'Phòng học online', 1, SYSUTCDATETIME(), SYSUTCDATETIME());

                UPDATE Rooms SET Name = N'Phòng A101', Note = N'Phòng học lý thuyết', UpdatedAt = SYSUTCDATETIME() WHERE Code = N'A101';
                UPDATE Rooms SET Name = N'Phòng B202', Note = N'Phòng học thực hành', UpdatedAt = SYSUTCDATETIME() WHERE Code = N'B202';
                UPDATE Rooms SET Name = N'Phòng C303', Note = N'Phòng học nhóm nhỏ', UpdatedAt = SYSUTCDATETIME() WHERE Code = N'C303';
                UPDATE Rooms SET Name = N'Phòng Lab 01', Note = N'Phòng máy tính', UpdatedAt = SYSUTCDATETIME() WHERE Code = N'LAB01';
                UPDATE Rooms SET Name = N'Lớp trực tuyến', Note = N'Phòng học online', UpdatedAt = SYSUTCDATETIME() WHERE Code = N'ONLINE';
                UPDATE Rooms SET Note = N'Phòng máy tính', UpdatedAt = SYSUTCDATETIME()
                WHERE (Note = N'Tu dong tao tu du lieu lop hoc hien co' OR Note = N'Phòng học được đồng bộ từ dữ liệu lớp hiện có.')
                  AND (UPPER(Code) LIKE N'LAB%' OR UPPER(Name) LIKE N'%LAB%');
                UPDATE Rooms SET Note = N'Phòng học trực tuyến', UpdatedAt = SYSUTCDATETIME()
                WHERE (Note = N'Tu dong tao tu du lieu lop hoc hien co' OR Note = N'Phòng học được đồng bộ từ dữ liệu lớp hiện có.')
                  AND (UPPER(Code) LIKE N'ONLINE%' OR UPPER(Name) LIKE N'%ONLINE%' OR UPPER(Name) LIKE N'%ZOOM%');
                UPDATE Rooms SET Note = N'Phòng học lý thuyết', UpdatedAt = SYSUTCDATETIME()
                WHERE Note = N'Tu dong tao tu du lieu lop hoc hien co'
                   OR Note = N'Phòng học được đồng bộ từ dữ liệu lớp hiện có.';
                """);

            await db.Database.ExecuteSqlRawAsync("""
                ;WITH ExistingRoomNames AS (
                    SELECT DISTINCT LTRIM(RTRIM(Room)) AS RoomName
                    FROM Classes
                    WHERE Room IS NOT NULL
                      AND LTRIM(RTRIM(Room)) <> N''
                ),
                MissingRooms AS (
                    SELECT RoomName,
                           LEFT(UPPER(REPLACE(REPLACE(RoomName, N' ', N''), N'-', N'')), 32) AS BaseCode,
                           ROW_NUMBER() OVER (ORDER BY RoomName) AS RowNo
                    FROM ExistingRoomNames e
                    WHERE NOT EXISTS (
                        SELECT 1 FROM Rooms r
                        WHERE r.Name = e.RoomName
                           OR r.Code = e.RoomName
                           OR r.Code = LEFT(UPPER(REPLACE(REPLACE(e.RoomName, N' ', N''), N'-', N'')), 40)
                    )
                )
                INSERT INTO Rooms (Id, Code, Name, Capacity, Note, IsActive, CreatedAt, UpdatedAt)
                SELECT NEWID(),
                       LEFT(BaseCode + N'-' + CAST(RowNo AS nvarchar(8)), 40),
                       RoomName,
                       30,
                       CASE
                           WHEN UPPER(RoomName) LIKE N'%LAB%' THEN N'Phòng máy tính'
                           WHEN UPPER(RoomName) LIKE N'%ONLINE%' OR UPPER(RoomName) LIKE N'%ZOOM%' THEN N'Phòng học trực tuyến'
                           ELSE N'Phòng học lý thuyết'
                       END,
                       1,
                       SYSUTCDATETIME(),
                       SYSUTCDATETIME()
                FROM MissingRooms;
                """);

            await db.Database.ExecuteSqlRawAsync("""
                UPDATE c
                SET RoomId = r.Id,
                    Room = r.Name
                FROM Classes c
                INNER JOIN Rooms r ON r.Name = LTRIM(RTRIM(c.Room)) OR r.Code = LTRIM(RTRIM(c.Room))
                WHERE c.RoomId IS NULL;

                UPDATE c
                SET Room = r.Name,
                    UpdatedAt = SYSUTCDATETIME()
                FROM Classes c
                INNER JOIN Rooms r ON r.Id = c.RoomId
                WHERE c.Room <> r.Name;

                UPDATE s
                SET Room = c.Room,
                    UpdatedAt = SYSUTCDATETIME()
                FROM Schedules s
                INNER JOIN Classes c ON c.Id = s.ClassId
                WHERE s.Room <> c.Room;
                """);

            await db.Database.ExecuteSqlRawAsync("""
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Classes_RoomId' AND object_id = OBJECT_ID('Classes'))
                    CREATE INDEX IX_Classes_RoomId ON Classes(RoomId);
                """);

            await db.Database.ExecuteSqlRawAsync("""
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Classes_Rooms_RoomId')
                    ALTER TABLE Classes WITH CHECK ADD CONSTRAINT FK_Classes_Rooms_RoomId FOREIGN KEY (RoomId) REFERENCES Rooms(Id);
                """);

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
                """);

            await db.Database.ExecuteSqlRawAsync("""
                IF OBJECT_ID('ClassTeachers', 'U') IS NULL
                BEGIN
                    CREATE TABLE ClassTeachers (
                        ClassId uniqueidentifier NOT NULL,
                        TeacherId uniqueidentifier NOT NULL,
                        TeacherNameSnapshot nvarchar(200) NOT NULL,
                        IsPrimary bit NOT NULL,
                        SortOrder int NOT NULL,
                        CreatedAt datetime2 NOT NULL,
                        CONSTRAINT PK_ClassTeachers PRIMARY KEY (ClassId, TeacherId),
                        CONSTRAINT FK_ClassTeachers_Classes_ClassId FOREIGN KEY (ClassId) REFERENCES Classes(Id) ON DELETE CASCADE,
                        CONSTRAINT FK_ClassTeachers_Teachers_TeacherId FOREIGN KEY (TeacherId) REFERENCES Teachers(Id)
                    );
                    CREATE INDEX IX_ClassTeachers_TeacherId ON ClassTeachers(TeacherId);
                END

                INSERT INTO ClassTeachers (ClassId, TeacherId, TeacherNameSnapshot, IsPrimary, SortOrder, CreatedAt)
                SELECT c.Id, c.TeacherId, c.TeacherNameSnapshot, 1, 0, SYSUTCDATETIME()
                FROM Classes c
                WHERE c.TeacherId IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM ClassTeachers ct
                      WHERE ct.ClassId = c.Id AND ct.TeacherId = c.TeacherId
                  );
                """);

            await db.Database.ExecuteSqlRawAsync("""
                IF COL_LENGTH('Schedules', 'AssignedTeacherId') IS NULL
                    ALTER TABLE Schedules ADD AssignedTeacherId uniqueidentifier NULL;
                IF COL_LENGTH('Schedules', 'AssignedTeacherNameSnapshot') IS NULL
                    ALTER TABLE Schedules ADD AssignedTeacherNameSnapshot nvarchar(200) NULL;
                IF COL_LENGTH('Schedules', 'SubstituteTeacherId') IS NULL
                    ALTER TABLE Schedules ADD SubstituteTeacherId uniqueidentifier NULL;
                IF COL_LENGTH('Schedules', 'SubstituteTeacherNameSnapshot') IS NULL
                    ALTER TABLE Schedules ADD SubstituteTeacherNameSnapshot nvarchar(200) NULL;
                """);

            await db.Database.ExecuteSqlRawAsync("""
                UPDATE s
                SET AssignedTeacherId = c.TeacherId,
                    AssignedTeacherNameSnapshot = c.TeacherNameSnapshot,
                    UpdatedAt = SYSUTCDATETIME()
                FROM Schedules s
                INNER JOIN Classes c ON c.Id = s.ClassId
                WHERE s.AssignedTeacherId IS NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Schedules_AssignedTeacherId' AND object_id = OBJECT_ID('Schedules'))
                    CREATE INDEX IX_Schedules_AssignedTeacherId ON Schedules(AssignedTeacherId);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Schedules_SubstituteTeacherId' AND object_id = OBJECT_ID('Schedules'))
                    CREATE INDEX IX_Schedules_SubstituteTeacherId ON Schedules(SubstituteTeacherId);

                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Schedules_Teachers_AssignedTeacherId')
                    ALTER TABLE Schedules WITH NOCHECK ADD CONSTRAINT FK_Schedules_Teachers_AssignedTeacherId FOREIGN KEY (AssignedTeacherId) REFERENCES Teachers(Id);
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Schedules_Teachers_SubstituteTeacherId')
                    ALTER TABLE Schedules WITH NOCHECK ADD CONSTRAINT FK_Schedules_Teachers_SubstituteTeacherId FOREIGN KEY (SubstituteTeacherId) REFERENCES Teachers(Id);
                """);

            await db.Database.ExecuteSqlRawAsync("""
                IF OBJECT_ID('TeachingSubstitutionRequests', 'U') IS NULL
                BEGIN
                    CREATE TABLE TeachingSubstitutionRequests (
                        Id uniqueidentifier NOT NULL PRIMARY KEY,
                        ScheduleId uniqueidentifier NOT NULL,
                        RequestingTeacherId uniqueidentifier NOT NULL,
                        RequestingTeacherNameSnapshot nvarchar(200) NOT NULL,
                        SubstituteTeacherId uniqueidentifier NOT NULL,
                        SubstituteTeacherNameSnapshot nvarchar(200) NOT NULL,
                        Reason nvarchar(500) NULL,
                        Status int NOT NULL,
                        AdminNote nvarchar(500) NULL,
                        CreatedAt datetime2 NOT NULL,
                        UpdatedAt datetime2 NOT NULL,
                        CONSTRAINT FK_TeachingSubstitutionRequests_Schedules_ScheduleId FOREIGN KEY (ScheduleId) REFERENCES Schedules(Id) ON DELETE CASCADE,
                        CONSTRAINT FK_TeachingSubstitutionRequests_Teachers_RequestingTeacherId FOREIGN KEY (RequestingTeacherId) REFERENCES Teachers(Id),
                        CONSTRAINT FK_TeachingSubstitutionRequests_Teachers_SubstituteTeacherId FOREIGN KEY (SubstituteTeacherId) REFERENCES Teachers(Id)
                    );
                    CREATE INDEX IX_TeachingSubstitutionRequests_ScheduleId ON TeachingSubstitutionRequests(ScheduleId);
                    CREATE INDEX IX_TeachingSubstitutionRequests_Status ON TeachingSubstitutionRequests(Status);
                END
                """);
            return;
        }
        catch (Exception ex) when (attempt < 3)
        {
            logger.LogWarning(ex, "Course database initialization failed. Retrying {Attempt}/3.", attempt);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Course database is unavailable. Service will continue; database-backed endpoints may return errors until SQL Server is available.");
        }
    }
}
