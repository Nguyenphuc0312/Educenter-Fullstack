using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDataProtection()
    .SetApplicationName("EduCenter")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, ".keys")));

var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddPolicy("EduCenterCors", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddHttpClient("PaymentReport", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:PaymentReport"] ?? "http://127.0.0.1:5003");
});
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseCors("EduCenterCors");
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (!context.User.Identity?.IsAuthenticated ?? true)
    {
        await next();
        return;
    }

    if (IsPublicAuthPath(context.Request.Path))
    {
        await next();
        return;
    }

    var authorization = context.Request.Headers.Authorization.ToString();
    if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    {
        await next();
        return;
    }

    var clients = context.RequestServices.GetRequiredService<IHttpClientFactory>();
    var client = clients.CreateClient("PaymentReport");
    using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authorization["Bearer ".Length..].Trim());

    try
    {
        using var response = await client.SendAsync(request, context.RequestAborted);
        if (!response.IsSuccessStatusCode)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Session is no longer valid", data = (object?)null, errors = (object?)null });
            return;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(context.RequestAborted);
        var payload = await JsonSerializer.DeserializeAsync<AuthMeResponse>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web), context.RequestAborted);
        if (payload?.Data?.Status is "Locked" or "2")
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Account is locked", data = (object?)null, errors = (object?)null });
            return;
        }
    }
    catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
    {
        throw;
    }
    catch
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsJsonAsync(new { success = false, message = "Cannot validate account session", data = (object?)null, errors = (object?)null });
        return;
    }

    await next();
});
app.Map("/health", healthApp => healthApp.Run(async context =>
{
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync("{\"status\":\"ok\",\"service\":\"EduCenter API Gateway\"}");
}));
await app.UseOcelot();

await app.RunAsync();

static bool IsPublicAuthPath(PathString path) =>
    path.StartsWithSegments("/gateway/auth/login", StringComparison.OrdinalIgnoreCase)
    || path.StartsWithSegments("/gateway/auth/register", StringComparison.OrdinalIgnoreCase)
    || path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase);

public sealed record AuthMeResponse(bool Success, AuthMeData? Data);
public sealed record AuthMeData(string Status);
