using Microsoft.Extensions.Options;
using Tacdent.Application.Options;

namespace Tacdent.Api.Middleware;

public class InternalApiMiddleware(RequestDelegate next)
{
    private static readonly PathString AppointmentsPath = new("/api/appointments");
    private static readonly PathString LoginPath = new("/api/auth/login");

    public async Task InvokeAsync(HttpContext context, IOptions<InternalApiOptions> options)
    {
        var configuredKey = options.Value.Key;

        if (!string.IsNullOrWhiteSpace(configuredKey)
            && RequiresInternalKey(context.Request.Method, context.Request.Path)
            && !IsAuthorized(context.Request.Headers, configuredKey))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message = "Forbidden." });
            return;
        }

        await next(context);
    }

    private static bool RequiresInternalKey(string method, PathString path) =>
        string.Equals(method, HttpMethods.Post, StringComparison.OrdinalIgnoreCase)
        && (path.StartsWithSegments(AppointmentsPath, StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments(LoginPath, StringComparison.OrdinalIgnoreCase));

    private static bool IsAuthorized(IHeaderDictionary headers, string configuredKey) =>
        headers.TryGetValue("X-Internal-Api-Key", out var providedKey)
        && string.Equals(providedKey.ToString(), configuredKey, StringComparison.Ordinal);
}
