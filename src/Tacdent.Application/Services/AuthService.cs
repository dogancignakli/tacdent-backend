using System.Security.Cryptography;
using System.Text;
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
        var expected = Encoding.UTF8.GetBytes(options.Value.AdminPassword);
        var actual = Encoding.UTF8.GetBytes(password ?? string.Empty);

        if (expected.Length != actual.Length ||
            !CryptographicOperations.FixedTimeEquals(expected, actual))
        {
            return Result.Failure(AuthErrors.InvalidCredentials);
        }

        return Result.Success();
    }
}
