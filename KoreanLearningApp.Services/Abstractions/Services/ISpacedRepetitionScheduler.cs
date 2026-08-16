using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface ISpacedRepetitionScheduler
{
    /// <summary>Создаёт начальный прогресс для слова, которое впервые показывается в практике.</summary>
    WordProgress CreateInitialProgress(int wordId, DateTime now);

    /// <summary>
    /// Применяет оценку ответа к текущему прогрессу и возвращает обновлённый объект
    /// (тот же экземпляр, изменённый на месте — вызывающий код сохраняет его через репозиторий).
    /// </summary>
    WordProgress ApplyReview(WordProgress progress, ReviewRatingEnum rating, DateTime now);
}