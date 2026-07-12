using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using ETicaret.Business.Interfaces;
using ETicaret.Data.Context;
using ETicaret.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ETicaret.Business.Services;

public class LogManager : ILogService
{
    private readonly AppDbContext _db;
    private static readonly JsonSerializerOptions _jsonOptions = new() { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles, WriteIndented = false };

    public LogManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogEndpointAsync(EndpointLogEntry entry)
    {
        try
        {
            var log = new EndpointLog
            {
                TraceId    = entry.TraceId,
                Method     = entry.Method,
                Path       = entry.Path,
                Query      = entry.Query,
                RequestBody  = MaskSensitiveData(entry.RequestBody),
                ResponseBody = entry.ResponseBody,
                StatusCode = entry.StatusCode,
                UserId     = entry.UserId,
                UserEmail  = entry.UserEmail,
                IpAddress  = entry.IpAddress,
                UserAgent  = entry.UserAgent,
                DurationMs = entry.DurationMs,
                CreatedAt  = DateTime.UtcNow
            };

            _db.EndpointLogs.Add(log);
            await _db.SaveChangesAsync();
        }
        catch
        {
            // Log yazma hatası uygulamayı çökertmemeli
        }
    }

    public async Task LogFunctionErrorAsync(
        string errorCode,
        Exception ex,
        object? inputData = null,
        Guid? userId = null,
        string? traceId = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            var className = Path.GetFileNameWithoutExtension(filePath);

            string? inputValue = null;
            if (inputData is not null)
            {
                try { inputValue = JsonSerializer.Serialize(inputData, _jsonOptions); }
                catch { inputValue = inputData.ToString(); }
            }

            var log = new FunctionLog
            {
                ErrorCode    = errorCode,
                ClassName    = className,
                MethodName   = memberName,
                FilePath     = filePath,
                LineNumber   = lineNumber,
                ErrorMessage = ex.Message,
                StackTrace   = ex.StackTrace,
                InputType    = inputData?.GetType().Name,
                InputValue   = MaskSensitiveData(inputValue),
                OutputType   = null,
                OutputValue  = null,
                IsSuccess    = false,
                DurationMs   = 0,
                UserId       = userId,
                TraceId      = traceId,
                Severity     = ex is OutOfMemoryException or StackOverflowException ? "Critical" : "Error",
                CreatedAt    = DateTime.UtcNow
            };

            _db.FunctionLogs.Add(log);
            await _db.SaveChangesAsync();
        }
        catch { }
    }

    public async Task LogFunctionSuccessAsync(
        string code,
        object? inputData,
        object? outputData,
        int durationMs,
        Guid? userId = null,
        string? traceId = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            var className = Path.GetFileNameWithoutExtension(filePath);

            string? inputValue = null;
            if (inputData is not null)
            {
                try { inputValue = JsonSerializer.Serialize(inputData, _jsonOptions); }
                catch { inputValue = inputData.ToString(); }
            }

            string? outputValue = null;
            if (outputData is not null)
            {
                try
                {
                    var json = JsonSerializer.Serialize(outputData, _jsonOptions);
                    outputValue = json.Length > 5000 ? json[..5000] + "...[truncated]" : json;
                }
                catch { outputValue = outputData.ToString(); }
            }

            var log = new FunctionLog
            {
                ErrorCode   = code,
                ClassName   = className,
                MethodName  = memberName,
                FilePath    = filePath,
                LineNumber  = lineNumber,
                InputType   = inputData?.GetType().Name,
                InputValue  = MaskSensitiveData(inputValue),
                OutputType  = outputData?.GetType().Name,
                OutputValue = MaskSensitiveData(outputValue),
                IsSuccess   = true,
                DurationMs  = durationMs,
                UserId      = userId,
                TraceId     = traceId,
                Severity    = "Info",
                CreatedAt   = DateTime.UtcNow
            };

            _db.FunctionLogs.Add(log);
            await _db.SaveChangesAsync();
        }
        catch { }
    }

    public async Task<List<FunctionLog>> GetFunctionLogsAsync(
        DateTime? from, DateTime? to,
        string? severity, string? className,
        bool? isSuccess, int page = 1, int pageSize = 50)
    {
        var query = _db.FunctionLogs.AsQueryable();

        if (from.HasValue)     query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue)       query = query.Where(l => l.CreatedAt <= to.Value);
        if (!string.IsNullOrEmpty(severity))  query = query.Where(l => l.Severity == severity);
        if (!string.IsNullOrEmpty(className)) query = query.Where(l => l.ClassName == className);
        if (isSuccess.HasValue) query = query.Where(l => l.IsSuccess == isSuccess.Value);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<EndpointLog>> GetEndpointLogsAsync(
        DateTime? from, DateTime? to,
        string? method, string? path,
        int? minStatusCode, int? maxStatusCode,
        int page = 1, int pageSize = 50)
    {
        var query = _db.EndpointLogs.AsQueryable();

        if (from.HasValue)     query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue)       query = query.Where(l => l.CreatedAt <= to.Value);
        if (!string.IsNullOrEmpty(method)) query = query.Where(l => l.Method == method);
        if (!string.IsNullOrEmpty(path))   query = query.Where(l => l.Path.Contains(path));
        if (minStatusCode.HasValue) query = query.Where(l => l.StatusCode >= minStatusCode.Value);
        if (maxStatusCode.HasValue) query = query.Where(l => l.StatusCode <= maxStatusCode.Value);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    private static string? MaskSensitiveData(string? json)
    {
        if (string.IsNullOrEmpty(json)) return json;

        return Regex.Replace(
            json,
            @"""(password|token|refreshToken|aesKey|ibanEncrypted|tcknEncrypted)""\s*:\s*""[^""]*""",
            @"""$1"":""***""",
            RegexOptions.IgnoreCase);
    }
}

