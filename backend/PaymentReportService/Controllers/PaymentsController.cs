using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReportService.Dtos;
using PaymentReportService.Extensions;
using PaymentReportService.Services;

namespace PaymentReportService.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public sealed class PaymentsController(IPaymentTransactionService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<PaymentTransactionResponse>>.Ok(await service.GetAllAsync(ct)));
    [HttpGet("{id:guid}")] public async Task<IActionResult> GetById(Guid id, CancellationToken ct) => Ok(ApiResponse<PaymentTransactionResponse>.Ok(await service.GetByIdAsync(id, ct)));
    [HttpGet("by-student/{studentId:guid}")] public async Task<IActionResult> ByStudent(Guid studentId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<PaymentTransactionResponse>>.Ok(await service.ByStudentAsync(studentId, ct)));
    [HttpGet("by-invoice/{invoiceId:guid}")] public async Task<IActionResult> ByInvoice(Guid invoiceId, CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<PaymentTransactionResponse>>.Ok(await service.ByInvoiceAsync(invoiceId, ct)));
    [HttpGet("export"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Export(CancellationToken ct) =>
        this.ToCsvFile(await service.GetAllAsync(ct), "payments.csv");
    [HttpPost, Authorize(Roles = "Admin")] public async Task<IActionResult> Create(CreatePaymentRequest request, CancellationToken ct) => Ok(ApiResponse<PaymentTransactionResponse>.Ok(await service.CreateAsync(request, ct), "Created"));
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
}
