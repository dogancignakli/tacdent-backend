using Microsoft.Extensions.Options;
using Tacdent.Application.Errors;
using Tacdent.Application.Options;
using Tacdent.Application.Services.Interfaces;
using Tacdent.Core.DTOs;
using Tacdent.Core.Results;
using Tacdent.Data.Repositories.Interfaces;

namespace Tacdent.Application.Services;

public class AuthService(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IOptions<AuthOptions> options) : IAuthService
{
    public async Task<Result<AuthenticatedUserDto>> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
        var user = await unitOfWork.Users.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            // Burn equivalent work so a missing/inactive account is not distinguishable by timing.
            passwordHasher.Hash(password ?? string.Empty);
            return Result.Failure<AuthenticatedUserDto>(AuthErrors.InvalidCredentials);
        }

        if (user.LockoutEndUtc is { } lockoutEnd && lockoutEnd > DateTime.UtcNow)
        {
            return Result.Failure<AuthenticatedUserDto>(AuthErrors.AccountLocked);
        }

        if (!passwordHasher.Verify(password ?? string.Empty, user.PasswordHash))
        {
            await RegisterFailedAttemptAsync(user, cancellationToken);
            return Result.Failure<AuthenticatedUserDto>(AuthErrors.InvalidCredentials);
        }

        await ResetLockoutAsync(user, cancellationToken);

        return Result.Success(new AuthenticatedUserDto(user.Id, user.Email, user.Role));
    }

    private async Task RegisterFailedAttemptAsync(
        Tacdent.Core.Entities.User user,
        CancellationToken cancellationToken)
    {
        user.AccessFailedCount++;

        if (user.AccessFailedCount >= options.Value.MaxFailedAttempts)
        {
            user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(options.Value.LockoutMinutes);
            user.AccessFailedCount = 0;
        }

        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ResetLockoutAsync(
        Tacdent.Core.Entities.User user,
        CancellationToken cancellationToken)
    {
        if (user.AccessFailedCount == 0 && user.LockoutEndUtc is null)
        {
            return;
        }

        user.AccessFailedCount = 0;
        user.LockoutEndUtc = null;
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
