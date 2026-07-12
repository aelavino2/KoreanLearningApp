using CommunityToolkit.Maui;
using KoreanLearningApp.Services;
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
        builder.Services.AddTransient<ImportExportPage>();
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<QuizSessionSettings>();
        builder.Services.AddTransient<WordsPage>();
        builder.Services.AddTransient<AddWordPage>();
        builder.Services.AddTransient<QuizSettingsPage>();
        builder.Services.AddTransient<QuizPage>();
        return builder.Build();
    }
}