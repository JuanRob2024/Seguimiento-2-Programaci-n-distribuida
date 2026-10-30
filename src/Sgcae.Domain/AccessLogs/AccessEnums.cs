namespace Sgcae.Domain.AccessLogs;

// EN: This enum indicates the type of movement requested at an access point.
// ES: Este enum indica el tipo de movimiento solicitado en un punto de acceso.
public enum MovementType
{
    Entry = 1,
    Exit = 2
}

// EN: This enum represents the final result of the validation of an access request.
// ES: Este enum representa el resultado final de la validación de una solicitud de acceso.
public enum AccessResult
{
    Granted = 1,
    Denied = 2
}

// EN: This enum explains why an access request was denied. It gives traceability to rejected attempts.
// ES: Este enum explica por qué se rechazó una solicitud de acceso. Da trazabilidad a los intentos rechazados.
public enum AccessDenialReason
{
    AccessPointNotFound = 1,
    AccessPointDisabled = 2,
    CredentialNotFound = 3,
    CredentialBlocked = 4,
    CredentialRevoked = 5,
    CredentialExpired = 6,
    EmployeeNotFound = 7,
    EmployeeInactive = 8,
    AlreadyInside = 9,
    NotInside = 10
}
