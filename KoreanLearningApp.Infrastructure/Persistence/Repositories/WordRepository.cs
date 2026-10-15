using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapster;
using KoreanLearningApp.Infrastructure.Persistence.Queries;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class WordRepository(DbContext dbContext, IGetWordsPageQuery getWordsPageQuery,
    IRepository<WordEntity> wordRepo, IRepository<KrDictEntity> krDictRepo,
    IRepository<KrDictSenseEntity> senseRepo, IRepository<AudioEntity> audioRepo,
    IRepository<LangInfoEntity> langInfoRepo)
    : IWordRepository
{
    public async Task<List<Word>> GetWordsAsync()
    {
        var allIds = (await wordRepo.GetAllAsync())
            .Select(w => w.Id)
            .ToList();
        
        return allIds.Count == 0 ? new List<Word>() : await BuildWordsAsync(allIds);
    }

    public Task<List<Word>> GetByIdsAsync(IReadOnlyCollection<int> wordIds) =>
        wordIds.Count == 0 ? Task.FromResult(new List<Word>()) : BuildWordsAsync(wordIds.ToList());

    public async Task<List<Word>> GetRandomAsync(int count)
    {
        var db = await dbContext.GetConnectionAsync();
        var rows = await db.QueryAsync<WordEntity>("SELECT Id FROM Words ORDER BY RANDOM() LIMIT ?", count);
        var ids = rows.Select(r => r.Id).ToList();

        return ids.Count == 0 ? new List<Word>() : await BuildWordsAsync(ids);
    }

    public async Task<(List<Word> Items, int TotalCount)> GetWordsPageAsync(int page, int pageSize, string? search)
    {
        var (pageIds, totalCount) = await getWordsPageQuery.ExecuteAsync(page, pageSize, search);

        if (pageIds.Count == 0)
            return (new List<Word>(), totalCount);

        var words = await BuildWordsAsync(pageIds);
        return (words, totalCount);
    }
    
    private async Task<List<Word>> BuildWordsAsync(List<int> wordIds)
    {
        var wordEntities = await wordRepo.GetWhereAsync(w => wordIds.Contains(w.Id));
        var krDictEntities = await krDictRepo.GetWhereAsync(k => wordIds.Contains(k.WordId));

        var krDictIds = krDictEntities.Select(k => k.Id).ToList();
        var senseEntities = await senseRepo.GetWhereAsync(s => krDictIds.Contains(s.KrDictId));

        var audioIds = krDictEntities.Where(k => k.AudioId.HasValue).Select(k => k.AudioId!.Value).ToList();
        var audioEntities = audioIds.Count > 0
            ? await audioRepo.GetWhereAsync(a => audioIds.Contains(a.Id))
            : new List<AudioEntity>();

        var langInfoIds = senseEntities
            .SelectMany(s => new[] { s.EnId, s.RuId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var langInfoEntities = langInfoIds.Count > 0
            ? await langInfoRepo.GetWhereAsync(l => langInfoIds.Contains(l.Id))
            : new List<LangInfoEntity>();

        var audiosById = audioEntities.ToDictionary(a => a.Id);
        var langInfoById = langInfoEntities.ToDictionary(l => l.Id);

        var sensesByKrDictId = senseEntities
            .GroupBy(s => s.KrDictId)
            .ToDictionary(g => g.Key, g => g.Select(s =>
            {
                var en = s.EnId.HasValue && langInfoById.TryGetValue(s.EnId.Value, out var enEntity)
                    ? enEntity.ToDomain() : null;
                var ru = s.RuId.HasValue && langInfoById.TryGetValue(s.RuId.Value, out var ruEntity)
                    ? ruEntity.ToDomain() : null;
                return s.ToDomain(en, ru);
            }).ToList());

        var krDictByWordId = krDictEntities.ToDictionary(k => k.WordId, k =>
        {
            var senses = sensesByKrDictId.GetValueOrDefault(k.Id, new List<Sense>());
            Audio? audio = k.AudioId.HasValue && audiosById.TryGetValue(k.AudioId.Value, out var audioEntity)
                ? audioEntity.ToDomain() : null;
            return k.ToDomain(senses, audio);
        });
        
        var wordsById = wordEntities.ToDictionary(w => w.Id);
        return wordIds
            .Where(id => wordsById.ContainsKey(id))
            .Select(id => wordsById[id].ToDomain(krDictByWordId.GetValueOrDefault(id)))
            .ToList();
    }

    public async Task<int> SaveWordAsync(Word word)
    {
        var db = await dbContext.GetConnectionAsync();
        var wordId = 0;
        await db.RunInTransactionAsync(conn =>
            wordId = word.Id == 0 ? WordGraphWriter.Insert(conn, word) : WordGraphWriter.Update(conn, word));
        return wordId;
    }

    public async Task<int> DeleteWordAsync(Word word)
    {
        var db = await dbContext.GetConnectionAsync();
        var deleted = 0;
        await db.RunInTransactionAsync(conn => deleted = WordGraphWriter.Delete(conn, word.Id));
        return deleted;
    }
}