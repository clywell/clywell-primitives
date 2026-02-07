namespace Clywell.Primitives;

/// <summary>
/// Extension members for <see cref="Result{TValue}"/> and <see cref="Result"/>
/// providing LINQ-style railway-oriented programming operations.
/// </summary>
public static class ResultExtensions
{
    // ============================================================
    // Non-Generic Result Instance Extensions
    // ============================================================

    /// <summary>
    /// Defines instance extension members for <see cref="Result"/>.
    /// </summary>
    /// <param name="result">The result instance.</param>
    extension(Result result)
    {
        /// <summary>
        /// Validates a condition on a successful result. If the predicate fails,
        /// returns a failure with the specified error.
        /// </summary>
        /// <param name="predicate">The validation predicate.</param>
        /// <param name="error">The error to use if validation fails.</param>
        /// <returns>The original result if the predicate passes; otherwise a failure.</returns>
        public Result Ensure(Func<bool> predicate, Error error)
        {
            if (result.IsFailure)
            {
                return result;
            }

            return predicate() ? result : Result.Failure(error);
        }

        /// <summary>
        /// Maps the error of a failed result to a different error.
        /// If the result is a success, it is returned unchanged.
        /// </summary>
        /// <param name="errorMapper">The function to transform the error.</param>
        /// <returns>The original success or a failure with the transformed error.</returns>
        public Result MapError(Func<Error, Error> errorMapper)
        {
            return result.IsSuccess ? result : Result.Failure(errorMapper(result.Error));
        }

        /// <summary>
        /// Executes a side-effect action on the error without changing the result.
        /// Useful for logging or telemetry on the failure path.
        /// </summary>
        /// <param name="action">The action to execute on the error.</param>
        /// <returns>The same result, unchanged.</returns>
        public Result TapError(Action<Error> action)
        {
            if (result.IsFailure)
            {
                action(result.Error);
            }

            return result;
        }
    }

    // ============================================================
    // Non-Generic Async Extensions for Task<Result>
    // ============================================================

    /// <summary>
    /// Defines async pipeline extension members for <see cref="Task{Result}"/>.
    /// </summary>
    /// <param name="resultTask">The task containing the result.</param>
    extension(Task<Result> resultTask)
    {
        /// <summary>
        /// Async pipeline: Matches over a <see cref="Task{Result}"/>.
        /// </summary>
        /// <typeparam name="TOut">The output type.</typeparam>
        /// <param name="onSuccess">Function for the success case.</param>
        /// <param name="onFailure">Function for the failure case.</param>
        /// <returns>A task containing the matched output.</returns>
        public async Task<TOut> Match<TOut>(
            Func<TOut> onSuccess,
            Func<Error, TOut> onFailure)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.Match(onSuccess, onFailure);
        }

