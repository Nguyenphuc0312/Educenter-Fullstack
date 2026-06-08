using CourseScheduleService.Dtos;
using CourseScheduleService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseScheduleService.Controllers;

[ApiController]
public sealed class SummaryController(ICourseCatalogService service) : ControllerBase
{
    [HttpGet("api/course-summary")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CourseSummary(CancellationToken cancellationToken) =>
        Ok(ApiResponse<CourseSummaryResponse>.Ok(await service.GetSummaryAsync(cancellationToken)));

    [HttpGet("api/public/home-summary")]
    [AllowAnonymous]
    public async Task<IActionResult> HomeSummary(CancellationToken cancellationToken) =>
        Ok(ApiResponse<HomeSummaryResponse>.Ok(await service.GetHomeSummaryAsync(cancellationToken)));
}
