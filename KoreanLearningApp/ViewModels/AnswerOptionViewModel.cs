using CommunityToolkit.Mvvm.ComponentModel;

namespace KoreanLearningApp.ViewModels;

/// <summary>Состояние варианта ответа для подсветки после выбора/окончания времени.</summary>
public enum AnswerOptionState
{
    /// <summary>Ответ ещё не дан — вариант выглядит нейтрально.</summary>
    Default,

    /// <summary>Правильный вариант — подсвечивается зелёным.</summary>
    Correct,

    /// <summary>Вариант, который выбрал пользователь и он оказался неверным — подсвечивается красным.</summary>
    Incorrect
}

/// <summary>Один из 4 вариантов ответа в карточке квиза.</summary>
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
