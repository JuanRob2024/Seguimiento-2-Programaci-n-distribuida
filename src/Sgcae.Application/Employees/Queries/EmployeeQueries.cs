using Sgcae.Application.Abstractions.Messaging;
using Sgcae.Application.Common;
using Sgcae.Domain.Employees;

namespace Sgcae.Application.Employees.Queries;

// EN: QUERY (RF-01): get one employee by its id.
// ES: CONSULTA (RF-01): obtener un empleado por su id.
public sealed record GetEmployeeByIdQuery(Guid EmployeeId) : IQuery<EmployeeDto>;

public sealed class GetEmployeeByIdHandler : IQueryHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly IEmployeeQueryService _queries;

    public GetEmployeeByIdHandler(IEmployeeQueryService queries) => _queries = queries;

    public async Task<EmployeeDto> HandleAsync(GetEmployeeByIdQuery query, CancellationToken cancellationToken) =>
        await _queries.GetByIdAsync(query.EmployeeId, cancellationToken)
        ?? throw new NotFoundException("Employee", query.EmployeeId);
}

// EN: QUERY (RF-01): list employees with optional filters and pagination.
// ES: CONSULTA (RF-01): listar empleados con filtros opcionales y paginación.
public sealed record GetEmployeesQuery(
    EmployeeStatus? Status,
    string? Area,
    string? Search,
    int Page = 1,
    int PageSize = Paging.DefaultPageSize) : IQuery<PagedResult<EmployeeDto>>;

public sealed class GetEmployeesHandler : IQueryHandler<GetEmployeesQuery, PagedResult<EmployeeDto>>
{
    private readonly IEmployeeQueryService _queries;

    public GetEmployeesHandler(IEmployeeQueryService queries) => _queries = queries;

    public Task<PagedResult<EmployeeDto>> HandleAsync(GetEmployeesQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize) = Paging.Normalize(query.Page, query.PageSize);
        return _queries.SearchAsync(query.Status, query.Area, query.Search, page, pageSize, cancellationToken);
    }
}
