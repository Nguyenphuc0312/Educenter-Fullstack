using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAttendanceService.Dtos;
using StudentAttendanceService.Services;
using System.Security.Claims;

namespace StudentAttendanceService.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize]
public sealed class ReviewsController(IReviewService reviews) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IReadOnlyList<CourseReviewResponse>>.Ok(await reviews.GetAllAsync(ct)));

    [HttpGet("by-student/{studentId:guid}")]
    public async Task<IActionResult> ByStudent(Guid studentId, CancellationToken ct)
    {
        if (User.IsInRole("Student") && SelfStudentId() != studentId) return Forbid();
        return Ok(ApiResponse<IReadOnlyList<CourseReviewResponse>>.Ok(await reviews.ByStudentAsync(studentId, ct)));
    }

    [HttpGet("by-enrollment/{enrollmentId:guid}")]
    public async Task<IActionResult> ByEnrollment(Guid enrollmentId, CancellationToken ct)
    {
        var review = await reviews.ByEnrollmentAsync(enrollmentId, ct);
        if (User.IsInRole("Student") && review is not null && SelfStudentId() != review.StudentId) return Forbid();
        return Ok(ApiResponse<CourseReviewResponse?>.Ok(review));
    }

    [HttpPost]
    [Authorize(Roles = "Student,Admin")]
    public async Task<IActionResult> Create(CreateCourseReviewRequest request, CancellationToken ct)
    {
        var selfStudentId = User.IsInRole("Student") ? SelfStudentId() : null;
        if (User.IsInRole("Student") && !selfStudentId.HasValue) return Forbid();
        return Ok(ApiResponse<CourseReviewResponse>.Ok(await reviews.CreateAsync(request, selfStudentId, ct), "Created"));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Student,Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateCourseReviewRequest request, CancellationToken ct)
    {
        var selfStudentId = User.IsInRole("Student") ? SelfStudentId() : null;
        if (User.IsInRole("Student") && !selfStudentId.HasValue) return Forbid();
        return Ok(ApiResponse<CourseReviewResponse>.Ok(await reviews.UpdateAsync(id, request, selfStudentId, ct)));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await reviews.DeleteAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(null, "Deleted"));
    }

    private Guid? SelfStudentId() => Guid.TryParse(User.FindFirstValue("referenceId"), out var id) ? id : null;
}
