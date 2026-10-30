using Sgcae.Application.Abstractions;
using Sgcae.Application.Abstractions.Messaging;
using Sgcae.Application.Common;
using Sgcae.Domain.AccessLogs;
using Sgcae.Domain.AccessPoints;
using Sgcae.Domain.Credentials;
using Sgcae.Domain.Employees;
using Sgcae.Domain.Repositories;

namespace Sgcae.Application.AccessControl.Commands;

// EN: COMMAND (RF-03): an access device sends the credential code, the access point and the movement type.
// ES: COMANDO (RF-03): un dispositivo de acceso envía el código de la credencial, el punto de acceso y el tipo de movimiento.
public sealed record RegisterAccessCommand(
    string CredentialCode,
    string AccessPointCode,
    MovementType Movement) : ICommand<AccessAttemptResultDto>;

// EN: This handler is the heart of the system. It loads the involved aggregates, asks the domain service
//     (AccessPolicy) for a decision and ALWAYS records the attempt, also when it is denied.
// ES: Este manejador es el corazón del sistema. Carga los agregados involucrados, le pide una decisión al
//     servicio de dominio (AccessPolicy) y SIEMPRE registra el intento, también cuando es rechazado.
public sealed class RegisterAccessHandler : ICommandHandler<RegisterAccessCommand, AccessAttemptResultDto>
{
    private readonly ICredentialRepository _credentials;
    private readonly IEmployeeRepository _employees;
    private readonly IAccessPointRepository _accessPoints;
    private readonly IAccessLogRepository _accessLogs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public RegisterAccessHandler(
        ICredentialRepository credentials,
        IEmployeeRepository employees,
        IAccessPointRepository accessPoints,
        IAccessLogRepository accessLogs,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _credentials = credentials;
        _employees = employees;
        _accessPoints = accessPoints;
        _accessLogs = accessLogs;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<AccessAttemptResultDto> HandleAsync(RegisterAccessCommand command, CancellationToken cancellationToken)
    {
        // EN: The request must have the minimum data; otherwise it is a malformed request (HTTP 400).
        // ES: La solicitud debe traer los datos mínimos; si no, es una solicitud mal formada (HTTP 400).
        if (string.IsNullOrWhiteSpace(command.CredentialCode) || string.IsNullOrWhiteSpace(command.AccessPointCode))
        {
            throw new RequestValidationException("The credential code and the access point code are required.");
        }

        if (!Enum.IsDefined(command.Movement))
        {
            throw new RequestValidationException("The movement type must be 'Entry' or 'Exit'.");
        }

        var now = _clock.UtcNow;

        // EN: Here I load the access point and the credential. If a code has an invalid format, I treat it as "not found".
        // ES: Aquí cargo el punto de acceso y la credencial. Si un código tiene formato inválido, lo trato como "no encontrado".
        AccessPoint? accessPoint = null;
        if (AccessPointCode.TryCreate(command.AccessPointCode, out var accessPointCode))
        {
            accessPoint = await _accessPoints.GetByCodeAsync(accessPointCode!, cancellationToken);
        }

        Credential? credential = null;
        if (Domain.Credentials.CredentialCode.TryCreate(command.CredentialCode, out var credentialCode))
        {
            credential = await _credentials.GetByCodeAsync(credentialCode!, cancellationToken);
        }

        Employee? employee = credential is null
            ? null
            : await _employees.GetByIdAsync(credential.EmployeeId, cancellationToken);

        // EN: Here I get the last authorized movement of the employee at this access point (for anti-passback).
        // ES: Aquí obtengo el último movimiento autorizado del empleado en este punto de acceso (para el anti-passback).
        MovementType? lastMovement = null;
        if (employee is not null && accessPoint is not null)
        {
            lastMovement = await _accessLogs.GetLastGrantedMovementAsync(employee.Id, accessPoint.Id, cancellationToken);
        }

        // EN: The domain decides. The application layer only orchestrates.
        // ES: El dominio decide. La capa de aplicación solo orquesta.
        var decision = AccessPolicy.Evaluate(accessPoint, credential, employee, command.Movement, lastMovement, now);

        var log = AccessLog.Record(
            command.CredentialCode,
            command.AccessPointCode,
            credential?.Id,
            employee?.Id ?? credential?.EmployeeId,
            accessPoint?.Id,
            command.Movement,
            decision,
            now);

        _accessLogs.Add(log);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AccessAttemptResultDto(
            log.Id,
            decision.IsGranted,
            decision.Result,
            decision.DenialReason,
            log.Movement,
            log.RequestedCredentialCode,
            log.RequestedAccessPointCode,
            log.EmployeeId,
            employee is null ? null : $"{employee.FirstNames} {employee.LastNames}",
            log.OccurredAtUtc);
    }
}
