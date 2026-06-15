using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReportService.Dtos;
using PaymentReportService.Enums;
using PaymentReportService.Services;
using System.Security.Claims;

namespace PaymentReportService.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService auth, IAccountService accounts) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct) => Ok(ApiResponse<LoginResponse>.Ok(await auth.LoginAsync(request, ct), "Login successful"));

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(CreateAccountRequest request, CancellationToken ct)
    {
        request.Role = UserRole.Student;
        request.ReferenceId = null;
        return Ok(ApiResponse<AccountResponse>.Ok(await auth.RegisterAsync(request, ct), "Registered"));
    }

    [HttpPost("complete-student-profile")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CompleteStudentProfile(CompleteStudentProfileRequest request, CancellationToken ct)
    {
        var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(ApiResponse<LoginResponse>.Ok(await auth.CompleteStudentProfileAsync(id, request, BearerToken(), ct), "Student profile completed"));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(ApiResponse<AccountResponse>.Ok(await accounts.GetByIdAsync(id, ct)));
    }

    [HttpPost("refresh-token")]
    [Authorize]
    public IActionResult RefreshToken() => Ok(ApiResponse<object>.Ok(new { refreshToken = Guid.NewGuid().ToString("N") }, "Refresh token placeholder"));

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout() => Ok(ApiResponse<object>.Ok(null, "Logged out"));

    private string? BearerToken()
    {
        var authorization = Request.Headers.Authorization.ToString();
        return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? authorization["Bearer ".Length..].Trim() : null;
    }
}
