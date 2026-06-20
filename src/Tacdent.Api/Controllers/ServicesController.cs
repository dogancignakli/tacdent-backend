using Microsoft.AspNetCore.Mvc;
using Tacdent.Api.Factories;
using Tacdent.Api.ViewModels;
using Tacdent.Application.Services.Interfaces;

namespace Tacdent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController(
    IServiceCatalogService serviceCatalogService,
    IServiceFactory factory) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var services = await serviceCatalogService.GetActiveAsync(cancellationToken);
        return Ok(services.Select(factory.ToResponse));
    }
}
