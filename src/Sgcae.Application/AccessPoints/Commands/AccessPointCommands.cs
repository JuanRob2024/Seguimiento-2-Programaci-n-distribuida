using Sgcae.Application.Abstractions;
using Sgcae.Application.Abstractions.Messaging;
using Sgcae.Application.Common;
using Sgcae.Domain.AccessPoints;
using Sgcae.Domain.Repositories;

namespace Sgcae.Application.AccessPoints.Commands;

// EN: COMMAND (RF-06): register a physical access point.
// ES: COMANDO (RF-06): registrar un punto de acceso físico.
public sealed record RegisterAccessPointCommand(
    string Code,
    string Name,
    AccessPointType Type,
    string? Description) : ICommand<AccessPointDto>;

// EN: COMMANDS (RF-06): enable or disable an access point.
// ES: COMANDOS (RF-06): habilitar o deshabilitar un punto de acceso.
public sealed record EnableAccessPointCommand(Guid AccessPointId) : ICommand<AccessPointDto>;

public sealed record DisableAccessPointCommand(Guid AccessPointId) : ICommand<AccessPointDto>;

public sealed class AccessPointCommandsHandler :
    ICommandHandler<RegisterAccessPointCommand, AccessPointDto>,
    ICommandHandler<EnableAccessPointCommand, AccessPointDto>,
    ICommandHandler<DisableAccessPointCommand, AccessPointDto>
{
    private readonly IAccessPointRepository _accessPoints;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public AccessPointCommandsHandler(IAccessPointRepository accessPoints, IUnitOfWork unitOfWork, IDateTimeProvider clock)
    {
        _accessPoints = accessPoints;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<AccessPointDto> HandleAsync(RegisterAccessPointCommand command, CancellationToken cancellationToken)
    {
        var code = AccessPointCode.Create(command.Code);

        // EN: Here I validate that the access point code is unique.
        // ES: Aquí valido que el código del punto de acceso sea único.
        if (await _accessPoints.ExistsByCodeAsync(code, cancellationToken))
        {
            throw new ConflictException($"An access point with code '{code}' already exists.");
        }

        var accessPoint = AccessPoint.Register(code, command.Name, command.Type, command.Description, _clock.UtcNow);
        _accessPoints.Add(accessPoint);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccessPointDto.From(accessPoint);
    }

    public Task<AccessPointDto> HandleAsync(EnableAccessPointCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.AccessPointId, ap => ap.Enable(), cancellationToken);

    public Task<AccessPointDto> HandleAsync(DisableAccessPointCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.AccessPointId, ap => ap.Disable(), cancellationToken);

    private async Task<AccessPointDto> ChangeAsync(
        Guid id, Action<AccessPoint> change, CancellationToken cancellationToken)
    {
        var accessPoint = await _accessPoints.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("AccessPoint", id);

        change(accessPoint);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return AccessPointDto.From(accessPoint);
    }
}
