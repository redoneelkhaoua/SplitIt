namespace TailoringApp.Domain.Customers.ValueObjects;

public sealed class ContactInfo
{
    public string Email { get; }
    public string Phone { get; }
    public string Address { get; }

    private ContactInfo() { Email = string.Empty; Phone = string.Empty; Address = string.Empty; }

    public ContactInfo(string email, string phone, string address)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Invalid email", nameof(email));
        Email = email.Trim();
        Phone = phone?.Trim() ?? string.Empty;
        Address = address?.Trim() ?? string.Empty;
    }
}
