using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReportService.Dtos;
using PaymentReportService.Extensions;
using PaymentReportService.Services;
using PaymentReportService.Enums;
using System.Security.Claims;

namespace PaymentReportService.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public sealed class PaymentsController(IPaymentTransactionService service) : ControllerBase
{
    [HttpGet, Authorize(Roles = "Admin")] public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<PaymentTransactionResponse>>.Ok(await service.GetAllAsync(ct)));
    [HttpGet("{id:guid}"), Authorize(Roles = "Admin")] public async Task<IActionResult> GetById(Guid id, CancellationToken ct) => Ok(ApiResponse<PaymentTransactionResponse>.Ok(await service.GetByIdAsync(id, ct)));
    [HttpGet("by-student/{studentId:guid}"), Authorize(Roles = "Admin,Student")]
    public async Task<IActionResult> ByStudent(Guid studentId, CancellationToken ct)
    {
        if (User.IsInRole(nameof(UserRole.Student)) && StudentReferenceId() != studentId) return Forbid();
        return Ok(ApiResponse<IReadOnlyList<PaymentTransactionResponse>>.Ok(await service.ByStudentAsync(studentId, ct)));
    }
    [HttpGet("by-invoice/{invoiceId:guid}"), Authorize(Roles = "Admin")] public async Task<IActionResult> ByInvoice(Guid invoiceId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<PaymentTransactionResponse>>.Ok(await service.ByInvoiceAsync(invoiceId, ct)));
    [HttpGet("export"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export(CancellationToken ct) =>
        this.ToCsvFile(await service.GetAllAsync(ct), "payments.csv");
    [HttpPost, Authorize(Roles = "Admin")] public async Task<IActionResult> Create(CreatePaymentRequest request, CancellationToken ct) => Ok(ApiResponse<PaymentTransactionResponse>.Ok(await service.CreateAsync(request, ct), "Created"));
    [HttpPost("student-request"), Authorize(Roles = "Student")]
    public async Task<IActionResult> StudentRequest(StudentPaymentRequest request, CancellationToken ct)
    {
        var studentId = StudentReferenceId();
        if (studentId is null) return Forbid();
        var createdBy = User.FindFirstValue("username") ?? User.Identity?.Name ?? "student";
        return Ok(ApiResponse<PaymentTransactionResponse>.Ok(await service.CreateStudentRequestAsync(request, studentId.Value, createdBy, ct), "Payment request submitted"));
    }
    [HttpPut("{id:guid}/confirm"), Authorize(Roles = "Admin")] public async Task<IActionResult> Confirm(Guid id, CancellationToken ct) => Ok(ApiResponse<PaymentTransactionResponse>.Ok(await service.ConfirmAsync(id, User.Identity?.Name ?? "admin", ct), "Payment confirmed"));
    [HttpPost("bulk-create"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkCreate(List<CreatePaymentRequest> requests, CancellationToken ct)
    {
        var items = new List<PaymentTransactionResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, ct));
        return Ok(ApiResponse<BulkOperationResult<PaymentTransactionResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Bulk created"));
    }
    [HttpPost("import"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
    {
        var requests = await AdminFileExtensions.ReadJsonArrayAsync<CreatePaymentRequest>(file, ct);
        var items = new List<PaymentTransactionResponse>();
        foreach (var request in requests) items.Add(await service.CreateAsync(request, ct));
        return Ok(ApiResponse<BulkOperationResult<PaymentTransactionResponse>>.Ok(new() { Items = items, Requested = requests.Count, Succeeded = items.Count }, "Imported"));
    }
    [HttpPut("{id:guid}/cancel"), Authorize(Roles = "Admin")] public async Task<IActionResult> Cancel(Guid id, CancellationToken ct) => Ok(ApiResponse<PaymentTransactionResponse>.Ok(await service.CancelAsync(id, ct)));
    [HttpPut("bulk-cancel"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkCancel(BulkDeleteRequest request, CancellationToken ct)
    {
        var items = new List<PaymentTransactionResponse>();
        foreach (var id in request.Ids.Distinct()) items.Add(await service.CancelAsync(id, ct));
        return Ok(ApiResponse<BulkOperationResult<PaymentTransactionResponse>>.Ok(new() { Items = items, Requested = request.Ids.Count, Succeeded = items.Count }, "Bulk cancelled"));
    }

    private Guid? StudentReferenceId()
    {
        var value = User.FindFirstValue("referenceId");
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
