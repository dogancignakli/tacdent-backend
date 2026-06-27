using Tacdent.Core.Entities;

namespace Tacdent.Core.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateUserDto(string Email, string Password, UserRole Role);

public record UpdateUserRoleDto(UserRole Role);

public record UpdateUserStatusDto(bool IsActive);

public record ResetPasswordDto(string Password);

public record AssignAppointmentDto(Guid? AssignedUserId);
