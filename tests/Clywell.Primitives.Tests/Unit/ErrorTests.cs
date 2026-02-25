namespace Clywell.Primitives.Tests;

/// <summary>
/// Tests for <see cref="Error"/>.
/// </summary>
public class ErrorTests
{
    // ============================================================
    // Constructor Tests
    // ============================================================

    [Fact]
    public void Constructor_ShouldSetCodeAndDescription()
    {
        var error = new Error(ErrorCode.NotFound, "Resource was not found.");

        Assert.Equal(ErrorCode.NotFound, error.Code);
        Assert.Equal("Resource was not found.", error.Description);
    }

    [Fact]
    public void InnerError_ShouldBeNullByDefault()
    {
        var error = new Error(ErrorCode.Failure, "Test");
        Assert.Null(error.InnerError);
    }

    [Fact]
    public void Metadata_ShouldBeEmptyByDefault()
    {
        var error = new Error(ErrorCode.Failure, "Test");
        Assert.Empty(error.Metadata);
    }

    // ============================================================
    // Factory Method Tests
    // ============================================================

    [Fact]
    public void Failure_ShouldCreateWithCorrectCode()
    {
        var error = Error.Failure("Something went wrong.");
        Assert.Equal(ErrorCode.Failure, error.Code);
        Assert.Equal("Something went wrong.", error.Description);
    }

    [Fact]
    public void NotFound_ShouldCreateWithCorrectCode()
    {
        var error = Error.NotFound("User not found.");
        Assert.Equal(ErrorCode.NotFound, error.Code);
        Assert.Equal("User not found.", error.Description);
    }

    [Fact]
    public void Conflict_ShouldCreateWithCorrectCode()
    {
        var error = Error.Conflict("Duplicate entry.");
        Assert.Equal(ErrorCode.Conflict, error.Code);
    }

    [Fact]
    public void Unauthorized_ShouldCreateWithCorrectCode()
    {
        var error = Error.Unauthorized("Not authenticated.");
        Assert.Equal(ErrorCode.Unauthorized, error.Code);
    }

    [Fact]
    public void Forbidden_ShouldCreateWithCorrectCode()
    {
        var error = Error.Forbidden("Access denied.");
        Assert.Equal(ErrorCode.Forbidden, error.Code);
    }

    [Fact]
    public void Unexpected_ShouldCreateWithCorrectCode()
    {
        var error = Error.Unexpected("Something broke.");
        Assert.Equal(ErrorCode.Unexpected, error.Code);
    }

    [Fact]
    public void Unavailable_ShouldCreateWithCorrectCode()
    {
        var error = Error.Unavailable("Service down.");
        Assert.Equal(ErrorCode.Unavailable, error.Code);
    }

    [Fact]
    public void Validation_SingleField_ShouldCreateValidationError()
    {
        var error = Error.Validation("Email", "Email is required.");

        Assert.IsType<ValidationError>(error);
        Assert.Equal(ErrorCode.Validation, error.Code);
        Assert.Single(error.Failures);
        Assert.Equal("Email", error.Failures[0].FieldName);
    }

    [Fact]
    public void Validation_MultipleFields_ShouldCreateValidationError()
    {
        var error = Error.Validation(
            new ValidationFailure("Name", "Required"),
            new ValidationFailure("Age", "Must be positive"));

        Assert.Equal(2, error.FailureCount);
    }

    // ============================================================
    // Builder Method Tests
    // ============================================================

    [Fact]
    public void WithMetadata_ShouldAddKeyValuePair()
    {
        var error = Error.Failure("Test").WithMetadata("RequestId", "abc-123");

        Assert.Single(error.Metadata);
        Assert.Equal("abc-123", error.Metadata["RequestId"]);
    }

    [Fact]
    public void WithMetadata_MultipleCalls_ShouldAccumulate()
    {
        var error = Error.Failure("Test")
            .WithMetadata("Key1", "Value1")
            .WithMetadata("Key2", "Value2");

        Assert.Equal(2, error.Metadata.Count);
        Assert.Equal("Value1", error.Metadata["Key1"]);
        Assert.Equal("Value2", error.Metadata["Key2"]);
    }

