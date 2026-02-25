namespace ScoreTracker.Models;

// Holds all the information we keep for one student
public class Student
{
    public Guid   Id       { get; set; } = Guid.NewGuid();
    public string Name     { get; set; } = string.Empty;
    public string Class    { get; set; } = string.Empty;   // e.g. "JSS 1", "SS 2"
    public List<Score> Scores { get; set; } = new();
}
