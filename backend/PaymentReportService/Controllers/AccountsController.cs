using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReportService.Dtos;
using PaymentReportService.Enums;
using PaymentReportService.Extensions;
using PaymentReportService.Services;

namespace PaymentReportService.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize(Roles = "Admin")]
public sealed class AccountsController(IAccountService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<AccountResponse>>.Ok(await service.GetAllAsync(ct)));
    [HttpGet("{id:guid}")] public async Task<IActionResult> GetById(Guid id, CancellationToken ct) => Ok(ApiResponse<AccountResponse>.Ok(await service.GetByIdAsync(id, ct)));
    [HttpPost] public async Task<IActionResult> Create(CreateAccountRequest request, CancellationToken ct) => Ok(ApiResponse<AccountResponse>.Ok(await service.CreateAsync(request, ct), "Created"));
    [HttpPost("bulk-create")]
    public async Task<IActionResult> BulkCreate(List<CreateAccountRequest> requests, CancellationToken ct)
    {
        var items = new List<AccountResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, ct));
        return Ok(ApiResponse<BulkOperationResult<AccountResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk created"));
    }
    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
    {
        var requests = await AdminFileExtensions.ReadJsonArrayAsync<CreateAccountRequest>(file, ct);
        var items = new List<AccountResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, ct));
        return Ok(ApiResponse<BulkOperationResult<AccountResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Imported"));
    }
    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken ct) =>
        this.ToCsvFile(await service.GetAllAsync(ct), "accounts.csv");
    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id, UpdateAccountRequest request, CancellationToken ct) => Ok(ApiResponse<AccountResponse>.Ok(await service.UpdateAsync(id, request, ct)));
    [HttpPut("bulk-update")]
    public async Task<IActionResult> BulkUpdate(List<BulkUpdateRequest<UpdateAccountRequest>> requests, CancellationToken ct)
    {
        var items = new List<AccountResponse>();
        foreach (var request in requests) items.Add(await service.UpdateAsync(request.Id, request.Data, ct));
        return Ok(ApiResponse<BulkOperationResult<AccountResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk updated"));
    }
    [HttpPut("{id:guid}/lock")] public async Task<IActionResult> Lock(Guid id, CancellationToken ct) => Ok(ApiResponse<AccountResponse>.Ok(await service.SetStatusAsync(id, AccountStatus.Locked, ct)));
    [HttpPut("{id:guid}/unlock")] public async Task<IActionResult> Unlock(Guid id, CancellationToken ct) => Ok(ApiResponse<AccountResponse>.Ok(await service.SetStatusAsync(id, AccountStatus.Active, ct)));
    [HttpPut("bulk-lock")]
    public async Task<IActionResult> BulkLock(BulkDeleteRequest request, CancellationToken ct)
    {
        var items = new List<AccountResponse>();
        foreach (var id in request.Ids.Distinct()) items.Add(await service.SetStatusAsync(id, AccountStatus.Locked, ct));
        return Ok(ApiResponse<BulkOperationResult<AccountResponse>>.Ok(new() { Items = items, Requested = request.Ids.Count, Succeeded = items.Count }, "Bulk locked"));
    }
    [HttpPut("bulk-unlock")]
    public async Task<IActionResult> BulkUnlock(BulkDeleteRequest request, CancellationToken ct)
    {
        var items = new List<AccountResponse>();
        foreach (var id in request.Ids.Distinct()) items.Add(await service.SetStatusAsync(id, AccountStatus.Active, ct));
        return Ok(ApiResponse<BulkOperationResult<AccountResponse>>.Ok(new() { Items = items, Requested = request.Ids.Count, Succeeded = items.Count }, "Bulk unlocked"));
    }
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await service.DeleteAsync(id, ct); return Ok(ApiResponse<object>.Ok(null, "Deleted")); }
    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDelete(BulkDeleteRequest request, CancellationToken ct)
    {
        foreach (var id in request.Ids.Distinct()) await service.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { requested = request.Ids.Count, succeeded = request.Ids.Distinct().Count() }, "Bulk deleted"));
    }
}
