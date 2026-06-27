using Tacdent.Api.ViewModels;
using Tacdent.Core.DTOs;

namespace Tacdent.Api.Factories;

public interface IUserFactory
{
    CreateUserDto ToCreateDto(CreateUserRequest request);

    UpdateUserRoleDto ToUpdateRoleDto(UpdateUserRoleRequest request);

    UpdateUserStatusDto ToUpdateStatusDto(UpdateUserStatusRequest request);

    ResetPasswordDto ToResetPasswordDto(ResetPasswordRequest request);

    UserResponse ToResponse(UserDto dto);
}
