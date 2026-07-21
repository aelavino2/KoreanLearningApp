using KoreanLearningApp.Domain.Models;

namespace KoreanLearningApp.Services.Abstractions.Services;

public interface IWordService
{
    Task<List<Word>> GetWordsAsync();
    Task<int> SaveWordAsync(Word word);
    Task<int> DeleteWordAsync(Word word);
}