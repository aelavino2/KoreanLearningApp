namespace KoreanLearningApp.Domain.Models;

public class QuizQuestion
{
    public required PracticeCard Card { get; init; }
    public required string QuestionText { get; init; }
    public required IReadOnlyList<QuizAnswerOption> Options { get; init; }
}
