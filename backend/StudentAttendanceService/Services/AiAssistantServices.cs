using Microsoft.EntityFrameworkCore;
using StudentAttendanceService.Data;
using StudentAttendanceService.Entities;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StudentAttendanceService.Services;

public sealed record AiChatMessage(string Role, string Content);
public sealed record AiChatResponse(string Answer, IReadOnlyList<AiCitation> Citations);
public sealed record AiCitation(Guid DocumentId, string Title, string Excerpt);
public sealed record EmailDraftResponse(Guid Id, string Recipients, string Subject, string Body, string Status, DateTime CreatedAt);
public sealed record AiReportResponse(string Title, string Markdown, string Csv, DateTime CreatedAt);
public sealed record AiCompletionResponse(string Text);

public sealed class AiChatRequest
{
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<AiChatMessage>? History { get; init; }
}

public sealed class CreateEmailDraftRequest
{
    public string Recipients { get; init; } = string.Empty;
    public string Instruction { get; init; } = string.Empty;
}

public sealed class CreateAiReportRequest
{
    public string Instruction { get; init; } = string.Empty;
}

public sealed class AiCompletionRequest
{
    public string Prompt { get; init; } = string.Empty;
    public string? System { get; init; }
    public IReadOnlyList<AiChatMessage>? History { get; init; }
    public bool JsonMode { get; init; }
    public int? MaxOutputTokens { get; init; }
}

public interface IAiAssistantService
{
    Task<AiChatResponse> ChatAsync(string role, Guid? referenceId, AiChatRequest request, CancellationToken ct);
    Task<EmailDraftResponse> CreateEmailDraftAsync(Guid accountId, CreateEmailDraftRequest request, CancellationToken ct);
    Task ConfirmEmailDraftAsync(Guid draftId, Guid accountId, string bearerToken, CancellationToken ct);
    Task<AiReportResponse> CreateReportAsync(CreateAiReportRequest request, CancellationToken ct);
    Task<AiCompletionResponse> CompleteAsync(string role, Guid? referenceId, AiCompletionRequest request, CancellationToken ct);
}

public interface IAiFallbackStore
{
    IReadOnlyList<AiKnowledgeChunk> Chunks();
    IReadOnlyList<AiKnowledgeDocument> Documents();
    IReadOnlyList<AiEmailDraft> EmailDrafts();
    void AddDocument(AiKnowledgeDocument document, IReadOnlyList<AiKnowledgeChunk> chunks);
    void AddEmailDraft(AiEmailDraft draft);
    AiEmailDraft? EmailDraft(Guid id, Guid accountId);
}

public sealed class AiFallbackStore : IAiFallbackStore
{
    private readonly ConcurrentDictionary<Guid, AiKnowledgeDocument> documents = new();
    private readonly ConcurrentDictionary<Guid, AiKnowledgeChunk> chunks = new();
    private readonly ConcurrentDictionary<Guid, AiEmailDraft> drafts = new();

    public IReadOnlyList<AiKnowledgeChunk> Chunks()
    {
        var docs = documents;
        return chunks.Values
            .Select(x =>
            {
                x.Document = docs.GetValueOrDefault(x.DocumentId);
                return x;
            })
            .OrderBy(x => x.DocumentId)
            .ThenBy(x => x.ChunkIndex)
            .ToList();
    }

    public IReadOnlyList<AiKnowledgeDocument> Documents() => documents.Values.OrderByDescending(x => x.UpdatedAt).ToList();
    public IReadOnlyList<AiEmailDraft> EmailDrafts() => drafts.Values.OrderByDescending(x => x.CreatedAt).ToList();

    public void AddDocument(AiKnowledgeDocument document, IReadOnlyList<AiKnowledgeChunk> documentChunks)
    {
        documents[document.Id] = document;
        foreach (var chunk in documentChunks) chunks[chunk.Id] = chunk;
    }

