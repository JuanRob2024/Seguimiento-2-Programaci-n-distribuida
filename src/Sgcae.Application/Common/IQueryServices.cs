using Sgcae.Domain.AccessLogs;
using Sgcae.Domain.AccessPoints;
using Sgcae.Domain.Employees;

namespace Sgcae.Application.Common;

// EN: These are the READ-SIDE contracts of CQRS. Queries do not use the domain repositories: they use these
//     read-only services, optimized for reading (no tracking, filters and pagination in the database).
//     The Infrastructure layer implements them.
// ES: Estos son los contratos del LADO DE LECTURA de CQRS. Las consultas no usan los repositorios del dominio: usan
//     estos servicios de solo lectura, optimizados para leer (sin seguimiento de cambios, filtros y paginación en la
//     base de datos). La capa de Infraestructura los implementa.

public interface IEmployeeQueryService
{
    Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<EmployeeDto>> SearchAsync(
        EmployeeStatus? status, string? area, string? search, int page, int pageSize, CancellationToken cancellationToken);
}

public interface ICredentialQueryService
{
    Task<CredentialDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CredentialDto?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<IReadOnlyList<CredentialDto>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken);
}

public interface IAccessPointQueryService
{
    Task<AccessPointDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<AccessPointDto>> ListAsync(bool? isEnabled, AccessPointType? type, CancellationToken cancellationToken);
}

// EN: These are the filters supported by the access history (RF-04).
// ES: Estos son los filtros que soporta el historial de accesos (RF-04).
public sealed record AccessHistoryFilter(
    Guid? EmployeeId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    Guid? AccessPointId,
    AccessResult? Result,
    MovementType? Movement);

public interface IAccessLogQueryService
{
    Task<PagedResult<AccessLogDto>> SearchAsync(
        AccessHistoryFilter filter, int page, int pageSize, CancellationToken cancellationToken);
}
