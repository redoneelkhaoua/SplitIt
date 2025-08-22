using MediatR;
using TailoringApp.Domain.Customers;
using TailoringApp.Application.Common.Paging;

namespace TailoringApp.Application.Customers.Queries.GetCustomers;

public record GetCustomersQuery(int Page = 1, int PageSize = 20, string? Search = null, string? SortBy = null, string SortDir = "desc", string Status = "enabled") : IRequest<GetCustomersResult>;

public sealed record GetCustomersResult : PagingResult<CustomerDto>
{
	public GetCustomersResult(IReadOnlyList<CustomerDto> items, int total, int page, int pageSize)
		: base(items, total, page, pageSize) { }
}

public record CustomerDto(Guid Id, string CustomerNumber, string FirstName, string LastName, string Email, DateTime RegistrationDate, bool Enabled);
