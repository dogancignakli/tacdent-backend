using Tacdent.Core.DTOs;

namespace Tacdent.Api.Auth;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) GenerateToken(AuthenticatedUserDto user);
}
