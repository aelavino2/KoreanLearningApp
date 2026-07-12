using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Abstractions.Repositories;
using KoreanLearningApp.Models;
using KoreanLearningApp.Services.Abstractions;

namespace KoreanLearningApp.Services;

public class WordService : IWordService
{
    private readonly IWordRepository _wordRepository;

    public WordService(IWordRepository wordRepository)
    {
        _wordRepository = wordRepository;
    }

    public Task<List<Word>> GetWordsAsync() => _wordRepository.GetWordsAsync();

    public Task<List<Word>> GetDueWordsAsync(int desiredCount) =>
        _wordRepository.GetDueWordsAsync(desiredCount);

    public Task<int> SaveWordAsync(Word word) => _wordRepository.SaveWordAsync(word);

    public Task<int> DeleteWordAsync(Word word) => _wordRepository.DeleteWordAsync(word);

    public Task<ImportResult> ImportWordsAsync(List<WordImportDto> incoming) =>
        _wordRepository.ImportWordsAsync(incoming);

    public Task<string> BuildExportJsonAsync() => _wordRepository.BuildExportJsonAsync();
}