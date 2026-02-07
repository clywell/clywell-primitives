using System.Collections.Immutable;
using System.Text;

namespace Clywell.Primitives;

/// <summary>
/// Represents a validation error containing one or more field-level validation failures.
/// Extends <see cref="Error"/> with structured validation details.
/// </summary>
public sealed record ValidationError : Error
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationError"/> class with a single failure.
    /// </summary>
    /// <param name="failure">The validation failure.</param>
    public ValidationError(ValidationFailure failure)
        : base(ErrorCode.Validation, "One or more validation errors occurred.")
    {
        Failures = [failure];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationError"/> class with multiple failures.
    /// </summary>
    /// <param name="failures">The collection of validation failures.</param>
    public ValidationError(params ValidationFailure[] failures)
        : base(ErrorCode.Validation, "One or more validation errors occurred.")
    {
        Failures = [.. failures];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationError"/> class with
    /// an enumerable of failures.
    /// </summary>
    /// <param name="failures">The collection of validation failures.</param>
    public ValidationError(IEnumerable<ValidationFailure> failures)
        : base(ErrorCode.Validation, "One or more validation errors occurred.")
    {
        Failures = [.. failures];
    }

    /// <summary>Gets the immutable list of validation failures.</summary>
    public ImmutableArray<ValidationFailure> Failures { get; }

    /// <summary>Gets the number of validation failures.</summary>
    public int FailureCount => Failures.Length;

    /// <summary>
    /// Gets all failures for a specific field.
    /// </summary>
    /// <param name="fieldName">The field name to filter by.</param>
    /// <returns>An enumerable of failures for the specified field.</returns>
    public IEnumerable<ValidationFailure> GetFailuresForField(string fieldName) =>
        Failures.Where(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Checks whether a specific field has any validation failures.
    /// </summary>
    /// <param name="fieldName">The field name to check.</param>
    /// <returns><see langword="true"/> if the field has failures; otherwise <see langword="false"/>.</returns>
    public bool HasFailureForField(string fieldName) =>
        Failures.Any(f => f.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Creates a new <see cref="ValidationError"/> with additional failures appended.
    /// </summary>
    /// <param name="failures">The failures to add.</param>
    /// <returns>A new <see cref="ValidationError"/> with the combined failures.</returns>
    public ValidationError AddFailures(params ValidationFailure[] failures) =>
        new([.. Failures, .. failures]);

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[{Code}] {Description}");

        foreach (var failure in Failures)
        {
            sb.AppendLine($"  - {failure}");
        }

        return sb.ToString().TrimEnd();
    }
}
