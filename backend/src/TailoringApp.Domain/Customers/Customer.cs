using MediatR;
using TailoringApp.Domain.Common;
using TailoringApp.Domain.Customers.Entities;
using TailoringApp.Domain.Customers.Events;
using TailoringApp.Domain.Customers.ValueObjects;

namespace TailoringApp.Domain.Customers;

public class Customer : Entity
{
    private readonly List<MeasurementRecord> _measurements = new();
    private readonly List<CustomerNote> _notes = new();

    public string CustomerNumber { get; private set; }
    public PersonalInfo PersonalInfo { get; private set; }
    public ContactInfo ContactInfo { get; private set; }
    public CustomerPreferences Preferences { get; private set; }
    public CustomerStatus Status { get; private set; }
    public decimal TotalSpent { get; private set; }
    public DateTime RegistrationDate { get; private set; }

    public IReadOnlyCollection<MeasurementRecord> MeasurementHistory => _measurements.AsReadOnly();
    public IReadOnlyCollection<CustomerNote> Notes => _notes.AsReadOnly();

    private Customer()
    {
    CustomerNumber = string.Empty;
    // Let EF populate owned types via their private constructors during materialization
    PersonalInfo = default!;
    ContactInfo = default!;
    Preferences = default!;
    }

    public Customer(string customerNumber, PersonalInfo personal, ContactInfo contact, CustomerPreferences prefs)
    {
        if (string.IsNullOrWhiteSpace(customerNumber)) throw new ArgumentException("Customer number required", nameof(customerNumber));
        CustomerNumber = customerNumber.Trim();
        PersonalInfo = personal;
        ContactInfo = contact;
        Preferences = prefs;
        Status = CustomerStatus.Active;
        TotalSpent = 0m;
        RegistrationDate = DateTime.UtcNow;
        Raise(new CustomerRegistered(Id, ContactInfo.Email));
    }

    public void UpdateContactInfo(ContactInfo newContact)
    {
        ContactInfo = newContact;
    }

    public void UpdatePersonalInfo(PersonalInfo newPersonal)
    {
        PersonalInfo = newPersonal;
    }

    public void UpdatePreferences(CustomerPreferences newPrefs)
    {
        Preferences = newPrefs;
    }

    public void AddMeasurement(MeasurementRecord record)
    {
        _measurements.Add(record);
        // Raise measurements taken event if needed
    }

    public void AddNote(string text, string? author)
    {
        _notes.Add(new CustomerNote(text, author));
        // Raise note added event if needed
    }

    public void PromoteToVIP()
    {
        if (Status == CustomerStatus.VIP) return;
        if (TotalSpent < 1000m) throw new InvalidOperationException("Customer must spend at least 1000 to be VIP");
        Status = CustomerStatus.VIP;
    }
}

public class CustomerNote
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime Date { get; private set; }
    public string Text { get; private set; }
    public string? Author { get; private set; }

    private CustomerNote() { Text = string.Empty; }

    public CustomerNote(string text, string? author)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Text required", nameof(text));
        Date = DateTime.UtcNow;
        Text = text.Trim();
        Author = author?.Trim();
    }
}
