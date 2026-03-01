using Clywell.Primitives;
using ScoreTracker.Models;

namespace ScoreTracker.Services;



public class StudentService
{
    private readonly List<Student> _students = new();

    public StudentService()
    {

        var classes = new[] { "JSS 1", "JSS 2", "SS 1", "SS 2" };

        var students = new[]
        {
            new Student { Name = "Alice Johnson",   Class = "JSS 1" },
            new Student { Name = "Bob Smith",        Class = "JSS 1" },
            new Student { Name = "Chisom Obi",       Class = "JSS 2" },
            new Student { Name = "David Eze",         Class = "JSS 2" },
            new Student { Name = "Emeka Nwosu",      Class = "SS 1"  },
            new Student { Name = "Fatima Bello",     Class = "SS 1"  },
            new Student { Name = "Grace Adeyemi",    Class = "SS 2"  },
            new Student { Name = "Henry Williams",   Class = "SS 2"  },
        };

        var subjects = new[] { "Mathematics", "English", "Science", "History", "Fine Art" };
        var random   = new Random(42); 

        foreach (var student in students)
        {
            foreach (var subject in subjects)
            {
                student.Scores.Add(new Score
                {
                    Subject = subject,
                    Value   = random.Next(40, 101)   
                });
            }

            _students.Add(student);
        }
    }

 
    public Result<List<Student>> GetAll()
    {
       
        return Result.Success(_students);
    }

    

    public Result<Student> GetById(Guid id)
    {
        var student = _students.FirstOrDefault(s => s.Id == id);

        
        if (student is not null)
        {
            return Result.Success(student);
        }

        
        return Error.NotFound($"No student with ID '{id}' was found.");
    }

    

    public Result<Student> GetByName(string name)
    {
        var student = _students.FirstOrDefault(
            s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (student is not null)
        {
            return Result.Success(student);
        }

        return Error.NotFound($"No student named '{name}' was found.");
    }

    

    public Result<List<Student>> GetByClass(string className)
    {
        var list = _students
            .Where(s => s.Class.Equals(className, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (list.Count > 0)
        {
            return Result.Success(list);
        }

        return Error.NotFound($"No students found in class '{className}'.");
    }

    

    public Result<Student> AddStudent(Student newStudent)
    {
    
        if (string.IsNullOrWhiteSpace(newStudent.Name))
        {
            return Error.Validation("Name", "Student name cannot be empty.");
        }

        
        if (string.IsNullOrWhiteSpace(newStudent.Class))
        {
            return Error.Validation("Class", "Class cannot be empty.");
        }

        
        bool alreadyExists = _students.Any(
            s => s.Name.Equals(newStudent.Name, StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            return Error.Conflict($"A student named '{newStudent.Name}' already exists.");
        }

        
        newStudent.Id = Guid.NewGuid();

        _students.Add(newStudent);
        return Result.Success(newStudent);
    }
}
