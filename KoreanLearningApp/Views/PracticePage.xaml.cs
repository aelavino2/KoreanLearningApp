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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Каждый заход на страницу начинается с экрана настроек — VM транзиентный
        // (пересоздаётся при навигации), но на случай, если это изменится, сбрасываем явно.
        _viewModel.BackToSetupCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Останавливаем таймер вопроса, если пользователь ушёл со страницы
        // посреди квиза (иначе таймер карточки продолжит тикать в фоне).
        _viewModel.StopSession();
    }
}