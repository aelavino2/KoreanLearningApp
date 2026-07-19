using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapster;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class WordRepository : IWordRepository
{
    private readonly IRepository<WordEntity> _base;
    private readonly IRepository<WordSenseEntity> _senseRepo;

    public WordRepository(IRepository<WordEntity> baseRepository, IRepository<WordSenseEntity> senseRepo)
    {
        _base = baseRepository;
        _senseRepo = senseRepo;
    }

    public async Task<List<Word>> GetWordsAsync()
    {
        var wordEntities = await _base.GetAllAsync();
        var allSenses = await _senseRepo.GetAllAsync();

        var sensesByWordId = allSenses
            .GroupBy(s => s.WordId)
            .ToDictionary(g => g.Key, g => g.Select(EntityMappingExtensions.ToDomain).ToList());

        return wordEntities
           .Select(e => EntityMappingExtensions.ToDomain(e, sensesByWordId.GetValueOrDefault(e.Id, new List<Sense>())))
            .ToList();
    }

    public async Task<int> SaveWordAsync(Word word)
    {
        var entity = EntityMappingExtensions.ToEntity(word);

        var wordId = word.Id == 0 
            ? await _base.InsertAsync(entity) 
            : await _base.UpdateAsync(entity);

        var resolvedId = word.Id == 0 ? wordId : word.Id;

        await _senseRepo.DeleteWhereAsync(s => s.WordId == resolvedId);

        var senseEntities = word.Senses.Select(s => s.ToEntity(resolvedId)).ToList();
        if (senseEntities.Count > 0)
            await _senseRepo.InsertAllAsync(senseEntities);

        return wordId;
    }

    public async Task<int> DeleteWordAsync(Word word)
    {
        await _senseRepo.DeleteWhereAsync(s => s.WordId == word.Id);
        var entity = EntityMappingExtensions.ToEntity(word);
        return await _base.DeleteAsync(entity);
    }
}