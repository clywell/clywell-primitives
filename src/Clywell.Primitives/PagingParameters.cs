using System.ComponentModel.DataAnnotations;

namespace Clywell.Primitives;

/// <summary>
/// Encapsulates pagination parameters for a paged query.
/// </summary>
/// <remarks>
/// Pass a <see cref="PagingParameters"/> instance to repository or query methods
/// that support pagination. Combine with <see cref="SortingParameters"/> for
/// sorted, paged results.
/// </remarks>
public sealed record PagingParameters : IValidatableObject
{
    /// <summary>The maximum allowed page size.</summary>
    public const int MaxPageSize = 100;

    /// <summary>The default page size when none is specified.</summary>
    public const int DefaultPageSize = 20;

    public PagingParameters()
        : this(1, DefaultPageSize)
    {
    }

    public PagingParameters(int page, int pageSize)
    {
        Page = page;
        PageSize = Math.Min(pageSize, MaxPageSize);
    }

    /// <summary>Gets the 1-based page number.</summary>
    [Range(1, int.MaxValue)]
    public int Page { get; init; }

    /// <summary>Gets the number of items per page (capped at <see cref="MaxPageSize"/>).</summary>
    [Range(1, MaxPageSize)]
    public int PageSize { get; init; }

    /// <summary>Gets the number of items to skip to reach this page.</summary>
    public int Skip => (Page - 1) * PageSize;

    /// <summary>Returns default paging parameters (page 1, default page size).</summary>
    public static PagingParameters Default => new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Page < 1)
        {
            yield return new ValidationResult("The field Page must be between 1 and 2147483647.", [nameof(Page)]);
        }

        if (PageSize < 1 || PageSize > MaxPageSize)
        {
            yield return new ValidationResult($"The field PageSize must be between 1 and {MaxPageSize}.", [nameof(PageSize)]);
        }
    }
}
