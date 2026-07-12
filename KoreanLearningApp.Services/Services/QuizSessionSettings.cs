using KoreanLearningApp.Models;
namespace KoreanLearningApp.Services;

public class QuizSessionSettings
{
    public QuizDifficulty Difficulty { get; set; } = QuizDifficulty.Normal;
    public int WordCount { get; set; } = 10;
}