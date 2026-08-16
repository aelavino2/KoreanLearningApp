using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Repositories;

public interface IWordProgressRepository
{
    /// <summary>Прогресс конкретного слова, либо null — слово ещё не появлялось в практике.</summary>
    Task<WordProgress?> GetByWordIdAsync(int wordId);

    /// <summary>Прогресс сразу для набора слов — используется, чтобы не дёргать GetByWordIdAsync в цикле.</summary>
    Task<List<WordProgress>> GetByWordIdsAsync(List<int> wordIds);

    /// <summary>
    /// Слова, у которых наступила (или прошла) дата следующего повторения.
    /// topikLevels: null/пусто — без фильтра по уровню, иначе только перечисленные уровни (например ["1","2"]).
    /// </summary>
    Task<List<WordProgress>> GetDueAsync(DateTime asOf, int limit, IReadOnlyCollection<string>? topikLevels = null);

    /// <summary>
    /// Id слов, у которых ещё нет записи прогресса, по возрастанию Rank (частотности).
    /// topikLevels: см. GetDueAsync.
    /// </summary>
    Task<List<int>> GetNewWordIdsAsync(int limit, IReadOnlyCollection<string>? topikLevels = null);

    /// <summary>Создаёт запись прогресса, если её ещё нет для этого WordId, иначе обновляет существующую.</summary>
    Task UpsertAsync(WordProgress progress);
}