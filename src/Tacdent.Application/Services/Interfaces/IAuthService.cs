using Tacdent.Core.DTOs;
using Tacdent.Core.Results;

namespace Tacdent.Application.Services.Interfaces;

public interface IAuthService
{
    Task<Result<AuthenticatedUserDto>> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}
