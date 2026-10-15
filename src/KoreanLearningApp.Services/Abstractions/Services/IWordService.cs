using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface IWordService
{
    Task<(List<Word> Items, int TotalCount)> GetWordsPageAsync(WordsPageRequest request);
    Task<List<TagUsage>> GetTagsAsync();
    Task<List<Word>> GetWordsAsync();
    Task<int> SaveWordAsync(Word word);
    Task<int> DeleteWordAsync(Word word);
}