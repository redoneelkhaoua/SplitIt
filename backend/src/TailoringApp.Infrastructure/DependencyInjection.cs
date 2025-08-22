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
        var cs = configuration.GetConnectionString("Default")
                 ?? "Server=(localdb)\\MSSQLLocalDB;Database=TailoringDB;Trusted_Connection=True;MultipleActiveResultSets=True";
        services.AddDbContext<TailoringDbContext>(options =>
        {
            options.UseSqlServer(cs, sql => sql.MigrationsAssembly(typeof(TailoringDbContext).Assembly.FullName));
        });

        // Auth
        services.AddScoped<IAuthService, AuthService>();
        var key = configuration["Jwt:Key"] ?? "dev_super_secret_key_change";
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
