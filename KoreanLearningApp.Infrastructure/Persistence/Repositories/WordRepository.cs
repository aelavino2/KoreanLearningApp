using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapster;
using KoreanLearningApp.Infrastructure.Persistence.Queries;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class WordRepository(IGetWordsPageQuery getWordsPageQuery,
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

    public Task<List<Word>> GetByIdsAsync(List<int> wordIds) =>
        wordIds.Count == 0 ? Task.FromResult(new List<Word>()) : BuildWordsAsync(wordIds);
    
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
        var wordEntity = word.ToEntity();

        if (word.Id == 0)
        {
            await wordRepo.InsertAsync(wordEntity);
        }
        else
        {
            wordEntity.Id = word.Id;
            await wordRepo.UpdateAsync(wordEntity);
        }

        var resolvedWordId = wordEntity.Id;

        var oldKrDicts = await krDictRepo.GetWhereAsync(k => k.WordId == resolvedWordId);
        foreach (var old in oldKrDicts)
        {
            var oldSenses = await senseRepo.GetWhereAsync(s => s.KrDictId == old.Id);
            foreach (var oldSense in oldSenses)
            {
                if (oldSense.EnId.HasValue)
                    await langInfoRepo.DeleteWhereAsync(l => l.Id == oldSense.EnId.Value);
                if (oldSense.RuId.HasValue)
                    await langInfoRepo.DeleteWhereAsync(l => l.Id == oldSense.RuId.Value);
            }
            await senseRepo.DeleteWhereAsync(s => s.KrDictId == old.Id);

            if (old.AudioId.HasValue)
                await audioRepo.DeleteWhereAsync(a => a.Id == old.AudioId.Value);
        }
        await krDictRepo.DeleteWhereAsync(k => k.WordId == resolvedWordId);

        if (word.KrDict is null)
            return resolvedWordId;

        int? audioId = null;
        if (word.KrDict.Audio is not null)
        {
            var audioEntity = word.KrDict.Audio.ToEntity();
            await audioRepo.InsertAsync(audioEntity);
            audioId = audioEntity.Id;
        }

        var krDictEntity = word.KrDict.ToEntity(resolvedWordId);
        krDictEntity.AudioId = audioId;
        await krDictRepo.InsertAsync(krDictEntity);
        var krDictId = krDictEntity.Id;

        foreach (var sense in word.KrDict.Senses)
        {
            int? enId = null;
            if (sense.En is not null)
            {
                var enEntity = sense.En.ToEntity();
                await langInfoRepo.InsertAsync(enEntity);
                enId = enEntity.Id;
            }

            int? ruId = null;
            if (sense.Ru is not null)
            {
                var ruEntity = sense.Ru.ToEntity();
                await langInfoRepo.InsertAsync(ruEntity);
                ruId = ruEntity.Id;
            }

            var senseEntity = sense.ToEntity(krDictId);
            senseEntity.EnId = enId;
            senseEntity.RuId = ruId;
            await senseRepo.InsertAsync(senseEntity);
        }

        return resolvedWordId;
    }

    public async Task<int> DeleteWordAsync(Word word)
    {
        var krDicts = await krDictRepo.GetWhereAsync(k => k.WordId == word.Id);
        foreach (var k in krDicts)
        {
            var senses = await senseRepo.GetWhereAsync(s => s.KrDictId == k.Id);
            foreach (var s in senses)
            {
                if (s.EnId.HasValue)
                    await langInfoRepo.DeleteWhereAsync(l => l.Id == s.EnId.Value);
                if (s.RuId.HasValue)
                    await langInfoRepo.DeleteWhereAsync(l => l.Id == s.RuId.Value);
            }
            await senseRepo.DeleteWhereAsync(s => s.KrDictId == k.Id);

            if (k.AudioId.HasValue)
                await audioRepo.DeleteWhereAsync(a => a.Id == k.AudioId.Value);
        }
        await krDictRepo.DeleteWhereAsync(k => k.WordId == word.Id);

        var entity = word.ToEntity();
        entity.Id = word.Id;
        return await wordRepo.DeleteAsync(entity);
    }
}