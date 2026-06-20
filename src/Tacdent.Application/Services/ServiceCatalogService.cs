using Tacdent.Application.Mapping;
using Tacdent.Application.Services.Interfaces;
using Tacdent.Core.DTOs;
using Tacdent.Data.Repositories.Interfaces;

namespace Tacdent.Application.Services;

public class ServiceCatalogService(IUnitOfWork unitOfWork, ServiceMapper mapper) : IServiceCatalogService
{
    public async Task<IReadOnlyList<ServiceDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var services = await unitOfWork.Services.GetActiveAsync(cancellationToken);
        return mapper.ToDtoList(services);
    }
}
