using KoreanLearningApp.Presentation.Dialogs;
using Microsoft.Extensions.Logging;

namespace KoreanLearningApp.Diagnostics;

/// <summary>
/// Перехватывает необработанные исключения: пишет их в лог и показывает пользователю сообщение вместо падения.
/// На iOS/MacCatalyst перехватить можно только логированием через <see cref="AppDomain.UnhandledException"/>.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IDialogService dialogService)
{
    private const string AlertTitle = "Ошибка";
    private const string AlertMessage = "Что-то пошло не так. Подробности записаны в лог приложения.";

    public void Register()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            logger.LogCritical(e.ExceptionObject as Exception, "Unhandled exception, terminating: {IsTerminating}", e.IsTerminating);

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            logger.LogError(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };

#if WINDOWS
        Microsoft.UI.Xaml.Application.Current.UnhandledException += (_, e) =>
        {
            e.Handled = true;
            Recover(e.Exception);
        };
#elif ANDROID
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (_, e) =>
        {
            e.Handled = true;
            Recover(e.Exception);
        };
#endif
    }

    private void Recover(Exception exception)
    {
        logger.LogError(exception, "Unhandled UI exception");
        MainThread.BeginInvokeOnMainThread(ShowAlert);
    }

    private async void ShowAlert()
    {
        try
        {
            await dialogService.ShowAlertAsync(AlertTitle, AlertMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to show error alert");
        }
    }
}
