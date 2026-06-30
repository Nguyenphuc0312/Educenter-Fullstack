using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using PaymentReportService.Data;
using PaymentReportService.Extensions;
using PaymentReportService.Middlewares;
using PaymentReportService.Repositories;
using PaymentReportService.Seed;
using PaymentReportService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddDataProtection()
    .SetApplicationName("EduCenter")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")));

builder.Services.AddEduCenterApi(builder.Configuration, "EduCenter Payment Report Service");
builder.Services.AddDbContext<PaymentDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("PaymentDB")));
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPaymentTransactionService, PaymentTransactionService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IEmailNotificationService, EmailNotificationService>();
builder.Services.AddHttpClient<IStudentProfileClient, StudentProfileClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:StudentAttendance"] ?? "http://127.0.0.1:5002");
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
            await PaymentDbInitializer.InitializeAsync(scope.ServiceProvider);
            return;
        }
        catch (Exception ex) when (attempt < 3)
        {
            logger.LogWarning(ex, "Payment database initialization failed. Retrying {Attempt}/3.", attempt);
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Payment database is unavailable. Service will continue with limited fallback auth/report data.");
        }
    }
}
