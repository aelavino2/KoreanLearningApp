using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapster;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class WordRepository : IWordRepository
{
    private readonly BaseRepository<WordEntity> _base;

    public WordRepository(BaseRepository<WordEntity> baseRepository)
    {
        _base = baseRepository;
    }

    public async Task<List<Word>> GetWordsAsync()
    {
        var entities = await _base.GetAllAsync();
        return entities.Select(WordMapper.ToDomain).ToList();
    }

    public async Task<int> SaveWordAsync(Word word)
    {
        var entity = WordMapper.ToEntity(word);
        return entity.Id == 0
            ? await _base.InsertAsync(entity)
            : await _base.UpdateAsync(entity);
    }

    public async Task<int> DeleteWordAsync(Word word)
    {
        var entity = WordMapper.ToEntity(word);
        return await _base.DeleteAsync(entity);
    }
}