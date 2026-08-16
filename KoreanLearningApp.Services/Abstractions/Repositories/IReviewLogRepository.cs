using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Repositories;

public interface IReviewLogRepository
{
    /// <summary>Добавляет запись в журнал. Записи только добавляются, не редактируются.</summary>
    Task AddAsync(ReviewLog log);

    /// <summary>Вся история повторений конкретного слова, от старых к новым.</summary>
    Task<List<ReviewLog>> GetByWordIdAsync(int wordId);
}