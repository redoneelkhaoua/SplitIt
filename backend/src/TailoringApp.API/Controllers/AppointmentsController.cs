using MediatR;
using Microsoft.AspNetCore.Mvc;
using TailoringApp.Application.Appointments.Queries.GetAppointments;

namespace TailoringApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly ISender _sender;
    public AppointmentsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid? customerId, [FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetAppointmentsQuery(customerId, status, page, pageSize), ct);
        return Ok(result);
    }
}
