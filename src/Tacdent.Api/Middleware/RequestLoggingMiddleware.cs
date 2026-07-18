using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Tacdent.Api.Extensions;
using Tacdent.Api.Logging;
using Tacdent.Api.Options;

namespace Tacdent.Api.Middleware;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger,
    IOptions<RequestLoggingOptions> options,
    SensitiveDataRedactor redactor)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        string? requestBody = null;
        MemoryStream? responseBuffer = null;
        var originalBody = context.Response.Body;

        try
        {
            requestBody = await TryReadRequestBodyAsync(context, settings);

            // Buffer response only when we may need the body later; content-type is
            // unknown until headers are written, so we always buffer through a memory
            // stream and decide in finally whether to include the body in the log.
            responseBuffer = new MemoryStream();
            context.Response.Body = responseBuffer;

            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            string? responseBody = null;
            try
            {
                if (responseBuffer is not null)
                {
                    responseBuffer.Position = 0;
                    await responseBuffer.CopyToAsync(originalBody);

                    responseBody = await TryReadResponseBodyAsync(
                        context,
                        responseBuffer,
                        settings);
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Failed to capture response body for logging.");
            }
            finally
            {
                context.Response.Body = originalBody;
                if (responseBuffer is not null)
                {
                    await responseBuffer.DisposeAsync();
                }
            }

            LogRequest(context, stopwatch.ElapsedMilliseconds, requestBody, responseBody);
        }
    }

    private async Task<string?> TryReadRequestBodyAsync(HttpContext context, RequestLoggingOptions settings)
    {
        var request = context.Request;

        if (IsBodyExcludedPath(request.Path, settings.BodyExcludedPaths))
        {
            return "[skipped:excluded-path]";
        }

        if (!IsLoggableContentType(request.ContentType, settings.LoggableContentTypes))
        {
            return null;
        }

        if (request.ContentLength is > 0 and var length && length > settings.MaxBodyBytes)
        {
            return "[skipped:too-large]";
        }

        if (!request.Body.CanSeek && !request.Body.CanRead)
        {
            return null;
        }

        request.EnableBuffering();

        try
        {
            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            var buffer = new char[settings.MaxBodyBytes + 1];
            var read = await reader.ReadBlockAsync(buffer, 0, buffer.Length);
            request.Body.Position = 0;

            if (read == 0)
            {
                return null;
            }

            var text = new string(buffer, 0, Math.Min(read, settings.MaxBodyBytes));
            if (read > settings.MaxBodyBytes)
            {
                text += "[truncated]";
            }

            return redactor.RedactJson(text);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to read request body for logging.");
            request.Body.Position = 0;
            return "[skipped:read-error]";
        }
    }

    private async Task<string?> TryReadResponseBodyAsync(
        HttpContext context,
        MemoryStream responseBuffer,
        RequestLoggingOptions settings)
    {
        var statusCode = context.Response.StatusCode;
        var isSuccess = statusCode is >= 200 and < 300;

        if (isSuccess && !settings.LogSuccessResponseBody)
        {
            return null;
        }

        if (IsBodyExcludedPath(context.Request.Path, settings.BodyExcludedPaths))
        {
            return "[skipped:excluded-path]";
        }

        var contentType = context.Response.ContentType;
        if (!IsLoggableContentType(contentType, settings.LoggableContentTypes))
        {
            return null;
        }

        if (IsNonCapturableContentType(contentType))
        {
            return "[skipped:non-capturable]";
        }

        var contentDisposition = context.Response.Headers.ContentDisposition.ToString();
        if (contentDisposition.Contains("attachment", StringComparison.OrdinalIgnoreCase))
        {
            return "[skipped:attachment]";
        }

        if (responseBuffer.Length > settings.MaxBodyBytes)
        {
            responseBuffer.Position = 0;
            var truncated = new byte[settings.MaxBodyBytes];
            _ = await responseBuffer.ReadAsync(truncated);
            var text = Encoding.UTF8.GetString(truncated) + "[truncated]";
            return redactor.RedactJson(text);
        }

        if (responseBuffer.Length == 0)
        {
            return null;
        }

        responseBuffer.Position = 0;
        using var reader = new StreamReader(responseBuffer, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        return redactor.RedactJson(body);
    }

    private void LogRequest(
        HttpContext context,
        long elapsedMs,
        string? requestBody,
        string? responseBody)
    {
        var statusCode = context.Response.StatusCode;
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");
        var role = context.User.FindFirstValue(ClaimTypes.Role);

        // Path only — never query string, cookies, or Authorization.
        var path = context.Request.Path.Value ?? string.Empty;

        if (statusCode >= 500)
        {
            logger.LogError(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms. ClientIp={ClientIp} UserId={UserId} Role={Role} RequestBody={RequestBody} ResponseBody={ResponseBody}",
                context.Request.Method,
                path,
                statusCode,
                elapsedMs,
                context.GetClientIpAddress(),
                userId,
                role,
                requestBody,
                responseBody);
            return;
        }

        if (statusCode >= 400)
        {
            logger.LogWarning(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms. ClientIp={ClientIp} UserId={UserId} Role={Role} RequestBody={RequestBody} ResponseBody={ResponseBody}",
                context.Request.Method,
                path,
                statusCode,
                elapsedMs,
                context.GetClientIpAddress(),
                userId,
                role,
                requestBody,
                responseBody);
            return;
        }

        logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms. ClientIp={ClientIp} UserId={UserId} Role={Role} RequestBody={RequestBody} ResponseBody={ResponseBody}",
            context.Request.Method,
            path,
            statusCode,
            elapsedMs,
            context.GetClientIpAddress(),
            userId,
            role,
            requestBody,
            responseBody);
    }

    private static bool IsBodyExcludedPath(PathString path, string[]? excludedPaths)
    {
        if (excludedPaths is null || excludedPaths.Length == 0)
        {
            return false;
        }

        foreach (var excluded in excludedPaths)
        {
            if (string.IsNullOrWhiteSpace(excluded))
            {
                continue;
            }

            if (path.StartsWithSegments(excluded, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsLoggableContentType(string? contentType, string[]? loggableTypes)
    {
        if (string.IsNullOrWhiteSpace(contentType) || loggableTypes is null || loggableTypes.Length == 0)
        {
            return false;
        }

        foreach (var type in loggableTypes)
        {
            if (!string.IsNullOrWhiteSpace(type)
                && contentType.Contains(type, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNonCapturableContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        return contentType.Contains("text/event-stream", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("application/octet-stream", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("application/pdf", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("image/", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("video/", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("audio/", StringComparison.OrdinalIgnoreCase);
    }
}
