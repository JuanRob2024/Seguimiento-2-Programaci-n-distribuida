using Sgcae.Application.Abstractions.Messaging;
using Sgcae.Application.Common;

namespace Sgcae.Application.Credentials.Queries;

// EN: QUERIES (RF-02): consult credentials and their status.
// ES: CONSULTAS (RF-02): consultar credenciales y su estado.
public sealed record GetCredentialByIdQuery(Guid CredentialId) : IQuery<CredentialDto>;

public sealed record GetCredentialByCodeQuery(string Code) : IQuery<CredentialDto>;

public sealed record GetEmployeeCredentialsQuery(Guid EmployeeId) : IQuery<IReadOnlyList<CredentialDto>>;

public sealed class CredentialQueriesHandler :
    IQueryHandler<GetCredentialByIdQuery, CredentialDto>,
    IQueryHandler<GetCredentialByCodeQuery, CredentialDto>,
    IQueryHandler<GetEmployeeCredentialsQuery, IReadOnlyList<CredentialDto>>
{
    private readonly ICredentialQueryService _credentials;
    private readonly IEmployeeQueryService _employees;

    public CredentialQueriesHandler(ICredentialQueryService credentials, IEmployeeQueryService employees)
    {
        _credentials = credentials;
        _employees = employees;
    }

    public async Task<CredentialDto> HandleAsync(GetCredentialByIdQuery query, CancellationToken cancellationToken) =>
        await _credentials.GetByIdAsync(query.CredentialId, cancellationToken)
        ?? throw new NotFoundException("Credential", query.CredentialId);

    public async Task<CredentialDto> HandleAsync(GetCredentialByCodeQuery query, CancellationToken cancellationToken) =>
        await _credentials.GetByCodeAsync(query.Code, cancellationToken)
        ?? throw new NotFoundException("Credential", query.Code);

    public async Task<IReadOnlyList<CredentialDto>> HandleAsync(
        GetEmployeeCredentialsQuery query, CancellationToken cancellationToken)
    {
        // EN: Here I first confirm that the employee exists, to answer 404 instead of an empty list.
        // ES: Aquí primero confirmo que el empleado exista, para responder 404 en vez de una lista vacía.
        _ = await _employees.GetByIdAsync(query.EmployeeId, cancellationToken)
            ?? throw new NotFoundException("Employee", query.EmployeeId);

        return await _credentials.GetByEmployeeAsync(query.EmployeeId, cancellationToken);
    }
}
