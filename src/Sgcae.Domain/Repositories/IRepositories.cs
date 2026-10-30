using Sgcae.Domain.AccessLogs;
using Sgcae.Domain.AccessPoints;
using Sgcae.Domain.Credentials;
using Sgcae.Domain.Employees;

namespace Sgcae.Domain.Repositories;

// EN: These are the repository CONTRACTS of the domain (write side). The domain only knows the interfaces;
//     the Infrastructure layer implements them with Entity Framework Core (dependency inversion).
// ES: Estos son los CONTRATOS de repositorio del dominio (lado de escritura). El dominio solo conoce las interfaces;
//     la capa de Infraestructura las implementa con Entity Framework Core (inversión de dependencias).

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByDocumentNumberAsync(DocumentNumber documentNumber, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(Email email, Guid? excludeEmployeeId = null, CancellationToken cancellationToken = default);
    void Add(Employee employee);
}

public interface ICredentialRepository
{
    Task<Credential?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Credential?> GetByCodeAsync(CredentialCode code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(CredentialCode code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Credential>> GetNonRevokedByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    void Add(Credential credential);
}

public interface IAccessPointRepository
{
    Task<AccessPoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AccessPoint?> GetByCodeAsync(AccessPointCode code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(AccessPointCode code, CancellationToken cancellationToken = default);
    void Add(AccessPoint accessPoint);
}

public interface IAccessLogRepository
{
    Task<MovementType?> GetLastGrantedMovementAsync(Guid employeeId, Guid accessPointId, CancellationToken cancellationToken = default);
    void Add(AccessLog accessLog);
}

// EN: The Unit of Work saves every change of a use case in a single transaction.
// ES: La Unidad de Trabajo guarda todos los cambios de un caso de uso en una sola transacción.
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
