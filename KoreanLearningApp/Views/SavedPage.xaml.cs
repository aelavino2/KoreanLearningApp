using KoreanLearningApp.ViewModels;

namespace KoreanLearningApp.Views;

public partial class SavedPage : ContentPage
{
    private readonly SavedViewModel _viewModel;

    public SavedPage(SavedViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSavedAsync();
    }
}