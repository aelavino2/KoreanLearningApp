using KoreanLearningApp.Infrastructure.Constants;
using KoreanLearningApp.Infrastructure.Import;
using KoreanLearningApp.Infrastructure.Persistence;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Queries;
using KoreanLearningApp.Infrastructure.Persistence.Repositories;
using KoreanLearningApp.Services.Services;
using KoreanLearningApp.Services.Services.Practice;

namespace KoreanLearningApp.Tests.Support;

/// <summary>
/// Собирает реальные сервисы и репозитории поверх тестовой БД, как это делает MauiProgram.
/// </summary>
public sealed class TestServices
{
    public TestServices(DbContext db, TimeProvider? timeProvider = null)
    {
        WordMappingConfig.Configure();
        TimeProvider = timeProvider ?? TimeProvider.System;

        var words = new BaseRepository<WordEntity>(db);
        var krDicts = new BaseRepository<KrDictEntity>(db);
        var senses = new BaseRepository<KrDictSenseEntity>(db);
        var audios = new BaseRepository<AudioEntity>(db);
        var langInfos = new BaseRepository<LangInfoEntity>(db);

        WordRepository = new WordRepository(db, new GetWordsPageQuery(db), words, krDicts, senses, audios, langInfos);
        ImportService = new WordImportService(
            new WordImportRepository(db, words, krDicts),
            new KrDictJsonSerializer(new ImportJsonOptions()));
        ExportService = new WordExportService(new ExportJsonOptions());

        ProgressRepository = new WordProgressRepository(db, new BaseRepository<WordProgressEntity>(db));
        SavedWordRepository = new SavedWordRepository(new BaseRepository<SavedWordEntity>(db), TimeProvider);
        SavedWordsService = new SavedWordsService(SavedWordRepository, WordRepository);
        PracticeService = new WordPracticeService(
            WordRepository,
            ProgressRepository,
            new ReviewLogRepository(new BaseRepository<ReviewLogEntity>(db)),
            SavedWordRepository,
            new Sm2SpacedRepetitionScheduler(),
            new QuizQuestionFactory(new Random(42)),
            TimeProvider);
    }

    public TimeProvider TimeProvider { get; }
    public WordRepository WordRepository { get; }
    public WordImportService ImportService { get; }
    public WordExportService ExportService { get; }
    public WordProgressRepository ProgressRepository { get; }
    public SavedWordRepository SavedWordRepository { get; }
    public SavedWordsService SavedWordsService { get; }
    public WordPracticeService PracticeService { get; }
}
