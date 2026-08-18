using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface ISavedWordsService
{
    Task<List<Word>> GetSavedWordsAsync();
    Task<bool> IsSavedAsync(int wordId);
    Task<HashSet<int>> GetSavedWordIdSetAsync();
    Task SaveAsync(int wordId);
    Task RemoveAsync(int wordId);
    Task<bool> ToggleAsync(int wordId);
}