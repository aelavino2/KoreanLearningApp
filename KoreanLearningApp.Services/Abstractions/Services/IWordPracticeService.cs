using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface IWordPracticeService
{
    /// <summary>
    /// Собирает сессию практики: слова с наступившей датой повторения + новые слова по частотности.
    /// Due-слова идут первыми (они отсортированы по NextReviewDate), затем новые (по Rank).
    /// </summary>
    Task<List<PracticeCard>> GetPracticeSessionAsync(int dueLimit = 20, int newLimit = 5);

    /// <summary>
    /// Обрабатывает ответ пользователя на слово: пересчитывает прогресс по SM-2,
    /// сохраняет его и добавляет запись в журнал повторений. Если слово показывается
    /// впервые — создаёт для него начальный прогресс.
    /// </summary>
    Task SubmitReviewAsync(int wordId, ReviewRatingEnum rating);
}