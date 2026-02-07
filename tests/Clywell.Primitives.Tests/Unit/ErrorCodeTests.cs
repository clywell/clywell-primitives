namespace Clywell.Primitives.Tests;

/// <summary>
/// Tests for <see cref="ErrorCode"/>.
/// </summary>
public class ErrorCodeTests
{
    [Fact]
    public void BuiltInCodes_ShouldHaveExpectedValues()
    {
        Assert.Equal("General.Failure", ErrorCode.Failure.Value);
        Assert.Equal("General.Validation", ErrorCode.Validation.Value);
        Assert.Equal("General.NotFound", ErrorCode.NotFound.Value);
        Assert.Equal("General.Conflict", ErrorCode.Conflict.Value);
        Assert.Equal("General.Unauthorized", ErrorCode.Unauthorized.Value);
        Assert.Equal("General.Forbidden", ErrorCode.Forbidden.Value);
        Assert.Equal("General.Unexpected", ErrorCode.Unexpected.Value);
        Assert.Equal("General.Unavailable", ErrorCode.Unavailable.Value);
    }

    [Fact]
    public void CustomCode_ShouldRetainValue()
    {
        var code = new ErrorCode("Custom.MyError");
        Assert.Equal("Custom.MyError", code.Value);
    }

    [Fact]
    public void ImplicitConversion_FromString_ShouldWork()
    {
        ErrorCode code = "My.Custom.Code";
        Assert.Equal("My.Custom.Code", code.Value);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        var code = new ErrorCode("Test.Code");
        string value = code;
        Assert.Equal("Test.Code", value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var code = new ErrorCode("Display.Test");
        Assert.Equal("Display.Test", code.ToString());
    }

    [Fact]
    public void Equality_SameValue_ShouldBeEqual()
    {
        var code1 = new ErrorCode("Same.Code");
        var code2 = new ErrorCode("Same.Code");
        Assert.Equal(code1, code2);
    }

    [Fact]
    public void Equality_DifferentValue_ShouldNotBeEqual()
    {
        var code1 = new ErrorCode("Code.A");
        var code2 = new ErrorCode("Code.B");
        Assert.NotEqual(code1, code2);
    }

    [Fact]
    public void GetHashCode_SameValue_ShouldMatch()
    {
        var code1 = new ErrorCode("Hash.Test");
        var code2 = new ErrorCode("Hash.Test");
        Assert.Equal(code1.GetHashCode(), code2.GetHashCode());
    }
}
