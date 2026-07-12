using KoreanLearningApp.Events;
using KoreanLearningApp.Services;
using KoreanLearningApp.ViewModels;

namespace KoreanLearningApp.Views;

public partial class AddWordPage : ContentPage
{
    private readonly AddWordPageViewModel _viewModel;

    public AddWordPage(DatabaseService db)
    {
        InitializeComponent();
        _viewModel = new AddWordPageViewModel(db);
        BindingContext = _viewModel;
        _viewModel.AlertRequested += OnAlertRequested;
    }

    private async void OnAlertRequested(object? sender, AlertRequestEventArgs e)
    {
        await DisplayAlert(e.Title, e.Message, e.Cancel);
    }
}