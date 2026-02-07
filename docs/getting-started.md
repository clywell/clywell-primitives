# Getting Started with Clywell.Primitives

This guide will walk you through installing, configuring, and using `Clywell.Primitives` — the Result pattern and error handling library for .NET applications.

---

## Prerequisites

- .NET 10.0 SDK or later
- Any IDE (Visual Studio, VS Code, Rider)

---

## Installation

```bash
dotnet add package Clywell.Primitives
```

---

## Core Concepts

### The Result Pattern

Instead of throwing exceptions for expected failures, the Result pattern encodes success and failure as explicit return values:

```csharp
// ❌ Exception-based (traditional)
public User GetUser(int id)
{
    var user = db.Find(id);
    if (user is null)
        throw new NotFoundException($"User {id} not found");
    return user;
}

// ✅ Result-based (Clywell.Primitives)
public Result<User> GetUser(int id)
{
    var user = db.Find(id);
    return Result.FromNullable(user, $"User {id} not found");
}
```

**Benefits:**

- Compile-time safety — callers are forced to handle both cases
- No exception overhead for expected failures
- Composable pipelines with `Map`, `Bind`, `Match`
- Self-documenting method signatures

---

## Creating Results

### Success

```csharp
using Clywell.Primitives;

// Implicit conversion (preferred — cleanest syntax)
Result<int> result = 42;

// Factory method
var result = Result.Success(42);

// Returning from a method
public Result<User> GetUser(int id)
{
    var user = repository.FindById(id);
    return user; // implicit conversion from User to Result<User>
}
```

### Failure

```csharp
// Implicit conversion from Error
Result<int> result = Error.NotFound("Item not found");

// Factory method with Error
var result = Result.Failure<int>(Error.Conflict("Already exists"));

// Factory method with string (creates General.Failure error)
var result = Result.Failure<int>("Something went wrong");

// Returning from method
public Result<User> GetUser(int id)
{
    return Error.NotFound($"User {id} not found");
}
```

### Non-Generic Result (Void-Like Operations)

For operations that succeed or fail but carry no return value (e.g., Delete, SendEmail), use the non-generic `Result`:

```csharp
// Factory methods
Result success = Result.Success();
Result failure = Result.Failure(Error.NotFound("Item not found"));
Result quick   = Result.Failure("Something went wrong"); // shorthand

// Implicit conversion from Error
Result fromError = Error.Forbidden("Not allowed");

// Returning from a method
public Result DeleteUser(int id)
{
    var user = repository.FindById(id);
    if (user is null)
        return Error.NotFound($"User {id} not found");

    repository.Delete(user);
    return Result.Success();
}
```

Non-generic `Result` supports the same operations as `Result<T>` (except value-dependent ones like `Map`/`Bind`/`ValueOr`):

```csharp
var result = Result.Success();

// Pattern matching
var message = result.Match(
    onSuccess: () => "Done",
    onFailure: error => $"Failed: {error.Description}");

result.Switch(
    onSuccess: () => Console.WriteLine("Completed"),
    onFailure: error => Console.WriteLine($"Error: {error}"));

// Side effects
result
    .Tap(() => logger.LogInformation("Operation succeeded"))
    .OnSuccess(() => metrics.Increment("ops.success"))
    .OnFailure(error => metrics.Increment("ops.failure"));

// Error transformation
result
    .MapError(e => Error.Unexpected(e.Description))
    .TapError(e => logger.LogWarning("Error: {Error}", e));

// Async match
var output = await result.MatchAsync(
    onSuccess: () => Task.FromResult("OK"),
    onFailure: error => SendAlertAsync(error));
```

---

## Working with Errors

### Error Codes

Built-in error codes cover common scenarios:

```csharp
Error.Failure("General failure")          // ErrorCode.Failure
Error.NotFound("Resource not found")      // ErrorCode.NotFound
Error.Conflict("Already exists")          // ErrorCode.Conflict
Error.Unauthorized("Bad credentials")     // ErrorCode.Unauthorized
Error.Forbidden("No permission")          // ErrorCode.Forbidden
Error.Unexpected("Internal error")        // ErrorCode.Unexpected
Error.Unavailable("Service down")         // ErrorCode.Unavailable
```

### Custom Error Codes

