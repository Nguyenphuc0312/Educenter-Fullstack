using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using StudentAttendanceService.Data;
using StudentAttendanceService.Extensions;
using StudentAttendanceService.Middlewares;
using StudentAttendanceService.Repositories;
using StudentAttendanceService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddDataProtection()
    .SetApplicationName("EduCenter")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")));

builder.Services.AddEduCenterApi(builder.Configuration, "EduCenter Student Attendance Service");
builder.Services.AddDbContext<StudentDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("StudentDB")));
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IStudentPortalService, StudentPortalService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAiAssistantService, AiAssistantService>();
builder.Services.AddSingleton<IAiFallbackStore, AiFallbackStore>();
builder.Services.AddHttpClient("AiRouter", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AiRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1/");
});
builder.Services.AddHttpClient("PaymentNotification", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:PaymentReport"] ?? "http://127.0.0.1:5003");
});
builder.Services.AddHttpClient<IClassCapacityClient, ClassCapacityClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:CourseSchedule"] ?? "http://127.0.0.1:5001");
});
builder.Services.AddHttpClient<IPaymentInvoiceClient, PaymentInvoiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:PaymentReport"] ?? "http://127.0.0.1:5003");
});
builder.Services.AddHttpClient<IPaymentLearningHoldClient, PaymentLearningHoldClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:PaymentReport"] ?? "http://127.0.0.1:5003");
});
builder.Services.AddHttpClient<IPaymentNotificationClient, PaymentNotificationClient>(client =>
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
            var db = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
            db.Database.SetCommandTimeout(5);
            await db.Database.EnsureCreatedAsync();
            await EnsureReviewSchemaAsync(db);
            await EnsureAiAssistantSchemaAsync(db);
            return;
        }
        catch (Exception ex) when (attempt < 3)
        {
            logger.LogWarning(ex, "Student database initialization failed. Retrying {Attempt}/3.", attempt);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Student database is unavailable. Service will continue; database-backed endpoints may return errors until SQL Server is available.");
        }
    }
}

static async Task EnsureReviewSchemaAsync(StudentDbContext db)
{
    await db.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'[dbo].[CourseReviews]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CourseReviews] (
        [Id] uniqueidentifier NOT NULL,
        [EnrollmentId] uniqueidentifier NOT NULL,
        [StudentId] uniqueidentifier NOT NULL,
        [StudentNameSnapshot] nvarchar(200) NOT NULL,
        [CourseId] uniqueidentifier NOT NULL,
        [CourseNameSnapshot] nvarchar(200) NOT NULL,
        [ClassId] uniqueidentifier NOT NULL,
        [ClassNameSnapshot] nvarchar(200) NOT NULL,
        [CourseRating] decimal(3,1) NOT NULL,
        [CourseComment] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CourseReviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CourseReviews_Enrollments_EnrollmentId] FOREIGN KEY ([EnrollmentId]) REFERENCES [dbo].[Enrollments] ([Id])
    );
END

IF OBJECT_ID(N'[dbo].[TeacherReviews]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TeacherReviews] (
        [Id] uniqueidentifier NOT NULL,
        [CourseReviewId] uniqueidentifier NOT NULL,
        [TeacherId] uniqueidentifier NOT NULL,
        [TeacherNameSnapshot] nvarchar(200) NOT NULL,
        [Rating] decimal(3,1) NOT NULL,
        [Comment] nvarchar(1000) NULL,
        CONSTRAINT [PK_TeacherReviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TeacherReviews_CourseReviews_CourseReviewId] FOREIGN KEY ([CourseReviewId]) REFERENCES [dbo].[CourseReviews] ([Id]) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CourseReviews_EnrollmentId' AND object_id = OBJECT_ID(N'[dbo].[CourseReviews]'))
    CREATE UNIQUE INDEX [IX_CourseReviews_EnrollmentId] ON [dbo].[CourseReviews] ([EnrollmentId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CourseReviews_StudentId_ClassId' AND object_id = OBJECT_ID(N'[dbo].[CourseReviews]'))
    CREATE UNIQUE INDEX [IX_CourseReviews_StudentId_ClassId] ON [dbo].[CourseReviews] ([StudentId], [ClassId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CourseReviews_CourseId' AND object_id = OBJECT_ID(N'[dbo].[CourseReviews]'))
    CREATE INDEX [IX_CourseReviews_CourseId] ON [dbo].[CourseReviews] ([CourseId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TeacherReviews_CourseReviewId' AND object_id = OBJECT_ID(N'[dbo].[TeacherReviews]'))
    CREATE INDEX [IX_TeacherReviews_CourseReviewId] ON [dbo].[TeacherReviews] ([CourseReviewId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_TeacherReviews_TeacherId' AND object_id = OBJECT_ID(N'[dbo].[TeacherReviews]'))
    CREATE INDEX [IX_TeacherReviews_TeacherId] ON [dbo].[TeacherReviews] ([TeacherId]);
""");
}

static async Task EnsureAiAssistantSchemaAsync(StudentDbContext db)
{
    await db.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'[dbo].[AiKnowledgeDocuments]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AiKnowledgeDocuments] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [Title] nvarchar(250) NOT NULL,
        [SourceType] nvarchar(40) NOT NULL,
        [Scope] nvarchar(40) NOT NULL,
        [AudienceRole] nvarchar(30) NULL,
        [OwnerReferenceId] uniqueidentifier NULL,
        [ClassId] uniqueidentifier NULL,
        [UploadedByAccountId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL
    );
    CREATE INDEX [IX_AiKnowledgeDocuments_Scope_AudienceRole_OwnerReferenceId_ClassId]
        ON [dbo].[AiKnowledgeDocuments] ([Scope], [AudienceRole], [OwnerReferenceId], [ClassId]);
END
IF OBJECT_ID(N'[dbo].[AiKnowledgeChunks]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AiKnowledgeChunks] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [DocumentId] uniqueidentifier NOT NULL,
        [ChunkIndex] int NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        CONSTRAINT [FK_AiKnowledgeChunks_AiKnowledgeDocuments_DocumentId]
            FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[AiKnowledgeDocuments]([Id]) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX [IX_AiKnowledgeChunks_DocumentId_ChunkIndex]
        ON [dbo].[AiKnowledgeChunks] ([DocumentId], [ChunkIndex]);
END
IF OBJECT_ID(N'[dbo].[AiEmailDrafts]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AiEmailDrafts] (
        [Id] uniqueidentifier NOT NULL PRIMARY KEY,
        [Recipients] nvarchar(500) NOT NULL,
        [Subject] nvarchar(300) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedByAccountId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [SentAt] datetime2 NULL
    );
    CREATE INDEX [IX_AiEmailDrafts_Status_CreatedAt] ON [dbo].[AiEmailDrafts] ([Status], [CreatedAt]);
END
""");
}
