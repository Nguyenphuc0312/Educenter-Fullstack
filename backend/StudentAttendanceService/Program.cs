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
            var db = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
            await db.Database.EnsureCreatedAsync();
            return;
        }
        catch when (attempt < 15)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}
