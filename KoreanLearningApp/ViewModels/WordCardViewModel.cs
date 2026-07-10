using System.ComponentModel;
using System.Runtime.CompilerServices;
using KoreanLearningApp.Models;

namespace KoreanLearningApp.ViewModels;

public class WordCardViewModel : INotifyPropertyChanged
{
    private readonly Word _word;
    private bool _isRevealed;
    private bool _koreanToRussian;

    public WordCardViewModel(Word word, bool koreanToRussian)
    {
        _word = word;
        _koreanToRussian = koreanToRussian;
    }

    // То, что видно сразу
    public string FrontText => _koreanToRussian ? _word.Korean : _word.TranslationRu;

    // Транскрипция — тоже видна сразу, помогает с чтением
    public string TranscriptionText => _word.TranscriptionRu;

    // Скрыто, пока не нажали кнопку
    public string BackText => _koreanToRussian ? _word.TranslationRu : _word.Korean;

    public bool IsRevealed
    {
        get => _isRevealed;
        set { _isRevealed = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowButton)); }
    }

    public bool ShowButton => !IsRevealed;

    public void SetDirection(bool koreanToRussian)
    {
        _koreanToRussian = koreanToRussian;
        IsRevealed = false;
        OnPropertyChanged(nameof(FrontText));
        OnPropertyChanged(nameof(BackText));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}