using Tacdent.Core.DTOs;
using Tacdent.Core.Results;

namespace Tacdent.Application.Services.Interfaces;

public interface IServiceCatalogService
{
    Task<IReadOnlyList<ServiceDto>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServiceDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<ServiceDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<ServiceDto>> CreateAsync(CreateServiceDto dto, CancellationToken cancellationToken = default);

    Task<Result<ServiceDto>> UpdateAsync(int id, UpdateServiceDto dto, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
