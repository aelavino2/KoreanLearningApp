using KoreanLearningApp.ViewModels;

namespace KoreanLearningApp.Views;

public partial class PracticePage : ContentPage
{
    private readonly PracticeViewModel _viewModel;

    public PracticePage(PracticeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.BackToSetupCommand.Execute(null);
    }
}
