using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Clywell.Primitives;

/// <summary>
/// Represents the outcome of an operation that can either succeed with a value of type
/// <typeparamref name="TValue"/> or fail with an <see cref="Primitives.Error"/>.
/// </summary>
/// <typeparam name="TValue">The type of the success value.</typeparam>
/// <remarks>
/// <para>
/// Result is a discriminated union that forces explicit handling of both success and failure cases,
/// eliminating the need for exceptions in expected failure scenarios.
/// </para>
/// <para>
/// Use the static factory methods <see cref="Result.Success{T}(T)"/> and <see cref="Result.Failure{T}(Error)"/>
/// (or implicit conversions) to create instances. Use <see cref="Match{TOut}"/>, <see cref="Map{TOut}"/>,
/// and <see cref="Bind{TOut}"/> for railway-oriented composition.
/// </para>
/// </remarks>
[DebuggerDisplay("{IsSuccess ? \"Success(\" + _value + \")\" : \"Failure(\" + _error + \")\"}")]
public readonly struct Result<TValue> : IEquatable<Result<TValue>>
{
    private readonly TValue? _value;
    private readonly Error? _error;
    private readonly bool _isSuccess;

    private Result(TValue value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
        _error = null;
        _isSuccess = true;
    }

    private Result(Error error)
    {
        _value = default;
        _error = error ?? throw new ArgumentNullException(nameof(error));
        _isSuccess = false;
    }

    // ============================================================
    // Properties
    // ============================================================

    /// <summary>Gets a value indicating whether the result represents a success.</summary>
    [MemberNotNullWhen(true, nameof(_value))]
    public bool IsSuccess => _isSuccess;

    /// <summary>Gets a value indicating whether the result represents a failure.</summary>
    [MemberNotNullWhen(true, nameof(_error))]
    public bool IsFailure => !_isSuccess;

    /// <summary>
    /// Gets the success value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public TValue Value => _isSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Cannot access Value on a failed Result. Error: {_error}");

    /// <summary>
    /// Gets the error.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result is a success.</exception>
    public Error Error => !_isSuccess
        ? _error!
        : throw new InvalidOperationException(
            "Cannot access Error on a successful Result.");

    // ============================================================
    // Factory Methods
    // ============================================================

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A successful <see cref="Result{TValue}"/>.</returns>
    internal static Result<TValue> Success(TValue value) => new(value);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A failed <see cref="Result{TValue}"/>.</returns>
    internal static Result<TValue> Failure(Error error) => new(error);

    // ============================================================
    // Implicit Conversions
    // ============================================================

    /// <summary>
    /// Implicitly converts a value to a successful <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="value">The success value.</param>
    public static implicit operator Result<TValue>(TValue value) => new(value);

    /// <summary>
    /// Implicitly converts an <see cref="Primitives.Error"/> to a failed <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result<TValue>(Error error) => new(error);

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
    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        return _isSuccess ? onSuccess(_value!) : onFailure(_error!);
    }

    /// <summary>
    /// Pattern matches over the result, executing the appropriate action
    /// for the success or failure case.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is a success.</param>
    /// <param name="onFailure">Action to execute if the result is a failure.</param>
    public void Switch(Action<TValue> onSuccess, Action<Error> onFailure)
    {
        if (_isSuccess)
        {
            onSuccess(_value!);
        }
        else
        {
            onFailure(_error!);
        }
    }

    /// <summary>
    /// Transforms the success value using the specified function.
    /// If the result is a failure, the error is propagated unchanged.
    /// </summary>
    /// <typeparam name="TOut">The type of the transformed value.</typeparam>
    /// <param name="mapper">The transformation function.</param>
    /// <returns>A new <see cref="Result{TValue}"/> with the transformed value, or the original error.</returns>
    public Result<TOut> Map<TOut>(Func<TValue, TOut> mapper)
    {
        return _isSuccess
            ? Result<TOut>.Success(mapper(_value!))
            : Result<TOut>.Failure(_error!);
    }

    /// <summary>
    /// Chains a result-producing function onto a successful result.
    /// If the result is a failure, the error is propagated unchanged.
    /// This is the monadic bind operation (flatMap).
    /// </summary>
    /// <typeparam name="TOut">The type of the next result's value.</typeparam>
    /// <param name="binder">The function that produces the next result.</param>
    /// <returns>The result of the binder function, or the original error.</returns>
    public Result<TOut> Bind<TOut>(Func<TValue, Result<TOut>> binder)
    {
        return _isSuccess
            ? binder(_value!)
            : Result<TOut>.Failure(_error!);
    }

    /// <summary>
    /// Executes a side-effect action on the success value without changing the result.
    /// Useful for logging, telemetry, or other side effects in a pipeline.
    /// </summary>
    /// <param name="action">The action to execute on the success value.</param>
    /// <returns>The same result, unchanged.</returns>
    public Result<TValue> Tap(Action<TValue> action)
    {
        if (_isSuccess)
        {
            action(_value!);
        }

        return this;
    }

    /// <summary>
    /// Executes an action when the result is a success.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The same result, unchanged.</returns>
    public Result<TValue> OnSuccess(Action<TValue> action)
    {
        if (_isSuccess)
        {
            action(_value!);
        }

        return this;
    }

    /// <summary>
    /// Executes an action when the result is a failure.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The same result, unchanged.</returns>
    public Result<TValue> OnFailure(Action<Error> action)
    {
        if (!_isSuccess)
        {
            action(_error!);
        }

        return this;
    }

    /// <summary>
    /// Discards the success value and returns a non-generic <see cref="Result"/>.
    /// Useful when a caller only cares about success/failure, not the value.
    /// </summary>
    /// <returns>A <see cref="Result"/> preserving the success/failure state and error.</returns>
    public Result ToResult()
    {
        return _isSuccess ? Result.Success() : Result.Failure(_error!);
    }

    /// <summary>
    /// Deconstructs the result into its components for tuple-style consumption.
    /// </summary>
    /// <param name="isSuccess">Whether the result is a success.</param>
    /// <param name="value">The value, or <see langword="default"/> if failure.</param>
    /// <param name="error">The error, or <see langword="null"/> if successful.</param>
    public void Deconstruct(out bool isSuccess, out TValue? value, out Error? error)
    {
        isSuccess = _isSuccess;
        value = _value;
        error = _error;
    }

    /// <summary>
    /// Returns an alternative value if this result is a failure.
    /// </summary>
    /// <param name="fallback">The fallback value.</param>
    /// <returns>The success value if successful; otherwise the fallback value.</returns>
    public TValue ValueOr(TValue fallback) => _isSuccess ? _value! : fallback;

    /// <summary>
    /// Returns the result of an alternative function if this result is a failure.
    /// </summary>
    /// <param name="fallbackFactory">A function that produces the fallback value.</param>
    /// <returns>The success value if successful; otherwise the result of the fallback factory.</returns>
    public TValue ValueOr(Func<Error, TValue> fallbackFactory)
    {
        return _isSuccess ? _value! : fallbackFactory(_error!);
    }

    // ============================================================
    // Async Operations
    // ============================================================

    /// <summary>
    /// Executes an async side-effect action on the success value without changing the result.
    /// </summary>
    /// <param name="action">The async action to execute on the success value.</param>
    /// <returns>A task containing the same result, unchanged.</returns>
    public async Task<Result<TValue>> TapAsync(Func<TValue, Task> action)
    {
        if (_isSuccess)
        {
            await action(_value!).ConfigureAwait(false);
        }

        return this;
    }

    /// <summary>
    /// Executes an async side-effect action on the error without changing the result.
    /// </summary>
    /// <param name="action">The async action to execute on the error.</param>
    /// <returns>A task containing the same result, unchanged.</returns>
    public async Task<Result<TValue>> TapErrorAsync(Func<Error, Task> action)
    {
        if (!_isSuccess)
        {
            await action(_error!).ConfigureAwait(false);
        }

        return this;
    }

    /// <summary>
    /// Async version of <see cref="Map{TOut}"/>.
    /// Transforms the success value using an async function.
    /// </summary>
    /// <typeparam name="TOut">The type of the transformed value.</typeparam>
    /// <param name="mapper">The async transformation function.</param>
    /// <returns>A task containing the new result.</returns>
    public async Task<Result<TOut>> MapAsync<TOut>(Func<TValue, Task<TOut>> mapper)
    {
        return _isSuccess
            ? Result<TOut>.Success(await mapper(_value!).ConfigureAwait(false))
            : Result<TOut>.Failure(_error!);
    }

    /// <summary>
    /// Async version of <see cref="Bind{TOut}"/>.
    /// Chains an async result-producing function onto a successful result.
    /// </summary>
    /// <typeparam name="TOut">The type of the next result's value.</typeparam>
    /// <param name="binder">The async function that produces the next result.</param>
    /// <returns>A task containing the result of the binder, or the original error.</returns>
    public async Task<Result<TOut>> BindAsync<TOut>(Func<TValue, Task<Result<TOut>>> binder)
    {
        return _isSuccess
            ? await binder(_value!).ConfigureAwait(false)
            : Result<TOut>.Failure(_error!);
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
        Func<TValue, Task<TOut>> onSuccess,
        Func<Error, Task<TOut>> onFailure)
    {
        return _isSuccess
            ? await onSuccess(_value!).ConfigureAwait(false)
            : await onFailure(_error!).ConfigureAwait(false);
    }

    // ============================================================
    // Equality
    // ============================================================

    /// <inheritdoc />
    public bool Equals(Result<TValue> other)
    {
        if (_isSuccess != other._isSuccess)
        {
            return false;
        }

        return _isSuccess
            ? EqualityComparer<TValue>.Default.Equals(_value, other._value)
            : EqualityComparer<Error>.Default.Equals(_error, other._error);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is Result<TValue> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        _isSuccess
            ? HashCode.Combine(true, _value)
            : HashCode.Combine(false, _error);

    /// <summary>Equality operator.</summary>
    public static bool operator ==(Result<TValue> left, Result<TValue> right) =>
        left.Equals(right);

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(Result<TValue> left, Result<TValue> right) =>
        !left.Equals(right);

    // ============================================================
    // Display
    // ============================================================

    /// <inheritdoc />
    public override string ToString() =>
        _isSuccess ? $"Success({_value})" : $"Failure({_error})";
}
