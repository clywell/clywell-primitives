namespace Clywell.Primitives.Tests;

/// <summary>
/// Tests for non-generic <see cref="Result"/> functionality.
/// </summary>
public class NonGenericResultTests
{
    // ============================================================
    // Success / Failure Creation Tests
    // ============================================================

    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Failure_WithError_ShouldCreateFailedResult()
    {
        var error = Error.NotFound("Not found");
        var result = Result.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Failure_WithString_ShouldCreateFailedResult()
    {
        var result = Result.Failure("Something went wrong");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCode.Failure, result.Error.Code);
        Assert.Equal("Something went wrong", result.Error.Description);
    }

    [Fact]
    public void Error_OnSuccess_ShouldThrowInvalidOperationException()
    {
        var result = Result.Success();

        Assert.Throws<InvalidOperationException>(() => result.Error);
    }

    // ============================================================
    // Implicit Conversion Tests
    // ============================================================

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailure()
    {
        Result result = Error.Conflict("Duplicate");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCode.Conflict, result.Error.Code);
    }

    // ============================================================
    // Match Tests
    // ============================================================

    [Fact]
    public void Match_OnSuccess_ShouldCallSuccessFunc()
    {
        var result = Result.Success();

        var output = result.Match(
            onSuccess: () => "ok",
            onFailure: e => $"fail: {e.Description}");

        Assert.Equal("ok", output);
    }

    [Fact]
    public void Match_OnFailure_ShouldCallFailureFunc()
    {
        var result = Result.Failure(Error.NotFound("Missing"));

        var output = result.Match(
            onSuccess: () => "ok",
            onFailure: e => $"fail: {e.Description}");

        Assert.Equal("fail: Missing", output);
    }

    // ============================================================
    // Switch Tests
    // ============================================================

    [Fact]
    public void Switch_OnSuccess_ShouldCallSuccessAction()
    {
        var result = Result.Success();
        bool called = false;

        result.Switch(
            onSuccess: () => called = true,
            onFailure: _ => { });

        Assert.True(called);
    }

    [Fact]
    public void Switch_OnFailure_ShouldCallFailureAction()
    {
        var result = Result.Failure("err");
        Error? captured = null;

        result.Switch(
            onSuccess: () => { },
            onFailure: e => captured = e);

        Assert.NotNull(captured);
    }

    // ============================================================
    // Tap / OnSuccess / OnFailure Tests
    // ============================================================

    [Fact]
    public void Tap_OnSuccess_ShouldExecuteAction()
    {
        var result = Result.Success();
        bool tapped = false;

        var returned = result.Tap(() => tapped = true);

        Assert.True(tapped);
        Assert.True(returned.IsSuccess);
    }

    [Fact]
    public void Tap_OnFailure_ShouldNotExecuteAction()
    {
        var result = Result.Failure("err");
        bool tapped = false;

        result.Tap(() => tapped = true);

        Assert.False(tapped);
    }

    [Fact]
    public void OnSuccess_ShouldExecuteOnSuccess()
    {
        var result = Result.Success();
        bool called = false;

        result.OnSuccess(() => called = true);

        Assert.True(called);
    }

    [Fact]
    public void OnSuccess_ShouldNotExecuteOnFailure()
    {
        var result = Result.Failure("err");
        bool called = false;

        result.OnSuccess(() => called = true);

        Assert.False(called);
    }

    [Fact]
    public void OnFailure_ShouldExecuteOnFailure()
    {
        var result = Result.Failure("err");
        bool called = false;

        result.OnFailure(_ => called = true);

        Assert.True(called);
    }

    [Fact]
    public void OnFailure_ShouldNotExecuteOnSuccess()
    {
        var result = Result.Success();
        bool called = false;

        result.OnFailure(_ => called = true);

        Assert.False(called);
    }

    // ============================================================
    // Try / TryAsync Tests
    // ============================================================

    [Fact]
    public void Try_SuccessfulAction_ShouldReturnSuccess()
    {
        var result = Result.Try(() => { /* no-op */ });

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Try_ThrowingAction_ShouldReturnFailure()
    {
        var result = Result.Try(() => throw new InvalidOperationException("boom"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCode.Unexpected, result.Error.Code);
        Assert.Equal("boom", result.Error.Description);
    }

    [Fact]
    public async Task TryAsync_SuccessfulAction_ShouldReturnSuccess()
    {
        var result = await Result.TryAsync(async () =>
        {
            await Task.CompletedTask;
        });

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task TryAsync_ThrowingAction_ShouldReturnFailure()
    {
        var result = await Result.TryAsync(async () =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("async boom");
        });

        Assert.True(result.IsFailure);
        Assert.Equal("async boom", result.Error.Description);
    }

    // ============================================================
    // MatchAsync Tests
    // ============================================================

    [Fact]
    public async Task MatchAsync_OnSuccess_ShouldCallSuccessFunc()
    {
        var result = Result.Success();

        var output = await result.MatchAsync(
            onSuccess: () => Task.FromResult("ok"),
            onFailure: e => Task.FromResult($"fail: {e.Description}"));

        Assert.Equal("ok", output);
    }

    [Fact]
    public async Task MatchAsync_OnFailure_ShouldCallFailureFunc()
    {
        var result = Result.Failure(Error.NotFound("Missing"));

        var output = await result.MatchAsync(
            onSuccess: () => Task.FromResult("ok"),
            onFailure: e => Task.FromResult($"fail: {e.Description}"));

        Assert.Equal("fail: Missing", output);
    }

    // ============================================================
    // Extension Methods Tests
    // ============================================================

    [Fact]
    public void MapError_OnFailure_ShouldTransformError()
    {
        var result = Result.Failure(Error.NotFound("original"));

        var mapped = result.MapError(e => Error.Unexpected($"Wrapped: {e.Description}"));

        Assert.True(mapped.IsFailure);
        Assert.Equal(ErrorCode.Unexpected, mapped.Error.Code);
        Assert.Contains("Wrapped: original", mapped.Error.Description);
    }

    [Fact]
    public void MapError_OnSuccess_ShouldReturnUnchanged()
    {
        var result = Result.Success();

        var mapped = result.MapError(e => Error.Unexpected("should not happen"));

        Assert.True(mapped.IsSuccess);
    }

    [Fact]
    public void TapError_OnFailure_ShouldExecuteAction()
    {
        var result = Result.Failure(Error.NotFound("err"));
        Error? captured = null;

        result.TapError(e => captured = e);

        Assert.NotNull(captured);
        Assert.Equal("err", captured.Description);
    }

    [Fact]
    public void TapError_OnSuccess_ShouldNotExecuteAction()
    {
        var result = Result.Success();
        bool called = false;

        result.TapError(_ => called = true);

        Assert.False(called);
    }

    // ============================================================
    // Async Extension Methods Tests
    // ============================================================

    [Fact]
    public async Task AsyncMatch_OnSuccess_ShouldWork()
    {
        var resultTask = Task.FromResult(Result.Success());

        var output = await resultTask.Match(
            onSuccess: () => "ok",
            onFailure: e => "fail");

        Assert.Equal("ok", output);
    }

    [Fact]
    public async Task AsyncTap_OnSuccess_ShouldWork()
    {
        var resultTask = Task.FromResult(Result.Success());
        bool tapped = false;

        var returned = await resultTask.Tap(() => tapped = true);

        Assert.True(tapped);
        Assert.True(returned.IsSuccess);
    }

    [Fact]
    public async Task AsyncTapError_OnFailure_ShouldWork()
    {
        var resultTask = Task.FromResult(Result.Failure("err"));
        bool called = false;

        var returned = await resultTask.TapError(_ => called = true);

        Assert.True(called);
        Assert.True(returned.IsFailure);
    }

    // ============================================================
    // Equality Tests
    // ============================================================

    [Fact]
    public void Equality_TwoSuccesses_ShouldBeEqual()
    {
        var a = Result.Success();
        var b = Result.Success();

        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Equality_TwoFailuresWithSameError_ShouldBeEqual()
    {
        var a = Result.Failure(Error.NotFound("x"));
        var b = Result.Failure(Error.NotFound("x"));

        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Equality_SuccessVsFailure_ShouldNotBeEqual()
    {
        var a = Result.Success();
        var b = Result.Failure("err");

        Assert.NotEqual(a, b);
        Assert.True(a != b);
    }

    [Fact]
    public void Equality_TwoFailuresWithDifferentErrors_ShouldNotBeEqual()
    {
        var a = Result.Failure(Error.NotFound("x"));
        var b = Result.Failure(Error.Conflict("y"));

        Assert.NotEqual(a, b);
    }

    // ============================================================
    // ToString Tests
    // ============================================================

    [Fact]
    public void ToString_Success_ShouldReturnSuccess()
    {
        var result = Result.Success();

        Assert.Equal("Success", result.ToString());
    }

    [Fact]
    public void ToString_Failure_ShouldContainError()
    {
        var result = Result.Failure(Error.NotFound("Missing"));

        Assert.Contains("Failure", result.ToString());
        Assert.Contains("Missing", result.ToString());
    }

    // ============================================================
    // Map (Bridging to Result<T>) Tests
    // ============================================================

    [Fact]
    public void Map_Success_ShouldProduceValueResult()
    {
        var result = Result.Success().Map(() => 42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Map_Failure_ShouldPropagateError()
    {
        var error = Error.NotFound("Missing");
        var result = Result.Failure(error).Map(() => 42);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    // ============================================================
    // Bind (Bridging to Result<T>) Tests
    // ============================================================

    [Fact]
    public void Bind_Success_ShouldCallBinder()
    {
        var result = Result.Success().Bind(() => Result.Success("hello"));

        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void Bind_Failure_ShouldPropagateError()
    {
        var error = Error.Forbidden("Denied");
        var result = Result.Failure(error).Bind(() => Result.Success("hello"));

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    // ============================================================
    // Deconstruct Tests
    // ============================================================

    [Fact]
    public void Deconstruct_Success_ShouldReturnTrueAndNullError()
    {
        var result = Result.Success();

        var (isSuccess, error) = result;

        Assert.True(isSuccess);
        Assert.Null(error);
    }

    [Fact]
    public void Deconstruct_Failure_ShouldReturnFalseAndError()
    {
        var expected = Error.NotFound("Missing");
        var result = Result.Failure(expected);

        var (isSuccess, error) = result;

        Assert.False(isSuccess);
        Assert.Equal(expected, error);
    }

    // ============================================================
    // TapAsync / TapErrorAsync Tests
    // ============================================================

    [Fact]
    public async Task TapAsync_Success_ShouldExecuteAsyncAction()
    {
        var executed = false;
        var result = await Result.Success().TapAsync(async () =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.True(executed);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task TapAsync_Failure_ShouldNotExecuteAsyncAction()
    {
        var executed = false;
        var result = await Result.Failure("fail").TapAsync(async () =>
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
        var error = Error.NotFound("Missing");
        var result = await Result.Failure(error).TapErrorAsync(async e =>
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
        var result = await Result.Success().TapErrorAsync(async _ =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.False(executed);
        Assert.True(result.IsSuccess);
    }
}
