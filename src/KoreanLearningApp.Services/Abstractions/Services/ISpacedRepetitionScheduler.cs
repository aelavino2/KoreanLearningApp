using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface ISpacedRepetitionScheduler
{
    WordProgress CreateInitialProgress(int wordId, DateTime now);

    /// <summary>
    /// Применяет оценку ответа к прогрессу (изменяет переданный объект) и планирует следующее повторение.
    /// </summary>
    void ApplyReview(WordProgress progress, ReviewRatingEnum rating, DateTime now);
}
