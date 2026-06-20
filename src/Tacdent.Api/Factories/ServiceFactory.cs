using Tacdent.Api.ViewModels;
using Tacdent.Core.DTOs;

namespace Tacdent.Api.Factories;

public class ServiceFactory : IServiceFactory
{
    public ServiceResponse ToResponse(ServiceDto dto) =>
        new(
            dto.Id,
            dto.Name,
            dto.Description,
            dto.Icon,
            dto.PriceFrom,
            dto.DurationMinutes);
}
