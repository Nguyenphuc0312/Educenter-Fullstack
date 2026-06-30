using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAttendanceService.Data;
using StudentAttendanceService.Entities;
using StudentAttendanceService.Services;
using System.Security.Claims;
using UglyToad.PdfPig;

namespace StudentAttendanceService.Controllers;

[ApiController, Route("api/ai/knowledge"), Authorize]
public sealed class AiKnowledgeController(StudentDbContext db, IAiAssistantService ai, IAiFallbackStore fallback) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var reference = ReferenceId();
        var query = db.AiKnowledgeDocuments.AsNoTracking();

        if (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            var normalized = role.Equals("Teacher", StringComparison.OrdinalIgnoreCase) ? "Teacher" : "Student";
            try
            {
                var classIds = await AccessibleClassIdsAsync(normalized, reference, ct);
                query = query.Where(x => x.AudienceRole == null || x.AudienceRole == normalized || x.AudienceRole == "All");
                query = query.Where(x =>
                    x.Scope == "Global" ||
                    x.Scope == "Role" ||
                    (x.Scope == "Account" && x.OwnerReferenceId == reference) ||
                    (x.Scope == "Class" && x.ClassId != null && classIds.Contains(x.ClassId.Value)));
            }
            catch
            {
                var docs = FilterFallbackDocuments(role, reference)
                    .Select(x => new { x.Id, x.Title, x.SourceType, x.Scope, x.AudienceRole, x.OwnerReferenceId, x.ClassId, x.CreatedAt, x.UpdatedAt })
                    .ToList();
                return Ok(docs);
            }
        }

        try
        {
            return Ok(await query
                .OrderByDescending(x => x.UpdatedAt)
                .Select(x => new { x.Id, x.Title, x.SourceType, x.Scope, x.AudienceRole, x.OwnerReferenceId, x.ClassId, x.CreatedAt, x.UpdatedAt })
                .ToListAsync(ct));
        }
        catch
        {
            var docs = FilterFallbackDocuments(role, reference)
                .Select(x => new { x.Id, x.Title, x.SourceType, x.Scope, x.AudienceRole, x.OwnerReferenceId, x.ClassId, x.CreatedAt, x.UpdatedAt })
                .ToList();
            return Ok(docs);
        }
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat(AiChatRequest request, CancellationToken ct)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? throw new UnauthorizedAccessException();
        return Ok(await ai.ChatAsync(role, ReferenceId(), request, ct));
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete(AiCompletionRequest request, CancellationToken ct)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? throw new UnauthorizedAccessException();
        return Ok(await ai.CompleteAsync(role, ReferenceId(), request, ct));
    }

    [HttpPost("email-drafts"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEmailDraft(CreateEmailDraftRequest request, CancellationToken ct)
    {
        return Ok(await ai.CreateEmailDraftAsync(AccountId(), request, ct));
    }

    [HttpGet("email-drafts"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> EmailDrafts(CancellationToken ct)
    {
        try
        {
            return Ok(await db.AiEmailDrafts
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new { x.Id, x.Recipients, x.Subject, x.Body, x.Status, x.CreatedAt, x.SentAt })
                .ToListAsync(ct));
        }
        catch
        {
            return Ok(fallback.EmailDrafts().Select(x => new { x.Id, x.Recipients, x.Subject, x.Body, x.Status, x.CreatedAt, x.SentAt }).ToList());
        }
    }

    [HttpPost("email-drafts/{id:guid}/confirm"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> ConfirmEmail(Guid id, CancellationToken ct)
    {
        var bearer = Request.Headers.Authorization.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
        await ai.ConfirmEmailDraftAsync(id, AccountId(), bearer, ct);
        return NoContent();
    }

    [HttpPost("reports"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateReport(CreateAiReportRequest request, CancellationToken ct)
    {
        return Ok(await ai.CreateReportAsync(request, ct));
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateKnowledgeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Title and content are required.");
        }

        var scope = NormalizeScope(request.Scope);
        var audienceRole = NormalizeAudienceRole(request.AudienceRole);
        var now = DateTime.UtcNow;
        var doc = new AiKnowledgeDocument
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            SourceType = "Text",
            Scope = scope,
            AudienceRole = audienceRole,
            OwnerReferenceId = request.OwnerReferenceId,
            ClassId = request.ClassId,
            UploadedByAccountId = AccountId(),
            CreatedAt = now,
            UpdatedAt = now
        };

        var chunks = ChunkText(request.Content, 1800)
            .Select((x, i) => new AiKnowledgeChunk { Id = Guid.NewGuid(), DocumentId = doc.Id, ChunkIndex = i, Content = x })
            .ToList();
        try
        {
            db.AiKnowledgeDocuments.Add(doc);
            db.AiKnowledgeChunks.AddRange(chunks);
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            fallback.AddDocument(doc, chunks);
        }
        return Ok(new { doc.Id, chunkCount = chunks.Count });
    }

    [HttpPost("upload"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] string? scope,
        [FromForm] string? audienceRole,
        [FromForm] Guid? ownerReferenceId,
        [FromForm] Guid? classId,
        CancellationToken ct)
    {
        if (file.Length == 0 || file.Length > 10_000_000) return BadRequest("File must be between 1 byte and 10 MB.");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not ".txt" and not ".md" and not ".docx" and not ".pdf") return BadRequest("Supported formats: PDF, DOCX, TXT, Markdown.");

        var content = await ExtractTextAsync(file, extension, ct);
        if (string.IsNullOrWhiteSpace(content)) return BadRequest("No readable text was found in the uploaded document.");

        var create = new CreateKnowledgeRequest(Path.GetFileNameWithoutExtension(file.FileName), content, scope, audienceRole, ownerReferenceId, classId);
        return await Create(create, ct);
    }

    private Guid AccountId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(raw, out var id)) throw new AppException("Invalid account token", 401);
        return id;
    }

    private Guid? ReferenceId() => Guid.TryParse(User.FindFirstValue("referenceId"), out var id) ? id : null;

    private async Task<List<Guid>> AccessibleClassIdsAsync(string normalizedRole, Guid? referenceId, CancellationToken ct)
    {
        if (referenceId is null) return [];
        if (normalizedRole == "Student")
        {
            return await db.Enrollments
                .Where(x => x.StudentId == referenceId && x.Status != Enums.EnrollmentStatus.Cancelled)
                .Select(x => x.ClassId)
                .Distinct()
                .ToListAsync(ct);
        }

        var attendanceClassIds = db.AttendanceSessions.Where(x => x.CreatedByTeacherId == referenceId).Select(x => x.ClassId);
        var resultClassIds = db.StudentResults.Where(x => x.EvaluatedByTeacherId == referenceId).Select(x => x.ClassId);
        return await attendanceClassIds.Concat(resultClassIds).Distinct().ToListAsync(ct);
    }

    private static string NormalizeScope(string? scope)
    {
        var value = string.IsNullOrWhiteSpace(scope) ? "Role" : scope.Trim();
        return value is "Global" or "Role" or "Account" or "Class" ? value : "Role";
    }

    private static string? NormalizeAudienceRole(string? audienceRole)
    {
        if (string.IsNullOrWhiteSpace(audienceRole) || audienceRole == "All") return "All";
        return audienceRole.Trim() is "Admin" or "Teacher" or "Student" ? audienceRole.Trim() : "All";
    }

    private static IEnumerable<string> ChunkText(string text, int size)
    {
        for (var i = 0; i < text.Length; i += size)
        {
            yield return text.Substring(i, Math.Min(size, text.Length - i));
        }
    }

    private static async Task<string> ExtractTextAsync(IFormFile file, string extension, CancellationToken ct)
    {
        if (extension is ".txt" or ".md")
        {
            using var reader = new StreamReader(file.OpenReadStream());
            return await reader.ReadToEndAsync(ct);
        }

        await using var stream = file.OpenReadStream();
        if (extension == ".docx")
        {
            using var document = WordprocessingDocument.Open(stream, false);
            return document.MainDocumentPart?.Document.Body?.InnerText ?? string.Empty;
        }

        using var pdf = PdfDocument.Open(stream);
        return string.Join("\n", pdf.GetPages().Select(page => page.Text));
    }

    private IReadOnlyList<AiKnowledgeDocument> FilterFallbackDocuments(string role, Guid? reference)
    {
        if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase)) return fallback.Documents();
        var normalized = role.Equals("Teacher", StringComparison.OrdinalIgnoreCase) ? "Teacher" : "Student";
        return fallback.Documents()
            .Where(x => x.AudienceRole is null || x.AudienceRole == normalized || x.AudienceRole == "All")
            .Where(x =>
                x.Scope == "Global" ||
                x.Scope == "Role" ||
                (x.Scope == "Account" && x.OwnerReferenceId == reference))
            .ToList();
    }
}

public sealed record CreateKnowledgeRequest(string Title, string Content, string? Scope, string? AudienceRole, Guid? OwnerReferenceId, Guid? ClassId);
