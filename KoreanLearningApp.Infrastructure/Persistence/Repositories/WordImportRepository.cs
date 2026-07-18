using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapster;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class WordImportRepository : IWordImportRepository
{
    private readonly IRepository<WordEntity> _base;

    public WordImportRepository(IRepository<WordEntity> baseRepository)
    {
        _base = baseRepository;
    }

    public async Task<HashSet<string>> GetExistingKeysAsync()
    {
        var entities = await _base.GetAllAsync();
        return entities
            .Select(e => BuildKey(e.Korean, e.SupNo))
            .ToHashSet();
    }

    public async Task<int> InsertManyAsync(List<Word> words)
    {
        if (words.Count == 0)
            return 0;

        var entities = words.Select(WordMapper.ToEntity).ToList();
        return await _base.InsertAllAsync(entities);
    }

    private static string BuildKey(string korean, int supNo) => $"{korean}_{supNo}";
}