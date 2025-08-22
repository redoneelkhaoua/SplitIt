using MediatR;
using Microsoft.AspNetCore.Mvc;
using TailoringApp.Application.Customers.Commands.RegisterCustomer;
using TailoringApp.Application.Customers.Queries.GetCustomers;
using TailoringApp.Application.Customers.Commands.DeleteCustomer;
using TailoringApp.Application.Customers.Commands.RestoreCustomer;
using TailoringApp.Application.Customers.Commands.AddMeasurement;
using TailoringApp.Application.Customers.Commands.AddNote;
using TailoringApp.Application.Customers.Queries.GetCustomerDetails;
using TailoringApp.Application.Customers.Commands.UpdateCustomer;
using TailoringApp.Application.Appointments.Commands.ScheduleAppointment;
using TailoringApp.Application.Appointments.Queries.GetCustomerAppointments;
using TailoringApp.Application.Appointments.Commands.RescheduleAppointment;
using TailoringApp.Application.Appointments.Commands.CancelAppointment;
using TailoringApp.Application.Appointments.Commands.CompleteAppointment;
using TailoringApp.Application.Appointments.Commands.UpdateAppointmentNotes;
using Microsoft.AspNetCore.Authorization;

namespace TailoringApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // MVP: open until test auth adaptation
public class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("{id:guid}/appointments")]
    public async Task<IActionResult> ScheduleAppointment([FromRoute] Guid id, [FromBody] ScheduleAppointmentCommand body, CancellationToken ct)
    {
        var cmd = body with { CustomerId = id };
        var apptId = await _sender.Send(cmd, ct);
        if (apptId == Guid.Empty) return BadRequest("Invalid customer or time conflict");
        return CreatedAtAction(nameof(GetAppointments), new { id, page = 1, pageSize = 50 }, new { id = apptId });
    }

    [HttpGet("{id:guid}/appointments")]
    public async Task<IActionResult> GetAppointments([FromRoute] Guid id, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetCustomerAppointmentsQuery(id, fromUtc, toUtc, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/appointments/{appointmentId:guid}")]
    public async Task<IActionResult> RescheduleAppointment([FromRoute] Guid id, [FromRoute] Guid appointmentId, [FromBody] RescheduleAppointmentCommand body, CancellationToken ct)
    {
        var cmd = body with { CustomerId = id, AppointmentId = appointmentId };
        var ok = await _sender.Send(cmd, ct);
        return ok ? NoContent() : BadRequest("Invalid id, ownership, or time conflict");
    }

    [HttpDelete("{id:guid}/appointments/{appointmentId:guid}")]
    public async Task<IActionResult> CancelAppointment([FromRoute] Guid id, [FromRoute] Guid appointmentId, CancellationToken ct)
    {
        var ok = await _sender.Send(new CancelAppointmentCommand(appointmentId, id), ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/appointments/{appointmentId:guid}/complete")]
    public async Task<IActionResult> CompleteAppointment([FromRoute] Guid id, [FromRoute] Guid appointmentId, CancellationToken ct)
    {
        var ok = await _sender.Send(new CompleteAppointmentCommand(appointmentId, id), ct);
        return ok ? NoContent() : BadRequest("Invalid id or status");
    }

    [HttpPatch("{id:guid}/appointments/{appointmentId:guid}/notes")]
    public async Task<IActionResult> UpdateAppointmentNotes([FromRoute] Guid id, [FromRoute] Guid appointmentId, [FromBody] string? notes, CancellationToken ct)
    {
        var ok = await _sender.Send(new UpdateAppointmentNotesCommand(appointmentId, id, notes), ct);
        return ok ? NoContent() : NotFound();
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Register([FromBody] RegisterCustomerCommand command, CancellationToken ct)
    {
        var id = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetCustomerDetailsQuery(id), ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCustomerCommand body, CancellationToken ct)
    {
        var cmd = body with { Id = id };
        var ok = await _sender.Send(cmd, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] string? sortBy = null, [FromQuery] string sortDir = "desc", [FromQuery] string status = "enabled", CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetCustomersQuery(page, pageSize, search, sortBy, sortDir, status), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
    var ok = await _sender.Send(new DeleteCustomerCommand(id), ct);
    return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/restore")]
    [Authorize(Roles="Admin")]
    public async Task<IActionResult> Restore([FromRoute] Guid id, CancellationToken ct)
    {
    var ok = await _sender.Send(new RestoreCustomerCommand(id), ct);
    return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/measurements")]
    [Authorize]
    public async Task<IActionResult> AddMeasurement([FromRoute] Guid id, [FromBody] AddMeasurementCommand body, CancellationToken ct)
    {
        var cmd = body with { CustomerId = id };
        await _sender.Send(cmd, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/notes")]
    [Authorize]
    public async Task<IActionResult> AddNote([FromRoute] Guid id, [FromBody] AddNoteCommand body, CancellationToken ct)
    {
        var cmd = body with { CustomerId = id };
        await _sender.Send(cmd, ct);
        return NoContent();
    }
}
