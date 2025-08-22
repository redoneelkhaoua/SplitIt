using TailoringApp.Domain.Common;

namespace TailoringApp.Domain.WorkOrders;

public sealed class WorkOrderItem : ValueObject
{
    public string Description { get; } = string.Empty;
    public int Quantity { get; }
    public Money UnitPrice { get; } = new Money(0m, "USD");

    private WorkOrderItem() { }

    public WorkOrderItem(string description, int quantity, Money unitPrice)
    {
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description required", nameof(description));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        Description = description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
    }

    public Money LineTotal() => new(UnitPrice.Amount * Quantity, UnitPrice.Currency);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Description;
        yield return Quantity;
        yield return UnitPrice;
    }
}
