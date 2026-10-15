using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface ISavedWordsService
{
    Task<List<Word>> GetSavedWordsAsync();
    Task<HashSet<int>> GetSavedWordIdsAsync();
    Task<bool> IsSavedAsync(int wordId);
    Task RemoveAsync(int wordId);

    /// <summary>
    /// Переключает признак «сохранено» и возвращает новое состояние.
    /// </summary>
    Task<bool> ToggleAsync(int wordId);
}
