using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReportService.Dtos;
using PaymentReportService.Extensions;
using PaymentReportService.Services;
using System.Security.Claims;

namespace PaymentReportService.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public sealed class PaymentsController(IPaymentTransactionService service) : ControllerBase
{
    [HttpGet, Authorize(Roles = "Admin")] public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<PaymentTransactionResponse>>.Ok(await service.GetAllAsync(ct)));
    [HttpGet("{id:guid}"), Authorize(Roles = "Admin,Student")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var payment = await service.GetByIdAsync(id, ct);
        if (User.IsInRole("Student") && payment.StudentId != GetReferenceId()) return Forbid();
        return Ok(ApiResponse<PaymentTransactionResponse>.Ok(payment));
    }
    [HttpGet("by-student/{studentId:guid}"), Authorize(Roles = "Admin,Student")]
    public async Task<IActionResult> ByStudent(Guid studentId, CancellationToken ct)
    {
        if (User.IsInRole("Student") && GetReferenceId() != studentId) return Forbid();
        return Ok(ApiResponse<IReadOnlyList<PaymentTransactionResponse>>.Ok(await service.ByStudentAsync(studentId, ct)));
    }
    [HttpGet("by-invoice/{invoiceId:guid}"), Authorize(Roles = "Admin,Student")]
    public async Task<IActionResult> ByInvoice(Guid invoiceId, CancellationToken ct)
    {
        var payments = await service.ByInvoiceAsync(invoiceId, ct);
        if (User.IsInRole("Student") && payments.Any(x => x.StudentId != GetReferenceId())) return Forbid();
        return Ok(ApiResponse<IReadOnlyList<PaymentTransactionResponse>>.Ok(payments));
    }
    [HttpGet("export"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export(CancellationToken ct) =>
        this.ToCsvFile(await service.GetAllAsync(ct), "payments.csv");
    [HttpPost, Authorize(Roles = "Admin")] public async Task<IActionResult> Create(CreatePaymentRequest request, CancellationToken ct) => Ok(ApiResponse<PaymentTransactionResponse>.Ok(await service.CreateAsync(request, ct), "Created"));
    [HttpPost("student-request"), Authorize(Roles = "Student")]
    public async Task<IActionResult> StudentRequest(CreatePaymentRequest request, CancellationToken ct)
    {
        var referenceId = GetReferenceId();
        if (referenceId is null) return Forbid();
        return Ok(ApiResponse<PaymentTransactionResponse>.Ok(await service.CreateStudentRequestAsync(request, referenceId.Value, ct), "Payment submitted"));
    }
    [HttpPost("mock-gateway"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> MockGateway(CreateMockPaymentRequest request, CancellationToken ct) =>
        Ok(ApiResponse<MockPaymentResponse>.Ok(await service.CreateMockPaymentAsync(request, ct), "Mock payment link created"));
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
    [HttpPut("{id:guid}/confirm"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct) => Ok(ApiResponse<PaymentTransactionResponse>.Ok(await service.ConfirmAsync(id, User.Identity?.Name ?? "admin", ct), "Payment confirmed"));
    [HttpPut("bulk-cancel"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkCancel(BulkDeleteRequest request, CancellationToken ct)
    {
        var items = new List<PaymentTransactionResponse>();
        foreach (var id in request.Ids.Distinct()) items.Add(await service.CancelAsync(id, ct));
        return Ok(ApiResponse<BulkOperationResult<PaymentTransactionResponse>>.Ok(new() { Items = items, Requested = request.Ids.Count, Succeeded = items.Count }, "Bulk cancelled"));
    }

    private Guid? GetReferenceId()
    {
        var value = User.FindFirstValue("referenceId");
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
