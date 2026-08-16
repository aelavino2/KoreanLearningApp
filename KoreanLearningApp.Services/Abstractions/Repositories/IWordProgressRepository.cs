using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Repositories;

public interface IWordProgressRepository
{
    Task<WordProgress?> GetByWordIdAsync(int wordId);
    Task<List<WordProgress>> GetByWordIdsAsync(List<int> wordIds);
    Task<List<WordProgress>> GetDueAsync(DateTime asOf, int limit, IReadOnlyCollection<string>? topikLevels = null);
    Task<List<int>> GetNewWordIdsAsync(int limit, IReadOnlyCollection<string>? topikLevels = null);
    Task UpsertAsync(WordProgress progress);
}