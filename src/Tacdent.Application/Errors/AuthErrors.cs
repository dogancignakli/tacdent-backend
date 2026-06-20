using Tacdent.Core.Results;

namespace Tacdent.Application.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "Auth.InvalidCredentials",
        "Invalid password.");
}
