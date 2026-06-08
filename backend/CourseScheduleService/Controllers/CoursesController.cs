using CourseScheduleService.Dtos;
using CourseScheduleService.Extensions;
using CourseScheduleService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseScheduleService.Controllers;

[ApiController]
[Route("api/courses")]
public sealed class CoursesController(ICourseCatalogService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<CourseResponse>>.Ok(await service.GetAllAsync(cancellationToken)));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(ApiResponse<CourseResponse>.Ok(await service.GetByIdAsync(id, cancellationToken)));

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken) =>
        Ok(ApiResponse<CourseResponse>.Ok(await service.GetBySlugAsync(slug, cancellationToken)));

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] CourseSearchQuery query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<CourseResponse>>.Ok(await service.SearchAsync(query, cancellationToken)));

    [HttpGet("opening")]
    [AllowAnonymous]
    public async Task<IActionResult> Opening(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<CourseResponse>>.Ok(await service.GetOpeningAsync(cancellationToken)));

    [HttpGet("best-selling")]
    [AllowAnonymous]
    public async Task<IActionResult> BestSelling(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<CourseResponse>>.Ok(await service.GetBestSellingAsync(cancellationToken)));

    [HttpGet("popular-this-week")]
    [AllowAnonymous]
    public async Task<IActionResult> Popular(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<CourseResponse>>.Ok(await service.GetPopularAsync(cancellationToken)));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<CourseResponse>.Ok(result, "Created"));
    }

    [HttpPost("bulk-create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkCreate(List<CreateCourseRequest> requests, CancellationToken cancellationToken)
    {
        var items = new List<CourseResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, cancellationToken));
        return Ok(ApiResponse<BulkOperationResult<CourseResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk created"));
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken cancellationToken)
    {
        var requests = await AdminFileExtensions.ReadJsonArrayAsync<CreateCourseRequest>(file, cancellationToken);
        var items = new List<CourseResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, cancellationToken));
        return Ok(ApiResponse<BulkOperationResult<CourseResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Imported"));
    }

    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export(CancellationToken cancellationToken) =>
        this.ToCsvFile(await service.GetAllAsync(cancellationToken), "courses.csv");

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateCourseRequest request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<CourseResponse>.Ok(await service.UpdateAsync(id, request, cancellationToken)));

    [HttpPut("bulk-update")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkUpdate(List<BulkUpdateRequest<UpdateCourseRequest>> requests, CancellationToken cancellationToken)
    {
        var items = new List<CourseResponse>();
        foreach (var request in requests) items.Add(await service.UpdateAsync(request.Id, request.Data, cancellationToken));
        return Ok(ApiResponse<BulkOperationResult<CourseResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk updated"));
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
