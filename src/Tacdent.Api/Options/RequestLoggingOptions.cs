namespace Tacdent.Api.Options;

public class RequestLoggingOptions
{
    public const string SectionName = "RequestLogging";

    public bool Enabled { get; set; } = true;

    /// <summary>Maximum request/response body bytes to capture before truncating.</summary>
    public int MaxBodyBytes { get; set; } = 4096;

    /// <summary>Content types eligible for body logging (substring match, case-insensitive).</summary>
    public string[] LoggableContentTypes { get; set; } = ["application/json"];

    /// <summary>
    /// Request paths whose bodies are never logged (prefix match).
    /// Defaults cover auth and appointment PII/password payloads.
    /// </summary>
    public string[] BodyExcludedPaths { get; set; } = ["/api/auth", "/api/appointments"];

    /// <summary>
    /// When false (recommended for Production), 2xx response bodies are not logged.
    /// Non-2xx bodies may still be logged when other capture rules allow it.
    /// </summary>
    public bool LogSuccessResponseBody { get; set; }

    /// <summary>JSON property names to mask (case-insensitive).</summary>
    public string[] SensitiveJsonFields { get; set; } =
    [
        "password",
        "currentPassword",
        "newPassword",
        "phone",
        "email",
        "patientName",
        "recaptchaToken",
        "token",
    ];

    /// <summary>Maximum accepted length for correlation/session header values.</summary>
    public int MaxCorrelationIdLength { get; set; } = 64;
}
