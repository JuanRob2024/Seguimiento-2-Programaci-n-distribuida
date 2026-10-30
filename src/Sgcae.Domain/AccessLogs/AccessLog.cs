using Sgcae.Domain.Common;

namespace Sgcae.Domain.AccessLogs;

// EN: This aggregate root records every access attempt (granted or denied). It is immutable once created,
//     because an audit trail must never be modified.
// ES: Esta raíz de agregado registra cada intento de acceso (autorizado o rechazado). Es inmutable una vez creada,
//     porque un registro de auditoría nunca debe modificarse.
public sealed class AccessLog : AggregateRoot
{
    public const int RequestedCodeMaxLength = 50;

    // EN: I keep the codes exactly as they were requested, even if they do not exist, for traceability.
    // ES: Guardo los códigos tal como fueron solicitados, aunque no existan, para tener trazabilidad.
    public string RequestedCredentialCode { get; private set; } = string.Empty;
    public string RequestedAccessPointCode { get; private set; } = string.Empty;

    public Guid? CredentialId { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public Guid? AccessPointId { get; private set; }
    public MovementType Movement { get; private set; }
    public AccessResult Result { get; private set; }
    public AccessDenialReason? DenialReason { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    // EN: Constructor reserved for Entity Framework.
    // ES: Constructor reservado para Entity Framework.
    private AccessLog()
    {
    }

    // EN: This factory method builds the log from the decision taken by the AccessPolicy.
    // ES: Este método de fábrica construye el registro a partir de la decisión tomada por la AccessPolicy.
    public static AccessLog Record(
        string requestedCredentialCode,
        string requestedAccessPointCode,
        Guid? credentialId,
        Guid? employeeId,
        Guid? accessPointId,
        MovementType movement,
        AccessDecision decision,
        DateTime nowUtc)
    {
        if (!Enum.IsDefined(movement))
        {
            throw new DomainValidationException($"The movement type '{movement}' is not valid.");
        }

        ArgumentNullException.ThrowIfNull(decision);

        return new AccessLog
        {
            RequestedCredentialCode = Truncate(requestedCredentialCode),
            RequestedAccessPointCode = Truncate(requestedAccessPointCode),
            CredentialId = credentialId,
            EmployeeId = employeeId,
            AccessPointId = accessPointId,
            Movement = movement,
            Result = decision.Result,
            DenialReason = decision.DenialReason,
            OccurredAtUtc = nowUtc
        };
    }

    // EN: Here I make sure the raw text sent by a device never exceeds the column size.
    // ES: Aquí me aseguro de que el texto enviado por un dispositivo nunca supere el tamaño de la columna.
    private static string Truncate(string? value)
    {
        var text = (value ?? string.Empty).Trim().ToUpperInvariant();
        return text.Length <= RequestedCodeMaxLength ? text : text[..RequestedCodeMaxLength];
    }
}
