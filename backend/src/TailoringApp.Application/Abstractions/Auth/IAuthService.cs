using TailoringApp.Domain.Users;

namespace TailoringApp.Application.Abstractions.Auth;

public interface IAuthService
{
    Task<User?> ValidateUserAsync(string username, string password, CancellationToken ct);
    string GenerateToken(User user);
}
