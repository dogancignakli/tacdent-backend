using Tacdent.Core.Entities;

namespace Tacdent.Core.DTOs;

public record AuthenticatedUserDto(Guid Id, string Email, UserRole Role);
