using TailoringApp.Domain.Appointments;

namespace TailoringApp.Application.Abstractions.Persistence;

public interface IAppointmentRepository
{
    Task AddAsync(Appointment appt, CancellationToken ct = default);
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Appointment> Items, int Total)> GetForCustomerAsync(Guid customerId, DateTime? fromUtc, DateTime? toUtc, int page, int pageSize, CancellationToken ct = default);
    Task<(IReadOnlyList<Appointment> Items, int Total)> GetPagedAsync(Guid? customerId, string? status, int page, int pageSize, CancellationToken ct = default);
    Task<bool> HasConflictAsync(Guid customerId, DateTime startUtc, DateTime endUtc, Guid? excludeId = null, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
