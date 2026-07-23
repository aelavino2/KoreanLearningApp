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
            var wordId = await _wordRepo.InsertAsync(wordEntity);
            inserted++;

            if (word.KrDict is null)
                continue;

            int? audioId = null;
            if (word.KrDict.Audio is not null)
            {
                audioId = await _audioRepo.InsertAsync(word.KrDict.Audio.ToEntity());
            }

            var krDictEntity = word.KrDict.ToEntity(wordId);
            krDictEntity.AudioId = audioId;
            var krDictId = await _krDictRepo.InsertAsync(krDictEntity);

            foreach (var sense in word.KrDict.Senses)
            {
                int? enId = sense.En is not null ? await _langInfoRepo.InsertAsync(sense.En.ToEntity()) : null;
                int? ruId = sense.Ru is not null ? await _langInfoRepo.InsertAsync(sense.Ru.ToEntity()) : null;

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