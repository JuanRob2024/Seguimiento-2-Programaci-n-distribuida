using Sgcae.Application.Abstractions;
using Sgcae.Application.Abstractions.Messaging;
using Sgcae.Application.Common;
using Sgcae.Domain.Employees;
using Sgcae.Domain.Repositories;

namespace Sgcae.Application.Employees.Commands;

// EN: COMMAND (RF-01): update the information of an existing employee.
// ES: COMANDO (RF-01): actualizar la información de un empleado existente.
public sealed record UpdateEmployeeCommand(
    Guid EmployeeId,
    string FirstNames,
    string LastNames,
    string Email,
    string Position,
    string Area) : ICommand<EmployeeDto>;

public sealed class UpdateEmployeeHandler : ICommandHandler<UpdateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employees;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public UpdateEmployeeHandler(IEmployeeRepository employees, IUnitOfWork unitOfWork, IDateTimeProvider clock)
    {
        _employees = employees;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<EmployeeDto> HandleAsync(UpdateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var employee = await _employees.GetByIdAsync(command.EmployeeId, cancellationToken)
            ?? throw new NotFoundException("Employee", command.EmployeeId);

        var email = Email.Create(command.Email);

        // EN: Here I verify that the new email is not used by a different employee.
        // ES: Aquí verifico que el nuevo correo no lo esté usando otro empleado.
        if (await _employees.ExistsByEmailAsync(email, employee.Id, cancellationToken))
        {
            throw new ConflictException($"Another employee already uses the email '{email}'.");
        }

        employee.UpdateInformation(
            command.FirstNames, command.LastNames, email, command.Position, command.Area, _clock.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return EmployeeDto.From(employee);
    }
}
