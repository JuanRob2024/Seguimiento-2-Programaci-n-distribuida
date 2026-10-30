using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sgcae.Application.Abstractions;
using Sgcae.Application.Common;
using Sgcae.Domain.Repositories;
using Sgcae.Infrastructure.Persistence;
using Sgcae.Infrastructure.Persistence.Queries;
using Sgcae.Infrastructure.Persistence.Repositories;
using Sgcae.Infrastructure.Services;

namespace Sgcae.Infrastructure;

// EN: This class registers the Infrastructure implementations: here the interfaces of the inner layers are
//     connected with their concrete classes (EF Core + SQL Server).
// ES: Esta clase registra las implementaciones de Infraestructura: aquí se conectan las interfaces de las capas
//     internas con sus clases concretas (EF Core + SQL Server).
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("The connection string 'SgcaeDb' is not configured.");
        }

        // EN: Here I configure EF Core to use SQL Server, with retries for transient failures.
        // ES: Aquí configuro EF Core para usar SQL Server, con reintentos ante fallos transitorios.
        services.AddDbContext<SgcaeDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(maxRetryCount: 3)));

        // EN: The same DbContext instance (scoped per HTTP request) acts as the Unit of Work.
        // ES: La misma instancia del DbContext (una por petición HTTP) actúa como Unidad de Trabajo.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SgcaeDbContext>());

        // EN: Write side (repositories of the domain).
        // ES: Lado de escritura (repositorios del dominio).
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ICredentialRepository, CredentialRepository>();
        services.AddScoped<IAccessPointRepository, AccessPointRepository>();
        services.AddScoped<IAccessLogRepository, AccessLogRepository>();

        // EN: Read side (query services).
        // ES: Lado de lectura (servicios de consulta).
        services.AddScoped<IEmployeeQueryService, EmployeeQueryService>();
        services.AddScoped<ICredentialQueryService, CredentialQueryService>();
        services.AddScoped<IAccessPointQueryService, AccessPointQueryService>();
        services.AddScoped<IAccessLogQueryService, AccessLogQueryService>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}
