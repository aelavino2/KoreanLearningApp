using KoreanLearningApp.Services;
using KoreanLearningApp.ViewModels;

namespace KoreanLearningApp.Views;

public partial class QuizPage : ContentPage
{
    private readonly QuizPageViewModel _viewModel;

    public QuizPage(DatabaseService db, QuizSessionSettings settings)
    {
        InitializeComponent();
        _viewModel = new QuizPageViewModel(db, settings);
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.StartQuizAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.StopTimer();
    }
}