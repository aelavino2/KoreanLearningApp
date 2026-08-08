using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapster;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class WordImportRepository(IRepository<WordEntity> wordRepo,
    IRepository<KrDictEntity> krDictRepo, IRepository<KrDictSenseEntity> senseRepo,
    IRepository<AudioEntity> audioRepo, IRepository<LangInfoEntity> langInfoRepo) : IWordImportRepository
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

        var inserted = 0;

        foreach (var word in words)
        {
            var wordEntity = word.ToEntity();
            await wordRepo.InsertAsync(wordEntity);
            var wordId = wordEntity.Id;
            inserted++;

            if (word.KrDict is null)
                continue;

            int? audioId = null;
            if (word.KrDict.Audio is not null)
            {
                var audioEntity = word.KrDict.Audio.ToEntity();
                await audioRepo.InsertAsync(audioEntity);
                audioId = audioEntity.Id;
            }

            var krDictEntity = word.KrDict.ToEntity(wordId);
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
        }

        return inserted;
    }

    private static string BuildKey(string korean, int supNo) => $"{korean}_{supNo}";
}