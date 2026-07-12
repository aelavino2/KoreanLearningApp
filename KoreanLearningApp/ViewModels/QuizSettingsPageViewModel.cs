using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using KoreanLearningApp.Models;
using KoreanLearningApp.Services;
using KoreanLearningApp.Views;
using Microsoft.Maui.Controls;

namespace KoreanLearningApp.ViewModels;

public class QuizSettingsPageViewModel : INotifyPropertyChanged
{
    private readonly QuizSessionSettings _settings;
    private QuizDifficulty _selectedDifficulty = QuizDifficulty.Normal;
    private double _wordCount = 10;

    public ICommand SelectNormalCommand { get; }
    public ICommand SelectHardCommand { get; }
    public ICommand StartCommand { get; }

    public QuizSettingsPageViewModel(QuizSessionSettings settings)
    {
        _settings = settings;

        SelectNormalCommand = new Command(() => SetDifficulty(QuizDifficulty.Normal));
        SelectHardCommand = new Command(() => SetDifficulty(QuizDifficulty.Hard));
        StartCommand = new Command(async () => await OnStartAsync());
    }

    public bool IsNormalSelected => _selectedDifficulty == QuizDifficulty.Normal;
    public bool IsHardSelected => _selectedDifficulty == QuizDifficulty.Hard;

    public string DifficultyDescriptionText => _selectedDifficulty == QuizDifficulty.Normal
        ? "Выбор перевода из четырёх вариантов."
        : "Перевод показан на русском — слово нужно написать по-корейски с клавиатуры хангыля.";

    public double WordCount
    {
        get => _wordCount;
        set
        {
            if (_wordCount.Equals(value)) return;
            _wordCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CountText));
        }
    }

    public string CountText => $"{(int)Math.Round(WordCount)} слов";

    private void SetDifficulty(QuizDifficulty difficulty)
    {
        _selectedDifficulty = difficulty;
        OnPropertyChanged(nameof(IsNormalSelected));
        OnPropertyChanged(nameof(IsHardSelected));
        OnPropertyChanged(nameof(DifficultyDescriptionText));
    }

    private async Task OnStartAsync()
    {
        _settings.Difficulty = _selectedDifficulty;
        _settings.WordCount = (int)Math.Round(WordCount);
        await Shell.Current.GoToAsync(nameof(QuizPage));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}