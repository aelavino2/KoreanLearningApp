using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Services.Abstractions.Repositories;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.Services.Services;

public class WordService(IWordRepository repository) : IWordService
{
    public Task<(List<Word> Items, int TotalCount)> GetWordsPageAsync(int page, int pageSize, string? search)
        => repository.GetWordsPageAsync(page, pageSize, search);

    public Task<List<Word>> GetWordsAsync() => repository.GetWordsAsync();

    public Task<int> SaveWordAsync(Word word) => repository.SaveWordAsync(word);

    public Task<int> DeleteWordAsync(Word word) => repository.DeleteWordAsync(word);
}