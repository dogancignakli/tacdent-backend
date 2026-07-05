using Tacdent.Api.ViewModels;
using Tacdent.Core.DTOs;

namespace Tacdent.Api.Factories;

public class ServiceFactory : IServiceFactory
{
    public ServiceResponse ToResponse(ServiceDto dto) =>
        new(
            dto.Id,
            dto.NameTr,
            dto.NameEn,
            dto.DescriptionTr,
            dto.DescriptionEn,
            dto.Icon,
            dto.PriceFromTry,
            dto.PriceFromEur,
            dto.DurationMinutes,
            dto.DisplayOrder);

    public AdminServiceResponse ToAdminResponse(ServiceDto dto) =>
        new(
            dto.Id,
            dto.NameTr,
            dto.NameEn,
            dto.DescriptionTr,
            dto.DescriptionEn,
            dto.Icon,
            dto.PriceFromTry,
            dto.PriceFromEur,
            dto.DurationMinutes,
            dto.DisplayOrder,
            dto.IsActive);

    public CreateServiceDto ToCreateDto(CreateServiceRequest request) =>
        new(
            request.NameTr.Trim(),
            request.NameEn.Trim(),
            request.DescriptionTr.Trim(),
            request.DescriptionEn.Trim(),
            string.IsNullOrWhiteSpace(request.Icon) ? null : request.Icon.Trim(),
            request.PriceFromTry,
            request.PriceFromEur,
            request.DurationMinutes,
            request.DisplayOrder,
            request.IsActive);

    public UpdateServiceDto ToUpdateDto(UpdateServiceRequest request) =>
        new(
            request.NameTr.Trim(),
            request.NameEn.Trim(),
            request.DescriptionTr.Trim(),
            request.DescriptionEn.Trim(),
            string.IsNullOrWhiteSpace(request.Icon) ? null : request.Icon.Trim(),
            request.PriceFromTry,
            request.PriceFromEur,
            request.DurationMinutes,
            request.DisplayOrder,
            request.IsActive);
}
