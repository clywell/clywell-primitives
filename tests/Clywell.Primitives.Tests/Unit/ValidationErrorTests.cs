namespace Clywell.Primitives.Tests;

/// <summary>
/// Tests for <see cref="ValidationError"/> and <see cref="ValidationFailure"/>.
/// </summary>
public class ValidationErrorTests
{
    // ============================================================
    // ValidationFailure Tests
    // ============================================================

    [Fact]
    public void ValidationFailure_ShouldStoreFieldNameAndMessage()
    {
        var failure = new ValidationFailure("Email", "Email is required.");

        Assert.Equal("Email", failure.FieldName);
        Assert.Equal("Email is required.", failure.Message);
    }

    [Fact]
    public void ValidationFailure_ToString_ShouldFormat()
    {
        var failure = new ValidationFailure("Name", "Must not be empty");
        Assert.Equal("Name: Must not be empty", failure.ToString());
    }

    [Fact]
    public void ValidationFailure_Equality_ShouldWork()
    {
        var f1 = new ValidationFailure("Email", "Required");
        var f2 = new ValidationFailure("Email", "Required");
        var f3 = new ValidationFailure("Name", "Required");

        Assert.Equal(f1, f2);
        Assert.NotEqual(f1, f3);
    }

    // ============================================================
    // ValidationError Construction Tests
    // ============================================================

    [Fact]
    public void SingleFailure_ShouldCreateCorrectly()
    {
        var error = new ValidationError(new ValidationFailure("Email", "Required"));

        Assert.Equal(ErrorCode.Validation, error.Code);
        Assert.Equal("One or more validation errors occurred.", error.Description);
        Assert.Single(error.Failures);
        Assert.Equal(1, error.FailureCount);
    }

    [Fact]
    public void MultipleFailures_ShouldCreateCorrectly()
    {
        var error = new ValidationError(
            new ValidationFailure("Name", "Required"),
            new ValidationFailure("Email", "Invalid format"),
            new ValidationFailure("Age", "Must be positive"));

        Assert.Equal(3, error.FailureCount);
    }

    [Fact]
    public void EnumerableFailures_ShouldCreateCorrectly()
    {
        var failures = new List<ValidationFailure>
        {
            new("Field1", "Error1"),
            new("Field2", "Error2")
        };

        var error = new ValidationError(failures);

        Assert.Equal(2, error.FailureCount);
    }

    // ============================================================
    // Query Methods Tests
    // ============================================================

    [Fact]
    public void GetFailuresForField_ExistingField_ShouldReturnMatches()
    {
        var error = new ValidationError(
            new ValidationFailure("Email", "Required"),
            new ValidationFailure("Email", "Invalid format"),
            new ValidationFailure("Name", "Too short"));

        var emailFailures = error.GetFailuresForField("Email").ToList();

        Assert.Equal(2, emailFailures.Count);
        Assert.All(emailFailures, f => Assert.Equal("Email", f.FieldName));
    }

    [Fact]
    public void GetFailuresForField_NonExistingField_ShouldReturnEmpty()
    {
        var error = new ValidationError(new ValidationFailure("Email", "Required"));

        var nameFailures = error.GetFailuresForField("Name");

        Assert.Empty(nameFailures);
    }

    [Fact]
    public void GetFailuresForField_ShouldBeCaseInsensitive()
    {
        var error = new ValidationError(new ValidationFailure("Email", "Required"));

        var failures = error.GetFailuresForField("email");

        Assert.Single(failures);
    }

    [Fact]
    public void HasFailureForField_ExistingField_ShouldReturnTrue()
    {
        var error = new ValidationError(new ValidationFailure("Email", "Required"));

        Assert.True(error.HasFailureForField("Email"));
    }

    [Fact]
    public void HasFailureForField_NonExistingField_ShouldReturnFalse()
    {
        var error = new ValidationError(new ValidationFailure("Email", "Required"));

        Assert.False(error.HasFailureForField("Name"));
    }

    [Fact]
    public void HasFailureForField_ShouldBeCaseInsensitive()
    {
        var error = new ValidationError(new ValidationFailure("Email", "Required"));

        Assert.True(error.HasFailureForField("email"));
    }

    // ============================================================
    // AddFailures Tests
    // ============================================================

    [Fact]
    public void AddFailures_ShouldReturnNewInstanceWithCombinedFailures()
    {
        var original = new ValidationError(new ValidationFailure("Email", "Required"));

        var updated = original.AddFailures(
            new ValidationFailure("Name", "Too short"));

        Assert.Equal(1, original.FailureCount);
        Assert.Equal(2, updated.FailureCount);
    }

    // ============================================================
    // ToString Tests
    // ============================================================

    [Fact]
    public void ToString_ShouldListAllFailures()
    {
        var error = new ValidationError(
            new ValidationFailure("Email", "Required"),
            new ValidationFailure("Name", "Too short"));

        var str = error.ToString();

        Assert.Contains("Email: Required", str);
        Assert.Contains("Name: Too short", str);
        Assert.Contains("[General.Validation]", str);
    }

    // ============================================================
    // Error.Validation Factory Tests
    // ============================================================

    [Fact]
    public void ErrorValidation_SingleField_ShouldReturnValidationError()
    {
        var error = Error.Validation("Password", "Too weak");

        Assert.IsType<ValidationError>(error);
        Assert.Equal(ErrorCode.Validation, error.Code);
        Assert.Single(error.Failures);
    }

    [Fact]
    public void ErrorValidation_MultipleFields_ShouldReturnValidationError()
    {
        var error = Error.Validation(
            new ValidationFailure("Field1", "Error1"),
            new ValidationFailure("Field2", "Error2"));

        Assert.Equal(2, error.FailureCount);
    }

    // ============================================================
    // Empty Failures Guard Tests
    // ============================================================

    [Fact]
    public void Constructor_EmptyParamsArray_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValidationError());
    }

    [Fact]
    public void Constructor_EmptyEnumerable_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(
            () => new ValidationError(Enumerable.Empty<ValidationFailure>()));
    }
}
