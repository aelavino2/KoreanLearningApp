namespace KoreanLearningApp.Domain.Models;

public class PracticeCard
{
    public Word Word { get; set; } = null!;
    public WordProgress? Progress { get; set; }

    public bool IsNew => Progress is null;
}