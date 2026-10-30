using Microsoft.Extensions.DependencyInjection;
using Sgcae.Application.Abstractions.Messaging;

namespace Sgcae.Application;

// EN: This class registers the Application layer services in the dependency injection container.
// ES: Esta clase registra los servicios de la capa de Aplicación en el contenedor de inyección de dependencias.
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        // EN: Here I scan this assembly and automatically register every command and query handler.
        //     This way, adding a new use case does not require touching this file.
        // ES: Aquí recorro este ensamblado y registro automáticamente todos los manejadores de comandos y consultas.
        //     Así, agregar un nuevo caso de uso no requiere modificar este archivo.
        var handlerInterfaces = new[] { typeof(ICommandHandler<,>), typeof(IQueryHandler<,>) };
        var types = typeof(DependencyInjection).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false });

        foreach (var type in types)
        {
            foreach (var @interface in type.GetInterfaces())
            {
                if (@interface.IsGenericType && handlerInterfaces.Contains(@interface.GetGenericTypeDefinition()))
                {
                    services.AddScoped(@interface, type);
                }
            }
        }

        return services;
    }
}
