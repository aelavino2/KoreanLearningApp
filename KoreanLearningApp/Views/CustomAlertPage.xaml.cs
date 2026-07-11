namespace KoreanLearningApp.Views;

public partial class CustomAlertPage : ContentPage
{
    private TaskCompletionSource<bool>? _resultSource;

    public CustomAlertPage()
    {
        InitializeComponent();
    }

    public static async Task ShowAsync(string title, string message, string buttonText = "ОК")
    {
        var page = new CustomAlertPage();
        page.TitleLabel.Text = title;
        page.MessageLabel.Text = message;
        page.PrimaryButton.Text = buttonText;
        page._resultSource = new TaskCompletionSource<bool>();

        await Shell.Current.Navigation.PushModalAsync(page, animated: true);
        await page._resultSource.Task;
    }

    public static async Task<bool> ShowConfirmAsync(string title, string message, string confirmText = "Да", string cancelText = "Отмена")
    {
        var page = new CustomAlertPage();
        page.TitleLabel.Text = title;
        page.MessageLabel.Text = message;
        page.PrimaryButton.Text = confirmText;
        page.SecondaryButton.Text = cancelText;
        page.SecondaryButton.IsVisible = true;
        page._resultSource = new TaskCompletionSource<bool>();

        await Shell.Current.Navigation.PushModalAsync(page, animated: true);
        return await page._resultSource.Task;
    }

    private async void OnPrimaryClicked(object sender, EventArgs e)
    {
        _resultSource?.TrySetResult(true);
        await Shell.Current.Navigation.PopModalAsync(animated: true);
    }

    private async void OnSecondaryClicked(object sender, EventArgs e)
    {
        _resultSource?.TrySetResult(false);
        await Shell.Current.Navigation.PopModalAsync(animated: true);
    }
}