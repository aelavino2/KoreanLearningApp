using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface ISavedWordsService
{
    Task<List<Word>> GetSavedWordsAsync();
    Task SaveAsync(int wordId);
    Task RemoveAsync(int wordId);
}