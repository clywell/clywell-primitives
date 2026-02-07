# Clywell.Primitives - Quick Reference

> One-page reference for Clywell.Primitives v1.0.0-alpha.1

---

## Installation

```bash
dotnet add package Clywell.Primitives
```

---

## Creating Results

```csharp
using Clywell.Primitives;

// ✅ Implicit conversions (preferred)
Result<int> success = 42;
Result<int> failure = Error.NotFound("Not found");

// ✅ Factory methods
var s = Result.Success(42);
var f = Result.Failure<int>(Error.Conflict("Duplicate"));
var f2 = Result.Failure<int>("Something went wrong");

// ✅ From exceptions
var r = Result.Try(() => int.Parse(input));
var ra = await Result.TryAsync(() => httpClient.GetAsync(url));

// ✅ From nullable
var r = Result.FromNullable(maybeNull, "Value was null");
```

---

## Error Types

```csharp
// Built-in factory methods
Error.Failure("General failure")
Error.NotFound("Resource not found")
Error.Conflict("Already exists")
Error.Unauthorized("Not authenticated")
Error.Forbidden("Not authorized")
Error.Unexpected("Internal error")
Error.Unavailable("Service down")

// Validation errors
Error.Validation("Email", "Required")
Error.Validation(
    new ValidationFailure("Email", "Required"),
    new ValidationFailure("Name", "Too short"))

// Enriching errors
error.WithMetadata("RequestId", "abc-123")
error.WithInnerError(innerError)
```

---

## Pattern Matching

```csharp
// Match (with return value)
var output = result.Match(
    onSuccess: value => $"Got: {value}",
    onFailure: error => $"Error: {error.Description}");

// Switch (side effects only)
result.Switch(
    onSuccess: value => Console.WriteLine(value),
    onFailure: error => logger.LogError(error.ToString()));
```

---

## Chaining (Railway-Oriented)

```csharp
// Synchronous pipeline
var result = Result.Success(input)
    .Map(v => Transform(v))          // Transform value
    .Ensure(v => v > 0, error)       // Validate
    .Bind(v => GetOther(v))          // Chain Result-returning fn
    .Tap(v => Log(v))                // Side effect
    .OnSuccess(v => Notify(v))       // On success only
    .OnFailure(e => LogError(e));    // On failure only

// Async pipeline
var result = await Task.FromResult(Result.Success(input))
    .Map(v => Transform(v))
    .BindAsync(v => GetOtherAsync(v))
    .Tap(v => Log(v));
```

---

## Common Patterns

```csharp
// Service method
public Result<User> GetUser(int id)
{
    var user = repository.FindById(id);
    return Result.FromNullable(user, $"User {id} not found");
}

// Chained validation
public Result<Order> ValidateOrder(Order order) =>
    Result.Success(order)
        .Ensure(o => o.Items.Any(), Error.Validation("Items", "Order must have items"))
        .Ensure(o => o.Total > 0, Error.Validation("Total", "Must be positive"))
        .Ensure(o => o.CustomerId > 0, Error.Validation("CustomerId", "Required"));

// Combining results
var combined = Result.Combine(result1, result2, result3);

// Collecting results
var collected = results.Collect(); // IEnumerable<Result<T>> → Result<IReadOnlyList<T>>

// Fallback values
var value = result.ValueOr(defaultValue);
var value = result.ValueOr(error => ComputeFallback(error));
```

---

## Error Codes

| Code                     | Value                  |
| ------------------------ | ---------------------- |
| `ErrorCode.Failure`      | `General.Failure`      |
| `ErrorCode.Validation`   | `General.Validation`   |
| `ErrorCode.NotFound`     | `General.NotFound`     |
| `ErrorCode.Conflict`     | `General.Conflict`     |
| `ErrorCode.Unauthorized` | `General.Unauthorized` |
| `ErrorCode.Forbidden`    | `General.Forbidden`    |
| `ErrorCode.Unexpected`   | `General.Unexpected`   |
| `ErrorCode.Unavailable`  | `General.Unavailable`  |

Custom codes: `ErrorCode code = "MyDomain.MyError";`

---

## API Surface

### Result\<T\> Methods

| Method               | Returns              | Description                   |
| -------------------- | -------------------- | ----------------------------- |
| `.IsSuccess`         | `bool`               | Is this a success?            |
| `.IsFailure`         | `bool`               | Is this a failure?            |
| `.Value`             | `T`                  | Get value (throws if failure) |
| `.Error`             | `Error`              | Get error (throws if success) |
| `.Match(s, f)`       | `TOut`               | Pattern match                 |
| `.Switch(s, f)`      | `void`               | Pattern match (actions)       |
| `.Map(fn)`           | `Result<TOut>`       | Transform value               |
| `.Bind(fn)`          | `Result<TOut>`       | Chain result fn               |
| `.Tap(action)`       | `Result<T>`          | Side effect on success        |
| `.OnSuccess(action)` | `Result<T>`          | Execute on success            |
| `.OnFailure(action)` | `Result<T>`          | Execute on failure            |
| `.ValueOr(fallback)` | `T`                  | Get value or fallback         |
| `.MapAsync(fn)`      | `Task<Result<TOut>>` | Async transform               |
| `.BindAsync(fn)`     | `Task<Result<TOut>>` | Async chain                   |
| `.MatchAsync(s, f)`  | `Task<TOut>`         | Async match                   |

### Extension Methods

| Method               | Description             |
| -------------------- | ----------------------- |
| `.Ensure(pred, err)` | Validate with predicate |
| `.MapError(fn)`      | Transform error         |
| `.TapError(action)`  | Side effect on error    |
| `.Collect()`         | Aggregate results       |

### Static Factory

| Method                   | Description       |
| ------------------------ | ----------------- |
| `Result.Success<T>(v)`   | Create success    |
| `Result.Failure<T>(err)` | Create failure    |
| `Result.Try(fn)`         | Wrap in try/catch |
| `Result.TryAsync(fn)`    | Async try/catch   |
| `Result.FromNullable(v)` | Nullable → Result |
| `Result.Combine(r1, r2)` | Combine results   |
