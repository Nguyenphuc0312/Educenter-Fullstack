using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using StudentAttendanceService.Dtos;
using StudentAttendanceService.Enums;
using StudentAttendanceService.Extensions;
using StudentAttendanceService.Services;

namespace StudentAttendanceService.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize]
public sealed class EnrollmentsController(IEnrollmentService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<EnrollmentResponse>>.Ok(await service.GetAllAsync(ct)));
    [HttpGet("{id:guid}")] public async Task<IActionResult> GetById(Guid id, CancellationToken ct) => Ok(ApiResponse<EnrollmentResponse>.Ok(await service.GetByIdAsync(id, ct)));
    [HttpGet("by-student/{studentId:guid}")] public async Task<IActionResult> ByStudent(Guid studentId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<EnrollmentResponse>>.Ok(await service.ByStudentAsync(studentId, ct)));
    [HttpGet("by-class/{classId:guid}")] public async Task<IActionResult> ByClass(Guid classId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<EnrollmentResponse>>.Ok(await service.ByClassAsync(classId, ct)));
    [HttpPost, Authorize(Roles = "Admin,Student")]
    public async Task<IActionResult> Create(CreateEnrollmentRequest request, CancellationToken ct)
    {
        if (User.IsInRole("Student"))
        {
            var referenceId = User.FindFirstValue("referenceId");
            if (!Guid.TryParse(referenceId, out var studentId) || studentId != request.StudentId) return Forbid();
        }

        return Ok(ApiResponse<EnrollmentResponse>.Ok(await service.CreateAsync(request, ct), "Created"));
    }
    [HttpPost("bulk-create"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkCreate(List<CreateEnrollmentRequest> requests, CancellationToken ct)
    {
        var items = new List<EnrollmentResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, ct));
        return Ok(ApiResponse<BulkOperationResult<EnrollmentResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk created"));
    }
    [HttpPost("import"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
    {
        var requests = await AdminFileExtensions.ReadJsonArrayAsync<CreateEnrollmentRequest>(file, ct);
        var items = new List<EnrollmentResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, ct));
        return Ok(ApiResponse<BulkOperationResult<EnrollmentResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Imported"));
    }
    [HttpGet("export"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export(CancellationToken ct) =>
        this.ToCsvFile(await service.GetAllAsync(ct), "enrollments.csv");
    [HttpPut("{id:guid}/confirm"), Authorize(Roles = "Admin")] public async Task<IActionResult> Confirm(Guid id, CancellationToken ct) => Ok(ApiResponse<EnrollmentResponse>.Ok(await service.SetStatusAsync(id, EnrollmentStatus.Studying, ct)));
    [HttpPut("{id:guid}/cancel"), Authorize(Roles = "Admin")] public async Task<IActionResult> Cancel(Guid id, CancellationToken ct) => Ok(ApiResponse<EnrollmentResponse>.Ok(await service.SetStatusAsync(id, EnrollmentStatus.Cancelled, ct)));
    [HttpPut("{id:guid}/complete"), Authorize(Roles = "Admin,Teacher")] public async Task<IActionResult> Complete(Guid id, CancellationToken ct) => Ok(ApiResponse<EnrollmentResponse>.Ok(await service.SetStatusAsync(id, EnrollmentStatus.Completed, ct)));
    [HttpPut("bulk-confirm"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkConfirm(BulkDeleteRequest request, CancellationToken ct)
    {
        var items = new List<EnrollmentResponse>();
        foreach (var id in request.Ids.Distinct()) items.Add(await service.SetStatusAsync(id, EnrollmentStatus.Studying, ct));
        return Ok(ApiResponse<BulkOperationResult<EnrollmentResponse>>.Ok(new() { Items = items, Requested = request.Ids.Count, Succeeded = items.Count }, "Bulk confirmed"));
    }
    [HttpPut("bulk-cancel"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkCancel(BulkDeleteRequest request, CancellationToken ct)
    {
        var items = new List<EnrollmentResponse>();
        foreach (var id in request.Ids.Distinct()) items.Add(await service.SetStatusAsync(id, EnrollmentStatus.Cancelled, ct));
        return Ok(ApiResponse<BulkOperationResult<EnrollmentResponse>>.Ok(new() { Items = items, Requested = request.Ids.Count, Succeeded = items.Count }, "Bulk cancelled"));
    }
    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await service.DeleteAsync(id, ct); return Ok(ApiResponse<object>.Ok(null, "Deleted")); }
    [HttpPost("bulk-delete"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkDelete(BulkDeleteRequest request, CancellationToken ct)
    {
        foreach (var id in request.Ids.Distinct()) await service.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { requested = request.Ids.Count, succeeded = request.Ids.Distinct().Count() }, "Bulk deleted"));
    }
}
