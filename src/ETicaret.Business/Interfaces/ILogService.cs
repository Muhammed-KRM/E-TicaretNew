using System.Runtime.CompilerServices;

using ETicaret.Data.Entities;

namespace ETicaret.Business.Interfaces;

public interface ILogService
{
    Task LogEndpointAsync(EndpointLogEntry entry);

    Task LogFunctionErrorAsync(
        string errorCode,
        Exception ex,
        object? inputData = null,
        Guid? userId = null,
        string? traceId = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    // ➕ YENİ — Fonksiyon BAŞARI logu (input + output + süre)
    Task LogFunctionSuccessAsync(
        string code,
        object? inputData,
        object? outputData,
        int durationMs,
        Guid? userId = null,
        string? traceId = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0);

    // ➕ YENİ — Fonksiyon loglarını listeleme (Admin paneli)
    Task<List<FunctionLog>> GetFunctionLogsAsync(
        DateTime? from, DateTime? to,
        string? severity, string? className,
        bool? isSuccess, int page = 1, int pageSize = 50);

    // ➕ YENİ — Endpoint loglarını listeleme (Admin paneli)
    Task<List<EndpointLog>> GetEndpointLogsAsync(
        DateTime? from, DateTime? to,
        string? method, string? path,
        int? minStatusCode, int? maxStatusCode,
        int page = 1, int pageSize = 50);
}

public class EndpointLogEntry
{
    public string? TraceId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string? Query { get; set; }
    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
    public int StatusCode { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public int DurationMs { get; set; }
}

