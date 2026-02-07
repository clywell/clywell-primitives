using System.Diagnostics;

namespace Clywell.Primitives;

/// <summary>
/// Represents the outcome of an operation that either succeeds (with no value)
/// or fails with an <see cref="Primitives.Error"/>.
/// Also provides static factory methods for creating <see cref="Result{TValue}"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="Result"/> for void-like operations (e.g., Delete, SendEmail)
/// where success carries no return value.
/// </para>
/// <para>
/// Use <see cref="Result{TValue}"/> when success carries a value.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Void-like operation
/// Result result = Result.Success();
/// Result failure = Result.Failure(Error.NotFound("Item not found"));
///
/// // Value operation
/// Result&lt;int&gt; success = Result.Success(42);
/// Result&lt;int&gt; fail = Result.Failure&lt;int&gt;(Error.NotFound("Item not found"));
/// </code>
/// </example>
[DebuggerDisplay("{IsSuccess ? \"Success\" : \"Failure(\" + _error + \")\"}")]
public readonly struct Result : IEquatable<Result>
{
    private readonly Error? _error;
    private readonly bool _isSuccess;

    private Result(bool isSuccess, Error? error)
    {
        _isSuccess = isSuccess;
        _error = error;
    }

    // ============================================================
    // Properties
    // ============================================================

    /// <summary>Gets a value indicating whether the result represents a success.</summary>
    public bool IsSuccess => _isSuccess;

    /// <summary>Gets a value indicating whether the result represents a failure.</summary>
    public bool IsFailure => !_isSuccess;

    /// <summary>
    /// Gets the error.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result is a success.</exception>
    public Error Error => !_isSuccess
        ? _error!
        : throw new InvalidOperationException(
            "Cannot access Error on a successful Result.");

    // ============================================================
    // Non-Generic Factory Methods
    // ============================================================

    /// <summary>
    /// Creates a successful result with no value.
    /// </summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result with a general failure error.
    /// </summary>
    /// <param name="description">The failure description.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(string description) =>
        new(false, Error.Failure(description));

    // ============================================================
    // Generic Factory Methods
    // ============================================================

    /// <summary>
    /// Creates a successful result containing the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The success value.</param>
    /// <returns>A successful <see cref="Result{TValue}"/>.</returns>
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <typeparam name="T">The type of the value (if it had succeeded).</typeparam>
    /// <param name="error">The error.</param>
    /// <returns>A failed <see cref="Result{TValue}"/>.</returns>
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);

    /// <summary>
    /// Creates a failed result with a general failure error.
    /// </summary>
    /// <typeparam name="T">The type of the value (if it had succeeded).</typeparam>
    /// <param name="description">The failure description.</param>
    /// <returns>A failed <see cref="Result{TValue}"/>.</returns>
    public static Result<T> Failure<T>(string description) =>
        Result<T>.Failure(Error.Failure(description));

    // ============================================================
    // Implicit Conversions
    // ============================================================

    /// <summary>
    /// Implicitly converts an <see cref="Primitives.Error"/> to a failed <see cref="Result"/>.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result(Error error) => Failure(error);

    // ============================================================
    // Core Operations
    // ============================================================

    /// <summary>
    /// Pattern matches over the result, executing the appropriate function
    /// for the success or failure case.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="onSuccess">Function to execute if the result is a success.</param>
    /// <param name="onFailure">Function to execute if the result is a failure.</param>
    /// <returns>The output of the matched function.</returns>
    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        return _isSuccess ? onSuccess() : onFailure(_error!);
    }

    /// <summary>
    /// Pattern matches over the result, executing the appropriate action
    /// for the success or failure case.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is a success.</param>
    /// <param name="onFailure">Action to execute if the result is a failure.</param>
    public void Switch(Action onSuccess, Action<Error> onFailure)
    {
        if (_isSuccess)
        {
            onSuccess();
        }
        else
        {
            onFailure(_error!);
        }
    }

    /// <summary>
    /// Executes a side-effect action on success without changing the result.
    /// Useful for logging, telemetry, or other side effects in a pipeline.
    /// </summary>
    /// <param name="action">The action to execute on success.</param>
    /// <returns>The same result, unchanged.</returns>
    public Result Tap(Action action)
    {
        if (_isSuccess)
        {
            action();
        }

        return this;
    }

    /// <summary>
    /// Executes an action when the result is a success.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The same result, unchanged.</returns>
    public Result OnSuccess(Action action)
    {
        if (_isSuccess)
        {
            action();
        }

        return this;
    }

    /// <summary>
    /// Executes an action when the result is a failure.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The same result, unchanged.</returns>
    public Result OnFailure(Action<Error> action)
    {
        if (!_isSuccess)
        {
            action(_error!);
        }

        return this;
    }

    // ============================================================
    // Bridging to Result<T>
    // ============================================================

    /// <summary>
    /// Transforms a successful <see cref="Result"/> into a <see cref="Result{T}"/>
    /// by producing a value. If the result is a failure, the error is propagated.
    /// </summary>
    /// <typeparam name="T">The type of the value to produce.</typeparam>
    /// <param name="mapper">A function that produces the value on success.</param>
    /// <returns>A <see cref="Result{T}"/> with the produced value, or the original error.</returns>
    public Result<T> Map<T>(Func<T> mapper)
    {
        return _isSuccess
            ? Result<T>.Success(mapper())
            : Result<T>.Failure(_error!);
    }

    /// <summary>
    /// Chains a result-producing function onto a successful <see cref="Result"/>.
    /// If the result is a failure, the error is propagated unchanged.
    /// </summary>
    /// <typeparam name="T">The type of the next result's value.</typeparam>
    /// <param name="binder">The function that produces the next result.</param>
    /// <returns>The result of the binder function, or the original error.</returns>
    public Result<T> Bind<T>(Func<Result<T>> binder)
    {
        return _isSuccess
            ? binder()
            : Result<T>.Failure(_error!);
    }

    // ============================================================
    // Deconstruct
    // ============================================================

    /// <summary>
    /// Deconstructs the result into its components for tuple-style consumption.
    /// </summary>
    /// <param name="isSuccess">Whether the result is a success.</param>
    /// <param name="error">The error, or <see langword="null"/> if successful.</param>
    public void Deconstruct(out bool isSuccess, out Error? error)
    {
        isSuccess = _isSuccess;
        error = _error;
    }

    // ============================================================
    // Async Operations
    // ============================================================

    /// <summary>
    /// Executes an async side-effect action on success without changing the result.
    /// </summary>
    /// <param name="action">The async action to execute on success.</param>
    /// <returns>A task containing the same result, unchanged.</returns>
    public async Task<Result> TapAsync(Func<Task> action)
    {
        if (_isSuccess)
        {
            await action().ConfigureAwait(false);
        }

        return this;
    }

    /// <summary>
    /// Executes an async side-effect action on the error without changing the result.
    /// </summary>
    /// <param name="action">The async action to execute on the error.</param>
    /// <returns>A task containing the same result, unchanged.</returns>
    public async Task<Result> TapErrorAsync(Func<Error, Task> action)
    {
        if (!_isSuccess)
        {
            await action(_error!).ConfigureAwait(false);
        }

        return this;
    }

    /// <summary>
    /// Async version of <see cref="Match{TOut}"/>.
    /// Pattern matches over the result using async functions.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="onSuccess">Async function to execute on success.</param>
    /// <param name="onFailure">Async function to execute on failure.</param>
    /// <returns>A task containing the output of the matched function.</returns>
    public async Task<TOut> MatchAsync<TOut>(
        Func<Task<TOut>> onSuccess,
        Func<Error, Task<TOut>> onFailure)
    {
        return _isSuccess
            ? await onSuccess().ConfigureAwait(false)
            : await onFailure(_error!).ConfigureAwait(false);
    }

    // ============================================================
    // Try / TryAsync
    // ============================================================

    /// <summary>
    /// Wraps an action in a try/catch, returning a <see cref="Result"/>
    /// instead of throwing an exception.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>A successful result, or a failure with the exception details.</returns>
    public static Result Try(Action action)
    {
        try
        {
            action();
            return Success();
        }
        catch (Exception ex)
        {
            return Failure(Error.Unexpected(ex.Message)
                .WithMetadata("ExceptionType", ex.GetType().FullName ?? ex.GetType().Name));
        }
    }

    /// <summary>
    /// Wraps a function call in a try/catch, returning a <see cref="Result{TValue}"/>
    /// instead of throwing an exception.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>A successful result with the function's return value, or a failure with the exception details.</returns>
    public static Result<T> Try<T>(Func<T> func)
    {
        try
        {
            return Success(func());
        }
        catch (Exception ex)
        {
            return Failure<T>(Error.Unexpected(ex.Message)
                .WithMetadata("ExceptionType", ex.GetType().FullName ?? ex.GetType().Name));
        }
    }

    /// <summary>
    /// Async version of <see cref="Try(Action)"/>.
    /// Wraps an async action in a try/catch.
    /// </summary>
    /// <param name="func">The async action to execute.</param>
    /// <returns>A task containing a successful result, or a failure with exception details.</returns>
    public static async Task<Result> TryAsync(Func<Task> func)
    {
        try
        {
            await func().ConfigureAwait(false);
            return Success();
        }
        catch (Exception ex)
        {
            return Failure(Error.Unexpected(ex.Message)
                .WithMetadata("ExceptionType", ex.GetType().FullName ?? ex.GetType().Name));
        }
    }

    /// <summary>
    /// Async version of <see cref="Try{T}"/>.
    /// Wraps an async function call in a try/catch.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The async function to execute.</param>
    /// <returns>A task containing a successful result or a failure with exception details.</returns>
    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func)
    {
        try
        {
            return Success(await func().ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            return Failure<T>(Error.Unexpected(ex.Message)
                .WithMetadata("ExceptionType", ex.GetType().FullName ?? ex.GetType().Name));
        }
    }

    // ============================================================
    // FromNullable / Combine
    // ============================================================

    /// <summary>
    /// Creates a successful result if the value is not null; otherwise creates a not-found failure.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="notFoundMessage">The error message if the value is null.</param>
    /// <returns>A successful result if value is not null; otherwise a not-found failure.</returns>
    public static Result<T> FromNullable<T>(T? value, string notFoundMessage = "Value was null.")
        where T : class =>
        value is not null
            ? Success(value)
            : Failure<T>(Error.NotFound(notFoundMessage));

    /// <summary>
    /// Creates a successful result if the nullable value type has a value; otherwise creates a not-found failure.
    /// </summary>
    /// <typeparam name="T">The underlying value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="notFoundMessage">The error message if the value is null.</param>
    /// <returns>A successful result if value has a value; otherwise a not-found failure.</returns>
    public static Result<T> FromNullable<T>(T? value, string notFoundMessage = "Value was null.")
        where T : struct =>
        value.HasValue
            ? Success(value.Value)
            : Failure<T>(Error.NotFound(notFoundMessage));

    /// <summary>
    /// Combines two results, returning a tuple of their values if both succeed,
    /// or the first error encountered.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <param name="result1">The first result.</param>
    /// <param name="result2">The second result.</param>
    /// <returns>A result containing a tuple of both values, or the first error.</returns>
    public static Result<(T1, T2)> Combine<T1, T2>(Result<T1> result1, Result<T2> result2)
    {
        if (result1.IsFailure)
        {
            return Failure<(T1, T2)>(result1.Error);
        }

        if (result2.IsFailure)
        {
            return Failure<(T1, T2)>(result2.Error);
        }

        return Success((result1.Value, result2.Value));
    }

    /// <summary>
    /// Combines three results, returning a tuple of their values if all succeed,
    /// or the first error encountered.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <typeparam name="T3">The type of the third value.</typeparam>
    /// <param name="result1">The first result.</param>
    /// <param name="result2">The second result.</param>
    /// <param name="result3">The third result.</param>
    /// <returns>A result containing a tuple of all values, or the first error.</returns>
    public static Result<(T1, T2, T3)> Combine<T1, T2, T3>(
        Result<T1> result1, Result<T2> result2, Result<T3> result3)
    {
        if (result1.IsFailure)
        {
            return Failure<(T1, T2, T3)>(result1.Error);
        }

        if (result2.IsFailure)
        {
            return Failure<(T1, T2, T3)>(result2.Error);
        }

        if (result3.IsFailure)
        {
            return Failure<(T1, T2, T3)>(result3.Error);
        }

        return Success((result1.Value, result2.Value, result3.Value));
    }

    /// <summary>
    /// Combines four results, returning a tuple of their values if all succeed,
    /// or the first error encountered.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <typeparam name="T3">The type of the third value.</typeparam>
    /// <typeparam name="T4">The type of the fourth value.</typeparam>
    /// <param name="result1">The first result.</param>
    /// <param name="result2">The second result.</param>
    /// <param name="result3">The third result.</param>
    /// <param name="result4">The fourth result.</param>
    /// <returns>A result containing a tuple of all values, or the first error.</returns>
    public static Result<(T1, T2, T3, T4)> Combine<T1, T2, T3, T4>(
        Result<T1> result1, Result<T2> result2, Result<T3> result3, Result<T4> result4)
    {
        if (result1.IsFailure)
        {
            return Failure<(T1, T2, T3, T4)>(result1.Error);
        }

        if (result2.IsFailure)
        {
            return Failure<(T1, T2, T3, T4)>(result2.Error);
        }

        if (result3.IsFailure)
        {
            return Failure<(T1, T2, T3, T4)>(result3.Error);
        }

        if (result4.IsFailure)
        {
            return Failure<(T1, T2, T3, T4)>(result4.Error);
        }

        return Success((result1.Value, result2.Value, result3.Value, result4.Value));
    }

    /// <summary>
    /// Combines five results, returning a tuple of their values if all succeed,
    /// or the first error encountered.
    /// </summary>
    /// <typeparam name="T1">The type of the first value.</typeparam>
    /// <typeparam name="T2">The type of the second value.</typeparam>
    /// <typeparam name="T3">The type of the third value.</typeparam>
    /// <typeparam name="T4">The type of the fourth value.</typeparam>
    /// <typeparam name="T5">The type of the fifth value.</typeparam>
    /// <param name="result1">The first result.</param>
    /// <param name="result2">The second result.</param>
    /// <param name="result3">The third result.</param>
    /// <param name="result4">The fourth result.</param>
    /// <param name="result5">The fifth result.</param>
    /// <returns>A result containing a tuple of all values, or the first error.</returns>
    public static Result<(T1, T2, T3, T4, T5)> Combine<T1, T2, T3, T4, T5>(
        Result<T1> result1, Result<T2> result2, Result<T3> result3,
        Result<T4> result4, Result<T5> result5)
    {
        if (result1.IsFailure)
        {
            return Failure<(T1, T2, T3, T4, T5)>(result1.Error);
        }

        if (result2.IsFailure)
        {
            return Failure<(T1, T2, T3, T4, T5)>(result2.Error);
        }

        if (result3.IsFailure)
        {
            return Failure<(T1, T2, T3, T4, T5)>(result3.Error);
        }

        if (result4.IsFailure)
        {
            return Failure<(T1, T2, T3, T4, T5)>(result4.Error);
        }

        if (result5.IsFailure)
        {
            return Failure<(T1, T2, T3, T4, T5)>(result5.Error);
        }

        return Success((result1.Value, result2.Value, result3.Value, result4.Value, result5.Value));
    }

    // ============================================================
    // Equality
    // ============================================================

    /// <inheritdoc />
    public bool Equals(Result other)
    {
        if (_isSuccess != other._isSuccess)
        {
            return false;
        }

        return _isSuccess || EqualityComparer<Error>.Default.Equals(_error, other._error);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is Result other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        _isSuccess
            ? HashCode.Combine(true)
            : HashCode.Combine(false, _error);

    /// <summary>Equality operator.</summary>
    public static bool operator ==(Result left, Result right) =>
        left.Equals(right);

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(Result left, Result right) =>
        !left.Equals(right);

    // ============================================================
    // Display
    // ============================================================

    /// <inheritdoc />
    public override string ToString() =>
        _isSuccess ? "Success" : $"Failure({_error})";
}
