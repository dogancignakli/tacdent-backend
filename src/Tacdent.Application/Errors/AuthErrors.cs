using Tacdent.Core.Results;

namespace Tacdent.Application.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "Auth.InvalidCredentials",
        "Invalid email or password.");

    public static readonly Error AccountLocked = Error.Unauthorized(
        "Auth.AccountLocked",
        "Account is temporarily locked due to too many failed attempts. Try again later.");
}
