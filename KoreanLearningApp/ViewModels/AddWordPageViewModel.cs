using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Events;
using KoreanLearningApp.Models;
using KoreanLearningApp.Services;
using Microsoft.Maui.Controls;

namespace KoreanLearningApp.ViewModels;

public class AddWordPageViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _db;

    /// <summary>Просьба к View показать DisplayAlert (нужен контекст Page).</summary>
    public event EventHandler<AlertRequestEventArgs>? AlertRequested;

    public ObservableCollection<string> TypeLabels { get; } =
        new(WordTypeHelper.AllTypes.Select(t => t.Label));

    public ICommand SaveCommand { get; }

    public AddWordPageViewModel(DatabaseService db)
    {
        _db = db;
        SaveCommand = new Command(async () => await OnSaveAsync());
        SelectedTypeLabel = WordTypeHelper.AllTypes.First(t => t.Type == WordType.Other).Label;
    }

    private string _koreanText = string.Empty;
    public string KoreanText
    {
        get => _koreanText;
        set { _koreanText = value; OnPropertyChanged(); }
    }

    private string _transcriptionRuText = string.Empty;
    public string TranscriptionRuText
    {
        get => _transcriptionRuText;
        set { _transcriptionRuText = value; OnPropertyChanged(); }
    }

    private string _transcriptionEnText = string.Empty;
    public string TranscriptionEnText
    {
        get => _transcriptionEnText;
        set { _transcriptionEnText = value; OnPropertyChanged(); }
    }

    private string _translationRuText = string.Empty;
    public string TranslationRuText
    {
        get => _translationRuText;
        set { _translationRuText = value; OnPropertyChanged(); }
    }

    private string _translationEnText = string.Empty;
    public string TranslationEnText
    {
        get => _translationEnText;
        set { _translationEnText = value; OnPropertyChanged(); }
    }

    private string _categoryText = string.Empty;
    public string CategoryText
    {
        get => _categoryText;
        set { _categoryText = value; OnPropertyChanged(); }
    }

    private string _ruleExplanationText = string.Empty;
    public string RuleExplanationText
    {
        get => _ruleExplanationText;
        set { _ruleExplanationText = value; OnPropertyChanged(); }
    }

    private string _pronunciationNoteText = string.Empty;
    public string PronunciationNoteText
    {
        get => _pronunciationNoteText;
        set { _pronunciationNoteText = value; OnPropertyChanged(); }
    }

    private string? _selectedTypeLabel;
    public string? SelectedTypeLabel
    {
        get => _selectedTypeLabel;
        set { _selectedTypeLabel = value; OnPropertyChanged(); }
    }

    private async Task OnSaveAsync()
    {
        if (string.IsNullOrWhiteSpace(KoreanText))
        {
            AlertRequested?.Invoke(this, new AlertRequestEventArgs("Ошибка", "Введите слово на корейском", "ОК"));
            return;
        }

        var selectedType = SelectedTypeLabel is string label
            ? WordTypeHelper.FromLabel(label)
            : WordType.Other;

        var word = new Word
        {
            Korean = KoreanText,
            TranscriptionRu = TranscriptionRuText ?? string.Empty,
            TranscriptionEn = TranscriptionEnText ?? string.Empty,
            TranslationRu = TranslationRuText ?? string.Empty,
            TranslationEn = TranslationEnText ?? string.Empty,
            RuleExplanation = RuleExplanationText ?? string.Empty,
            Category = CategoryText ?? string.Empty,
            Type = selectedType,
            Status = LearningStatus.Learning,
            PronunciationNote = PronunciationNoteText ?? string.Empty
        };

        await _db.SaveWordAsync(word);
        await Shell.Current.GoToAsync("..");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}