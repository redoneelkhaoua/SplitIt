using Microsoft.EntityFrameworkCore;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Domain.WorkOrders;
using TailoringApp.Infrastructure.Persistence;

namespace TailoringApp.Infrastructure.Repositories;

public sealed class WorkOrderRepository : IWorkOrderRepository
{
    private readonly TailoringDbContext _db;
    public WorkOrderRepository(TailoringDbContext db) => _db = db;

    public async Task AddAsync(WorkOrder entity, CancellationToken ct)
        => await _db.Set<WorkOrder>().AddAsync(entity, ct);

    public Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Set<WorkOrder>().AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<WorkOrder?> GetByIdForUpdateAsync(Guid id, CancellationToken ct)
        => _db.Set<WorkOrder>().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    public async Task<(IReadOnlyList<WorkOrder> Items, int TotalCount)> GetForCustomerAsync(Guid customerId, int page, int pageSize, string? sortBy, bool desc, string? status, DateTime? fromUtc, DateTime? toUtc, string? search, CancellationToken ct)
    {
        var query = _db.Set<WorkOrder>().AsNoTracking().Include(x => x.Items)
            .Where(x => x.CustomerId == customerId && x.Enabled);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<WorkOrderStatus>(status, true, out var st))
        {
            query = query.Where(x => x.Status == st);
        }
        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.CreatedDate >= fromUtc.Value);
        }
        if (toUtc.HasValue)
        {
            query = query.Where(x => x.CreatedDate <= toUtc.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(x => x.Items.Any(i => i.Description.ToLower().Contains(s)));
        }

        query = (sortBy?.ToLowerInvariant(), desc) switch
        {
            ("created", true) => query.OrderByDescending(x => x.CreatedDate),
            ("created", false) => query.OrderBy(x => x.CreatedDate),
            ("status", true) => query.OrderByDescending(x => x.Status).ThenByDescending(x => x.CreatedDate),
            ("status", false) => query.OrderBy(x => x.Status).ThenBy(x => x.CreatedDate),
            _ => query.OrderByDescending(x => x.CreatedDate)
        };

    var total = await query.CountAsync(ct);
    var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
