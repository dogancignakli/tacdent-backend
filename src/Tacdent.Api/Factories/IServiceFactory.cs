using Tacdent.Api.ViewModels;
using Tacdent.Core.DTOs;

namespace Tacdent.Api.Factories;

public interface IServiceFactory
{
    ServiceResponse ToResponse(ServiceDto dto);
}
