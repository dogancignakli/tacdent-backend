using Tacdent.Core.DTOs;

namespace Tacdent.Application.Services.Interfaces;

public interface IServiceCatalogService
{
    Task<IReadOnlyList<ServiceDto>> GetActiveAsync(CancellationToken cancellationToken = default);
}
