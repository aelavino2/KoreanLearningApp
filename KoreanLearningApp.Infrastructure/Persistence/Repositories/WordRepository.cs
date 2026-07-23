using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapster;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class WordRepository(
    IRepository<WordEntity> wordRepo,
    IRepository<KrDictEntity> krDictRepo,
    IRepository<KrDictSenseEntity> senseRepo,
    IRepository<AudioEntity> audioRepo,
    IRepository<LangInfoEntity> langInfoRepo)
    : IWordRepository
{
    public async Task<List<Word>> GetWordsAsync()
    {
        var wordEntities = await wordRepo.GetAllAsync();
        var krDictEntities = await krDictRepo.GetAllAsync();
        var senseEntities = await senseRepo.GetAllAsync();
        var audioEntities = await audioRepo.GetAllAsync();
        var langInfoEntities = await langInfoRepo.GetAllAsync();

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

        return wordEntities
            .Select(e => e.ToDomain(krDictByWordId.GetValueOrDefault(e.Id)))
            .ToList();
    }

    public async Task<int> SaveWordAsync(Word word)
    {
        var wordEntity = word.ToEntity();
        var wordId = word.Id == 0
            ? await wordRepo.InsertAsync(wordEntity)
            : await wordRepo.UpdateAsync(wordEntity);
        var resolvedWordId = word.Id == 0 ? wordId : word.Id;
        
        var oldKrDicts = (await krDictRepo.GetAllAsync()).Where(k => k.WordId == resolvedWordId).ToList();
        foreach (var old in oldKrDicts)
        {
            await senseRepo.DeleteWhereAsync(s => s.KrDictId == old.Id);
        }
        await krDictRepo.DeleteWhereAsync(k => k.WordId == resolvedWordId);

        if (word.KrDict is null)
            return wordId;

        int? audioId = null;
        if (word.KrDict.Audio is not null)
        {
            var audioEntity = word.KrDict.Audio.ToEntity();
            audioId = await audioRepo.InsertAsync(audioEntity);
        }

        var krDictEntity = word.KrDict.ToEntity(resolvedWordId);
        krDictEntity.AudioId = audioId;
        var krDictId = await krDictRepo.InsertAsync(krDictEntity);

        foreach (var sense in word.KrDict.Senses)
        {
            int? enId = sense.En is not null ? await langInfoRepo.InsertAsync(sense.En.ToEntity()) : null;
            int? ruId = sense.Ru is not null ? await langInfoRepo.InsertAsync(sense.Ru.ToEntity()) : null;

            var senseEntity = sense.ToEntity(krDictId);
            senseEntity.EnId = enId;
            senseEntity.RuId = ruId;
            await senseRepo.InsertAsync(senseEntity);
        }

        return wordId;
    }

    public async Task<int> DeleteWordAsync(Word word)
    {
        var krDicts = (await krDictRepo.GetAllAsync()).Where(k => k.WordId == word.Id).ToList();
        foreach (var k in krDicts)
        {
            await senseRepo.DeleteWhereAsync(s => s.KrDictId == k.Id);
            if (k.AudioId.HasValue)
                await audioRepo.DeleteWhereAsync(a => a.Id == k.AudioId.Value);
        }
        await krDictRepo.DeleteWhereAsync(k => k.WordId == word.Id);

        var entity = word.ToEntity();
        return await wordRepo.DeleteAsync(entity);
    }
}