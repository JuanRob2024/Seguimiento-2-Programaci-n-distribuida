using Sgcae.Application.Abstractions;
using Sgcae.Application.Abstractions.Messaging;
using Sgcae.Application.Common;
using Sgcae.Domain.Employees;
using Sgcae.Domain.Repositories;

namespace Sgcae.Application.Employees.Commands;

// EN: COMMAND (RF-01): register a new employee.
// ES: COMANDO (RF-01): registrar un nuevo empleado.
public sealed record RegisterEmployeeCommand(
    string DocumentNumber,
    string FirstNames,
    string LastNames,
    string Email,
    string Position,
    string Area) : ICommand<EmployeeDto>;

// EN: This handler executes the use case: it validates uniqueness, creates the aggregate and saves it.
// ES: Este manejador ejecuta el caso de uso: valida unicidad, crea el agregado y lo guarda.
public sealed class RegisterEmployeeHandler : ICommandHandler<RegisterEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _employees;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public RegisterEmployeeHandler(IEmployeeRepository employees, IUnitOfWork unitOfWork, IDateTimeProvider clock)
    {
        _employees = employees;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<EmployeeDto> HandleAsync(RegisterEmployeeCommand command, CancellationToken cancellationToken)
    {
        // EN: First I build the value objects; they throw an exception if the format is invalid.
        // ES: Primero construyo los objetos de valor; lanzan una excepción si el formato es inválido.
        var documentNumber = DocumentNumber.Create(command.DocumentNumber);
        var email = Email.Create(command.Email);

        // EN: Here I check the uniqueness rules, which need to look at the other employees.
        // ES: Aquí verifico las reglas de unicidad, que necesitan consultar a los demás empleados.
        if (await _employees.ExistsByDocumentNumberAsync(documentNumber, cancellationToken))
        {
            throw new ConflictException($"An employee with document number '{documentNumber}' already exists.");
        }

        if (await _employees.ExistsByEmailAsync(email, null, cancellationToken))
        {
            throw new ConflictException($"An employee with email '{email}' already exists.");
        }

        var employee = Employee.Register(
            documentNumber, command.FirstNames, command.LastNames, email,
            command.Position, command.Area, _clock.UtcNow);

        _employees.Add(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EmployeeDto.From(employee);
    }
}
