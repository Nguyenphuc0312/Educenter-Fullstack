using CourseScheduleService.Dtos;
using CourseScheduleService.Entities;
using CourseScheduleService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseScheduleService.Controllers;

[ApiController]
[Route("api/schedule-changes")]
[Authorize]
public sealed class ScheduleChangeRequestsController(IScheduleChangeService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IReadOnlyList<ScheduleChangeResponse>>.Ok(await service.GetAllAsync(ct)));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        Ok(ApiResponse<ScheduleChangeResponse>.Ok(await service.GetByIdAsync(id, ct)));

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Create(CreateScheduleChangeRequest request, CancellationToken ct) =>
        Ok(ApiResponse<ScheduleChangeResponse>.Ok(await service.CreateAsync(request, ct), "Yêu cầu thay đổi lịch học đã được tạo thành công."));

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateScheduleChangeStatusRequest request, CancellationToken ct) =>
        Ok(ApiResponse<ScheduleChangeResponse>.Ok(await service.UpdateStatusAsync(id, request.Status, ct), "Trạng thái yêu cầu thay đổi lịch học đã được cập nhật."));
}
