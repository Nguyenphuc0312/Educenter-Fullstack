using CourseScheduleService.Dtos;
using CourseScheduleService.Extensions;
using CourseScheduleService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseScheduleService.Controllers;

[ApiController]
[Route("api/schedules")]
public sealed class SchedulesController(IScheduleManagementService service) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<ScheduleResponse>>.Ok(await service.GetAllAsync(cancellationToken)));

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(ApiResponse<ScheduleResponse>.Ok(await service.GetByIdAsync(id, cancellationToken)));

    [HttpGet("by-class/{classId:guid}")]
    [Authorize]
    public async Task<IActionResult> ByClass(Guid classId, CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<ScheduleResponse>>.Ok(await service.ByClassAsync(classId, cancellationToken)));

    [HttpGet("by-teacher/{teacherId:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> ByTeacher(Guid teacherId, CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<ScheduleResponse>>.Ok(await service.ByTeacherAsync(teacherId, cancellationToken)));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateScheduleRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ScheduleResponse>.Ok(result, "Created"));
    }

    [HttpPost("bulk-create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkCreate(List<CreateScheduleRequest> requests, CancellationToken cancellationToken)
    {
        var items = new List<ScheduleResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, cancellationToken));
        return Ok(ApiResponse<BulkOperationResult<ScheduleResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk created"));
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken cancellationToken)
    {
        var requests = await AdminFileExtensions.ReadJsonArrayAsync<CreateScheduleRequest>(file, cancellationToken);
        var items = new List<ScheduleResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, cancellationToken));
        return Ok(ApiResponse<BulkOperationResult<ScheduleResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Imported"));
    }

    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export(CancellationToken cancellationToken) =>
        this.ToCsvFile(await service.GetAllAsync(cancellationToken), "schedules.csv");

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateScheduleRequest request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<ScheduleResponse>.Ok(await service.UpdateAsync(id, request, cancellationToken)));

    [HttpPut("bulk-update")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkUpdate(List<BulkUpdateRequest<UpdateScheduleRequest>> requests, CancellationToken cancellationToken)
    {
        var items = new List<ScheduleResponse>();
        foreach (var request in requests) items.Add(await service.UpdateAsync(request.Id, request.Data, cancellationToken));
        return Ok(ApiResponse<BulkOperationResult<ScheduleResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk updated"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Deleted"));
    }

    [HttpPost("bulk-delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkDelete(BulkDeleteRequest request, CancellationToken cancellationToken)
    {
        foreach (var id in request.Ids.Distinct()) await service.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { requested = request.Ids.Count, succeeded = request.Ids.Distinct().Count() }, "Bulk deleted"));
    }
}
