using KoreanLearningApp.Models;
using KoreanLearningApp.Services;
namespace KoreanLearningApp.Views;

public partial class QuizPage : ContentPage
{
    private const int QuestionSeconds = 20;
    private const int MinWordsForQuiz = 4;

    private readonly DatabaseService _db;
    private readonly QuizSessionSettings _settings;
    private readonly Random _random = new();

    private List<Word> _allWords = new();
    private List<Word> _quizQueue = new();
    private int _currentIndex;
    private int _correctCount;
    private double _timeLeft;
    private IDispatcherTimer? _timer;
    private bool _answerLocked;
    private Word? _currentWord;

    // Состояние ввода для клавиатуры хангыля (сложный режим)
    private int? _pendingCho;
    private int? _pendingJung;
    private int? _pendingJong;
    private readonly System.Text.StringBuilder _committedAnswer = new();

    public QuizPage(DatabaseService db, QuizSessionSettings settings)
    {
        InitializeComponent();
        _db = db;
        _settings = settings;
        BuildHangulKeyboard();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        NormalModeGrid.IsVisible = _settings.Difficulty == QuizDifficulty.Normal;
        HardModeLayout.IsVisible = _settings.Difficulty == QuizDifficulty.Hard;
        await StartQuizAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopTimer();
    }

    private void BuildHangulKeyboard()
    {
        foreach (var ch in HangulComposer.Cho)
            ChoKeyboard.Children.Add(CreateKeyButton(ch, OnChoKeyClicked));

        foreach (var ch in HangulComposer.Jung)
            JungKeyboard.Children.Add(CreateKeyButton(ch, OnJungKeyClicked));

        foreach (var ch in HangulComposer.CommonJong)
            JongKeyboard.Children.Add(CreateKeyButton(ch, OnJongKeyClicked));
    }

    private Button CreateKeyButton(string text, EventHandler handler)
    {
        var button = new Button
        {
            Text = text,
            WidthRequest = 46,
            HeightRequest = 46,
            Margin = new Thickness(3),
            Padding = 0,
            FontSize = 16,
            CornerRadius = 10,
            BackgroundColor = (Color)this.Resources["CardColor"],
            TextColor = (Color)this.Resources["InkColor"]
        };
        button.Clicked += handler;
        return button;
    }

    private async Task StartQuizAsync()
    {
        _allWords = await _db.GetWordsAsync();

        if (_allWords.Count < MinWordsForQuiz)
        {
            await CustomAlertPage.ShowAsync(
                "Недостаточно слов",
                $"Для проверки знаний нужно минимум {MinWordsForQuiz} слов в словаре. Сейчас их: {_allWords.Count}.",
                "Понятно");
            await Shell.Current.GoToAsync("..");
            return;
        }

        var sessionSize = Math.Clamp(_settings.WordCount, 1, 30);
        var due = await _db.GetDueWordsAsync(sessionSize);
        _quizQueue = due.OrderBy(_ => _random.Next()).Take(sessionSize).ToList();

        if (_quizQueue.Count == 0)
        {
            await CustomAlertPage.ShowAsync(
                "Пока нечего повторять",
                "Все слова уже повторены недавно. Возвращайтесь позже, либо добавьте новые слова.",
                "Понятно");
            await Shell.Current.GoToAsync("..");
            return;
        }

        _currentIndex = 0;
        _correctCount = 0;
        ScoreLabel.Text = "Правильно: 0";

        ShowQuestion();
    }

    private void ShowQuestion()
    {
        if (_currentIndex >= _quizQueue.Count)
        {
            FinishQuiz();
            return;
        }

        _answerLocked = false;
        _currentWord = _quizQueue[_currentIndex];
        ProgressLabel.Text = $"Вопрос {_currentIndex + 1} из {_quizQueue.Count}";
        BoxLabel.Text = SpacedRepetitionHelper.ProgressStars(_currentWord.LeitnerBox);

        if (_settings.Difficulty == QuizDifficulty.Normal)
        {
            QuestionLabel.Text = _currentWord.Korean;
            SetupNormalOptions();
        }
        else
        {
            QuestionLabel.Text = _currentWord.TranslationRu;
            ResetHangulInput();
        }

        StartTimer();
    }

