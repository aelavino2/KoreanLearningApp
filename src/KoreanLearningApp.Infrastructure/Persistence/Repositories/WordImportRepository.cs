using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class WordImportRepository(DbContext dbContext, IRepository<WordEntity> wordRepo,
    IRepository<KrDictEntity> krDictRepo) : IWordImportRepository
{
    public async Task<HashSet<string>> GetExistingKeysAsync() =>
        (await GetWordIdsByKeyAsync()).Keys.ToHashSet();

    public async Task<int> InsertManyAsync(List<Word> words)
    {
        if (words.Count == 0)
            return 0;

        var db = await dbContext.GetConnectionAsync();
        await db.RunInTransactionAsync(conn =>
        {
            foreach (var word in words)
                WordGraphWriter.Insert(conn, word);
        });

        return words.Count;
    }

    public async Task<int> AddTagsAsync(IReadOnlyDictionary<string, List<Tag>> tagsByWordKey)
    {
        if (tagsByWordKey.Count == 0)
            return 0;

        var wordIdsByKey = await GetWordIdsByKeyAsync();
        var db = await dbContext.GetConnectionAsync();
        var taggedWords = 0;

        await db.RunInTransactionAsync(conn =>
        {
            foreach (var (key, tags) in tagsByWordKey)
            {
                if (wordIdsByKey.TryGetValue(key, out var wordId) && WordGraphWriter.AddTags(conn, wordId, tags))
                    taggedWords++;
            }
        });

        return taggedWords;
    }

    private async Task<Dictionary<string, int>> GetWordIdsByKeyAsync()
    {
        var words = await wordRepo.GetAllAsync();
        var supNoByWordId = (await krDictRepo.GetAllAsync()).ToDictionary(k => k.WordId, k => k.SupNo);

        return words
            .GroupBy(w => BuildKey(w.Korean, supNoByWordId.GetValueOrDefault(w.Id, 0)))
            .ToDictionary(g => g.Key, g => g.First().Id);
    }

    private static string BuildKey(string korean, int supNo) => $"{korean}_{supNo}";
}