using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface ISavedWordsService
{
    Task<List<Word>> GetSavedWordsAsync();

    /// <summary>Проверка статуса одного слова — используется на странице деталей слова.</summary>
    Task<bool> IsSavedAsync(int wordId);

    /// <summary>
    /// Batch-версия для списков (практика, список слов): один запрос к БД вместо N,
    /// чтобы не дёргать IsSavedAsync в цикле по каждой карточке.
    /// </summary>
    Task<HashSet<int>> GetSavedWordIdSetAsync();

    Task SaveAsync(int wordId);
    Task RemoveAsync(int wordId);

    /// <summary>Переключает статус сохранения и возвращает новое состояние (true — сохранено).</summary>
    Task<bool> ToggleAsync(int wordId);
}