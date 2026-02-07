using System.Collections.Immutable;
using System.Text;

namespace Clywell.Primitives;

/// <summary>
/// Represents an error with a code, description, and optional metadata.
/// This is the base error type used throughout the Result pattern.
/// </summary>
/// <remarks>
/// <para>Error is implemented as a record for value-based equality and immutability.</para>
/// <para>Errors are composable — use <see cref="WithMetadata(string, object)"/> and <see cref="WithInnerError"/>
/// to enrich errors with additional context without mutation.</para>
/// </remarks>
public record Error
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="code">The categorized error code.</param>
    /// <param name="description">A human-readable description of the error.</param>
    public Error(ErrorCode code, string description)
    {
        Code = code;
        Description = description;
    }

    /// <summary>Gets the categorized error code.</summary>
    public ErrorCode Code { get; }

    /// <summary>Gets the human-readable description of the error.</summary>
    public string Description { get; }

    /// <summary>Gets the optional inner error that caused this error.</summary>
    public Error? InnerError { get; init; }

    /// <summary>Gets optional metadata associated with this error.</summary>
    public ImmutableDictionary<string, object> Metadata { get; init; } = [];

    // ============================================================
    // Factory Methods
    // ============================================================

    /// <summary>
    /// Creates a general failure error.
    /// </summary>
    /// <param name="description">A description of the failure.</param>
    /// <returns>A new <see cref="Error"/> with <see cref="ErrorCode.Failure"/> code.</returns>
    public static Error Failure(string description) =>
        new(ErrorCode.Failure, description);

    /// <summary>
    /// Creates a not-found error.
    /// </summary>
    /// <param name="description">A description of what was not found.</param>
    /// <returns>A new <see cref="Error"/> with <see cref="ErrorCode.NotFound"/> code.</returns>
    public static Error NotFound(string description) =>
        new(ErrorCode.NotFound, description);

    /// <summary>
    /// Creates a conflict error.
    /// </summary>
    /// <param name="description">A description of the conflict.</param>
    /// <returns>A new <see cref="Error"/> with <see cref="ErrorCode.Conflict"/> code.</returns>
    public static Error Conflict(string description) =>
        new(ErrorCode.Conflict, description);

    /// <summary>
    /// Creates an unauthorized error.
    /// </summary>
    /// <param name="description">A description of the authorization failure.</param>
    /// <returns>A new <see cref="Error"/> with <see cref="ErrorCode.Unauthorized"/> code.</returns>
    public static Error Unauthorized(string description) =>
        new(ErrorCode.Unauthorized, description);

    /// <summary>
    /// Creates a forbidden error.
    /// </summary>
    /// <param name="description">A description of the permission failure.</param>
    /// <returns>A new <see cref="Error"/> with <see cref="ErrorCode.Forbidden"/> code.</returns>
    public static Error Forbidden(string description) =>
        new(ErrorCode.Forbidden, description);

    /// <summary>
    /// Creates an unexpected/internal error.
    /// </summary>
    /// <param name="description">A description of the unexpected failure.</param>
    /// <returns>A new <see cref="Error"/> with <see cref="ErrorCode.Unexpected"/> code.</returns>
    public static Error Unexpected(string description) =>
        new(ErrorCode.Unexpected, description);

    /// <summary>
    /// Creates an unavailable error.
    /// </summary>
    /// <param name="description">A description of the availability issue.</param>
    /// <returns>A new <see cref="Error"/> with <see cref="ErrorCode.Unavailable"/> code.</returns>
    public static Error Unavailable(string description) =>
        new(ErrorCode.Unavailable, description);

    /// <summary>
    /// Creates a validation error with a single field violation.
    /// </summary>
    /// <param name="fieldName">The field that failed validation.</param>
    /// <param name="message">The validation error message.</param>
    /// <returns>A new <see cref="ValidationError"/> with the specified violation.</returns>
    public static ValidationError Validation(string fieldName, string message) =>
        new(new ValidationFailure(fieldName, message));

    /// <summary>
    /// Creates a validation error with multiple violations.
    /// </summary>
    /// <param name="failures">The validation failures.</param>
    /// <returns>A new <see cref="ValidationError"/> with the specified violations.</returns>
    public static ValidationError Validation(params ValidationFailure[] failures) =>
        new(failures);

    // ============================================================
    // Builder Methods (Immutable)
    // ============================================================

    /// <summary>
    /// Creates a new error with additional metadata.
    /// </summary>
    /// <param name="key">The metadata key.</param>
    /// <param name="value">The metadata value.</param>
    /// <returns>A new <see cref="Error"/> with the metadata added.</returns>
    public Error WithMetadata(string key, object value) =>
        this with { Metadata = Metadata.SetItem(key, value) };

    /// <summary>
    /// Creates a new error with additional metadata from a dictionary.
    /// </summary>
    /// <param name="metadata">The metadata to add.</param>
    /// <returns>A new <see cref="Error"/> with the metadata added.</returns>
    public Error WithMetadata(IEnumerable<KeyValuePair<string, object>> metadata) =>
        this with { Metadata = Metadata.SetItems(metadata) };

    /// <summary>
    /// Creates a new error with an inner error for error chaining.
    /// </summary>
    /// <param name="innerError">The inner error that caused this error.</param>
    /// <returns>A new <see cref="Error"/> with the inner error set.</returns>
    public Error WithInnerError(Error innerError) =>
        this with { InnerError = innerError };

    // ============================================================
    // Display
    // ============================================================

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"[{Code}] {Description}");

        if (Metadata.Count > 0)
        {
            sb.Append(" {");
            sb.AppendJoin(", ", Metadata.Select(kv => $"{kv.Key}={kv.Value}"));
            sb.Append('}');
        }

        if (InnerError is not null)
        {
            sb.Append($" --> {InnerError}");
        }

        return sb.ToString();
    }
}
