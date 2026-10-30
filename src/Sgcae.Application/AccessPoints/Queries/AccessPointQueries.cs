using Sgcae.Application.Abstractions.Messaging;
using Sgcae.Application.Common;
using Sgcae.Domain.AccessPoints;

namespace Sgcae.Application.AccessPoints.Queries;

// EN: QUERIES (RF-06): consult the access points, optionally only the enabled ones.
// ES: CONSULTAS (RF-06): consultar los puntos de acceso, opcionalmente solo los habilitados.
public sealed record GetAccessPointsQuery(bool? IsEnabled, AccessPointType? Type) : IQuery<IReadOnlyList<AccessPointDto>>;

public sealed record GetAccessPointByIdQuery(Guid AccessPointId) : IQuery<AccessPointDto>;

public sealed class AccessPointQueriesHandler :
    IQueryHandler<GetAccessPointsQuery, IReadOnlyList<AccessPointDto>>,
    IQueryHandler<GetAccessPointByIdQuery, AccessPointDto>
{
    private readonly IAccessPointQueryService _queries;

    public AccessPointQueriesHandler(IAccessPointQueryService queries) => _queries = queries;

    public Task<IReadOnlyList<AccessPointDto>> HandleAsync(GetAccessPointsQuery query, CancellationToken cancellationToken) =>
        _queries.ListAsync(query.IsEnabled, query.Type, cancellationToken);

    public async Task<AccessPointDto> HandleAsync(GetAccessPointByIdQuery query, CancellationToken cancellationToken) =>
        await _queries.GetByIdAsync(query.AccessPointId, cancellationToken)
        ?? throw new NotFoundException("AccessPoint", query.AccessPointId);
}
