using Clywell.Primitives;

Console.WriteLine("--- Clywell.Primitives Test ---");

// 1. Result Creation
var success = Result.Success(42);
Console.WriteLine($"Success Result: IsSuccess={success.IsSuccess}, Value={success.Value}");

var failure = Result.Failure<int>(Error.NotFound("Value not found"));
Console.WriteLine($"Failure Result: IsFailure={failure.IsFailure}, Error={failure.Error.Description}");

// 2. Chaining
Console.WriteLine("\n--- Pipeline Test ---");
var pipeline = Result.Success("100")
    .Map(int.Parse)
    .Ensure(x => x > 50, Error.Validation("Value", "Must be > 50"))
    .Tap(x => Console.WriteLine($"Pipeline Step: Parsed {x}"))
    .Bind(x => Result.Success(x * 2))
    .Tap(x => Console.WriteLine($"Pipeline Step: Doubled to {x}"));

pipeline.Switch(
    val => Console.WriteLine($"Pipeline Final: {val}"),
    err => Console.WriteLine($"Pipeline Error: {err.Description}")
);

// 3. Error Handling with Try
Console.WriteLine("\n--- Exception Handling Test ---");
var handled = Result.Try(() => int.Parse("invalid"))
    .TapError(e => Console.WriteLine($"Caught exception: {e.Description}"));

// 4. Void Result
Console.WriteLine("\n--- Void Result Test ---");
var voidResult = Result.Success();
voidResult.Switch(
    () => Console.WriteLine("Void Success"),
    e => Console.WriteLine($"Void Error: {e}")
);

Console.WriteLine("\nTest Completed.");
