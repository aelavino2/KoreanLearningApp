using CommunityToolkit.Maui;
using KoreanLearningApp.Infrastructure.Import;
using KoreanLearningApp.Infrastructure.Persistence;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Repositories;
using KoreanLearningApp.Infrastructure.Persistence.Repositories.Abstractions;
using KoreanLearningApp.Navigation;
using KoreanLearningApp.Services;
using KoreanLearningApp.Services.Abstractions.Repositories;
using KoreanLearningApp.Services.Abstractions.Services;
using KoreanLearningApp.ViewModels;
using KoreanLearningApp.Views;
using Microsoft.Extensions.Logging;
namespace KoreanLearningApp;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
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
        builder.Services.AddSingleton<IRepository<WordSenseEntity>, BaseRepository<WordSenseEntity>>();
        builder.Services.AddSingleton<IWordRepository, WordRepository>();
        builder.Services.AddSingleton<IWordService, WordService>();
        builder.Services.AddSingleton<IWordImportService, WordImportService>();
        builder.Services.AddTransient<WordsViewModel>();
        builder.Services.AddTransient<WordsPage>();
        builder.Services.AddTransient<ImportExportViewModel>();
        builder.Services.AddTransient<ImportExportPage>();
        builder.Services.AddSingleton<IWordImportRepository, WordImportRepository>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddTransient<WordDetailViewModel>();
        builder.Services.AddTransient<WordDetailPage>();


        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();

            Task.Run(async () => await dbContext.SeedIfEmptyAsync())
                .GetAwaiter()
                .GetResult();
        }

        return app;
    }

    static string GetDatabasePath(string fileName)
    {
        string baseDir;

#if WINDOWS && DEBUG
    var projectRoot = FindProjectRoot();
    baseDir = projectRoot is not null
        ? Path.Combine(projectRoot, "Database")
        : Path.Combine(FileSystem.AppDataDirectory, "Database");
#else
        baseDir = Path.Combine(FileSystem.AppDataDirectory, "Database");
#endif

        if (!Directory.Exists(baseDir))
            Directory.CreateDirectory(baseDir);

        return Path.Combine(baseDir, fileName);
    }

#if WINDOWS && DEBUG
static string? FindProjectRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir is not null)
    {
        if (dir.GetFiles("*.csproj").Length > 0)
            return dir.FullName;
        dir = dir.Parent;
    }
    return null;
}
#endif
}