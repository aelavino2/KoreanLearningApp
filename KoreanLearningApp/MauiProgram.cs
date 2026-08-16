using CommunityToolkit.Maui;
using KoreanLearningApp.Infrastructure.Constants;
using KoreanLearningApp.Infrastructure.Import;
using KoreanLearningApp.Infrastructure.Import.Abstraction;
using KoreanLearningApp.Infrastructure.Persistence;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Repositories;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services;
using KoreanLearningApp.Services.Abstractions.Repositories;
using KoreanLearningApp.Services.Abstractions.Services;
using KoreanLearningApp.Services.Services;
using KoreanLearningApp.Services.Services.SpacedRepetition;
using KoreanLearningApp.ViewModels;
using KoreanLearningApp.Views;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using System.Text.Json;
using KoreanLearningApp.Infrastructure.Persistence.Queries;

namespace KoreanLearningApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        WordMappingConfig.Configure();
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var dbPath = GetDatabasePath("KoreanLearning.db");
        builder.Services.AddSingleton(new DbContext(dbPath));

        builder.Services.AddSingleton<IRepository<WordEntity>, BaseRepository<WordEntity>>();
        builder.Services.AddSingleton<IRepository<KrDictEntity>, BaseRepository<KrDictEntity>>();
        builder.Services.AddSingleton<IRepository<KrDictSenseEntity>, BaseRepository<KrDictSenseEntity>>();
        builder.Services.AddSingleton<IRepository<AudioEntity>, BaseRepository<AudioEntity>>();
        builder.Services.AddSingleton<IRepository<LangInfoEntity>, BaseRepository<LangInfoEntity>>();
        builder.Services.AddSingleton<IRepository<WordProgressEntity>, BaseRepository<WordProgressEntity>>();
        builder.Services.AddSingleton<IRepository<ReviewLogEntity>, BaseRepository<ReviewLogEntity>>();
        builder.Services.AddSingleton<IRepository<SavedWordEntity>, BaseRepository<SavedWordEntity>>();

        builder.Services.AddSingleton<IWordProgressRepository, WordProgressRepository>();
        builder.Services.AddSingleton<IReviewLogRepository, ReviewLogRepository>();
        builder.Services.AddSingleton<ISavedWordRepository, SavedWordRepository>();
        builder.Services.AddSingleton<ISpacedRepetitionScheduler, Sm2SpacedRepetitionScheduler>();

        builder.Services.AddSingleton<IWordRepository, WordRepository>();
        builder.Services.AddSingleton<IWordService, WordService>();
        builder.Services.AddSingleton<IWordImportService, WordImportService>();
        builder.Services.AddSingleton<IWordImportRepository, WordImportRepository>();
        builder.Services.AddSingleton<ImportJsonOptions>();
        builder.Services.AddSingleton<ExportJsonOptions>();

        builder.Services.AddSingleton<IKrDictJsonSerializer, KrDictJsonSerializer>();
        builder.Services.AddSingleton<IWordImportService, WordImportService>();
        builder.Services.AddTransient<SavedViewModel>();
        builder.Services.AddTransient<SavedPage>();
        builder.Services.AddSingleton<IWordExportService, WordExportService>();
        builder.Services.AddTransient<PracticeViewModel>();
        builder.Services.AddTransient<PracticePage>();
        builder.Services.AddSingleton<IGetWordsPageQuery, GetWordsPageQuery>();
        builder.Services.AddTransient<WordsViewModel>();
        builder.Services.AddTransient<WordsPage>();
        builder.Services.AddTransient<ImportExportViewModel>();
        builder.Services.AddTransient<ImportExportPage>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddTransient<WordDetailViewModel>();
        builder.Services.AddTransient<WordDetailPage>();
        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<IAudioPlayerService, AudioPlayerService>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
        }

        return app;
    }

    static string GetDatabasePath(string fileName)
    {
        var baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "koreanApp");

        if (!Directory.Exists(baseDir))
            Directory.CreateDirectory(baseDir);

        return Path.Combine(baseDir, fileName);
    }
}