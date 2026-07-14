using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Services.Abstractions;

namespace KoreanLearningApp.Services;

public class WordService : IWordService
{
    private readonly IWordRepository _repository;

    public WordService(IWordRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Word>> GetWordsAsync() => _repository.GetWordsAsync();

    public Task<int> SaveWordAsync(Word word) => _repository.SaveWordAsync(word);

    public Task<int> DeleteWordAsync(Word word) => _repository.DeleteWordAsync(word);
}