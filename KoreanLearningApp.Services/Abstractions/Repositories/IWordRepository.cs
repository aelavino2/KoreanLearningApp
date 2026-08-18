using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Repositories
{
    public interface IWordRepository
    {
        Task<List<Word>> GetWordsAsync();
        Task<List<Word>> GetByIdsAsync(List<int> wordIds);
        Task<(List<Word> Items, int TotalCount)> GetWordsPageAsync(int page, int pageSize, string? search);
        Task<int> SaveWordAsync(Word word);
        Task<int> DeleteWordAsync(Word word);
    }
}