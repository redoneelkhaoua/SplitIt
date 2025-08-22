using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TailoringApp.Application.Abstractions.Auth;
using TailoringApp.Domain.Users;
using TailoringApp.Infrastructure.Persistence;

namespace TailoringApp.Infrastructure.Auth;

public sealed class AuthService : IAuthService
{
    private readonly TailoringDbContext _db;
    private readonly IConfiguration _config;
    private readonly string _jwtKey;

    public AuthService(TailoringDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
        _jwtKey = _config["Jwt:Key"] ?? "dev_super_secret_key_change_replace_me_with_longer_64b"; // dev fallback (>=32 bytes)
        if (Encoding.UTF8.GetBytes(_jwtKey).Length < 32)
        {
            throw new InvalidOperationException("JWT signing key length insufficient. Provide Jwt:Key with at least 32 bytes.");
        }
    }

    public async Task<User?> ValidateUserAsync(string username, string password, CancellationToken ct)
    {
        var hash = Hash(password);
        return await _db.Users.FirstOrDefaultAsync(u => u.Username == username.ToLower() && u.PasswordHash == hash && u.Enabled, ct);
    }

    public string GenerateToken(User user)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);
        return handler.WriteToken(token);
    }

    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(input)));
    }
}
