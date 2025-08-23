using TailoringApp.Domain.Common;

namespace TailoringApp.Domain.WorkOrders;

public sealed class WorkOrderItem : ValueObject
{
    public string Description { get; } = string.Empty;
    public int Quantity { get; }
    public Money UnitPrice { get; } = new Money(0m, "USD");
    public GarmentType GarmentType { get; }
    public GarmentMeasurements? Measurements { get; }

    private WorkOrderItem() { }

    public WorkOrderItem(string description, int quantity, Money unitPrice, GarmentType garmentType = GarmentType.Other, GarmentMeasurements? measurements = null)
    {
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description required", nameof(description));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        Description = description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        GarmentType = garmentType;
        Measurements = measurements;
    }

    public Money LineTotal() => new(UnitPrice.Amount * Quantity, UnitPrice.Currency);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Description;
        yield return Quantity;
        yield return UnitPrice;
        yield return GarmentType;
        yield return Measurements;
    }
}
