using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class WordImportRepository(DbContext dbContext, IRepository<WordEntity> wordRepo,
    IRepository<KrDictEntity> krDictRepo) : IWordImportRepository
{
    public async Task<HashSet<string>> GetExistingKeysAsync()
    {
        var words = await wordRepo.GetAllAsync();
        var krDicts = await krDictRepo.GetAllAsync();

        var krDictByWordId = krDicts.ToDictionary(k => k.WordId, k => k.SupNo);

        return words
            .Select(w => BuildKey(w.Korean, krDictByWordId.GetValueOrDefault(w.Id, 0)))
            .ToHashSet();
    }

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

    private static string BuildKey(string korean, int supNo) => $"{korean}_{supNo}";
}