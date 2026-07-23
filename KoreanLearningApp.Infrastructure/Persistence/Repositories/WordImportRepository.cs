using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Mapster;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Services.Abstractions.Repositories;

namespace KoreanLearningApp.Infrastructure.Persistence.Repositories;

public class WordImportRepository : IWordImportRepository
{
    private readonly IRepository<WordEntity> _wordRepo;
    private readonly IRepository<KrDictEntity> _krDictRepo;
    private readonly IRepository<KrDictSenseEntity> _senseRepo;
    private readonly IRepository<AudioEntity> _audioRepo;
    private readonly IRepository<LangInfoEntity> _langInfoRepo;

    public WordImportRepository(
        IRepository<WordEntity> wordRepo,
        IRepository<KrDictEntity> krDictRepo,
        IRepository<KrDictSenseEntity> senseRepo,
        IRepository<AudioEntity> audioRepo,
        IRepository<LangInfoEntity> langInfoRepo)
    {
        _wordRepo = wordRepo;
        _krDictRepo = krDictRepo;
        _senseRepo = senseRepo;
        _audioRepo = audioRepo;
        _langInfoRepo = langInfoRepo;
    }

    public async Task<HashSet<string>> GetExistingKeysAsync()
    {
        var words = await _wordRepo.GetAllAsync();
        var krDicts = await _krDictRepo.GetAllAsync();

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
            await _wordRepo.InsertAsync(wordEntity);
            var wordId = wordEntity.Id; // <-- берём Id из сущности, не из результата InsertAsync
            inserted++;

            if (word.KrDict is null)
                continue;

            int? audioId = null;
            if (word.KrDict.Audio is not null)
            {
                var audioEntity = word.KrDict.Audio.ToEntity();
                await _audioRepo.InsertAsync(audioEntity);
                audioId = audioEntity.Id;
            }

            var krDictEntity = word.KrDict.ToEntity(wordId);
            krDictEntity.AudioId = audioId;
            await _krDictRepo.InsertAsync(krDictEntity);
            var krDictId = krDictEntity.Id;

            foreach (var sense in word.KrDict.Senses)
            {
                int? enId = null;
                if (sense.En is not null)
                {
                    var enEntity = sense.En.ToEntity();
                    await _langInfoRepo.InsertAsync(enEntity);
                    enId = enEntity.Id;
                }

                int? ruId = null;
                if (sense.Ru is not null)
                {
                    var ruEntity = sense.Ru.ToEntity();
                    await _langInfoRepo.InsertAsync(ruEntity);
                    ruId = ruEntity.Id;
                }

                var senseEntity = sense.ToEntity(krDictId);
                senseEntity.EnId = enId;
                senseEntity.RuId = ruId;
                await _senseRepo.InsertAsync(senseEntity);
            }
        }

        return inserted;
    }

    private static string BuildKey(string korean, int supNo) => $"{korean}_{supNo}";
}