using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAttendanceService.Dtos;
using StudentAttendanceService.Services;

namespace StudentAttendanceService.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Teacher")]
public sealed class AttendanceController(IAttendanceService service) : ControllerBase
{
    [HttpGet("api/attendance-sessions/by-class/{classId:guid}")]
    public async Task<IActionResult> SessionsByClass(Guid classId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<AttendanceSessionResponse>>.Ok(await service.SessionsByClassAsync(classId, ct)));
    [HttpGet("api/attendance-sessions/{id:guid}")]
    public async Task<IActionResult> GetSession(Guid id, CancellationToken ct) => Ok(ApiResponse<AttendanceSessionResponse>.Ok(await service.GetSessionAsync(id, ct)));
    [HttpPost("api/attendance-sessions")]
    public async Task<IActionResult> CreateSession(CreateAttendanceSessionRequest request, CancellationToken ct) => Ok(ApiResponse<AttendanceSessionResponse>.Ok(await service.CreateSessionAsync(request, ct), "Created"));
    [HttpPut("api/attendance-sessions/{id:guid}/lock")]
    public async Task<IActionResult> Lock(Guid id, CancellationToken ct) => Ok(ApiResponse<AttendanceSessionResponse>.Ok(await service.LockSessionAsync(id, ct)));
    [HttpGet("api/attendance-records/by-session/{sessionId:guid}")]
    public async Task<IActionResult> RecordsBySession(Guid sessionId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<AttendanceRecordResponse>>.Ok(await service.RecordsBySessionAsync(sessionId, ct)));
    [HttpGet("api/attendance-records/by-student/{studentId:guid}")]
    [Authorize]
    public async Task<IActionResult> RecordsByStudent(Guid studentId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<AttendanceRecordResponse>>.Ok(await service.RecordsByStudentAsync(studentId, ct)));
    [HttpPost("api/attendance-records/bulk")]
    public async Task<IActionResult> Bulk(BulkAttendanceRecordRequest request, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<AttendanceRecordResponse>>.Ok(await service.BulkAsync(request, ct), "Saved"));
    [HttpPut("api/attendance-records/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateAttendanceRecordRequest request, CancellationToken ct) => Ok(ApiResponse<AttendanceRecordResponse>.Ok(await service.UpdateRecordAsync(id, request, ct)));
    [HttpDelete("api/attendance-records/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await service.DeleteRecordAsync(id, ct); return Ok(ApiResponse<object>.Ok(null, "Deleted")); }
}
