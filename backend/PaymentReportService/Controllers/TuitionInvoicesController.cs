using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReportService.Dtos;
using PaymentReportService.Extensions;
using PaymentReportService.Services;
using PaymentReportService.Enums;
using System.Security.Claims;

namespace PaymentReportService.Controllers;

[ApiController]
[Route("api/tuition-invoices")]
[Authorize]
public sealed class TuitionInvoicesController(IInvoiceService service) : ControllerBase
{
    [HttpGet, Authorize(Roles = "Admin")] public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<TuitionInvoiceResponse>>.Ok(await service.GetAllAsync(ct)));
    [HttpGet("{id:guid}"), Authorize(Roles = "Admin")] public async Task<IActionResult> GetById(Guid id, CancellationToken ct) => Ok(ApiResponse<TuitionInvoiceResponse>.Ok(await service.GetByIdAsync(id, ct)));
    [HttpGet("by-student/{studentId:guid}"), Authorize(Roles = "Admin,Student")]
    public async Task<IActionResult> ByStudent(Guid studentId, CancellationToken ct)
    {
        if (User.IsInRole(nameof(UserRole.Student)) && StudentReferenceId() != studentId) return Forbid();
        return Ok(ApiResponse<IReadOnlyList<TuitionInvoiceResponse>>.Ok(await service.ByStudentAsync(studentId, ct)));
    }
    [HttpGet("by-class/{classId:guid}")] public async Task<IActionResult> ByClass(Guid classId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<TuitionInvoiceResponse>>.Ok(await service.ByClassAsync(classId, ct)));
    [HttpGet("debts")] public async Task<IActionResult> Debts(CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<TuitionInvoiceResponse>>.Ok(await service.DebtsAsync(ct)));
    [HttpGet("export"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export(CancellationToken ct) =>
        this.ToCsvFile(await service.GetAllAsync(ct), "tuition-invoices.csv");
    [HttpPost, Authorize(Roles = "Admin")] public async Task<IActionResult> Create(CreateTuitionInvoiceRequest request, CancellationToken ct) => Ok(ApiResponse<TuitionInvoiceResponse>.Ok(await service.CreateAsync(request, ct), "Created"));
    [HttpPost("from-enrollment"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateFromEnrollment(CreateInvoiceFromEnrollmentRequest request, CancellationToken ct) => Ok(ApiResponse<TuitionInvoiceResponse>.Ok(await service.CreateFromEnrollmentAsync(request, ct), "Invoice synced from enrollment"));
    [HttpPost("bulk-create"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkCreate(List<CreateTuitionInvoiceRequest> requests, CancellationToken ct)
    {
        var items = new List<TuitionInvoiceResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, ct));
        return Ok(ApiResponse<BulkOperationResult<TuitionInvoiceResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk created"));
    }
    [HttpPost("import"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
    {
        var requests = await AdminFileExtensions.ReadJsonArrayAsync<CreateTuitionInvoiceRequest>(file, ct);
        var items = new List<TuitionInvoiceResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, ct));
        return Ok(ApiResponse<BulkOperationResult<TuitionInvoiceResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Imported"));
    }
    [HttpPut("{id:guid}"), Authorize(Roles = "Admin")] public async Task<IActionResult> Update(Guid id, UpdateTuitionInvoiceRequest request, CancellationToken ct) => Ok(ApiResponse<TuitionInvoiceResponse>.Ok(await service.UpdateAsync(id, request, ct)));
    [HttpPut("bulk-update"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkUpdate(List<BulkUpdateRequest<UpdateTuitionInvoiceRequest>> requests, CancellationToken ct)
    {
        var items = new List<TuitionInvoiceResponse>();
        foreach (var request in requests) items.Add(await service.UpdateAsync(request.Id, request.Data, ct));
        return Ok(ApiResponse<BulkOperationResult<TuitionInvoiceResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk updated"));
    }
    [HttpPut("{id:guid}/mark-overdue"), Authorize(Roles = "Admin")] public async Task<IActionResult> MarkOverdue(Guid id, CancellationToken ct) => Ok(ApiResponse<TuitionInvoiceResponse>.Ok(await service.MarkOverdueAsync(id, ct)));
    [HttpPut("scan-overdue"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> ScanOverdue(CancellationToken ct) => Ok(ApiResponse<OverdueScanResponse>.Ok(await service.ScanOverdueAsync(ct), "Overdue scan completed"));
    [HttpGet("learning-holds"), Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> LearningHolds([FromQuery] Guid? studentId, [FromQuery] Guid? classId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<LearningHoldResponse>>.Ok(await service.LearningHoldsAsync(studentId, classId, ct)));
    [HttpGet("learning-holds/by-student/{studentId:guid}"), Authorize(Roles = "Admin,Student")]
    public async Task<IActionResult> LearningHoldsByStudent(Guid studentId, CancellationToken ct)
    {
        if (User.IsInRole(nameof(UserRole.Student)) && StudentReferenceId() != studentId) return Forbid();
        return Ok(ApiResponse<IReadOnlyList<LearningHoldResponse>>.Ok(await service.LearningHoldsAsync(studentId, null, ct)));
    }
    [HttpPut("bulk-mark-overdue"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkMarkOverdue(BulkDeleteRequest request, CancellationToken ct)
    {
        var items = new List<TuitionInvoiceResponse>();
        foreach (var id in request.Ids.Distinct()) items.Add(await service.MarkOverdueAsync(id, ct));
        return Ok(ApiResponse<BulkOperationResult<TuitionInvoiceResponse>>.Ok(new() { Items = items, Requested = request.Ids.Count, Succeeded = items.Count }, "Bulk marked overdue"));
    }
    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { await service.DeleteAsync(id, ct); return Ok(ApiResponse<object>.Ok(null, "Deleted")); }
    [HttpPost("bulk-delete"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkDelete(BulkDeleteRequest request, CancellationToken ct)
    {
        foreach (var id in request.Ids.Distinct()) await service.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { requested = request.Ids.Count, succeeded = request.Ids.Distinct().Count() }, "Bulk deleted"));
    }

    private Guid? StudentReferenceId()
    {
        var value = User.FindFirstValue("referenceId");
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
