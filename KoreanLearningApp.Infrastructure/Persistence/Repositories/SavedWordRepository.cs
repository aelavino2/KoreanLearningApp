using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class SavedWordRepository(IRepository<SavedWordEntity> savedRepo) : ISavedWordRepository
{
    public async Task<List<int>> GetSavedWordIdsAsync()
    {
        var entities = await savedRepo.GetAllAsync();
        return entities
            .OrderByDescending(e => e.SavedAt)
            .Select(e => e.WordId)
            .ToList();
    }

    public async Task<bool> IsSavedAsync(int wordId)
    {
        var existing = await savedRepo.GetWhereAsync(s => s.WordId == wordId);
        return existing.Count > 0;
    }

    public async Task AddAsync(int wordId)
    {
        if (await IsSavedAsync(wordId))
            return;

        await savedRepo.InsertAsync(new SavedWordEntity
        {
            WordId = wordId,
            SavedAt = DateTime.UtcNow
        });
    }

    public Task RemoveAsync(int wordId) => savedRepo.DeleteWhereAsync(s => s.WordId == wordId);
}