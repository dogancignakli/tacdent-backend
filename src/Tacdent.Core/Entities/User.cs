using Tacdent.Core.Entities.Common;

namespace Tacdent.Core.Entities;

public class User : AuditableEntity
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.Staff;
    public bool IsActive { get; set; } = true;
    public int AccessFailedCount { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
}
