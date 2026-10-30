using Microsoft.AspNetCore.Mvc;
using Sgcae.Api.Contracts;
using Sgcae.Application.Abstractions.Messaging;
using Sgcae.Application.Common;
using Sgcae.Application.Credentials.Commands;
using Sgcae.Application.Credentials.Queries;

namespace Sgcae.Api.Controllers;

// EN: This controller exposes the credential management endpoints (RF-02).
// ES: Este controlador expone los endpoints de gestión de credenciales (RF-02).
[ApiController]
[Route("api/credentials")]
[Produces("application/json")]
public sealed class CredentialsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public CredentialsController(IDispatcher dispatcher) => _dispatcher = dispatcher;

    /// <summary>EN: Assigns a credential to an employee. / ES: Asigna una credencial a un empleado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CredentialDto>> Assign(AssignCredentialRequest request, CancellationToken cancellationToken)
    {
        var credential = await _dispatcher.SendAsync(
            new AssignCredentialCommand(request.EmployeeId, request.Code, request.ExpiresAtUtc), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = credential.Id }, credential);
    }

    /// <summary>EN: Gets a credential and its status by id. / ES: Consulta una credencial y su estado por id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CredentialDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _dispatcher.QueryAsync(new GetCredentialByIdQuery(id), cancellationToken));

    /// <summary>EN: Gets a credential and its status by code. / ES: Consulta una credencial y su estado por código.</summary>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CredentialDto>> GetByCode(string code, CancellationToken cancellationToken) =>
        Ok(await _dispatcher.QueryAsync(new GetCredentialByCodeQuery(code), cancellationToken));

    /// <summary>EN: Blocks an active credential. / ES: Bloquea una credencial activa.</summary>
    [HttpPatch("{id:guid}/block")]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CredentialDto>> Block(
        Guid id, CredentialStatusChangeRequest request, CancellationToken cancellationToken) =>
        Ok(await _dispatcher.SendAsync(new BlockCredentialCommand(id, request.Reason), cancellationToken));

    /// <summary>EN: Unblocks a blocked credential. / ES: Desbloquea una credencial bloqueada.</summary>
    [HttpPatch("{id:guid}/unblock")]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CredentialDto>> Unblock(Guid id, CancellationToken cancellationToken) =>
        Ok(await _dispatcher.SendAsync(new UnblockCredentialCommand(id), cancellationToken));

    /// <summary>EN: Revokes a credential permanently. / ES: Revoca una credencial de forma permanente.</summary>
    [HttpPatch("{id:guid}/revoke")]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CredentialDto>> Revoke(
        Guid id, CredentialStatusChangeRequest request, CancellationToken cancellationToken) =>
        Ok(await _dispatcher.SendAsync(new RevokeCredentialCommand(id, request.Reason), cancellationToken));
}
