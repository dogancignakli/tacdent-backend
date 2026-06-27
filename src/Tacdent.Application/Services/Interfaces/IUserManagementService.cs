using Tacdent.Core.DTOs;
using Tacdent.Core.Results;

namespace Tacdent.Application.Services.Interfaces;

public interface IUserManagementService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);

    Task<Result<UserDto>> UpdateRoleAsync(
        Guid id,
        UpdateUserRoleDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<UserDto>> SetActiveAsync(
        Guid id,
        UpdateUserStatusDto dto,
        CancellationToken cancellationToken = default);

    Task<Result> ResetPasswordAsync(
        Guid id,
        ResetPasswordDto dto,
        CancellationToken cancellationToken = default);
}
