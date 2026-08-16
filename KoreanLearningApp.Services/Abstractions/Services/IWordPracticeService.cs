using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface IWordPracticeService
{
    /// <summary>
    /// Собирает сессию практики согласно опциям пользователя. Порядок приоритета:
    /// 1) сохранённые слова (если options.IncludeSavedWords), 2) due-слова (по NextReviewDate),
    /// 3) новые слова (по Rank). Список ограничен options.WordCount и отфильтрован
    /// по options.TopikLevels (пусто — без фильтра).
    /// </summary>
    Task<List<PracticeCard>> GetPracticeSessionAsync(PracticeSessionOptions options);

    /// <summary>
    /// Обрабатывает ответ пользователя на слово: пересчитывает прогресс по SM-2,
    /// сохраняет его и добавляет запись в журнал повторений. Если слово показывается
    /// впервые — создаёт для него начальный прогресс.
    /// </summary>
    Task SubmitReviewAsync(int wordId, ReviewRatingEnum rating);
}