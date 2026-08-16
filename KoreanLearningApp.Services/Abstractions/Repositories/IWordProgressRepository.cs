using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Repositories;

public interface IWordProgressRepository
{
    /// <summary>Прогресс конкретного слова, либо null — слово ещё не появлялось в практике.</summary>
    Task<WordProgress?> GetByWordIdAsync(int wordId);

    /// <summary>Слова, у которых наступила (или прошла) дата следующего повторения.</summary>
    Task<List<WordProgress>> GetDueAsync(DateTime asOf, int limit);

    /// <summary>Id слов, у которых ещё нет записи прогресса, по возрастанию Rank (частотности).</summary>
    Task<List<int>> GetNewWordIdsAsync(int limit);

    /// <summary>Создаёт запись прогресса, если её ещё нет для этого WordId, иначе обновляет существующую.</summary>
    Task UpsertAsync(WordProgress progress);
}