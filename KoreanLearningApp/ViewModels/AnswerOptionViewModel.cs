using CommunityToolkit.Mvvm.ComponentModel;

namespace KoreanLearningApp.ViewModels;

public partial class AnswerOptionViewModel(string text, bool isCorrect) : ObservableObject
{
    public string Text { get; } = text;

    public bool IsCorrect { get; } = isCorrect;

    [ObservableProperty]
    private bool _isRevealedCorrect;

    [ObservableProperty]
    private bool _isRevealedWrong;

    public void Reveal(bool wasSelected)
    {
        IsRevealedCorrect = IsCorrect;
        IsRevealedWrong = wasSelected && !IsCorrect;
    }
}
