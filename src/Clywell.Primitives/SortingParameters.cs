namespace Clywell.Primitives;

/// <summary>
/// Specifies the direction in which results should be sorted.
/// </summary>
public enum SortDirection
{
    /// <summary>Sort from lowest to highest (A → Z, 0 → 9, oldest → newest).</summary>
    Ascending,

    /// <summary>Sort from highest to lowest (Z → A, 9 → 0, newest → oldest).</summary>
    Descending
}

/// <summary>
/// Encapsulates sorting parameters for a sortable query.
/// </summary>
/// <param name="SortBy">The name of the field or property to sort by.</param>
/// <param name="Direction">The sort direction. Defaults to <see cref="SortDirection.Ascending"/>.</param>
/// <remarks>
/// Pass a <see cref="SortingParameters"/> instance to repository or query methods
/// that support ordering. Combine with <see cref="PagingParameters"/> for
/// sorted, paged results.
/// </remarks>
public sealed record SortingParameters(string SortBy, SortDirection Direction = SortDirection.Ascending)
{
    /// <summary>Gets a value indicating whether the sort direction is descending.</summary>
    public bool IsDescending => Direction == SortDirection.Descending;
}
