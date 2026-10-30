using Sgcae.Application.Abstractions;
using Sgcae.Application.Abstractions.Messaging;
using Sgcae.Application.Common;
using Sgcae.Domain.Repositories;

namespace Sgcae.Application.Employees.Commands;

// EN: COMMAND (RF-01): deactivate an employee (logical deletion: the history is preserved).
// ES: COMANDO (RF-01): desactivar un empleado (eliminación lógica: el historial se conserva).
public sealed record DeactivateEmployeeCommand(Guid EmployeeId) : ICommand<EmployeeDto>;

public sealed class DeactivateEmployeeHandler : ICommandHandler<DeactivateEmployeeCommand, EmployeeDto>
{
    private const string RevocationReason = "Employee deactivated.";

    private readonly IEmployeeRepository _employees;
    private readonly ICredentialRepository _credentials;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public DeactivateEmployeeHandler(
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

    public async Task<EmployeeDto> HandleAsync(DeactivateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var employee = await _employees.GetByIdAsync(command.EmployeeId, cancellationToken)
            ?? throw new NotFoundException("Employee", command.EmployeeId);

        var now = _clock.UtcNow;
        employee.Deactivate(now);

        // EN: Here I also revoke every credential that is still valid, so the inactive employee can not use them.
        //     The employee and the credentials are saved in the same transaction (Unit of Work).
        // ES: Aquí también revoco todas las credenciales que siguen vigentes, para que el empleado inactivo no pueda
        //     usarlas. El empleado y las credenciales se guardan en la misma transacción (Unidad de Trabajo).
        var credentials = await _credentials.GetNonRevokedByEmployeeAsync(employee.Id, cancellationToken);
        foreach (var credential in credentials)
        {
            credential.Revoke(RevocationReason, now);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return EmployeeDto.From(employee);
    }
}
