using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Serilog.Context;
using Tacdent.Api.Logging;
using Tacdent.Api.Options;

namespace Tacdent.Api.Middleware;

public sealed partial class CorrelationMiddleware(
    RequestDelegate next,
    IOptions<RequestLoggingOptions> options)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var maxLength = Math.Clamp(options.Value.MaxCorrelationIdLength, 1, 128);

        var correlationId = ResolveCorrelationId(context.Request.Headers[TraceContext.CorrelationIdHeader], maxLength)
            ?? Guid.NewGuid().ToString("D");

        var sessionId = ResolveSessionId(context.Request.Headers[TraceContext.SessionIdHeader], maxLength);

        context.Items[TraceContext.CorrelationIdItemKey] = correlationId;
        if (sessionId is not null)
        {
            context.Items[TraceContext.SessionIdItemKey] = sessionId;
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[TraceContext.CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("SessionId", sessionId ?? string.Empty))
        using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
        {
            await next(context);
        }
    }

    private static string? ResolveCorrelationId(string? raw, int maxLength)
    {
        var value = NormalizeHeaderValue(raw, maxLength);
        if (value is null)
        {
            return null;
        }

        return IsValidTraceId(value) ? value : null;
    }

    private static string? ResolveSessionId(string? raw, int maxLength)
    {
        var value = NormalizeHeaderValue(raw, maxLength);
        if (value is null)
        {
            return null;
        }

        return IsValidTraceId(value) ? value : null;
    }

    private static string? NormalizeHeaderValue(string? raw, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var value = raw.Trim().Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal);

        if (value.Length == 0 || value.Length > maxLength)
        {
            return null;
        }

        return value;
    }

    private static bool IsValidTraceId(string value)
        => Guid.TryParse(value, out _) || TraceIdRegex().IsMatch(value);

    [GeneratedRegex("^[A-Za-z0-9-]{1,64}$", RegexOptions.CultureInvariant)]
    private static partial Regex TraceIdRegex();
}
