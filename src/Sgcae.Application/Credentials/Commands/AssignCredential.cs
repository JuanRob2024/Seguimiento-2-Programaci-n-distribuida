using Sgcae.Application.Abstractions;
using Sgcae.Application.Abstractions.Messaging;
using Sgcae.Application.Common;
using Sgcae.Domain.Credentials;
using Sgcae.Domain.Repositories;

namespace Sgcae.Application.Credentials.Commands;

// EN: COMMAND (RF-02): assign a credential to an employee. If no code is sent, I generate one.
// ES: COMANDO (RF-02): asignar una credencial a un empleado. Si no se envía código, genero uno.
public sealed record AssignCredentialCommand(
    Guid EmployeeId,
    string? Code,
    DateTime? ExpiresAtUtc) : ICommand<CredentialDto>;

public sealed class AssignCredentialHandler : ICommandHandler<AssignCredentialCommand, CredentialDto>
{
    private readonly IEmployeeRepository _employees;
    private readonly ICredentialRepository _credentials;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public AssignCredentialHandler(
        IEmployeeRepository employees,
        ICredentialRepository credentials,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _employees = employees;
        _credentials = credentials;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<CredentialDto> HandleAsync(AssignCredentialCommand command, CancellationToken cancellationToken)
    {
        var employee = await _employees.GetByIdAsync(command.EmployeeId, cancellationToken)
            ?? throw new NotFoundException("Employee", command.EmployeeId);

        // EN: Business rule: an inactive employee can not receive credentials.
        // ES: Regla de negocio: un empleado inactivo no puede recibir credenciales.
        if (!employee.IsActive)
        {
            throw new ConflictException("Credentials can not be assigned to an inactive employee.");
        }

        // EN: Business rule: an employee can hold only ONE non-revoked credential (active or blocked) at a time.
        // ES: Regla de negocio: un empleado solo puede tener UNA credencial no revocada (activa o bloqueada) a la vez.
        var current = await _credentials.GetNonRevokedByEmployeeAsync(employee.Id, cancellationToken);
        if (current.Count > 0)
        {
            throw new ConflictException(
                $"The employee already has the credential '{current[0].Code}'. Revoke it before assigning a new one.");
        }

        var code = string.IsNullOrWhiteSpace(command.Code)
            ? CredentialCode.Generate()
            : CredentialCode.Create(command.Code);

        if (await _credentials.ExistsByCodeAsync(code, cancellationToken))
        {
            throw new ConflictException($"The credential code '{code}' is already in use.");
        }

        var now = _clock.UtcNow;
        var credential = Credential.Issue(employee.Id, code, now, command.ExpiresAtUtc);

        _credentials.Add(credential);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CredentialDto.From(credential, now);
    }
}
