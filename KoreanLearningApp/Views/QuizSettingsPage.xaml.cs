using KoreanLearningApp.Models;
using KoreanLearningApp.Services;
namespace KoreanLearningApp.Views;

public partial class QuizSettingsPage : ContentPage
{
    private readonly QuizSessionSettings _settings;
    private QuizDifficulty _selectedDifficulty = QuizDifficulty.Normal;

    public QuizSettingsPage(QuizSessionSettings settings)
    {
        InitializeComponent();
        _settings = settings;
    }

    private void OnNormalClicked(object sender, EventArgs e)
    {
        _selectedDifficulty = QuizDifficulty.Normal;
        NormalButton.BackgroundColor = (Color)this.Resources["JadeColor"];
        NormalButton.TextColor = Colors.White;
        HardButton.BackgroundColor = (Color)this.Resources["CardColor"];
        HardButton.TextColor = (Color)this.Resources["JadeColor"];
        DifficultyDescriptionLabel.Text = "Выбор перевода из четырёх вариантов.";
    }

    private void OnHardClicked(object sender, EventArgs e)
    {
        _selectedDifficulty = QuizDifficulty.Hard;
        HardButton.BackgroundColor = (Color)this.Resources["JadeColor"];
        HardButton.TextColor = Colors.White;
        NormalButton.BackgroundColor = (Color)this.Resources["CardColor"];
        NormalButton.TextColor = (Color)this.Resources["JadeColor"];
        DifficultyDescriptionLabel.Text = "Перевод показан на русском — слово нужно написать по-корейски с клавиатуры хангыля.";
    }

    private void OnCountChanged(object sender, ValueChangedEventArgs e)
    {
        var count = (int)Math.Round(e.NewValue);
        CountLabel.Text = $"{count} слов";
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        _settings.Difficulty = _selectedDifficulty;
        _settings.WordCount = (int)Math.Round(CountSlider.Value);
        await Shell.Current.GoToAsync(nameof(QuizPage));
    }
}