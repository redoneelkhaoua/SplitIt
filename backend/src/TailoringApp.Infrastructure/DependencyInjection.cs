using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TailoringApp.Application;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Infrastructure.Persistence;
using TailoringApp.Infrastructure.Repositories;
using TailoringApp.Infrastructure.Auth;
using TailoringApp.Application.Abstractions.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TailoringApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var cs = configuration.GetConnectionString("Default")
                 ?? "Server=(localdb)\\MSSQLLocalDB;Database=TailoringDB;Trusted_Connection=True;MultipleActiveResultSets=True";
        // Skip registering SQL Server if: running in Testing environment (integration tests) OR already registered.
        var alreadyHasDbContext = services.Any(s => s.ServiceType == typeof(DbContextOptions<TailoringDbContext>));
        var isTesting = string.Equals(environment, "Testing", StringComparison.OrdinalIgnoreCase);
        if (!alreadyHasDbContext && !isTesting)
        {
            services.AddDbContext<TailoringDbContext>(options =>
            {
                options.UseSqlServer(cs, sql => sql.MigrationsAssembly(typeof(TailoringDbContext).Assembly.FullName));
            });
        }

        // Auth
        services.AddScoped<IAuthService, AuthService>();
        var key = configuration["Jwt:Key"] ?? "dev_super_secret_key_change_replace_me_with_longer_64b"; // fallback dev key (>=32 bytes)
        if (Encoding.UTF8.GetBytes(key).Length < 32)
        {
            throw new InvalidOperationException("JWT signing key length insufficient. Provide Jwt:Key with at least 32 bytes.");
        }
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
        });

    // repositories
    services.AddScoped<ICustomerRepository, CustomerRepository>();
    services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
    services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        return services;
    }
}
