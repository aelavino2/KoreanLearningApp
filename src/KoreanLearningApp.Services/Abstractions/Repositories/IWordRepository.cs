using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Repositories
{
    public interface IWordRepository
    {
        Task<List<Word>> GetWordsAsync();

        /// <summary>
        /// Слова в порядке переданных Id; отсутствующие Id пропускаются.
        /// </summary>
        Task<List<Word>> GetByIdsAsync(IReadOnlyCollection<int> wordIds);

        Task<List<Word>> GetRandomAsync(int count);
        Task<(List<Word> Items, int TotalCount)> GetWordsPageAsync(int page, int pageSize, string? search);
        Task<int> SaveWordAsync(Word word);
        Task<int> DeleteWordAsync(Word word);
    }
}
