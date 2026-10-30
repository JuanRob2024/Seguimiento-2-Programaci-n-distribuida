using Sgcae.Domain.Common;

namespace Sgcae.Domain.Credentials;

// EN: This is the Credential aggregate root. It controls the valid status transitions of a credential
//     and answers whether it can be used at a given moment.
// ES: Esta es la raíz de agregado Credencial. Controla las transiciones de estado válidas de una credencial
//     y responde si puede usarse en un momento dado.
public sealed class Credential : AggregateRoot
{
    public const int ReasonMaxLength = 250;

    public Guid EmployeeId { get; private set; }
    public CredentialCode Code { get; private set; } = null!;
    public CredentialStatus Status { get; private set; }
    public DateTime IssuedAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public DateTime? StatusChangedAtUtc { get; private set; }
    public string? StatusReason { get; private set; }

    // EN: Constructor reserved for Entity Framework.
    // ES: Constructor reservado para Entity Framework.
    private Credential()
    {
    }

    // EN: This factory method issues a new active credential for an employee. The expiration date is optional,
    //     but if it is sent it must be in the future.
    // ES: Este método de fábrica emite una nueva credencial activa para un empleado. La fecha de expiración es opcional,
    //     pero si se envía debe estar en el futuro.
    public static Credential Issue(Guid employeeId, CredentialCode code, DateTime nowUtc, DateTime? expiresAtUtc = null)
    {
        if (employeeId == Guid.Empty)
        {
            throw new DomainValidationException("The employee id is required.");
        }

        if (expiresAtUtc.HasValue && expiresAtUtc.Value <= nowUtc)
        {
            throw new DomainValidationException("The expiration date must be in the future.");
        }

        return new Credential
        {
            EmployeeId = employeeId,
            Code = code ?? throw new DomainValidationException("The credential code is required."),
            Status = CredentialStatus.Active,
            IssuedAtUtc = nowUtc,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    // EN: Here I check if the credential is expired at the given moment.
    // ES: Aquí verifico si la credencial está vencida en el momento indicado.
    public bool IsExpired(DateTime nowUtc) => ExpiresAtUtc.HasValue && ExpiresAtUtc.Value <= nowUtc;

    // EN: A credential can be used only when it is Active and not expired.
    // ES: Una credencial solo puede usarse cuando está Activa y no está vencida.
    public bool CanBeUsed(DateTime nowUtc) => Status == CredentialStatus.Active && !IsExpired(nowUtc);

    // EN: Here I block the credential. Only an active credential can be blocked.
    // ES: Aquí bloqueo la credencial. Solo una credencial activa puede bloquearse.
    public void Block(string? reason, DateTime nowUtc)
    {
        if (Status != CredentialStatus.Active)
        {
            throw new BusinessRuleException($"Only an active credential can be blocked. Current status: {Status}.");
        }

        ChangeStatus(CredentialStatus.Blocked, reason, nowUtc);
    }

    // EN: Here I unblock the credential. Only a blocked credential can return to Active.
    // ES: Aquí desbloqueo la credencial. Solo una credencial bloqueada puede volver a Activa.
    public void Unblock(DateTime nowUtc)
    {
        if (Status != CredentialStatus.Blocked)
        {
            throw new BusinessRuleException($"Only a blocked credential can be unblocked. Current status: {Status}.");
        }

        ChangeStatus(CredentialStatus.Active, null, nowUtc);
    }

    // EN: Here I revoke the credential. Revoked is a final state: it can never be used again.
    // ES: Aquí revoco la credencial. Revocada es un estado final: nunca podrá volver a usarse.
    public void Revoke(string? reason, DateTime nowUtc)
    {
        if (Status == CredentialStatus.Revoked)
        {
            throw new BusinessRuleException("The credential is already revoked.");
        }

        ChangeStatus(CredentialStatus.Revoked, reason, nowUtc);
    }

    private void ChangeStatus(CredentialStatus newStatus, string? reason, DateTime nowUtc)
    {
        Status = newStatus;
        StatusReason = Guard.OptionalText(reason, nameof(StatusReason), ReasonMaxLength);
        StatusChangedAtUtc = nowUtc;
    }
}
