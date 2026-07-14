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
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, DatabaseConstants.DatabaseFileName);
        builder.Services.AddSingleton(new DbContext(dbPath));

        builder.Services.AddSingleton(sp =>
        {
            var dbContext = sp.GetRequiredService<DbContext>();
            return dbContext.GetConnectionAsync().GetAwaiter().GetResult();
        });

        builder.Services.AddSingleton<BaseRepository<WordEntity>>();
        builder.Services.AddSingleton<IWordRepository, WordRepository>();
        builder.Services.AddSingleton<IWordService, WordService>();
        builder.Services.AddTransient<WordsViewModel>();
        builder.Services.AddTransient<WordsPage>();

        return builder.Build();
    }
}