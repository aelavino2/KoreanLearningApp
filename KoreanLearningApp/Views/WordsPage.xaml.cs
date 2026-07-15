// Views/WordsPage.xaml.cs
using KoreanLearningApp.ViewModels;

namespace KoreanLearningApp.Views;

public partial class WordsPage : ContentPage
{
    private readonly WordsViewModel _viewModel;

    // ViewModel приходит через DI (см. регистрацию ниже) Ч страница ничего сама не создаЄт
    public WordsPage(WordsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    // OnAppearing вызываетс€ каждый раз при переходе на страницу Ч хорошее место дл€ загрузки данных
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadWordsAsync();
    }

    // Ћовим изменение размера окна (актуально дл€ десктопа при ресайзе Ч на телефоне почти не сработает,
    // ориентаци€ экрана мен€етс€ отдельным событием, если понадобитс€ Ч добавим)
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        // ”словна€ граница: уже Ч телефон (1 колонка), шире Ч десктоп/планшет (несколько колонок)
        var span = width switch
        {
            < 600 => 1,
            < 900 => 2,
            _ => 3
        };

        if (WordsCollectionView.ItemsLayout is GridItemsLayout gridLayout && gridLayout.Span != span)
            gridLayout.Span = span;
    }
}