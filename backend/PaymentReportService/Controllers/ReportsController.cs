using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentReportService.Dtos;
using PaymentReportService.Extensions;
using PaymentReportService.Services;

namespace PaymentReportService.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin")]
public sealed class ReportsController(IReportService service) : ControllerBase
{
    [HttpGet("revenue/overview")] public async Task<IActionResult> Overview(CancellationToken ct) => Ok(ApiResponse<RevenueOverviewResponse>.Ok(await service.OverviewAsync(ct)));
    [HttpGet("revenue/by-course")] public async Task<IActionResult> RevenueByCourse(CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<GroupAmountResponse>>.Ok(await service.RevenueByCourseAsync(ct)));
    [HttpGet("revenue/by-class")] public async Task<IActionResult> RevenueByClass(CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<GroupAmountResponse>>.Ok(await service.RevenueByClassAsync(ct)));
    [HttpGet("debt/by-student")] public async Task<IActionResult> DebtByStudent(CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<GroupAmountResponse>>.Ok(await service.DebtByStudentAsync(ct)));
    [HttpGet("debt/by-class")] public async Task<IActionResult> DebtByClass(CancellationToken ct) => Ok(ApiResponse<IReadOnlyList<GroupAmountResponse>>.Ok(await service.DebtByClassAsync(ct)));
    [HttpGet("dashboard")] public async Task<IActionResult> Dashboard(CancellationToken ct) => Ok(ApiResponse<DashboardResponse>.Ok(await service.DashboardAsync(ct)));
    [HttpGet("revenue/by-course/export")]
    public async Task<IActionResult> ExportRevenueByCourse(CancellationToken ct) =>
        this.ToCsvFile(await service.RevenueByCourseAsync(ct), "revenue-by-course.csv");
    [HttpGet("revenue/by-class/export")]
    public async Task<IActionResult> ExportRevenueByClass(CancellationToken ct) =>
        this.ToCsvFile(await service.RevenueByClassAsync(ct), "revenue-by-class.csv");
    [HttpGet("debt/by-student/export")]
    public async Task<IActionResult> ExportDebtByStudent(CancellationToken ct) =>
        this.ToCsvFile(await service.DebtByStudentAsync(ct), "debt-by-student.csv");
    [HttpGet("debt/by-class/export")]
    public async Task<IActionResult> ExportDebtByClass(CancellationToken ct) =>
        this.ToCsvFile(await service.DebtByClassAsync(ct), "debt-by-class.csv");
}
