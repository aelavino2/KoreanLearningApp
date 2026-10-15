using KoreanLearningApp.Helpers;
using KoreanLearningApp.Presentation.ViewModels;

namespace KoreanLearningApp.Views;

public partial class WordsPage : ContentPage
{
    private readonly WordsViewModel _viewModel;

    public WordsPage(WordsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.ScrollToTopRequested += OnScrollToTopRequested;
    }

    private void OnScrollToTopRequested(object? sender, EventArgs e)
    {
        if (_viewModel.Words.Count > 0)
            WordsCollectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshAsync();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        var span = ResponsiveLayout.GetColumnSpan(width);

        if (WordsCollectionView.ItemsLayout is GridItemsLayout gridLayout && gridLayout.Span != span)
            gridLayout.Span = span;
    }
}