using CourseScheduleService.Dtos;
using CourseScheduleService.Extensions;
using CourseScheduleService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseScheduleService.Controllers;

[ApiController]
[Route("api/rooms")]
public sealed class RoomsController(IRoomService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<RoomResponse>>.Ok(await service.GetAllAsync(cancellationToken)));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(ApiResponse<RoomResponse>.Ok(await service.GetByIdAsync(id, cancellationToken)));

    [HttpGet("{id:guid}/usage")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUsage(Guid id, CancellationToken cancellationToken) =>
        Ok(ApiResponse<RoomDetailResponse>.Ok(await service.GetUsageAsync(id, cancellationToken)));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateRoomRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<RoomResponse>.Ok(result, "Created"));
    }

    [HttpPost("bulk-create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkCreate(List<CreateRoomRequest> requests, CancellationToken cancellationToken)
    {
        var items = new List<RoomResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, cancellationToken));
        return Ok(ApiResponse<BulkOperationResult<RoomResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk created"));
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken cancellationToken)
    {
        var requests = await AdminFileExtensions.ReadJsonArrayAsync<CreateRoomRequest>(file, cancellationToken);
        var items = new List<RoomResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, cancellationToken));
        return Ok(ApiResponse<BulkOperationResult<RoomResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Imported"));
    }

    [HttpGet("export")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export(CancellationToken cancellationToken) =>
        this.ToCsvFile(await service.GetAllAsync(cancellationToken), "rooms.csv");

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateRoomRequest request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<RoomResponse>.Ok(await service.UpdateAsync(id, request, cancellationToken)));

    [HttpPut("bulk-update")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkUpdate(List<BulkUpdateRequest<UpdateRoomRequest>> requests, CancellationToken cancellationToken)
    {
        var items = new List<RoomResponse>();
        foreach (var request in requests) items.Add(await service.UpdateAsync(request.Id, request.Data, cancellationToken));
        return Ok(ApiResponse<BulkOperationResult<RoomResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk updated"));
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
