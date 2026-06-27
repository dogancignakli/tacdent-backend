using Riok.Mapperly.Abstractions;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;

namespace Tacdent.Application.Mapping;

[Mapper]
public partial class UserMapper
{
    [MapperIgnoreSource(nameof(User.PasswordHash))]
    [MapperIgnoreSource(nameof(User.AccessFailedCount))]
    [MapperIgnoreSource(nameof(User.LockoutEndUtc))]
    public partial UserDto ToDto(User entity);

    public partial IReadOnlyList<UserDto> ToDtoList(IReadOnlyList<User> entities);
}
