using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Repositories
{
    public interface IWordRepository
    {
        Task<List<Word>> GetWordsAsync();
        Task<int> SaveWordAsync(Word word);
        Task<int> DeleteWordAsync(Word word);
    }
}
