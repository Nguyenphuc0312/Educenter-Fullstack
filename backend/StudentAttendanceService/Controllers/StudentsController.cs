using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using StudentAttendanceService.Dtos;
using StudentAttendanceService.Extensions;
using StudentAttendanceService.Services;

namespace StudentAttendanceService.Controllers;

[ApiController]
[Route("api/students")]
[Authorize]
public sealed class StudentsController(IStudentService students, IAttendanceService attendance, IResultService results, IStudentPortalService portal) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<StudentResponse>>.Ok(await students.GetAllAsync(ct)));
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) => Ok(ApiResponse<StudentResponse>.Ok(await students.GetByIdAsync(id, ct)));
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] StudentSearchQuery query, CancellationToken ct) => Ok(ApiResponse<PagedResult<StudentResponse>>.Ok(await students.SearchAsync(query, ct)));
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateStudentRequest request, CancellationToken ct)
    {
        var result = await students.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<StudentResponse>.Ok(result, "Created"));
    }
    [HttpPost("self-profile")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> EnsureSelfProfile(CompleteStudentProfileRequest request, CancellationToken ct)
    {
        Guid? referenceId = null;
        var value = User.FindFirstValue("referenceId");
        if (Guid.TryParse(value, out var parsed)) referenceId = parsed;
        return Ok(ApiResponse<StudentResponse>.Ok(await students.EnsureSelfProfileAsync(request, referenceId, ct), "Student profile ready"));
    }
    [HttpPost("bulk-create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkCreate(List<CreateStudentRequest> requests, CancellationToken ct)
    {
        var items = new List<StudentResponse>();
        foreach (var request in requests) items.Add(await students.CreateAsync(request, ct));
        return Ok(ApiResponse<BulkOperationResult<StudentResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk created"));
    }
    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
    {
        var requests = await AdminFileExtensions.ReadJsonArrayAsync<CreateStudentRequest>(file, ct);
        var items = new List<StudentResponse>();
        foreach (var request in requests) items.Add(await students.CreateAsync(request, ct));
        return Ok(ApiResponse<BulkOperationResult<StudentResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Imported"));
    }
    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export(CancellationToken ct) =>
        this.ToCsvFile(await students.GetAllAsync(ct), "students.csv");
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateStudentRequest request, CancellationToken ct) => Ok(ApiResponse<StudentResponse>.Ok(await students.UpdateAsync(id, request, ct)));
    [HttpPut("bulk-update")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkUpdate(List<BulkUpdateRequest<UpdateStudentRequest>> requests, CancellationToken ct)
    {
        var items = new List<StudentResponse>();
        foreach (var request in requests) items.Add(await students.UpdateAsync(request.Id, request.Data, ct));
        return Ok(ApiResponse<BulkOperationResult<StudentResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk updated"));
    }
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await students.DeleteAsync(id, ct); return Ok(ApiResponse<object>.Ok(null, "Deleted")); }
    [HttpPost("bulk-delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkDelete(BulkDeleteRequest request, CancellationToken ct)
    {
        foreach (var id in request.Ids.Distinct()) await students.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { requested = request.Ids.Count, succeeded = request.Ids.Distinct().Count() }, "Bulk deleted"));
    }
    [HttpGet("{studentId:guid}/attendance-summary")]
    public async Task<IActionResult> AttendanceSummary(Guid studentId, CancellationToken ct) => Ok(ApiResponse<AttendanceSummaryResponse>.Ok(await attendance.StudentSummaryAsync(studentId, ct)));
    [HttpGet("{studentId:guid}/learning-profile")]
    public async Task<IActionResult> LearningProfile(Guid studentId, CancellationToken ct) => Ok(ApiResponse<LearningProfileResponse>.Ok(await portal.LearningProfileAsync(studentId, ct)));
    [HttpGet("{studentId:guid}/my-courses")]
    public async Task<IActionResult> MyCourses(Guid studentId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<MyCourseResponse>>.Ok(await portal.MyCoursesAsync(studentId, ct)));
    [HttpGet("{studentId:guid}/my-attendance")]
    public async Task<IActionResult> MyAttendance(Guid studentId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<AttendanceRecordResponse>>.Ok(await attendance.RecordsByStudentAsync(studentId, ct)));
    [HttpGet("{studentId:guid}/my-results")]
    public async Task<IActionResult> MyResults(Guid studentId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<StudentResultResponse>>.Ok(await results.ByStudentAsync(studentId, ct)));
}
