using Microsoft.AspNetCore.Mvc;
using Sgcae.Api.Contracts;
using Sgcae.Application.AccessControl.Commands;
using Sgcae.Application.AccessControl.Queries;
using Sgcae.Application.Abstractions.Messaging;
using Sgcae.Application.Common;
using Sgcae.Domain.AccessLogs;

namespace Sgcae.Api.Controllers;

// EN: This controller exposes the entry/exit registration (RF-03) and the access history (RF-04).
// ES: Este controlador expone el registro de entradas/salidas (RF-03) y el historial de accesos (RF-04).
[ApiController]
[Route("api")]
[Produces("application/json")]
public sealed class AccessController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public AccessController(IDispatcher dispatcher) => _dispatcher = dispatcher;

    /// <summary>
    /// EN: Validates and records an entry or exit. Denied attempts are also recorded (the response says why).
    /// ES: Valida y registra una entrada o salida. Los intentos rechazados también se registran (la respuesta indica el motivo).
    /// </summary>
    [HttpPost("access-requests")]
    [ProducesResponseType(typeof(AccessAttemptResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccessAttemptResultDto>> RegisterAccess(AccessRequest request, CancellationToken cancellationToken)
    {
        var result = await _dispatcher.SendAsync(
            new RegisterAccessCommand(request.CredentialCode, request.AccessPointCode, request.Movement), cancellationToken);

        // EN: I answer 201 because an access log is ALWAYS created; "isGranted" tells if the access was authorized.
        // ES: Respondo 201 porque SIEMPRE se crea un registro de acceso; "isGranted" indica si el acceso fue autorizado.
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>EN: Queries the access history with filters and pagination. / ES: Consulta el historial de accesos con filtros y paginación.</summary>
    /// <param name="employeeId">EN: Employee id. / ES: Id del empleado.</param>
    /// <param name="fromUtc">EN: Start date (UTC). / ES: Fecha inicial (UTC).</param>
    /// <param name="toUtc">EN: End date (UTC). / ES: Fecha final (UTC).</param>
    /// <param name="accessPointId">EN: Access point id. / ES: Id del punto de acceso.</param>
    /// <param name="result">EN: Granted or Denied. / ES: Granted o Denied.</param>
    /// <param name="movement">EN: Entry or Exit. / ES: Entry o Exit.</param>
    /// <param name="page">EN: Page number (starts at 1). / ES: Número de página (inicia en 1).</param>
    /// <param name="pageSize">EN: Page size (max 100). / ES: Tamaño de página (máximo 100).</param>
    /// <param name="cancellationToken">EN: Cancellation token. / ES: Token de cancelación.</param>
    [HttpGet("access-logs")]
    [ProducesResponseType(typeof(PagedResult<AccessLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<AccessLogDto>>> GetHistory(
        [FromQuery] Guid? employeeId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] Guid? accessPointId,
        [FromQuery] AccessResult? result,
        [FromQuery] MovementType? movement,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = Paging.DefaultPageSize)
    {
        var query = new GetAccessHistoryQuery(employeeId, fromUtc, toUtc, accessPointId, result, movement, page, pageSize);
        return Ok(await _dispatcher.QueryAsync(query, cancellationToken));
    }
}
