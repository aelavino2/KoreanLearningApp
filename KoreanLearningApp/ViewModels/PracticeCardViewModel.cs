using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Dialogs;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.ViewModels;

public sealed partial class PracticeCardViewModel : ObservableObject, IDisposable
{
    public const int QuestionSeconds = 10;

    private const int WarningSeconds = 3;
    private static readonly TimeSpan TickInterval = TimeSpan.FromMilliseconds(50);

    private readonly ISavedWordsService _savedWordsService;
    private readonly IDialogService _dialogService;
    private readonly Action<PracticeCardViewModel, bool> _onAnswered;
    private PeriodicTimer? _timer;

    public PracticeCardViewModel(QuizQuestion question, bool isSaved, ISavedWordsService savedWordsService,
        IDialogService dialogService, Action<PracticeCardViewModel, bool> onAnswered)
    {
        _savedWordsService = savedWordsService;
        _dialogService = dialogService;
        _onAnswered = onAnswered;

        WordId = question.Card.Word.Id;
        IsNew = question.Card.IsNew;
        PartOfSpeechDisplay = question.Card.Word.PartOfSpeech.ToRussian();
        QuestionText = question.QuestionText;
        Options = question.Options.Select(o => new AnswerOptionViewModel(o.Text, o.IsCorrect)).ToList();
        _isSaved = isSaved;
    }

    public int WordId { get; }
    public bool IsNew { get; }
    public string PartOfSpeechDisplay { get; }
    public string QuestionText { get; }
    public IReadOnlyList<AnswerOptionViewModel> Options { get; }

    [ObservableProperty]
    private bool _isSaved;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SelectAnswerCommand))]
    private bool _isAnswered;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTimeRunningOut))]
    private int _timeLeft = QuestionSeconds;

    [ObservableProperty]
    private double _timeProgress = 1.0;

    public bool IsTimeRunningOut => TimeLeft <= WarningSeconds;

    /// <summary>
    /// Запускает обратный отсчёт. Вызывать из UI-потока: тики продолжаются в его контексте синхронизации.
    /// </summary>
    public void StartTimer()
    {
        StopTimer();
        TimeLeft = QuestionSeconds;
        TimeProgress = 1.0;

        _timer = new PeriodicTimer(TickInterval);
        _ = RunTimerAsync(_timer, Stopwatch.GetTimestamp());
    }

    public void Dispose() => StopTimer();

    private async Task RunTimerAsync(PeriodicTimer timer, long startedAt)
    {
        while (await timer.WaitForNextTickAsync())
        {
            var remainingSeconds = Math.Max(0, QuestionSeconds - Stopwatch.GetElapsedTime(startedAt).TotalSeconds);
            TimeProgress = remainingSeconds / QuestionSeconds;
            TimeLeft = (int)Math.Ceiling(remainingSeconds);

            if (remainingSeconds <= 0)
                Reveal(selected: null);
        }
    }

    private void StopTimer()
    {
        _timer?.Dispose();
        _timer = null;
    }

    private bool CanSelectAnswer(AnswerOptionViewModel? option) => !IsAnswered;

    [RelayCommand(CanExecute = nameof(CanSelectAnswer))]
    private void SelectAnswer(AnswerOptionViewModel? option)
    {
        if (option is null)
            return;

        Reveal(option);
    }

    private void Reveal(AnswerOptionViewModel? selected)
    {
        if (IsAnswered)
            return;

        StopTimer();

        foreach (var option in Options)
            option.Reveal(wasSelected: option == selected);

        IsAnswered = true;
        _onAnswered(this, selected?.IsCorrect ?? false);
    }

    [RelayCommand]
    private async Task ToggleSaveAsync()
    {
        try
        {
            IsSaved = await _savedWordsService.ToggleAsync(WordId);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowAlertAsync("Ошибка", $"Не удалось сохранить слово: {ex.Message}");
        }
    }
}
