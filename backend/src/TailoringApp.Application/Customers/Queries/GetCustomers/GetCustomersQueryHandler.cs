using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.Customers.Queries.GetCustomers;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, GetCustomersResult>
{
    private readonly ICustomerRepository _repo;
    public GetCustomersQueryHandler(ICustomerRepository repo) => _repo = repo;

    public async Task<GetCustomersResult> Handle(GetCustomersQuery request, CancellationToken ct)
    {
    var sortDesc = string.Equals(request.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
    var (items, total) = await _repo.GetPagedAsync(request.Page, request.PageSize, request.Search, request.SortBy, sortDesc, request.Status, ct);
        var dtos = items.Select(c => new CustomerDto(
            c.Id,
            c.CustomerNumber,
            c.PersonalInfo.FirstName,
            c.PersonalInfo.LastName,
            c.ContactInfo.Email,
            c.RegistrationDate,
            c.Enabled
        )).ToList();

    var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);
    var hasNext = request.Page < totalPages;
    var hasPrevious = request.Page > 1 && totalPages > 0;
    return new GetCustomersResult(dtos, total, request.Page, request.PageSize, totalPages, hasNext, hasPrevious);
    }
}