```csharp
ErrorCode myCode = "Payment.InsufficientFunds";
var error = new Error(myCode, "Insufficient balance for this transaction");
```

### Validation Errors

```csharp
// Single field
var error = Error.Validation("Email", "Email is required");

// Multiple fields
var error = Error.Validation(
    new ValidationFailure("Email", "Email is required"),
    new ValidationFailure("Password", "Must be at least 8 characters"),
    new ValidationFailure("Age", "Must be 18 or older"));

// Query validation failures
if (error is ValidationError ve)
{
    var emailErrors = ve.GetFailuresForField("Email");
    bool hasAgeError = ve.HasFailureForField("Age");
    int totalErrors = ve.FailureCount;
}

// Append additional failures (returns a new instance)
var expanded = error.AddFailures(
    new ValidationFailure("Phone", "Invalid format"));
```

### Error Metadata

```csharp
var error = Error.NotFound("Order not found")
    .WithMetadata("OrderId", "ORD-123")
    .WithMetadata("RequestedBy", "user@example.com");

// Chain inner errors
var inner = Error.Unexpected("Database timeout");
var outer = Error.Unavailable("Failed to fetch order")
    .WithInnerError(inner);
```

---

## Consuming Results

### Pattern Matching

The safest way to consume a result:

```csharp
// Match — returns a value
var message = result.Match(
    onSuccess: user => $"Welcome, {user.Name}!",
    onFailure: error => $"Error: {error.Description}");

// Switch — side effects only
result.Switch(
    onSuccess: user => SendWelcomeEmail(user),
    onFailure: error => logger.LogError("{Error}", error));
```

### Direct Property Access

```csharp
if (result.IsSuccess)
{
    var value = result.Value; // safe here
}

if (result.IsFailure)
{
    var error = result.Error; // safe here
}

// ⚠️  Throws InvalidOperationException if wrong state
// var value = failedResult.Value; // throws!
```

### Fallback Values

```csharp
// Static fallback
var value = result.ValueOr(0);

// Dynamic fallback based on error
var value = result.ValueOr(error =>
    error.Code == ErrorCode.NotFound ? GetDefault() : throw new Exception(error.Description));
```

### Deconstruct — Tuple-Style Consumption

```csharp
// Non-generic Result
var (ok, error) = Result.Success();
// ok = true, error = null

var (ok, error) = Result.Failure(Error.NotFound("Missing"));
// ok = false, error = Error { Code = NotFound, ... }

// Generic Result<T>
var (isSuccess, value, err) = Result.Success(42);
// isSuccess = true, value = 42, err = null

var (isSuccess, value, err) = Result.Failure<int>(Error.NotFound("Missing"));
// isSuccess = false, value = 0, err = Error { ... }
```

---

## Railway-Oriented Programming

Chain operations that short-circuit on failure:

### Map — Transform the Success Value

```csharp
var result = Result.Success("42")
    .Map(int.Parse)           // "42" → 42
    .Map(v => v * 10);        // 42 → 420

// If any step fails, subsequent Maps are skipped
```

### Bind — Chain Result-Producing Functions

```csharp
public Result<User> GetUser(int id) => ...;
public Result<Order> GetLatestOrder(User user) => ...;

var result = Result.Success(userId)
    .Bind(id => GetUser(id))           // int → Result<User>
    .Bind(user => GetLatestOrder(user)); // User → Result<Order>
```

### Ensure — Validate with Predicates

```csharp
var result = Result.Success(order)
    .Ensure(o => o.Items.Any(), Error.Validation("Items", "Order must have items"))
    .Ensure(o => o.Total > 0, Error.Validation("Total", "Must be positive"))
    .Ensure(o => o.Total <= 10000, Error.Validation("Total", "Exceeds maximum"));
```

Use the lazy error factory overload when the error depends on the value:

```csharp
var result = Result.Success(user)
    .Ensure(
        u => u.Age >= 18,
        u => Error.Validation("Age", $"{u.Name} is only {u.Age} — must be 18+"));
```

### Tap / OnSuccess / OnFailure — Side Effects

```csharp
var result = GetUser(id)
    .Tap(user => logger.LogInformation("Found user {Name}", user.Name))
    .OnSuccess(user => metrics.IncrementCounter("user.found"))
    .OnFailure(error => metrics.IncrementCounter("user.not_found"));
```

