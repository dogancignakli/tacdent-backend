using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tacdent.Api.Auth;
using Tacdent.Api.Extensions;
using Tacdent.Api.Factories;
using Tacdent.Api.ViewModels;
using Tacdent.Application.Services.Interfaces;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;

namespace Tacdent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController(
    IAppointmentService appointmentService,
    IAppointmentFactory factory,
    IRecaptchaValidator recaptchaValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AppointmentResponse>>> GetAll(
        [FromQuery] AppointmentQueryRequest request,
        CancellationToken cancellationToken)
    {
        var page = await appointmentService.GetPagedAsync(factory.ToQuery(request), cancellationToken);
        var response = new PagedResult<AppointmentResponse>(
            page.Items.Select(factory.ToResponse).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await appointmentService.GetByIdAsync(id, cancellationToken);
        return result.ToOkResult(factory.ToResponse);
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("booking")]
    public async Task<IActionResult> Create(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var clientIp = HttpContext.GetClientIpAddress();

        var recaptchaResult = await recaptchaValidator.ValidateAsync(
            request.RecaptchaToken,
            "booking",
            clientIp,
            cancellationToken);

        if (recaptchaResult.IsFailure)
        {
            return recaptchaResult.Error.ToProblemResult();
        }

        var result = await appointmentService.CreateAsync(
            factory.ToCreateDto(request, clientIp),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemResult();
        }

        var response = factory.ToResponse(result.Value);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateAppointmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await appointmentService.UpdateStatusAsync(id, factory.ToUpdateStatusDto(request), cancellationToken);
        return result.ToOkResult(factory.ToResponse);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await appointmentService.DeleteAsync(id, cancellationToken);
        return result.ToNoContentResult();
    }

    [HttpPatch("{id:guid}/assignee")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Assign(
        Guid id,
        [FromBody] AssignAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await appointmentService.AssignAsync(id, factory.ToAssignDto(request), cancellationToken);
        return result.ToOkResult(factory.ToResponse);
    }
}
