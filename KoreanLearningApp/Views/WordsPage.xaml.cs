using KoreanLearningApp.Helpers;
using KoreanLearningApp.ViewModels;

namespace KoreanLearningApp.Views;

public partial class WordsPage : ContentPage
{
    private readonly WordsViewModel _viewModel;

    public WordsPage(WordsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadWordsAsync();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        var span = ResponsiveLayout.GetColumnSpan(width);

        if (WordsCollectionView.ItemsLayout is GridItemsLayout gridLayout && gridLayout.Span != span)
            gridLayout.Span = span;
    }
}