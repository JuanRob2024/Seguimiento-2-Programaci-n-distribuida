using Sgcae.Domain.AccessLogs;
using Sgcae.Domain.AccessPoints;

namespace Sgcae.Api.Contracts;

// EN: These records are the request bodies received by the API. I keep them separate from the commands so the
//     public HTTP contract can evolve without affecting the use cases.
// ES: Estos records son los cuerpos de las peticiones que recibe la API. Los mantengo separados de los comandos para
//     que el contrato HTTP público pueda evolucionar sin afectar los casos de uso.

/// <summary>EN: Data to register an employee. / ES: Datos para registrar un empleado.</summary>
public sealed record RegisterEmployeeRequest(
    string DocumentNumber,
    string FirstNames,
    string LastNames,
    string Email,
    string Position,
    string Area);

/// <summary>EN: Data to update an employee (the document can not change). / ES: Datos para actualizar un empleado (el documento no cambia).</summary>
public sealed record UpdateEmployeeRequest(
    string FirstNames,
    string LastNames,
    string Email,
    string Position,
    string Area);

/// <summary>EN: Data to assign a credential. If Code is empty, it is generated. / ES: Datos para asignar una credencial. Si Code va vacío, se genera.</summary>
public sealed record AssignCredentialRequest(Guid EmployeeId, string? Code, DateTime? ExpiresAtUtc);

/// <summary>EN: Optional reason for blocking or revoking. / ES: Motivo opcional para bloquear o revocar.</summary>
public sealed record CredentialStatusChangeRequest(string? Reason);

/// <summary>EN: Data to register an access point. / ES: Datos para registrar un punto de acceso.</summary>
public sealed record RegisterAccessPointRequest(string Code, string Name, AccessPointType Type, string? Description);

/// <summary>EN: Access request sent by a device. / ES: Solicitud de acceso enviada por un dispositivo.</summary>
public sealed record AccessRequest(string CredentialCode, string AccessPointCode, MovementType Movement);
