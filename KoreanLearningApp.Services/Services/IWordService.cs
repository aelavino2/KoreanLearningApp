using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Models;

namespace KoreanLearningApp.Services;

public interface IWordService
{
    Task<List<Word>> GetWordsAsync();
    Task<List<Word>> GetDueWordsAsync(int desiredCount);
    Task<int> SaveWordAsync(Word word);
    Task<int> DeleteWordAsync(Word word);
    Task<ImportResult> ImportWordsAsync(List<WordImportDto> incoming);
    Task<string> BuildExportJsonAsync();
}