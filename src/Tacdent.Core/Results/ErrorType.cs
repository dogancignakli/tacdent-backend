namespace Tacdent.Core.Results;

/// <summary>
/// Classifies an <see cref="Error"/> so the presentation layer can translate it
/// into an appropriate transport status (e.g. HTTP 400/404/409/500).
/// </summary>
public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Failure
}
