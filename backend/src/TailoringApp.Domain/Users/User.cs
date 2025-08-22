using TailoringApp.Domain.Common;

namespace TailoringApp.Domain.Users;

public sealed class User : Entity
{
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty; // Admin or Staff
    public bool Enabled { get; private set; } = true;

    private User() { }
    private User(string username, string passwordHash, string role)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("username required", nameof(username));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("hash required", nameof(passwordHash));
        if (string.IsNullOrWhiteSpace(role)) throw new ArgumentException("role required", nameof(role));
        Username = username.ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
    }

    public static User Create(string username, string passwordHash, string role) => new(username, passwordHash, role);

    public void Disable() => Enabled = false;
}
