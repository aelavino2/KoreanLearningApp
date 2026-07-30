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

        // чистим старые связанные записи перед перезаписью
        var oldKrDicts = (await krDictRepo.GetAllAsync()).Where(k => k.WordId == resolvedWordId).ToList();
        foreach (var old in oldKrDicts)
        {
            var oldSenses = (await senseRepo.GetAllAsync()).Where(s => s.KrDictId == old.Id).ToList();
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
        var krDicts = (await krDictRepo.GetAllAsync()).Where(k => k.WordId == word.Id).ToList();
        foreach (var k in krDicts)
        {
            var senses = (await senseRepo.GetAllAsync()).Where(s => s.KrDictId == k.Id).ToList();
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