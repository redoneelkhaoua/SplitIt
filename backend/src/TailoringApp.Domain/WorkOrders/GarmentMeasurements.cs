using TailoringApp.Domain.Common;

namespace TailoringApp.Domain.WorkOrders;

public sealed class GarmentMeasurements : ValueObject
{
    public decimal Chest { get; }
    public decimal Waist { get; }
    public decimal Hips { get; }
    public decimal Sleeve { get; }
    public string? Notes { get; }

    private GarmentMeasurements() 
    {
        Notes = string.Empty;
    }

    public GarmentMeasurements(decimal chest, decimal waist, decimal hips, decimal sleeve, string? notes = null)
    {
        if (chest < 0) throw new ArgumentOutOfRangeException(nameof(chest));
        if (waist < 0) throw new ArgumentOutOfRangeException(nameof(waist));
        if (hips < 0) throw new ArgumentOutOfRangeException(nameof(hips));
        if (sleeve < 0) throw new ArgumentOutOfRangeException(nameof(sleeve));
        
        Chest = chest;
        Waist = waist;
        Hips = hips;
        Sleeve = sleeve;
        Notes = notes?.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Chest;
        yield return Waist;
        yield return Hips;
        yield return Sleeve;
        yield return Notes;
    }
}
