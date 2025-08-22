using MediatR;

namespace TailoringApp.Application.Customers.Commands.AddNote;

public record AddNoteCommand(Guid CustomerId, string Text, string? Author) : IRequest;
