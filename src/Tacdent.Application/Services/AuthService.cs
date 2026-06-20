using Microsoft.Extensions.Options;
using Tacdent.Application.Errors;
using Tacdent.Application.Options;
using Tacdent.Application.Services.Interfaces;
using Tacdent.Core.Results;

namespace Tacdent.Application.Services;

public class AuthService(IOptions<AuthOptions> options) : IAuthService
{
    public Result Authenticate(string password)
    {
        if (string.IsNullOrEmpty(password) || password != options.Value.AdminPassword)
        {
            return Result.Failure(AuthErrors.InvalidCredentials);
        }

        return Result.Success();
    }
}
