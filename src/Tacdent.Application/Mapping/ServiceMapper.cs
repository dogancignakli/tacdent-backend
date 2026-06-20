using Riok.Mapperly.Abstractions;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;

namespace Tacdent.Application.Mapping;

[Mapper]
public partial class ServiceMapper
{
    [MapperIgnoreSource(nameof(DentalService.IsActive))]
    [MapperIgnoreSource(nameof(DentalService.CreatedAt))]
    [MapperIgnoreSource(nameof(DentalService.UpdatedAt))]
    public partial ServiceDto ToDto(DentalService entity);

    public partial IReadOnlyList<ServiceDto> ToDtoList(IReadOnlyList<DentalService> entities);
}
