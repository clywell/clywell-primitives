namespace Clywell.Primitives.Tests;

/// <summary>
/// Tests for <see cref="Result{TValue}"/> core functionality.
/// </summary>
public class ResultTests
{
    // ============================================================
    // Success Creation Tests
    // ============================================================

    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Success_StringValue_ShouldWork()
    {
        var result = Result.Success("hello");

        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void Success_ComplexType_ShouldWork()
    {
        var obj = new TestPerson("John", 30);
        var result = Result.Success(obj);

        Assert.True(result.IsSuccess);
        Assert.Equal("John", result.Value.Name);
        Assert.Equal(30, result.Value.Age);
    }

    // ============================================================
    // Failure Creation Tests
    // ============================================================

    [Fact]
    public void Failure_WithError_ShouldCreateFailedResult()
    {
        var error = Error.NotFound("Not found");
        var result = Result.Failure<int>(error);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Failure_WithString_ShouldCreateFailedResult()
    {
        var result = Result.Failure<int>("Something went wrong");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCode.Failure, result.Error.Code);
        Assert.Equal("Something went wrong", result.Error.Description);
    }

    // ============================================================
    // Value/Error Access Tests
    // ============================================================

    [Fact]
    public void Value_OnFailure_ShouldThrowInvalidOperationException()
    {
        var result = Result.Failure<int>(Error.NotFound("Missing"));

        var ex = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Contains("Cannot access Value", ex.Message);
    }

    [Fact]
    public void Error_OnSuccess_ShouldThrowInvalidOperationException()
    {
        var result = Result.Success(42);

        var ex = Assert.Throws<InvalidOperationException>(() => result.Error);
        Assert.Contains("Cannot access Error", ex.Message);
    }

    // ============================================================
    // Implicit Conversion Tests
    // ============================================================

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccess()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailure()
    {
        var error = Error.Conflict("Duplicate");
        Result<string> result = error;

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void ImplicitConversion_InMethodReturn_ShouldWork()
    {
        var success = GetResult(true);
        var failure = GetResult(false);

        Assert.True(success.IsSuccess);
        Assert.True(failure.IsFailure);
    }

    // ============================================================
    // Match Tests
    // ============================================================

    [Fact]
    public void Match_OnSuccess_ShouldCallSuccessFunction()
    {
        var result = Result.Success(42);

        var output = result.Match(
            onSuccess: v => $"Got: {v}",
            onFailure: e => $"Error: {e.Description}");

        Assert.Equal("Got: 42", output);
    }

    [Fact]
    public void Match_OnFailure_ShouldCallFailureFunction()
    {
        var result = Result.Failure<int>(Error.NotFound("Missing"));

        var output = result.Match(
            onSuccess: v => $"Got: {v}",
            onFailure: e => $"Error: {e.Description}");

        Assert.Equal("Error: Missing", output);
    }

    // ============================================================
    // Switch Tests
    // ============================================================

    [Fact]
    public void Switch_OnSuccess_ShouldCallSuccessAction()
    {
        var result = Result.Success(42);
        int captured = 0;

        result.Switch(
            onSuccess: v => captured = v,
            onFailure: _ => captured = -1);

        Assert.Equal(42, captured);
    }

    [Fact]
    public void Switch_OnFailure_ShouldCallFailureAction()
    {
        var result = Result.Failure<int>(Error.NotFound("Missing"));
        string? capturedCode = null;

        result.Switch(
            onSuccess: _ => { },
            onFailure: e => capturedCode = e.Code.Value);

        Assert.Equal("General.NotFound", capturedCode);
    }

    // ============================================================
    // Map Tests
    // ============================================================

    [Fact]
    public void Map_OnSuccess_ShouldTransformValue()
    {
        var result = Result.Success(42);

        var mapped = result.Map(v => v.ToString());

        Assert.True(mapped.IsSuccess);
        Assert.Equal("42", mapped.Value);
    }

    [Fact]
    public void Map_OnFailure_ShouldPropagateError()
    {
        var error = Error.NotFound("Missing");
        var result = Result.Failure<int>(error);

        var mapped = result.Map(v => v.ToString());

        Assert.True(mapped.IsFailure);
        Assert.Equal(error, mapped.Error);
    }

    // ============================================================
    // Bind Tests
    // ============================================================

    [Fact]
    public void Bind_OnSuccess_ShouldChainResults()
    {
        var result = Result.Success(42);

        var bound = result.Bind(v =>
            v > 0 ? Result.Success(v * 2) : Result.Failure<int>("Negative"));

        Assert.True(bound.IsSuccess);
        Assert.Equal(84, bound.Value);
    }

    [Fact]
    public void Bind_OnSuccess_WhenBinderFails_ShouldReturnFailure()
    {
        var result = Result.Success(-1);

        var bound = result.Bind(v =>
            v > 0 ? Result.Success(v * 2) : Result.Failure<int>("Negative"));

        Assert.True(bound.IsFailure);
    }

    [Fact]
    public void Bind_OnFailure_ShouldPropagateError()
    {
        var error = Error.NotFound("Missing");
        var result = Result.Failure<int>(error);

        var bound = result.Bind(v => Result.Success(v * 2));

        Assert.True(bound.IsFailure);
        Assert.Equal(error, bound.Error);
    }

    // ============================================================
    // Tap / OnSuccess / OnFailure Tests
    // ============================================================

    [Fact]
    public void Tap_OnSuccess_ShouldExecuteAction()
    {
        int captured = 0;
        var result = Result.Success(42).Tap(v => captured = v);

        Assert.Equal(42, captured);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Tap_OnFailure_ShouldNotExecuteAction()
    {
        int captured = 0;
        var result = Result.Failure<int>(Error.Failure("Oops")).Tap(v => captured = v);

        Assert.Equal(0, captured);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void OnSuccess_ShouldExecuteOnSuccess()
    {
        int captured = 0;
        Result.Success(42).OnSuccess(v => captured = v);
        Assert.Equal(42, captured);
    }

    [Fact]
    public void OnSuccess_ShouldNotExecuteOnFailure()
    {
        int captured = 0;
        Result.Failure<int>(Error.Failure("Oops")).OnSuccess(v => captured = v);
        Assert.Equal(0, captured);
    }

    [Fact]
    public void OnFailure_ShouldExecuteOnFailure()
    {
        string? capturedCode = null;
        Result.Failure<int>(Error.NotFound("Missing"))
            .OnFailure(e => capturedCode = e.Code.Value);
        Assert.Equal("General.NotFound", capturedCode);
    }

    [Fact]
    public void OnFailure_ShouldNotExecuteOnSuccess()
    {
        string? capturedCode = null;
        Result.Success(42).OnFailure(e => capturedCode = e.Code.Value);
        Assert.Null(capturedCode);
    }

    // ============================================================
    // ValueOr Tests
    // ============================================================

    [Fact]
    public void ValueOr_OnSuccess_ShouldReturnValue()
    {
        var result = Result.Success(42);
        Assert.Equal(42, result.ValueOr(0));
    }

    [Fact]
    public void ValueOr_OnFailure_ShouldReturnFallback()
    {
        var result = Result.Failure<int>(Error.Failure("Oops"));
        Assert.Equal(-1, result.ValueOr(-1));
    }

    [Fact]
    public void ValueOr_WithFactory_OnSuccess_ShouldReturnValue()
    {
        var result = Result.Success(42);
        Assert.Equal(42, result.ValueOr(_ => 0));
    }

    [Fact]
    public void ValueOr_WithFactory_OnFailure_ShouldCallFactory()
    {
        var result = Result.Failure<int>(Error.Failure("Oops"));
        Assert.Equal(-1, result.ValueOr(_ => -1));
    }

    // ============================================================
    // Async Tests
    // ============================================================

    [Fact]
    public async Task MapAsync_OnSuccess_ShouldTransformAsync()
    {
        var result = Result.Success(42);

        var mapped = await result.MapAsync(async v =>
        {
            await Task.Delay(1);
            return v.ToString();
        });

        Assert.True(mapped.IsSuccess);
        Assert.Equal("42", mapped.Value);
    }

    [Fact]
    public async Task MapAsync_OnFailure_ShouldPropagateError()
    {
        var error = Error.Failure("Oops");
        var result = Result.Failure<int>(error);

        var mapped = await result.MapAsync(async v =>
        {
            await Task.Delay(1);
            return v.ToString();
        });

        Assert.True(mapped.IsFailure);
        Assert.Equal(error, mapped.Error);
    }

    [Fact]
    public async Task BindAsync_OnSuccess_ShouldChain()
    {
        var result = Result.Success(42);

        var bound = await result.BindAsync(async v =>
        {
            await Task.Delay(1);
            return Result.Success(v * 2);
        });

        Assert.True(bound.IsSuccess);
        Assert.Equal(84, bound.Value);
    }

    [Fact]
    public async Task BindAsync_OnFailure_ShouldPropagateError()
    {
        var error = Error.Failure("Oops");
        var result = Result.Failure<int>(error);

        var bound = await result.BindAsync(async v =>
        {
            await Task.Delay(1);
            return Result.Success(v * 2);
        });

        Assert.True(bound.IsFailure);
    }

    [Fact]
    public async Task MatchAsync_OnSuccess_ShouldCallSuccessFunc()
    {
        var result = Result.Success(42);

        var output = await result.MatchAsync(
            async v => { await Task.Delay(1); return $"Got: {v}"; },
            async e => { await Task.Delay(1); return $"Error: {e.Description}"; });

        Assert.Equal("Got: 42", output);
    }

    [Fact]
    public async Task MatchAsync_OnFailure_ShouldCallFailureFunc()
    {
        var result = Result.Failure<int>(Error.NotFound("Missing"));

        var output = await result.MatchAsync(
            async v => { await Task.Delay(1); return $"Got: {v}"; },
            async e => { await Task.Delay(1); return $"Error: {e.Description}"; });

        Assert.Equal("Error: Missing", output);
    }

    // ============================================================
    // Equality Tests
    // ============================================================

    [Fact]
    public void Equality_SameSuccessValue_ShouldBeEqual()
    {
        var r1 = Result.Success(42);
        var r2 = Result.Success(42);

        Assert.Equal(r1, r2);
        Assert.True(r1 == r2);
    }

    [Fact]
    public void Equality_DifferentSuccessValue_ShouldNotBeEqual()
    {
        var r1 = Result.Success(42);
        var r2 = Result.Success(99);

        Assert.NotEqual(r1, r2);
        Assert.True(r1 != r2);
    }

    [Fact]
    public void Equality_SameError_ShouldBeEqual()
    {
        var error = Error.NotFound("Missing");
        var r1 = Result.Failure<int>(error);
        var r2 = Result.Failure<int>(error);

        Assert.Equal(r1, r2);
    }

    [Fact]
    public void Equality_SuccessVsFailure_ShouldNotBeEqual()
    {
        var success = Result.Success(42);
        var failure = Result.Failure<int>(Error.Failure("Oops"));

        Assert.NotEqual(success, failure);
    }

    [Fact]
    public void GetHashCode_SameSuccessValue_ShouldMatch()
    {
        var r1 = Result.Success(42);
        var r2 = Result.Success(42);

        Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
    }

    // ============================================================
    // ToString Tests
    // ============================================================

    [Fact]
    public void ToString_Success_ShouldFormat()
    {
        var result = Result.Success(42);
        Assert.Equal("Success(42)", result.ToString());
    }

    [Fact]
    public void ToString_Failure_ShouldFormat()
    {
        var result = Result.Failure<int>(Error.NotFound("Missing"));
        Assert.Contains("Failure(", result.ToString());
    }

    // ============================================================
    // Chaining / Pipeline Tests
    // ============================================================

    [Fact]
    public void FullPipeline_SuccessPath_ShouldChainCorrectly()
    {
        var log = new List<string>();

        var result = Result.Success("42")
            .Tap(v => log.Add($"Raw: {v}"))
            .Map(int.Parse)
            .Tap(v => log.Add($"Parsed: {v}"))
            .Bind(v => v > 0
                ? Result.Success(v * 10)
                : Result.Failure<int>("Must be positive"))
            .Tap(v => log.Add($"Multiplied: {v}"))
            .OnSuccess(v => log.Add($"Final: {v}"));

        Assert.True(result.IsSuccess);
        Assert.Equal(420, result.Value);
        Assert.Equal(4, log.Count);
    }

    [Fact]
    public void FullPipeline_FailurePath_ShouldShortCircuit()
    {
        var log = new List<string>();

        var result = Result.Success("not-a-number")
            .Bind(v => Result.Try(() => int.Parse(v)))
            .Tap(v => log.Add($"Parsed: {v}"))
            .Map(v => v * 10);

        Assert.True(result.IsFailure);
        Assert.Empty(log); // Tap should not execute after failure
    }

    // ============================================================
    // ToResult Tests
    // ============================================================

    [Fact]
    public void ToResult_Success_ShouldReturnNonGenericSuccess()
    {
        var result = Result.Success(42).ToResult();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ToResult_Failure_ShouldPreserveError()
    {
        var error = Error.NotFound("Missing");
        Result<int> failed = error;
        var result = failed.ToResult();

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    // ============================================================
    // Deconstruct Tests
    // ============================================================

    [Fact]
    public void Deconstruct_Success_ShouldReturnValueAndNullError()
    {
        Result<int> result = 42;

        var (isSuccess, value, error) = result;

        Assert.True(isSuccess);
        Assert.Equal(42, value);
        Assert.Null(error);
    }

    [Fact]
    public void Deconstruct_Failure_ShouldReturnDefaultValueAndError()
    {
        var expected = Error.NotFound("Missing");
        Result<int> result = expected;

        var (isSuccess, value, error) = result;

        Assert.False(isSuccess);
        Assert.Equal(0, value);
        Assert.Equal(expected, error);
    }

    // ============================================================
    // TapAsync / TapErrorAsync Tests
    // ============================================================

    [Fact]
    public async Task TapAsync_Success_ShouldExecuteAsyncAction()
    {
        int? captured = null;
        var result = await Result.Success(42).TapAsync(async v =>
        {
            await Task.Delay(1);
            captured = v;
        });

        Assert.Equal(42, captured);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task TapAsync_Failure_ShouldNotExecuteAsyncAction()
    {
        var executed = false;
        Result<int> failed = Error.Failure("fail");
        var result = await failed.TapAsync(async _ =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.False(executed);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task TapErrorAsync_Failure_ShouldExecuteAsyncAction()
    {
        Error? captured = null;
        var error = Error.Conflict("Dup");
        Result<int> failed = error;
        var result = await failed.TapErrorAsync(async e =>
        {
            await Task.Delay(1);
            captured = e;
        });

        Assert.Equal(error, captured);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task TapErrorAsync_Success_ShouldNotExecuteAsyncAction()
    {
        var executed = false;
        var result = await Result.Success(42).TapErrorAsync(async _ =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.False(executed);
        Assert.True(result.IsSuccess);
    }

    // ============================================================
    // Helpers
    // ============================================================

    private static Result<string> GetResult(bool succeed) =>
        succeed ? "success" : Error.Failure("failed");

    private record TestPerson(string Name, int Age);
}
