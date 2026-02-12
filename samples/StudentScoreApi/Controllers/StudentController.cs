using Clywell.Primitives;
using Microsoft.AspNetCore.Mvc;
using StudentScoreApi.Models;
using StudentScoreApi.Services;

namespace StudentScoreApi.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetStudent(Guid id)
    {
        var result = _studentService.GetStudent(id);
        return result.Match<IActionResult>(
            onSuccess: Ok,
            onFailure: error => NotFound(error));
    }

    [HttpPost]
    public IActionResult CreateStudent([FromBody] string name)
    {
        var result = _studentService.CreateStudent(name);
        return result.Match<IActionResult>(
            onSuccess: student => CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student),
            onFailure: error => BadRequest(error));
    }

    [HttpPost("{id:guid}/scores")]
    public IActionResult AddScore(Guid id, [FromBody] int score)
    {
        var result = _studentService.AddScore(id, score);
        return result.Match<IActionResult>(
            onSuccess: Ok,
            onFailure: error => error.Code == "Student.NotFound" ? NotFound(error) : BadRequest(error));
    }

    [HttpGet]
    public IActionResult GetAllStudents()
    {
        var result = _studentService.GetAllStudents();
        return result.Match<IActionResult>(
            onSuccess: Ok,
            onFailure: error => BadRequest(error));
    }
}
