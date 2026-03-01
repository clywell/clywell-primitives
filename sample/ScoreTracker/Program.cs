using Clywell.Primitives;
using ScoreTracker.Models;
using ScoreTracker.Services;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<StudentService>();

var app = builder.Build();

app.MapGet("/api/students", (StudentService svc) =>
{
    Result<List<Student>> result = svc.GetAll();

    return result.Match(
        onSuccess: students => Results.Ok(students),
        onFailure: error    => Results.Problem(error.Description)
    );
});


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


app.Run();
