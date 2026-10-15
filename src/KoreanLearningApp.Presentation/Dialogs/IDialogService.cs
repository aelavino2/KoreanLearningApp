namespace KoreanLearningApp.Presentation.Dialogs;

public interface IDialogService
{
    Task ShowAlertAsync(string title, string message);
}
