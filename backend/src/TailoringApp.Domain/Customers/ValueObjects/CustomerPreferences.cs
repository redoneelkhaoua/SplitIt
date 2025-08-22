namespace TailoringApp.Domain.Customers.ValueObjects;

public sealed class CustomerPreferences
{
    public string Style { get; }
    public string Fit { get; }
    public string? Notes { get; }

    private CustomerPreferences() { Style = string.Empty; Fit = string.Empty; }

    public CustomerPreferences(string style, string fit, string? notes = null)
    {
        Style = (style ?? string.Empty).Trim();
        Fit = (fit ?? string.Empty).Trim();
        Notes = notes?.Trim();
    }
}