    [Fact]
    public void WithMetadata_Dictionary_ShouldAddAll()
    {
        var metadata = new Dictionary<string, object>
        {
            ["A"] = 1,
            ["B"] = "two"
        };

        var error = Error.Failure("Test").WithMetadata(metadata);

        Assert.Equal(2, error.Metadata.Count);
    }

    [Fact]
    public void WithMetadata_ShouldBeImmutable()
    {
        var original = Error.Failure("Test");
        var withMeta = original.WithMetadata("Key", "Value");

        Assert.Empty(original.Metadata);
        Assert.Single(withMeta.Metadata);
    }

    [Fact]
    public void WithInnerError_ShouldSetInnerError()
    {
        var inner = Error.Unexpected("Root cause");
        var outer = Error.Failure("Wrapper").WithInnerError(inner);

        Assert.NotNull(outer.InnerError);
        Assert.Equal(ErrorCode.Unexpected, outer.InnerError.Code);
        Assert.Equal("Root cause", outer.InnerError.Description);
    }

    [Fact]
    public void WithInnerError_ShouldBeImmutable()
    {
        var original = Error.Failure("Test");
        var inner = Error.Unexpected("Inner");
        var withInner = original.WithInnerError(inner);

        Assert.Null(original.InnerError);
        Assert.NotNull(withInner.InnerError);
    }

    // ============================================================
    // ToString Tests
    // ============================================================

    [Fact]
    public void ToString_BasicError_ShouldFormatCorrectly()
    {
        var error = Error.NotFound("User not found.");
        Assert.Equal("[General.NotFound] User not found.", error.ToString());
    }

    [Fact]
    public void ToString_WithMetadata_ShouldIncludeMetadata()
    {
        var error = Error.Failure("Test").WithMetadata("RequestId", "123");
        var str = error.ToString();

        Assert.Contains("RequestId=123", str);
    }

    [Fact]
    public void ToString_WithInnerError_ShouldChainErrors()
    {
        var inner = Error.Unexpected("Root cause");
        var outer = Error.Failure("Wrapper").WithInnerError(inner);
        var str = outer.ToString();

        Assert.Contains("-->", str);
        Assert.Contains("Root cause", str);
    }

    // ============================================================
    // Record Equality Tests
    // ============================================================

    [Fact]
    public void Equality_SameCodeAndDescription_ShouldBeEqual()
    {
        var error1 = new Error(ErrorCode.NotFound, "Not found.");
        var error2 = new Error(ErrorCode.NotFound, "Not found.");

        Assert.Equal(error1, error2);
    }

    [Fact]
    public void Equality_DifferentDescription_ShouldNotBeEqual()
    {
        var error1 = new Error(ErrorCode.NotFound, "User not found.");
        var error2 = new Error(ErrorCode.NotFound, "Order not found.");

        Assert.NotEqual(error1, error2);
    }

    [Fact]
    public void Equality_DifferentCode_ShouldNotBeEqual()
    {
        var error1 = new Error(ErrorCode.NotFound, "Test");
        var error2 = new Error(ErrorCode.Conflict, "Test");

        Assert.NotEqual(error1, error2);
    }

    // ============================================================
    // Equality With Metadata Tests
    // ============================================================

    [Fact]
    public void Equality_SameErrorDifferentMetadataInstances_ShouldNotBeEqual()
    {
        // Record equality uses reference equality for ImmutableDictionary,
        // so two errors with independently-built metadata won't be equal.
        var e1 = Error.Failure("test").WithMetadata("key", "value");
        var e2 = Error.Failure("test").WithMetadata("key", "value");

        Assert.NotEqual(e1, e2);
    }

    [Fact]
    public void Equality_SameMetadataInstance_ShouldBeEqual()
    {
        var e1 = Error.Failure("test");
        var e2 = Error.Failure("test");

        // Without metadata, the default empty ImmutableDictionary is shared.
        Assert.Equal(e1, e2);
    }
}
