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

    public Task<bool> IsSavedAsync(int wordId) => savedWordRepository.IsSavedAsync(wordId);

    public async Task<HashSet<int>> GetSavedWordIdSetAsync()
    {
        var ids = await savedWordRepository.GetSavedWordIdsAsync();
        return ids.ToHashSet();
    }

    public Task SaveAsync(int wordId) => savedWordRepository.AddAsync(wordId);

    public Task RemoveAsync(int wordId) => savedWordRepository.RemoveAsync(wordId);

    public async Task<bool> ToggleAsync(int wordId)
    {
        var isSaved = await savedWordRepository.IsSavedAsync(wordId);

        if (isSaved)
        {
            await savedWordRepository.RemoveAsync(wordId);
            return false;
        }

        await savedWordRepository.AddAsync(wordId);
        return true;
    }
}