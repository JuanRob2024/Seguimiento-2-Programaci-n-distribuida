using Microsoft.AspNetCore.Mvc;
using Sgcae.Api.Contracts;
using Sgcae.Application.Abstractions.Messaging;
using Sgcae.Application.Common;
using Sgcae.Application.Credentials.Queries;
using Sgcae.Application.Employees.Commands;
using Sgcae.Application.Employees.Queries;
using Sgcae.Domain.Employees;

namespace Sgcae.Api.Controllers;

// EN: This controller exposes the employee management endpoints (RF-01). It contains no business logic:
//     it only turns HTTP requests into commands/queries and sends them to the dispatcher.
// ES: Este controlador expone los endpoints de gestión de empleados (RF-01). No contiene lógica de negocio:
//     solo convierte las peticiones HTTP en comandos/consultas y los envía al despachador.
[ApiController]
[Route("api/employees")]
[Produces("application/json")]
public sealed class EmployeesController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public EmployeesController(IDispatcher dispatcher) => _dispatcher = dispatcher;

    /// <summary>EN: Registers a new employee. / ES: Registra un nuevo empleado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeDto>> Register(RegisterEmployeeRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterEmployeeCommand(
            request.DocumentNumber, request.FirstNames, request.LastNames,
            request.Email, request.Position, request.Area);

        var employee = await _dispatcher.SendAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    /// <summary>EN: Lists employees with filters and pagination. / ES: Lista empleados con filtros y paginación.</summary>
    /// <param name="status">EN: Active or Inactive. / ES: Active o Inactive.</param>
    /// <param name="area">EN: Exact organizational area. / ES: Área organizacional exacta.</param>
    /// <param name="search">EN: Text in names, last names or exact document. / ES: Texto en nombres, apellidos o documento exacto.</param>
    /// <param name="page">EN: Page number (starts at 1). / ES: Número de página (inicia en 1).</param>
    /// <param name="pageSize">EN: Page size (max 100). / ES: Tamaño de página (máximo 100).</param>
    /// <param name="cancellationToken">EN: Cancellation token. / ES: Token de cancelación.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<EmployeeDto>>> Search(
        [FromQuery] EmployeeStatus? status,
        [FromQuery] string? area,
        [FromQuery] string? search,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = Paging.DefaultPageSize)
    {
        var result = await _dispatcher.QueryAsync(
            new GetEmployeesQuery(status, area, search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>EN: Gets an employee by id. / ES: Obtiene un empleado por id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _dispatcher.QueryAsync(new GetEmployeeByIdQuery(id), cancellationToken));

    /// <summary>EN: Updates the employee's information. / ES: Actualiza la información del empleado.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeDto>> Update(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateEmployeeCommand(
            id, request.FirstNames, request.LastNames, request.Email, request.Position, request.Area);
        return Ok(await _dispatcher.SendAsync(command, cancellationToken));
    }

    /// <summary>EN: Deactivates an employee and revokes their credentials. / ES: Desactiva un empleado y revoca sus credenciales.</summary>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeDto>> Deactivate(Guid id, CancellationToken cancellationToken) =>
        Ok(await _dispatcher.SendAsync(new DeactivateEmployeeCommand(id), cancellationToken));

    /// <summary>EN: Lists the credentials of an employee. / ES: Lista las credenciales de un empleado.</summary>
    [HttpGet("{id:guid}/credentials")]
    [ProducesResponseType(typeof(IReadOnlyList<CredentialDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CredentialDto>>> GetCredentials(Guid id, CancellationToken cancellationToken) =>
        Ok(await _dispatcher.QueryAsync(new GetEmployeeCredentialsQuery(id), cancellationToken));
}
