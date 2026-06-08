using CourseScheduleService.Dtos;
using CourseScheduleService.Extensions;
using CourseScheduleService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseScheduleService.Controllers;

[ApiController]
[Route("api/classes")]
public sealed class ClassesController(IClassManagementService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<ClassResponse>>.Ok(await service.GetAllAsync(cancellationToken)));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(ApiResponse<ClassResponse>.Ok(await service.GetByIdAsync(id, cancellationToken)));

    [HttpGet("by-course/{courseId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> ByCourse(Guid courseId, CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<ClassResponse>>.Ok(await service.ByCourseAsync(courseId, cancellationToken)));

    [HttpGet("by-teacher/{teacherId:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> ByTeacher(Guid teacherId, CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<ClassResponse>>.Ok(await service.ByTeacherAsync(teacherId, cancellationToken)));

    [HttpGet("opening")]
    [AllowAnonymous]
    public async Task<IActionResult> Opening(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<ClassResponse>>.Ok(await service.OpeningAsync(cancellationToken)));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateClassRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ClassResponse>.Ok(result, "Created"));
    }

    [HttpPost("bulk-create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkCreate(List<CreateClassRequest> requests, CancellationToken cancellationToken)
    {
        var items = new List<ClassResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, cancellationToken));
        return Ok(ApiResponse<BulkOperationResult<ClassResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk created"));
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken cancellationToken)
    {
        var requests = await AdminFileExtensions.ReadJsonArrayAsync<CreateClassRequest>(file, cancellationToken);
        var items = new List<ClassResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, cancellationToken));
        return Ok(ApiResponse<BulkOperationResult<ClassResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Imported"));
    }

    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export(CancellationToken cancellationToken) =>
        this.ToCsvFile(await service.GetAllAsync(cancellationToken), "classes.csv");

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateClassRequest request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<ClassResponse>.Ok(await service.UpdateAsync(id, request, cancellationToken)));

    [HttpPut("bulk-update")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkUpdate(List<BulkUpdateRequest<UpdateClassRequest>> requests, CancellationToken cancellationToken)
    {
        var items = new List<ClassResponse>();
        foreach (var request in requests) items.Add(await service.UpdateAsync(request.Id, request.Data, cancellationToken));
        return Ok(ApiResponse<BulkOperationResult<ClassResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk updated"));
    }

    [HttpPut("{id:guid}/increase-student-count")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Increase(Guid id, CancellationToken cancellationToken) =>
        Ok(ApiResponse<ClassResponse>.Ok(await service.IncreaseAsync(id, cancellationToken)));

    [HttpPut("{id:guid}/decrease-student-count")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Decrease(Guid id, CancellationToken cancellationToken) =>
        Ok(ApiResponse<ClassResponse>.Ok(await service.DecreaseAsync(id, cancellationToken)));

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
