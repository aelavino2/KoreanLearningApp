using CommunityToolkit.Maui;
using KoreanLearningApp.Infrastructure.Persistence;
using KoreanLearningApp.Infrastructure.Persistence.Entities;
using KoreanLearningApp.Infrastructure.Persistence.Repositories;
using KoreanLearningApp.Services;
using KoreanLearningApp.Services.Abstractions;
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
        builder.Services.AddSingleton<BaseRepository<WordEntity>>();
        builder.Services.AddSingleton<IWordRepository, WordRepository>();
        builder.Services.AddSingleton<IWordService, WordService>();
        builder.Services.AddTransient<WordsViewModel>();
        builder.Services.AddTransient<WordsPage>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
            Task.Run(() => dbContext.SeedIfEmptyAsync()).GetAwaiter().GetResult();
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