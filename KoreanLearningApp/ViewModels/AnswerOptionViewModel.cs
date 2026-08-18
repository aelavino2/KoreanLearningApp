using CommunityToolkit.Mvvm.ComponentModel;

namespace KoreanLearningApp.ViewModels;

public enum AnswerOptionState
{
    Default,
    Correct,
    Incorrect
}

public partial class AnswerOptionViewModel : ObservableObject
{
    public string Text { get; }

    public bool IsCorrect { get; }

    [ObservableProperty]
    private AnswerOptionState _state = AnswerOptionState.Default;

    public AnswerOptionViewModel(string text, bool isCorrect)
    {
        Text = text;
        IsCorrect = isCorrect;
    }
}