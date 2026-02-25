namespace ScoreTracker.Models;

// A single subject score for a student
public class Score
{
    public string Subject { get; set; } = string.Empty;
    public int Value { get; set; }            // score out of 100
}
