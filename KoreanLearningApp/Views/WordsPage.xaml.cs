using KoreanLearningApp.Events;
using KoreanLearningApp.Services;
using KoreanLearningApp.ViewModels;

namespace KoreanLearningApp.Views;

public partial class WordsPage : ContentPage
{
    private readonly WordsPageViewModel _viewModel;

    public WordsPage(DatabaseService db)
    {
        InitializeComponent();

        _viewModel = new WordsPageViewModel(db);
        BindingContext = _viewModel;

        _viewModel.AlertRequested += OnAlertRequested;
        _viewModel.NavigationRequested += OnNavigationRequested;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadWordsAsync();
    }

    private async void OnAlertRequested(object? sender, AlertRequestEventArgs e)
    {
        await DisplayAlert(e.Title, e.Message, e.Cancel);
    }

    private async void OnNavigationRequested(object? sender, string route)
    {
        await Shell.Current.GoToAsync(route);
    }
}