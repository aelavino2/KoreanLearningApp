using System.ComponentModel;
using System.Runtime.CompilerServices;
using KoreanLearningApp.Models;
namespace KoreanLearningApp.ViewModels;
public class WordCardViewModel : INotifyPropertyChanged
{
    private readonly Word _word;
    private bool _isRevealed;
    private bool _koreanToRussian;
    private int _number;

    public WordCardViewModel(Word word, bool koreanToRussian)
    {
        _word = word;
        _koreanToRussian = koreanToRussian;
    }

    public Word UnderlyingWord => _word;

    public int Number
    {
        get => _number;
        set { _number = value; OnPropertyChanged(); }
    }

    public string FrontText => _koreanToRussian ? _word.Korean : _word.TranslationRu;
    public string TranscriptionText => (_koreanToRussian || IsRevealed) ? _word.TranscriptionRu : string.Empty;
    public string BackText => _koreanToRussian ? _word.TranslationRu : _word.Korean;

    public string BoxText => SpacedRepetitionHelper.ProgressStars(_word.LeitnerBox);

    public bool IsDueForReview => SpacedRepetitionHelper.IsDue(_word);
    public string RuleText => _word.RuleExplanation;
    public bool HasRule => !string.IsNullOrWhiteSpace(_word.RuleExplanation);

    public string PronunciationNoteText => _word.PronunciationNote;
    public bool HasPronunciationNote => !string.IsNullOrWhiteSpace(_word.PronunciationNote);

    public string CategoryText => _word.Category;
    public bool HasCategory => !string.IsNullOrWhiteSpace(_word.Category);

    public string TypeText => WordTypeHelper.ToLabel(_word.Type);

    public bool IsLearned => _word.Status == LearningStatus.Learned;
    public string StatusButtonText => IsLearned ? "Выучено" : "Учу";

    public void ToggleStatus()
    {
        _word.Status = IsLearned ? LearningStatus.Learning : LearningStatus.Learned;
        OnPropertyChanged(nameof(IsLearned));
        OnPropertyChanged(nameof(StatusButtonText));
    }

    public bool IsRevealed
    {
        get => _isRevealed;
        set
        {
            _isRevealed = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowButton));
            OnPropertyChanged(nameof(TranscriptionText));
        }
    }
    public bool ShowButton => !IsRevealed;

    public void SetDirection(bool koreanToRussian)
    {
        _koreanToRussian = koreanToRussian;
        IsRevealed = false;
        OnPropertyChanged(nameof(FrontText));
        OnPropertyChanged(nameof(BackText));
        OnPropertyChanged(nameof(TranscriptionText));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}