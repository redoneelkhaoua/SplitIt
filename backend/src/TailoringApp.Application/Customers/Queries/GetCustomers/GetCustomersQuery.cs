using MediatR;
using TailoringApp.Domain.Customers;

namespace TailoringApp.Application.Customers.Queries.GetCustomers;

public record GetCustomersQuery(int Page = 1, int PageSize = 20, string? Search = null, string? SortBy = null, string SortDir = "desc", string Status = "enabled") : IRequest<GetCustomersResult>;

public record GetCustomersResult(
	IReadOnlyList<CustomerDto> Items,
	int Total,
	int Page,
	int PageSize,
	int TotalPages,
	bool HasNext,
	bool HasPrevious
);

public record CustomerDto(Guid Id, string CustomerNumber, string FirstName, string LastName, string Email, DateTime RegistrationDate, bool Enabled);
