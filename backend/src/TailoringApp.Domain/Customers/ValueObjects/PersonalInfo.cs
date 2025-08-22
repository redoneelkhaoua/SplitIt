namespace TailoringApp.Domain.Customers.ValueObjects;

public sealed class PersonalInfo
{
    public string FirstName { get; } = string.Empty;
    public string LastName { get; } = string.Empty;
    public DateTime? DateOfBirth { get; }

    // EF Core may use this when materializing owned types
    private PersonalInfo() { }

    public PersonalInfo(string firstName, string lastName, DateTime? dateOfBirth = null)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name required", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name required", nameof(lastName));
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        DateOfBirth = dateOfBirth;
    }
}
