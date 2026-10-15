namespace KoreanLearningApp.Domain.Models;

public class PracticeCard
{
    public required Word Word { get; init; }
    public WordProgress? Progress { get; init; }

    public bool IsNew => Progress is null;
}
