using Sgcae.Application.Abstractions;
using Sgcae.Application.Abstractions.Messaging;
using Sgcae.Application.Common;
using Sgcae.Domain.Credentials;
using Sgcae.Domain.Repositories;

namespace Sgcae.Application.Credentials.Commands;

// EN: COMMANDS (RF-02): block, unblock and revoke a credential. The status rules live inside the Credential aggregate.
// ES: COMANDOS (RF-02): bloquear, desbloquear y revocar una credencial. Las reglas de estado viven dentro del agregado Credencial.
public sealed record BlockCredentialCommand(Guid CredentialId, string? Reason) : ICommand<CredentialDto>;

public sealed record UnblockCredentialCommand(Guid CredentialId) : ICommand<CredentialDto>;

public sealed record RevokeCredentialCommand(Guid CredentialId, string? Reason) : ICommand<CredentialDto>;

// EN: I use one handler class for the three commands because they share the same flow:
//     load the aggregate -> call its method -> save.
// ES: Uso una sola clase manejadora para los tres comandos porque comparten el mismo flujo:
//     cargar el agregado -> invocar su método -> guardar.
public sealed class ChangeCredentialStatusHandler :
    ICommandHandler<BlockCredentialCommand, CredentialDto>,
    ICommandHandler<UnblockCredentialCommand, CredentialDto>,
    ICommandHandler<RevokeCredentialCommand, CredentialDto>
{
    private readonly ICredentialRepository _credentials;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public ChangeCredentialStatusHandler(ICredentialRepository credentials, IUnitOfWork unitOfWork, IDateTimeProvider clock)
    {
        _credentials = credentials;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public Task<CredentialDto> HandleAsync(BlockCredentialCommand command, CancellationToken cancellationToken) =>
        ExecuteAsync(command.CredentialId, (credential, now) => credential.Block(command.Reason, now), cancellationToken);

    public Task<CredentialDto> HandleAsync(UnblockCredentialCommand command, CancellationToken cancellationToken) =>
        ExecuteAsync(command.CredentialId, (credential, now) => credential.Unblock(now), cancellationToken);

    public Task<CredentialDto> HandleAsync(RevokeCredentialCommand command, CancellationToken cancellationToken) =>
        ExecuteAsync(command.CredentialId, (credential, now) => credential.Revoke(command.Reason, now), cancellationToken);

    private async Task<CredentialDto> ExecuteAsync(
        Guid credentialId, Action<Credential, DateTime> change, CancellationToken cancellationToken)
    {
        var credential = await _credentials.GetByIdAsync(credentialId, cancellationToken)
            ?? throw new NotFoundException("Credential", credentialId);

        var now = _clock.UtcNow;
        change(credential, now);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CredentialDto.From(credential, now);
    }
}
