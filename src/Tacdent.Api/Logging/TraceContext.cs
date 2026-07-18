namespace Tacdent.Api.Logging;

public static class TraceContext
{
    public const string CorrelationIdHeader = "X-Correlation-ID";
    public const string SessionIdHeader = "X-Session-Id";

    public const string CorrelationIdItemKey = "CorrelationId";
    public const string SessionIdItemKey = "SessionId";

    public static string? GetCorrelationId(HttpContext context)
        => context.Items.TryGetValue(CorrelationIdItemKey, out var value)
            ? value as string
            : null;

    public static string? GetSessionId(HttpContext context)
        => context.Items.TryGetValue(SessionIdItemKey, out var value)
            ? value as string
            : null;
}
