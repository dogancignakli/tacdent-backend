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
public class TestimonialsController(
    ITestimonialService testimonialService,
    ITestimonialFactory factory) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TestimonialResponse>>> GetActive(CancellationToken cancellationToken)
    {
        var testimonials = await testimonialService.GetActiveAsync(cancellationToken);
        return Ok(testimonials.Select(factory.ToResponse));
    }

    [HttpGet("all")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IEnumerable<AdminTestimonialResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var testimonials = await testimonialService.GetAllAsync(cancellationToken);
        return Ok(testimonials.Select(factory.ToAdminResponse));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTestimonialRequest request,
        CancellationToken cancellationToken)
    {
        var result = await testimonialService.CreateAsync(factory.ToCreateDto(request), cancellationToken);
        return result.ToCreatedResult(factory.ToAdminResponse);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateTestimonialRequest request,
        CancellationToken cancellationToken)
    {
        var result = await testimonialService.UpdateAsync(id, factory.ToUpdateDto(request), cancellationToken);
        return result.ToOkResult(factory.ToAdminResponse);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await testimonialService.DeleteAsync(id, cancellationToken);
        return result.ToNoContentResult();
    }
}
