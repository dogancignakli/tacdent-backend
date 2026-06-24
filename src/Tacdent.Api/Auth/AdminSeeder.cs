using Microsoft.Extensions.Options;
using Tacdent.Application.Options;
using Tacdent.Application.Services.Interfaces;
using Tacdent.Core.Entities;
using Tacdent.Data.Repositories.Interfaces;

namespace Tacdent.Api.Auth;

/// <summary>
/// First-run bootstrap: if the users table is empty, creates the initial admin from the
/// <c>Auth:AdminEmail</c>/<c>Auth:AdminPassword</c> secrets (password stored hashed). After the
/// first user exists this is a no-op and the database becomes the source of truth.
/// </summary>
public static class AdminSeeder
{
    private const string DefaultAdminEmail = "admin@tacdent.local";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        var authOptions = services.GetRequiredService<IOptions<AuthOptions>>().Value;

        var existing = await unitOfWork.Users.GetAllAsync(cancellationToken);
        if (existing.Count > 0)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(authOptions.AdminPassword))
        {
            throw new InvalidOperationException(
                "Cannot seed the initial admin: Auth:AdminPassword is not configured.");
        }

        var email = string.IsNullOrWhiteSpace(authOptions.AdminEmail)
            ? DefaultAdminEmail
            : authOptions.AdminEmail.Trim().ToLowerInvariant();

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.Hash(authOptions.AdminPassword),
            Role = UserRole.Admin,
            IsActive = true,
        };

        await unitOfWork.Users.AddAsync(admin, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