        /// <summary>
        /// Async pipeline: Taps the success of a <see cref="Task{Result}"/>.
        /// </summary>
        /// <param name="action">The side-effect action.</param>
        /// <returns>A task containing the same result.</returns>
        public async Task<Result> Tap(Action action)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.Tap(action);
        }

        /// <summary>
        /// Async pipeline: Taps the error of a <see cref="Task{Result}"/>.
        /// </summary>
        /// <param name="action">The side-effect action on error.</param>
        /// <returns>A task containing the same result.</returns>
        public async Task<Result> TapError(Action<Error> action)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.TapError(action);
        }

        /// <summary>
        /// Async pipeline: Ensures a condition on a <see cref="Task{Result}"/>.
        /// </summary>
        /// <param name="predicate">The validation predicate.</param>
        /// <param name="error">The error if validation fails.</param>
        /// <returns>A task containing the validated result.</returns>
        public async Task<Result> Ensure(Func<bool> predicate, Error error)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.Ensure(predicate, error);
        }

        /// <summary>
        /// Async pipeline: Maps a successful <see cref="Task{Result}"/> into a <see cref="Result{T}"/>.
        /// </summary>
        /// <typeparam name="T">The value type to produce.</typeparam>
        /// <param name="mapper">A function that produces the value on success.</param>
        /// <returns>A task containing the mapped result.</returns>
        public async Task<Result<T>> Map<T>(Func<T> mapper)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.Map(mapper);
        }

        /// <summary>
        /// Async pipeline: Binds a successful <see cref="Task{Result}"/> into a <see cref="Result{T}"/>.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="binder">The function that produces the next result.</param>
        /// <returns>A task containing the bound result.</returns>
        public async Task<Result<T>> Bind<T>(Func<Result<T>> binder)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.Bind(binder);
        }
    }

    // ============================================================
    // Generic Result<T> Instance Extensions
    // ============================================================

    /// <summary>
    /// Defines instance extension members for <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result instance.</param>
    extension<T>(Result<T> result)
    {
        /// <summary>
        /// Validates the success value against a predicate. If the predicate fails,
        /// returns a failure with the specified error.
        /// </summary>
        /// <param name="predicate">The validation predicate.</param>
        /// <param name="error">The error to use if validation fails.</param>
        /// <returns>The original result if the predicate passes; otherwise a failure.</returns>
        public Result<T> Ensure(Func<T, bool> predicate, Error error)
        {
            if (result.IsFailure)
            {
                return result;
            }

            return predicate(result.Value) ? result : error;
        }

        /// <summary>
        /// Validates the success value against a predicate with a lazy error factory.
        /// </summary>
        /// <param name="predicate">The validation predicate.</param>
        /// <param name="errorFactory">A factory that produces the error from the value.</param>
        /// <returns>The original result if the predicate passes; otherwise a failure.</returns>
        public Result<T> Ensure(Func<T, bool> predicate, Func<T, Error> errorFactory)
        {
            if (result.IsFailure)
            {
                return result;
            }

            return predicate(result.Value) ? result : errorFactory(result.Value);
        }

        /// <summary>
        /// Maps the error of a failed result to a different error.
        /// If the result is a success, it is returned unchanged.
        /// </summary>
        /// <param name="errorMapper">The function to transform the error.</param>
        /// <returns>The original success or a failure with the transformed error.</returns>
        public Result<T> MapError(Func<Error, Error> errorMapper)
        {
            return result.IsSuccess ? result : Result.Failure<T>(errorMapper(result.Error));
        }

        /// <summary>
        /// Executes a side-effect action on the error without changing the result.
        /// Useful for logging or telemetry on the failure path.
        /// </summary>
        /// <param name="action">The action to execute on the error.</param>
        /// <returns>The same result, unchanged.</returns>
        public Result<T> TapError(Action<Error> action)
        {
            if (result.IsFailure)
            {
                action(result.Error);
            }

            return result;
        }
    }

    // ============================================================
    // Generic Async Extensions for Task<Result<T>>
    // ============================================================

    /// <summary>
    /// Defines async pipeline extension members for <see cref="Task{Result}"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    extension<T>(Task<Result<T>> resultTask)
    {
        /// <summary>
        /// Async pipeline: Maps the success value.
        /// </summary>
        /// <typeparam name="TOut">The target value type.</typeparam>
        /// <param name="mapper">The transformation function.</param>
        /// <returns>A task containing the mapped result.</returns>
        public async Task<Result<TOut>> Map<TOut>(Func<T, TOut> mapper)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.Map(mapper);
        }

        /// <summary>
        /// Async pipeline: Binds the success value.
        /// </summary>
        /// <typeparam name="TOut">The target value type.</typeparam>
        /// <param name="binder">The function that produces the next result.</param>
        /// <returns>A task containing the bound result.</returns>
        public async Task<Result<TOut>> Bind<TOut>(Func<T, Result<TOut>> binder)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.Bind(binder);
        }

        /// <summary>
        /// Async pipeline: Binds the success value with an async binder.
        /// </summary>
        /// <typeparam name="TOut">The target value type.</typeparam>
        /// <param name="binder">The async function that produces the next result.</param>
        /// <returns>A task containing the bound result.</returns>
        public async Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> binder)
        {
            var result = await resultTask.ConfigureAwait(false);
            return await result.BindAsync(binder).ConfigureAwait(false);
        }

        /// <summary>
        /// Async pipeline: Matches over a <see cref="Task{Result}"/>.
        /// </summary>
        /// <typeparam name="TOut">The output type.</typeparam>
        /// <param name="onSuccess">Function for the success case.</param>
        /// <param name="onFailure">Function for the failure case.</param>
        /// <returns>A task containing the matched output.</returns>
        public async Task<TOut> Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.Match(onSuccess, onFailure);
        }

        /// <summary>
        /// Async pipeline: Ensures a condition on a <see cref="Task{Result}"/>.
        /// </summary>
        /// <param name="predicate">The validation predicate.</param>
        /// <param name="error">The error if validation fails.</param>
        /// <returns>A task containing the validated result.</returns>
        public async Task<Result<T>> Ensure(Func<T, bool> predicate, Error error)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.Ensure(predicate, error);
        }

        /// <summary>
        /// Async pipeline: Taps the success value.
        /// </summary>
        /// <param name="action">The side-effect action.</param>
        /// <returns>A task containing the same result.</returns>
        public async Task<Result<T>> Tap(Action<T> action)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.Tap(action);
        }

        /// <summary>
        /// Async pipeline: Taps the error.
        /// </summary>
        /// <param name="action">The side-effect action on error.</param>
        /// <returns>A task containing the same result.</returns>
        public async Task<Result<T>> TapError(Action<Error> action)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.TapError(action);
        }

        /// <summary>
        /// Async pipeline: Ensures a condition with lazy error factory on a <see cref="Task{Result}"/>.
        /// </summary>
        /// <param name="predicate">The validation predicate.</param>
        /// <param name="errorFactory">A factory that produces the error from the value.</param>
        /// <returns>A task containing the validated result.</returns>
        public async Task<Result<T>> Ensure(Func<T, bool> predicate, Func<T, Error> errorFactory)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.Ensure(predicate, errorFactory);
        }
    }

    // ============================================================
    // Collection Extensions
    // ============================================================

    /// <summary>
    /// Defines extension members for sequences of <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="results">The sequence of results.</param>
    extension<T>(IEnumerable<Result<T>> results)
    {
        /// <summary>
        /// Collects a sequence of results into a single result containing all values.
        /// Returns a failure with the first error encountered.
        /// </summary>
        /// <returns>A result containing all values, or the first error.</returns>
        public Result<IReadOnlyList<T>> Collect()
        {
            var values = new List<T>();

            foreach (var result in results)
            {
                if (result.IsFailure)
                {
                    return result.Error;
                }

                values.Add(result.Value);
            }

            return values.AsReadOnly();
        }
    }
}
