namespace Sgcae.Application.Abstractions.Messaging;

// EN: Here I define the CQRS contracts. I built my own minimal dispatcher instead of using MediatR
//     so the separation between commands (write) and queries (read) is explicit and has no license dependencies.
// ES: Aquí defino los contratos de CQRS. Construí mi propio despachador mínimo en lugar de usar MediatR
//     para que la separación entre comandos (escritura) y consultas (lectura) sea explícita y sin dependencias de licencia.

// EN: A COMMAND changes the state of the system and returns the result of the operation.
// ES: Un COMANDO cambia el estado del sistema y retorna el resultado de la operación.
public interface ICommand<TResult>
{
}

// EN: A QUERY only reads information; it must never modify the state.
// ES: Una CONSULTA solo lee información; nunca debe modificar el estado.
public interface IQuery<TResult>
{
}

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}

// EN: The dispatcher receives a command or a query and sends it to its handler. Controllers only know this interface.
// ES: El despachador recibe un comando o una consulta y lo envía a su manejador. Los controladores solo conocen esta interfaz.
public interface IDispatcher
{
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
