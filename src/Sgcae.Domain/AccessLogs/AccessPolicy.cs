using Sgcae.Domain.AccessPoints;
using Sgcae.Domain.Credentials;
using Sgcae.Domain.Employees;

namespace Sgcae.Domain.AccessLogs;

// EN: This is a DOMAIN SERVICE. The access rule involves several aggregates (access point, credential and
//     employee), so it does not belong to any single one of them. It is a pure function: it does not access
//     the database, which makes it very easy to unit test.
// ES: Este es un SERVICIO DE DOMINIO. La regla de acceso involucra varios agregados (punto de acceso, credencial
//     y empleado), por eso no pertenece a ninguno de ellos en particular. Es una función pura: no accede a la
//     base de datos, lo que la hace muy fácil de probar con pruebas unitarias.
public static class AccessPolicy
{
    // EN: Here I evaluate the rules in order. The first rule that fails determines the denial reason.
    //     "lastGrantedMovementAtPoint" is the last authorized movement of the employee at the same access point,
    //     and I use it to apply the anti-passback rule (no double entry and no exit without entry).
    // ES: Aquí evalúo las reglas en orden. La primera regla que falle determina el motivo del rechazo.
    //     "lastGrantedMovementAtPoint" es el último movimiento autorizado del empleado en el mismo punto de acceso,
    //     y lo uso para aplicar la regla anti-passback (no doble entrada y no salida sin entrada previa).
    public static AccessDecision Evaluate(
        AccessPoint? accessPoint,
        Credential? credential,
        Employee? employee,
        MovementType movement,
        MovementType? lastGrantedMovementAtPoint,
        DateTime nowUtc)
    {
        // EN: Rule 1: the access point must exist and be enabled.
        // ES: Regla 1: el punto de acceso debe existir y estar habilitado.
        if (accessPoint is null)
        {
            return AccessDecision.Denied(AccessDenialReason.AccessPointNotFound);
        }

        if (!accessPoint.IsEnabled)
        {
            return AccessDecision.Denied(AccessDenialReason.AccessPointDisabled);
        }

        // EN: Rule 2: the credential must exist, be active and not be expired.
        // ES: Regla 2: la credencial debe existir, estar activa y no estar vencida.
        if (credential is null)
        {
            return AccessDecision.Denied(AccessDenialReason.CredentialNotFound);
        }

        switch (credential.Status)
        {
            case CredentialStatus.Revoked:
                return AccessDecision.Denied(AccessDenialReason.CredentialRevoked);
            case CredentialStatus.Blocked:
                return AccessDecision.Denied(AccessDenialReason.CredentialBlocked);
        }

        if (credential.IsExpired(nowUtc))
        {
            return AccessDecision.Denied(AccessDenialReason.CredentialExpired);
        }

        // EN: Rule 3: the credential owner must exist and have an active employment status.
        // ES: Regla 3: el dueño de la credencial debe existir y tener un estado laboral activo.
        if (employee is null)
        {
            return AccessDecision.Denied(AccessDenialReason.EmployeeNotFound);
        }

        if (!employee.IsActive)
        {
            return AccessDecision.Denied(AccessDenialReason.EmployeeInactive);
        }

        // EN: Rule 4 (anti-passback): the movement must be consistent with the previous one at the same point.
        // ES: Regla 4 (anti-passback): el movimiento debe ser coherente con el anterior en el mismo punto.
        if (movement == MovementType.Entry && lastGrantedMovementAtPoint == MovementType.Entry)
        {
            return AccessDecision.Denied(AccessDenialReason.AlreadyInside);
        }

        if (movement == MovementType.Exit && lastGrantedMovementAtPoint != MovementType.Entry)
        {
            return AccessDecision.Denied(AccessDenialReason.NotInside);
        }

        return AccessDecision.Granted();
    }
}
