using System.Reflection;

namespace Sgcae.Application.Abstractions.Messaging;

// EN: This is the implementation of the dispatcher. It looks for the right handler in the dependency
//     injection container using the concrete type of the command/query.
// ES: Esta es la implementación del despachador. Busca el manejador correcto en el contenedor de
//     inyección de dependencias usando el tipo concreto del comando/consulta.
public sealed class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        return Invoke<TResult>(handlerType, command, cancellationToken);
    }

    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        return Invoke<TResult>(handlerType, query, cancellationToken);
    }

    // EN: Here I resolve the handler and call its HandleAsync method dynamically.
    // ES: Aquí resuelvo el manejador e invoco su método HandleAsync de forma dinámica.
    private Task<TResult> Invoke<TResult>(Type handlerType, object request, CancellationToken cancellationToken)
    {
        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for '{request.GetType().Name}'.");

        // EN: DoNotWrapExceptions keeps the original exception (e.g. NotFoundException) instead of wrapping it in a
        //     TargetInvocationException, so the middleware can return the correct HTTP status code.
        // ES: DoNotWrapExceptions conserva la excepción original (ej. NotFoundException) en lugar de envolverla en una
        //     TargetInvocationException, para que el middleware pueda retornar el código HTTP correcto.
        var method = handlerType.GetMethod("HandleAsync")!;
        return (Task<TResult>)method.Invoke(
            handler, BindingFlags.DoNotWrapExceptions, binder: null, new[] { request, cancellationToken }, culture: null)!;
    }
}
