using MediatR;
using Microsoft.AspNetCore.Mvc;
using TailoringApp.Application.WorkOrders.Queries.GetAllWorkOrders;
using TailoringApp.Application.WorkOrders.Queries.GetWorkOrderDetails;
using TailoringApp.Application.WorkOrders.Queries.GetWorkOrderSummary;
using TailoringApp.Application.WorkOrders.Commands.Start;
using TailoringApp.Application.WorkOrders.Commands.Complete;
using TailoringApp.Application.WorkOrders.Commands.Cancel;
using Microsoft.AspNetCore.Authorization;

namespace TailoringApp.API.Controllers;

[ApiController]
[Route("api/workorders")]
public sealed class AllWorkOrdersController : ControllerBase
{
    private readonly ISender _sender;
    public AllWorkOrdersController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = false,
        [FromQuery] string? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var pageResult = await _sender.Send(new GetAllWorkOrdersQuery(page, pageSize, sortBy, desc, status, customerId, fromUtc, toUtc, search), ct);
        return Ok(pageResult);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetails(Guid id, CancellationToken ct)
    {
        // First get basic work order info to find the customerId
        var workOrder = await _sender.Send(new GetWorkOrderSummaryQuery(id), ct);
        if (workOrder == null) return NotFound();
        
        // Then get full details using the customerId and workOrderId
        var details = await _sender.Send(new GetWorkOrderDetailsQuery(workOrder.CustomerId, id), ct);
        return details is null ? NotFound() : Ok(details);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        // First get basic work order info to find the customerId
        var workOrder = await _sender.Send(new GetWorkOrderSummaryQuery(id), ct);
        if (workOrder == null) return NotFound();
        
        // Update the status using the appropriate command
        bool ok = request.Status.ToLower() switch
        {
            "inprogress" => await _sender.Send(new StartWorkOrderCommand(id), ct),
            "completed" => await _sender.Send(new CompleteWorkOrderCommand(id), ct),
            "cancelled" => await _sender.Send(new CancelWorkOrderCommand(id), ct),
            _ => false
        };
        
        return ok ? NoContent() : BadRequest("Invalid status");
    }
}

public record UpdateStatusRequest(string Status);
