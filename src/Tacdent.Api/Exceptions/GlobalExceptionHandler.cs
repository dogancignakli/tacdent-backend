using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Tacdent.Api.Logging;

namespace Tacdent.Api.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = TraceContext.GetCorrelationId(httpContext) ?? httpContext.TraceIdentifier;
        var sessionId = TraceContext.GetSessionId(httpContext);

        logger.LogError(
            exception,
            "Unhandled exception for {Method} {Path}. CorrelationId={CorrelationId} SessionId={SessionId} TraceId={TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path.Value,
            correlationId,
            sessionId,
            httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        var message = environment.IsDevelopment()
            ? exception.Message
            : "An unexpected error occurred.";

        var payload = new
        {
            code = "Server.UnexpectedError",
            message,
            traceId = httpContext.TraceIdentifier,
            correlationId,
        };

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(payload, JsonOptions),
            cancellationToken);

        return true;
    }
}
