using Tacdent.Api.ViewModels;
using Tacdent.Core.DTOs;

namespace Tacdent.Api.Factories;

public class UserFactory : IUserFactory
{
    public CreateUserDto ToCreateDto(CreateUserRequest request) =>
        new(
            request.Email.Trim(),
            request.Password,
            request.Role);

    public UpdateUserRoleDto ToUpdateRoleDto(UpdateUserRoleRequest request) =>
        new(request.Role);

    public UpdateUserStatusDto ToUpdateStatusDto(UpdateUserStatusRequest request) =>
        new(request.IsActive);

    public ResetPasswordDto ToResetPasswordDto(ResetPasswordRequest request) =>
        new(request.Password);

    public UserResponse ToResponse(UserDto dto) =>
        new(
            dto.Id,
            dto.Email,
            dto.Role,
            dto.IsActive,
            dto.CreatedAt,
            dto.UpdatedAt);
}
