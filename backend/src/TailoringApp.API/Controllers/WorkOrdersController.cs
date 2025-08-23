using MediatR;
using Microsoft.AspNetCore.Mvc;
using TailoringApp.Application.WorkOrders.Commands.AddItem;
using TailoringApp.Application.WorkOrders.Commands.Cancel;
using TailoringApp.Application.WorkOrders.Commands.Complete;
using TailoringApp.Application.WorkOrders.Commands.CreateWorkOrder;
using TailoringApp.Application.WorkOrders.Commands.RemoveItem;
using TailoringApp.Application.WorkOrders.Commands.Start;
using TailoringApp.Application.WorkOrders.Commands.UpdateItemQuantity;
using TailoringApp.Application.WorkOrders.Commands.Discount;
using TailoringApp.Application.WorkOrders.Queries.GetCustomerWorkOrders;
using TailoringApp.Application.WorkOrders.Queries.GetWorkOrderDetails;
using Microsoft.AspNetCore.Authorization;

namespace TailoringApp.API.Controllers;

[ApiController]
[Route("api/customers/{customerId:guid}/workorders")]
public sealed class WorkOrdersController : ControllerBase
{
    private readonly ISender _sender;
    public WorkOrdersController(ISender sender) => _sender = sender;

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(Guid customerId, [FromBody] CreateRequest req, CancellationToken ct)
    {
    var id = await _sender.Send(new CreateWorkOrderCommand(customerId, req.Currency, req.AppointmentId), ct);
    if (id == Guid.Empty) return BadRequest("Invalid customer or appointment");
        return CreatedAtAction(nameof(GetForCustomer), new { customerId, page = 1, pageSize = 20 }, new { id });
    }

    [HttpGet]
    public async Task<IActionResult> GetForCustomer(Guid customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = false,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
    var pageResult = await _sender.Send(new GetCustomerWorkOrdersQuery(customerId, page, pageSize, sortBy, desc, status, fromUtc, toUtc, search), ct);
    return Ok(pageResult);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetails(Guid customerId, Guid id, CancellationToken ct)
    {
        var dto = await _sender.Send(new GetWorkOrderDetailsQuery(customerId, id), ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("{id:guid}/items")]
    [Authorize]
    public async Task<IActionResult> AddItem(Guid customerId, Guid id, [FromBody] AddItemRequest req, CancellationToken ct)
    {
        var ok = await _sender.Send(new AddWorkOrderItemCommand(
            id, 
            req.Description, 
            req.Quantity, 
            req.UnitPrice, 
            req.Currency,
            req.GarmentType,
            req.ChestMeasurement,
            req.WaistMeasurement,
            req.HipsMeasurement,
            req.SleeveMeasurement,
            req.MeasurementNotes
        ), ct);
        return ok ? NoContent() : BadRequest("Invalid id, status, or currency");
    }

    [HttpPut("{id:guid}/items/{description}")]
    [Authorize]
    public async Task<IActionResult> UpdateItemQuantity(Guid customerId, Guid id, string description, [FromBody] UpdateItemQuantityRequest req, CancellationToken ct)
    {
        var ok = await _sender.Send(new UpdateWorkOrderItemQuantityCommand(id, description, req.Quantity), ct);
        return ok ? NoContent() : BadRequest("Invalid id, status, or description");
    }

    [HttpDelete("{id:guid}/items/{description}")]
    [Authorize]
    public async Task<IActionResult> RemoveItem(Guid customerId, Guid id, string description, CancellationToken ct)
    {
        var ok = await _sender.Send(new RemoveWorkOrderItemCommand(id, description), ct);
        return ok ? NoContent() : BadRequest("Invalid id, status, or description");
    }

    [HttpPost("{id:guid}/start")]
    [Authorize]
    public async Task<IActionResult> Start(Guid customerId, Guid id, CancellationToken ct)
    {
    var ok = await _sender.Send(new StartWorkOrderCommand(id), ct);
    return ok ? NoContent() : BadRequest("Invalid id or status");
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize]
    public async Task<IActionResult> Complete(Guid customerId, Guid id, CancellationToken ct)
    {
    var ok = await _sender.Send(new CompleteWorkOrderCommand(id), ct);
    return ok ? NoContent() : BadRequest("Invalid id or status");
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Cancel(Guid customerId, Guid id, CancellationToken ct)
    {
    var ok = await _sender.Send(new CancelWorkOrderCommand(id), ct);
    return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/discount")] 
    [Authorize]
    public async Task<IActionResult> SetDiscount(Guid customerId, Guid id, [FromBody] SetDiscountRequest req, CancellationToken ct)
    {
        var ok = await _sender.Send(new SetWorkOrderDiscountCommand(id, req.Amount, req.Currency), ct);
        return ok ? NoContent() : BadRequest("Invalid discount");
    }

    [HttpDelete("{id:guid}/discount")] 
    [Authorize]
    public async Task<IActionResult> ClearDiscount(Guid customerId, Guid id, CancellationToken ct)
    {
        var ok = await _sender.Send(new ClearWorkOrderDiscountCommand(id), ct);
        return ok ? NoContent() : BadRequest("Invalid id or status");
    }

    public sealed record CreateRequest(string Currency, Guid? AppointmentId);
    public sealed record AddItemRequest(
        string Description, 
        int Quantity, 
        decimal UnitPrice, 
        string Currency,
        string? GarmentType = null,
        decimal? ChestMeasurement = null,
        decimal? WaistMeasurement = null,
        decimal? HipsMeasurement = null,
        decimal? SleeveMeasurement = null,
        string? MeasurementNotes = null
    );
    public sealed record UpdateItemQuantityRequest(int Quantity);
    public sealed record SetDiscountRequest(decimal Amount, string Currency);
}