    private void SetupNormalOptions()
    {
        if (_currentWord is null) return;

        var wrongOptions = _allWords
            .Where(w => w.Id != _currentWord.Id && !string.IsNullOrWhiteSpace(w.TranslationRu))
            .Select(w => w.TranslationRu)
            .Distinct()
            .OrderBy(_ => _random.Next())
            .Take(3)
            .ToList();

        var options = wrongOptions.Append(_currentWord.TranslationRu).OrderBy(_ => _random.Next()).ToList();

        var buttons = new[] { Option1Button, Option2Button, Option3Button, Option4Button };
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].Text = i < options.Count ? options[i] : string.Empty;
            buttons[i].IsVisible = i < options.Count;
            buttons[i].BackgroundColor = (Color)this.Resources["CardColor"];
            buttons[i].TextColor = (Color)this.Resources["InkColor"];
        }
    }

    // --- Клавиатура хангыля ---

    private void ResetHangulInput()
    {
        _pendingCho = null;
        _pendingJung = null;
        _pendingJong = null;
        _committedAnswer.Clear();
        UpdateTypedAnswerDisplay();
    }

    private void UpdateTypedAnswerDisplay()
    {
        var preview = BuildPendingPreview();
        var text = _committedAnswer.ToString() + preview;
        TypedAnswerLabel.Text = string.IsNullOrEmpty(text) ? " " : text;
    }

    private string BuildPendingPreview()
    {
        if (_pendingCho is null)
            return string.Empty;

        if (_pendingJung is null)
            return HangulComposer.Cho[_pendingCho.Value];

        var composed = HangulComposer.Compose(_pendingCho.Value, _pendingJung.Value, _pendingJong ?? 0);
        return composed?.ToString() ?? string.Empty;
    }

    private void CommitPendingSyllable()
    {
        if (_pendingCho is not null && _pendingJung is not null)
        {
            var composed = HangulComposer.Compose(_pendingCho.Value, _pendingJung.Value, _pendingJong ?? 0);
            if (composed is not null)
                _committedAnswer.Append(composed.Value);
        }
        _pendingCho = null;
        _pendingJung = null;
        _pendingJong = null;
    }

    private void OnChoKeyClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button) return;
        var idx = Array.IndexOf(HangulComposer.Cho, button.Text);
        if (idx < 0) return;

        // Если уже был полный слог (cho+jung) — фиксируем его и начинаем новый
        if (_pendingCho is not null && _pendingJung is not null)
            CommitPendingSyllable();

        _pendingCho = idx;
        _pendingJung = null;
        _pendingJong = null;
        UpdateTypedAnswerDisplay();
    }

    private void OnJungKeyClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || _pendingCho is null) return;
        var idx = Array.IndexOf(HangulComposer.Jung, button.Text);
        if (idx < 0) return;

        _pendingJung = idx;
        _pendingJong = null;
        UpdateTypedAnswerDisplay();
    }

    private void OnJongKeyClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || _pendingCho is null || _pendingJung is null) return;
        var idx = Array.IndexOf(HangulComposer.Jong, button.Text);
        if (idx < 0) return;

        _pendingJong = idx;
        CommitPendingSyllable();
        UpdateTypedAnswerDisplay();
    }

    private void OnCommitSyllableClicked(object sender, EventArgs e)
    {
        CommitPendingSyllable();
        UpdateTypedAnswerDisplay();
    }

    private void OnSpaceClicked(object sender, EventArgs e)
    {
        CommitPendingSyllable();
        _committedAnswer.Append(' ');
        UpdateTypedAnswerDisplay();
    }

    private void OnBackspaceClicked(object sender, EventArgs e)
    {
        if (_pendingJong is not null) _pendingJong = null;
        else if (_pendingJung is not null) _pendingJung = null;
        else if (_pendingCho is not null) _pendingCho = null;
        else if (_committedAnswer.Length > 0) _committedAnswer.Remove(_committedAnswer.Length - 1, 1);

        UpdateTypedAnswerDisplay();
    }

    private async void OnSubmitAnswerClicked(object sender, EventArgs e)
    {
        if (_answerLocked || _currentWord is null) return;

        CommitPendingSyllable();
        _answerLocked = true;
        StopTimer();

        var typed = _committedAnswer.ToString().Trim();
        bool isCorrect = string.Equals(typed, _currentWord.Korean.Trim(), StringComparison.Ordinal);

        await RecordAnswerAsync(isCorrect);

        if (isCorrect)
        {
            _correctCount++;
            ScoreLabel.Text = $"Правильно: {_correctCount}";
            TypedAnswerLabel.TextColor = (Color)this.Resources["JadeColor"];
        }
        else
        {
            TypedAnswerLabel.TextColor = (Color)this.Resources["PersimmonColor"];
            TypedAnswerLabel.Text = $"{typed}  ?  {_currentWord.Korean}";
        }

        await Task.Delay(1200);
        TypedAnswerLabel.TextColor = (Color)this.Resources["InkColor"];
        MoveToNextQuestion();
    }

    // --- Обычный режим (выбор варианта) ---

    private async void OnOptionClicked(object? sender, EventArgs e)
    {
        if (_answerLocked || sender is not Button clicked || _currentWord is null)
            return;

        _answerLocked = true;
        StopTimer();

        bool isCorrect = clicked.Text == _currentWord.TranslationRu;
        await RecordAnswerAsync(isCorrect);

        if (isCorrect)
        {
            _correctCount++;
            ScoreLabel.Text = $"Правильно: {_correctCount}";
            clicked.BackgroundColor = (Color)this.Resources["JadeColor"];
            clicked.TextColor = Colors.White;
        }
        else
        {
            clicked.BackgroundColor = (Color)this.Resources["PersimmonColor"];
            clicked.TextColor = Colors.White;
            HighlightCorrectAnswer();
        }

        await Task.Delay(700);
        MoveToNextQuestion();
    }

    private void HighlightCorrectAnswer()
    {
        if (_currentWord is null) return;

        var buttons = new[] { Option1Button, Option2Button, Option3Button, Option4Button };
        foreach (var button in buttons)
        {
            if (button.Text == _currentWord.TranslationRu)
            {
                button.BackgroundColor = (Color)this.Resources["JadeColor"];
                button.TextColor = Colors.White;
            }
        }
    }

    // --- Общая логика таймера / прогресса ---

    private void StartTimer()
    {
        StopTimer();
        _timeLeft = QuestionSeconds;
        TimerBar.Progress = 1;
        TimerBar.ProgressColor = (Color)this.Resources["JadeColor"];

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(100);
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void StopTimer()
    {
        if (_timer is not null)
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }
    }

    private async void OnTimerTick(object? sender, EventArgs e)
    {
        _timeLeft -= 0.1;
        var fraction = Math.Max(_timeLeft / QuestionSeconds, 0);
        TimerBar.Progress = fraction;

        if (fraction < 0.3)
            TimerBar.ProgressColor = (Color)this.Resources["PersimmonColor"];

        if (_timeLeft <= 0 && !_answerLocked)
        {
            _answerLocked = true;
            StopTimer();
            await RecordAnswerAsync(wasCorrect: false);

            if (_settings.Difficulty == QuizDifficulty.Normal)
                HighlightCorrectAnswer();
            else if (_currentWord is not null)
                TypedAnswerLabel.Text = $"Время вышло: {_currentWord.Korean}";

            await Task.Delay(900);
            MoveToNextQuestion();
        }
    }

    private async Task RecordAnswerAsync(bool wasCorrect)
    {
        if (_currentWord is null) return;
        SpacedRepetitionHelper.ApplyAnswer(_currentWord, wasCorrect);
        await _db.SaveWordAsync(_currentWord);
    }

    private void MoveToNextQuestion()
    {
        _currentIndex++;
        ShowQuestion();
    }

    private async void FinishQuiz()
    {
        StopTimer();
        var total = _quizQueue.Count;
        var percent = total == 0 ? 0 : (int)Math.Round(_correctCount * 100.0 / total);

        await CustomAlertPage.ShowAsync(
            "Результат",
            $"Правильных ответов: {_correctCount} из {total} ({percent}%)",
            "Готово");

        await Shell.Current.GoToAsync("..");
    }

    private async void OnFinishClicked(object sender, EventArgs e)
    {
        bool confirm = await CustomAlertPage.ShowConfirmAsync(
            "Завершить проверку?",
            "Прогресс по уже отвеченным словам сохранён. Оставшиеся слова останутся в очереди на следующий раз.",
            "Завершить",
            "Продолжить");

        if (confirm)
        {
            StopTimer();
            await Shell.Current.GoToAsync("..");
        }
    }
}