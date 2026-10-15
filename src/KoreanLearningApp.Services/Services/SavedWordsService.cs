using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Services.Abstractions.Repositories;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.Services.Services;

public sealed class SavedWordsService(ISavedWordRepository savedWordRepository, IWordRepository wordRepository)
    : ISavedWordsService
{
    public async Task<List<Word>> GetSavedWordsAsync()
    {
        var ids = await savedWordRepository.GetSavedWordIdsAsync();
        return await wordRepository.GetByIdsAsync(ids);
    }

    public async Task<HashSet<int>> GetSavedWordIdsAsync() =>
        (await savedWordRepository.GetSavedWordIdsAsync()).ToHashSet();

    public Task<bool> IsSavedAsync(int wordId) => savedWordRepository.IsSavedAsync(wordId);

    public Task RemoveAsync(int wordId) => savedWordRepository.RemoveAsync(wordId);

    public async Task<bool> ToggleAsync(int wordId)
    {
        if (await savedWordRepository.IsSavedAsync(wordId))
        {
            await savedWordRepository.RemoveAsync(wordId);
            return false;
        }

        await savedWordRepository.AddAsync(wordId);
        return true;
    }
}
