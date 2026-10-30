using Microsoft.EntityFrameworkCore;
using Sgcae.Domain.AccessLogs;
using Sgcae.Domain.AccessPoints;
using Sgcae.Domain.Credentials;
using Sgcae.Domain.Employees;
using Sgcae.Domain.Repositories;

namespace Sgcae.Infrastructure.Persistence.Repositories;

// EN: These classes implement the domain repositories (WRITE side of CQRS) using EF Core.
//     They return tracked entities so the changes made by the aggregates are detected and saved.
// ES: Estas clases implementan los repositorios del dominio (lado de ESCRITURA de CQRS) usando EF Core.
//     Retornan entidades con seguimiento para que los cambios hechos por los agregados se detecten y se guarden.

internal sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly SgcaeDbContext _db;

    public EmployeeRepository(SgcaeDbContext db) => _db = db;

    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<bool> ExistsByDocumentNumberAsync(DocumentNumber documentNumber, CancellationToken cancellationToken = default) =>
        _db.Employees.AnyAsync(e => e.DocumentNumber == documentNumber, cancellationToken);

    // EN: Here I check if the email exists, optionally ignoring one employee (useful when updating).
    // ES: Aquí verifico si el correo existe, opcionalmente ignorando a un empleado (útil al actualizar).
    public Task<bool> ExistsByEmailAsync(Email email, Guid? excludeEmployeeId = null, CancellationToken cancellationToken = default) =>
        excludeEmployeeId.HasValue
            ? _db.Employees.AnyAsync(e => e.Email == email && e.Id != excludeEmployeeId.Value, cancellationToken)
            : _db.Employees.AnyAsync(e => e.Email == email, cancellationToken);

    public void Add(Employee employee) => _db.Employees.Add(employee);
}

internal sealed class CredentialRepository : ICredentialRepository
{
    private readonly SgcaeDbContext _db;

    public CredentialRepository(SgcaeDbContext db) => _db = db;

    public Task<Credential?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Credentials.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Credential?> GetByCodeAsync(CredentialCode code, CancellationToken cancellationToken = default) =>
        _db.Credentials.FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

    public Task<bool> ExistsByCodeAsync(CredentialCode code, CancellationToken cancellationToken = default) =>
        _db.Credentials.AnyAsync(c => c.Code == code, cancellationToken);

    public async Task<IReadOnlyList<Credential>> GetNonRevokedByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default) =>
        await _db.Credentials
            .Where(c => c.EmployeeId == employeeId && c.Status != CredentialStatus.Revoked)
            .ToListAsync(cancellationToken);

    public void Add(Credential credential) => _db.Credentials.Add(credential);
}

internal sealed class AccessPointRepository : IAccessPointRepository
{
    private readonly SgcaeDbContext _db;

    public AccessPointRepository(SgcaeDbContext db) => _db = db;

    public Task<AccessPoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.AccessPoints.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<AccessPoint?> GetByCodeAsync(AccessPointCode code, CancellationToken cancellationToken = default) =>
        _db.AccessPoints.FirstOrDefaultAsync(a => a.Code == code, cancellationToken);

    public Task<bool> ExistsByCodeAsync(AccessPointCode code, CancellationToken cancellationToken = default) =>
        _db.AccessPoints.AnyAsync(a => a.Code == code, cancellationToken);

    public void Add(AccessPoint accessPoint) => _db.AccessPoints.Add(accessPoint);
}

internal sealed class AccessLogRepository : IAccessLogRepository
{
    private readonly SgcaeDbContext _db;

    public AccessLogRepository(SgcaeDbContext db) => _db = db;

    // EN: Here I look for the most recent GRANTED movement of the employee at the access point (anti-passback).
    // ES: Aquí busco el movimiento AUTORIZADO más reciente del empleado en el punto de acceso (anti-passback).
    public Task<MovementType?> GetLastGrantedMovementAsync(Guid employeeId, Guid accessPointId, CancellationToken cancellationToken = default) =>
        _db.AccessLogs
            .AsNoTracking()
            .Where(l => l.EmployeeId == employeeId &&
                        l.AccessPointId == accessPointId &&
                        l.Result == AccessResult.Granted)
            .OrderByDescending(l => l.OccurredAtUtc)
            .Select(l => (MovementType?)l.Movement)
            .FirstOrDefaultAsync(cancellationToken);

    public void Add(AccessLog accessLog) => _db.AccessLogs.Add(accessLog);
}
