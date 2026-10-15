using KoreanLearningApp.Presentation.Dialogs;

namespace KoreanLearningApp.Dialogs;

public sealed class ShellDialogService : IDialogService
{
    private const string AcceptButtonText = "OK";

    public Task ShowAlertAsync(string title, string message) =>
        Shell.Current.DisplayAlertAsync(title, message, AcceptButtonText);
}