### MapError / TapError — Error Handling

```csharp
var result = externalService.GetData()
    .MapError(e => Error.Unavailable($"External service: {e.Description}"))
    .TapError(e => logger.LogWarning("Service error: {Error}", e));
```

### Ensure on Non-Generic Result

The non-generic `Result` also supports `Ensure` for predicate validation:

```csharp
var result = Result.Success()
    .Ensure(() => userExists, Error.NotFound("User not found"))
    .Ensure(() => hasPermission, Error.Forbidden("Not allowed"));
```

### Bridging Result ↔ Result&lt;T&gt;

#### Result → Result&lt;T&gt; via Map / Bind

```csharp
Result deleted = DeleteOrder(id);

// Map: produce a value on success
Result<string> message = deleted.Map(() => "Order deleted");

// Bind: chain to a result-producing function
Result<Order> order = deleted.Bind(() => LoadReplacementOrder(id));
```

#### Result&lt;T&gt; → Result via ToResult

```csharp
// Discard the value, keep success/failure state
Result<User> userResult = GetUser(id);
Result plain = userResult.ToResult();
```

Both `Map<T>` and `Bind<T>` on `Result`, and `ToResult()` on `Result<T>`, also work in async pipelines on `Task<Result>` and `Task<Result<T>>`.

---

## Async Pipelines

### Pipeline Extensions on `Task<Result<T>>`

Chain operations directly off async calls without intermediate `await`:

```csharp
var result = await Result.TryAsync(() => httpClient.GetStringAsync(url))
    .Map(json => JsonSerializer.Deserialize<Data>(json)!)
    .Ensure(data => data.IsValid, Error.Validation("Data", "Invalid response"))
    .BindAsync(data => SaveToDatabase(data))
    .Tap(saved => logger.LogInformation("Saved: {Id}", saved.Id))
    .TapError(error => logger.LogError("Pipeline failed: {Error}", error));
```

Available pipeline extensions on `Task<Result<T>>`: `Map`, `Bind`, `BindAsync`, `Match`, `Ensure` (with static error and lazy factory), `Tap`, `TapError`.

### Pipeline Extensions on `Task<Result>`

Non-generic `Task<Result>` has its own async pipeline:

```csharp
await Result.TryAsync(() => emailService.SendAsync(message))
    .Match(
        onSuccess: () => "Sent",
        onFailure: error => $"Failed: {error.Description}")
```

```csharp
await Result.TryAsync(() => repository.DeleteAsync(id))
    .Tap(() => logger.LogInformation("Deleted {Id}", id))
    .TapError(e => logger.LogWarning("Delete failed: {Error}", e));
```

Available pipeline extensions on `Task<Result>`: `Match`, `Ensure`, `Map<T>`, `Bind<T>`, `Tap`, `TapError`.

### Async Side Effects — TapAsync / TapErrorAsync

Both `Result<T>` and `Result` have `TapAsync` and `TapErrorAsync` instance methods for async side effects:

```csharp
// On Result<T>
var result = await Result.Success(user)
    .TapAsync(async u => await SendWelcomeEmailAsync(u))
    .TapErrorAsync(async e => await LogErrorAsync(e));

// On non-generic Result
var result = await Result.Success()
    .TapAsync(async () => await NotifyAsync())
    .TapErrorAsync(async e => await AlertAsync(e));
```

### Async Instance Methods

`Result<T>` also has async instance methods when you already have a result and need async handlers:

```csharp
// MapAsync — transform with an async function
var result = Result.Success(userId)
    .MapAsync(id => repository.FindByIdAsync(id));  // Task<Result<User>>

// BindAsync — chain with an async result-producing function
var result = Result.Success(user)
    .BindAsync(u => ValidateAndSaveAsync(u));       // Task<Result<User>>

// MatchAsync — pattern match with async handlers
var message = await result.MatchAsync(
    onSuccess: user => FormatAsync(user),
    onFailure: error => LogAndMessageAsync(error));
```

The non-generic `Result` also has `MatchAsync`.

---

## Combining Results

### Combine — Multiple Results into Tuple

