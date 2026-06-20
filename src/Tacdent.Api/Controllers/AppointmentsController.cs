using Microsoft.AspNetCore.Mvc;
using Tacdent.Api.Extensions;
using Tacdent.Api.Factories;
using Tacdent.Api.ViewModels;
using Tacdent.Application.Services.Interfaces;
using Tacdent.Core.Entities;

namespace Tacdent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController(
    IAppointmentService appointmentService,
    IAppointmentFactory factory) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAll(
        [FromQuery] AppointmentStatus? status,
        CancellationToken cancellationToken)
    {
        var appointments = await appointmentService.GetAllAsync(status, cancellationToken);
        return Ok(appointments.Select(factory.ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await appointmentService.GetByIdAsync(id, cancellationToken);
        return result.ToOkResult(factory.ToResponse);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await appointmentService.CreateAsync(factory.ToCreateDto(request), cancellationToken);

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
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await appointmentService.DeleteAsync(id, cancellationToken);
        return result.ToNoContentResult();
    }
}
