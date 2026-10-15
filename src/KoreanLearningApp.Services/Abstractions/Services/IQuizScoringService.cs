using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface IQuizScoringService
{
    /// <summary>
    /// Неверный ответ — Forgot; верный за первую половину времени — Good, иначе Hard.
    /// </summary>
    ReviewRatingEnum DetermineRating(bool wasCorrect, int secondsLeft, int questionSeconds);

    int CalculateScore(bool wasCorrect, int secondsLeft);
}
