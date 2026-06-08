using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace StudentAttendanceService.Extensions;

public static class AdminFileExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<List<T>> ReadJsonArrayAsync<T>(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0) return [];
        await using var stream = file.OpenReadStream();
        return await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonOptions, cancellationToken) ?? [];
    }

    public static FileContentResult ToCsvFile<T>(this ControllerBase controller, IEnumerable<T> rows, string fileName)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(",", properties.Select(p => Escape(p.Name))));

        foreach (var row in rows)
        {
            builder.AppendLine(string.Join(",", properties.Select(p => Escape(Convert.ToString(p.GetValue(row)) ?? string.Empty))));
        }

        return controller.File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray(), "text/csv; charset=utf-8", fileName);
    }

    private static string Escape(string value)
    {
        var escaped = value.Replace("\"", "\"\"");
        return escaped.Contains(',') || escaped.Contains('"') || escaped.Contains('\n') || escaped.Contains('\r') ? $"\"{escaped}\"" : escaped;
    }
}
