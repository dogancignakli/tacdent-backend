using Tacdent.Application.Errors;
using Tacdent.Application.Mapping;
using Tacdent.Application.Services.Interfaces;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;
using Tacdent.Core.Results;
using Tacdent.Data.Repositories.Interfaces;

namespace Tacdent.Application.Services;

public class UserManagementService(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    UserMapper mapper) : IUserManagementService
{
    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await unitOfWork.Users.GetAllOrderedAsync(cancellationToken);
        return mapper.ToDtoList(users);
    }

    public async Task<Result<UserDto>> CreateAsync(
        CreateUserDto dto,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(dto.Email);

        if (await unitOfWork.Users.EmailExistsAsync(email, cancellationToken))
        {
            return Result.Failure<UserDto>(UserErrors.EmailAlreadyExists);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.Hash(dto.Password),
            Role = dto.Role,
            IsActive = true,
        };

        await unitOfWork.Users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDto(user));
    }

    public async Task<Result<UserDto>> UpdateRoleAsync(
        Guid id,
        UpdateUserRoleDto dto,
        CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>(UserErrors.NotFound(id));
        }

        if (user.Role == UserRole.Admin
            && dto.Role != UserRole.Admin
            && user.IsActive
            && await unitOfWork.Users.CountActiveAdminsAsync(cancellationToken) <= 1)
        {
            return Result.Failure<UserDto>(UserErrors.CannotModifyLastAdmin);
        }

        user.Role = dto.Role;
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDto(user));
    }

    public async Task<Result<UserDto>> SetActiveAsync(
        Guid id,
        UpdateUserStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDto>(UserErrors.NotFound(id));
        }

        if (user.Role == UserRole.Admin
            && user.IsActive
            && !dto.IsActive
            && await unitOfWork.Users.CountActiveAdminsAsync(cancellationToken) <= 1)
        {
            return Result.Failure<UserDto>(UserErrors.CannotModifyLastAdmin);
        }

        user.IsActive = dto.IsActive;
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDto(user));
    }

    public async Task<Result> ResetPasswordAsync(
        Guid id,
        ResetPasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(id));
        }

        user.PasswordHash = passwordHasher.Hash(dto.Password);
        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static string NormalizeEmail(string email)
        => (email ?? string.Empty).Trim().ToLowerInvariant();
}
