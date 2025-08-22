using TailoringApp.Domain.Common;

namespace TailoringApp.Domain.WorkOrders;

public sealed class WorkOrder : Entity
{
    private readonly List<WorkOrderItem> _items = new();

    public Guid CustomerId { get; private set; }
    public Guid? AppointmentId { get; private set; }
    public string Currency { get; private set; } = "USD";
    public WorkOrderStatus Status { get; private set; } = WorkOrderStatus.Draft;
    public IReadOnlyCollection<WorkOrderItem> Items => _items;
    public Money Subtotal => _items.Aggregate(Money.Zero(Currency), (sum, i) => sum.Add(i.LineTotal()));
    public Money? Discount { get; private set; }
    public Money Total
    {
        get
        {
            var subtotal = Subtotal;
            if (Discount is null) return subtotal;
            var applied = Discount.Amount > subtotal.Amount ? subtotal : Discount;
            return subtotal.Subtract(applied);
        }
    }

    private WorkOrder() { }

    private WorkOrder(Guid customerId, string currency, Guid? appointmentId = null)
    {
        if (customerId == Guid.Empty) throw new ArgumentException("Customer required", nameof(customerId));
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3) throw new ArgumentException("Currency 3-letter code", nameof(currency));
        CustomerId = customerId;
        Currency = currency.ToUpperInvariant();
        AppointmentId = appointmentId;
    }

    public static WorkOrder Create(Guid customerId, string currency, Guid? appointmentId = null)
        => new(customerId, currency, appointmentId);

    public void LinkAppointment(Guid appointmentId)
    {
        if (appointmentId == Guid.Empty) throw new ArgumentException(nameof(appointmentId));
        AppointmentId = appointmentId;
    }

    public void AddItem(string description, int quantity, Money unitPrice)
    {
        if (Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot modify a finalized work order");
        if (!string.Equals(unitPrice.Currency, Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Item currency must match work order currency");
        _items.Add(new WorkOrderItem(description, quantity, unitPrice));
    }

    public bool RemoveItem(int index)
    {
        if (Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled) return false;
        if (index < 0 || index >= _items.Count) return false;
        _items.RemoveAt(index);
        return true;
    }

    public bool RemoveItem(string description)
    {
        if (Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled) return false;
        var idx = _items.FindIndex(i => string.Equals(i.Description, description, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) return false;
        _items.RemoveAt(idx);
        return true;
    }

    public bool UpdateItemQuantity(string description, int quantity)
    {
        if (Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled) return false;
        if (quantity <= 0) return false;
        var found = _items.Any(i => string.Equals(i.Description, description, StringComparison.OrdinalIgnoreCase));
        if (!found) return false;
        var rebuilt = _items
            .Select(i => string.Equals(i.Description, description, StringComparison.OrdinalIgnoreCase)
                ? new WorkOrderItem(i.Description, quantity, i.UnitPrice)
                : i)
            .ToList();
        _items.Clear();
        _items.AddRange(rebuilt);
        return true;
    }

    public void Start()
    {
        if (Status != WorkOrderStatus.Draft) throw new InvalidOperationException("Can start only from Draft");
        Status = WorkOrderStatus.InProgress;
    }

    public void SetDiscount(Money discount)
    {
        if (Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot modify a finalized work order");
        if (!string.Equals(discount.Currency, Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Discount currency must match work order currency");
        if (discount.Amount < 0) throw new ArgumentOutOfRangeException(nameof(discount.Amount));
        Discount = discount.Amount == 0 ? null : discount;
    }

    public void ClearDiscount()
    {
        if (Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot modify a finalized work order");
        Discount = null;
    }

    public void Complete()
    {
        if (Status != WorkOrderStatus.InProgress) throw new InvalidOperationException("Can complete only from InProgress");
        Status = WorkOrderStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == WorkOrderStatus.Completed) throw new InvalidOperationException("Cannot cancel a completed work order");
        Status = WorkOrderStatus.Cancelled;
    }
}
