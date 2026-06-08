using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAttendanceService.Dtos;
using StudentAttendanceService.Services;

namespace StudentAttendanceService.Controllers;

[ApiController]
[Route("api/classes")]
[Authorize]
public sealed class ClassStudentsController(IEnrollmentService enrollments, IAttendanceService attendance) : ControllerBase
{
    [HttpGet("{classId:guid}/students")]
    public async Task<IActionResult> Students(Guid classId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<StudentResponse>>.Ok(await enrollments.ClassStudentsAsync(classId, ct)));
    [HttpGet("{classId:guid}/attendance-summary")]
    public async Task<IActionResult> AttendanceSummary(Guid classId, CancellationToken ct) => Ok(ApiResponse<AttendanceSummaryResponse>.Ok(await attendance.ClassSummaryAsync(classId, ct)));
}
