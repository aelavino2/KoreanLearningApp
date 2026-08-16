using System.Diagnostics;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.ViewModels;

public partial class PracticeCardViewModel : ObservableObject, IDisposable
{
    public const int QuestionSeconds = 10;

    private const int TimerTickIntervalMs = 50;

    private readonly ISavedWordsService _savedWordsService;
    private readonly Action<PracticeCardViewModel, bool> _onAnswered;
    private readonly Stopwatch _stopwatch = new();
    private System.Timers.Timer? _timer;
    private bool _disposed;

    public int WordId { get; }
    public string Korean { get; }
    public string Ru { get; }
    public string En { get; }
    public string PartOfSpeechDisplay { get; }

    public bool IsNew { get; }

    public string QuestionText { get; }

    public List<AnswerOptionViewModel> Options { get; }

    [ObservableProperty]
    private bool _isSaved;

    [ObservableProperty]
    private bool _isSaveBusy;

    [ObservableProperty]
    private bool _isAnswered;

    [ObservableProperty]
    private bool? _wasCorrect;

    [ObservableProperty]
    private int _timeLeft = QuestionSeconds;

    [ObservableProperty]
    private double _timeProgress = 1.0;

    public Color TimerColor => TimeLeft <= 3 ? Color.FromArgb("#EF4444") : Color.FromArgb("#3B82F6");

    partial void OnTimeLeftChanged(int value) => OnPropertyChanged(nameof(TimerColor));

    public PracticeCardViewModel(PracticeCard card, ISavedWordsService savedWordsService,
        bool isSavedInitially, string questionText,
        List<AnswerOptionViewModel> options, Action<PracticeCardViewModel, bool> onAnswered)
    {
        _savedWordsService = savedWordsService;
        _onAnswered = onAnswered;

        WordId = card.Word.Id;
        Korean = card.Word.Korean;

        var firstSense = card.Word.KrDict?.Senses.FirstOrDefault();
        Ru = firstSense?.Ru?.Word ?? string.Empty;
        En = firstSense?.En?.Word ?? string.Empty;

        PartOfSpeechDisplay = PartOfSpeechMapper.Parse(card.Word.PartOfSpeech).ToRussian();
        IsNew = card.IsNew;
        IsSaved = isSavedInitially;

        QuestionText = questionText;
        Options = options;
    }

    public void StartTimer()
    {
        StopTimer();

        TimeLeft = QuestionSeconds;
        TimeProgress = 1.0;

        _stopwatch.Restart();

        _timer = new System.Timers.Timer(TimerTickIntervalMs);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
        _timer.Start();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (IsAnswered || _disposed)
                return;

            var elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
            var remainingSeconds = Math.Max(0, QuestionSeconds - elapsedSeconds);

            TimeProgress = remainingSeconds / QuestionSeconds;

            var wholeSecondsLeft = (int)Math.Ceiling(remainingSeconds);
            if (wholeSecondsLeft != TimeLeft)
                TimeLeft = wholeSecondsLeft;

            if (remainingSeconds <= 0)
                Reveal(selected: null);
        });
    }

    [RelayCommand]
    private void SelectAnswer(AnswerOptionViewModel option)
    {
        if (IsAnswered || option is null)
            return;

        Reveal(option);
    }

    private void Reveal(AnswerOptionViewModel? selected)
    {
        StopTimer();

        var correctOption = Options.FirstOrDefault(o => o.IsCorrect);
        if (correctOption is not null)
            correctOption.State = AnswerOptionState.Correct;

        if (selected is not null && !selected.IsCorrect)
            selected.State = AnswerOptionState.Incorrect;

        var correct = selected?.IsCorrect ?? false;
        WasCorrect = correct;
        IsAnswered = true;

        _onAnswered(this, correct);
    }

    public void StopTimer()
    {
        _stopwatch.Stop();

        if (_timer is null)
            return;

        _timer.Elapsed -= OnTimerElapsed;
        _timer.Stop();
        _timer.Dispose();
        _timer = null;
    }

    [RelayCommand]
    private async Task ToggleSaveAsync()
    {
        if (IsSaveBusy)
            return;

        try
        {
            IsSaveBusy = true;
            IsSaved = await _savedWordsService.ToggleAsync(WordId);
        }
        finally
        {
            IsSaveBusy = false;
        }
    }

    public void Dispose()
    {
        _disposed = true;
        StopTimer();
    }
}