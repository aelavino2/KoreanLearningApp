using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Repositories;

public interface IWordImportRepository
{
    Task<HashSet<string>> GetExistingKeysAsync();
    Task<int> InsertManyAsync(List<Word> words);

    /// <summary>
    /// Добавляет теги к существующим словам по ключу «Korean_SupNo»; возвращает число слов, у которых появились новые теги.
    /// </summary>
    Task<int> AddTagsAsync(IReadOnlyDictionary<string, List<Tag>> tagsByWordKey);
}