using TailoringApp.Domain.Common;

namespace TailoringApp.Domain.Appointments;

public class Appointment : Entity
{
    public Guid CustomerId { get; private set; }
    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public string? Notes { get; private set; }
    public AppointmentStatus Status { get; private set; }

    private Appointment() { }

    public Appointment(Guid customerId, DateTime startUtc, DateTime endUtc, string? notes)
    {
        if (endUtc <= startUtc) throw new ArgumentException("End must be after Start");
        CustomerId = customerId;
        StartUtc = startUtc;
        EndUtc = endUtc;
        Notes = notes;
        Status = AppointmentStatus.Scheduled;
    }

    public void Reschedule(DateTime newStartUtc, DateTime newEndUtc)
    {
        if (Status != AppointmentStatus.Scheduled) throw new InvalidOperationException("Only scheduled appointments can be rescheduled");
        if (newEndUtc <= newStartUtc) throw new ArgumentException("End must be after Start");
        StartUtc = newStartUtc;
        EndUtc = newEndUtc;
    }

    public void Complete()
    {
        if (Status != AppointmentStatus.Scheduled) throw new InvalidOperationException("Only scheduled appointments can be completed");
        Status = AppointmentStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == AppointmentStatus.Completed) throw new InvalidOperationException("Cannot cancel a completed appointment");
        Status = AppointmentStatus.Cancelled;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
