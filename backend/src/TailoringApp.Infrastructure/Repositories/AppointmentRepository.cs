using Microsoft.EntityFrameworkCore;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Domain.Appointments;
using TailoringApp.Infrastructure.Persistence;

namespace TailoringApp.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly TailoringDbContext _db;
    public AppointmentRepository(TailoringDbContext db) => _db = db;

    public async Task AddAsync(Appointment appt, CancellationToken ct = default)
    {
        await _db.Appointments.AddAsync(appt, ct);
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Appointment> Items, int Total)> GetForCustomerAsync(Guid customerId, DateTime? fromUtc, DateTime? toUtc, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _db.Appointments.AsNoTracking().Where(a => a.CustomerId == customerId);
        if (fromUtc.HasValue) q = q.Where(a => a.StartUtc >= fromUtc.Value);
        if (toUtc.HasValue) q = q.Where(a => a.EndUtc <= toUtc.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(a => a.StartUtc).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<(IReadOnlyList<Appointment> Items, int Total)> GetPagedAsync(Guid? customerId, string? status, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _db.Appointments.AsNoTracking();
        if(customerId.HasValue) q = q.Where(a => a.CustomerId == customerId.Value);
        if(!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            if(Enum.TryParse<AppointmentStatus>(status, true, out var st))
            {
                q = q.Where(a => a.Status == st);
            }
        }
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(a => a.StartUtc).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<bool> HasConflictAsync(Guid customerId, DateTime startUtc, DateTime endUtc, Guid? excludeId = null, CancellationToken ct = default)
    {
        return await _db.Appointments.AnyAsync(a => a.CustomerId == customerId
            && a.Status == AppointmentStatus.Scheduled
            && (excludeId == null || a.Id != excludeId)
            && (startUtc < a.EndUtc && endUtc > a.StartUtc), ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
