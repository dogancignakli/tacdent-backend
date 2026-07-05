using Tacdent.Api.ViewModels;
using Tacdent.Core.DTOs;

namespace Tacdent.Api.Factories;

public interface IServiceFactory
{
    ServiceResponse ToResponse(ServiceDto dto);

    AdminServiceResponse ToAdminResponse(ServiceDto dto);

    CreateServiceDto ToCreateDto(CreateServiceRequest request);

    UpdateServiceDto ToUpdateDto(UpdateServiceRequest request);
}
