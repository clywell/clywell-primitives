namespace Clywell.Primitives;

/// <summary>
/// Represents a single field-level validation failure.
/// </summary>
/// <param name="FieldName">The name of the field that failed validation.</param>
/// <param name="Message">The validation failure message.</param>
public readonly record struct ValidationFailure(string FieldName, string Message)
{
    /// <inheritdoc />
    public override string ToString() => $"{FieldName}: {Message}";
}
