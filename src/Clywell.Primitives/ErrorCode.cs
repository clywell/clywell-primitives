namespace Clywell.Primitives;

/// <summary>
/// Represents a categorized error code for classifying errors.
/// Uses a string-based approach for extensibility while providing
/// common built-in codes for standard error scenarios.
/// </summary>
/// <remarks>
/// ErrorCode is implemented as a readonly record struct for value semantics,
/// zero-allocation comparisons, and immutability.
/// </remarks>
/// <param name="Value">The string value of the error code.</param>
public readonly record struct ErrorCode(string Value)
{
    // ============================================================
    // Built-in Error Codes
    // ============================================================

    /// <summary>General/unspecified failure.</summary>
    public static readonly ErrorCode Failure = new("General.Failure");

    /// <summary>One or more validation rules were violated.</summary>
    public static readonly ErrorCode Validation = new("General.Validation");

    /// <summary>The requested resource was not found.</summary>
    public static readonly ErrorCode NotFound = new("General.NotFound");

    /// <summary>A conflict occurred (e.g., duplicate resource).</summary>
    public static readonly ErrorCode Conflict = new("General.Conflict");

    /// <summary>The caller is not authorized to perform this action.</summary>
    public static readonly ErrorCode Unauthorized = new("General.Unauthorized");

    /// <summary>The caller is authenticated but lacks sufficient permissions.</summary>
    public static readonly ErrorCode Forbidden = new("General.Forbidden");

    /// <summary>An unexpected internal error occurred.</summary>
    public static readonly ErrorCode Unexpected = new("General.Unexpected");

    /// <summary>The service or resource is temporarily unavailable.</summary>
    public static readonly ErrorCode Unavailable = new("General.Unavailable");

    // ============================================================
    // Conversions
    // ============================================================

    /// <summary>
    /// Implicitly converts a string to an <see cref="ErrorCode"/>.
    /// </summary>
    /// <param name="value">The error code string.</param>
    public static implicit operator ErrorCode(string value) => new(value);

    /// <summary>
    /// Implicitly converts an <see cref="ErrorCode"/> to its string representation.
    /// </summary>
    /// <param name="code">The error code.</param>
    public static implicit operator string(ErrorCode code) => code.Value;

    /// <inheritdoc />
    public override string ToString() => Value;
}
