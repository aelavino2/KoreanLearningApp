using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions
{
    public interface IWordRepository
    {
        Task<List<Word>> GetWordsAsync();
        Task<int> SaveWordAsync(Word word);
        Task<int> DeleteWordAsync(Word word);
    }
}
