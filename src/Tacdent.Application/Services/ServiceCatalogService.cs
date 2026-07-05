using Tacdent.Application.Errors;
using Tacdent.Application.Mapping;
using Tacdent.Application.Services.Interfaces;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;
using Tacdent.Core.Results;
using Tacdent.Data.Repositories.Interfaces;

namespace Tacdent.Application.Services;

public class ServiceCatalogService(IUnitOfWork unitOfWork, ServiceMapper mapper) : IServiceCatalogService
{
    public async Task<IReadOnlyList<ServiceDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var services = await unitOfWork.Services.GetActiveAsync(cancellationToken);
        return mapper.ToDtoList(services);
    }

    public async Task<IReadOnlyList<ServiceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var services = await unitOfWork.Services.GetAllOrderedAsync(cancellationToken);
        return mapper.ToDtoList(services);
    }

    public async Task<Result<ServiceDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var service = await unitOfWork.Services.GetByIdAsync(id, cancellationToken);
        return service is null
            ? Result.Failure<ServiceDto>(ServiceErrors.NotFound(id))
            : Result.Success(mapper.ToDto(service));
    }

    public async Task<Result<ServiceDto>> CreateAsync(
        CreateServiceDto dto,
        CancellationToken cancellationToken = default)
    {
        var service = new DentalService
        {
            NameTr = dto.NameTr.Trim(),
            NameEn = dto.NameEn.Trim(),
            DescriptionTr = dto.DescriptionTr.Trim(),
            DescriptionEn = dto.DescriptionEn.Trim(),
            Icon = string.IsNullOrWhiteSpace(dto.Icon) ? null : dto.Icon.Trim(),
            PriceFromTry = dto.PriceFromTry,
            PriceFromEur = dto.PriceFromEur,
            DurationMinutes = dto.DurationMinutes,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive,
        };

        await unitOfWork.Services.AddAsync(service, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDto(service));
    }

    public async Task<Result<ServiceDto>> UpdateAsync(
        int id,
        UpdateServiceDto dto,
        CancellationToken cancellationToken = default)
    {
        var service = await unitOfWork.Services.GetByIdAsync(id, cancellationToken);
        if (service is null)
        {
            return Result.Failure<ServiceDto>(ServiceErrors.NotFound(id));
        }

        service.NameTr = dto.NameTr.Trim();
        service.NameEn = dto.NameEn.Trim();
        service.DescriptionTr = dto.DescriptionTr.Trim();
        service.DescriptionEn = dto.DescriptionEn.Trim();
        service.Icon = string.IsNullOrWhiteSpace(dto.Icon) ? null : dto.Icon.Trim();
        service.PriceFromTry = dto.PriceFromTry;
        service.PriceFromEur = dto.PriceFromEur;
        service.DurationMinutes = dto.DurationMinutes;
        service.DisplayOrder = dto.DisplayOrder;
        service.IsActive = dto.IsActive;

        unitOfWork.Services.Update(service);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDto(service));
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var service = await unitOfWork.Services.GetByIdAsync(id, cancellationToken);
        if (service is null)
        {
            return Result.Failure(ServiceErrors.NotFound(id));
        }

        if (await unitOfWork.Services.HasAppointmentsAsync(id, cancellationToken))
        {
            return Result.Failure(ServiceErrors.InUse);
        }

        unitOfWork.Services.Remove(service);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
