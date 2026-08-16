using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Services.Abstractions.Repositories;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.Services.Services;

public class SavedWordsService(ISavedWordRepository savedWordRepository, IWordRepository wordRepository)
    : ISavedWordsService
{
    public async Task<List<Word>> GetSavedWordsAsync()
    {
        // Id уже отсортированы по дате сохранения (новые сверху) — GetByIdsAsync
        // сохраняет порядок переданных id, так что порядок карточек не потеряется.
        var ids = await savedWordRepository.GetSavedWordIdsAsync();
        return await wordRepository.GetByIdsAsync(ids);
    }

    public Task SaveAsync(int wordId) => savedWordRepository.AddAsync(wordId);

    public Task RemoveAsync(int wordId) => savedWordRepository.RemoveAsync(wordId);
}