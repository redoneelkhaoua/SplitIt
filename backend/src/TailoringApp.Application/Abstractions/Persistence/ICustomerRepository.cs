using TailoringApp.Domain.Customers;

namespace TailoringApp.Application.Abstractions.Persistence;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Customer?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default);
    Task<Customer?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default);
    Task<Customer?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Customer> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search, string? sortBy, bool sortDesc, string status, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
