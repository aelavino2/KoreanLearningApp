using KoreanLearningApp.ViewModels;

namespace KoreanLearningApp.Views;

public partial class WordDetailPage : ContentPage
{
    public WordDetailPage(WordDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}