```csharp
// Combine 2 results
var result = Result.Combine(
    GetUser(userId),
    GetOrder(orderId));

// result is Result<(User, Order)>
result.Match(
    onSuccess: tuple => $"{tuple.Item1.Name}'s order: {tuple.Item2.Total}",
    onFailure: error => error.Description);

// Combine 3 results
var summary = Result.Combine(
    GetUser(userId),
    GetOrder(orderId),
    GetPayment(paymentId))
    .Map(t => new OrderSummary(t.Item1.Name, t.Item2.Total, t.Item3.Status));

// Combine 4 and 5 results — same pattern
var combo4 = Result.Combine(name, email, age, address);
var combo5 = Result.Combine(name, email, age, address, phone);
```

### Collect — Sequence of Results into Single Result

```csharp
var ids = new[] { 1, 2, 3 };
var results = ids.Select(id => GetUser(id));
var collected = results.Collect();

// collected is Result<IReadOnlyList<User>>
```

---

## Try/Catch Wrapping

Convert exception-based APIs to Result-based:

```csharp
// Synchronous — value-returning
var result = Result.Try(() => int.Parse(userInput));
var result = Result.Try(() => File.ReadAllText(path));

// Synchronous — void (returns non-generic Result)
var result = Result.Try(() => File.Delete(tempPath));

// Asynchronous — value-returning
var result = await Result.TryAsync(() => httpClient.GetStringAsync(url));

// Asynchronous — void (returns Task<Result>)
var result = await Result.TryAsync(() => emailService.SendAsync(message));

// On failure, the error includes:
// - Code: ErrorCode.Unexpected
// - Description: exception message
// - Metadata["ExceptionType"]: full exception type name
```

---

## FromNullable — Bridging Nullable Values

Convert nullable values to Results instead of manual null checks:

```csharp
// Reference types
Result<User> user = Result.FromNullable(
    repository.FindById(id),
    "User not found");

// Nullable value types (int?, DateTime?, etc.)
Result<int> count = Result.FromNullable(
    GetOptionalCount(),   // int?
    "Count unavailable");

// Default message is "Value was null." if omitted
Result<User> user = Result.FromNullable(repository.FindById(id));
```

The resulting failure uses `ErrorCode.NotFound`.

---

## Testing

### Asserting Results

```csharp
[Fact]
public void CreateUser_ValidInput_ShouldReturnSuccess()
{
    var result = service.CreateUser(validRequest);

    Assert.True(result.IsSuccess);
    Assert.Equal("John", result.Value.Name);
}

[Fact]
public void CreateUser_DuplicateEmail_ShouldReturnConflict()
{
    var result = service.CreateUser(duplicateRequest);

    Assert.True(result.IsFailure);
    Assert.Equal(ErrorCode.Conflict, result.Error.Code);
}

[Fact]
public void CreateUser_InvalidInput_ShouldReturnValidationError()
{
    var result = service.CreateUser(invalidRequest);

    Assert.True(result.IsFailure);
    var validationError = Assert.IsType<ValidationError>(result.Error);
    Assert.True(validationError.HasFailureForField("Email"));
}
```

---

## Best Practices

1. **Use implicit conversions** — `return user;` and `return Error.NotFound(...)` are cleaner than factory methods
2. **Prefer `Match`** over direct property access — it forces handling both cases
3. **Use `Ensure`** for validation chains — more readable than nested if/else
4. **Use `Tap`** for logging/telemetry — keeps the pipeline clean
5. **Use `Error.Validation`** for input validation — provides structured field-level errors
6. **Keep error codes consistent** — use built-in codes or define domain-specific ones
7. **Add metadata** to errors for debugging — `WithMetadata("EntityId", id)`
8. **Don't use Result for programmer errors** — null arguments, index out of range, etc. should still throw

---

## Troubleshooting

### "Cannot access Value on a failed Result"

You're accessing `.Value` on a failed result. Use `.Match()` or check `.IsSuccess` first:

```csharp
// ❌ Unsafe
var name = result.Value.Name;

// ✅ Safe
var name = result.Match(
    onSuccess: u => u.Name,
    onFailure: _ => "Unknown");
```

### "Cannot access Error on a successful Result"

Same issue, reversed. Check `.IsFailure` before accessing `.Error`.

### Implicit conversion not working

Ensure the value type matches `T` in `Result<T>`. The implicit conversion won't work with interface/base class mismatches.
