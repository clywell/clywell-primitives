# Clywell.Primitives

[![NuGet](https://img.shields.io/nuget/v/Clywell.Primitives.svg)](https://www.nuget.org/packages/Clywell.Primitives/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Generic Result pattern and functional primitives for .NET. Provides railway-oriented programming extensions, typed error handling, and pattern matching support with **zero dependencies** and **zero business logic** — can be used in any .NET application.

---

## Features

- **Result\<T\> & Result** — Type-safe success/failure for value-returning and void-like operations
- **Typed Error Hierarchy** — `Error`, `ErrorCode`, `ValidationError`, `ValidationFailure` with metadata, inner errors, and field-level details
- **Railway-Oriented Programming** — `Map`, `Bind`, `Match`, `Tap`, `Ensure`, `MapError`, `TapError` for composable pipelines
- **Implicit Conversions** — Ergonomic syntax: assign values or errors directly to `Result<T>` / `Result`
- **Async Pipeline Support** — Full async extensions for `Task<Result<T>>` and `Task<Result>` composition
- **Pattern Matching** — `Match` (with return) and `Switch` (side-effects) for exhaustive handling
- **Collection Operations** — `Collect` to aggregate `IEnumerable<Result<T>>` and `Combine` for tuples
- **Try/Catch Wrapping** — `Result.Try()` / `Result.TryAsync()` to convert exception-based code to Results
- **Nullable Bridging** — `Result.FromNullable()` for reference types and nullable value types
- **Deconstruct & Bridging** — `Deconstruct` for tuple-style consumption, `ToResult()` / `Map<T>` / `Bind<T>` to bridge between `Result` and `Result<T>`
- **Zero Dependencies** — 100% standalone, no external NuGet packages
- **.NET 10.0+ / C# 14** — Modern language features including extension members
- **Source Link Enabled** — Step into source code when debugging NuGet package
- **190+ Unit Tests** — Comprehensive coverage across all types and operations

---

## Installation

```bash
dotnet add package Clywell.Primitives
```

---

## Quick Start

```csharp
using Clywell.Primitives;

// ── Value-returning operations: Result<T> ──────────────────────

// Implicit conversions — no factory needed
Result<int> success = 42;
Result<int> failure = Error.NotFound("User not found.");

// Factory methods
var ok   = Result.Success(42);
var fail = Result.Failure<int>(Error.Conflict("Already exists"));

// ── Void-like operations: Result ───────────────────────────────

Result deleted   = Result.Success();
Result notFound  = Result.Failure(Error.NotFound("Item not found"));
Result fromError = Error.Forbidden("Not allowed");  // implicit conversion

// ── Pattern matching ───────────────────────────────────────────

string message = ok.Match(
    onSuccess: value => $"Got: {value}",
    onFailure: error => $"Failed: {error.Description}");

deleted.Switch(
    onSuccess: () => Console.WriteLine("Done"),
    onFailure: error => Console.WriteLine($"Error: {error}"));

// ── Railway-oriented pipeline ──────────────────────────────────

var result = Result.Success("42")
    .Map(int.Parse)                                    // string → int
    .Ensure(v => v > 0, Error.Failure("Must be > 0"))  // validate
    .Bind(v => LookupUser(v))                          // int → Result<User>
    .Tap(user => Console.WriteLine(user.Name))         // side effect
    .MapError(e => Error.Unexpected(e.Description))    // remap error
    .Map(user => user.Email);                          // User → string
```

---

## Core Types

### `ErrorCode`

A `readonly record struct` classifying error categories. Supports implicit conversion to/from `string` for extensibility.

| Code                     | Value                    | Usage                             |
| ------------------------ | ------------------------ | --------------------------------- |
| `ErrorCode.Failure`      | `"General.Failure"`      | General/unspecified failure       |
| `ErrorCode.Validation`   | `"General.Validation"`   | Validation rule violations        |
| `ErrorCode.NotFound`     | `"General.NotFound"`     | Resource not found                |
| `ErrorCode.Conflict`     | `"General.Conflict"`     | Duplicate/conflict scenarios      |
| `ErrorCode.Unauthorized` | `"General.Unauthorized"` | Authentication failures           |
| `ErrorCode.Forbidden`    | `"General.Forbidden"`    | Authorization/permission failures |
| `ErrorCode.Unexpected`   | `"General.Unexpected"`   | Internal/unexpected errors        |
| `ErrorCode.Unavailable`  | `"General.Unavailable"`  | Service/resource unavailable      |

```csharp
// Custom error codes via implicit conversion
ErrorCode custom = "Billing.PaymentDeclined";

// String comparison
string code = ErrorCode.NotFound; // "General.NotFound"
```

### `Error`

A `record` with `Code`, `Description`, optional `InnerError`, and `Metadata`. Immutable — builder methods return new instances.

#### Factory Methods

```csharp
Error.Failure("Something went wrong")
Error.NotFound("User not found")
Error.Conflict("Email already registered")
Error.Unauthorized("Invalid credentials")
Error.Forbidden("Insufficient permissions")
Error.Unexpected("Unhandled exception occurred")
Error.Unavailable("Service temporarily down")
Error.Validation("Email", "Email is required")          // single field
Error.Validation(                                        // multiple fields
    new ValidationFailure("Email", "Required"),
    new ValidationFailure("Age", "Must be ≥ 18"))
```

#### Builder Methods (Immutable)

```csharp
var error = Error.NotFound("Order not found")
    .WithMetadata("OrderId", orderId)
    .WithMetadata("RequestId", correlationId)
    .WithInnerError(originalError);

// Bulk metadata
var enriched = error.WithMetadata(new Dictionary<string, object>
{
    ["Timestamp"] = DateTime.UtcNow,
    ["Retry"] = 3
});
```

#### Properties

| Property      | Type                                 | Description                         |
| ------------- | ------------------------------------ | ----------------------------------- |
| `Code`        | `ErrorCode`                          | Categorized error code              |
| `Description` | `string`                             | Human-readable error message        |
| `InnerError`  | `Error?`                             | Optional causal error (error chain) |
| `Metadata`    | `ImmutableDictionary<string,object>` | Key-value pairs for context         |

### `ValidationFailure`

A `readonly record struct` representing a single field-level validation failure.

```csharp
var failure = new ValidationFailure("Email", "Email is required");
failure.FieldName; // "Email"
failure.Message;   // "Email is required"
failure.ToString(); // "Email: Email is required"
```

### `ValidationError`

A `sealed record` extending `Error` with structured validation details. Always has `ErrorCode.Validation`.

```csharp
var error = Error.Validation(
    new ValidationFailure("Email", "Required"),
    new ValidationFailure("Name", "Too long"));

error.Failures;                     // ImmutableArray<ValidationFailure>
error.FailureCount;                 // 2
error.HasFailureForField("Email");  // true
error.GetFailuresForField("Email"); // IEnumerable<ValidationFailure>

// Append failures (returns new instance)
var combined = error.AddFailures(
    new ValidationFailure("Age", "Must be positive"));
```

---

## `Result<T>` — Value-Returning Operations

A `readonly struct` representing success with a `TValue` or failure with an `Error`. Implicit conversions allow assigning values and errors directly.

### Creating

```csharp
// Implicit conversions
Result<User> success = user;
Result<User> failure = Error.NotFound("User not found");

// Factory methods
Result.Success(user);
Result.Failure<User>(error);
Result.Failure<User>("Something went wrong"); // shorthand for Error.Failure(...)
```

### Properties

| Property    | Type    | Description                            |
| ----------- | ------- | -------------------------------------- |
| `IsSuccess` | `bool`  | `true` if the result contains a value  |
| `IsFailure` | `bool`  | `true` if the result contains an error |
| `Value`     | `T`     | The success value (throws if failure)  |
| `Error`     | `Error` | The error (throws if success)          |

### Instance Methods

| Method                             | Returns              | Description                                |
| ---------------------------------- | -------------------- | ------------------------------------------ |
| `Match(onSuccess, onFailure)`      | `TOut`               | Pattern match with return value            |
| `Switch(onSuccess, onFailure)`     | `void`               | Pattern match with side effects            |
| `Map(fn)`                          | `Result<TOut>`       | Transform success value                    |
| `Bind(fn)`                         | `Result<TOut>`       | Chain result-producing function (flatMap)  |
| `Tap(action)`                      | `Result<T>`          | Side effect on success                     |
| `OnSuccess(action)`                | `Result<T>`          | Execute action on success                  |
| `OnFailure(action)`                | `Result<T>`          | Execute action on failure                  |
| `ValueOr(fallback)`                | `T`                  | Get value or fallback                      |
| `ValueOr(fn)`                      | `T`                  | Get value or compute fallback from error   |
| `ToResult()`                       | `Result`             | Discard value, preserve success/failure    |
| `Deconstruct`                      | `(bool, T?, Error?)` | Tuple-style: `var (ok, val, err) = result` |
| `MapAsync(fn)`                     | `Task<Result<TOut>>` | Transform with async function              |
| `BindAsync(fn)`                    | `Task<Result<TOut>>` | Chain with async result-producing function |
| `TapAsync(fn)`                     | `Task<Result<T>>`    | Async side effect on success               |
| `TapErrorAsync(fn)`                | `Task<Result<T>>`    | Async side effect on failure               |
| `MatchAsync(onSuccess, onFailure)` | `Task<TOut>`         | Pattern match with async functions         |

### Extension Methods

| Method                            | Returns     | Description                         |
| --------------------------------- | ----------- | ----------------------------------- |
| `Ensure(predicate, error)`        | `Result<T>` | Validate with predicate             |
| `Ensure(predicate, errorFactory)` | `Result<T>` | Validate with lazy error from value |
| `MapError(fn)`                    | `Result<T>` | Transform the error                 |
| `TapError(action)`                | `Result<T>` | Side effect on failure              |

### Async Pipeline Extensions (on `Task<Result<T>>`)

These allow chaining directly off async operations without `await`:

| Method                         | Returns              | Description                           |
| ------------------------------ | -------------------- | ------------------------------------- |
| `.Map(fn)`                     | `Task<Result<TOut>>` | Transform success value               |
| `.Bind(fn)`                    | `Task<Result<TOut>>` | Chain sync result-producing function  |
| `.BindAsync(fn)`               | `Task<Result<TOut>>` | Chain async result-producing function |
| `.Match(onSuccess, onFailure)` | `Task<TOut>`         | Pattern match                         |
| `.Ensure(predicate, error)`    | `Task<Result<T>>`    | Validate                              |
| `.Ensure(pred, errorFactory)`  | `Task<Result<T>>`    | Validate with lazy error from value   |
| `.Tap(action)`                 | `Task<Result<T>>`    | Side effect on success                |
| `.TapError(action)`            | `Task<Result<T>>`    | Side effect on failure                |

```csharp
var email = await GetUserAsync(id)       // Task<Result<User>>
    .Ensure(u => u.IsActive, Error.Failure("Inactive"))
    .Map(u => u.Email)
    .TapError(e => logger.LogWarning("Failed: {Error}", e));
```

---

## `Result` — Void-Like Operations

A `readonly struct` for operations that succeed or fail but carry no value (e.g., Delete, SendEmail).

### Creating

```csharp
Result success = Result.Success();
Result failure = Result.Failure(Error.NotFound("Item not found"));
Result quick   = Result.Failure("Something went wrong");
Result fromErr = Error.Forbidden("Not allowed"); // implicit conversion
```

### Properties

| Property    | Type    | Description                       |
| ----------- | ------- | --------------------------------- |
| `IsSuccess` | `bool`  | `true` if the operation succeeded |
| `IsFailure` | `bool`  | `true` if the operation failed    |
| `Error`     | `Error` | The error (throws if success)     |

### Instance Methods

| Method                             | Returns      | Description                        |
| ---------------------------------- | ------------ | ---------------------------------- |
| `Match(onSuccess, onFailure)`      | `TOut`       | Pattern match with return value    |
| `Switch(onSuccess, onFailure)`     | `void`       | Pattern match with side effects    |
| `Tap(action)`                      | `Result`          | Side effect on success                    |
| `OnSuccess(action)`                | `Result`          | Execute action on success                 |
| `OnFailure(action)`                | `Result`          | Execute action on failure                 |
| `Map<T>(fn)`                       | `Result<T>`       | Bridge to Result<T> on success            |
| `Bind<T>(fn)`                      | `Result<T>`       | Chain to Result<T> via binder             |
| `Deconstruct`                      | `(bool, Error?)`  | Tuple-style: `var (ok, err) = result`     |
| `TapAsync(fn)`                     | `Task<Result>`    | Async side effect on success              |
| `TapErrorAsync(fn)`                | `Task<Result>`    | Async side effect on failure              |
| `MatchAsync(onSuccess, onFailure)` | `Task<TOut>`      | Pattern match with async functions        |

### Extension Methods

| Method                     | Returns  | Description              |
| -------------------------- | -------- | ------------------------ |
| `Ensure(predicate, error)` | `Result` | Validate with predicate  |
| `MapError(fn)`             | `Result` | Transform the error      |
| `TapError(action)`         | `Result` | Side effect on failure   |

### Async Pipeline Extensions (on `Task<Result>`)

| Method                         | Returns        | Description            |
| ------------------------------ | -------------- | ---------------------- |
| `.Match(onSuccess, onFailure)` | `Task<TOut>`      | Pattern match                    |
| `.Ensure(predicate, error)`    | `Task<Result>`    | Validate with predicate          |
| `.Map<T>(fn)`                  | `Task<Result<T>>` | Bridge to Result<T> on success   |
| `.Bind<T>(fn)`                 | `Task<Result<T>>` | Chain to Result<T> via binder    |
| `.Tap(action)`                 | `Task<Result>`    | Side effect on success           |
| `.TapError(action)`            | `Task<Result>`    | Side effect on failure           |

```csharp
await Result.TryAsync(() => emailService.SendAsync(message))
    .Tap(() => logger.LogInformation("Email sent"))
    .TapError(e => logger.LogError("Send failed: {Error}", e));
```

---

## Static Factory & Utility Methods

All accessible via `Result.*`:

| Method                                | Returns              | Description                                    |
| ------------------------------------- | -------------------- | ---------------------------------------------- |
| `Result.Success()`                    | `Result`             | Non-generic success                            |
| `Result.Failure(error)`               | `Result`             | Non-generic failure from error                 |
| `Result.Failure(description)`         | `Result`             | Non-generic failure from string                |
| `Result.Success<T>(value)`            | `Result<T>`          | Generic success                                |
| `Result.Failure<T>(error)`            | `Result<T>`          | Generic failure from error                     |
| `Result.Failure<T>(description)`      | `Result<T>`          | Generic failure from string                    |
| `Result.Try(action)`                  | `Result`             | Wrap void action in try/catch                  |
| `Result.Try<T>(func)`                 | `Result<T>`          | Wrap function in try/catch                     |
| `Result.TryAsync(func)`               | `Task<Result>`       | Wrap async action in try/catch                 |
| `Result.TryAsync<T>(func)`            | `Task<Result<T>>`    | Wrap async function in try/catch               |
| `Result.FromNullable<T>(value, msg)`  | `Result<T>`          | Null → NotFound failure (reference types)      |
| `Result.FromNullable<T>(value?, msg)` | `Result<T>`          | Null → NotFound failure (nullable value types) |
| `Result.Combine(r1, r2)`              | `Result<(T1, T2)>`        | Combine 2 results into tuple                   |
| `Result.Combine(r1, r2, r3)`          | `Result<(T1,T2,T3)>`      | Combine 3 results into tuple                   |
| `Result.Combine(r1, r2, r3, r4)`      | `Result<(T1,T2,T3,T4)>`   | Combine 4 results into tuple                   |
| `Result.Combine(r1, r2, r3, r4, r5)`  | `Result<(T1,...,T5)>`      | Combine 5 results into tuple                   |

### Collection Extensions

| Method              | Returns                    | Description                             |
| ------------------- | -------------------------- | --------------------------------------- |
| `results.Collect()` | `Result<IReadOnlyList<T>>` | Aggregate sequence; fail on first error |

---

## Real-World Examples

### Service Method

```csharp
public Result<User> CreateUser(CreateUserRequest request)
{
    return ValidateRequest(request)
        .Bind(req => CheckDuplicateEmail(req.Email))
        .Map(email => new User(request.Name, email))
        .Bind(user => repository.Save(user))
        .Tap(user => eventBus.Publish(new UserCreatedEvent(user.Id)));
}
```

### Void-Like Service Method

```csharp
public async Task<Result> DeleteOrderAsync(int orderId)
{
    return await FindOrder(orderId)
        .Map(order => order.Id)
        .BindAsync(id => repository.DeleteAsync(id))
        .Tap(() => logger.LogInformation("Deleted order {Id}", orderId))
        .TapError(e => logger.LogWarning("Delete failed: {Error}", e));
}
```

### API Controller

```csharp
[HttpPost]
public IActionResult Create(CreateUserRequest request)
{
    return userService.CreateUser(request).Match(
        onSuccess: user => CreatedAtAction(nameof(Get), new { id = user.Id }, user),
        onFailure: error => error.Code.Value switch
        {
            "General.Validation" => BadRequest(error),
            "General.Conflict"   => Conflict(error),
            "General.NotFound"   => NotFound(error),
            _                    => StatusCode(500, error)
        });
}
```

### Combining Results

```csharp
var combined = Result.Combine(
    GetUser(userId),
    GetOrder(orderId),
    GetPayment(paymentId));

return combined.Map(tuple =>
{
    var (user, order, payment) = tuple;
    return new OrderSummary(user.Name, order.Total, payment.Status);
});

// 4 & 5 result overloads available too
var all = Result.Combine(name, email, age, address);
var full = Result.Combine(name, email, age, address, phone);
```

### Deconstructing Results

```csharp
// Non-generic Result
var (ok, error) = Result.Success();
// ok = true, error = null

// Generic Result<T>
var (isSuccess, value, err) = Result.Success(42);
// isSuccess = true, value = 42, err = null
```

### Bridging Result ↔ Result&lt;T&gt;

```csharp
// Result → Result<T> via Map/Bind
Result deleted = DeleteOrder(id);
Result<string> message = deleted.Map(() => "Order deleted");
Result<Order> order = deleted.Bind(() => LoadOrder(id));

// Result<T> → Result via ToResult  (discards value)
Result<User> userResult = GetUser(id);
Result plain = userResult.ToResult();
```

### Error Enrichment

```csharp
var error = Error.NotFound("Order not found")
    .WithMetadata("OrderId", orderId)
    .WithInnerError(originalError);

// Validation with rich details
var validation = Error.Validation(
    new ValidationFailure("Email", "Required"),
    new ValidationFailure("Age", "Must be 18 or older"));

validation.Failures;                     // ImmutableArray<ValidationFailure>
validation.HasFailureForField("Email");  // true

// Combine validation errors
var merged = validation.AddFailures(
    new ValidationFailure("Name", "Too long"));
```

### Collecting Sequences

```csharp
var results = ids.Select(id => ParseId(id)); // IEnumerable<Result<int>>

Result<IReadOnlyList<int>> all = results.Collect();
// Success with all values, or first error encountered
```

### Converting from Exceptions

```csharp
// Sync — value-returning
var parsed = Result.Try(() => int.Parse(userInput));

// Sync — void
var written = Result.Try(() => File.WriteAllText(path, content));

// Async — value-returning
var data = await Result.TryAsync(() => httpClient.GetStringAsync(url));

// Async — void
var sent = await Result.TryAsync(() => emailService.SendAsync(msg));
```

### Nullable Bridging

```csharp
// Reference type
Result<User> user = Result.FromNullable(
    repository.FindById(id),
    "User not found");

// Nullable value type
Result<int> count = Result.FromNullable(
    GetOptionalCount(),  // int?
    "Count unavailable");
```

---

## Samples

### StudentScoreApi

A sample ASP.NET Core Web API application demonstrates how to use `Clywell.Primitives` in a real-world scenario, implementing the Result pattern in services and controllers to handle success and failure flows gracefully.

Location: [samples/StudentScoreApi](samples/StudentScoreApi)

To run the sample:
1. Navigate to `samples/StudentScoreApi`
2. Run `dotnet run`
3. Use the provided `demo_result.ps1` PowerShell script to test success and failure scenarios against the API.

### SampleApp

A simple console application demonstrating basic usage of `Result` types, chaining, and error handling.

Location: [samples/SampleApp](samples/SampleApp)

---

## Documentation

- [Getting Started](docs/getting-started.md) — Full setup and usage guide
- [Quick Reference](QUICK_REFERENCE.md) — Cheat sheet for common operations

---

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Commit changes: `git commit -m 'feat: add my feature'`
4. Push to branch: `git push origin feature/my-feature`
5. Create a Pull Request

### Commit Convention

Follow [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` — New feature
- `fix:` — Bug fix
- `test:` — Test additions/changes
- `docs:` — Documentation only
- `chore:` — Build/tooling changes

---

## License

[MIT](LICENSE) © 2026 Clywell
