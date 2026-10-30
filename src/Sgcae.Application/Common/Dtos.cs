using Sgcae.Domain.AccessLogs;
using Sgcae.Domain.AccessPoints;
using Sgcae.Domain.Credentials;
using Sgcae.Domain.Employees;

namespace Sgcae.Application.Common;

// EN: These DTOs (Data Transfer Objects) are what the API returns. I never expose the domain entities directly.
// ES: Estos DTOs (Objetos de Transferencia de Datos) son lo que retorna la API. Nunca expongo directamente las entidades del dominio.

public sealed record EmployeeDto(
    Guid Id,
    string DocumentNumber,
    string FirstNames,
    string LastNames,
    string Email,
    string Position,
    string Area,
    EmployeeStatus Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? DeactivatedAtUtc)
{
    // EN: Here I map the entity to the DTO (a single place for this mapping).
    // ES: Aquí mapeo la entidad al DTO (un único lugar para este mapeo).
    public static EmployeeDto From(Employee e) => new(
        e.Id, e.DocumentNumber.Value, e.FirstNames, e.LastNames, e.Email.Value,
        e.Position, e.Area, e.Status, e.CreatedAtUtc, e.UpdatedAtUtc, e.DeactivatedAtUtc);
}

public sealed record CredentialDto(
    Guid Id,
    Guid EmployeeId,
    string Code,
    CredentialStatus Status,
    bool CanBeUsed,
    DateTime IssuedAtUtc,
    DateTime? ExpiresAtUtc,
    DateTime? StatusChangedAtUtc,
    string? StatusReason)
{
    // EN: "CanBeUsed" tells the client, in one field, whether the credential is valid right now.
    // ES: "CanBeUsed" le indica al cliente, en un solo campo, si la credencial es válida en este momento.
    public static CredentialDto From(Credential c, DateTime nowUtc) => new(
        c.Id, c.EmployeeId, c.Code.Value, c.Status, c.CanBeUsed(nowUtc),
        c.IssuedAtUtc, c.ExpiresAtUtc, c.StatusChangedAtUtc, c.StatusReason);
}

public sealed record AccessPointDto(
    Guid Id,
    string Code,
    string Name,
    AccessPointType Type,
    string? Description,
    bool IsEnabled,
    DateTime CreatedAtUtc)
{
    public static AccessPointDto From(AccessPoint a) => new(
        a.Id, a.Code.Value, a.Name, a.Type, a.Description, a.IsEnabled, a.CreatedAtUtc);
}

public sealed record AccessLogDto(
    Guid Id,
    DateTime OccurredAtUtc,
    MovementType Movement,
    AccessResult Result,
    AccessDenialReason? DenialReason,
    string RequestedCredentialCode,
    string RequestedAccessPointCode,
    Guid? CredentialId,
    Guid? EmployeeId,
    string? EmployeeDocumentNumber,
    string? EmployeeFullName,
    Guid? AccessPointId,
    string? AccessPointName);

// EN: This DTO is the answer returned to the access device after evaluating a request.
// ES: Este DTO es la respuesta que se devuelve al dispositivo de acceso después de evaluar una solicitud.
public sealed record AccessAttemptResultDto(
    Guid AccessLogId,
    bool IsGranted,
    AccessResult Result,
    AccessDenialReason? DenialReason,
    MovementType Movement,
    string CredentialCode,
    string AccessPointCode,
    Guid? EmployeeId,
    string? EmployeeFullName,
    DateTime OccurredAtUtc);
