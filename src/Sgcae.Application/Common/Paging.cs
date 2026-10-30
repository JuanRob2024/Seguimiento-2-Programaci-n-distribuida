namespace Sgcae.Application.Common;

// EN: This record holds a page of results plus the information the client needs to keep paginating.
// ES: Este record contiene una página de resultados más la información que el cliente necesita para seguir paginando.
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

// EN: This helper normalizes the page parameters so a client can never request huge volumes of data at once.
// ES: Este helper normaliza los parámetros de paginación para que un cliente nunca pida grandes volúmenes de datos de una vez.
public static class Paging
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static (int Page, int PageSize) Normalize(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);
        return (normalizedPage, normalizedSize);
    }
}
