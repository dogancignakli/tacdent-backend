using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tacdent.Api.Auth;
using Tacdent.Api.Extensions;
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
    public async Task<ActionResult<IEnumerable<ServiceResponse>>> GetActive(CancellationToken cancellationToken)
    {
        var services = await serviceCatalogService.GetActiveAsync(cancellationToken);
        return Ok(services.Select(factory.ToResponse));
    }

    [HttpGet("all")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IEnumerable<AdminServiceResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var services = await serviceCatalogService.GetAllAsync(cancellationToken);
        return Ok(services.Select(factory.ToAdminResponse));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(
        [FromBody] CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await serviceCatalogService.CreateAsync(factory.ToCreateDto(request), cancellationToken);
        return result.ToCreatedResult(factory.ToAdminResponse);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await serviceCatalogService.UpdateAsync(id, factory.ToUpdateDto(request), cancellationToken);
        return result.ToOkResult(factory.ToAdminResponse);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await serviceCatalogService.DeleteAsync(id, cancellationToken);
        return result.ToNoContentResult();
    }
}
