using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Services.Abstractions.Repositories;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.Services.Services;

public class WordService(IWordRepository repository, ITagRepository tagRepository) : IWordService
{
    public Task<(List<Word> Items, int TotalCount)> GetWordsPageAsync(WordsPageRequest request)
        => repository.GetWordsPageAsync(request);

    public Task<List<TagUsage>> GetTagsAsync() => tagRepository.GetAllWithUsageAsync();

    public Task<List<Word>> GetWordsAsync() => repository.GetWordsAsync();

    public Task<int> SaveWordAsync(Word word) => repository.SaveWordAsync(word);

    public Task<int> DeleteWordAsync(Word word) => repository.DeleteWordAsync(word);
}