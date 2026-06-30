using CourseScheduleService.Dtos;
using CourseScheduleService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseScheduleService.Controllers;

[ApiController]
[Route("api/teaching-substitutions")]
public sealed class TeachingSubstitutionsController(ITeachingSubstitutionService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<TeachingSubstitutionResponse>>.Ok(await service.GetAllAsync(cancellationToken)));

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<TeachingSubstitutionResponse>>.Ok(await service.GetPendingAsync(cancellationToken)));

    [HttpGet("by-teacher/{teacherId:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> ByTeacher(Guid teacherId, CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<TeachingSubstitutionResponse>>.Ok(await service.ByTeacherAsync(teacherId, cancellationToken)));

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create(CreateTeachingSubstitutionRequest request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<TeachingSubstitutionResponse>.Ok(await service.CreateAsync(request, cancellationToken), "Created"));

    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid id, ReviewTeachingSubstitutionRequest request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<TeachingSubstitutionResponse>.Ok(await service.ApproveAsync(id, request, BearerToken(), cancellationToken), "Approved"));

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid id, ReviewTeachingSubstitutionRequest request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<TeachingSubstitutionResponse>.Ok(await service.RejectAsync(id, request, BearerToken(), cancellationToken), "Rejected"));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Deleted"));
    }

    private string? BearerToken()
    {
        var authorization = Request.Headers.Authorization.ToString();
        return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? authorization["Bearer ".Length..].Trim() : null;
    }
}
