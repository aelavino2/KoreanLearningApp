using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Repositories;

public interface IWordProgressRepository
{
    Task<WordProgress?> GetByWordIdAsync(int wordId);
    Task<List<WordProgress>> GetByWordIdsAsync(IReadOnlyCollection<int> wordIds);

    /// <summary>
    /// Прогресс слов, у которых подошёл срок повторения, по возрастанию даты.
    /// </summary>
    Task<List<WordProgress>> GetDueAsync(DateTime asOf, int limit, IReadOnlyCollection<string> topikLevels);

    /// <summary>
    /// Id слов, которые ещё ни разу не практиковались, по возрастанию ранга.
    /// </summary>
    Task<List<int>> GetNewWordIdsAsync(int limit, IReadOnlyCollection<string> topikLevels);

    Task UpsertAsync(WordProgress progress);
}
