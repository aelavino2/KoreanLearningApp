using System.Diagnostics;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KoreanLearningApp.Domain.Models;
using KoreanLearningApp.Domain.Models.Enums;
using KoreanLearningApp.Services.Abstractions.Services;

namespace KoreanLearningApp.ViewModels;

/// <summary>
/// Одна карточка-вопрос квиза: слово/перевод для показа, 4 варианта ответа
/// (1 верный + 3 отвлекающих), обратный отсчёт времени на вопрос и подсветка
/// результата после ответа или истечения времени.
/// </summary>
public partial class PracticeCardViewModel : ObservableObject, IDisposable
{
    /// <summary>Сколько секунд даётся на один вопрос.</summary>
    public const int QuestionSeconds = 10;

    /// <summary>
    /// Интервал тика таймера в мс. Раньше было 1000 (раз в секунду), из-за чего
    /// прогресс-бар двигался скачками. Тикаем чаще и считаем прогресс по реальному
    /// прошедшему времени (Stopwatch), а не по количеству тиков — так полоса едет
    /// плавно, а не дёргается.
    /// </summary>
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

    /// <summary>true — слово ещё не начато (Progress был null), можно показать бейдж "новое"</summary>
    public bool IsNew { get; }

    /// <summary>Текст вопроса — то, что показываем крупно (корейское слово или перевод).</summary>
    public string QuestionText { get; }

    /// <summary>4 варианта ответа (перемешаны), ровно один из них IsCorrect == true.</summary>
    public List<AnswerOptionViewModel> Options { get; }

    [ObservableProperty]
    private bool _isSaved;

    [ObservableProperty]
    private bool _isSaveBusy;

    /// <summary>Ответил ли пользователь (или истекло время) — варианты заблокированы, показана подсветка.</summary>
    [ObservableProperty]
    private bool _isAnswered;

    /// <summary>Оказался ли ответ верным. Null, пока ответа не было.</summary>
    [ObservableProperty]
    private bool? _wasCorrect;

    /// <summary>Секунд осталось на текущий вопрос (обратный отсчёт от QuestionSeconds до 0).</summary>
    [ObservableProperty]
    private int _timeLeft = QuestionSeconds;

    /// <summary>Доля оставшегося времени (0..1) — удобно для ProgressBar.</summary>
    [ObservableProperty]
    private double _timeProgress = 1.0;

    /// <summary>Цвет индикатора времени: обычный, пока времени много, красный — когда осталось мало.</summary>
    public Color TimerColor => TimeLeft <= 3 ? Color.FromArgb("#EF4444") : Color.FromArgb("#3B82F6");

    partial void OnTimeLeftChanged(int value) => OnPropertyChanged(nameof(TimerColor));

    /// <param name="card">Карточка слова + прогресс из сессии практики.</param>
    /// <param name="savedWordsService">Сервис сохранённых слов — нужен для тоггла звёздочки.</param>
    /// <param name="isSavedInitially">
    /// Статус сохранения, посчитанный один раз batch-запросом в PracticeViewModel
    /// (через GetSavedWordIdSetAsync), а не отдельным IsSavedAsync на каждую карточку.
    /// </param>
    /// <param name="questionText">Текст вопроса, уже выбранный PracticeViewModel по направлению квиза.</param>
    /// <param name="options">4 готовых варианта ответа (перемешаны).</param>
    /// <param name="onAnswered">
    /// Колбэк в PracticeViewModel: (эта карточка, верный ли ответ) — используется для
    /// начисления очков, отправки SM-2 оценки и перехода к следующему вопросу.
    /// </param>
    public PracticeCardViewModel(
        PracticeCard card,
        ISavedWordsService savedWordsService,
        bool isSavedInitially,
        string questionText,
        List<AnswerOptionViewModel> options,
        Action<PracticeCardViewModel, bool> onAnswered)
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

    /// <summary>Запускает обратный отсчёт на вопрос. Вызывается, когда карточка становится текущей.</summary>
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

            // Прогресс считаем по реальному прошедшему времени, а не по числу тиков —
            // так полоса едет плавно вне зависимости от нагрузки на UI-поток.
            var elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
            var remainingSeconds = Math.Max(0, QuestionSeconds - elapsedSeconds);

            TimeProgress = remainingSeconds / QuestionSeconds;

            // Целое число секунд для текстового лейбла обновляем только когда оно
            // реально меняется, чтобы не дёргать биндинг 20 раз в секунду.
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

    /// <summary>Показывает результат: подсвечивает верный вариант зелёным, а неверно выбранный — красным.</summary>
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