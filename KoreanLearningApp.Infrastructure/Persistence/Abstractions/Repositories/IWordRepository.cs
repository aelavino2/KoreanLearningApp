using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Models;

namespace KoreanLearningApp.Infrastructure.Persistence.Abstractions.Repositories;

public interface IWordRepository
{
    Task<List<Word>> GetWordsAsync();
    Task<List<Word>> GetDueWordsAsync(int desiredCount);
    Task<int> SaveWordAsync(Word word);
    Task<ImportResult> ImportWordsAsync(List<WordImportDto> incoming);
    Task<int> DeleteWordAsync(Word word);
    Task<string> BuildExportJsonAsync();
}