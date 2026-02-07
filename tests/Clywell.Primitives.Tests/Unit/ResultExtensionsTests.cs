namespace Clywell.Primitives.Tests;

/// <summary>
/// Tests for <see cref="ResultExtensions"/> and <see cref="Result"/> factory methods.
/// </summary>
public class ResultExtensionsTests
{
    // ============================================================
    // Ensure Tests
    // ============================================================

    [Fact]
    public void Ensure_PredicateTrue_ShouldReturnOriginalResult()
    {
        var result = Result.Success(42)
            .Ensure(v => v > 0, Error.Failure("Must be positive"));

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Ensure_PredicateFalse_ShouldReturnFailure()
    {
        var result = Result.Success(-1)
            .Ensure(v => v > 0, Error.Failure("Must be positive"));

        Assert.True(result.IsFailure);
        Assert.Equal("Must be positive", result.Error.Description);
    }

    [Fact]
    public void Ensure_OnFailure_ShouldPropagateOriginalError()
    {
        var original = Error.NotFound("Missing");
        var result = Result.Failure<int>(original)
            .Ensure(v => v > 0, Error.Failure("Must be positive"));

        Assert.True(result.IsFailure);
        Assert.Equal(original, result.Error);
    }

    [Fact]
    public void Ensure_WithErrorFactory_PredicateFalse_ShouldUseFactory()
    {
        var result = Result.Success(-5)
            .Ensure(v => v > 0, v => Error.Failure($"Value {v} must be positive"));

        Assert.True(result.IsFailure);
        Assert.Contains("-5", result.Error.Description);
    }

    [Fact]
    public void Ensure_ChainedMultiple_AllPass_ShouldSucceed()
    {
        var result = Result.Success(42)
            .Ensure(v => v > 0, Error.Failure("Must be positive"))
            .Ensure(v => v < 100, Error.Failure("Must be less than 100"))
            .Ensure(v => v % 2 == 0, Error.Failure("Must be even"));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Ensure_ChainedMultiple_FirstFails_ShouldShortCircuit()
    {
        var result = Result.Success(-1)
            .Ensure(v => v > 0, Error.Failure("Must be positive"))
            .Ensure(v => v < 100, Error.Failure("Must be less than 100"));

        Assert.True(result.IsFailure);
        Assert.Equal("Must be positive", result.Error.Description);
    }

    // ============================================================
    // MapError Tests
    // ============================================================

    [Fact]
    public void MapError_OnFailure_ShouldTransformError()
    {
        var result = Result.Failure<int>(Error.Failure("Original"))
            .MapError(e => Error.NotFound($"Mapped: {e.Description}"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCode.NotFound, result.Error.Code);
        Assert.Contains("Mapped: Original", result.Error.Description);
    }

    [Fact]
    public void MapError_OnSuccess_ShouldReturnSameResult()
    {
        var result = Result.Success(42)
            .MapError(e => Error.NotFound("Should not reach"));

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    // ============================================================
    // TapError Tests
    // ============================================================

    [Fact]
    public void TapError_OnFailure_ShouldExecuteAction()
    {
        string? captured = null;
        var result = Result.Failure<int>(Error.NotFound("Missing"))
            .TapError(e => captured = e.Description);

        Assert.Equal("Missing", captured);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void TapError_OnSuccess_ShouldNotExecuteAction()
    {
        string? captured = null;
        var result = Result.Success(42)
            .TapError(e => captured = e.Description);

        Assert.Null(captured);
        Assert.True(result.IsSuccess);
    }

    // ============================================================
    // Async Extension Tests
    // ============================================================

    [Fact]
    public async Task AsyncMap_OnSuccess_ShouldTransform()
    {
        var resultTask = Task.FromResult(Result.Success(42));

        var mapped = await resultTask.Map(v => v.ToString());

        Assert.True(mapped.IsSuccess);
        Assert.Equal("42", mapped.Value);
    }

    [Fact]
    public async Task AsyncBind_OnSuccess_ShouldChain()
    {
        var resultTask = Task.FromResult(Result.Success(42));

        var bound = await resultTask.Bind(v => Result.Success(v * 2));

        Assert.True(bound.IsSuccess);
        Assert.Equal(84, bound.Value);
    }

    [Fact]
    public async Task AsyncBindAsync_OnSuccess_ShouldChainAsync()
    {
        var resultTask = Task.FromResult(Result.Success(42));

        var bound = await resultTask.BindAsync(async v =>
        {
            await Task.Delay(1);
            return Result.Success(v * 2);
        });

        Assert.True(bound.IsSuccess);
        Assert.Equal(84, bound.Value);
    }

    [Fact]
    public async Task AsyncMatch_OnSuccess_ShouldCallSuccessFunc()
    {
        var resultTask = Task.FromResult(Result.Success(42));

        var output = await resultTask.Match(
            v => $"Got: {v}",
            e => $"Error: {e.Description}");

        Assert.Equal("Got: 42", output);
    }

    [Fact]
    public async Task AsyncEnsure_OnSuccess_PredicateTrue_ShouldPass()
    {
        var resultTask = Task.FromResult(Result.Success(42));

        var ensured = await resultTask.Ensure(v => v > 0, Error.Failure("Must be positive"));

        Assert.True(ensured.IsSuccess);
    }

    [Fact]
    public async Task AsyncTap_OnSuccess_ShouldExecuteAction()
    {
        int captured = 0;
        var resultTask = Task.FromResult(Result.Success(42));

        var tapped = await resultTask.Tap(v => captured = v);

        Assert.Equal(42, captured);
        Assert.True(tapped.IsSuccess);
    }

    [Fact]
    public async Task AsyncTapError_OnFailure_ShouldExecuteAction()
    {
        string? captured = null;
        var resultTask = Task.FromResult(Result.Failure<int>(Error.NotFound("Missing")));

        var tapped = await resultTask.TapError(e => captured = e.Description);

        Assert.Equal("Missing", captured);
        Assert.True(tapped.IsFailure);
    }

    // ============================================================
    // Async Full Pipeline Tests
    // ============================================================

    [Fact]
    public async Task AsyncPipeline_ShouldChainCorrectly()
    {
        var log = new List<string>();

        var result = await Task.FromResult(Result.Success("42"))
            .Tap(v => log.Add($"Raw: {v}"))
            .Map(int.Parse)
            .Tap(v => log.Add($"Parsed: {v}"))
            .Bind(v => v > 0
                ? Result.Success(v * 10)
                : Result.Failure<int>("Must be positive"))
            .Tap(v => log.Add($"Final: {v}"));

        Assert.True(result.IsSuccess);
        Assert.Equal(420, result.Value);
        Assert.Equal(3, log.Count);
    }

    // ============================================================
    // Collect Tests
    // ============================================================

    [Fact]
    public void Collect_AllSuccess_ShouldReturnAllValues()
    {
        var results = new[]
        {
            Result.Success(1),
            Result.Success(2),
            Result.Success(3)
        };

        var collected = results.Collect();

        Assert.True(collected.IsSuccess);
        Assert.Equal(3, collected.Value.Count);
        Assert.Equal([1, 2, 3], collected.Value);
    }

    [Fact]
    public void Collect_WithFailure_ShouldReturnFirstError()
    {
        var results = new[]
        {
            Result.Success(1),
            Result.Failure<int>(Error.NotFound("Second failed")),
            Result.Success(3)
        };

        var collected = results.Collect();

        Assert.True(collected.IsFailure);
        Assert.Equal("Second failed", collected.Error.Description);
    }

    [Fact]
    public void Collect_Empty_ShouldReturnEmptyList()
    {
        var results = Array.Empty<Result<int>>();

        var collected = results.Collect();

        Assert.True(collected.IsSuccess);
        Assert.Empty(collected.Value);
    }

    // ============================================================
    // Result.Try Tests
    // ============================================================

    [Fact]
    public void Try_SuccessfulFunction_ShouldReturnSuccess()
    {
        var result = Result.Try(() => int.Parse("42"));

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Try_ThrowingFunction_ShouldReturnFailure()
    {
        var result = Result.Try(() => int.Parse("not-a-number"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCode.Unexpected, result.Error.Code);
        Assert.True(result.Error.Metadata.ContainsKey("ExceptionType"));
    }

    [Fact]
    public async Task TryAsync_SuccessfulFunction_ShouldReturnSuccess()
    {
        var result = await Result.TryAsync(async () =>
        {
            await Task.Delay(1);
            return 42;
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task TryAsync_ThrowingFunction_ShouldReturnFailure()
    {
        var result = await Result.TryAsync<int>(async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Boom!");
        });

        Assert.True(result.IsFailure);
        Assert.Contains("Boom!", result.Error.Description);
    }

    // ============================================================
    // Result.FromNullable Tests
    // ============================================================

    [Fact]
    public void FromNullable_ReferenceType_NotNull_ShouldReturnSuccess()
    {
        string? value = "hello";
        var result = Result.FromNullable(value);

        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void FromNullable_ReferenceType_Null_ShouldReturnFailure()
    {
        string? value = null;
        var result = Result.FromNullable(value, "Value was missing");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCode.NotFound, result.Error.Code);
        Assert.Equal("Value was missing", result.Error.Description);
    }

    [Fact]
    public void FromNullable_ValueType_HasValue_ShouldReturnSuccess()
    {
        int? value = 42;
        var result = Result.FromNullable(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void FromNullable_ValueType_Null_ShouldReturnFailure()
    {
        int? value = null;
        var result = Result.FromNullable(value);

        Assert.True(result.IsFailure);
    }

    // ============================================================
    // Result.Combine Tests
    // ============================================================

    [Fact]
    public void Combine2_AllSuccess_ShouldReturnTuple()
    {
        var r1 = Result.Success(1);
        var r2 = Result.Success("two");

        var combined = Result.Combine(r1, r2);

        Assert.True(combined.IsSuccess);
        Assert.Equal(1, combined.Value.Item1);
        Assert.Equal("two", combined.Value.Item2);
    }

    [Fact]
    public void Combine2_FirstFails_ShouldReturnFirstError()
    {
        var r1 = Result.Failure<int>(Error.NotFound("First"));
        var r2 = Result.Success("two");

        var combined = Result.Combine(r1, r2);

        Assert.True(combined.IsFailure);
        Assert.Equal("First", combined.Error.Description);
    }

    [Fact]
    public void Combine2_SecondFails_ShouldReturnSecondError()
    {
        var r1 = Result.Success(1);
        var r2 = Result.Failure<string>(Error.NotFound("Second"));

        var combined = Result.Combine(r1, r2);

        Assert.True(combined.IsFailure);
        Assert.Equal("Second", combined.Error.Description);
    }

    [Fact]
    public void Combine3_AllSuccess_ShouldReturnTuple()
    {
        var r1 = Result.Success(1);
        var r2 = Result.Success("two");
        var r3 = Result.Success(3.0);

        var combined = Result.Combine(r1, r2, r3);

        Assert.True(combined.IsSuccess);
        Assert.Equal(1, combined.Value.Item1);
        Assert.Equal("two", combined.Value.Item2);
        Assert.Equal(3.0, combined.Value.Item3);
    }

    [Fact]
    public void Combine3_MiddleFails_ShouldReturnError()
    {
        var r1 = Result.Success(1);
        var r2 = Result.Failure<string>(Error.Conflict("Dup"));
        var r3 = Result.Success(3.0);

        var combined = Result.Combine(r1, r2, r3);

        Assert.True(combined.IsFailure);
        Assert.Equal("Dup", combined.Error.Description);
    }

    // ============================================================
    // Non-Generic Result Ensure Tests
    // ============================================================

    [Fact]
    public void Ensure_NonGeneric_PredicateTrue_ShouldReturnSuccess()
    {
        var result = Result.Success().Ensure(() => true, Error.Failure("Bad"));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Ensure_NonGeneric_PredicateFalse_ShouldReturnFailure()
    {
        var result = Result.Success().Ensure(() => false, Error.Failure("Bad"));

        Assert.True(result.IsFailure);
        Assert.Equal("Bad", result.Error.Description);
    }

    [Fact]
    public void Ensure_NonGeneric_AlreadyFailed_ShouldPropagateOriginalError()
    {
        var original = Error.NotFound("Missing");
        var result = Result.Failure(original).Ensure(() => true, Error.Failure("Bad"));

        Assert.True(result.IsFailure);
        Assert.Equal(original, result.Error);
    }

    // ============================================================
    // Ensure with Lazy Error Factory (Generic) Tests
    // ============================================================

    [Fact]
    public void Ensure_LazyFactory_PredicateTrue_ShouldReturnOriginal()
    {
        var result = Result.Success(42)
            .Ensure(v => v > 0, v => Error.Failure($"{v} is negative"));

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Ensure_LazyFactory_PredicateFalse_ShouldUseFactoryError()
    {
        var result = Result.Success(-5)
            .Ensure(v => v > 0, v => Error.Failure($"{v} is negative"));

        Assert.True(result.IsFailure);
        Assert.Equal("-5 is negative", result.Error.Description);
    }

    // ============================================================
    // Combine 4 & 5 Tests
    // ============================================================

    [Fact]
    public void Combine4_AllSuccess_ShouldReturnTupleOfValues()
    {
        var combined = Result.Combine(
            Result.Success(1),
            Result.Success("two"),
            Result.Success(3.0),
            Result.Success(true));

        Assert.True(combined.IsSuccess);
        Assert.Equal((1, "two", 3.0, true), combined.Value);
    }

    [Fact]
    public void Combine4_OneFailure_ShouldReturnFirstError()
    {
        var error = Error.NotFound("Missing");
        var combined = Result.Combine(
            Result.Success(1),
            Result.Success("two"),
            Result.Failure<double>(error),
            Result.Success(true));

        Assert.True(combined.IsFailure);
        Assert.Equal(error, combined.Error);
    }

    [Fact]
    public void Combine5_AllSuccess_ShouldReturnTupleOfValues()
    {
        var combined = Result.Combine(
            Result.Success(1),
            Result.Success("two"),
            Result.Success(3.0),
            Result.Success(true),
            Result.Success('X'));

        Assert.True(combined.IsSuccess);
        Assert.Equal((1, "two", 3.0, true, 'X'), combined.Value);
    }

    [Fact]
    public void Combine5_OneFailure_ShouldReturnFirstError()
    {
        var error = Error.Conflict("Dup");
        var combined = Result.Combine(
            Result.Success(1),
            Result.Success("two"),
            Result.Success(3.0),
            Result.Success(true),
            Result.Failure<char>(error));

        Assert.True(combined.IsFailure);
        Assert.Equal(error, combined.Error);
    }

    // ============================================================
    // Async Pipeline: Task<Result> — Ensure, Map, Bind Tests
    // ============================================================

    [Fact]
    public async Task TaskResult_Ensure_PredicateTrue_ShouldReturnSuccess()
    {
        var result = await Task.FromResult(Result.Success())
            .Ensure(() => true, Error.Failure("Bad"));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task TaskResult_Ensure_PredicateFalse_ShouldReturnFailure()
    {
        var result = await Task.FromResult(Result.Success())
            .Ensure(() => false, Error.Failure("Bad"));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task TaskResult_Map_Success_ShouldProduceValueResult()
    {
        var result = await Task.FromResult(Result.Success())
            .Map(() => 42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task TaskResult_Map_Failure_ShouldPropagateError()
    {
        var error = Error.NotFound("Missing");
        var result = await Task.FromResult(Result.Failure(error))
            .Map(() => 42);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public async Task TaskResult_Bind_Success_ShouldCallBinder()
    {
        var result = await Task.FromResult(Result.Success())
            .Bind(() => Result.Success("hello"));

        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public async Task TaskResult_Bind_Failure_ShouldPropagateError()
    {
        var error = Error.Forbidden("Denied");
        var result = await Task.FromResult(Result.Failure(error))
            .Bind(() => Result.Success("hello"));

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    // ============================================================
    // Async Pipeline: Task<Result<T>> — Ensure with Lazy Factory
    // ============================================================

    [Fact]
    public async Task TaskResultT_Ensure_LazyFactory_PredicateTrue_ShouldReturnOriginal()
    {
        var result = await Task.FromResult(Result.Success(42))
            .Ensure(v => v > 0, v => Error.Failure($"{v} is negative"));

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public async Task TaskResultT_Ensure_LazyFactory_PredicateFalse_ShouldUseFactory()
    {
        var result = await Task.FromResult(Result.Success(-5))
            .Ensure(v => v > 0, v => Error.Failure($"{v} is negative"));

        Assert.True(result.IsFailure);
        Assert.Equal("-5 is negative", result.Error.Description);
    }
}
