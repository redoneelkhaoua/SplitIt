using TailoringApp.Domain.WorkOrders;

namespace TailoringApp.Application.Abstractions.Persistence;

public interface IWorkOrderRepository
{
    Task AddAsync(WorkOrder entity, CancellationToken ct);
    Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<WorkOrder?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task<(IReadOnlyList<WorkOrder> Items, int TotalCount)> GetForCustomerAsync(
        Guid customerId,
        int page,
        int pageSize,
        string? sortBy,
    bool desc,
    string? status,
    DateTime? fromUtc,
    DateTime? toUtc,
    string? search,
        CancellationToken ct);
}
