using MediatR;
using TailoringApp.Application.Abstractions.Persistence;

namespace TailoringApp.Application.Customers.Commands.AddNote;

public class AddNoteCommandHandler : IRequestHandler<AddNoteCommand>
{
    private readonly ICustomerRepository _repo;
    public AddNoteCommandHandler(ICustomerRepository repo) => _repo = repo;

    public async Task Handle(AddNoteCommand request, CancellationToken ct)
    {
        var customer = await _repo.GetByIdForUpdateAsync(request.CustomerId, ct);
        if (customer is null) return; // or throw NotFoundException
        customer.AddNote(request.Text, request.Author);
        await _repo.SaveChangesAsync(ct);
    }
}
