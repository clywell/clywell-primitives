using Clywell.Primitives;
using ScoreTracker.Models;

namespace ScoreTracker.Services;

// ─────────────────────────────────────────────────────────────────────────────
// StudentService
//
// This class keeps one in-memory list of students.
// Every method returns a Result<T> from the Clywell.Primitives package.
//
//   Result.Success(value)         ← everything went well, here is the data
//   Error.NotFound("message")     ← something went wrong, here is why
//
// The caller (Program.cs) then uses .Match() to turn the result into an
// HTTP response without any if/else checks.
// ─────────────────────────────────────────────────────────────────────────────

public class StudentService
{
    private readonly List<Student> _students = new();

    public StudentService()
    {
        // --- Seed some students so the app works straight away ---

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
        var random   = new Random(42); // fixed seed so IDs stay predictable on restart

        foreach (var student in students)
        {
            foreach (var subject in subjects)
            {
                student.Scores.Add(new Score
                {
                    Subject = subject,
                    Value   = random.Next(40, 101)   // score between 40 – 100
                });
            }

            _students.Add(student);
        }
    }

    // ─── GET all students ────────────────────────────────────────────────────

    public Result<List<Student>> GetAll()
    {
        // Always succeeds — we always have a list (even if empty)
        return Result.Success(_students);
    }

    // ─── GET one student by their GUID id ───────────────────────────────────

    public Result<Student> GetById(Guid id)
    {
        var student = _students.FirstOrDefault(s => s.Id == id);

        // If we found the student → success
        if (student is not null)
        {
            return Result.Success(student);
        }

        // If we did NOT find the student → failure with a "not found" error
        return Error.NotFound($"No student with ID '{id}' was found.");
    }

    // ─── GET one student by name (case-insensitive) ──────────────────────────

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

    // ─── GET all students in a class ────────────────────────────────────────

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

    // ─── POST — add a brand-new student ─────────────────────────────────────

    public Result<Student> AddStudent(Student newStudent)
    {
        // Basic validation — name must not be blank
        if (string.IsNullOrWhiteSpace(newStudent.Name))
        {
            return Error.Validation("Name", "Student name cannot be empty.");
        }

        // Basic validation — class must not be blank
        if (string.IsNullOrWhiteSpace(newStudent.Class))
        {
            return Error.Validation("Class", "Class cannot be empty.");
        }

        // Make sure the name is not already taken
        bool alreadyExists = _students.Any(
            s => s.Name.Equals(newStudent.Name, StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            return Error.Conflict($"A student named '{newStudent.Name}' already exists.");
        }

        // Assign a fresh ID (even if the caller sent one, we create our own)
        newStudent.Id = Guid.NewGuid();

        _students.Add(newStudent);
        return Result.Success(newStudent);
    }
}
