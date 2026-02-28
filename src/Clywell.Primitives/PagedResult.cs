namespace Clywell.Primitives;

/// <summary>
/// Represents a single page of results from a paged query.
/// </summary>
/// <typeparam name="T">The type of items in the page.</typeparam>
/// <param name="Items">The items on the current page.</param>
/// <param name="TotalCount">The total number of items across all pages.</param>
/// <param name="Page">The 1-based page number of the current page.</param>
/// <param name="PageSize">The maximum number of items per page.</param>
/// <remarks>
/// <para>
/// Use <see cref="PagedResult{T}"/> as the return type from any repository or query
/// that supports pagination. It carries the requested page of items together with
/// enough metadata for callers to build pagination controls (next/previous page,
/// total page count).
/// </para>
/// </remarks>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>Gets a value indicating whether there is a page before this one.</summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>Gets a value indicating whether there is a page after this one.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Creates an empty paged result (zero items, zero total count) for the given page parameters.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The page size.</param>
    public static PagedResult<T> Empty(int page = 1, int pageSize = 20)
        => new([], 0, page, pageSize);
}
