using Clywell.Primitives;
using StudentScoreApi.Models;

namespace StudentScoreApi.Services;

public interface IStudentService
{
    public Result<Student> GetStudent(Guid id);
    public Result<Student> CreateStudent(string name);
    public Result<Student> AddScore(Guid studentId, int score);
    public Result<List<Student>> GetAllStudents();
}

public class StudentService : IStudentService
{
    private readonly List<Student> _students = new();

    public Result<Student> GetStudent(Guid id)
    {
        var student = _students.FirstOrDefault(s => s.Id == id);
        return student is not null 
            ? Result.Success(student) 
            : Result.Failure<Student>(Error.NotFound($"Student with ID {id} not found."));
    }

    public Result<Student> CreateStudent(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Student>(Error.Validation("Student.NameEmpty", "Name cannot be empty."));
        }

        var student = new Student(Guid.NewGuid(), name);
        _students.Add(student);
        return Result.Success(student);
    }

    public Result<Student> AddScore(Guid studentId, int score)
    {
        if (score < 0 || score > 100)
        {
            return Result.Failure<Student>(Error.Validation("Score.Invalid", "Score must be between 0 and 100."));
        }

        return GetStudent(studentId)
            .Tap(s => s.Scores.Add(score));
    }

    public Result<List<Student>> GetAllStudents()
    {
        return Result.Success(_students);
    }
}
