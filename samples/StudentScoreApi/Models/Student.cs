namespace StudentScoreApi.Models;

public record Student(Guid Id, string Name)
{
    public List<int> Scores { get; init; } = new();
}
