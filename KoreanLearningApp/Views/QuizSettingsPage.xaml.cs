using KoreanLearningApp.Services;
using KoreanLearningApp.ViewModels;

namespace KoreanLearningApp.Views;

public partial class QuizSettingsPage : ContentPage
{
    public QuizSettingsPage(QuizSessionSettings settings)
    {
        InitializeComponent();
        BindingContext = new QuizSettingsPageViewModel(settings);
    }
}