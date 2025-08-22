namespace TailoringApp.Domain.Customers.Entities;

public class MeasurementRecord
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime Date { get; private set; }
    public decimal Chest { get; private set; }
    public decimal Waist { get; private set; }
    public decimal Hips { get; private set; }
    public decimal Sleeve { get; private set; }

    private MeasurementRecord() {}

    public MeasurementRecord(DateTime date, decimal chest, decimal waist, decimal hips, decimal sleeve)
    {
        Date = date;
        Chest = chest;
        Waist = waist;
        Hips = hips;
        Sleeve = sleeve;
    }
}
