using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface IWordService
{
    Task<(List<Word> Items, int TotalCount)> GetWordsPageAsync(int page, int pageSize, string? search);
    Task<List<Word>> GetWordsAsync();
    Task<int> SaveWordAsync(Word word);
    Task<int> DeleteWordAsync(Word word);
}