using Microsoft.EntityFrameworkCore;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Domain.Customers;
using TailoringApp.Infrastructure.Persistence;

namespace TailoringApp.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly TailoringDbContext _db;

    public CustomerRepository(TailoringDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Customer customer, CancellationToken ct = default)
    {
        await _db.Customers.AddAsync(customer, ct);
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Customer?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Customers
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Customer?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<(IReadOnlyList<Customer> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search, string? sortBy, bool sortDesc, string status, CancellationToken ct = default)
    {
        var query = _db.Customers.AsNoTracking().AsQueryable();
        // Status filter (enabled = true/false or all)
        switch (status.ToLowerInvariant())
        {
            case "enabled":
                query = query.Where(c => c.Enabled);
                break;
            case "disabled":
                query = query.Where(c => !c.Enabled);
                break;
            case "all":
            default:
                break;
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(c =>
                c.CustomerNumber.Contains(search) ||
                c.ContactInfo.Email.Contains(search) ||
                c.PersonalInfo.FirstName.Contains(search) ||
                c.PersonalInfo.LastName.Contains(search));
        }

        var total = await query.CountAsync(ct);

        // Sorting
        query = (sortBy?.ToLowerInvariant()) switch
        {
            "firstname" => sortDesc ? query.OrderByDescending(c => c.PersonalInfo.FirstName) : query.OrderBy(c => c.PersonalInfo.FirstName),
            "lastname" => sortDesc ? query.OrderByDescending(c => c.PersonalInfo.LastName) : query.OrderBy(c => c.PersonalInfo.LastName),
            "email" => sortDesc ? query.OrderByDescending(c => c.ContactInfo.Email) : query.OrderBy(c => c.ContactInfo.Email),
            "customernumber" => sortDesc ? query.OrderByDescending(c => c.CustomerNumber) : query.OrderBy(c => c.CustomerNumber),
            "created" or "registrationdate" => sortDesc ? query.OrderByDescending(c => c.RegistrationDate) : query.OrderBy(c => c.RegistrationDate),
            _ => query.OrderByDescending(c => c.RegistrationDate)
        };

    var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Customer?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Customers
            .Include(c => c.MeasurementHistory)
            .Include(c => c.Notes)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }
}