    public void AddEmailDraft(AiEmailDraft draft) => drafts[draft.Id] = draft;
    public AiEmailDraft? EmailDraft(Guid id, Guid accountId) => drafts.TryGetValue(id, out var draft) && draft.CreatedByAccountId == accountId ? draft : null;
}

public sealed class AiAssistantService(StudentDbContext db, IHttpClientFactory clients, IConfiguration config, IAiFallbackStore fallback) : IAiAssistantService
{
    public async Task<AiChatResponse> ChatAsync(string role, Guid? referenceId, AiChatRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message)) throw new AppException("Message is required", 400);

        var chunks = await AccessibleChunks(role, referenceId, ct);
        var ranked = Rank(chunks, request.Message).Take(6).ToList();
        var context = ranked.Count == 0
            ? "Không có tài liệu phù hợp."
            : string.Join("\n\n", ranked.Select(x => $"[Tài liệu: {x.Document!.Title}]\n{x.Content}"));

        var system =
            $"Bạn là trợ lý EduCenter cho vai trò {role}. " +
            "Chỉ trả lời bằng thông tin được cấp và từ chối yêu cầu vượt quyền. " +
            "Không tiết lộ dữ liệu tài khoản khác hoặc role khác. " +
            "Nếu thiếu dữ liệu, nói rõ không có thông tin. Trả lời tiếng Việt, ngắn gọn.\n\n" +
            $"NGỮ CẢNH ĐƯỢC PHÉP:\n{context}";

        var input = new List<object> { new { role = "developer", content = system } };
        foreach (var item in request.History?.TakeLast(10) ?? [])
        {
            input.Add(new { role = item.Role == "model" ? "assistant" : "user", content = item.Content });
        }
        input.Add(new { role = "user", content = request.Message.Trim() });

        var answer = await AskRouterAsync(input, ct);
        var citations = ranked
            .Select(x => new AiCitation(x.DocumentId, x.Document!.Title, x.Content[..Math.Min(220, x.Content.Length)]))
            .ToList();
        return new AiChatResponse(answer, citations);
    }

    public async Task<EmailDraftResponse> CreateEmailDraftAsync(Guid accountId, CreateEmailDraftRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Recipients) || string.IsNullOrWhiteSpace(request.Instruction))
        {
            throw new AppException("Recipients and instruction are required", 400);
        }

        var prompt = new List<object>
        {
            new
            {
                role = "developer",
                content = "Soạn email thông báo EduCenter bằng tiếng Việt. Trả về đúng định dạng: SUBJECT: <tiêu đề>\nBODY: <nội dung>. Không tự gửi email."
            },
            new { role = "user", content = request.Instruction }
        };
        var generated = await AskRouterAsync(prompt, ct);
        var split = generated.Split("BODY:", 2, StringSplitOptions.TrimEntries);
        var subject = split[0].Replace("SUBJECT:", "", StringComparison.OrdinalIgnoreCase).Trim();
        var body = split.Length > 1 ? split[1].Trim() : generated;

        var draft = new AiEmailDraft
        {
            Id = Guid.NewGuid(),
            Recipients = request.Recipients.Trim(),
            Subject = string.IsNullOrWhiteSpace(subject) ? "Thông báo từ EduCenter" : subject,
            Body = body,
            Status = "Draft",
            CreatedByAccountId = accountId,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            db.AiEmailDrafts.Add(draft);
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            fallback.AddEmailDraft(draft);
        }
        return ToResponse(draft);
    }

    public async Task ConfirmEmailDraftAsync(Guid draftId, Guid accountId, string bearerToken, CancellationToken ct)
    {
        AiEmailDraft? draft;
        try
        {
            draft = await db.AiEmailDrafts.SingleOrDefaultAsync(x => x.Id == draftId && x.CreatedByAccountId == accountId, ct);
        }
        catch
        {
            draft = fallback.EmailDraft(draftId, accountId);
        }
        draft ??= fallback.EmailDraft(draftId, accountId);
        if (draft is null) throw new AppException("Draft not found", 404);
        if (draft.Status != "Draft") throw new AppException("Draft has already been processed", 409);

        var emails = draft.Recipients
            .Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (emails.Count == 0) throw new AppException("No valid recipients", 400);

        var client = clients.CreateClient("PaymentNotification");
        foreach (var email in emails)
        {
            using var msg = new HttpRequestMessage(HttpMethod.Post, "/api/notifications/email")
            {
                Content = JsonContent.Create(new { toEmail = email, subject = draft.Subject, body = draft.Body })
            };
            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            using var response = await client.SendAsync(msg, ct);
            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync(ct);
                throw new AppException(
                    string.IsNullOrWhiteSpace(detail)
                        ? "Email service rejected the notification"
                        : $"Email service rejected the notification: {detail}",
                    502);
            }
        }

        draft.Status = "Sent";
        draft.SentAt = DateTime.UtcNow;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            // Fallback drafts are already updated in memory.
        }
    }

    public async Task<AiReportResponse> CreateReportAsync(CreateAiReportRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Instruction)) throw new AppException("Instruction is required", 400);

        var studentCount = 0;
        var enrollmentCount = 0;
        var activeEnrollmentCount = 0;
        var attendanceCount = 0;
        var presentCount = 0;
        var resultCount = 0;
        var passedCount = 0;
        var averageScore = 0m;
        var topClasses = new List<(string ClassName, int Total)>();

        try
        {
            studentCount = await db.Students.CountAsync(ct);
            enrollmentCount = await db.Enrollments.CountAsync(ct);
            activeEnrollmentCount = await db.Enrollments.CountAsync(x => x.Status != Enums.EnrollmentStatus.Cancelled, ct);
            attendanceCount = await db.AttendanceRecords.CountAsync(ct);
            presentCount = await db.AttendanceRecords.CountAsync(x => x.Status == Enums.AttendanceStatus.Present, ct);
            resultCount = await db.StudentResults.CountAsync(ct);
            passedCount = await db.StudentResults.CountAsync(x => x.ResultStatus == Enums.ResultStatus.Passed, ct);
            averageScore = await db.StudentResults.AnyAsync(ct) ? await db.StudentResults.AverageAsync(x => x.AverageScore, ct) : 0;
            topClasses = await db.Enrollments
                .GroupBy(x => x.ClassNameSnapshot)
                .Select(g => new ValueTuple<string, int>(g.Key, g.Count()))
                .OrderByDescending(x => x.Item2)
                .Take(8)
                .ToListAsync(ct);
        }
        catch
        {
            // Keep report generation available in demo/offline mode with zeroed metrics.
        }

        var csv = new StringBuilder();
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Students,{studentCount}");
        csv.AppendLine($"Enrollments,{enrollmentCount}");
        csv.AppendLine($"ActiveEnrollments,{activeEnrollmentCount}");
        csv.AppendLine($"AttendanceRecords,{attendanceCount}");
        csv.AppendLine($"PresentRecords,{presentCount}");
        csv.AppendLine($"StudentResults,{resultCount}");
        csv.AppendLine($"PassedResults,{passedCount}");
        csv.AppendLine($"AverageScore,{averageScore:0.##}");
        foreach (var row in topClasses) csv.AppendLine($"Class:{row.ClassName},{row.Total}");

        var facts = csv.ToString();
        var prompt = new List<object>
        {
            new
            {
                role = "developer",
                content = "Bạn là trợ lý báo cáo EduCenter cho Admin. Dựa trên số liệu CSV, viết báo cáo Markdown tiếng Việt có: tóm tắt, nhận định, rủi ro, đề xuất hành động. Không bịa số liệu ngoài CSV."
            },
            new { role = "user", content = $"Yêu cầu báo cáo: {request.Instruction}\n\nCSV:\n{facts}" }
        };
        var markdown = await AskRouterAsync(prompt, ct);
        return new AiReportResponse("Báo cáo AI EduCenter", markdown, facts, DateTime.UtcNow);
    }

    public async Task<AiCompletionResponse> CompleteAsync(string role, Guid? referenceId, AiCompletionRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt)) throw new AppException("Prompt is required", 400);

        var guard =
            $"Bạn là trợ lý AI EduCenter cho vai trò {role}. " +
            "Chỉ hỗ trợ đúng tác vụ trong hệ thống EduCenter. " +
            "Kh�ng ti?t l? d? li?u t�i kho?n/role kh�c, kh�ng b? qua ph�n quy?n, kh�ng t? g?i email hay t? th?c hi?n thao t�c thay ngu?i d�ng. " +
            "N?u thi?u d? li?u ho?c y�u c?u vu?t quy?n, h�y n�i r� gi?i h?n.";

        if (request.JsonMode)
        {
            guard += " Phản hồi phải là JSON hợp lệ, không bọc markdown, không thêm giải thích ngoài JSON.";
        }

        var input = new List<object>
        {
            new { role = "developer", content = $"{guard}\n\n{request.System?.Trim()}".Trim() }
        };

        foreach (var item in request.History?.TakeLast(12) ?? [])
        {
            input.Add(new { role = item.Role == "model" ? "assistant" : "user", content = item.Content });
        }

        input.Add(new { role = "user", content = request.Prompt.Trim() });
        var answer = await AskRouterAsync(input, ct, request.MaxOutputTokens is > 0 ? request.MaxOutputTokens.Value : 1600);
        return new AiCompletionResponse(answer);
    }

    private async Task<List<AiKnowledgeChunk>> AccessibleChunks(string role, Guid? referenceId, CancellationToken ct)
    {
        try
        {
            var query = db.AiKnowledgeChunks.Include(x => x.Document).AsNoTracking();
            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase)) return await query.ToListAsync(ct);

            var normalized = role.Equals("Teacher", StringComparison.OrdinalIgnoreCase) ? "Teacher" : "Student";
            var classIds = await AccessibleClassIdsAsync(normalized, referenceId, ct);
            query = query.Where(x => x.Document!.AudienceRole == null || x.Document.AudienceRole == normalized || x.Document.AudienceRole == "All");
            query = query.Where(x =>
                x.Document!.Scope == "Global" ||
                x.Document.Scope == "Role" ||
                (x.Document.Scope == "Account" && x.Document.OwnerReferenceId == referenceId) ||
                (x.Document.Scope == "Class" && x.Document.ClassId != null && classIds.Contains(x.Document.ClassId.Value)));
            return await query.ToListAsync(ct);
        }
        catch
        {
            return FilterFallbackChunks(role, referenceId);
        }
    }

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

        var attendanceClassIds = db.AttendanceSessions
            .Where(x => x.CreatedByTeacherId == referenceId)
            .Select(x => x.ClassId);
        var resultClassIds = db.StudentResults
            .Where(x => x.EvaluatedByTeacherId == referenceId)
            .Select(x => x.ClassId);
        return await attendanceClassIds.Concat(resultClassIds).Distinct().ToListAsync(ct);
    }

    private async Task<string> AskRouterAsync(object input, CancellationToken ct, int maxOutputTokens = 1200)
    {
        var key = config["AiRouter:ApiKey"] ?? Environment.GetEnvironmentVariable("EDUCENTER_AI_ROUTER_API_KEY");
        if (string.IsNullOrWhiteSpace(key)) throw new AppException("AI Router is not configured on the server", 503);

        var client = clients.CreateClient("AiRouter");
        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        request.Headers.TryAddWithoutValidation("HTTP-Referer", "http://103.72.96.117");
        request.Headers.TryAddWithoutValidation("X-Title", "EduCenter");
        request.Content = JsonContent.Create(new
        {
            model = config["AiRouter:Model"] ?? "openai/gpt-4o-mini",
            messages = NormalizeMessages(input),
            max_tokens = maxOutputTokens
        });

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, ct);
        }
        catch (HttpRequestException ex)
        {
            throw new AppException($"Không thể kết nối AI Router ({ex.Message}). Vui lòng kiểm tra AiRouter:BaseUrl hoặc cấu hình DNS/server.", StatusCodes.Status503ServiceUnavailable);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new AppException("AI Router ph?n h?i qu� l�u. Vui l�ng th? l?i sau.", StatusCodes.Status504GatewayTimeout);
        }

        using (response)
        {
            var raw = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode) throw new AppException($"AI Router request failed: {response.StatusCode}. {raw}", 502);

            using var json = JsonDocument.Parse(raw);
            var text = ExtractResponseText(json.RootElement);
            return string.IsNullOrWhiteSpace(text) ? "AI không trả về nội dung." : text;
        }
    }

    private static List<object> NormalizeMessages(object input)
    {
        var messages = new List<object>();
        var root = JsonSerializer.SerializeToElement(input);

        if (root.ValueKind == JsonValueKind.String)
        {
            messages.Add(new { role = "user", content = root.GetString() ?? string.Empty });
            return messages;
        }

        if (root.ValueKind != JsonValueKind.Array)
        {
            messages.Add(new { role = "user", content = root.ToString() });
            return messages;
        }

        foreach (var item in root.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object) continue;
            var role = item.TryGetProperty("role", out var roleElement) && roleElement.ValueKind == JsonValueKind.String
                ? roleElement.GetString() ?? "user"
                : "user";
            var content = item.TryGetProperty("content", out var contentElement) && contentElement.ValueKind == JsonValueKind.String
                ? contentElement.GetString() ?? string.Empty
                : string.Empty;

            if (role.Equals("developer", StringComparison.OrdinalIgnoreCase)) role = "system";
            if (role is not "system" and not "assistant" and not "user") role = "user";
            if (!string.IsNullOrWhiteSpace(content)) messages.Add(new { role, content });
        }

        if (messages.Count == 0) messages.Add(new { role = "user", content = string.Empty });
        return messages;
    }

    private static IEnumerable<AiKnowledgeChunk> Rank(IEnumerable<AiKnowledgeChunk> chunks, string query)
    {
        var terms = query.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length > 1)
            .Distinct()
            .ToArray();

        return chunks
            .Select(x => new { Chunk = x, Score = terms.Sum(t => Count(x.Content, t)) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Select(x => x.Chunk);
    }

    private static string? ExtractResponseText(JsonElement root)
    {
        if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array)
        {
            foreach (var choice in choices.EnumerateArray())
            {
                if (!choice.TryGetProperty("message", out var message) || message.ValueKind != JsonValueKind.Object) continue;
                if (message.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                {
                    return content.GetString();
                }
            }
        }

        if (root.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
        {
            return outputText.GetString();
        }

        if (!root.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array) return null;
        foreach (var outputItem in output.EnumerateArray())
        {
            if (!outputItem.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array) continue;
            foreach (var contentItem in content.EnumerateArray())
            {
                if (contentItem.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                {
                    return text.GetString();
                }
            }
        }
        return null;
    }

    private static int Count(string text, string term) => text.ToLowerInvariant().Split(term, StringSplitOptions.None).Length - 1;
    private static EmailDraftResponse ToResponse(AiEmailDraft x) => new(x.Id, x.Recipients, x.Subject, x.Body, x.Status, x.CreatedAt);

    private List<AiKnowledgeChunk> FilterFallbackChunks(string role, Guid? referenceId)
    {
        if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase)) return fallback.Chunks().ToList();
        var normalized = role.Equals("Teacher", StringComparison.OrdinalIgnoreCase) ? "Teacher" : "Student";
        return fallback.Chunks()
            .Where(x => x.Document is not null)
            .Where(x => x.Document!.AudienceRole is null || x.Document.AudienceRole == normalized || x.Document.AudienceRole == "All")
            .Where(x =>
                x.Document!.Scope == "Global" ||
                x.Document.Scope == "Role" ||
                (x.Document.Scope == "Account" && x.Document.OwnerReferenceId == referenceId))
            .ToList();
    }
}
