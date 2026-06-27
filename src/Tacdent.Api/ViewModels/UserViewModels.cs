using Tacdent.Core.Entities;

namespace Tacdent.Api.ViewModels;

public record UserResponse(
    Guid Id,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Staff;
}

public class UpdateUserRoleRequest
{
    public UserRole Role { get; set; }
}

public class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}

public class ResetPasswordRequest
{
    public string Password { get; set; } = string.Empty;
}

public class AssignAppointmentRequest
{
    public Guid? AssignedUserId { get; set; }
}
