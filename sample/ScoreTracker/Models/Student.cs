namespace ScoreTracker.Models;

public class Student
{
    public Guid   Id       { get; set; } = Guid.NewGuid();
    public string Name     { get; set; } = string.Empty;
    public string Class    { get; set; } = string.Empty;
    public List<Score> Scores { get; set; } = new();
}
