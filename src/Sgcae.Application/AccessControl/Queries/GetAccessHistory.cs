using Sgcae.Application.Abstractions.Messaging;
using Sgcae.Application.Common;
using Sgcae.Domain.AccessLogs;

namespace Sgcae.Application.AccessControl.Queries;

// EN: QUERY (RF-04): consult the access history filtering by employee, date range, access point and result.
//     It is ALWAYS paginated to avoid returning large volumes of data at once.
// ES: CONSULTA (RF-04): consultar el historial de accesos filtrando por empleado, rango de fechas, punto de acceso y
//     resultado. SIEMPRE es paginada para evitar retornar grandes volúmenes de información de una sola vez.
public sealed record GetAccessHistoryQuery(
    Guid? EmployeeId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    Guid? AccessPointId,
    AccessResult? Result,
    MovementType? Movement,
    int Page = 1,
    int PageSize = Paging.DefaultPageSize) : IQuery<PagedResult<AccessLogDto>>;

public sealed class GetAccessHistoryHandler : IQueryHandler<GetAccessHistoryQuery, PagedResult<AccessLogDto>>
{
    private readonly IAccessLogQueryService _queries;

    public GetAccessHistoryHandler(IAccessLogQueryService queries) => _queries = queries;

    public Task<PagedResult<AccessLogDto>> HandleAsync(GetAccessHistoryQuery query, CancellationToken cancellationToken)
    {
        // EN: Here I validate that the date range makes sense.
        // ES: Aquí valido que el rango de fechas tenga sentido.
        if (query.FromUtc.HasValue && query.ToUtc.HasValue && query.FromUtc > query.ToUtc)
        {
            throw new RequestValidationException("'fromUtc' must be earlier than or equal to 'toUtc'.");
        }

        var (page, pageSize) = Paging.Normalize(query.Page, query.PageSize);
        var filter = new AccessHistoryFilter(
            query.EmployeeId, query.FromUtc, query.ToUtc, query.AccessPointId, query.Result, query.Movement);

        return _queries.SearchAsync(filter, page, pageSize, cancellationToken);
    }
}
