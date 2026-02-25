using Clywell.Primitives;
using ScoreTracker.Models;
using ScoreTracker.Services;

// ─────────────────────────────────────────────────────────────────────────────
// ScoreTracker — Test App for the Clywell.Primitives package
//
// This app demonstrates how Clywell.Primitives gives you clear success/failure
// feedback when fetching student records.
//
// Every service call returns a Result<T>.  We then call .Match() on it:
//
//   result.Match(
//       onSuccess: data  => Results.Ok(data),            ← 200 OK, here is the data
//       onFailure: error => Results.NotFound(new {...})   ← 404, here is why it failed
//   );
//
// No null checks, no try/catch — the package forces us to handle both cases.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// Register our student service as a singleton
// (one shared instance for the whole app lifetime)
builder.Services.AddSingleton<StudentService>();

var app = builder.Build();

// ─── Endpoints ───────────────────────────────────────────────────────────────

// 1. GET /api/students
//    Returns the full list of all students.
//    Always succeeds — we just return whatever is in the list.
app.MapGet("/api/students", (StudentService svc) =>
{
    Result<List<Student>> result = svc.GetAll();

    return result.Match(
        onSuccess: students => Results.Ok(students),
        onFailure: error    => Results.Problem(error.Description)
    );
});

// 2. GET /api/students/{id}
//    Returns one student by their GUID.
//    FAIL → 404 with error message if ID does not exist.
app.MapGet("/api/students/{id:guid}", (Guid id, StudentService svc) =>
{
    Result<Student> result = svc.GetById(id);

    return result.Match(
        onSuccess: student => Results.Ok(student),
        onFailure: error   => Results.NotFound(new
        {
            feedback = "FAIL",
            code     = error.Code.Value,
            message  = error.Description
        })
    );
});

// 3. GET /api/students/by-name/{name}
//    Returns one student by their full name (case-insensitive).
//    FAIL → 404 if name does not match anyone.
app.MapGet("/api/students/by-name/{name}", (string name, StudentService svc) =>
{
    Result<Student> result = svc.GetByName(name);

    return result.Match(
        onSuccess: student => Results.Ok(student),
        onFailure: error   => Results.NotFound(new
        {
            feedback = "FAIL",
            code     = error.Code.Value,
            message  = error.Description
        })
    );
});

// 4. GET /api/students/by-class/{className}
//    Returns all students in a given class (e.g. "JSS 1", "SS 2").
//    FAIL → 404 if the class name does not match any student.
app.MapGet("/api/students/by-class/{className}", (string className, StudentService svc) =>
{
    Result<List<Student>> result = svc.GetByClass(className);

    return result.Match(
        onSuccess: students => Results.Ok(students),
        onFailure: error    => Results.NotFound(new
        {
            feedback = "FAIL",
            code     = error.Code.Value,
            message  = error.Description
        })
    );
});

// 5. POST /api/students
//    Adds a new student.
//    FAIL → 400 Bad Request if name or class is missing, or name already exists.
app.MapPost("/api/students", (Student newStudent, StudentService svc) =>
{
    Result<Student> result = svc.AddStudent(newStudent);

    return result.Match(
        onSuccess: student => Results.Created($"/api/students/{student.Id}", new
        {
            feedback = "SUCCESS",
            message  = $"Student '{student.Name}' was added successfully.",
            data     = student
        }),
        onFailure: error   => Results.BadRequest(new
        {
            feedback = "FAIL",
            code     = error.Code.Value,
            message  = error.Description
        })
    );
});

// ─────────────────────────────────────────────────────────────────────────────

app.Run();
