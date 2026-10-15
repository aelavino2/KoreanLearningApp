using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Presentation.Navigation;
using KoreanLearningApp.Presentation.ViewModels;

namespace KoreanLearningApp.Views;

[QueryProperty(nameof(Word), NavigationParameters.Word)]
public partial class WordDetailPage : ContentPage
{
    private readonly WordDetailViewModel _viewModel;

    public WordDetailPage(WordDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public Word Word
    {
        get => _viewModel.Word;
        set => _viewModel.Word = value;
    }
}
