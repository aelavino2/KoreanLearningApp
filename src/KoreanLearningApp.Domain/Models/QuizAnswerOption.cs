namespace KoreanLearningApp.Domain.Models;

public class QuizAnswerOption
{
    public required string Text { get; init; }
    public required bool IsCorrect { get; init; }
}
