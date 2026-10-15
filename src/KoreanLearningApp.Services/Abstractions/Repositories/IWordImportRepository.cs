using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Repositories;

public interface IWordImportRepository
{
    Task<HashSet<string>> GetExistingKeysAsync();
    Task<int> InsertManyAsync(List<Word> words);
}