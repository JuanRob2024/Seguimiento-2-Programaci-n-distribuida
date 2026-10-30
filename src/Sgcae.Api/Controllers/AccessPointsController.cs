using Microsoft.AspNetCore.Mvc;
using Sgcae.Api.Contracts;
using Sgcae.Application.AccessPoints.Commands;
using Sgcae.Application.AccessPoints.Queries;
using Sgcae.Application.Abstractions.Messaging;
using Sgcae.Application.Common;
using Sgcae.Domain.AccessPoints;

namespace Sgcae.Api.Controllers;

// EN: This controller exposes the access point administration endpoints (RF-06).
// ES: Este controlador expone los endpoints de administración de puntos de acceso (RF-06).
[ApiController]
[Route("api/access-points")]
[Produces("application/json")]
public sealed class AccessPointsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public AccessPointsController(IDispatcher dispatcher) => _dispatcher = dispatcher;

    /// <summary>EN: Registers an access point. / ES: Registra un punto de acceso.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccessPointDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccessPointDto>> Register(RegisterAccessPointRequest request, CancellationToken cancellationToken)
    {
        var accessPoint = await _dispatcher.SendAsync(
            new RegisterAccessPointCommand(request.Code, request.Name, request.Type, request.Description), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = accessPoint.Id }, accessPoint);
    }

    /// <summary>EN: Lists access points; use isEnabled=true to see only the enabled ones. / ES: Lista puntos de acceso; use isEnabled=true para ver solo los habilitados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccessPointDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AccessPointDto>>> List(
        [FromQuery] bool? isEnabled, [FromQuery] AccessPointType? type, CancellationToken cancellationToken) =>
        Ok(await _dispatcher.QueryAsync(new GetAccessPointsQuery(isEnabled, type), cancellationToken));

    /// <summary>EN: Gets an access point by id. / ES: Obtiene un punto de acceso por id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccessPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccessPointDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _dispatcher.QueryAsync(new GetAccessPointByIdQuery(id), cancellationToken));

    /// <summary>EN: Enables an access point. / ES: Habilita un punto de acceso.</summary>
    [HttpPatch("{id:guid}/enable")]
    [ProducesResponseType(typeof(AccessPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccessPointDto>> Enable(Guid id, CancellationToken cancellationToken) =>
        Ok(await _dispatcher.SendAsync(new EnableAccessPointCommand(id), cancellationToken));

    /// <summary>EN: Disables an access point. / ES: Deshabilita un punto de acceso.</summary>
    [HttpPatch("{id:guid}/disable")]
    [ProducesResponseType(typeof(AccessPointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccessPointDto>> Disable(Guid id, CancellationToken cancellationToken) =>
        Ok(await _dispatcher.SendAsync(new DisableAccessPointCommand(id), cancellationToken));
}
