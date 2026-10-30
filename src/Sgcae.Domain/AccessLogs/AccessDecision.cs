namespace Sgcae.Domain.AccessLogs;

// EN: This value object holds the decision taken by the access policy: granted, or denied with a reason.
// ES: Este objeto de valor contiene la decisión tomada por la política de acceso: autorizado, o rechazado con un motivo.
public sealed record AccessDecision
{
    public AccessResult Result { get; }
    public AccessDenialReason? DenialReason { get; }

    public bool IsGranted => Result == AccessResult.Granted;

    private AccessDecision(AccessResult result, AccessDenialReason? denialReason)
    {
        Result = result;
        DenialReason = denialReason;
    }

    public static AccessDecision Granted() => new(AccessResult.Granted, null);

    public static AccessDecision Denied(AccessDenialReason reason) => new(AccessResult.Denied, reason);
}
