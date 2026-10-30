using Microsoft.EntityFrameworkCore;
using Sgcae.Application.Abstractions;
using Sgcae.Application.Common;
using Sgcae.Domain.AccessPoints;
using Sgcae.Domain.Credentials;
using Sgcae.Domain.Employees;

namespace Sgcae.Infrastructure.Persistence.Queries;

// EN: These classes implement the READ side of CQRS. They always use AsNoTracking() (lighter and faster,
//     because nothing is going to be modified) and they apply filters and pagination inside SQL Server.
// ES: Estas clases implementan el lado de LECTURA de CQRS. Siempre usan AsNoTracking() (más liviano y rápido,
//     porque nada se va a modificar) y aplican los filtros y la paginación dentro de SQL Server.

internal sealed class EmployeeQueryService : IEmployeeQueryService
{
    private readonly SgcaeDbContext _db;

    public EmployeeQueryService(SgcaeDbContext db) => _db = db;

    public async Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        return employee is null ? null : EmployeeDto.From(employee);
    }

    public async Task<PagedResult<EmployeeDto>> SearchAsync(
        EmployeeStatus? status, string? area, string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.Employees.AsNoTracking().AsQueryable();

        // EN: Here I add each filter only when the client sends it.
        // ES: Aquí agrego cada filtro solo cuando el cliente lo envía.
        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(area))
        {
            var areaValue = area.Trim();
            query = query.Where(e => e.Area == areaValue);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var text = search.Trim();
            query = DocumentNumber.TryCreate(text, out var document)
                ? query.Where(e => e.FirstNames.Contains(text) || e.LastNames.Contains(text) || e.DocumentNumber == document)
                : query.Where(e => e.FirstNames.Contains(text) || e.LastNames.Contains(text));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(e => e.LastNames).ThenBy(e => e.FirstNames)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<EmployeeDto>(items.Select(EmployeeDto.From).ToList(), page, pageSize, total);
    }
}

internal sealed class CredentialQueryService : ICredentialQueryService
{
    private readonly SgcaeDbContext _db;
    private readonly IDateTimeProvider _clock;

    public CredentialQueryService(SgcaeDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<CredentialDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var credential = await _db.Credentials.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        return credential is null ? null : CredentialDto.From(credential, _clock.UtcNow);
    }

    public async Task<CredentialDto?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        if (!CredentialCode.TryCreate(code, out var credentialCode))
        {
            return null;
        }

        var credential = await _db.Credentials.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == credentialCode, cancellationToken);
        return credential is null ? null : CredentialDto.From(credential, _clock.UtcNow);
    }

    public async Task<IReadOnlyList<CredentialDto>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var credentials = await _db.Credentials.AsNoTracking()
            .Where(c => c.EmployeeId == employeeId)
            .OrderByDescending(c => c.IssuedAtUtc)
            .ToListAsync(cancellationToken);

        return credentials.Select(c => CredentialDto.From(c, now)).ToList();
    }
}

internal sealed class AccessPointQueryService : IAccessPointQueryService
{
    private readonly SgcaeDbContext _db;

    public AccessPointQueryService(SgcaeDbContext db) => _db = db;

    public async Task<AccessPointDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var accessPoint = await _db.AccessPoints.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        return accessPoint is null ? null : AccessPointDto.From(accessPoint);
    }

    public async Task<IReadOnlyList<AccessPointDto>> ListAsync(
        bool? isEnabled, AccessPointType? type, CancellationToken cancellationToken)
    {
        var query = _db.AccessPoints.AsNoTracking().AsQueryable();

        if (isEnabled.HasValue)
        {
            query = query.Where(a => a.IsEnabled == isEnabled.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(a => a.Type == type.Value);
        }

        var items = await query.OrderBy(a => a.Name).ToListAsync(cancellationToken);
        return items.Select(AccessPointDto.From).ToList();
    }
}

internal sealed class AccessLogQueryService : IAccessLogQueryService
{
    private readonly SgcaeDbContext _db;

    public AccessLogQueryService(SgcaeDbContext db) => _db = db;

    public async Task<PagedResult<AccessLogDto>> SearchAsync(
        AccessHistoryFilter filter, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _db.AccessLogs.AsNoTracking().AsQueryable();

        // EN: Here I apply the RF-04 filters: employee, date range, access point, result and movement type.
        // ES: Aquí aplico los filtros del RF-04: empleado, rango de fechas, punto de acceso, resultado y tipo de movimiento.
        if (filter.EmployeeId.HasValue)
        {
            query = query.Where(l => l.EmployeeId == filter.EmployeeId.Value);
        }

        if (filter.FromUtc.HasValue)
        {
            query = query.Where(l => l.OccurredAtUtc >= filter.FromUtc.Value);
        }

        if (filter.ToUtc.HasValue)
        {
            query = query.Where(l => l.OccurredAtUtc <= filter.ToUtc.Value);
        }

        if (filter.AccessPointId.HasValue)
        {
            query = query.Where(l => l.AccessPointId == filter.AccessPointId.Value);
        }

        if (filter.Result.HasValue)
        {
            query = query.Where(l => l.Result == filter.Result.Value);
        }

        if (filter.Movement.HasValue)
        {
            query = query.Where(l => l.Movement == filter.Movement.Value);
        }

        // EN: First I count the total and then I bring ONLY the requested page (Skip/Take = OFFSET/FETCH in SQL).
        // ES: Primero cuento el total y luego traigo SOLO la página solicitada (Skip/Take = OFFSET/FETCH en SQL).
        var total = await query.CountAsync(cancellationToken);
        var logs = await query
            .OrderByDescending(l => l.OccurredAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // EN: Here I load, in two small queries, the names of the employees and access points of this page only.
        // ES: Aquí cargo, en dos consultas pequeñas, los nombres de los empleados y puntos de acceso solo de esta página.
        var employeeIds = logs.Where(l => l.EmployeeId.HasValue).Select(l => l.EmployeeId!.Value).Distinct().ToList();
        var accessPointIds = logs.Where(l => l.AccessPointId.HasValue).Select(l => l.AccessPointId!.Value).Distinct().ToList();

        var employees = await _db.Employees.AsNoTracking()
            .Where(e => employeeIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        var accessPoints = await _db.AccessPoints.AsNoTracking()
            .Where(a => accessPointIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        var items = logs.Select(l =>
        {
            Employee? employee = l.EmployeeId.HasValue && employees.TryGetValue(l.EmployeeId.Value, out var e) ? e : null;
            AccessPoint? accessPoint = l.AccessPointId.HasValue && accessPoints.TryGetValue(l.AccessPointId.Value, out var a) ? a : null;

            return new AccessLogDto(
                l.Id,
                l.OccurredAtUtc,
                l.Movement,
                l.Result,
                l.DenialReason,
                l.RequestedCredentialCode,
                l.RequestedAccessPointCode,
                l.CredentialId,
                l.EmployeeId,
                employee?.DocumentNumber.Value,
                employee is null ? null : $"{employee.FirstNames} {employee.LastNames}",
                l.AccessPointId,
                accessPoint?.Name);
        }).ToList();

        return new PagedResult<AccessLogDto>(items, page, pageSize, total);
    }
}
