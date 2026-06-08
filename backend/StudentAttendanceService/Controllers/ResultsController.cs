using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAttendanceService.Dtos;
using StudentAttendanceService.Extensions;
using StudentAttendanceService.Services;

namespace StudentAttendanceService.Controllers;

[ApiController]
[Route("api/results")]
[Authorize]
public sealed class ResultsController(IResultService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<StudentResultResponse>>.Ok(await service.GetAllAsync(ct)));
    [HttpGet("{id:guid}")] public async Task<IActionResult> GetById(Guid id, CancellationToken ct) => Ok(ApiResponse<StudentResultResponse>.Ok(await service.GetByIdAsync(id, ct)));
    [HttpGet("by-student/{studentId:guid}")] public async Task<IActionResult> ByStudent(Guid studentId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<StudentResultResponse>>.Ok(await service.ByStudentAsync(studentId, ct)));
    [HttpGet("by-class/{classId:guid}")] public async Task<IActionResult> ByClass(Guid classId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<StudentResultResponse>>.Ok(await service.ByClassAsync(classId, ct)));
    [HttpPost, Authorize(Roles = "Admin,Teacher")] public async Task<IActionResult> Create(CreateStudentResultRequest request, CancellationToken ct) => Ok(ApiResponse<StudentResultResponse>.Ok(await service.CreateAsync(request, ct), "Created"));
    [HttpPost("bulk-create"), Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> BulkCreate(List<CreateStudentResultRequest> requests, CancellationToken ct)
    {
        var items = new List<StudentResultResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, ct));
        return Ok(ApiResponse<BulkOperationResult<StudentResultResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk created"));
    }
    [HttpPost("import"), Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
    {
        var requests = await AdminFileExtensions.ReadJsonArrayAsync<CreateStudentResultRequest>(file, ct);
        var items = new List<StudentResultResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, ct));
        return Ok(ApiResponse<BulkOperationResult<StudentResultResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Imported"));
    }
    [HttpGet("export"), Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Export(CancellationToken ct) =>
        this.ToCsvFile(await service.GetAllAsync(ct), "student-results.csv");
    [HttpPut("{id:guid}"), Authorize(Roles = "Admin,Teacher")] public async Task<IActionResult> Update(Guid id, UpdateStudentResultRequest request, CancellationToken ct) => Ok(ApiResponse<StudentResultResponse>.Ok(await service.UpdateAsync(id, request, ct)));
    [HttpPut("bulk-update"), Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> BulkUpdate(List<BulkUpdateRequest<UpdateStudentResultRequest>> requests, CancellationToken ct)
    {
        var items = new List<StudentResultResponse>();
        foreach (var request in requests) items.Add(await service.UpdateAsync(request.Id, request.Data, ct));
        return Ok(ApiResponse<BulkOperationResult<StudentResultResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk updated"));
    }
    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await service.DeleteAsync(id, ct); return Ok(ApiResponse<object>.Ok(null, "Deleted")); }
    [HttpPost("bulk-delete"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkDelete(BulkDeleteRequest request, CancellationToken ct)
    {
        foreach (var id in request.Ids.Distinct()) await service.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { requested = request.Ids.Count, succeeded = request.Ids.Distinct().Count() }, "Bulk deleted"));
    }
}
