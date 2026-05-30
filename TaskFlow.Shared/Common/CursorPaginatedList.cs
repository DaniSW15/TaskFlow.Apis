namespace TaskFlow.Shared.Common;

/// <summary>
/// Resultado de una consulta paginada con cursores (keyset pagination).
/// A diferencia de PaginatedList, no requiere COUNT(*) ni OFFSET, lo que
/// lo hace significativamente más eficiente en tablas grandes.
/// </summary>
public sealed class CursorPaginatedList<T>
{
    /// <summary>Los elementos de la página actual.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Cursor opaco (Base64) para obtener la siguiente página.
    /// Es null si no hay más resultados.
    /// </summary>
    public string? NextCursor { get; }

    /// <summary>Indica si existe una página siguiente.</summary>
    public bool HasNextPage => NextCursor is not null;

    /// <summary>Cantidad de ítems solicitada por página.</summary>
    public int PageSize { get; }

    public CursorPaginatedList(IReadOnlyList<T> items, string? nextCursor, int pageSize)
    {
        Items = items;
        NextCursor = nextCursor;
        PageSize = pageSize;
    }
}
