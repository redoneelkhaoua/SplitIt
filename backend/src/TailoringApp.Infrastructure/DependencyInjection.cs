using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TailoringApp.Application;
using TailoringApp.Application.Abstractions.Persistence;
using TailoringApp.Infrastructure.Persistence;
using TailoringApp.Infrastructure.Repositories;

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

    // repositories
    services.AddScoped<ICustomerRepository, CustomerRepository>();
    services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
    services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        return services;
    }
}
